# Agentic AEC AI — How THE CORE Federates with BIM

**Positioning (say this first):** the twin is **not** a BIM/Revit replacement. Revit owns
geometry and detailed design — 25 years of kernel engineering you will not rebuild. The twin is
the **contextual-intelligence, compliance, relationship, and operational layer** BIM lacks. They
**federate** via open standards. *Revit knows the geometry; the twin knows the rules, the
relationships, and what's compliant; the agent reasons across both.*

This also resolves the geometry-kernel question: **you consume BIM's kernel; you do not build
your own.** The twin holds AABB + spatial index + relationships + rules; BIM holds the detailed
B-rep. Clean division of labor.

---

## Division of authority

| Concern | Authority | Why |
|---|---|---|
| Detailed geometry, MEP, structure | **BIM (Revit / IFC)** | purpose-built kernel; where architects author |
| Spatial structure (building→floor→room) | **shared** (IFC ↔ twin, 1:1) | IFC spatial tree == twin containment tree |
| Rules / codes / compliance state | **Twin** | BIM doesn't run FGI/NFPA healthcare logic natively |
| Cross-system relationships, blast radius | **Twin** | the property graph + version pinning |
| Agentic reasoning (LLM + GraphRAG) | **Twin** | grounded proposal → deterministic gate → HITL |
| Live operational state (post-occupancy IoT) | **Twin** | BIM is design-time; the twin is continuous |
| Issues / coordination round-trip | **shared** (BCF) | open standard both sides speak |

Neither is master. The twin is the **brain**; BIM is the **detailed model the brain reasons over
and writes back to**.

---

## Your model is already IFC-aligned (credibility)

| Twin concept | IFC equivalent |
|---|---|
| Building → Floor → Room (containment tree) | `IfcProject → IfcSite → IfcBuilding → IfcBuildingStorey → IfcSpace` |
| Moducule instance in a room | `IfcElement` placed via `IfcRelContainedInSpatialStructure` |
| Port (med-gas provides/requires) | `IfcDistributionPort` (FLOWSOURCE / FLOWSINK) |
| Earned MED_GAS edge | `IfcRelConnectsPorts` |
| Moducule rules / attributes | `IfcPropertySet` via `IfcRelDefinesByProperties` |
| Room Moducule (reusable template) | `IfcElementAssembly` / a parametric Revit family-type |

We are the intelligence layer **over** the standard schema, not a competing one.

---

## The integration seam: one port, many adapters

Consistent with the rest of the platform — **hexagonal ports-and-adapters**. A single
`IBimAdapter` contract; one implementation per BIM source. Swap or add a source without touching
the core.

```
                         ┌──────────────────────────────┐
                         │           THE CORE            │
                         │   twin (truth) + gate +       │
                         │   GraphRAG grounding + agents │
                         └──────────────┬───────────────┘
                                        │  IBimAdapter (port)
        ┌───────────────┬───────────────┼───────────────┬──────────────────┐
        ▼               ▼               ▼               ▼                  ▼
  ┌───────────┐  ┌─────────────┐  ┌──────────┐  ┌──────────────┐  ┌─────────────────┐
  │ IFC       │  │ Autodesk    │  │ BCF      │  │ Revit plugin │  │ Modutecture     │
  │ adapter   │  │ APS / AEC   │  │ adapter  │  │ (Dynamo/.NET)│  │ proprietary BIM │
  │ (ISO16739)│  │ Data Model  │  │ (issues) │  │              │  │ adapter         │
  │ read+write│  │ GraphQL live│  │ round-   │  │ deep embed   │  │ (same contract) │
  │           │  │ Revit data  │  │ trip     │  │ Space Bot    │  │                 │
  └───────────┘  └─────────────┘  └──────────┘  └──────────────┘  └─────────────────┘
```

### The four adapters (real standards / APIs)

1. **IFC adapter** — vendor-neutral interchange (ISO 16739). Parse with **IfcOpenShell**; map the
   spatial tree → twin containment, elements → Moducule instances, `IfcDistributionPort` → ports,
   Psets → attributes. **Read** to ingest any BIM; **write** validated changes back as IFC. Works
   with *any* BIM tool, not just Autodesk. This is the standards-based MVP.

2. **Autodesk APS adapter** (Platform Services, formerly Forge) — the Autodesk-native live path:
   - **AEC Data Model API** — GraphQL, exposes granular Revit element data as a graph. *Both sides
     are GraphQL → graph-to-graph federation*, not file shuffling.
   - **Model Derivative API** — translate `.rvt` → SVF2 (Viewer), glTF (your 3D/Unity/UE5 lenses),
     or IFC. *This is how Revit geometry becomes a lens over the twin.*
   - **Data Management + Webhooks** — sync on model change.

3. **BCF adapter** (BIM Collaboration Format, buildingSMART) — the **agentic feedback loop made
   standard**. The gate finds a violation → emit a BCF topic → it appears as an issue in Revit /
   Navisworks / BIMcollab with the element, the rule (FGI 2.1-8.4), and the fix. The twin's
   intelligence flows *back into the authoring tool the architect already uses.*

4. **Revit plugin** (Dynamo / Revit .NET API) — the deepest embed: Modutecture's Space Bot inside
   Revit, proposing Moducule placements and showing compliance live as the architect designs.

Each is one `IBimAdapter` implementation. **Modutecture's proprietary BIM is just another adapter
behind the identical contract** — no special-casing in the core.

---

## Moducule ↔ BIM mapping

A **Moducule = geometry (from BIM) + rules (from the twin), bound together.**

| Layer | Source |
|---|---|
| Geometry (the headwall's shape, the bed's footprint) | Revit family / IFC element — **BIM owns this** |
| Footprint / clearance / port positions | derived from BIM geometry (or authored once) |
| Rules / behaviors (med-gas reach, clearances, compliance) | **the twin owns this** — BIM doesn't carry active healthcare logic |
| Version + pinning + blast radius | **the twin owns this** — the celebrity-write graph |

So Modutecture's "rule-bearing modular block" = a Revit/IFC family *plus* the twin's rule layer.
The geometry comes from the BIM authority; the **intelligence** is the twin's contribution. That
intelligence is the moat — anyone can model a headwall family; only the twin knows whether *this*
headwall, in *this* room, satisfies *this* code, and what breaks if you change it.

---

## The Agentic AEC AI pipeline (extended over BIM)

The same governed pipeline, now grounded across BIM + codes + twin:

```
goal ─► retrieve ─► plan ─► review ─► validate ─► approve ─► commit
         │GraphRAG over:        │gate            │HITL       │
         │ • twin graph         │(deterministic) │           ├─► twin (events)
         │ • code vectors       │                │           └─► BIM (BCF + IFC write-back)
         │ • BIM model (APS/IFC)│
```

- **retrieve** now fuses three sources: the twin's relationship graph, the code corpus (FGI/NFPA
  vectors), **and the live BIM model** (APS GraphQL / parsed IFC). The agent reasons over the
  *actual building*, not a toy.
- **commit** now writes to **two** places: the twin (events, truth) **and** back to BIM (a BCF
  issue for a flagged clash, or a validated change written as IFC). The loop closes into the
  architect's tool.

**The unchanged rule:** the LLM proposes; the **deterministic gate** is the only authority that
commits. AEC-BIM-LLM intelligence, with healthcare-grade safety. The brain proposes, the reflex
disposes, the spine remembers — now spanning BIM and the twin.

---

## Honest roadmap (none of this is built yet — it's the architecture)

| Phase | Scope | Standards / effort |
|---|---|---|
| **1. IFC read** | ingest any BIM → twin graph; run the gate over real models | IfcOpenShell; well-trodden; weeks |
| **2. BCF write-back** | gate violations → issues in Revit/Navisworks | BCF-API; the agentic loop, vendor-neutral |
| **3. APS live** | GraphQL graph-to-graph; glTF geometry into the lenses | Autodesk APS; auth + ecosystem |
| **4. Revit plugin** | Space Bot inside Revit, live compliance while authoring | Revit .NET / Dynamo; deepest embed |

Phases 1–2 are **vendor-neutral** (work with any IFC-exporting tool); 3–4 are the Autodesk-native
deep path. Sequenced, standards-based, each earned with a working integration — not a research
project. The twin is real today; this is how it reaches into the AEC industry.

---

## The line for the room

> "We don't compete with Revit — we make it *intelligent*. The twin federates with BIM through
> the open standards the industry already runs on: IFC for the model, BCF for the issues, and
> Autodesk's own GraphQL APIs for live Revit data. Our graph already maps onto IFC's spatial
> structure and port semantics, so integration is natural, not forced. The agent grounds itself in
> the *actual* BIM model plus the codes, proposes, and the deterministic gate validates — then the
> result flows back into Revit as a BCF issue the architect sees in their own tool. That's
> AEC-BIM-LLM agentic intelligence: Revit owns the geometry, the twin owns the rules and the
> reasoning, and the architect gets a copilot that's correct by construction."
