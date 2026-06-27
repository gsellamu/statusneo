"""
domain.py — the genome + the cerebellum.

Pure functions only: no database, no network. This is where domain truth is
*defined* (Moducule type contracts = MDS) and *judged* (validation verdicts).
Because it is pure, it is unit-testable in isolation and could be reused
verbatim on an edge node, in a worker, or compiled to another language.

Architectural mapping:
  - MDS / REGISTRY      -> the genome (inherited type definitions)
  - validate(...)       -> the cerebellum (verdicts; never stores, never renders)
  - fold(...)           -> the twin as a left-fold over the event journal
"""
from __future__ import annotations
from dataclasses import dataclass, field, asdict
from typing import Literal
import math

EPS = 1.0  # mm tolerance: allow envelopes to touch, not overlap

# ----------------------------------------------------------------------------
# MDS — Moducule Definition Schema (the contract per building block)
# ----------------------------------------------------------------------------
@dataclass(frozen=True)
class Port:
    name: str
    kind: Literal["med_gas", "electrical", "data", "plumbing"]
    role: Literal["provides", "requires"]

@dataclass(frozen=True)
class MDS:
    type_id: str
    name: str
    version: str
    footprint_w: int          # mm, local X before rotation
    footprint_d: int          # mm, local Y before rotation
    # directional keep-clear margins (mm) in local frame: (left, top, right, bottom)
    clearance: tuple[int, int, int, int]
    ports: tuple[Port, ...] = ()
    geometry_ref: str = ""    # content-addressed glTF/USD pointer (not embedded)

    def provides(self, kind: str) -> bool:
        return any(p.kind == kind and p.role == "provides" for p in self.ports)

    def requires(self, kind: str) -> bool:
        return any(p.kind == kind and p.role == "requires" for p in self.ports)

# Seed registry. In production this is rows in the registry service, served via
# the same GraphQL layer; here it is a dict so the demo runs with zero infra.
REGISTRY: dict[str, MDS] = {
    "headwall-hw204": MDS(
        type_id="headwall-hw204", name="Headwall HW-204", version="2.3.0",
        footprint_w=1800, footprint_d=300, clearance=(0, 0, 0, 900),  # access in front
        ports=(Port("mg-out", "med_gas", "provides"),
               Port("pwr-out", "electrical", "provides")),
        geometry_ref="gltf://sha256-hw204",
    ),
    "bed-icu": MDS(
        type_id="bed-icu", name="ICU Bed", version="1.4.0",
        footprint_w=1000, footprint_d=2200, clearance=(600, 0, 600, 0),  # egress sides
        ports=(Port("mg-in", "med_gas", "requires"),),
        geometry_ref="gltf://sha256-bedicu",
    ),
    "sink-clinical": MDS(
        type_id="sink-clinical", name="Clinical Sink", version="1.0.1",
        footprint_w=600, footprint_d=600, clearance=(300, 0, 300, 450),
        ports=(Port("h2o", "plumbing", "requires"),),
        geometry_ref="gltf://sha256-sink",
    ),
}

MED_GAS_REACH_MM = 2500  # a bed must be within this of a med-gas source

# ----------------------------------------------------------------------------
# Instances + geometry helpers
# ----------------------------------------------------------------------------
@dataclass
class Instance:
    instance_id: str
    type_id: str
    x: float            # mm, center
    y: float            # mm, center
    rotation: int       # 0 | 90 | 180 | 270

    def dims(self) -> tuple[int, int]:
        d = REGISTRY[self.type_id]
        return (d.footprint_d, d.footprint_w) if self.rotation in (90, 270) \
            else (d.footprint_w, d.footprint_d)

def _footprint(inst: Instance) -> tuple[float, float, float, float]:
    w, d = inst.dims()
    return (inst.x - w / 2, inst.y - d / 2, inst.x + w / 2, inst.y + d / 2)

def _clearance_rotated(inst: Instance) -> tuple[int, int, int, int]:
    l, t, r, b = REGISTRY[inst.type_id].clearance
    for _ in range((inst.rotation // 90) % 4):
        l, t, r, b = b, l, t, r            # rotate margins clockwise with the part
    return (l, t, r, b)

def _envelope(inst: Instance) -> tuple[float, float, float, float]:
    x0, y0, x1, y1 = _footprint(inst)
    l, t, r, b = _clearance_rotated(inst)
    return (x0 - l, y0 - t, x1 + r, y1 + b)

def _overlap(a, b) -> bool:
    return (a[0] < b[2] - EPS and a[2] > b[0] + EPS and
            a[1] < b[3] - EPS and a[3] > b[1] + EPS)

def _inside(inner, outer) -> bool:
    return (inner[0] >= outer[0] - EPS and inner[1] >= outer[1] - EPS and
            inner[2] <= outer[2] + EPS and inner[3] <= outer[3] + EPS)

def _dist(a: Instance, b: Instance) -> float:
    return math.hypot(a.x - b.x, a.y - b.y)

# ----------------------------------------------------------------------------
# Verdicts
# ----------------------------------------------------------------------------
Severity = Literal["ERROR", "WARNING"]

@dataclass
class Violation:
    rule: str
    severity: Severity
    message: str
    refs: list[str] = field(default_factory=list)

@dataclass
class Verdict:
    ok: bool                              # False if any ERROR -> commit blocked
    violations: list[Violation]
    bindings: list[dict]                  # directed edges earned by this candidate

def validate(candidate: Instance,
             others: list[Instance],
             room: tuple[float, float, float, float]) -> Verdict:
    """propose -> VALIDATE. Returns a verdict; writes nothing.

    Rules (this thin slice):
      R1 boundary  : footprint inside room          [ERROR]
      R2 clearance : envelopes must not overlap      [ERROR]  ("only snaps if legal")
      R3 med-gas   : requirer within reach of source [ERROR]  (earns a directed edge)
      R4 orientation: advisory facing                [WARNING] (commits, but flagged)
    """
    defn = REGISTRY[candidate.type_id]
    v: list[Violation] = []
    bindings: list[dict] = []

    # R1 — boundary
    if not _inside(_footprint(candidate), room):
        v.append(Violation("R1-boundary", "ERROR",
                            "Footprint extends outside the room boundary.",
                            [candidate.instance_id]))

    # R2 — clash: hard footprint overlap is an ERROR; clearance encroachment warns
    cand_fp, cand_env = _footprint(candidate), _envelope(candidate)
    for o in others:
        if _overlap(cand_fp, _footprint(o)):
            v.append(Violation("R2-collision", "ERROR",
                               f"Footprint physically overlaps {o.type_id} ({o.instance_id}).",
                               [candidate.instance_id, o.instance_id]))
        elif _overlap(cand_env, _envelope(o)):
            v.append(Violation("R2-clearance", "WARNING",
                               f"Keep-clear zone encroaches {o.type_id} ({o.instance_id}).",
                               [candidate.instance_id, o.instance_id]))

    # R3 — med-gas reach (dependency edge)
    if defn.requires("med_gas"):
        sources = [o for o in others if REGISTRY[o.type_id].provides("med_gas")]
        in_reach = [o for o in sources if _dist(candidate, o) <= MED_GAS_REACH_MM]
        if not sources:
            v.append(Violation("R3-medgas", "ERROR",
                               "No med-gas source placed in this room.",
                               [candidate.instance_id]))
        elif not in_reach:
            nearest = min(_dist(candidate, o) for o in sources)
            v.append(Violation("R3-medgas", "ERROR",
                               f"Nearest med-gas source is {nearest:.0f}mm away "
                               f"(limit {MED_GAS_REACH_MM}mm).",
                               [candidate.instance_id]))
        else:
            src = min(in_reach, key=lambda o: _dist(candidate, o))
            bindings.append({"kind": "med_gas",
                             "from": candidate.instance_id,
                             "to": src.instance_id})

    # R4 — advisory orientation
    if candidate.type_id == "bed-icu" and candidate.rotation != 0:
        v.append(Violation("R4-orientation", "WARNING",
                            "Bed not facing entry wall (advisory).",
                            [candidate.instance_id]))

    ok = not any(x.severity == "ERROR" for x in v)
    return Verdict(ok=ok, violations=v, bindings=bindings)

# ----------------------------------------------------------------------------
# fold — the twin is a left-fold over the event journal
# ----------------------------------------------------------------------------
def fold(events: list[dict],
         room: tuple[float, float, float, float]) -> dict:
    """commit history -> current read model. Pure reducer over events."""
    instances: dict[str, Instance] = {}
    for e in events:
        if e["type"] == "MODUCULE_PLACED":
            p = e["payload"]
            instances[p["instance_id"]] = Instance(**{
                k: p[k] for k in ("instance_id", "type_id", "x", "y", "rotation")})
        elif e["type"] == "MODUCULE_REMOVED":
            instances.pop(e["payload"]["instance_id"], None)

    # bindings are a *function of validated state*: recompute med-gas edges
    bindings: list[dict] = []
    insts = list(instances.values())
    for b in insts:
        if REGISTRY[b.type_id].requires("med_gas"):
            srcs = [o for o in insts
                    if REGISTRY[o.type_id].provides("med_gas")
                    and _dist(b, o) <= MED_GAS_REACH_MM]
            if srcs:
                src = min(srcs, key=lambda o: _dist(b, o))
                bindings.append({"kind": "med_gas",
                                 "from": b.instance_id, "to": src.instance_id})

    return {
        "room": {"x0": room[0], "y0": room[1], "x1": room[2], "y1": room[3]},
        "instances": [asdict(i) for i in insts],
        "bindings": bindings,
    }
