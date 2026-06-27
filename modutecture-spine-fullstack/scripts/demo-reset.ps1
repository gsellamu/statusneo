<#
  demo-reset.ps1 - friction-free reset for rehearsal: drops & recreates the modutecture
  journal DB (clean rooms) and reloads the Neo4j graph. Run between practice runs.
  Usage:  .\scripts\demo-reset.ps1
  NOTE: this clears the twin journal (the modutecture DB only). Restart run.ps1 after.
#>
[CmdletBinding()]
param(
  [string]$PgContainer = "jeethhypno-postgres",
  [string]$Database = "modutecture",
  [switch]$KeepGraph
)
function Ok($m){ Write-Host "  [OK]  $m" -ForegroundColor Green }
function Step($m){ Write-Host "`n== $m ==" -ForegroundColor Cyan }

Step "Reset journal DB '$Database' (clean rooms)"
# terminate connections, drop, recreate
docker exec $PgContainer psql -U postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname='$Database' AND pid<>pg_backend_pid();" 2>$null | Out-Null
docker exec $PgContainer psql -U postgres -c "DROP DATABASE IF EXISTS $Database;" | Out-Null
docker exec $PgContainer psql -U postgres -c "CREATE DATABASE $Database;" | Out-Null
if ($LASTEXITCODE -eq 0) { Ok "journal reset — all rooms empty" } else { Write-Host "  [!] reset failed; check postgres password" -ForegroundColor Yellow }

if (-not $KeepGraph) {
  Step "Reload Neo4j knowledge graph"
  & (Join-Path $PSScriptRoot "seed.ps1")
}

Write-Host "`nClean slate ready. Start the service:  .\scripts\run.ps1 -Port 5005" -ForegroundColor Green
Write-Host "Tip: in the planner, use room ids icu-101 / icu-102 / exam-201 to light up the Hierarchy lens." -ForegroundColor Gray
