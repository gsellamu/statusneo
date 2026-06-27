# agentic-graph — the PROPOSE seam (LangGraph pods → the spine)

This is the external reasoning service the .NET twin calls when `Agent:Mode=langgraph`.
It runs the **planner pod's** grounded reasoning and returns ONE bed-icu placement
proposal. **Propose-only** — the .NET deterministic gate validates the placement
before anything commits. The twin stays the single source of truth.

## Contract
- `GET /healthz` → `{status, brain}` (used by `LangGraphAgentBrain.HealthyAsync`)
- `POST /propose` ← grounded facts + live instances → `{proposal:{typeId,x,y,rotation}|null, rationale}`

## Routing posture
planner = OpenAI (cloud) · reviewer = Anthropic (cloud) · embeddings = local Ollama.
Keys via environment only. Degrades to a deterministic geometric proposal if cloud
is unavailable, so the demo always returns a valid suggestion.

## Run
```bash
pip install fastapi uvicorn httpx pydantic
set OPENAI_API_KEY=...                 # optional; falls back if absent (Windows: setx)
uvicorn propose_service:app --port 8088
```
Then start the twin with:
```bash
# in twin-service-dotnet/
dotnet run --  # with appsettings: "Agent": { "Mode": "langgraph", "LangGraphUrl": "http://localhost:8088" }
```
Or set env: `Agent__Mode=langgraph`.

The full multi-pod graph (4 hospital pods + reflective reviewer + gate checkpoint)
lives in the `modu-agentic` scaffold; this service is the thin propose seam that the
spine consumes. To use the full graph, point `propose` at `build_graph()` instead of
the single planner call — the request/response contract is unchanged.

## Verified
`propose_service.py` compiles and returns:
- a valid `bed-icu` placement when a headwall is present (cloud or deterministic fallback),
- `proposal: null` with a reason when no med-gas source exists yet.
