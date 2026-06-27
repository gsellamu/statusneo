<# ============================================================
   consolidate-statusneo.ps1
   One-run setup for the StatusNeo Agentic FDE suite inside this repo.

   Run from: <repo>\statusneo\
       powershell -ExecutionPolicy Bypass -File .\consolidate-statusneo.ps1

   It does two things:
     1. SOURCES  -> copies every *.md source into .\sources\
                    (from the external statusneo-fde-charter folder, if present).
     2. BINARIES -> copies each artifact (.pdf/.docx/.pptx) into
                    ..\twin-service-dotnet\wwwroot\artifacts\statusneo\
                    so /statusneo.html can serve it. Binaries are sourced from
                    your Downloads / Desktop (where they were delivered in-chat),
                    and from the legacy ..\..\StatusNeo folder if it exists.

   Nothing is deleted. After it runs, commit & push:
       git add . ; git commit -m "StatusNeo FDE suite" ; git push
   ============================================================ #>

$ErrorActionPreference = "Stop"
$here     = Split-Path -Parent $MyInvocation.MyCommand.Path          # <repo>\statusneo
$repo     = Split-Path -Parent $here                                 # <repo>
$srcDir   = Join-Path $here "sources"
$wwwArt   = Join-Path $repo "twin-service-dotnet\wwwroot\artifacts\statusneo"

New-Item -ItemType Directory -Force -Path $srcDir | Out-Null
New-Item -ItemType Directory -Force -Path $wwwArt | Out-Null

# ---- candidate source locations (first hit wins, per file) ----
$charterDir = Join-Path $repo "..\..\statusneo-fde-charter"          # external markdown source
$legacyDir  = Join-Path $repo "..\StatusNeo"                         # legacy deliverable folder
$downloads  = Join-Path $env:USERPROFILE "Downloads"
$desktop    = Join-Path $env:USERPROFILE "Desktop"

Write-Host "StatusNeo consolidation" -ForegroundColor Cyan
Write-Host ("  repo root : {0}" -f $repo) -ForegroundColor DarkGray
Write-Host ""

# ===== 1. SOURCES (markdown) =====================================
Write-Host "[1/2] Markdown sources -> .\sources\" -ForegroundColor Cyan
$mdCopied = 0
$mdSearch = @($charterDir, $legacyDir, $here) | Where-Object { $_ -and (Test-Path $_) }
$seen = @{}
foreach ($dir in $mdSearch) {
    Get-ChildItem -Path $dir -Filter *.md -ErrorAction SilentlyContinue | ForEach-Object {
        if (-not $seen.ContainsKey($_.Name) -and $_.Name -ne "README.md") {
            Copy-Item $_.FullName -Destination (Join-Path $srcDir $_.Name) -Force
            $seen[$_.Name] = $true; $mdCopied++
            Write-Host ("    [md] {0}" -f $_.Name) -ForegroundColor Green
        }
    }
}
if ($mdCopied -eq 0) { Write-Host "    (no .md sources found - check the statusneo-fde-charter path)" -ForegroundColor DarkYellow }

# ===== 2. BINARIES (.pdf/.docx/.pptx) ===========================
Write-Host ""
Write-Host "[2/2] Binary artifacts -> wwwroot\artifacts\statusneo\" -ForegroundColor Cyan

$artifacts = @(
    "StatusNeo-Agentic-FDE-MASTER.pdf","StatusNeo-Agentic-FDE-MASTER.docx",
    "01-FDE-CHARTER-PROPOSAL.pdf","01-FDE-CHARTER-PROPOSAL.docx",
    "StatusNeo_FDE_Charter.pptx",
    "StatusNeo_FDE_PR-FAQ_Playbook.pdf","StatusNeo_FDE_PR-FAQ_Playbook.docx",
    "FDE-80-20-Roles-and-Lifecycle.pdf","FDE-80-20-Roles-and-Lifecycle.docx",
    "Agentic-FDE-Lifecycle.pdf","Agentic-FDE-Lifecycle.docx",
    "Multi-Cloud-Agentic-FDE-Tooling-Directory.pdf","Multi-Cloud-Agentic-FDE-Tooling-Directory.docx",
    "AEC-Modutecture-FDE-UseCase.pdf","AEC-Modutecture-FDE-UseCase.docx",
    "02-FDE-LOOP-COOKBOOK.docx","03-LANDING-RUNBOOK.docx",
    "09-FDE-30-60-90-OKR-STAR.pdf","09-FDE-30-60-90-OKR-STAR.docx",
    "10-FDE-OKR-SCORECARD.pdf","10-FDE-OKR-SCORECARD.docx"
)
$binSearch = @($wwwArt, $legacyDir, $downloads, $desktop, $repo) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

$binCopied = 0; $missing = @()
foreach ($name in $artifacts) {
    $src = $null
    foreach ($d in $binSearch) { $cand = Join-Path $d $name; if (Test-Path $cand) { $src = $cand; break } }
    if (-not $src) { $missing += $name; continue }
    $dest = Join-Path $wwwArt $name
    if ((Resolve-Path $src).Path -ne (Resolve-Path -LiteralPath $dest -ErrorAction SilentlyContinue).Path) {
        Copy-Item $src -Destination $dest -Force
    }
    Write-Host ("    [bin] {0}" -f $name) -ForegroundColor Green
    $binCopied++
}

# ---- summary ----
Write-Host ""
Write-Host ("Done. {0} markdown source(s), {1}/{2} binaries in place." -f $mdCopied, $binCopied, $artifacts.Count) -ForegroundColor Cyan
Write-Host ("  sources : {0}" -f $srcDir)
Write-Host ("  served  : {0}" -f $wwwArt)
if ($missing.Count) {
    Write-Host ""
    Write-Host ("{0} binary(ies) not found in Downloads/Desktop/legacy folder:" -f $missing.Count) -ForegroundColor Yellow
    $missing | ForEach-Object { Write-Host ("   - {0}" -f $_) -ForegroundColor Yellow }
    Write-Host "Re-download those from the chat (or drop them into Downloads) and run again." -ForegroundColor Yellow
}
Write-Host ""
Write-Host "Next: browse /statusneo.html (nav > StatusNeo FDE), then commit:" -ForegroundColor Yellow
Write-Host "  git add . ; git commit -m `"Add StatusNeo Agentic FDE suite`" ; git push" -ForegroundColor White
