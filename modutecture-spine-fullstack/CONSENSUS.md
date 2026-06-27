# Modutecture Spine — Final Consensus (architecture of record)

This records the design we converged on, what the reference implementation now
**proves**, and what is deliberately deferred. It supersedes earlier framing where
they differ. Companion artifacts: the running code (this repo), the test harnesses
(`tests/`, `harness/`), and the viability dashboard (`dashboard/dashboard.html`).

---

## 1. The thesis (unchanged, now proven)

A **directed, state-and-event-driven graph** is the source of truth; the renderer
(Unity or web) owns nothing. Every operation is **propose → validate → gate →
commit → propagate**. Edges (e.g. med-gas bindings) exist only when validation
permits them — *connected, never promiscuous*. The twin is a **fold over an
append-only journal**. Unity is the GPU; the twin holds the truth; the edge carries
the bytes; the brain stays where the data is.

## 2. Confirmed against Modutecture's own onboarding docs

- Stack is **Unity ⇄ Web/App ⇄ GraphQL (+REST) ⇄ Databricks ⇄ SQL Server + Lakebase/Postgres** — the reference targets exactly this.
- **Collision is a "concept,"** not a shipped capability; there is **no geometry kernel** anywhere in the stack. The end-to-end chain stalls precisely where validation has no home — which is the gap the spine fills.
- Rules/standards already live as **data** (ASP/SPA → Databricks → SSMS); Moducule **behavior** lives in Unity. Geometry and truth are split across two unconnected worlds.
- **AltTester + Cucumber/Gherkin + Azure DevOps** traceability (`@TC_<id>`) is already live — Track 2 *extends* a real asset, it does not introduce one.
- Decisions are "made in meetings" with output-only metrics (story points, defects) — the ADR/outcome-metric gap is real, not assumed.

## 3. Decisions of record (ADR-style)

| # | Decision | Status |
|---|---|---|
| D1 | Truth lives in an event-sourced **composition graph** behind a GraphQL twin service; renderers are thin lenses | **proven** (place-a-Moducule slice, both stacks) |
| D2 | **Validation always runs against current truth**; clients may *render* stale state but the **gate never judges stale** | **proven** (CC1) |
| D3 | **Optimistic concurrency via a version token**; stale-but-legal commits rebase, conflicts reject | **proven** (CC2) |
| D4 | **Commands are idempotent** (client `commandId`) — safe retries, no double-write | **proven** (RES1) |
| D5 | **Instances pin the exact type version** they were validated against; a later type bump flags-for-review, never silently mutates | **proven** (SCH1) |
| D6 | Persistence = **change journal v1** (append-only + replay); full event-sourcing with stored projections = v2 | **proven** (ES1/ES2); v2 deferred |
| D7 | v1 geometry = **server-side AABB + (future) spatial index**; **no B-rep kernel** until curved walls / true booleans / parametric mating demand it | kernel = **triggered ADR**, not day-one |
| D8 | Thin client holds **zero domain rules** (enforced in CI) | **proven** (INV3 static check) |
| D9 | Agent **proposes only**; a human approval is the sole write path (HITL) | **proven** (AG1/AG2); LangGraph drops in behind the same contract |

## 3b. Boundary mechanics — Unity/web ↔ backend (decisions D10–D13)

| # | Decision | State |
|---|---|---|
| D10 | **One contract, two lenses**: clients receive twin payloads + emit intents; never mutate truth locally. Same GraphQL drives Unity and web. | proven (served web viewer + Unity client on identical schema) |
| D11 | **Live sync = subscription, with polling fallback**: `onTwinChanged` wired server-side; demo polls for robustness. Reconnect = **snapshot-then-resume** (re-query twin, then resubscribe) so no delta is missed. | subscription wired; polling proven in the demo viewer |
| D12 | **Coordinate transform lives in exactly one place** at the boundary (mm/top-down server ⇄ metres/Y-up Unity), applied symmetrically down and up. | recorded in client code; ADR-008 |
| D15 | **Plug-and-play proven, not asserted**: `IAgentBrain` has 3 real adapters (deterministic / single-shot Ollama / reflective planner→reviewer) swappable by `Agent:Mode` config; `/capabilities` is a self-describing manifest; `/architecture.html` renders live wiring. LLM self-critique augments but never replaces the deterministic gate. | built; manifest verified by execution; brain build-verified, live LLM path runs against infra |
| D14 | **Real-infra integration behind seams**: grounding=Neo4j, brain=Ollama (mistral-nemo), journal=Postgres, metrics=Prometheus/Grafana, tracing=OTLP — each with a deterministic fallback and a live `/health` status. LLM output is grounded-only and **gate-validated** before surfacing. | built; verified by build + fallback tests; run against live infra per RUN_REAL.md |
| D13 | **Presence/ephemeral channel** (cursors, selection) is separate from the durable command path and is never event-sourced. | design-of-record |

Threading discipline (D10): all client I/O (HTTP, socket, JSON) runs off the render
thread; only the resulting state-apply is marshalled back to the main thread. Engine
APIs are never touched from a socket callback.

## 4b. Desktop demo (runnable, on the proposed stack)

`docker compose up --build` → Postgres + the .NET twin service + a zero-dependency web
viewer at `http://localhost:8099`. Real Postgres (not in-memory) → the "production stack"
claim is honest. The served viewer's exact GraphQL operations were verified end-to-end
through the Hot Chocolate pipeline. See `DEMO_SCRIPT.md` for the CTO/Architect walkthrough,
the 30–60 day plan, and prepared scale-path answers.

## 4. What the reference implementation proves (39/39, parity MATCH)

- Runs on the **proposed stack**: .NET 8 + Hot Chocolate + EF Core/Postgres twin service, Unity + Angular thin lenses, Python oracle as the tested reference.
- **.NET ≡ Python parity** on a canonical scenario (13 fields, field-by-field) → the *architecture* works, not just one codebase.
- **Validation gate p99 = 0.0149 ms** (.NET) → "snaps instantly" is measured, not claimed. Full commit p95 = 0.29 ms.
- Categories: RULES · INVARIANTS · CONCURRENCY · RESILIENCE · SCHEMA · EVENT-SOURCING · AGENT-HITL · PERFORMANCE. Every test names the claim it proves.

## 5. Scaling — three channels, three playbooks (design of record)

| Channel | Physics | Pattern | Mechanism |
|---|---|---|---|
| Geometry | immutable, huge | **Netflix Open Connect** | content-addressed glTF/USD at edge; **LOD-as-bitrate**; transcode-on-publish |
| State | mutable, shared, small | **Meta TAO + feed fan-out** | regional read replicas + edge **working-set views**; **instance-change = push**, **type-change = invalidate-not-fan-out** (the celebrity write) |
| AI | bursty, compute-heavy | **edge inference + pre-warm** | regional GPU co-located with graph/vector; warm model + pre-fetched grounding; **never on the frame path**; *pre-warm context, not answers* |

Four latency tiers: **T0** <16 ms (never networked) · **T1** 50–150 ms (optimistic + edge-confirm, cached rule subset) · **T2** seconds (streamed overlays) · **T3** minutes–hours (durable HITL). Load asymmetry is the design driver: **interactions ≫ reads ≫ writes**, geometry bytes ≫ state bytes — the common case must never touch the authoritative store. Spikes that matter: **design storm, launch day, type-change cascade** — absorbed by CDN (geometry), T0/T1 optimism (interaction), and invalidate-not-fan-out (cascade). **Earn each tier with a metric** (CDN first → read replicas → edge working-sets → predictive pre-warm last), each an ADR with a trigger and revisit date.

## 6. FAANG/Autodesk-grade backlog (triggered, not now)

- **Geometry & precision** — spatial index (BVH/grid) for clash as scale grows; **floating-point precision at building coordinates** (local-origin / double-precision relative-to-center, the Autodesk-hard part); parametric constraint solving only if Moducule mating demands it.
- **Schema evolution** — event **upcasters**; semver'd registry; version-pinned instances (D5) as the cascade hook. *(pinning already proven.)*
- **Multi-tenancy** — isolation tier decision (row-security → schema-per-tenant → DB-per-tenant); per-tenant quotas (noisy-neighbor); tenant-id enforced at every layer.
- **AuthZ** — multi-axis **tenant × project × role × object-state** (OPA/Cedar-style externalized policy), SSO/SCIM, field-level authz in GraphQL.
- **Cost model** — per active-builder-session, per GPU-inference-hour, per stored-twin, per GB egress; tie every scaling tier to a margin lever (keeps "local LLM everywhere" from eroding gross margin).
- **Observability** — RED/USE + distributed tracing; **AI-observability** (proposal + retrieval context + accept/reject + eval score); per-tenant cost attribution from day one.
- **Data platform** — **CDC out of Postgres** (outbox/Debezium), graph + vector as **rebuildable derived projections**; streaming reindex for GraphRAG freshness.
- **Resilience** — degrade-to-read-only when the regional brain is down; per-room command queue with backpressure; idempotency (D4, proven); DR'd journal (its RPO/RTO *is* the platform's).
- **Security** — threat-model tenant-isolation breach, **prompt-injection via Moducule/spec content**, registry supply-chain integrity (content-addressing + signing), PHI handling; SOC 2 / HIPAA as a **sales gate**.

## 7. Explicitly out of scope (the senior "not yet")

- **Global multi-region active-active** — single write-region + read replicas until builders span continents.
- **Full CRDT collaborative-CAD** — optimistic concurrency + event ordering covers most value (D2/D3); Figma-grade simultaneous geometry editing is deferred until the demand is proven.
- **In-house model pre-training** — adapt open weights; the flywheel is **data-gated** (instrument now, train when enough validated deltas exist).
- **B-rep geometry kernel** — deferred per D7.

## 8. Two decisions flagged for the CTO's judgment first

1. **Geometry kernel / precision strategy** (D7) — hardest to retrofit, CAD-deep, and the docs confirm there's nothing there yet.
2. **Tenant-isolation tier** — wrong choice is both a painful migration and a security/sales risk.

Naming these as "least confident, want your eyes first" is deliberate: decision rights
stay with the client; the reference proves the spine; first-week discovery (where does
overlap run today? are Moducules fixed or parametric?) calibrates the rest.
