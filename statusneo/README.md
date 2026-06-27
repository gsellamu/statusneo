# StatusNeo — Agentic FDE Practice

The Forward-Deployed Engineering (FDE) charter-and-scoping suite for the Modutecture engagement, consolidated inside this repo (`github.com/gsellamu/statusneo`).

**One line:** a single Elite Pod Commander, backed by a governed AI swarm, automates ~80% of the AEC twin lifecycle and reserves the judgment-bound ~20% — lands one regulated account through a deterministic correctness gate, then scales it into a self-improving StatusNeo GCC. *The AI proposes; the human disposes; the gate is the point.*

> **Sourcing note.** These are charter-and-scoping artifacts, not a delivery commitment. Several reference specific June-2026 products, metrics, and dates drawn from source briefings and **pending verification** — treat them as the target operating model. The Modutecture engagement is framed as a **validated demonstration, not a closed-and-scaled account**.

---

## Where things live in this repo

```
modutecture-spine-fullstack/                 <- repo root (github.com/gsellamu/statusneo)
├── statusneo/                               <- THIS folder: the FDE suite source-of-truth
│   ├── README.md                            <- you are here
│   ├── sources/                             <- canonical markdown for every document (00–10)
│   └── consolidate-statusneo.ps1            <- one-run setup: copies sources in + syncs binaries
└── twin-service-dotnet/wwwroot/
    ├── statusneo.html                       <- the /statusneo page (the suite, as a web page)
    ├── nav.js                               <- shared nav (StatusNeo group wired in)
    └── artifacts/statusneo/                 <- the binaries the page serves (.pdf/.docx/.pptx)
```

The landing hub (`wwwroot/showcase.html`) links to `/statusneo.html` from its hero, its Live-surfaces grid, and the guided voice tour.

---

## The suite (00–10)

| # | Document | What it is |
|---|---|---|
| 00 | Master Reference | The capstone — 11 parts consolidating the whole practice. |
| 01 | FDE Charter Proposal | The strategic case + 30/60/90 + change leadership + the Copilot×Twin toolbox. |
| 02 | FDE Loop™ Cookbook | The embedded-delivery methodology, recipe by recipe. |
| 03 | Landing Runbook | The operational field manual for the first 90 days inside a customer. |
| 05 | Agentic FDE Lifecycle | The 7-stage agentic SDLC — blueprint, playbook, cookbook, runbook. |
| 06 | FDE 80:20 Roles & Lifecycle | The role split, per-stage AI:Human ratios, and the full RACI. |
| 07 | Multi-Cloud Tooling Directory | Generic/AWS/GCP/Azure agentic toolchains, Embedded vs. Remote. |
| 08 | AEC / Modutecture Use Case | Contextual construction across the four loops + the correctness gate. |
| 09 | 30/60/90 — OKR & STAR | The plan re-expressed as OKRs and as STAR narratives. |
| 10 | OKR Scorecard | The printable tracking grid — actuals vs. targets, scored 0.0–1.0. |

(There is no 04; the numbering intentionally skips it. The PR-FAQ Playbook and the board deck are binary-only and live in `artifacts/statusneo/`.)

Each document exists as **canonical markdown** in `sources/` and as a rendered **PDF + DOCX** (and PPTX for the deck) in `wwwroot/artifacts/statusneo/`.

---

## Setup — one run

The markdown sources and the served binaries are pulled together by a single script. From this folder:

```powershell
powershell -ExecutionPolicy Bypass -File .\consolidate-statusneo.ps1
```

It will:
1. Copy every `*.md` source into `statusneo/sources/`.
2. Copy each binary artifact into `wwwroot/artifacts/statusneo/` (so the page serves it) — sourcing them from your Downloads / Desktop, where they were delivered.
3. Report anything it could not find, so you can re-download just those and run again.

Then browse to `/statusneo.html` (or use the nav menu → **StatusNeo FDE**), and commit:

```powershell
git add .
git commit -m "Add StatusNeo Agentic FDE suite (sources, page, artifacts)"
git push
```

---

## Honesty discipline (non-negotiable)

Claims are labeled by confidence; every June-2026 product/metric/date is flagged pending verification. Durable layer = the structure (the loops, the 80:20 split, the architecture pattern). Perishable layer = the named tools, metrics, and dates. Protect that precision — it is the integrity signal that wins regulated rooms.
