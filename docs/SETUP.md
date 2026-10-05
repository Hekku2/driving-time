# WSL development and Windows export

Keep the checkout and development tools in WSL2's Linux filesystem. Editing/import, C# builds/tests, offline conversion and Windows export run in WSL; Windows receives only the exported runnable game and required companion files. No Windows development SDK, Python, Node.js, container or external GIS CLI is required.

## Development preflight — 2026-10-05

From an existing Bash terminal in the repository root:

```bash
bash scripts/check-environment.sh
```

The Bash bootstrap reports missing/unselectable SDKs before running dependency-free C# checks. Required checks cover WSL2/x64, Linux checkout/tools, installed .NET 10, the exact global.json SDK, Git, the complete matching Godot .NET editor and Windows x64 template files. Display configuration and setup-helper prerequisites are reported separately. Exit 0 means required inventory passes; 1 means a failed requirement; 2 means invalid arguments. `--help` describes usage. No installation or solution restore is performed; .NET compiles the checker into its per-user cache on first run.

The launcher uses a subshell and does not use `exec`; sourcing it preserves the parent working directory/options and returns under normal interactive Bash settings. A caller explicitly using `set -e` must handle failure, for example `bash scripts/check-environment.sh || :`. A terminal configured to run only one command can still close when that command finishes.

Latest actual inspection: SDK 10.0.112, Git 2.43.0 and WSL2/x64 pass. Godot and templates are missing from checked locations. curl, unzip, sha256sum and display configuration are available. Set `GODOT4` to a custom complete Linux editor executable; `XDG_DATA_HOME` controls template lookup. Default editor path appears below; PATH names `godot-mono`, `godot4` and `godot` are fallback candidates. This is a bounded inventory, not a full-disk search or proof of GUI/build/export operation.

## Selected toolchain

| Component | Selection |
|---|---|
| Editor | Godot 4.5.2-stable .NET, Linux x86_64; extract the full archive including GodotSharp |
| Export templates | Matching Godot 4.5.2-stable .NET templates, Windows x64 debug/release files |
| SDK | Linux .NET SDK 10.0.112; global.json disables roll-forward/prereleases |
| Game | Godot.NET.Sdk/4.5.2, net8.0 |
| Pipeline/tests | net10.0, independent of Godot |

Historical checks on 2026-10-03/04 verified this SDK building the game/pipeline, editor import/runtime and Windows cross-export. The editor plugin permits LatestMajor runtime roll-forward; exported Windows runtime includes .NET 8.0.31. Earlier installed-path observations do not establish the current editor installation. See [dated evidence](TESTING.md#minimal-scene-build-export-and-native-startup). The recorded .NET 8 support deadline is 2026-11-10; review the complete game toolchain before then.

## Install or locate the tools

1. Use an Ubuntu WSL2 checkout in your Linux home. Install Git if missing: `sudo apt install git`. Use WSLg for interactive editing; a display variable alone does not prove graphics/driver functionality.
2. Use the exact Linux SDK selected by global.json. This environment has 10.0.112. On another machine, obtain that SDK from [Microsoft .NET 10 downloads](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), verify its provider checksum and install inside WSL. Adjust PATH/DOTNET_ROOT to its Linux installation. Windows dotnet.exe is not the development toolchain.
3. Obtain the selected editor and .NET templates from the [official release archive](https://godotengine.org/download/archive/4.5.2-stable/). Extract the complete editor; install the TPZ using Editor → Manage Export Templates → Install from File. Default templates live under `${XDG_DATA_HOME:-$HOME/.local/share}/godot/export_templates/4.5.2.stable.mono`.
4. The local `scripts/setup-godot-wsl.sh` helper is an alternative download/cache/install path. Run its `--help` first for overrides. It requires curl, unzip and sha256sum; missing Ubuntu packages can be installed with `sudo apt install curl unzip coreutils`. It records retrieval time and local checksums, but does not compare against provider checksums. Its actual installation was not performed in the recent preflight work; do not infer download success from this guide.

Configure and verify a Bash session, adjusting paths to the actual installation:

```bash
export DOTNET_ROOT=/usr/lib/dotnet
export PATH="$DOTNET_ROOT:$PATH"
export GODOT4="$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64"
dotnet --list-sdks
dotnet --version
dotnet --info
git --version
"$GODOT4" --headless --version
bash scripts/check-environment.sh
```

Expect selected SDK 10.0.112 and Godot 4.5.2.stable.mono with a build suffix. Keep the complete GodotSharp directory beside the executable. For graphical editing use `scripts/launch-godot-wsl.sh` or `"$GODOT4" --path game --editor`; inspect the editor visually and record actual errors. The scripts' presence does not prove installation or launch success.

## Restore, build and export

Initial project restore needs NuGet access or a complete local cache:

```bash
dotnet restore DrivingTime.sln --locked-mode
dotnet build DrivingTime.sln --no-restore
```

Follow [pipeline restore/lock-file policy](PIPELINE.md#restore-and-run). The preflight does not check NuGet connectivity or cache completeness. Follow [RUN.md](RUN.md) for import/manual tests and the C# Windows export/transfer helper. Preserve EXE, PCK and the complete exporter-produced runtime directory together. Export output and logs stay in WSL first; transfer runtime files only. Use a separate artifact folder per tested build and distinguish startup from visual/driving acceptance.

## Sources and checked dates

Checked 2026-10-05: [Microsoft file-based C# apps](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps) for `dotnet run --file`; [Godot 4.5 C# prerequisites](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html) for the .NET editor/SDK requirement. Earlier compatibility/workflow checks, dated 2026-10-03: [Microsoft SDK selection](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json), [WSLg prerequisites](https://learn.microsoft.com/en-us/windows/wsl/tutorials/gui-apps), [Godot export guide](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_projects.html), [Windows export](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_windows.html) and [Microsoft support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core). These sources are retained from earlier research; no new compatibility research or installs are claimed by this documentation cleanup.
