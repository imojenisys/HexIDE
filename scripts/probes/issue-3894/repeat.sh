#!/usr/bin/env bash
# Run from the repository root after building HexIDE.Tests. Keep each failure's DAP frames and stderr.
set -u
export PATH="$(printf '%s' "$PATH" | tr ':' '\n' | grep -v '^/mnt/' | paste -sd:)"
export DOTNET_CLI_HOME="$PWD/artifacts/dotnet-home"
export NUGET_PACKAGES="$PWD/artifacts/nuget-packages"
output="$PWD/artifacts/issue-3894"
mkdir -p "$output"
workers=()
for ((i=0; i<2; i++)); do yes > /dev/null & workers+=("$!"); done
trap 'kill "${workers[@]}" 2>/dev/null || true' EXIT
failed=0
for ((i=1; i<=${1:-200}; i++)); do
    dotnet IDE/HexIDE.Tests/bin/Debug/net10.0/HexIDE.Tests.dll -method '*Debugpy_runs_a_complete_session' > "$output/$i.log" 2>&1
    code=$?
    if ((code != 0)) || ! rg -q 'Total: 1, Errors: 0, Failed: 0, Skipped: 0, Not Run: 0' "$output/$i.log"; then
        failed=$((failed+1))
        printf 'iteration %d: runner exit %d (inspect %s/%d.log)\n' "$i" "$code" "$output" "$i"
    fi
    if ((i % 10 == 0)); then printf '%d completed; %d failures\n' "$i" "$failed"; fi
done
printf 'Total: %d iterations; %d failures\n' "${1:-200}" "$failed"
((failed == 0))
