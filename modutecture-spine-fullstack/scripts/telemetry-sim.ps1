<#
  telemetry-sim.ps1 - simulate occupied-room sensors feeding the operational twin (POC).
  Default: HTTP ingest to the running service (robust, always works).
  -UseRedpanda: also publish to the Redpanda bus via rpk (the distributed path).
  Usage:  .\scripts\telemetry-sim.ps1 -Port 5005   [-UseRedpanda] [-Seconds 60]
#>
[CmdletBinding()]
param(
  [int]$Port = 5005,
  [int]$Seconds = 60,
  [switch]$UseRedpanda,
  [string]$RedpandaContainer = "jeethhypno-redpanda",
  [string[]]$Rooms = @("icu-101","icu-102","icu-103","exam-201")
)
$base = "http://localhost:$Port"
Write-Host "Streaming simulated sensor telemetry to $base/telemetry/ingest for $Seconds s..." -ForegroundColor Cyan
Write-Host "Open the lens: $base/telemetry.html`n" -ForegroundColor Gray

$end = (Get-Date).AddSeconds($Seconds)
$occ = @{}; foreach($r in $Rooms){ $occ[$r] = Get-Random -Min 0 -Max 3 }

while((Get-Date) -lt $end){
  foreach($room in $Rooms){
    # random walk occupancy 0..4, plausible temp + CO2 that rises with people
    $occ[$room] = [Math]::Max(0,[Math]::Min(4, $occ[$room] + (Get-Random -Min -1 -Max 2)))
    $people = $occ[$room]
    $temp = [Math]::Round(21.0 + ($people * 0.6) + ((Get-Random -Min -5 -Max 6)/10.0), 1)
    $co2  = 450 + ($people * 220) + (Get-Random -Min -40 -Max 60)
    $reading = @{ room=$room; occupancy=$people; tempC=$temp; co2Ppm=[int]$co2; ts=[DateTimeOffset]::UtcNow.ToUnixTimeSeconds() }
    $json = $reading | ConvertTo-Json -Compress

    # robust path: HTTP ingest
    try { Invoke-RestMethod -Uri "$base/telemetry/ingest" -Method Post -ContentType "application/json" -Body $json | Out-Null }
    catch { Write-Host "  ingest failed for $room (is run.ps1 up?)" -ForegroundColor Yellow }

    # distributed path: publish to Redpanda via rpk
    if($UseRedpanda){
      try { $json | docker exec -i $RedpandaContainer rpk topic produce room.telemetry 2>$null | Out-Null }
      catch {}
    }
  }
  Write-Host ("  t={0:HH:mm:ss}  " -f (Get-Date)) -NoNewline
  foreach($r in $Rooms){ Write-Host ("{0}={1} " -f $r,$occ[$r]) -NoNewline }
  Write-Host ""
  Start-Sleep -Milliseconds 1200
}
Write-Host "`nTelemetry sim complete." -ForegroundColor Green
