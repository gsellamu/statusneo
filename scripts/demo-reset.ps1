# demo-reset.ps1 - friction-free reset for rehearsal.
# Drops and recreates the modutecture journal DB (clean rooms) and reloads the Neo4j graph.
# Pure ASCII. Connects to the 'postgres' DB to drop modutecture (cannot drop the open DB).
# Usage:  .\scripts\demo-reset.ps1   [-PgUser jeethhypno_user]  [-KeepGraph]
[CmdletBinding()]
param(
  [string]$PgContainer = "jeethhypno-postgres",
  [string]$PgUser = "jeethhypno_user",
  [string]$Database = "modutecture",
  [string]$AdminDb = "postgres",
  [switch]$KeepGraph
)
function Ok($m){ Write-Host ("  [OK]  " + $m) -ForegroundColor Green }
function Warn($m){ Write-Host ("  [!]   " + $m) -ForegroundColor Yellow }
function Step($m){ Write-Host ("== " + $m + " ==") -ForegroundColor Cyan }

Step ("Reset journal DB '" + $Database + "' (clean rooms)")

# terminate other connections to the target DB (run from the admin DB)
docker exec $PgContainer psql -U $PgUser -d $AdminDb -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='$Database' AND pid<>pg_backend_pid();" 2>$null | Out-Null

# drop + recreate, connected to the admin DB (NOT the one being dropped)
docker exec $PgContainer psql -U $PgUser -d $AdminDb -c "DROP DATABASE IF EXISTS $Database;" | Out-Null
docker exec $PgContainer psql -U $PgUser -d $AdminDb -c "CREATE DATABASE $Database;" | Out-Null
if ($LASTEXITCODE -eq 0) { Ok "journal reset - all rooms empty" }
else { Warn ("reset failed; check that user '" + $PgUser + "' can manage databases") }

if (-not $KeepGraph) {
  Step "Reload Neo4j knowledge graph"
  & (Join-Path $PSScriptRoot "seed.ps1")
}

Write-Host "Clean slate ready. Start the service:  .\scripts\run.ps1 -Port 5005" -ForegroundColor Green
Write-Host "Tip: use room ids icu-101 / icu-102 / exam-201 in the planner to light up the Hierarchy lens." -ForegroundColor Gray
