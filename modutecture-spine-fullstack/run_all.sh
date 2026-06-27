#!/usr/bin/env bash
# One command: run both harnesses, prove parity, build the viability dashboard.
set -e
export PATH="$PATH:/tmp/dotnet" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
echo "== Python oracle harness =="
( cd tests && python harness.py )
echo "== .NET twin-service harness =="
( cd harness && dotnet run -c Release )
echo "== Build viability dashboard =="
( cd dashboard && python build_dashboard.py )
echo "Open dashboard/dashboard.html"
