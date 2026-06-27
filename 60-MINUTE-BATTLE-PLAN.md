# 60-MINUTE INTERVIEW BATTLE PLAN
**Structure:** ~40 min conversation (the decks + your mental model) → 20 min live demo (the close).
**Audience:** David Wilson (Tech Head/CTO) + Lead Architect.
**The reframe that matters:** the interview is won in the *conversation*. The demo *confirms* what you already established. Don't save your best thinking for the demo — lead with it.

---

## ⏱️ THE ARC AT A GLANCE

| Time | Segment | Your asset | Goal |
|------|---------|-----------|------|
| 0–2 min | Warm open | — | Calm, present, curious |
| 2–4 min | **The 90-second pitch** | (memorized) | Reframe: candidate → diagnostician |
| 4–12 min | Their vision, your read | `Who-Owns-What` | Prove you understand their platform |
| 12–22 min | The blockers + rescue | `Rescue Steps` | Name the real gap, show the path |
| 22–32 min | Differentiation + #1 ambition | `Core Differentiators` | Show how they win the category |
| 32–40 min | The two-track plan | `CTO Two-Track Blueprint` | Platform + execution discipline |
| 40–58 min | **LIVE DEMO** | The running spine | Working software = proof |
| 58–60 min | The close | (memorized) | Hand them the floor, stop |

> Flex: if they drive hard with questions, *let them* — the deck order is a spine, not a script. Answer, then steer back. The AI Interview tab (`/interview.html`) is your safety net for any question.

---

## 0–2 MIN — THE WARM OPEN
- Let them set the tone. Don't rush.
- If they say "tell me about yourself" → go straight to the pitch.
- If they open with small talk → match it briefly, then: *"I'm genuinely excited about this — I've spent real time inside your platform. Want me to start there?"*

---

## 2–4 MIN — THE 90-SECOND PITCH *(the most important 90 seconds)*
Deliver it from memory (full text in `90-SECOND-PITCH-AND-BLUEPRINT.md`). The five beats:
1. **Who I am** — FDE who ships agentic AI to production (UE5 clinical product).
2. **What you've built** — twin, Moducules, catalog, Unity — credit them, echo their mission.
3. **The blocker** — "validation has no home"; the chain stalls where correctness should live.
4. **The rescue** — governed agentic AI + inline gate; AI proposes, gate disposes; I built a working model.
5. **The ambition** — earn FAANG/Autodesk grade one metric at a time → #1 in AEC.

End: *"Where would you like me to start — the working model, or the gaps I think I see?"* → **stop.**

---

## 4–12 MIN — THEIR VISION, YOUR READ  ·  deck: WHO-OWNS-WHAT
**Goal:** prove you see their system clearly and you're not here to oversell.

Open with the honesty move (it's your credibility):
> "Before I propose anything — here's what's *yours*, and it's good. The twin, the Moducules, the reusable catalog, the Unity experience. I'm not here to rebuild what works. I'm here for the one layer your roadmap doesn't show yet."

Walk the three columns: **theirs** (don't touch) · **StatusNeo's pod** (hardening) · **the net-new layer I'd own** (governed AI + inline gate + knowledge graph + BIM federation).

**Then ask — curiosity = seniority.** These four questions de-risk everything and prove you think in *their* system:
1. "Today, are Applicable-Standards enforced **inline at design time**, or as a **batch** ASP/SPA pipeline?"
2. "Does the twin's authoritative state live in the **data layer**, or partly in **Unity's scene graph**?"
3. "Is there any **LLM/agentic** capability in Space Bot today, or is it a designer/navigation UI?"
4. "How do you exchange **BIM/Revit/IFC** today, if at all?"

> **Listen hard to the answers — they tell you which parts of your demo to emphasize in the last 20 minutes.** This is reconnaissance, not just rapport.

---

## 12–22 MIN — THE BLOCKERS + THE RESCUE  ·  deck: RESCUE STEPS
**Goal:** name the real reason they're not at paying customers, and show the way out.

The frame:
> "The gap between a beautiful demo and a platform an owner will stake a hospital on is *correctness with a home*. Right now collision is a concept because nothing authoritative computes it. Here's the rescue — and it's not a rewrite, it's the missing layer."

Walk the rescue steps. Land these:
- **Inline deterministic gate** — correct-by-construction at placement time, not batch.
- **Governed agentic AI** — the brain proposes, the gate disposes, the twin remembers.
- **Knowledge graph grounding** — cites real codes (FGI/ADA/IBC), never hallucinates.
- **Renderer-independence** — twin owns truth, Unity becomes the GPU.

The honesty anchor (say it before they ask):
> "This is a working vertical slice, not a production cloud deployment — nobody should hand you that yet. What it proves is the architecture and the discipline. The scale story is a plan with triggers."

---

## 22–32 MIN — DIFFERENTIATION + #1 AMBITION  ·  deck: CORE DIFFERENTIATORS
**Goal:** connect your build to *their* stated mission, and show the path to category leadership.

Mirror their language back, with your bridge to each:
- "**Modularize data for continuous reuse**" → "your Moducules do this; I add the agent that *composes* from them and the gate that *validates* every placement."
- "**Eliminate fragmentation and waste**" → "fragmentation is exactly what one event-sourced truth kills."
- "**Contextual intelligence**" → "intelligence needs grounding — that's the knowledge graph."
- "**Vendor-neutral, technology-agnostic**" → "I'd extend that to formal BIM interop — IFC/BCF — so intelligence flows back to the architect's tool."
- "**Gamification & visual wayfinding**" → "renderer-independence makes every lens — 2D, 3D, Unity, XR — a window on one truth."

The #1 ambition (the fast-follow, disciplined):
> "Then we scale like the best in the world — but only when a metric demands it. Geometry like Netflix, state like Meta's feed, AI like edge inference. Autodesk-grade geometry is a *triggered* decision, not day-one. Knowing what **not** to build yet is how we reach #1 without melting under our own ambition."

---

## 32–40 MIN — THE TWO-TRACK PLAN  ·  deck: CTO TWO-TRACK BLUEPRINT
**Goal:** show you think in both platform architecture *and* execution discipline — the FDE's double mandate.

- **Track 1 — Platform:** value-stream org, the Context Spine, Moducule component contracts, the digital-twin hierarchy, persistence-by-workload, the managed Unity posture.
- **Track 2 — Execution:** ownership + ADRs, a single backlog, Spec→BDD→ATDD, AI-augmented SDLC, CI/CD, the pod transition.

The 60-day proof (concrete, fundable):
> "In 60 days, Designer-to-Floor runs end-to-end in production on **one flagship room type** with a real design partner. Two decisions I'd want **your** eyes on first — the geometry-kernel strategy and the tenant-isolation tier — because they're hardest to retrofit, and the decision rights are yours."

> **Transition line to the demo:** *"I've been talking about the architecture — let me stop talking and show it running. It's live on your stack right now."*

---

## 40–58 MIN — THE LIVE DEMO  *(the proof — see GAME-DAY-STUDY-SHEET.md for click sequence)*
**Goal:** everything you claimed, working. ~18 minutes, unhurried.

Open the showcase front door (`/showcase.html`) — *"this is the whole platform in one place."* Then:
1. **Place a legal Moducule** → commits, one immutable event. *"The renderer owns nothing."*
2. **Illegal one rejected with a cited rule** → no trace. *"It only snaps if it's legal."* **(the thesis)**
3. **Legal placement earns a med-gas edge** → gold line. *"An edge the system earned by validation."*
4. **Grounded agent proposes → you approve** → *"The brain proposes, the gate disposes."*
5. **`/health`** → postgres/neo4j/ollama up. *"Not mocked. Degrades, never crashes."*
6. **Hierarchy stamp** → 3 rooms flip COMPLIANT, roll up. *"The whole pitch in one click."*
7. **(If time / if hot)** Lens Studio multi-renderer, or the AI Interview tab as the "interrogate my thinking" kicker.

**Demo discipline:** narrate *less* than you want to. Let the screen do the work. If something wobbles → `/health` shows the fallback, or switch to `-InMemory`, or the recorded capture. *It degrades, it doesn't crash.*

---

## 58–60 MIN — THE CLOSE
> "Your vision, my disciplined execution, measured proof. The spine is real, runs on your stack, and it's tested. Give me one flagship room type and a design partner, and in 60 days Designer-to-Floor runs end-to-end in production — and everything else is that same spine, grown one earned step at a time. **Where would you like to go deep?**"

Then **stop talking. Let the silence sit. Let them drive.**

---

## 🎛️ READING THE ROOM — REAL-TIME ADJUSTMENTS

| If you see… | Do this |
|---|---|
| **David leaning in, asking pointed questions** | Slow down, go deeper, drop the deck pace. He's engaged — engagement > coverage. |
| **Lead Architect probing scale/geometry** | Go to the scale-path answers. Name "earned with a metric." Never bluff a number. |
| **Skepticism / "is this real?"** | Jump the demo *earlier*. `/health` + the cited-rejection kill-shot. Working code ends doubt. |
| **They love the vision, glaze at detail** | Compress. Pitch → differentiators → demo. Skip the two-track depth. |
| **Time slipping** | Protect the demo's first 3 clicks (place / reject / agent) — those are non-negotiable. Cut hierarchy/studio. |
| **A question you can't answer** | "Good question — here's what I'd measure before deciding." Never bluff. Humility = seniority. |

---

## 🧠 THE THREE THINGS TO HOLD ALL HOUR
1. **Humble about their build, certain about your diagnosis.** You studied *their* platform.
2. **The AI proposes; the gate disposes.** Never blur this — it's the healthcare-safety signal.
3. **Built vs. proven vs. next.** Label every claim. Honesty *is* the senior move. Overclaiming is the only way to lose this.

**The pitch earns the conversation. The conversation earns the demo. The demo earns the contract. Don't rush any stage to reach the next — each one is why they'll grant you the next.**
