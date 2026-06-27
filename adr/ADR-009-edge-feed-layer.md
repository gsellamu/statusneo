# ADR-009 — Renderer-agnostic twin, rendering-optimized edge/feed layer

| | |
|---|---|
| **Status** | Proposed |
| **Date** | 2026-06-14 |
| **Deciders** | David Wilson (CTO), Lead Architect, Platform pod |
| **Supersedes / relates** | Builds on the Twin API + four-tier latency model; relates to ADR on Unity-as-projection |
| **Context tier** | Telco-grade runtime, sub-second interaction |

---

## Decision (one sentence)

The twin remains the single, rendering-agnostic source of truth; we insert a thin
**edge/feed layer** of rendering-optimized adapters between the twin and each renderer
that *shape the same event-sourced truth* for each consumer's latency and bandwidth
profile — never forking truth, never embedding rendering logic in the twin.

> One twin, many edges, many lenses. Truth is shaped at the edge, never forked.

---

## Context

### What exists today (the demo — verified in code)

Every lens talks **directly** to one GraphQL endpoint (`/graphql`) on `twin-service-dotnet`
and pulls the **same full payload**: `twin(room){ version, instances[], bindings[] }`.
Five renderers — 2D canvas, Schematic SVG, 3D Three.js, a Twin-Data table, and the Unity
client — each reconstruct the room from `instances[] + bindings[]` with **zero server-side
rendering code**. The web lenses are not separate services; they are static-served browser
clients of the one contract.

This already proves the thesis that matters most: **the renderer owns nothing; the twin owns
the truth.** It is the correct demo simplification. It is also the thing that does not scale.

### Why direct-to-GraphQL does not scale

- **One payload for all consumers.** A data table needs a few hundred bytes; a Unity scene
  with full geometry needs megabytes of mesh. Serving both from one query forces the heaviest
  shape on the lightest consumer, or starves the heaviest.
- **The twin would be pressured to stream triangles.** Mesh/geometry is large, immutable, and
  cacheable — fundamentally different from mutable twin state. Mixing them co-locates two
  opposite caching strategies on one path.
- **Felt latency dies at the edge, not the core.** Most perceived latency is killed by
  prediction and immutable-asset caching near the client (T0/T1), not by moving the core
  closer. A single central endpoint cannot do edge prediction.
- **Jobsite reality.** Construction trailers have poor connectivity; a single synchronous
  endpoint cannot serve an offline-first viewer.

### The four-tier latency model (the anchor — unchanged)

| Tier | Budget | Mechanism | What lives here |
|------|--------|-----------|-----------------|
| **T0** | < 16 ms | Never networked — pure Unity | Camera, hover, selection, drag preview |
| **T1** | 50–150 ms | Optimistic + edge confirm (snaps instantly, certifies async) | Placement validated against a cached rule subset; authoritative verdict replaces the provisional one |
| **T2** | Seconds | Streamed, non-blocking overlays | AI proposals, GraphRAG answers with citations |
| **T3** | Minutes–hours | Durable workflows; client re-syncs on resume | HITL approval, flywheel delta capture |

The edge/feed layer is precisely the apparatus that delivers T1 (optimistic + edge confirm)
and T0-friendly content delivery, while T2/T3 remain twin-side.

---

## Decision detail — three movement mechanisms, deliberately separated

The crux: **content is cached, state is synced, the brain is relocated.** Three different
data-movement problems get three different mechanisms — never one pipe.

### 1. Geometry / mesh → edge CDN (immutable, content-addressed)

- Moducule render meshes (glTF / USD bundles) are **immutable** and **content-addressed**
  (hash = identity). A given Moducule version's geometry never changes; a change is a new hash.
- Served from an **edge CDN**, cache-forever. Unity / WebGL pull a mesh bundle **once** and
  cache it; subsequent rooms reusing that Moducule cost zero bytes.
- **The twin never streams triangles.** It emits a *reference* (typeId + version → content
  hash); the renderer resolves the hash against the CDN. The twin's payload stays tiny.
- Data-table and schematic lenses simply ignore the geometry reference — they need none.

### 2. Mutable twin state → CQRS snapshot + delta stream

- The twin is **event-sourced** (commands in, events out). Reads are served as a **snapshot
  plus a delta stream** (CQRS read model), not as repeated full-payload polls.
- **Same truth, different envelope per edge.** A web lens consumes a small JSON diff over
  `graphql-transport-ws`; Unity consumes the *same logical delta* but may take it over a binary
  channel. The read model is shared; only the transport envelope differs.
- This is a **read-model projection**, not a second source of truth. There is exactly one
  event log. Edges are differently-shaped *reads* of it.
- Regional **read replicas** let edges serve snapshots locally; the write path stays central
  and authoritative.

### 3. The brain (LLM + GraphRAG) → co-located regional, off the render path

- Keep retrieval and inference **together** and **near the data** — a GraphRAG round-trip
  across regions costs more than the inference itself. The local-first posture already *is*
  edge inference.
- The brain **never sits on the interaction path.** Proposals stream in as T2 overlays; they
  never block T0/T1. "It snaps instantly; it certifies asynchronously."

### Optional AEC differentiator — the jobsite edge node

A small box in the construction trailer runs: the viewer payload, a read snapshot, and a
**local validation worker**. It serves an offline-first viewer and confirms optimistic
placements locally, syncing to the regional write path when the link returns. This turns
offline-first from a slide into a product differentiator.

---

## What stays invariant (the guardrails)

1. **One event log. One source of truth.** Edges are projections/caches, never authorities.
   No edge may accept a write it has not forwarded to the twin.
2. **No rendering logic in the twin.** The twin emits typed truth (instances, bindings,
   geometry references). It does not know what a triangle is.
3. **No truth in the renderer.** Unity/WebGL hold ephemeral view state only — the same thin-
   state discipline already established. The edge does not change that.
4. **Optimistic ≠ authoritative.** An edge-confirmed placement is provisional; the twin's
   committed verdict is the truth and replaces it. The deterministic gate still disposes.
5. **Content-addressed immutability for geometry.** A mesh bundle is identified by hash; it is
   never mutated in place. Cache invalidation is therefore a non-problem.

---

## Consequences

### Positive
- Each consumer gets the lightest correct shape: bytes for a table, mesh-once for Unity.
- Geometry caching + optimistic confirm kill felt latency at T0/T1 without moving the LLM.
- Offline-first becomes real (jobsite node).
- The renderer-independence thesis is *preserved and strengthened* — more lenses, same truth.

### Negative / cost
- Operational surface grows: a CDN, read replicas, a delta-stream projection, edge workers.
- A read-model projection must be kept consistent with the event log (eventual consistency
  window on reads — acceptable because writes remain strongly validated at the core).
- More moving parts to monitor (each edge needs health + lag metrics).

### Neutral
- The current direct-to-GraphQL path is **not thrown away** — it is the degenerate case of this
  model (one edge = the origin). The demo keeps working; the edge layer is additive.

---

## Alternatives considered

| Option | Why not |
|--------|---------|
| **Keep direct-to-GraphQL everywhere** | Correct for the demo; forces the heaviest payload shape on every consumer; cannot do edge prediction or offline-first. Does not reach telco-grade. |
| **Per-renderer twins (fork truth)** | Fastest to ship per lens, but forks the source of truth — exactly the silo failure being rescued. Rejected on principle. |
| **Render server-side, ship pixels/video** | Centralizes rendering, kills the renderer-independence thesis, and re-couples truth to one renderer. Rejected. |
| **One pipe for content + state** | Couples immutable cache-forever geometry with mutable never-cache state; you lose both optimizations. Rejected. |

---

## Rollout (incremental, gated — no big bang)

1. **Phase 0 (today):** direct-to-GraphQL. Proven. Ship the demo on this.
2. **Phase 1:** add the CQRS snapshot + delta read model behind the existing GraphQL
   subscription. No client change required; the envelope is the same.
3. **Phase 2:** extract geometry to content-addressed bundles on a CDN; twin emits references.
   Unity/WebGL resolve hashes. Light lenses unaffected.
4. **Phase 3:** regional read replicas + co-located brain.
5. **Phase 4:** jobsite edge node (offline-first viewer + local validation worker).

Each phase is independently shippable and reversible. Truth never forks at any phase.

---

## The line for the room

> Unity becomes the GPU. The twin holds the truth. The edge carries the bytes. The brain stays
> where the data is.

One twin, many edges, many lenses — truth is shaped at the edge, never forked.
