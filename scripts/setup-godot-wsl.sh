#!/usr/bin/env bash
# Local SETUP.md step 3 helper; intentionally excluded from Git.
set -euo pipefail

if [[ "${1:-}" == --help ]]; then
    cat <<'HELP'
Usage: ./scripts/setup-godot-wsl.sh
Download/cache and install Godot 4.5.2 .NET Linux x86_64 and export templates.
Requires curl, unzip and sha256sum. No sudo or editor launch needed.
Overrides: GODOT_INSTALL_DIR, GODOT_DOWNLOAD_DIR, XDG_DATA_HOME, XDG_CACHE_HOME.
Existing release files are refreshed; cached downloads are reused.
HELP
    exit 0
fi
[[ $# == 0 ]] || { printf '%s\n' 'Unexpected argument. Use --help.' >&2; exit 1; }
[[ "$(uname -s)" == Linux && "$(uname -m)" == x86_64 ]] || {
    printf '%s\n' 'Run this helper inside Linux x86_64 WSL2.' >&2; exit 1
}
for tool in curl unzip sha256sum; do
    command -v "$tool" >/dev/null || {
        printf 'Missing %s. Install it in WSL first.\n' "$tool" >&2; exit 1
    }
done

install_dir="${GODOT_INSTALL_DIR:-$HOME/.local/share/driving-time/godot-4.5.2}"
download_dir="${GODOT_DOWNLOAD_DIR:-${XDG_CACHE_HOME:-$HOME/.cache}/driving-time/godot-4.5.2}"
templates_dir="${XDG_DATA_HOME:-$HOME/.local/share}/godot/export_templates/4.5.2.stable.mono"
editor_archive=Godot_v4.5.2-stable_mono_linux_x86_64.zip
templates_archive=Godot_v4.5.2-stable_mono_export_templates.tpz
release_url=https://github.com/godotengine/godot-builds/releases/download/4.5.2-stable
mkdir -p -- "$download_dir"

download() {
    local name="$1"
    if [[ ! -f "$download_dir/$name" ]]; then
        printf 'Downloading %s\n' "$name"
        curl --fail --location --show-error --proto '=https' --proto-redir '=https' \
            --connect-timeout 30 --max-time 1800 --retry 2 \
            --output "$download_dir/$name.part" "$release_url/$name" || {
            printf 'Download failed. Rerun to retry: %s\n' "$release_url/$name" >&2; exit 1
        }
        mv -- "$download_dir/$name.part" "$download_dir/$name"
    fi
    unzip -tq "$download_dir/$name" >/dev/null || {
        printf 'Invalid cached archive: %s. Move it aside and rerun.\n' "$download_dir/$name" >&2; exit 1
    }
}
download "$editor_archive"
download "$templates_archive"
(
    cd -- "$download_dir"
    sha256sum "$editor_archive" "$templates_archive" > SHA256SUMS.local
    printf 'Recorded UTC: %s\nSource: %s\n' "$(date -u +%FT%TZ)" "$release_url" > download-record.txt
)

# Preserve GodotSharp and flatten the editor archive's outer directory.
# Retain staging for inspection; archive/layout errors precede installation.
staging_dir="$(mktemp -d "$download_dir/extract.XXXXXX")"
unzip -q "$download_dir/$editor_archive" -d "$staging_dir/editor"
unzip -q "$download_dir/$templates_archive" -d "$staging_dir/export"
editor_root="$staging_dir/editor/Godot_v4.5.2-stable_mono_linux_x86_64"
editor_name=Godot_v4.5.2-stable_mono_linux.x86_64
[[ -f "$editor_root/$editor_name" && -d "$editor_root/GodotSharp" ]] || {
    printf 'Unexpected editor archive layout. Inspect %s\n' "$staging_dir/editor" >&2; exit 1
}
template_version="$(cat "$staging_dir/export/templates/version.txt")"
[[ "$template_version" == 4.5.2.stable.mono ]] || {
    printf 'Unexpected template version: %s\n' "$template_version" >&2; exit 1
}
mkdir -p -- "$install_dir" "$templates_dir"
cp -a -- "$editor_root/." "$install_dir/"
cp -a -- "$staging_dir/export/templates/." "$templates_dir/"
chmod u+x -- "$install_dir/$editor_name"
printf 'Editor: %s\nTemplates: %s\nChecksums: %s\nRetained staging: %s\n' \
    "$install_dir/$editor_name" "$templates_dir" "$download_dir/SHA256SUMS.local" "$staging_dir"
printf '%s\n' 'Files installed. Verify using SETUP.md step 5; launch with scripts/launch-godot-wsl.sh.'
