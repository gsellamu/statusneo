# Contextual Intelligence Construction
## Automating the FDE Lifecycle (80:20) for the Modutecture AEC use case, inside StatusNeo's four-loop GCC model
*Use-case scenario · AEC / Healthcare · scoping draft for the charter*

**What this is:** a use-case scenario showing how the Forward Deployed Engineer's lifecycle automates — 80% by an agentic AI swarm, 20% by the human Pod Commander — applied to Modutecture's "Continuous Contextual Construction" platform, and mapped onto StatusNeo's Clarity → Build → Velocity → Improvement loop. This is a scoping artifact, not a delivery commitment.

> **STATUS & SOURCING NOTE.** This scenario integrates June-2026 claims from the Modutecture blueprint and StatusNeo GCC material: specific tools (Perspective AI, OpenSpace, NeoLens™, TestCraft™, QueryButler™, Backstage) and metrics (14-day TTFV, 85% gross margin, 39-test correctness gate). Treat them as the **target operating model, pending verification — not validated results.** The durable layer is the loop structure and the 80:20 split; the perishable layer is the named tools and the specific numbers.

---

## PART I — The Use Case: Contextual Construction

**The problem the FDE is dropped into.** Modutecture builds a "Continuous Contextual Construction" platform for the AEC sector — an **Agentic Twin** that reasons across building codes, BIM models, and live site/sensor data, targeting high-compliance verticals (e.g., the Kaiser Permanente healthcare context). The hard part is not rendering a model; it is keeping the twin *correct and code-compliant* as reality changes — where clinical safety and building-code compliance are non-negotiable.

**The FDE's job:** embed in that environment and stand up the agentic pipeline that takes a stakeholder's intent → a code-compliant, twin-verified construction artifact — automating the mundane 80% and reserving the 20% (architecture, compliance sign-off, client trust) for the human.

**The 80:20 framing.** In 2026 the FDE no longer manually wires APIs or writes glue code. They orchestrate PODs that autonomously handle the 80% of mundane execution, leaving the 20% of high-stakes strategic reasoning and safety sign-off to the human — critical in a domain where an unsafe artifact has physical consequences.

| The AI swarm owns (80%) | The human Pod Commander owns (20%) |
|---|---|
| BIM federation, GraphRAG grounding over codes, draft generation, test authoring, site-to-twin diffing. | The architecture of the twin, the compliance interpretation, the clinical-safety sign-off, client trust. |
| Running the 14-day velocity loop to first verified artifact. | Deciding the artifact is safe to act on — the irreversible call. |

---

## PART II — Execution Across the Four Loops

StatusNeo's GCC operates as a four-loop system — Observe → Learn → Decide → Act, expressed as Clarity → Build → Velocity → Improvement.

```
        ┌──────────────────────────────────────────────────────┐
        ▼                                                      │
   ┌─────────┐    ┌─────────┐    ┌──────────┐    ┌─────────────┐
   │ CLARITY │──> │  BUILD  │──> │ VELOCITY │──> │ IMPROVEMENT │
   │ Observe │    │  Learn  │    │  Decide  │    │     Act     │
   └─────────┘    └─────────┘    └──────────┘    └─────────────┘
    Discovery      Engineering    Automation       Governance
     30:70           90:10          80:20            60:40
```

### CLARITY LOOP — Product & Experience (Observe) · AI 30% / HITL 70%
- **Capabilities:** experience & interface design, product management & strategy, AI product design.
- **AI swarm (30%):** Perspective AI runs stakeholder discovery; ingests building codes, BIM topology, and site context into a GraphRAG knowledge layer; drafts the candidate scope and spec.
- **Human FDE (70%):** frames the real problem with the client, validates scope against clinical/operational reality, authors the intent the swarm builds to.
- **Gate:** Human approves the spec and scope before any build begins.

### BUILD LOOP — Engineering (Learn) · AI 90% / HITL 10%
- **Capabilities:** product engineering, data & AI engineering, platform automation engineering.
- **AI swarm (90%):** performs BIM federation, generates the twin logic and the construction artifact via Spec-Driven Development, self-corrects against the GraphRAG-grounded codes.
- **Human FDE (10%):** makes the architecture calls and spot-reviews novel structural/clinical logic; otherwise stays out of the way.
- **Gate:** Architecture/contracts frozen as ADRs; output flows to the velocity loop.

### VELOCITY LOOP — Automation (Decide) · AI 80% / HITL 20%
- **Capabilities:** DevSecOps, SRE & CloudOps; test engineering & automation.
- **AI swarm (80%):** TestCraft™ + the Adversarial SDET run the correctness gate (the 39-test compliance suite per source); OpenSpace performs site-to-twin visual verification; canary/rolling deploys run within guardrails.
- **Human FDE (20%):** holds the ship gate — the decision that a code-compliant, twin-verified artifact is safe to release into a clinical environment.
- **Gate:** THE CORRECTNESS GATE — 100% test pass + compliance suite + human safety sign-off.

### IMPROVEMENT LOOP — Governance & Scale (Act) · AI 60% / HITL 40%
- **Capabilities:** AI & engineering governance; compliance, security & risk; observability & maturity index.
- **AI swarm (60%):** NeoLens™ (AIOps) monitors the live twin, detects drift between site and model, re-enters the loop; QueryButler™ keeps the knowledge/data layer current.
- **Human FDE (40%):** owns the governance posture, tunes the guardrails and maturity index, owns the post-incident call — then decides where to scale next.
- **Gate:** Auto-heals within policy; escalates to the human on drift or repeated failure.

---

## PART III — The FDE Lifecycle Automation Stack

*Named tools and metrics are source-attributed and pending verification.*

| Lifecycle stage | Tool (per source) | Role in the use case | Split |
|---|---|---|---|
| Discovery | Perspective AI | Stakeholder discovery; intent capture into GraphRAG. | 30:70 |
| Knowledge/data | QueryButler™ | Grounds the swarm in building codes + BIM topology. | 85:15 |
| Engineering | Spec-Driven Dev + swarm | BIM federation + twin-logic generation. | 90:10 |
| Quality gate | TestCraft™ + Adversarial SDET | 39-test correctness/compliance suite. | 80:20 |
| Visual verify | OpenSpace | Site-to-twin visual verification (reality vs. model). | 85:15 |
| Dev portal | Backstage | Golden paths, policy-as-code, self-service governance. | 80:20 |
| Observability | NeoLens™ (AIOps) | Live twin monitoring + drift detection + self-heal. | 80:20 |

**The deterministic gate — why this use case needs it.** In a clinical/AEC context, agentic speed (the 14-day loop per source) must never compromise integrity. The **deterministic correctness gate** is the non-negotiable: an artifact does not pass on an agent's say-so — it passes a compile + a compliance test suite (39 tests per source) + a human safety sign-off. This is the 80:20 made safe: the AI proposes the artifact; the human disposes on whether it is safe to build.

---

## PART IV — How the Use Case Scales: FDE Lands, GCC Scales

The Modutecture engagement is the **tip of the spear**: a single elite Pod Commander backed by an autonomous AI swarm proves the contextual-construction motion and secures architectural trust — before it scales into a full StatusNeo GCC.

| Phase | What happens | Topology |
|---|---|---|
| **Land (FDE)** | The Pod Commander embeds, runs the four loops on the Modutecture twin, hits first verified artifact, earns trust. | Embedded — 70:30 |
| **Prove** | The correctness gate + site-to-twin verification demonstrate safety to the client's compliance teams. | Embedded |
| **Expand (GCC)** | Once trusted, the offshore GCC pods scale delivery behind the FDE using the same loop OS and accelerators. | Remote — 90:10 |

**The 90-day arc (per StatusNeo GCC model):** Day 0–15 blueprint + landing zone; Day 16–45 pods live + portal active; Day 46–90 operate, measure, optimize — the GCC goes from slideware to running software.

### Open scoping questions (to resolve in the charter)
1. Which Modutecture loop is the pilot's first proof point — the Build loop (twin generation) or the Velocity loop (the correctness gate)?
2. What is the real composition of the "39-test correctness gate" for the Kaiser/clinical context, and who owns its definition — Modutecture, the client's compliance team, or the FDE?
3. Where does authoritative twin geometry live — in the Unity/BIM layer or the twin backend? (The decisive test: can a non-Unity client reconstruct a Moducule from backend data alone?)
4. Local compute (RTX 4090 nodes per source) vs. cloud G7 — which substrate for the Adversarial SDET's fuzzing load, given air-gapped/data-residency constraints?
5. Which named tools are real and licensed today vs. aspirational — Perspective AI, OpenSpace, and the StatusNeo ™ accelerators all need a build/buy/verify status before they enter the scope.

> **The one-line use case:** embed a Pod Commander + AI swarm in Modutecture's contextual-construction platform; automate 80% of the twin pipeline and reserve the 20% (architecture, compliance, clinical safety sign-off) for the human; prove it on one clinical account through a deterministic correctness gate; then scale into a StatusNeo GCC.

---

*Source attribution: tool names, metrics (TTFV, margin, 39-test gate), and June-2026 dates are drawn from user-supplied Modutecture/StatusNeo material and are pending independent verification.*
