<#
  smoke.ps1 - drive the live service via GraphQL and verify the demo path end-to-end.
  Run in a SECOND terminal while run.ps1 is up:   .\scripts\smoke.ps1
  Prints the health panel, then exercises place/reject/agent and checks the journal.
#>
[CmdletBinding()]
param([int]$Port = 5000, [string]$Room = ("smoke-" + (Get-Random -Max 9999)))
$ErrorActionPreference = "Stop"
$base = "http://localhost:$Port"
function Ok($m){ Write-Host "  [PASS] $m" -ForegroundColor Green }
function No($m){ Write-Host "  [FAIL] $m" -ForegroundColor Red }
function Step($m){ Write-Host "`n== $m ==" -ForegroundColor Cyan }

function Gql([string]$query){
  $body = @{ query = $query } | ConvertTo-Json -Depth 8 -Compress
  return Invoke-RestMethod -Uri "$base/graphql" -Method Post -ContentType "application/json" -Body $body
}

Step "Health (shows real vs. fallback)"
try { Invoke-RestMethod "$base/health" | ConvertTo-Json -Depth 6 | Write-Host }
catch { No "health endpoint unreachable - is run.ps1 up on port $Port?"; exit 1 }

Step "1) place a legal headwall"
$r1 = Gql "mutation{ placeModucule(room:`"$Room`",cmd:{typeId:`"headwall-hw204`",x:2000,y:200},expectedVersion:0){ status version event{seq} } }"
if ($r1.data.placeModucule.status -eq "ACCEPTED") { Ok "headwall ACCEPTED (v$($r1.data.placeModucule.version))" } else { No "headwall not accepted" }

Step "2) reject an illegal bed (far from med-gas) - must cite a rule and write nothing"
$r2 = Gql "mutation{ placeModucule(room:`"$Room`",cmd:{typeId:`"bed-icu`",x:500,y:2800},expectedVersion:1){ status violations{rule severity message} } }"
$rules = ($r2.data.placeModucule.violations | ForEach-Object { $_.rule }) -join ", "
if ($r2.data.placeModucule.status -eq "REJECTED") { Ok "bed REJECTED, cited: $rules" } else { No "illegal bed was not rejected" }

Step "3) legal bed in front of headwall - commits, earns a med-gas binding"
$r3 = Gql "mutation{ placeModucule(room:`"$Room`",cmd:{typeId:`"bed-icu`",x:2000,y:1500},expectedVersion:1){ status event{seq} violations{rule severity} } }"
if ($r3.data.placeModucule.status -eq "ACCEPTED") { Ok "bed ACCEPTED (seq $($r3.data.placeModucule.event.seq))" } else { No "legal bed not accepted" }
$tw = Gql "query{ twin(room:`"$Room`"){ version instances{typeId} bindings{kind from to} } }"
if ($tw.data.twin.bindings.Count -ge 1) { Ok "med-gas binding present ($($tw.data.twin.bindings[0].kind))" } else { No "no binding earned" }

Step "4) grounded agent proposes (Neo4j -> mistral-nemo -> gate). May take a few seconds."
$ag = Gql "mutation{ agentSuggest(room:`"$Room`",goal:`"observation room`"){ proposal{typeId x y rotation} rationale citations } }"
$p = $ag.data.agentSuggest.proposal
if ($p) {
  Ok "agent proposed $($p.typeId) at ($([int]$p.x),$([int]$p.y))"
  Write-Host "       rationale: $($ag.data.agentSuggest.rationale)" -ForegroundColor Gray
  Write-Host "       citations: $(( $ag.data.agentSuggest.citations) -join ', ')" -ForegroundColor Gray
} else {
  Write-Host "       agent returned no proposal: $($ag.data.agentSuggest.rationale)" -ForegroundColor Yellow
}

Step "5) journal integrity - rejects left no trace"
$ev = Gql "query{ events(room:`"$Room`"){ seq type } }"
$placed = ($ev.data.events | Where-Object { $_.type -eq 'MODUCULE_PLACED' }).Count
if ($placed -eq 2) { Ok "exactly 2 events written (the REJECT wrote nothing)" } else { No "expected 2 events, got $($ev.data.events.Count)" }

Write-Host "`nSmoke test complete. If these are green, your live demo path works end-to-end." -ForegroundColor Green
