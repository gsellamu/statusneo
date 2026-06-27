"""
propose_service.py — the agentic-graph PROPOSE seam for the Modutecture Spine.

The .NET twin service (Agent:Mode=langgraph) calls POST /propose here. This service
runs the PLANNER pod's grounded reasoning and returns ONE bed-icu placement. It is
PROPOSE-ONLY: the .NET deterministic gate validates the placement before any commit.

Routing posture (locked): planner=OpenAI (cloud), reviewer=Anthropic (cloud),
embeddings=local Ollama. Keys come from the environment, never hard-coded.
Degrades to a deterministic geometric proposal if cloud is unavailable, so the
demo always returns a valid suggestion.

Run:
    pip install -r requirements.txt
    # put OPENAI_API_KEY (and optional ANTHROPIC_API_KEY) in ../.env — auto-loaded
    uvicorn propose_service:app --port 8088
"""
from __future__ import annotations
import json
import logging
import os
from typing import Any

import httpx
from fastapi import FastAPI
from pydantic import BaseModel

# Load keys from the spine-root .env (git-ignored) into the environment, if present.
# Secret VALUES live only in that file on disk; nothing here hard-codes a key.
try:
    from dotenv import load_dotenv
    load_dotenv(os.path.join(os.path.dirname(__file__), "..", ".env"))
except ImportError:
    pass  # python-dotenv optional; env vars can also be set by the shell

logging.basicConfig(level=logging.INFO)
log = logging.getLogger("modu.propose")

OPENAI_API_KEY = os.environ.get("OPENAI_API_KEY", "")
PLANNER_MODEL = os.environ.get("MODU_PLANNER_MODEL", "gpt-4o")
LLM_TIMEOUT_S = float(os.environ.get("MODU_LLM_TIMEOUT_S", "60"))

app = FastAPI(title="Modutecture agentic-graph · propose seam")


# ---------- brain port (planner = cloud OpenAI) ----------
class BrainError(RuntimeError):
    pass


def planner_json(system: str, user: str) -> dict:
    if not OPENAI_API_KEY:
        raise BrainError("OPENAI_API_KEY not set")
    r = httpx.post(
        "https://api.openai.com/v1/chat/completions",
        headers={"Authorization": f"Bearer {OPENAI_API_KEY}"},
        json={
            "model": PLANNER_MODEL,
            "temperature": 0.1,
            "response_format": {"type": "json_object"},
            "messages": [{"role": "system", "content": system},
                         {"role": "user", "content": user}],
        },
        timeout=LLM_TIMEOUT_S,
    )
    r.raise_for_status()
    raw = r.json()["choices"][0]["message"]["content"]
    return json.loads(raw)


# ---------- request/response models ----------
class RuleIn(BaseModel):
    id: str
    text: str
    ref: str | None = None


class InstanceIn(BaseModel):
    type_id: str
    x: float
    y: float
    rotation: int = 0


class ProposeRequest(BaseModel):
    goal: str = "observation room"
    room: dict[str, float] = {"w": 4000, "h": 3000}
    med_gas_reach_mm: int = 2500
    headwall_type: str = "headwall-hw204"
    rules: list[RuleIn] = []
    instances: list[InstanceIn] = []


# ---------- endpoints ----------
@app.get("/healthz")
def healthz() -> dict[str, str]:
    return {"status": "ok", "brain": "openai" if OPENAI_API_KEY else "deterministic-fallback"}


def _deterministic(req: ProposeRequest) -> dict[str, Any] | None:
    hw = next((i for i in req.instances if i.type_id == req.headwall_type), None)
    if hw is None:
        return None
    return {
        "proposal": {"typeId": "bed-icu", "x": int(hw.x), "y": int(hw.y + 1500), "rotation": 0},
        "rationale": (f"Deterministic fallback: bed 1500mm in front of headwall at "
                      f"({int(hw.x)},{int(hw.y)}); within {req.med_gas_reach_mm}mm reach. "
                      f"Deterministic gate validates next."),
    }


@app.post("/propose")
def propose(req: ProposeRequest) -> dict[str, Any]:
    hw = next((i for i in req.instances if i.type_id == req.headwall_type), None)
    if hw is None:
        return {"proposal": None,
                "rationale": "No med-gas source (headwall) placed yet; cannot satisfy med-gas reach."}

    rules_txt = "\n".join(f"- {r.id}: {r.text}" for r in req.rules) \
        or "- R3-medgas: bed within reach of a headwall"
    others = [{"type": i.type_id, "x": i.x, "y": i.y} for i in req.instances]
    system = ("You are the PLANNER pod of a hospital-design agent. You PROPOSE only; a "
              "deterministic gate validates your output afterward. Never invent code numbers.")
    user = (
        f"Room {int(req.room['w'])}x{int(req.room['h'])}mm, origin top-left, mm. Goal: {req.goal}.\n"
        f"Rules:\n{rules_txt}\nMed-gas reach: {req.med_gas_reach_mm}mm.\n"
        f"Headwall at ({hw.x},{hw.y}). Other equipment: {others}.\n"
        'Propose ONE bed-icu placement inside the room, within med-gas reach of the headwall, '
        'clear of equipment. Respond ONLY as JSON: '
        '{"x":<int>,"y":<int>,"rotation":0,"rationale":"<short>"}'
    )
    try:
        out = planner_json(system, user)
        return {
            "proposal": {"typeId": "bed-icu", "x": int(out["x"]),
                         "y": int(out["y"]), "rotation": int(out.get("rotation", 0))},
            "rationale": f"LangGraph planner pod: {out.get('rationale','')} Deterministic gate validates next.",
        }
    except Exception as e:
        log.warning("planner degraded -> deterministic: %s", e)
        fb = _deterministic(req)
        return fb or {"proposal": None, "rationale": f"degraded: {e}"}
