# Who Has What — Modutecture vs StatusNeo vs Jeeth's Proposal

**Honest grounding (read before the meeting).** Modutecture already has the digital twin, the
Moducules, the reusable catalog, the rules pipeline, and the Unity experience. Do **not** pitch
"we propose the twin" — their own onboarding doc and website say they have it. What is genuinely
net-new is the layer their docs show **zero** of: a governed **agentic AI** layer, an **inline
deterministic correctness gate**, a **knowledge graph / GraphRAG**, and **BIM federation**. That
corrected story is stronger, not weaker — it shows you read their system and target real gaps.

Sources: Modutecture website (modutecture.com) + the Team Onboarding Document (esp. §1–2 scope,
§18 System Architecture). Absences below were verified by search of the onboarding doc.

---

## Column 1 — What MODUTECTURE already has (theirs; do not claim)

| Capability | Evidence |
|---|---|
| **Digital twins** of rooms + their contents, as the single source of truth | onboarding: "integrated with digital twins of rooms and their contents"; website: "we deliver digital twins, the single source of truth" |
| **Moducules** — modular, *rule-bearing* ("predefined rules and behaviors"), reusable lego-blocks | onboarding Project Overview |
| **Base Moducules (Get/Create)** — the reusable-template catalog | onboarding §18 API layer |
| **Space Bot / Designer, Room Builder / Room Planner, Catalog** | onboarding §1–2 |
| **Unity 3D Experience Layer** as the main user-facing runtime | onboarding §18 |
| **ASP/SPA rules pipeline** — Applicable Standards + Space Programming ingestion, validation, dedup, rollback (Databricks) | onboarding §2C, §18 |
| **GraphQL + REST** API layer | onboarding §18 |
| **SQL Server/SSMS + Lakebase/Postgres** storage | onboarding §18 |
| Multitenant **AAA**, **CI/CD**, **QA automation** (Cucumber/Selenium/RestAssured/AltTester, Azure DevOps) | onboarding §2D–E, §17 |
| Contextual intelligence · lifecycle two-way data flow · operational performance data · vendor-neutral | website |

**Implication:** the twin, Moducules, reusable catalog, standards pipeline, and Unity experience
are *the platform you are joining* — not things to propose. Speak about them as **"your twin,"
"your Base Moducules,"** etc.

---

## Column 2 — What the STATUSNEO pod is currently delivering (the engagement)

Per the onboarding doc, the pod's present mandate is **product + platform engineering**, not AI:

- Product/UX workflows: Room Builder/Planner/Catalog, Space Bot navigation & UI modernization
- UI platform / component library (stepper, dimension controls, reusable foundations)
- Data engineering: ASP/SPA pipeline reliability, Excel→JSON, Base Moducule extraction
- Backend/API + security: GraphQL capabilities, multitenant row-level filtering, **GraphQL → Lakebase/Postgres migration + query parity**
- Continuous QA automation + Azure DevOps traceability

**Implication:** StatusNeo is trusted to build and harden *their* product. The agentic-AI mandate
is **not yet** in scope — which is the opening for your role.

---

## Column 3 — What JEETH proposes to ADD / EXTEND (the net-new thesis)

Framed as building **on** their twin, never replacing it. Verified absent from the onboarding doc.

| Proposed addition | Why it's net-new | Honesty flag |
|---|---|---|
| **Agentic AI layer** — LangGraph orchestration + MCP tools + reflective agents; turn "Space Bot" from a designer UI into a *governed AI copilot* | onboarding shows **no** LLM/agentic/GenAI anywhere; this is StatusNeo's core DNA | **Clearest win. Lead with this.** |
| **Inline deterministic gate** — correct-by-construction validation at *placement time*, not only batch | they validate standards in a **batch** Databricks pipeline (ASP/SPA); inline design-time gating is additive | **Confirm**: is validation inline today or batch? |
| **Knowledge graph + GraphRAG** (Neo4j) — relationships, blast-radius, AI grounding | stack is SQL Server + Lakebase + Databricks; **no graph DB / RAG** | additive modeling + grounding layer |
| **Event-sourced truth + renderer-independence** — twin owns truth; Unity becomes a thin client ("the GPU") | depends on how much truth lives in Unity's scene today | **Confirm**: where does truth live — data layer or Unity scene? |
| **BIM federation** — Revit/APS (GraphQL), IFC (ISO 16739), BCF round-trip | onboarding shows **no** BIM/Revit/IFC/BCF | extends "vendor-neutral" to formal BIM interop |
| **Custom/local LLM + LoRA flywheel** — domain-tuned model as a moat | no model layer in docs | roadmap moat |
| **Safety invariant** — "the LLM proposes, the deterministic gate disposes" | makes agentic AI safe for healthcare | the governance spine |

**What the demo is:** a **reference implementation / demonstrator** of *this proposed layer* —
the agentic + gate + graph architecture, running in miniature over a faithful slice of their
domain (headwall / bed / sink / med-gas). It is **not** a claim that they lack a twin. Say so
plainly: *"This is a working model of the AI + correctness layer I'd add to your platform."*

---

## The corrected one-liners (use these in the room)

- ❌ "We propose the twin as the core."
- ✅ **"You've built the twin and the contextual-intelligence platform. I'm proposing the layer your roadmap doesn't show yet: a *governed agentic-AI* layer and an *inline correctness gate* that make the twin reason and stay correct-by-construction — and I built a working model of it to show exactly how."**
- ✅ "Your Base Moducules are the reusable catalog; I'd add the agent that *composes* from them and the gate that *validates* every placement inline."
- ✅ "Revit owns geometry, your twin owns the rules and reasoning — I'd federate them via IFC/BCF so the intelligence flows back into the architect's tool."

---

## Questions to confirm in the room (humility = credibility)

Asking these *first* de-risks the proposal and proves you think in terms of **their** system:

1. **Validation:** today, are Applicable-Standards/rules enforced **inline at design time**, or as a **batch** ASP/SPA pipeline? (Determines how additive the inline gate is.)
2. **Truth ownership:** does the twin's authoritative state live in the **data layer** (SQL/Lakebase), or partly in **Unity's scene graph**? (Determines the renderer-independence work.)
3. **AI today:** is there any **LLM/agentic** capability in Space Bot now, or is it a designer/navigation UI? (Confirms the agentic layer is net-new.)
4. **BIM:** how do you exchange **BIM/Revit/IFC** today, if at all? (Sizes the federation work.)
5. **Graph:** is there appetite for a **knowledge-graph / GraphRAG** layer alongside the relational store, for relationships + AI grounding?

Lead with curiosity about their system; *then* show the demo as "here's the layer I'd add." That
sequence — their platform first, your addition second — is what reads as senior.
