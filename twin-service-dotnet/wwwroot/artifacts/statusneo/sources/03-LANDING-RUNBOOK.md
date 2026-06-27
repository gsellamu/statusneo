# The Landing Runbook
## An FDE's First 90 Days Embedded Inside a Customer
**Companion to:** the FDE Charter + the FDE Loop™ Cookbook · **Audience:** the FDE on the ground

> A runbook is *operational* — exact steps, checklists, and gates. The cookbook is the method; this is the field manual for a single FDE landing a single account. Repeatable across every engagement.

---

## PRE-LANDING — before day 1 on site
- [ ] Account brief from the regional head: customer, stated scope, commercial frame, known risks.
- [ ] Identify the offshore pod that will scale behind you; meet the pod lead.
- [ ] Confirm the commercial/contract boundary (what's in scope, decision rights, IP).
- [ ] Read everything the customer has shared — architecture, docs, roadmap — before you arrive.
- [ ] **Gate:** you can state the customer's likely load-bearing gap in one sentence before day 1.

---

## PHASE 1 — LISTEN & DIAGNOSE (Weeks 1–2)  · Clarity
**Objective:** understand the customer's system better than their job description does.

- [ ] Meet the customer's technical decision-maker and map the real org.
- [ ] Walk their existing system live; find where "the product lives between the tools."
- [ ] Run the **four recon questions** (adapt per domain):
  1. Where is correctness/validation enforced today — inline or batch?
  2. Where does authoritative state live — data layer or rendering/scene/UI layer?
  3. What AI/agentic capability exists today vs. is assumed?
  4. How is regulated/external data exchanged today?
- [ ] Map the gap to one OS Fabric™ layer.
- [ ] **Validate the diagnosis live:** "here's what I see — tell me where I'm wrong."
- [ ] **Gate:** customer agrees on the problem and wants your architecture.

---

## PHASE 2 — LAND A WORKING SLICE (Weeks 3–6)  · Build
**Objective:** running code on their stack that proves the thesis.

- [ ] Scope the thinnest end-to-end vertical slice that proves the architecture.
- [ ] Freeze the contracts (data model, payloads, command set) as lightweight ADRs.
- [ ] Build on the customer's actual stack — real infra, not a sandbox demo.
- [ ] Enforce governed-AI discipline: model proposes, deterministic gate disposes.
- [ ] Instrument health, metrics, governance from the first commit.
- [ ] Label everything **built / proven / next** — no overclaiming.
- [ ] **Gate:** customer-validated working slice runs on their stack.

---

## PHASE 3 — PULL THE POD IN (Weeks 5–8, overlapping)  · Velocity
**Objective:** scale delivery behind earned trust — no handoff gap.

- [ ] Define the offshore-pull point: what the pod takes now that the slice is trusted.
- [ ] Onboard the offshore pod *behind* you — you stay the single customer owner.
- [ ] Bring StatusNeo automation IP: Agents Library, AI-Native SDLC, agentic CI/CD, autonomous QA.
- [ ] Set up the delivery cadence (the customer's rhythm, not the GCC's).
- [ ] Instrument the correction-delta flywheel.
- [ ] **Gate:** offshore velocity is landing behind you; no trust gap; account scaling.

---

## PHASE 4 — GOVERN & EXPAND (Weeks 9–12)  · Improvement
**Objective:** durable, governed, growing account.

- [ ] Stand up continuous governance: observability, compliance, SLO/SLA loops.
- [ ] Score the account on the (extended) AI Maturity Index.
- [ ] Measure time-to-trust and baseline expansion opportunities.
- [ ] Identify the next adjacent problem to land (land → expand).
- [ ] Package the engagement as a reference case.
- [ ] Feed learnings back into the FDE Loop™ and this runbook.
- [ ] **Gate:** account is self-sustaining, governed, expanding, referenceable.

---

## THE BREAK-GLASS RULES (when an engagement wobbles)
- **Customer doubts it's real?** → show running code on their stack + health/observability. Working software ends doubt.
- **Scope creep?** → return to the frozen ADRs and the agreed slice. Expand deliberately, not reactively.
- **Offshore handoff friction?** → the FDE owns the seam; never let the customer feel a gap. Re-anchor on the single-owner model.
- **Architecture disagreement with the customer?** → "here's what I'd measure before we decide." Never bluff; decision rights stay with the customer.
- **Regulated-data risk surfaces?** → stop, escalate, govern first. In healthcare/BFSI/defense, correctness and compliance precede velocity, always.

---

## WHAT "DONE" MEANS AT DAY 90
A single FDE has: diagnosed the real problem, landed a customer-validated slice on the customer's stack, pulled an offshore pod in behind earned trust, stood up governance, and identified the expansion path — producing one referenceable, scaling, governed account.

**That is one unit of the FDE practice. Repeat across accounts, regions, and verticals.**
