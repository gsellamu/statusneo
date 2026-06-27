# Modutecture Spine — Demo Script, 30/60-Day Plan & Scale-Path Q&A

Audience: **David Wilson (Tech Head)** + **Lead Architect**. Goal: buy-in via working
code → contract to make the platform shippable in 30–60 days. Tone: humble, respectful,
authoritative. **Lead with working software; never overclaim cloud-scale.**

---

## 0. Before the meeting (5 min, on your laptop)

> **Real-infra run** (Postgres + Neo4j + Ollama + metrics): follow **RUN_REAL.md**. The steps below are the zero-infra fallback.

```bash
cd modutecture-spine-fullstack
docker compose up --build          # Postgres + .NET twin service + live viewer
# wait for "Application started", then open:
open http://localhost:8099         # tab 1
open http://localhost:8099         # tab 2 (for the live-sync moment)
```

Fallbacks if Docker hiccups in the room (rehearse both):
- **No Docker** → `cd twin-service-dotnet && dotnet run --InMemory` then open `http://localhost:5000`. Same demo, in-memory store.
- **Show the proof first** → open `dashboard/dashboard.html` (static, always works) and walk the 39/39 + parity while the stack boots.

Keep `dashboard/dashboard.html` and `CONSENSUS.md` open in other tabs.

---

## 1. The 90-second frame (say this first)

> "I didn't study the job description — I studied your platform. Your bet is right:
> buildings should compound, not restart. The reason the Designer → Space Bot → Room →
> Floor chain doesn't hold together yet is that the **product lives between the tools**,
> and right now there's nothing there — collision is still a 'concept' in your own docs,
> because validation has no home: it can't be authoritative in the Unity client, and
> nothing server-side computes it. I built the missing piece, on your stack, and I want
> to show it running — then you tell me where I'm wrong."

---

## 2. The live demo (5–7 min) — click by click

**A. Place a legal Moducule.** Pick *Headwall HW-204*, click near the top wall.
→ status: `✓ COMMITTED seq #1 → version 1`. Journal shows the event.
> "A click isn't a mutation — it's an *intent*. The server validated against current
> truth, and only then wrote one immutable event. The renderer owns nothing."

**B. Watch an illegal one get rejected — with a cited rule.** Pick *ICU Bed*, click far
bottom-left (away from the headwall).
→ `✗ REJECTED — nothing written. [R1-boundary]… [R3-medgas] Nearest med-gas source is
3002mm away (limit 2500mm).` Journal does **not** grow.
> "This is the whole thesis. The gate is real: the rejected design left no trace, and the
> error cites the rule — a med-gas dependency, by name. *It only snaps if it's legal.*"

**C. Place it legally — and watch an edge get earned.** Pick *ICU Bed*, click just in
front of the headwall.
→ `✓ COMMITTED seq #2 → version 2 ⚠ Keep-clear zone encroaches…` A **gold med-gas line**
appears between bed and headwall.
> "Two things: it committed *with a warning*, not a block — that's the snaps-instantly /
> certify-asynchronously tier. And that gold line is a **directed edge the system earned**
> by validation — connected, never promiscuous. Delete the headwall and the edge vanishes,
> because the edge is a function of validated state, not a line someone drew."

**D. Live multi-tab sync.** Switch to tab 2 — the bed and edge are already there.
> "Two planners, one truth. Both clients render the same server state. This is polling for
> demo-robustness; the GraphQL subscription is wired for production — same payload, push
> instead of poll."

**E. A REAL grounded agent proposes; a human commits.** Click *Suggest a bed (agent)*.
Behind that click: **Neo4j** is queried for the observation-room rules and med-gas reach →
those facts + the live headwall position are sent to **mistral-nemo** (local, via Ollama) →
the model drafts a placement → our **gate validates the model's output** → it surfaces with
a rationale and **citations that came from the graph** (e.g. R3-medgas / FGI 2.1-8.4). Click
*Approve & commit*.
> "This is the Context Spine, live on my infra. The **graph grounds** the rules, a **local LLM
> proposes** using only those facts — no hallucinated parts or codes — and the **rule gate
> validates** what it proposed before a human ever sees it. The agent cannot write; approval
> is the only path to a commit. If the model proposes something illegal, the gate rejects it
> and tells you which rule — I can show you that too."

Optional kill-shot: place the headwall, run the agent, then *manually drag the bed out of
reach* and re-run — watch the model's own proposal (or yours) get **rejected with the cited
rule**. An LLM grounded by a graph and policed by a deterministic gate is the whole thesis.

**E2. Show it's really connected.** Open `localhost:5000/health`.
> "Not mocked — `journal: postgres:up`, `grounding: neo4j:up`, `brain: ollama:up`. And if any
> of these drops, it degrades to a labeled fallback instead of crashing. The service is also
> reporting to your Grafana right now — `/metrics` is being scraped by your Prometheus."

**F. (Optional) The audit trail.** Scroll the journal.
> "Every commit is one ordered, immutable row — and each placed instance pins the exact
> Moducule *version* it was validated against. A future type bump can't silently change an
> approved design; it flags it for review. That's your compliance audit trail, for free."

---

## 3. The proof (2 min) — open dashboard.html

> "Don't take the architecture on faith. **39 tests, both stacks, green.** The .NET service
> behaves identically to a reference implementation, field-for-field — so it's the
> *architecture* that works, not one lucky codebase. The validation gate runs in ~15
> **microseconds**, which is why it feels instant. And you can re-run all of it with one
> command. Categories cover the rules, the consistency boundary, idempotent retries,
> type-version pinning, event-sourced rebuild, and the HITL agent."

Point at the **consistency-boundary** line specifically:
> "This is the one I'd lose sleep over if it were wrong: two people editing the same room
> can never produce a stale-approval, because validation always runs against current truth
> while the screen is allowed to lag. Stale for the eyes, current for the gate."

---

## 4. What this is — and isn't (say it before they ask)

> "To be straight with you: this is a **working vertical slice on your stack**, not a
> production cloud deployment — nobody should hand you that at this stage. What it proves
> is the **architecture and the engineering discipline**. The scale story is a *plan with
> triggers*, not a thing I'm pretending to have already deployed. Knowing what **not** to
> build yet — I haven't added a geometry kernel or a CDN, because your room layouts don't
> need them yet — is the part I'd want you to hold me to."

---

## 5. The 30–60 day plan (make it shippable)

**Days 0–15 — Foundations & truth.**
- Sign the **authority charter** (one page: each system's authoritative-for / must-never-own); Lloyd co-owns the genome & rules.
- Stand up the **composition service** on your Postgres (finish the migration you started) with the change journal.
- Freeze the **contracts**: MDS, twin payload, command set — incl. units, axes, IDs (the coordinate transform lives in one place). Record as ADR-007/008.
- Measure the **baseline**: time-to-compliant-design today, on one real room.

**Days 16–35 — Connect & validate.**
- **Validation service v1** (ports + clearances + boundary), rules harvested from your C#/SSMS with golden tests.
- **Room Planner** reads & writes via the twin payload — same screens, new data path.
- Reflex-arc client snap (cached rule subset) for instant feedback; subscription for live sync.
- Instrument correction deltas (flywheel data from day one).

**Days 36–60 — Demonstrate & convert.**
- End-to-end on **one flagship room type** (e.g. exam room) with a design partner's real program data: Designer → registry → Room → Floor aggregate → validated → data-sheet export.
- **Space Bot v1** as a thin agent over the substrate (HITL).
- Publish the scorecard vs. baseline; package the pilot for conversion.

**Deliberately deferred to days 61+** (and why): Floor Planner full detail, Designer publishing UI, hardened agent, the FAANG-grade backlog (spatial index, multi-axis authz, CDC indexes) — each earned with a metric, sequenced after the spine is stable. *Smaller promise, delivered.*

---

## 6. The Lead Architect's scale-path Q&A (prepared answers)

**Q: "This is a desktop demo. How does it scale to many hospitals and concurrent builders?"**
> "Three channels, three playbooks. **Geometry** scales like Netflix — immutable,
> content-addressed glTF/USD at the edge, LOD as adaptive bitrate. **State** scales like
> Meta's feed — a per-builder working set materialized at their regional edge;
> instance-changes push, type-changes invalidate-not-fan-out. **AI** scales like edge
> inference — warm model and grounding co-located with the data, never on the frame path.
> I earn each tier with a metric: CDN first, read replicas when geographic read-latency
> hurts, edge working-sets for a second continent, predictive pre-warm last."

**Q: "What about consistency when two people edit at once?"**
> "Optimistic concurrency with a version token. The client may render stale, but every
> command is re-validated against current truth before commit — stale-but-legal rebases,
> a real conflict rejects. A clash-check against a stale twin would be a code violation in
> concrete; we never allow it. That's tested — CC1 and CC2 on the dashboard."

**Q: "Spikes — a whole team editing before a submission deadline?"**
> "The load is wildly asymmetric: interactions ≫ reads ≫ writes, and geometry bytes dwarf
> state bytes. So the common case never touches the authoritative store — the CDN absorbs
> geometry, optimistic T0/T1 absorbs interaction, and the only real bottleneck, the
> per-room write path, gets a stateless validator pool (the validator is pure) and per-room
> queue with backpressure. The type-change cascade is handled by invalidate-not-fan-out so
> one standards revision can't melt the platform."

**Q: "Geometry — don't you need a real CAD kernel?"**
> "Not for v1, and I wouldn't buy one yet. Your docs show no kernel and collision as a
> concept; for dimensioned room layouts, server-side bounding-box clash plus a spatial
> index covers it — which is what the reference does. A B-rep kernel is a *triggered* ADR
> for curved walls, true booleans, or parametric mating, with floating-point precision at
> building coordinates handled by a local-origin transform. I'd want your eyes on that
> decision and on the tenant-isolation tier first — they're the two hardest to retrofit."

**Q: "Why not just fix Unity in place?"**
> "Unity isn't the problem — Unity-as-the-source-of-truth is. A scene graph is where domain
> truth goes to become invisible to your AI: GraphRAG can't read a prefab. Invert it — the
> twin owns truth, Unity renders it and emits intents — and Unity becomes a swappable lens
> *and* your XR authoring differentiator. The same payload already drives the web viewer
> you're looking at; that's the engine bake-off, decided by evidence, not opinion."

**Q: "What runs in production vs. what's in this demo?"**
> "Demo: Postgres + the .NET twin service + this viewer, real stack, on my laptop.
> Production adds the scaling tiers above, SSO/SCIM and multi-axis authz, observability
> incl. AI-observability, CDC-derived rebuildable indexes, and the resilience/DR story for
> the journal — all in CONSENSUS.md as design-of-record with triggers. None of it is a
> rewrite of what you're seeing; it's the same spine, extended."

---

## 7. The close

> "Your vision, disciplined execution, measured proof. The spine is real, it runs on your
> stack, and it's tested. Give me one flagship room type and a design partner, and in 60
> days Designer-to-Floor runs end-to-end in production — and everything else we discussed
> is that same spine, grown one earned step at a time. Where would you like to go deep?"

Then **stop talking** and let them drive.


---

## 8. Lens Studio + Unity/UE5 — the renderer-independence showpiece

Open **http://localhost:5005/studio.html** (the proven simple viewer stays at `/`).

Split-screen, multiple lenses over the **same twin**, all live: **2D plan · Schematic (SVG) ·
3D (Three.js) · Data table**. Toggle lenses with the checkboxes. Place a Moducule in the 2D
pane → **every lens updates together**, because none of them owns the truth.

Sync indicator (top-right): **● LIVE (push)** when the GraphQL subscription is delivering
deltas, **● polling** as the always-on baseline. `Δ N` counts pushed deltas.

> "Same twin, four renderers, updating together. The 2D plan, an architectural schematic,
> a real 3D view, and the raw data — all the same GraphQL payload hitting different draw
> code. Switch them live. None of them holds state; they're windows onto the twin. That's
> the whole architecture in one screen: **the renderer is swappable.**"

**Then the kicker — real engines as lenses (you run these):**
- Bring up your **Unity Editor** project (the `unity-client/` code: TwinClient/RoomRenderer/
  IntentEmitter) pointed at `localhost:5005`, beside the browser.
- Bring up your **UE5** project as a third real engine on the same backend.
- Place something in the web 2D pane → it appears in **Unity and UE5 too**.

> "And it isn't just web. Here's your actual Unity project, and Unreal, both reading the same
> twin. A 2D web canvas, a 3D web view, Unity, and Unreal — four engines, one source of truth,
> none of them owning a single rule. Your Unity team keeps the rich authoring experience; it
> just gets thinner and more powerful because it sheds the domain logic a scene graph was
> never meant to hold."

**Honesty note for the room:** the web lenses are real and live; Unity/UE5 are your real
engines wired to the same GraphQL contract. On one laptop everything is localhost, so it's
genuinely instant. At scale, each lens is fed from a regional **edge cache** so it *stays*
instant for a builder in another city — that's the three-channel design in CONSENSUS.md,
earned with a metric, not faked here.

*If the 3D pane is blank: it needs the Three.js CDN (internet). The other lenses are
unaffected — say "3D needs network; here it is on my phone hotspot" or just demo the others.*


---

## 9. Principal-grade capstone: self-describing platform + reflective agents

**A. The platform describes itself.** Open **http://localhost:5005/architecture.html**.
It renders live from `/capabilities` + `/health` — the running system telling you its own
plug-points, wiring, genome, rules, patterns, and scaling design.

> "This isn't a slide — it's the service describing itself. Every seam is a port with
> swappable adapters: the journal is Postgres or in-memory; grounding is Neo4j or a vector
> store; the brain is deterministic, a single LLM, or a multi-agent planner; the renderer is
> any of five lenses. The green dots are *live* — health-probed. This is hexagonal
> architecture, and it's why everything is plug-and-play: nothing in the core knows or cares
> which adapter is wired."

**B. Plug-and-play, proven by config — not asserted.** The agent brain is a port with three
real implementations. Swap it without touching code:
```powershell
.\scripts
un.ps1 -Port 5005 -AgentMode reflective    # planner → reviewer → gate
# vs -AgentMode ollama (single-shot)  vs  -AgentMode deterministic (no LLM)
```
Refresh `/architecture.html` → the **Brain** port now shows `Reflective (planner→reviewer)`
active. Same contract, swapped implementation, zero code change.

> "Watch — I change one config value and the reasoning engine swaps from a single LLM call
> to a two-agent planner-reviewer loop, and the platform map updates to show it. The seam is
> real; the adapters are interchangeable. That's how you add LangGraph or a cloud model later
> without a rewrite."

**C. The reflective agent — agentic-era reasoning, still gated.** With `-AgentMode reflective`,
click *Suggest a bed*: a **Planner** drafts a placement, a **Reviewer** critiques it against
the rules and corrects it, *then the deterministic gate has the final say*.

> "Two agents collaborate — a planner and a reviewer — so the AI catches its own mistakes
> before a human sees them. But notice the order: the LLM reviewer is advisory; the
> deterministic gate is authoritative. AI reasoning improves the proposal; it never *becomes*
> the safety mechanism. In healthcare, that distinction is everything."

This is the principal signal: not more features, but **clean seams, swappable by config,
proven live — and the judgment to keep the deterministic gate above the LLM.**


---

## 10. The composition hierarchy — Moducule → Room → Floor → Building (live roll-up)

Open **http://localhost:5005/hierarchy.html** beside a planner tab.

This is Modutecture's Room/Floor/Building Planner problem, made real: a live roll-up over the
composition graph. Each room's compliance is computed from its twin; floors and the building
roll up from their children — in real time.

**Demo the propagation (the bar-riser):**
1. Open the planner on a hierarchy room: **http://localhost:5005/?room=icu-101**
2. Place a Headwall + a Bed in front (bed earns its med-gas edge) → in the Hierarchy tab,
   **ICU Room 101 turns COMPLIANT**, and Floor 1 updates.
3. Open **/?room=icu-102**, place just a Bed (no headwall) → it can't earn med-gas →
   **the room goes AT RISK, and the whole floor rolls up to AT RISK.**
4. Watch the **Building** badge change as floors change.

> "A change in one room propagates up to the floor and the building, live. That's the
> composition graph — the same directed, validated structure, now at portfolio scale. This
> is your Room-to-Floor-to-Building chain, working, with compliance rolling up automatically."

**The impact panel = change-blast radius (the celebrity write, visualized).**
> "And here's the operational payoff: every Moducule version is mapped to the rooms using it.
> Revise Headwall HW-204 to v2.4, and *these* are the exact rooms that flag for review — not
> a guess, a query. That's how a standards change propagates through a portfolio without
> melting it: invalidate-and-review, scoped to the blast radius."

**Friction-free rehearsal:** `.\scripts\demo-reset.ps1` clears all rooms + reseeds in one
command between practice runs. Room ids `icu-101 / icu-102 / icu-103 / exam-201 / exam-202`
drive the hierarchy.


---

## 11. The bar-raiser: operational twin + the immersive roadmap (honest)

This is where you separate from every other candidate — by showing the architecture extends
from *designing* hospitals to *operating* them, backed by code you've already shipped.

**A. Operational twin — live telemetry POC.** Start with `-Redpanda` to light the bus:
```powershell
.\scriptsun.ps1 -Port 5005 -Redpanda
.\scripts	elemetry-sim.ps1 -Port 5005 -UseRedpanda   # second terminal
```
Open **http://localhost:5005/telemetry.html** → live occupancy / temperature / CO₂ per room,
comfort status, updating in real time. `/telemetry` shows `busEnabled: true`.

> "The same twin that validates a design also carries the building's *operational* signals —
> occupancy, environment — streaming over Redpanda. This is a POC, clearly, but it's a real
> pipeline: sensors → event bus → twin → live view. The design twin and the operational twin
> are the same spine."

*(Robustness: the sim also POSTs over HTTP, so the lens shows data even if the bus hiccups.)*

**B. The immersive roadmap — your proven UE5 work.** Open **http://localhost:5005/vision.html**.

> "And here's where it goes — and why I can say it credibly. I've already shipped real-time,
> biometric-driven avatar experiences in UE5 for a clinical product: Sophia, with live gaze,
> breathing, lip-sync, backend streaming, Quest-3 optimization, and Meta TRIBE v2 session
> monitoring. That immersive engine becomes another *lens* onto this operational twin —
> patient and clinician avatars in an occupied room, fed by the live signals you just saw.
> It plugs into the same contract; it's weeks of integration on proven parts, not research.
> That closes the loop: every operated room teaches the next design, and that insight flows
> back to your leaders and to us as FDEs. That's Context Intelligence that compounds."

**The honesty that makes it land** (say it): *built* = stages 1–2, running on your stack now;
*proven* = the UE5 immersive layer, shipped in a sibling product; *roadmap* = wiring them
together, earned with a metric. No faked simulation — real parts, real bridge, honest scope.

This is the principal move: a vision a CTO can fund, every claim inspectable or backed by
prior art, and the discipline to label exactly what's built vs. proven vs. next.
