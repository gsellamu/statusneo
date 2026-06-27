# The Agentic Pipeline — LangGraph + MCP + GraphRAG

**The one rule that governs everything below:** the AI *reads* the knowledge graph (grounding)
and *proposes* changes; the **deterministic gate** is the only thing that may *write the twin*.
The brain proposes, the reflex disposes, the spine remembers. AI is a governed spoke — never
the system of record.

This is the **third graph** in the system, and it is a *control-flow* DAG, not a data graph.

---

## The three graphs, kept separate

| | What it is | Role | Who writes it |
|---|---|---|---|
| **Twin graph** | Building→Floor→Room→Instance (tree) + Room-Moducule composition (DAG) + earned edges | **Truth** / source of record | the **gate** only |
| **Knowledge graph** | Codes (FGI/NFPA), rules, ports, reach limits | **Grounding** (GraphRAG retrieval) | curators / ingestion |
| **LangGraph pipeline** | retrieve → plan → review → validate → approve → commit | **Orchestration** (control flow) | n/a (it's behaviour) |

The pipeline **reads** the knowledge graph and **writes** the twin graph — and the write path
runs through the gate. That single arrow direction is the whole safety story.

---

## The pipeline (LangGraph state machine)

```
            ┌─────────────────────────────────────────────────────────────┐
            │                     LangGraph (control flow)                 │
            │                                                              │
  goal ───► retrieve ──► plan ──► review ──► validate ──► approve ──► commit ──► END
            │  ▲           │         │           │ (gate)     (HITL)     │       │
            │  │           └────◄────┘           │                       │       │
            │  │         reflection loop      FAIL│ (bounded retries)    │       │
            │  │                                  └──────► back to plan   │       │
            └──┼──────────────────────────────────────────────────────────┼──────┘
               │                                                          │
        reads ▼ (GraphRAG + vector RAG)                          writes ▼ (events)
        ┌───────────────┐                                        ┌───────────────┐
        │ KNOWLEDGE     │   MCP tools the agent calls:           │  TWIN (truth) │
        │ graph (Neo4j) │   • grounding.retrieve  (read KG)      │  event store  │
        │ + code vectors│   • twin.read           (read truth)   │  + gate guard │
        └───────────────┘   • gate.validate       (the authority)└───────────────┘
                            • twin.commit          (write, gated)
```

### Nodes

1. **retrieve** — GraphRAG over the knowledge graph (`grounding.retrieve` MCP tool):
   pull the rules, ports, and reach limits that govern this room program. Optionally fuse with
   **vector RAG** over the code PDFs (FGI/NFPA) for citations. Output: a grounded fact set.
2. **plan** — the Planner LLM proposes a placement, *constrained to the retrieved facts only*
   (no free-form invention). Output: a candidate placement + rationale.
3. **review** — the Reviewer LLM critiques the candidate against the same rules and corrects it
   (the reflection loop). This catches the LLM's own mistakes before a human sees them.
4. **validate** — the **deterministic gate** (`gate.validate` MCP tool). This is the authority.
   The LLM's self-critique is advisory; the gate's verdict is binding. On `FAIL`, loop back to
   **plan** with the violation as feedback (bounded retries, then surface to the human).
5. **approve** — human-in-the-loop. A clinician/planner approves the gated proposal.
6. **commit** — `twin.commit` appends the event to the twin. The only write path. Propagates to
   every lens via the subscription.

### Why each piece is the right tool

- **LangGraph** — gives you an explicit, inspectable state machine with loops (reflection,
  bounded retries) and checkpoints (HITL). You can see and audit every transition. A linear
  chain can't express "critique, then maybe re-plan, then require human sign-off."
- **MCP tools** — the agent's capabilities are *tools*, not hard-coded calls: `grounding.retrieve`,
  `twin.read`, `gate.validate`, `twin.commit`. This is the same ports-and-adapters discipline as
  the rest of the platform — swap the grounding store or the gate without touching the agent.
- **GraphRAG (not just vector RAG)** — healthcare rules are *relational* ("a bed REQUIRES a
  med-gas port that a headwall PROVIDES, GOVERNED_BY rule R3"). A graph retrieval returns the
  connected sub-graph of constraints; pure vector similarity would miss the relationships. Fuse
  the two: graph for structure, vectors for the prose of the codes.

---

## How it maps to what's already built

The reference implementation already has the seams this pipeline needs:

- `IGroundingStore` (Neo4j) = the **retrieve** node's backend (GraphRAG).
- `IAgentBrain` with the **reflective** mode (planner→reviewer) = the **plan**/**review** nodes.
- `Validator` (the gate) = the **validate** node — already authoritative, already gating the
  agent's output today.
- The event store + `agentSuggest` (propose-only) + human commit = **approve**/**commit**.

LangGraph + MCP would formalize the orchestration that the service performs in-process today —
the *same* directed flow, made into an explicit, checkpointed, auditable state machine. That is
the 30–60 day evolution, on proven seams, not a rewrite.

---

## The line for the room

> "AI is the cortex of this system — it reasons, it grounds itself in the codes via GraphRAG,
> it critiques its own proposals. But it is a *governed* spoke: it reads the knowledge graph and
> proposes to the gate, and the gate — deterministic, auditable — is the only thing that writes
> the twin. The brain proposes, the reflex disposes, the spine remembers. In a hospital, that
> ordering isn't a nicety; it's the difference between a copilot and a liability."
