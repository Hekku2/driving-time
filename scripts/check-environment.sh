#!/usr/bin/env bash
# Bootstrap only: development checks live in the C# script.
# Isolate options, cwd and exits when this file is sourced into an existing shell.
(
set -euo pipefail
repo_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
cd -- "$repo_root"
if ! command -v dotnet >/dev/null 2>&1; then
    printf '%s\n' 'FAIL: Linux .NET SDK is missing from PATH. Install SDK 10.0.112 in WSL (docs/SETUP.md step 2), then rerun.' >&2
    exit 1
fi
if ! dotnet --version >/dev/null 2>&1; then
    printf '%s\n' 'FAIL: Cannot select the SDK pinned in global.json. Installed SDKs:' >&2
    dotnet --list-sdks
    printf '%s\n' 'Install the exact pinned Linux SDK (currently 10.0.112); see docs/SETUP.md step 2.' >&2
    exit 1
fi
dotnet run --file scripts/check-environment.cs -- "$@"
)
