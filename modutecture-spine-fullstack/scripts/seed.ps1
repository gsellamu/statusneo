<#
  seed.ps1 - load the Neo4j space-planning knowledge graph. Finds seed.cypher even if
  folders moved, and skips gracefully (with a clear message) if Neo4j isn't reachable.
  Usage:  .\scripts\seed.ps1   [-Neo4jPass jeeth2025]
#>
[CmdletBinding()]
param(
  [string]$Neo4jContainer = "jeethhypno-neo4j",
  [string]$Neo4jUser = "neo4j",
  [string]$Neo4jPass = "jeeth2025",
  [string]$Seed
)
function Ok($m){ Write-Host "  [OK]  $m" -ForegroundColor Green }
function Warn($m){ Write-Host "  [!]   $m" -ForegroundColor Yellow }

# locate seed.cypher: explicit -Seed, else search from repo root
if (-not $Seed) {
  $root = Split-Path -Parent $PSScriptRoot
  $cand = Join-Path $root "neo4j\seed.cypher"
  if (Test-Path $cand) { $Seed = $cand }
  else { $found = Get-ChildItem -Path $root -Recurse -Filter seed.cypher -ErrorAction SilentlyContinue | Select-Object -First 1
         if ($found) { $Seed = $found.FullName } }
}
if (-not $Seed -or -not (Test-Path $Seed)) { Warn "seed.cypher not found; skipping (app uses in-memory grounding fallback)"; return }

# is Neo4j answering?
$ping = docker exec $Neo4jContainer cypher-shell -u $Neo4jUser -p $Neo4jPass "RETURN 1" 2>$null
if ($LASTEXITCODE -ne 0) { Warn "Neo4j not reachable; skipping seed (app uses in-memory grounding fallback)"; return }

Get-Content $Seed -Raw | docker exec -i $Neo4jContainer cypher-shell -u $Neo4jUser -p $Neo4jPass
if ($LASTEXITCODE -eq 0) { Ok "Neo4j seed loaded from $Seed" } else { Warn "seed load failed; in-memory grounding fallback still works" }
