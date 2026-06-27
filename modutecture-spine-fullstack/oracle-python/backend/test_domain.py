"""
test_domain.py — proof the cerebellum is correct. Run: python test_domain.py
Pure-function tests: no server, no DB. Each asserts one rule, then we fold a
small event history to prove the read model derives correctly.
"""
from domain import Instance, validate, fold, REGISTRY

ROOM = (0.0, 0.0, 4000.0, 3000.0)

def hw(x, y, r=0): return Instance("hw1", "headwall-hw204", x, y, r)
def bed(x, y, r=0): return Instance("bed1", "bed-icu", x, y, r)

def check(name, cond):
    print(f"  [{'PASS' if cond else 'FAIL'}] {name}")
    assert cond, name

print("R1 boundary")
check("headwall inside room is OK",
      validate(hw(2000, 200), [], ROOM).ok)
check("headwall crossing top wall is REJECTED",
      not validate(hw(2000, 50), [], ROOM).ok)

print("R2 clash: collision = ERROR, clearance encroachment = WARNING")
h = hw(2000, 200)
check("bed in front of headwall, no footprint overlap, in reach is OK",
      validate(bed(2000, 1500), [h], ROOM).ok)
check("two headwalls with overlapping footprints is REJECTED",
      not validate(hw(2400, 200), [hw(2000, 200)], ROOM).ok)
# bed + sink: footprints clear, but side keep-clear zones touch -> WARNING
ctx = [hw(1500, 200), Instance("sink1", "sink-clinical", 2350, 1600, 0)]
enc = validate(bed(1500, 1600), ctx, ROOM)
check("footprint-clear but clearance-touching commits with a WARNING",
      enc.ok and any(v.rule == "R2-clearance" for v in enc.violations))

print("R3 med-gas reach (dependency edge)")
check("bed with no med-gas source is REJECTED",
      not validate(bed(2000, 1500), [], ROOM).ok)
far = validate(bed(500, 2800), [hw(3500, 200)], ROOM)
check("bed beyond reach is REJECTED", not far.ok)
near = validate(bed(2000, 1500), [hw(2000, 200)], ROOM)
check("bed within reach earns a med_gas binding",
      near.ok and len(near.bindings) == 1 and near.bindings[0]["kind"] == "med_gas")

print("R4 advisory orientation (commits, but WARNING)")
warn = validate(bed(2000, 1500, r=90), [hw(2000, 200)], ROOM)
check("rotated bed still commits", warn.ok)
check("rotated bed carries a WARNING",
      any(v.severity == "WARNING" for v in warn.violations))

print("fold — twin is a left-fold over the journal")
events = [
    {"type": "MODUCULE_PLACED",
     "payload": {"instance_id": "hw1", "type_id": "headwall-hw204",
                 "x": 2000, "y": 200, "rotation": 0}},
    {"type": "MODUCULE_PLACED",
     "payload": {"instance_id": "bed1", "type_id": "bed-icu",
                 "x": 2000, "y": 1500, "rotation": 0}},
]
state = fold(events, ROOM)
check("two instances after two PLACE events", len(state["instances"]) == 2)
check("read model derives the med-gas binding", len(state["bindings"]) == 1)

events.append({"type": "MODUCULE_REMOVED", "payload": {"instance_id": "hw1"}})
state = fold(events, ROOM)
check("removing the headwall drops the instance", len(state["instances"]) == 1)
check("and the binding disappears (edge is a function of state)",
      len(state["bindings"]) == 0)

print("\nALL DOMAIN TESTS PASSED")
