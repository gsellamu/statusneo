# Run the demo against your live infra (host mode, 36-hour plan)

The twin service runs **on the host** via `dotnet run` and reaches your containers at
`localhost:<port>`. Every external dependency sits behind an interface with a deterministic
fallback, so a flaky container **degrades** the demo — it never crashes it. `/health` shows
you exactly what's real vs. fallback, live.

## 0. One-time setup (≈5 min)

```bash
# a) create the isolated journal DB inside your existing Postgres (Jeeth.ai data untouched)
docker exec -it jeethhypno-postgres psql -U postgres -c "CREATE DATABASE modutecture;"
#    (if the password isn't 'postgres', edit appsettings.Real.json -> ConnectionStrings:Twin)

# b) load the space-planning knowledge graph into Neo4j
cat neo4j/seed.cypher | docker exec -i jeethhypno-neo4j cypher-shell -u neo4j -p jeeth2025

# c) confirm the model is present
docker exec jeethhypno-ollama ollama list      # expect mistral-nemo
```

## 1. Start the twin service (real infra)

```bash
cd twin-service-dotnet
dotnet run -c Release --launch-profile Real           # or:
ASPNETCORE_ENVIRONMENT=Real dotnet run -c Release
# it prints which deps are live vs. fallback at startup
```

Open the viewer + the proof tabs:
```
http://localhost:5000            # the live Room Planner (open a 2nd tab for sync)
http://localhost:5000/health     # per-dependency status (show this to the CTO)
http://localhost:5000/metrics    # Prometheus metrics (already scraped into your Grafana)
```

## 2. Confirm each integration is REAL (do this before the meeting)

```bash
# journal on real Postgres:
curl -s localhost:5000/health | jq        # "journal":"postgres:up"

# grounding on real Neo4j:                 # "grounding":"neo4j:up"
# brain on real Ollama:                    # "brain":"ollama:up"

# end-to-end grounded agent (Neo4j -> mistral-nemo -> gate):
#  - in the viewer, place a Headwall, then click "Suggest a bed (agent)"
#  - the proposal's citations (R3-medgas / FGI 2.1-8.4 ...) come from the GRAPH
#  - the placement was drafted by mistral-nemo and then validated by the gate
```

## 3. What's real vs. stubbed (say this to the CTO, verbatim honesty)

| Capability | In this demo |
|---|---|
| Twin journal | **real** — Postgres (`modutecture` DB) |
| Rule/standard grounding | **real** — Neo4j Cypher (`neo4j/seed.cypher`) |
| Agent proposal | **real** — `mistral-nemo` via Ollama, grounded-only, gate-validated |
| Validation gate | **real** — the pure validator (39 tests) |
| Metrics | **real** — Prometheus → your Grafana (`localhost:5000`/`:9090`) |
| Tracing | **real spans** — OTLP; point `OTEL_EXPORTER_OTLP_ENDPOINT` at Tempo/Jaeger to view |
| Live sync | **polling** (bulletproof); GraphQL subscription wired, Redpanda adapter is the next step |
| Geometry bundles (MinIO), vector recall (Qdrant/pgvector), analytics (Flink) | **available in your stack, not wired in this slice** — interfaces/notes show where they drop in |

## 4. Degradation (why it can't break in the room)

- Neo4j down → `grounding:"neo4j:degraded"` → in-memory rules (same content) → agent still proposes.
- Ollama down/slow → `brain:"ollama:degraded"` → deterministic proposer → agent still proposes.
- Postgres down → start with `--InMemory` and the whole demo still runs.
- Live infra wobble → fall back to the **recorded screen capture** (make one in hour 28–36).

## 5. Notes

- `dotnet run` profile: add to `Properties/launchSettings.json` a "Real" profile that sets
  `ASPNETCORE_ENVIRONMENT=Real` and `applicationUrl=http://localhost:5000`, or just run with
  `--InMemory` off and the `appsettings.Real.json` values (already the defaults in code).
- The OTel OTLP exporter logs one moderate-severity advisory (transitive gRPC); harmless for
  the demo. Remove the exporter package if you want a zero-warning build.
