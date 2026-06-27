# Optional — only for a machine with NO Postgres

These files spin up a self-contained Postgres + the twin service via Docker, for a fresh
demo box that doesn't already have infra.

**If your Docker infra (Postgres/Neo4j/Ollama/Redpanda) is already running, IGNORE this folder.**
Use the host-mode scripts instead:  `.\scripts\run.ps1 -Port 5005`

Note: this compose path uses in-memory grounding/brain fallbacks (no Neo4j/Ollama wired here)
and a throwaway Postgres on port 8099 — it is NOT the full real-integration demo.
