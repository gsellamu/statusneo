<#
  run.ps1 - start the twin service on the host against your live infra.
  Reaches containers via localhost:<port>. Opens the viewer.
  Usage:   .\scripts\run.ps1            (real infra)
           .\scripts\run.ps1 -InMemory  (zero-infra fallback for the room)
#>
[CmdletBinding()]
param(
  [switch]$InMemory,
  [int]$Port = 5000,
  [string]$PgConn  = "Host=localhost;Port=5431;Database=modutecture;Username=postgres;Password=postgres",
  [string]$Neo4j   = "bolt://localhost:7687",
  [string]$Neo4jUser = "neo4j",
  [string]$Neo4jPass = "jeeth2025",
  [string]$Ollama  = "http://localhost:11434",
  [string]$Model   = "mistral-nemo",
  [ValidateSet("ollama","reflective","deterministic")][string]$AgentMode = "ollama",
  [switch]$Redpanda
)
$ErrorActionPreference = "Stop"
function Step($m){ Write-Host "`n== $m ==" -ForegroundColor Cyan }

$Root = Split-Path -Parent $PSScriptRoot
$Svc  = Join-Path $Root "twin-service-dotnet"

Step "Check .NET SDK"
$null = & dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) { throw "`.NET 8 SDK not found. Install from https://dot.net then re-run." }
Write-Host "  dotnet $(dotnet --version)" -ForegroundColor Green

# pass config via environment (overrides appsettings). Double-underscore = nested keys.
$env:ASPNETCORE_URLS               = "http://localhost:$Port"
$env:ConnectionStrings__Twin       = $PgConn
$env:Neo4j__Uri                    = $Neo4j
$env:Neo4j__User                   = $Neo4jUser
$env:Neo4j__Pass                   = $Neo4jPass
$env:Ollama__Url                   = $Ollama
$env:Ollama__Model                 = $Model
$env:Agent__Mode                   = $AgentMode
if ($Redpanda) { $env:Telemetry__Bootstrap = "localhost:19092" } else { $env:Telemetry__Bootstrap = "" }

$argsList = @("run","-c","Release","--project",$Svc)
if ($InMemory) { $argsList += @("--","--InMemory"); $env:InMemory = "true" } else { $env:InMemory = "false" }

Step ("Starting twin service on http://localhost:{0}  (mode: {1})" -f $Port, ($(if($InMemory){"in-memory"}else{"REAL infra"})))
Write-Host "  viewer : http://localhost:$Port"
Write-Host "  health : http://localhost:$Port/health"
Write-Host "  metrics: http://localhost:$Port/metrics"
Write-Host "  studio : http://localhost:$Port/studio.html   (multi-lens)"
Write-Host "  arch   : http://localhost:$Port/architecture.html (self-describing)"
Write-Host "  bldg   : http://localhost:$Port/hierarchy.html (Building roll-up)"
Write-Host "  telem  : http://localhost:$Port/telemetry.html (operational POC)"
Write-Host "  vision : http://localhost:$Port/vision.html (Context Intelligence roadmap)"
Write-Host ("  agent mode: {0}" -f $AgentMode)
Write-Host "  (startup prints which deps are live vs. fallback)`n"

# open the viewer shortly after launch, then run in foreground (Ctrl+C to stop)
Start-Job { Start-Sleep 6; Start-Process "http://localhost:$using:Port" } | Out-Null
& dotnet @argsList
