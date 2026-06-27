"""
app.py — the spinal cord. Every signal travels here, through typed contracts.

  GET  /api/registry              afferent: the genome (MDS types)
  GET  /api/twin/{room}           afferent: derived read model (CQRS read side)
  GET  /api/events/{room}         afferent: the raw journal (audit / flywheel source)
  POST /api/commands/{room}       efferent: propose -> validate -> gate -> commit -> propagate
  POST /api/agent/suggest/{room}  the cerebrum: PROPOSES a command (never commits)
  WS   /ws/{room}                 propagation: snapshot on connect, push on commit

The command handler is the whole architecture in one function: a candidate is
judged by the pure cerebellum; only an ERROR-free verdict becomes an event;
the event is appended, the read model re-derived, and every client notified.
An illegal placement writes nothing — the directed edge is simply never created.
"""
from __future__ import annotations
import uuid, asyncio
from pathlib import Path
from dataclasses import asdict
from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from fastapi.responses import HTMLResponse, JSONResponse
from pydantic import BaseModel

from domain import REGISTRY, MED_GAS_REACH_MM, Instance, validate
from store import EventStore, ROOM

app = FastAPI(title="Modutecture Spine — thin slice")
store = EventStore()

# --- WebSocket fan-out (propagation) ----------------------------------------
class Hub:
    def __init__(self): self.peers: dict[str, list[WebSocket]] = {}
    async def join(self, room, ws):
        await ws.accept()
        self.peers.setdefault(room, []).append(ws)
    def leave(self, room, ws):
        self.peers.get(room, []).remove(ws)
    async def broadcast(self, room, msg):
        for ws in list(self.peers.get(room, [])):
            try: await ws.send_json(msg)
            except Exception: pass
hub = Hub()

# --- command contracts (the synapse) ----------------------------------------
class PlaceCmd(BaseModel):
    type: str = "PLACE"
    type_id: str
    x: float
    y: float
    rotation: int = 0

class RemoveCmd(BaseModel):
    type: str = "REMOVE"
    instance_id: str

class SuggestReq(BaseModel):
    goal: str = "add_bed"

# --- queries (afferent) -----------------------------------------------------
@app.get("/api/registry")
def get_registry():
    return {tid: asdict(m) for tid, m in REGISTRY.items()} | \
           {"_meta": {"med_gas_reach_mm": MED_GAS_REACH_MM}}

@app.get("/api/twin/{room}")
def get_twin(room: str):
    return store.read_model(room)

@app.get("/api/events/{room}")
def get_events(room: str):
    return store.events(room)

# --- the command handler (propose -> validate -> gate -> commit -> propagate)
def _handle(room: str, cmd: dict, actor: str) -> dict:
    # idempotency: a replayed command_id returns the original result, never double-writes
    cid = cmd.get("command_id")
    if cid:
        prior = store.by_command_id(room, cid)
        if prior:
            return {"status": "ACCEPTED", "event": prior, "violations": [],
                    "idempotent_replay": True, "version": store.version(room)}

    current_version = store.version(room)
    expected = cmd.get("expected_version")
    rebased = expected is not None and expected != current_version  # client saw a stale twin

    state = store.read_model(room)
    others = [Instance(i["instance_id"], i["type_id"], i["x"], i["y"], i["rotation"])
              for i in state["instances"]]

    if cmd["type"] == "REMOVE":
        if not any(o.instance_id == cmd["instance_id"] for o in others):
            return {"status": "REJECTED", "version": current_version,
                    "violations": [{"rule": "exists", "severity": "ERROR",
                                    "message": "No such instance.", "refs": []}]}
        ev = store.append(room, "MODUCULE_REMOVED",
                          {"instance_id": cmd["instance_id"]}, actor, cid)
        return {"status": "ACCEPTED", "event": ev, "violations": [],
                "rebased": rebased, "version": store.version(room)}

    # PLACE — always validated against CURRENT truth (the consistency boundary)
    candidate = Instance(instance_id=f"i-{uuid.uuid4().hex[:8]}",
                         type_id=cmd["type_id"], x=cmd["x"], y=cmd["y"],
                         rotation=cmd.get("rotation", 0))
    verdict = validate(candidate, others, ROOM)
    vlist = [asdict(v) for v in verdict.violations]

    if not verdict.ok:                       # GATE: any ERROR blocks the commit
        return {"status": "REJECTED", "violations": vlist, "rebased": rebased,
                "version": current_version}

    defn = REGISTRY[candidate.type_id]
    ev = store.append(room, "MODUCULE_PLACED", {       # COMMIT
        "instance_id": candidate.instance_id, "type_id": candidate.type_id,
        "type_version": defn.version,                  # pin the exact type version validated
        "x": candidate.x, "y": candidate.y, "rotation": candidate.rotation,
        "bindings": verdict.bindings,
        "warnings": [v for v in vlist if v["severity"] == "WARNING"],
    }, actor, cid)
    return {"status": "ACCEPTED", "event": ev, "violations": vlist,
            "rebased": rebased, "version": store.version(room)}

@app.post("/api/commands/{room}")
async def post_command(room: str, body: dict):
    result = _handle(room, body, actor="planner")
    if result["status"] == "ACCEPTED":                 # PROPAGATE
        await hub.broadcast(room, {"event": "twin_changed",
                                   "twin": store.read_model(room)})
    return JSONResponse(result)

# --- agent: PROPOSES only; a human must approve (HITL gate) ------------------
@app.post("/api/agent/suggest/{room}")
def agent_suggest(room: str, req: SuggestReq):
    """
    DETERMINISTIC STUB — not an LLM. A LangGraph agent plugs in here; its output
    contract is identical: a proposed command + cited rationale, never a commit.
    Strategy: place an ICU bed centered in front of the first headwall, inside
    reach and clear of its envelope.
    """
    state = store.read_model(room)
    insts = state["instances"]
    headwall = next((i for i in insts if i["type_id"] == "headwall-hw204"), None)
    if not headwall:
        return {"proposal": None,
                "rationale": "No med-gas source present; cannot satisfy R3 yet.",
                "citations": ["R3-medgas"]}
    proposal = {"type": "PLACE", "type_id": "bed-icu",
                "x": headwall["x"], "y": headwall["y"] + 1500, "rotation": 0}
    return {
        "proposal": proposal,
        "rationale": ("Bed centered 1500mm in front of headwall "
                      f"{headwall['instance_id']} — within {MED_GAS_REACH_MM}mm "
                      "med-gas reach (R3) and clear of its keep-clear zone (R2)."),
        "citations": ["R3-medgas", "R2-clearance"],
        "note": "Proposal only. Approve to route through the normal commit path.",
    }

# --- websocket --------------------------------------------------------------
@app.websocket("/ws/{room}")
async def ws(room: str, sock: WebSocket):
    await hub.join(room, sock)
    await sock.send_json({"event": "snapshot", "twin": store.read_model(room)})
    try:
        while True:
            await sock.receive_text()       # client is render-only; ignore inbound
    except WebSocketDisconnect:
        hub.leave(room, sock)

# --- serve the thin client --------------------------------------------------
@app.get("/", response_class=HTMLResponse)
def index():
    return (Path(__file__).parent.parent / "frontend" / "index.html").read_text()
