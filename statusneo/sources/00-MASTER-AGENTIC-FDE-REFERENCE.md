# The StatusNeo Agentic FDE Practice — Master Reference
## Charter · Operating Model · Lifecycle · Tooling · Security · Architecture · AEC Use Case
*Master v1 · consolidated June 2026 · companion documents indexed at the end*

**What this is.** A single master reference consolidating the Forward Deployed Engineering practice: the strategic thesis, the 80:20 (AI:Human) operating model, the agentic lifecycle, the multi-cloud tooling directory, the 2026 security and CI/CD standards, the Modutecture AEC enterprise architecture, the contextual-construction use case, and the Kaiser healthcare correctness gate — wrapped in the StatusNeo four-loop GCC execution engine.

**Who it's for.** The FDE practice lead, the StatusNeo sponsor, and the Modutecture technical stakeholders. A working charter-and-scoping reference, not a delivery commitment.

> **READ THIS FIRST — sourcing & verification.** This document consolidates June-2026 source briefings (Modutecture, StatusNeo GCC, the multi-cloud ecosystem). It names specific products, metrics, and dates — AgentCore, Entra Agent IDs, the 7-layer architecture, FGI 2026, 14-day TTFV, 85% margin. **Treat them as the target operating model, pending verification — not validated results.** Durable: the structure (loops, 80:20, architecture). Perishable: the named tools, metrics, and dates.

---

## Contents
- **Part I — The Thesis:** the gap, the Elite Pod Commander, land-and-expand
- **Part II — The 80:20 Operating Model:** the two halves of the FDE, roles, the RACI
- **Part III — The Agentic FDE Lifecycle:** the 7-stage SDLC and the four-loop GCC expression
- **Part IV — The Multi-Cloud Tooling Directory:** Generic/AWS/GCP/Azure, Embedded vs. Remote
- **Part V — Agentic Security & Guardrails:** perimeter-level security across the three clouds
- **Part VI — CI/CD for Remote PODs:** SLSA Level 3 pipeline and deployment gates
- **Part VII — The Modutecture AEC Architecture:** the 7-layer stack and the 4 governance pillars
- **Part VIII — The AEC Use Case:** the four loops applied + the FDE 80:20 case study
- **Part IX — The Kaiser Correctness Gate:** FGI 2026, NFPA 99 med-gas, HIPAA
- **Part X — The GCC Execution Engine:** the 90-day roadmap and the Enterprise OS Fabric™
- **Part XI — Roadmap, Benchmarks & Open Questions**

---

## Part I — The Thesis

Every enterprise wanting AI transformation faces the same problem: the gap between a brilliant framework and a landed, trusted production system. What is missing is the **onshore tip of the spear** that lands those frameworks inside the customer, de-risks the handoff, and unlocks the regulated, air-gapped enterprises (healthcare, BFSI, defense) that structurally will not buy pure-offshore delivery.

**The Elite Pod Commander model.** The 2026 FDE has evolved from a custom coder into an **Elite Pod Commander** who orchestrates an autonomous AI swarm. Per the source, this lets a single FDE manage pipelines that previously required 5–10 manual engineers — replacing a heavy-headcount consulting team with one commander backed by standardized state machines.

| The old model | The Agentic FDE model (per source) |
|---|---|
| A 10-person team, multi-year headcount commitment. | One Elite Pod Commander + an autonomous AI swarm. |
| Custom glue code, manual integration, slow trust-building. | Spec-Driven Development, localized compute, a 14-day velocity loop. |
| Time-to-first-value in 3–6 months. | Time-to-first-value targeted at 14 days (per source, to verify). |

**Land-and-expand: FDE lands, GCC scales.** The FDE is a **mandatory precursor**, not a competing silo. The Pod Commander proves architectural trust in a short, high-intensity engagement; once trusted, the offshore StatusNeo GCC scales delivery behind them. The FDE earns the relationship; the GCC scales the volume.

> **The governing principle (carried throughout):** the model proposes; a deterministic gate disposes. The swarm gives the velocity of a team; the human Pod Commander gives the accountability of a senior engineer. In a regulated enterprise, that gate — the human's 20% — is the whole point.

---

## Part II — The 80:20 Operating Model

The FDE is a two-part entity: the **Agentic FDE** (the 80% — velocity, breadth, repetition) and the **Human FDE / Pod Commander** (the 20% — judgment, trust, architecture, governance). The 80:20 is a leverage ratio, not a uniform rule: the human owns the front (intent + architecture) and the end (the ship gate); the swarm owns the dense middle.

**The Agentic FDE — the 80% (the swarm roles):** Spec Architect (intent→plan, stages 1–2), Systems Engineer (memory-safe Rust, stage 3), Adversarial SDET (tests + 100% coverage, stage 4), Security Sentinel (SAST/compliance, stage 5), DevOps Overseer (zero-trust deploy, stage 6), Reliability Agent (monitor + hotfix, stage 7), Platform Architect (Spoke→Hub abstraction, cross-cutting).

**The Human FDE — the 20% (judgment-owned):** diagnosis & architecture; client trust & evangelism; spec authorship & intent; governance & the ship gate.

**The RACI — accountability never automates.** The human is *Accountable for every stage* even where the AI is Responsible for the work. (Full per-stage RACI in the companion *FDE 80:20 Roles & Lifecycle* document.)

---

## Part III — The Agentic FDE Lifecycle

Autonomous agents execute a closed-loop lifecycle — pulling tasks from a backlog, pushing verified code to production. Production telemetry feeds back into planning, so the system self-heals and re-plans without a human restart.

```
[1. Context & Plan] ──> [2. Architecture] ──> [3. Implementation] ──> [4. Agentic QA]
        ▲                                                                  │
        └──────────── [7. Monitor & Self-Heal] <── [6. CD] <── [5. Security] ◄┘
```

**The four-loop GCC expression** (LoopManifesto: Observe → Learn → Decide → Act):

| Loop | Phase | Focus | Stages |
|---|---|---|---|
| Clarity | Observe | Product & experience — discovery, strategy, AI product design. | 1–2 |
| Build | Learn | Engineering — product, data & AI, platform automation. | 3 |
| Velocity | Decide | Automation — DevSecOps, SRE, CloudOps, test automation. | 4–6 |
| Improvement | Act | Governance & scale — compliance, observability, maturity index. | 7 |

**Two topologies, one codebase:** Embedded (~70:30, human approves merge, legacy/regulated) vs. Remote (~90:10, automated gates, greenfield). (Full blueprint/playbook/cookbook/runbook in the companion *Agentic FDE Lifecycle* document.)

---

## Part IV — The Multi-Cloud Tooling Directory

Cloud-agnostic core, cloud-native bindings. One orchestration codebase; four interchangeable substrates. *[Product names and dates per source, pending verification.]*

| Pillar | Generic / OSS | AWS | GCP | Azure |
|---|---|---|---|---|
| Orchestration | LangGraph, CrewAI, AutoGen | Bedrock AgentCore / Flows | Gemini Enterprise Agent Platform | Azure AI Agent Service |
| Compute Sandbox | E2B, Docker, Fly.io | Lambda/ECS; AgentCore microVMs | Cloud Run v2 (gVisor) | Container Apps Dynamic Sessions |
| Context / Memory | Greptile, Qdrant | AWS Context Service | Agent Engine "Memory Banks" | Cosmos DB Agent Memory |
| Code Models | DeepSeek-Coder, Llama-3 | Anthropic Claude | Gemini 1.5 / 3 | Azure OpenAI GPT / o-series |
| Compute (per src) | Local RTX 4090 nodes | EC2 G7 (Blackwell) | TPU 8t / 8i | — |

**Embedded vs. Remote crosswalk:** persona (developers/team-leads vs. DevOps/SRE/PO); interface (IDE/Slack/GitHub vs. API gateway/event bus); tools (Kiro/Foundry-VSCode/Strands vs. AgentCore/GCP Agent Runtime); acceptance (human review vs. automated evals/canary).

---

## Part V — Agentic Security & Guardrails

In 2026 the industry has shifted to **perimeter-level security**, where guardrails live in the cloud gateway rather than inside the agent's application code — so they cannot be bypassed by agent-logic failures. *[Vendor features and dates per source, pending verification.]*

| Concern | AWS Bedrock AgentCore | Azure AI Foundry | GCP Gemini Platform |
|---|---|---|---|
| Prompt injection | Gateway-native: blocks injection before the model. | Cross-prompt classifier scans tools/email triggers. | Adaptive filters in a Gemini-native SOC. |
| PII & data | InvokeGuardrail API — character-offset redaction. | Microsoft Purview — end-to-end DLP. | Zero Data Retention commitment. |
| Identity | IAM Agent Roles — scoped tool permissions. | Entra Agent ID — eliminates "shadow agents". | Workspace Studio — integrated RBAC. |
| Monitoring | AgentCore Observability — logs every policy eval. | Agent Registry — centralized behavior audit. | A2A Audit — cross-cloud call tracking. |

**2026 best practices:** (1) strict 100% input/output validation for critical traffic; (2) decoupled policy enforcement via cloud-native gateways; (3) deterministic pre-checks (regex) before slow LLM evaluators; (4) agent identity — agents as autonomous identities with their own RBAC.

---

## Part VI — CI/CD for Remote PODs

Deploying a Remote FDE POD requires a pipeline with mandatory security and evaluation gates before production. Per source, **SLSA Level 3** is the minimum for any agent touching production. (Implementation-level config held for the build phase; this is the gate model.)

| Stage | Gate | What it enforces |
|---|---|---|
| 1. Security audit | Secret scanning + config validation | Entropy analysis for LLM keys (TruffleHog); validate guardrail policy files against schema. |
| 2. Evaluation gate | Batch evals + groundedness | Catch reasoning regressions; fail if groundedness < 0.95 (2026 standard). |
| 3. Deploy | Isolated runtime + agent identity | Deploy to isolated compute; register the agent identity (Entra Agent ID). |

**Essential gates:** secret management (Vault/Secrets Manager + OIDC just-in-time); supply-chain integrity (SLSA L3 minimum); automated rollbacks (A/B splitting + auto-rollback on guardrail spikes); SLA compliance (time-to-first-integration ≤ 14 days, production within 90 days, per source).

---

## Part VII — The Modutecture AEC Architecture

The Modutecture Enterprise Architecture (per source) transforms static models into **Governed Agentic Engines**. The Digital Twin is the authoritative source of truth; a multi-agent layer handles reasoning, validation, and real-time operations. The foundation is domain-agnostic AEC, specialized for the Kaiser Permanente healthcare vertical.

**The 7-layer production stack:**

| Layer | Component | Function (per source) |
|---|---|---|
| 1. Experience | Interchangeable renderers | Unity/UE5/Web act as thin-client lenses; the renderer "owns nothing". |
| 2. Domain Context | Vertical packs | Healthcare ontology (med-gas, clinical room types) and tuned LoRAs. |
| 3. AEC Context | Intelligence foundation | Geometry, spatial semantics, collision prediction (GraphRAG). |
| 4. Agentic AI | Orchestration | Brain proposes, gate disposes: LangGraph + MCP tools for clinical workflows. |
| 5. Twin & Knowledge | Core memory | Event-sourced twin (source of truth) + semantic memory (Neo4j). |
| 6. Platform Services | Seams & contracts | GraphQL + REST APIs with a CQRS command/query split. |
| 7. Data & Processing | Polyglot persistence | Redpanda event backbone, Databricks, SQL/NoSQL storage. |

**The 4 governance pillars (cross-cutting):** business & stakeholder context (outcome metrics like "time-to-compliant-design"); security/identity/multitenancy (HIPAA/PHI + RBAC via Entra Agent IDs); governance/safety/compliance (the Deterministic Gate with binding authority over FGI/ADA/IBC); DevOps/observability/quality (SLSA CI/CD + OpenTelemetry agent metrics).

**Three implementation insights:** (1) the renderer is a lens — Unity/UE5 are an "execution lens" (GPU); the agentic engine owns the logic (open question: where authoritative geometry lives — see Part XI); (2) correctness-by-construction — the deterministic gate prevents illegal placements in real time; (3) A2A interoperability — A2A Protocol + MCP plug into IFC and BCF.

---

## Part VIII — The AEC Use Case: Contextual Construction

Embed a Pod Commander + AI swarm in Modutecture's "Continuous Contextual Construction" platform; automate 80% of the twin pipeline (federation, grounding, generation, testing, verification) and reserve the 20% (architecture, compliance, clinical safety sign-off) for the human; prove it on one clinical account through a deterministic correctness gate; then scale into a StatusNeo GCC.

**The 14-day velocity loop (the FDE 80:20 case study, per source):**

| Phase | Autonomous swarm (80%) | Pod Commander (20%) | Tools (per src) |
|---|---|---|---|
| Discovery (Days 1–3) | SDD compilation: maps stakeholder needs to strict Gherkin specs. | C-suite alignment; defines the "why." | Perspective AI |
| Engineering (Days 4–10) | Swarm generates memory-safe Rust binaries. | Architectural oversight; HIR < 2 commits/1k LOC. | Agentic Workbench™ |
| Testing & Sync (Days 4–10) | Adversarial SDET → 100% coverage before human review. | Resolves BIM-vs-clinical clashes. | OpenSpace, TestCraft™ |
| Deployment (Days 11–13) | DevOps Agent deploys via zero-trust templates. | Scopes GCC scale-out; abstracts IP to the Hub. | NeoLens™ |

**The four loops, applied:** Clarity (30:70 — discovery into GraphRAG, human authors intent); Build (90:10 — BIM federation + twin-logic via SDD, human makes architecture calls); Velocity (80:20 — TestCraft™ + Adversarial SDET run the 39-test gate, OpenSpace verifies site-to-twin, human holds ship gate); Improvement (60:40 — NeoLens™ monitors drift, human owns governance).

**FDE performance benchmarks (per source):** TTFV reduced from 3–6 months to 14 days; gross margin targeted 85%+ by scaling compute (RTX 4090 nodes) not headcount; one Commander per client.

---

## Part IX — The Kaiser Correctness Gate

In a clinical environment, agentic speed must never compromise integrity. The **Deterministic Gate** has binding authority over building codes — an artifact passes only on a compile + a compliance suite + a human safety sign-off, never on an agent's say-so. The 2026 healthcare-compliance landscape has shifted toward **deterministic safety**, where mandatory requirements are decoupled from supplemental guidance to enable automated verification.

> **A note on the regulatory claims.** FGI Guidelines, NFPA 99, ADA, IBC, and HIPAA are real, established frameworks, and healthcare codes update on multi-year cycles. The specific 2026 editions and dates below are per source — confirm the exact current editions and effective dates with the client's compliance team before they enter a binding gate.

| Requirement | Standard | Gate behavior |
|---|---|---|
| Med-gas systems | NFPA 99 (into the Joint Commission's new "Physical Environment" chapter) | Validate med-gas placement and clinical-room requirements automatically. |
| Patient privacy | HIPAA — incl. SUD record updates (Feb 16, 2026 per source) | Enforce PHI safeguards and privacy-by-design in the artifact. |
| Design codes | FGI 2026, ADA, IBC | Prevent illegal placements in real time (correctness-by-construction). |

The deterministic gate is the 80:20 made safe for healthcare: **the AI proposes the contextual artifact; the human disposes on whether it is safe to build.**

---

## Part X — The GCC Execution Engine

The StatusNeo GCC Blueprint transforms offshore centers from "shared services" into intelligent systems that improve themselves, using the Agentic FDE as the mandatory precursor that proves trust before the GCC scales.

**The 90-day execution roadmap:** Day 1–30 **The Forge** (define MCP toolsets, establish local compute, map telemetry); Day 31–60 **Proving Grounds** (run 14-day Agentic FDE pilots, tune HIR); Day 61–90 **Global Launch** (publish sanitized case studies, scale out via product pods).

**The Enterprise OS Fabric™ (6 layers, per source):** Agentic Workbench™ (governed sandboxes / RTX 4090 nodes); NeoLens™ (observability & AIOps); TestCraft™ (quality / Adversarial SDET loops); QueryButler™ (NL query of the BIM source of truth); NeoArkitect™ (golden paths / zero-trust templates); Enterprise OS Fabric™ (the SDLC blueprint connecting pilot to scale-out).

**Measured outcomes (per source):** +40–60% faster release velocity (SDD); −50% MTTR (autonomous AIOps); functional product pods live in ≤ 90 days.

---

## Part XI — Roadmap, Benchmarks & Open Questions

**The open scoping questions (Day-1 priorities):**
1. **Geometry ownership.** Does authoritative Moducule geometry live in the Unity/BIM lens or the twin backend? Decisive test: can a non-Unity client reconstruct a Moducule from backend data alone?
2. **The 39-test gate.** What is the real composition of the correctness gate for the Kaiser/clinical context, and who owns its definition — Modutecture, the client's compliance team, or the FDE?
3. **Compute substrate.** Local RTX 4090 nodes vs. cloud G7 for the Adversarial SDET's fuzzing load, given air-gapped/data-residency constraints?
4. **Rules authority.** Where does rules authority split between Unity-side behaviors and the batch (Databricks) pipeline?
5. **Tool build/buy/verify.** Which named tools are real and licensed today vs. aspirational — Perspective AI, OpenSpace, and the StatusNeo ™ accelerators all need a status before they enter scope.

**The honesty discipline (non-negotiable).** Throughout this suite, claims are labeled by confidence and every June-2026 product/metric/date is flagged pending verification. The Modutecture engagement is framed as a **validated demonstration, not a closed-and-scaled account**. Protect that precision — it is the integrity signal that wins regulated rooms.

**Companion documents (the full suite):**

| # | Document | What it holds |
|---|---|---|
| 01 | FDE Charter Proposal | The strategic case + change-leadership + the Copilot×Twin toolbox. |
| 02–03 | Loop Cookbook + Landing Runbook | The methodology and the operational field manual. |
| 05 | Agentic FDE Lifecycle | The 7-stage SDLC: blueprint, playbook, cookbook, runbook. |
| 06 | FDE 80:20 Roles & Lifecycle | The role split, per-stage ratios, and the full RACI. |
| 07 | Multi-Cloud Tooling Directory | Generic/AWS/GCP/Azure, Embedded vs. Remote. |
| 08 | AEC/Modutecture Use Case | Contextual construction across the four loops. |
| — | PR-FAQ Playbook + Deck | The Amazon-style narrative and the board deck. |

> **The whole thing, in one line:** a single Elite Pod Commander, backed by a governed AI swarm, automates 80% of the AEC twin lifecycle and reserves the judgment-bound 20% — lands one regulated account through a deterministic correctness gate, then scales it into a self-improving StatusNeo GCC. The AI proposes; the human disposes; the gate is the point.

---

*The StatusNeo Agentic FDE Practice · Master Reference v1 · June 2026 · named tools, metrics, and dates per source, pending verification.*
