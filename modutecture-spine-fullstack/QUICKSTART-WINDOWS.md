# Windows / PowerShell quickstart (36-hour demo)

Three scripts. Run from the repo root in **Windows PowerShell**.

## One-time setup (≈5 min) — against your live Jeeth.ai infra
```powershell
.\scripts\setup.ps1
```
Creates the isolated `modutecture` DB in `jeethhypno-postgres` (your data untouched),
loads the Neo4j knowledge graph, confirms `mistral-nemo`. Safe to re-run.
If your Postgres password isn't `postgres`, edit `twin-service-dotnet\appsettings.Real.json`.

## Run the service (real infra)
```powershell
.\scripts\run.ps1
```
Starts the twin service on **http://localhost:5000**, reaching your containers via
`localhost` (Postgres 5431, Neo4j 7687, Ollama 11434). It prints which dependencies are
live vs. fallback, and opens the viewer. Open a **second browser tab** for the live-sync moment.

- viewer: http://localhost:5000
- health: http://localhost:5000/health  (show this — `journal:postgres:up · grounding:neo4j:up · brain:ollama:up`)
- metrics: http://localhost:5000/metrics  (scraped into your Grafana at :5000 / Prometheus :9090)

## Smoke-test it end-to-end (second terminal) — and screen-record this
```powershell
.\scripts\smoke.ps1
```
Drives place / cited-reject / legal-with-binding / **grounded agent (Neo4j → mistral-nemo →
gate)** / journal-integrity via GraphQL and prints PASS/FAIL for each. Green here = your live
demo path works. **Record this run** as in-room insurance.

## In-room fallback (if live infra hiccups)
```powershell
.\scripts\run.ps1 -InMemory        # zero external deps; whole demo still runs
```
Or fall back to the recorded smoke-test capture, and walk the static
`dashboard\dashboard.html` (39/39, parity MATCH) while you recover.

## Prereqs
- **.NET 8 SDK** (`dotnet --version` ≥ 8). Install: https://dot.net
- Docker Desktop running with your `infra-stack-compose.yml` up.
- That's it — no Node/Unity needed for the core demo (the viewer is served by the .NET service).

## Notes
- `run.ps1` passes config via environment variables (override anything: `-Port`, `-Model`, `-Neo4jPass`, …).
- First `dotnet run` restores NuGet packages (Neo4j.Driver, OpenTelemetry, prometheus-net) — allow a minute.
- One moderate-severity advisory on the OTLP tracing exporter (transitive gRPC) is harmless;
  remove the `OpenTelemetry.Exporter.OpenTelemetryProtocol` package for a zero-warning build.
