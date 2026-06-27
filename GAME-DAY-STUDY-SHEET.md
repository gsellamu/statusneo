# CTO Showtime — Game-Day Study Sheet
**Audience:** David Wilson (Tech Head) + Lead Architect · **Goal:** working code → 30–60 day contract
**One line to live by:** *Lead with working software. Never overclaim cloud-scale. Label built vs. proven vs. next.*

---

## ⚡ PRE-FLIGHT (T-30 min, on your laptop)

```powershell
# 1. infra up (your containers)            2. reset + run
docker ps                                   # confirm postgres/neo4j/ollama
.\scripts\demo-reset.ps1                     # clean + reseed
.\scripts\run.ps1 -Port 5005                 # twin on :5005
```
- [ ] `http://localhost:5005/health` → **journal:postgres:up · grounding:neo4j:up · brain:ollama:up**
- [ ] `http://localhost:5005/studio.html` → all 5 lenses render, Unity says "ready to wire"
- [ ] `http://localhost:5005/showcase.html` → front door + voice tour work
- [ ] 2nd browser tab open (for the live-sync moment)
- [ ] Tabs pre-opened: `/` · `/studio.html` · `/hierarchy.html` · `dashboard/dashboard.html` · `CONSENSUS.md`
- [ ] Phone hotspot ready (3D lens needs CDN/internet)
- [ ] Recorded smoke-test capture ready (in-room insurance)

> ⚠️ **PORT NOTE:** older docs say `:5000` — we standardized on **:5005**. The app now defaults to 5005. If anything 404s, check the port.

**If it breaks live:** `.\scripts\run.ps1 -InMemory` (zero deps, same demo) → or play the recorded capture → or walk `dashboard.html` (39/39 green) while it recovers. *It degrades, it doesn't crash.*

---

## 🎯 THE 90-SECOND FRAME (say this first, almost verbatim)

> "I didn't study the job description — I studied your platform. Your bet is right: buildings should compound, not restart. The reason the Designer → Space Bot → Room → Floor chain doesn't hold together yet is that **the product lives between the tools**, and right now there's nothing there — collision is still a 'concept' in your own docs, because **validation has no home**: it can't be authoritative in the Unity client, and nothing server-side computes it. I built the missing piece, on your stack, and I want to show it running — then you tell me where I'm wrong."

Then **stop and demo.** Don't explain the architecture in words — show it.

---

## 🖱️ THE DEMO — CLICK SEQUENCE (5–7 min)

| # | Action | What they see | The line |
|---|--------|---------------|----------|
| **A** | Headwall HW-204 → click near top wall | ✓ COMMITTED seq#1 → v1 | "A click is an *intent*, not a mutation. Server validated, then wrote one immutable event. **The renderer owns nothing.**" |
| **B** | ICU Bed → click far bottom-left | ✗ REJECTED — [R3-medgas] 3002mm > 2500mm. Journal doesn't grow | "**This is the whole thesis.** Rejected design left no trace, error cites the rule by name. *It only snaps if it's legal.*" |
| **C** | ICU Bed → click in front of headwall | ✓ COMMITTED v2 ⚠ keep-clear warning + **gold med-gas line** | "Committed *with a warning*, not a block — snaps-instantly / certify-async. That gold line is an **edge the system earned** by validation. Delete the headwall, the edge vanishes." |
| **D** | Switch to tab 2 | bed + edge already there | "Two planners, one truth. Polling for demo-robustness; subscription wired for prod." |
| **E** | Suggest a bed (agent) → Approve | proposal + citations from graph | "**Context Spine, live.** Graph grounds the rules, local LLM proposes using only those facts, gate validates before a human sees it. **The agent cannot write — approval is the only path to commit.**" |
| **E2** | Open `/health` | postgres:up · neo4j:up · ollama:up | "Not mocked. And if any drops, it degrades to a labeled fallback. Also reporting to your Grafana right now." |

**KILL-SHOT (if they lean in):** place headwall → run agent → drag bed out of reach → re-run → model's own proposal **rejected with cited rule**. *"An LLM grounded by a graph and policed by a deterministic gate — that's the whole thesis."*

---

## 🧩 THE SHOWPIECES (pick based on the room's energy)

- **Lens Studio** (`/studio.html`) — place in 2D → all lenses update. *"Same twin, four renderers, none owns a rule. The renderer is swappable."* Add Unity/UE5 live if you bring up the editors.
- **Hierarchy** (`/hierarchy.html`) — "Stamp into all ICU rooms" → 3 rooms flip COMPLIANT → floor → building roll up. *"The whole pitch in one click. Reuse the design, re-earn the compliance — that's the difference between a CAD macro and a contextual platform."*
- **Blast radius** — revise HW-204 → exactly these rooms flag. *"Not a guess, a query. Invalidate-and-review, scoped."*
- **Self-describing** (`/architecture.html`) — renders live from `/capabilities` + `/health`. *"Not a slide — the service describing itself. Green dots are health-probed."*
- **Config-swap brain** — `-AgentMode reflective` → planner→reviewer loop, map updates. *"One config value, reasoning engine swaps, zero code change."*
- **Operational twin** (`/telemetry.html`, `-Redpanda`) — live occupancy/CO₂. *"The same twin that validates a design carries the building's operational signals."*

---

## 🛡️ THE HONESTY SLIDE (say it BEFORE they ask — it's your credibility)

> "To be straight: this is a **working vertical slice on your stack**, not a production cloud deployment — nobody should hand you that at this stage. What it proves is the **architecture and the engineering discipline**. The scale story is a *plan with triggers*. Knowing what **not** to build yet — no geometry kernel, no CDN, because your room layouts don't need them — is the part I'd want you to hold me to."

**Built vs. Proven vs. Next:**
- **Built** (running on your stack now): twin, gate, 5 lenses, grounded agent, hierarchy roll-up, operational POC.
- **Proven** (shipped in a sibling product): the UE5 immersive layer — Sophia, biometric avatars, Quest-3, Meta TRIBE v2.
- **Next** (earned with a metric): scale tiers, geometry kernel, CDN, multi-axis authz.

---

## 🔥 HARDEST QUESTIONS — CRISP ANSWERS

**"It's a desktop demo. How does it scale?"**
> "Three channels, three playbooks. **Geometry** scales like Netflix — immutable content-addressed glTF/USD at the edge, LOD as adaptive bitrate. **State** scales like Meta's feed — per-builder working set at their regional edge; instance-changes push, type-changes invalidate-not-fan-out. **AI** scales like edge inference — model + grounding co-located with data, never on the frame path. Each tier earned with a metric: CDN first, read replicas when read-latency hurts, edge working-sets for a second continent, predictive pre-warm last."

**"Consistency when two people edit at once?"**
> "Optimistic concurrency with a version token. Client may render stale, but every command re-validates against current truth before commit — stale-but-legal rebases, a real conflict rejects. A clash-check against a stale twin would be a code violation in concrete; we never allow it. Tested — CC1/CC2 on the dashboard. **Stale for the eyes, current for the gate.**"

**"Don't you need a real CAD kernel?"**
> "Not for v1, and I wouldn't buy one yet. Your docs show no kernel, collision as a concept. For dimensioned room layouts, server-side bounding-box clash + a spatial index covers it. A B-rep kernel is a *triggered* ADR — curved walls, true booleans, parametric mating — with building-coordinate precision handled by a local-origin transform. I'd want your eyes on that and on tenant-isolation first — they're the hardest to retrofit."

**"Why not just fix Unity in place?"**
> "Unity isn't the problem — **Unity-as-the-source-of-truth** is. A scene graph is where domain truth goes to become invisible to your AI: GraphRAG can't read a prefab. Invert it — twin owns truth, Unity renders and emits intents — and Unity becomes a swappable lens *and* your XR authoring differentiator. Same payload already drives the web viewer. Engine bake-off, decided by evidence."

**"What's the edge-feed layer / how does the twin stay agnostic at scale?"** (ADR-009)
> "Today the twin is rendering-agnostic and every lens pulls the same payload — proves renderer-independence, doesn't scale. Production inserts rendering-optimized **edge adapters**: content cached (immutable CDN), state synced (CQRS snapshot+delta), brain relocated (co-located regional). Not separate twins — differently-shaped reads of one event log. **One twin, many edges, many lenses; truth is shaped at the edge, never forked.** It's ADR-009 — Proposed; Phase 0 (direct-to-GraphQL) is what's running."

**"What runs in production vs. this demo?"**
> "Demo: Postgres + .NET twin + viewer, real stack, my laptop. Production adds the scaling tiers, SSO/SCIM + multi-axis authz, AI-observability, CDC-rebuildable indexes, DR for the journal — all in CONSENSUS.md with triggers. **None of it is a rewrite of what you're seeing; it's the same spine, extended.**"

---

## 🤝 THE 30–60 DAY PLAN (one breath each)

- **0–15:** authority charter signed · composition service on your Postgres · freeze contracts (MDS, twin payload, command set) as ADR-007/008 · baseline time-to-compliant-design.
- **16–35:** validation service v1 (ports + clearances + boundary, golden tests) · Room Planner reads/writes via twin · client snap + subscription · instrument correction deltas (flywheel from day 1).
- **36–60:** end-to-end on ONE flagship room type with a partner's real data · Space Bot v1 (HITL) · scorecard vs. baseline · package for conversion.
- **Deferred 61+ (and why):** Floor Planner detail, publishing UI, hardened agent, FAANG backlog — *each earned with a metric. Smaller promise, delivered.*

---

## 🎬 THE CLOSE

> "Your vision, disciplined execution, measured proof. The spine is real, runs on your stack, and it's tested. Give me one flagship room type and a design partner, and in 60 days Designer-to-Floor runs end-to-end in production — and everything else is that same spine, grown one earned step at a time. **Where would you like to go deep?**"

Then **stop talking. Let them drive.**

---

## 🧠 MINDSET
- Humble, respectful, authoritative. You studied *their* platform.
- When unsure: "Good question — here's what I'd measure before deciding." Never bluff.
- The deck/ADRs are backup, not the show. **The running code is the show.**
- Silence after the close is good. Let them fill it.
