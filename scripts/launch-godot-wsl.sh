#!/usr/bin/env bash
# Local helper for docs/SETUP.md step 4; intentionally excluded from Git.
set -euo pipefail

repo_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
export DOTNET_ROOT="${DOTNET_ROOT:-/usr/lib/dotnet}"
export PATH="$DOTNET_ROOT:$PATH"
export GODOT4="${GODOT4:-$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64}"

if [[ -z "${DISPLAY:-}" && -z "${WAYLAND_DISPLAY:-}" ]]; then
    printf '%s\n' 'No graphical display detected. Run this script in a WSL2 session with WSLg enabled (SETUP.md step 4).' >&2
    exit 1
fi

if [[ ! -x "$GODOT4" ]]; then
    printf 'Godot executable not found or not executable: %s\n' "$GODOT4" >&2
    printf '%s\n' 'Complete SETUP.md step 3, or set GODOT4 to the extracted Linux .NET editor executable.' >&2
    exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
    printf '%s\n' 'Linux dotnet was not found. Complete SETUP.md step 2 or set DOTNET_ROOT to its installation directory.' >&2
    exit 1
fi

cd -- "$repo_root"
dotnet --version
exec "$GODOT4" "$@"
