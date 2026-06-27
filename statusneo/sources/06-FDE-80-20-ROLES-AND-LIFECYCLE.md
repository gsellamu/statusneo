# The Automated FDE — Roles, Responsibilities & Lifecycle
## An 80:20 (AI : Human-in-the-Loop) split for Forward-Deployed Engineering
*Prepared for the StatusNeo FDE practice*

**The goal.** Automate the Forward Deployed Engineer's lifecycle so that ~80% of the mechanical work is executed by an agentic AI FDE (the swarm), and ~20% is reserved for the human FDE — the Pod Commander — who supplies judgment, trust, architecture, and governance.

**The 80:20 is a leverage ratio, not a uniform rule.** Some stages are 95% AI (boilerplate codegen); others are 80% human (architecture sign-off, client trust, regulated-data governance). The discipline is putting the human 20% exactly where judgment is irreplaceable — and letting the AI own everything else.

> **The governing principle:** AI proposes; the human disposes. The agentic FDE generates, tests, and self-heals at machine speed. The human FDE owns the irreversible and the unquantifiable — the architecture call, the trust in the room, the compliance gate, the "ship / don't ship" decision. The 80% buys velocity; the 20% buys safety and adoption.

---

## PART I — The Two Halves of the FDE

"FDE" is now a two-part entity: the **Agentic FDE** (the 80%) and the **Human FDE / Pod Commander** (the 20%). They are not peers doing the same job — they own different classes of work, divided by what only a human can be accountable for.

### The Agentic FDE — the 80% (machine-owned)

A multi-agent swarm of specialized roles, each a node in the lifecycle. It owns **velocity, breadth, and repetition**.

| Agent role | Owns | Lifecycle stage |
|---|---|---|
| **Spec Architect** | Translates the human's intent + spec into a rigorous, machine-executable plan. | 1–2 |
| **Systems Engineer** | Writes the code, resolves conflicts, self-corrects syntax, builds the binary. | 3 |
| **Adversarial SDET** | Generates and runs unit/integration/E2E tests; stress-tests its own output. | 4 |
| **Security Sentinel** | Runs SAST, dependency scans, and compliance checks as agent tools. | 5 |
| **DevOps Overseer** | Manages PRs, canary deploys, rolling updates within guardrails. | 6 |
| **Reliability Agent** | Monitors production, maps stack traces, cuts auto-hotfix PRs. | 7 |
| **Platform Architect** | Abstracts reusable components from the Spoke back up to the Hub (background). | cross-cutting |

### The Human FDE / Pod Commander — the 20% (judgment-owned)

One senior, embedded, customer-trusted engineer. They do not out-type the swarm; they *out-judge* it. They own **context, trust, architecture, and governance** — the four things an agent cannot be accountable for.

| Human responsibility | Why it can't be automated |
|---|---|
| **Diagnosis & architecture** | Reading the client's real problem — the gap that "lives between the tools" — requires judgment the swarm has no context for. |
| **Client trust & evangelism** | A CISO trusts a person in the room, not a PR bot. Change leadership is human work. |
| **Spec authorship & intent** | The swarm builds what the spec says; a wrong spec fails at machine speed. The human owns intent. |
| **Governance & the ship gate** | In regulated domains, a human is accountable for the irreversible call. The gate is never delegated. |
| **Exception triage** | When an agent loops or hits an edge case, the human breaks the tie and re-plans. |

---

## PART II — The 80:20 FDE Lifecycle, Stage by Stage

The split is per-stage. The human owns the **front** (intent + architecture) and the **end** (the ship gate) — the swarm owns the dense middle.

```
HUMAN-HEAVY            AI-HEAVY (the dense middle)            HUMAN-HEAVY
  ┌─────┐   ┌─────┐   ┌─────┐   ┌─────┐   ┌─────┐   ┌─────┐   ┌─────┐
  │  1  │──>│  2  │──>│  3  │──>│  4  │──>│  5  │──>│  6  │──>│  7  │
  │Plan │   │Arch │   │Code │   │ QA  │   │ Sec │   │ CD  │   │ Ops │
  └─────┘   └─────┘   └─────┘   └─────┘   └─────┘   └─────┘   └─────┘
   30:70     40:60     95:5      90:10     85:15     70:30     80:20
                              ▲                                  │
                              └────── self-heal feedback ────────┘
```

| Stage | AI:HITL | Agentic FDE does | Human FDE does | Gate |
|---|---|---|---|---|
| **1. Context & Planning** | 30:70 | Ingests topology, parses issues, retrieves prior art, drafts a plan. | Sets intent/priorities, frames the real problem, validates against client reality. | Human approves the plan before any code is touched. |
| **2. Architecture & Design** | 40:60 | Queries docs, proposes API specs, checks schema, surfaces trade-offs. | Makes the architecture call, enforces governance, accepts/rejects the design. | Human signs off on architecture + contracts (ADRs frozen). |
| **3. Implementation** | 95:5 | Coding squads write code, resolve conflicts, self-correct, build the binary. | Spot-reviews novel logic; otherwise stays out of the way. | No gate mid-stage; output flows to QA. |
| **4. Agentic QA** | 90:10 | Generates tests from acceptance criteria; Adversarial SDET stress-tests. | Reviews coverage on high-risk paths; confirms the criteria were right. | Automated: 100% pass + coverage threshold. |
| **5. Security** | 85:15 | Runs SAST, dependency/license scans, compliance checks; blocks on sev-1. | Owns compliance interpretation; signs risk acceptance where required. | Human signs regulated-data acceptance; else automated. |
| **6. Continuous Deployment** | 70:30 | Opens PR, runs canary + rolling deploys within guardrails, watches health. | Holds the ship gate (approves merge in Embedded; sets policy in Remote). | **THE SHIP GATE** — human (Embedded) or policy (Remote). |
| **7. Observability & Self-Heal** | 80:20 | Monitors prod, maps stack traces, auto-cuts hotfix PRs, re-enters at stage 1. | Triages escalations, tunes guardrails/budgets, owns the post-incident call. | Auto-heals within policy; escalates on repeated failure. |

---

## PART III — The 80:20 RACI

The pattern that matters: the **human is Accountable for every stage** even where the AI is Responsible for the work. Accountability never automates.

| Stage | Agentic FDE (AI) | Human FDE | Accountable |
|---|---|---|---|
| 1. Plan | Responsible (drafts) | Approves intent/scope | **Human** |
| 2. Architecture | Consulted (proposes) | Responsible (decides) | **Human** |
| 3. Code | Responsible (builds) | Informed / spot-review | **Human** |
| 4. QA | Responsible (tests) | Consulted (coverage) | **Human** |
| 5. Security | Responsible (scans) | Accountable (risk sign-off) | **Human** |
| 6. CD | Responsible (deploys) | Accountable (ship gate) | **Human** |
| 7. Observability | Responsible (heals) | Consulted (escalations) | **Human** |

**The leverage math.** One human FDE supervises a swarm that executes the equivalent of a multi-engineer team. The ratio is not about effort hours — it is about **where accountability and judgment must sit**. The 80% is delegated because it is mechanical and verifiable; the 20% is retained because it is irreversible or unquantifiable.

| What scales (the 80%) | What doesn't (the 20%) |
|---|---|
| Code generation, refactoring, glue code | The architecture decision and its trade-offs |
| Test authoring and execution | Whether the acceptance criteria are even correct |
| Security scanning and dependency hygiene | The compliance interpretation and risk sign-off |
| Canary deploys, rollbacks, hotfix PRs | The "ship / don't ship" call in a regulated domain |
| Production monitoring and anomaly mapping | Client trust, evangelism, and change leadership |

---

## PART IV — The Split Across Embedded & Remote PODs

The 80:20 ratio shifts with topology. Embedded PODs keep the human closer to the loop (more HITL); Remote PODs push toward autonomy (less HITL, stricter automated gate). The human's **accountability is constant**; only the interaction surface changes.

| Dimension | Embedded POD | Remote POD |
|---|---|---|
| **Effective split** | ~70:30 (AI:Human) — human reviews more | ~90:10 (AI:Human) — automated gates |
| **The ship gate** | Human approves the merge in Slack/GitHub | Automated policy gate, human-defined |
| **Best for** | Legacy apps, core features, regulated work | Greenfield, ephemeral micro-services |
| **Trust posture** | "Super-powered intern" earning trust | Sovereign service, trust already earned |
| **Human role** | Reviewer + architect + evangelist | Policy-setter + exception handler |

**The adoption path:** start Embedded (70:30) to earn trust on the client's PR outputs, then offer the toggle to Remote (90:10). Trust is earned in Embedded and spent in Remote — the same land-and-expand logic that governs the whole FDE practice.

**The guardrails that make the 80% safe**
1. **Branch protection.** Agents never push to main/production — every change is a PR. The human (or policy) holds merge.
2. **Token & compute budgets.** Hard per-task spend caps; breach triggers teardown + SRE alert. The kill-switch is non-negotiable.
3. **Loop detection.** If an agent fails the same test >5 times, it stops and escalates to the human triage queue.
4. **Deterministic gates.** 100% test pass + coverage threshold + zero sev-1 security before any deploy. Non-negotiable, automated.
5. **Unified telemetry.** Every agent action is logged to a central observability layer — the human supervises the swarm, not each keystroke.

> **The 80:20 in one line:** automate the 80% that is mechanical, verifiable, and reversible. Retain the 20% that is judgment-bound, trust-bound, and irreversible. The agentic FDE gives you the velocity of a team; the human FDE gives you the accountability of a senior engineer. AI proposes; the human disposes — and in a regulated enterprise, that gate is the whole job.
