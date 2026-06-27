<#
  setup.ps1 - one-time prep against your live Jeeth.ai infra (Windows PowerShell).
  Creates the isolated journal DB, loads the Neo4j knowledge graph, checks the model.
  Safe to re-run. Run from the repo root:   .\scripts\setup.ps1
#>
[CmdletBinding()]
param(
  [string]$PgContainer    = "jeethhypno-postgres",
  [string]$Neo4jContainer = "jeethhypno-neo4j",
  [string]$OllamaContainer = "jeethhypno-ollama",
  [string]$Neo4jUser      = "neo4j",
  [string]$Neo4jPass      = "jeeth2025",
  [string]$OllamaModel    = "mistral-nemo",
  [string]$Database       = "modutecture"
)
$ErrorActionPreference = "Stop"
function Ok($m){ Write-Host "  [OK]  $m" -ForegroundColor Green }
function Warn($m){ Write-Host "  [!]   $m" -ForegroundColor Yellow }
function Step($m){ Write-Host "`n== $m ==" -ForegroundColor Cyan }

# resolve repo root (parent of this script's folder)
$Root = Split-Path -Parent $PSScriptRoot
$Seed = Join-Path $Root "neo4j\seed.cypher"

Step "Docker reachable?"
docker ps --format "{{.Names}}" | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Docker not reachable. Start Docker Desktop and your infra stack." }
Ok "docker responding"

Step "Create isolated journal DB '$Database' in $PgContainer (Jeeth.ai data untouched)"
$exists = docker exec $PgContainer psql -U postgres -tAc "SELECT 1 FROM pg_database WHERE datname='$Database'" 2>$null
if ($exists -match "1") {
  Ok "database '$Database' already exists"
} else {
  docker exec $PgContainer psql -U postgres -c "CREATE DATABASE $Database;" | Out-Null
  if ($LASTEXITCODE -eq 0) { Ok "created database '$Database'" }
  else { Warn "could not create DB - check the postgres password; edit appsettings.Real.json if not 'postgres'" }
}

Step "Load space-planning knowledge graph into Neo4j"
if (-not (Test-Path $Seed)) { throw "seed not found: $Seed" }
Get-Content $Seed -Raw | docker exec -i $Neo4jContainer cypher-shell -u $Neo4jUser -p $Neo4jPass
if ($LASTEXITCODE -eq 0) { Ok "Neo4j seed loaded (room programs + rules + Moducule types)" }
else { Warn "Neo4j seed failed - the app will fall back to in-memory grounding (demo still works)" }

Step "Confirm the LLM is present"
$models = docker exec $OllamaContainer ollama list 2>$null
if ($models -match $OllamaModel) { Ok "model '$OllamaModel' available" }
else { Warn "model '$OllamaModel' not found. Pull it:  docker exec $OllamaContainer ollama pull $OllamaModel" }

Write-Host "`nSetup complete. Next:  .\scripts\run.ps1" -ForegroundColor Green
