# WSL development and Windows export

Updated 2026-10-03 following the user's WSL workflow requirement (D012) and approval of .NET 10 (D013). All development, Godot editing/import, C# builds, tests, offline pipeline work and Windows exports run in WSL2. Keep the repository in the WSL Linux filesystem. Transfer only the exported game to Windows for native playtesting. Windows desktop x64 remains the first target. C#/.NET remains fixed for both game and pipeline. No second language runtime, container or external GIS CLI is required.

## P02 verification update — 2026-10-03

The editor and templates are now installed at the paths in step 5. P02 verified SDK 10.0.112 building an explicitly authored net8.0 game, Godot 4.5.2 .NET import/runtime/editor launch, Windows release cross-export, runtime-only transfer and native Windows startup. Export includes a self-contained .NET 8.0.31 runtime directory, EXE and PCK. Visual acceptance remains pending; WSLg uses software rendering and lacks Linux audio libraries. Follow [RUN.md](RUN.md) for the current project commands and [TESTING.md](TESTING.md#p02--minimal-scene-build-export-and-native-startup) for actual results. The inventory and unperformed-setup wording below describe the earlier P01 session and are historical where superseded by this update. Tagged project-generator source was unavailable; the csproj was not editor-generated.

## Observed tools

| Component | Actual evidence | Readiness |
|---|---|---|
| Agent OS | Ubuntu 24.04.4 LTS, x86_64; kernel 6.6.87.2-microsoft-standard-WSL2 | WSL2 Linux session confirmed |
| Windows host | Windows NT version 10.0.26200.0; Windows PowerShell 5.1.26100.9444 | Interop inspection succeeded after sandbox escalation; product name and physical architecture not independently established |
| Linux .NET | SDK 10.0.112; runtime 10.0.12; inventory reconfirmed 2026-10-03 | Installed SDK matches the repository pin |
| Windows .NET | SDKs 3.0.100, 3.1.100, 5.0.102, 5.0.200; selected before pin 5.0.200; x64 host/runtime 8.0.4 also present | Historical inventory only; Windows SDK is not required for this workflow |
| Git | Linux 2.43.0; Windows 2.27.0.windows.1 at G:\Git\cmd\git.exe | Available; no Git install needed for P02 |
| Godot .NET | godot/godot4/godot-mono absent from Linux PATH; godot/godot4 absent from Windows PATH and no editor found in inspected common locations | Required editor location/install unresolved; a portable copy elsewhere is not ruled out |
| Export templates | Default Linux and C-drive Windows profile export-template locations absent | Matching .NET templates required; custom locations unverified |
| Projects | No solution, csproj or project.godot found in repository | P02 creates game; P06 creates pipeline |
| Reference hardware | No CPU/GPU/RAM performance reference selected or benchmark run | Unknown |

Commands and actual limitations are recorded in [TESTING.md](TESTING.md). A later bounded PowerShell/CIM inspection returned blank output; it supplies no additional evidence. The inventory is not a claim to have exhaustively searched every Windows drive.

## Selected versions and compatibility

| Component | Exact selection | Compatibility basis |
|---|---|---|
| Editor | Godot 4.5.2-stable .NET, Linux x86_64 (`Godot_v4.5.2-stable_mono_linux_x86_64.zip`) | [Official 4.5.2 archive](https://godotengine.org/download/archive/4.5.2-stable/) lists this .NET build |
| Templates | Godot 4.5.2-stable .NET (`Godot_v4.5.2-stable_mono_export_templates.tpz`) | Same release's .NET export templates; do not mix standard/.NET or release versions |
| SDK | .NET SDK 10.0.112 Linux x64, already installed in WSL | User-approved .NET 10; actual local inventory and SDK selection verified; [Microsoft version selection](https://learn.microsoft.com/en-us/dotnet/core/versions/selection) separates SDK selection from project targets |
| Pipeline / pipeline tests | Planned `net10.0` | Use the approved SDK/runtime; verify projects and dependencies in P06 |
| Game | Retain Godot-generated target initially; `net8.0` is the documented 4.5 baseline | [Godot 4.5 C# documentation](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html) identifies .NET 8 and recommends the latest stable SDK; P02 verifies the actual project and export |

SDK discovery is verified; the Godot build/export combination is not yet tested. .NET 10 approval does not require every project to target net10.0. Leave Godot-generated platform conditions intact and verify the actual P02 csproj, editor runtime loading and Windows export. Pipeline code must remain independent of Godot. If a small shared contract becomes necessary, choose a target supported by both actual consumers; create it only on demonstrated need. The installed inventory contains only .NET 10 runtimes: a standalone framework-dependent net8.0 test or CLI may need a .NET 8 runtime. Do not treat SDK availability as proof of runtime compatibility.

The current Godot Windows page offers 4.7.2, but a newer engine is not necessary for the first slice and its precise generated project configuration was not verified in this session. Select the versioned 4.5 baseline for now. Attempts to read tagged project-generator source failed; the framework conclusion uses official versioned docs, not a source inspection claim. The archive's binary redirects were inaccessible to the browsing tool; no archives were downloaded or checksums verified.

.NET 10 is an LTS release per [Microsoft's support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core). The Godot 4.5 net8.0 baseline still needs review before .NET 8 support ends **2026-11-10**. P02 should record the supported game target and runtime/export requirements; changing the SDK alone does not upgrade that target. Official Godot C# and Microsoft version-selection/global.json/support sources were rechecked 2026-10-03.

The repository [global.json](../global.json) pins 10.0.112 with `rollForward: disable` and prereleases disabled. [Microsoft documents exact SDK selection](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json); an absent SDK must fail rather than silently select another SDK. The installed WSL SDK satisfies this pin. The SDK version and target framework are different settings ([version selection](https://learn.microsoft.com/en-us/dotnet/core/versions/selection)).

Use one root `DrivingTime.sln`, created in P02 (or P06 if selected first). Add the game, pipeline and targeted .NET test projects as they are created. Godot may create a game-local solution; the root solution will be the documented repository build entry point. No solution or project is created by P01.

## Required WSL setup (not performed here)

1. Use the existing Ubuntu WSL2 checkout at `/home/hekku2/projects/personal/driving-time`. Keep source, caches, raw data, generated assets, SDK and editor in the Linux filesystem. Windows needs only the exported game's runnable files; no Windows SDK, Godot editor, source checkout or pipeline is required.
2. Use the existing **SDK 10.0.112 Linux x64** at `/usr/lib/dotnet/sdk`. No SDK download is needed on this machine. On another WSL machine, obtain the exact SDK from [Microsoft's .NET 10 downloads](https://dotnet.microsoft.com/en-us/download/dotnet/10.0), verify its provider checksum and install it inside WSL. Use the Linux SDK, not dotnet.exe through Windows interop. Keep tools outside Git.
3. Run `./scripts/setup-godot-wsl.sh` from the repository root. This local, Git-ignored helper downloads and caches the **Linux x86_64 .NET** editor ZIP and matching **4.5.2 .NET** templates TPZ from the official release, records local SHA-256 checksums and retrieval time, extracts the whole editor including GodotSharp to `$HOME/.local/share/driving-time/godot-4.5.2`, and installs templates into `${XDG_DATA_HOME:-$HOME/.local/share}/godot/export_templates/4.5.2.stable.mono`. Run with `--help` for path overrides. It requires `curl`, `unzip` and `sha256sum`; existing files for this release are refreshed. Cache and extraction staging remain outside Git. Recorded checksums identify downloaded files; they are not a comparison against provider checksums. Actual downloads/installation have not been run in this session.

   Manual alternative: obtain both files from the [official archive](https://godotengine.org/download/archive/4.5.2-stable/), extract the complete editor, then use Editor → Manage Export Templates → Install from File for the TPZ. Sources checked 2026-10-03: official archive and [Godot's tagged template installer](https://github.com/godotengine/godot/blob/4.5.2-stable/editor/export/export_template_manager.cpp), which uses the TPZ's `version.txt` for its installation directory. The archive layout and actual editor launch remain unverified until setup is run. This helper is available only in this local checkout.
4. For interactive editing, use the Linux editor through WSLg. [Microsoft documents WSL2 Linux GUI support and graphics prerequisites](https://learn.microsoft.com/en-us/windows/wsl/tutorials/gui-apps). WSLg/display/GPU availability has not been verified here. If unavailable, record that limitation; headless preparation does not satisfy the editor's visual check.

   Local helper: run `./scripts/launch-godot-wsl.sh` from the repository root after step 3. It configures SDK discovery, checks for a display and the editor executable, verifies SDK selection via `dotnet --version`, and launches the editor. Override `GODOT4` or `DOTNET_ROOT` if your installation paths differ; additional arguments are passed to Godot. The script is intentionally ignored by Git and is available only in this local checkout.

5. Open Bash in WSL, configure this session and verify versions. Adjust the executable path to the actual extracted directory.

```bash
export DOTNET_ROOT="/usr/lib/dotnet"
export PATH="$DOTNET_ROOT:$PATH"
export GODOT4="$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64"

cd /home/hekku2/projects/personal/driving-time
command -v dotnet git
dotnet --list-sdks
dotnet --version
dotnet --info
git --version
"$GODOT4" --headless --version
ls "${XDG_DATA_HOME:-$HOME/.local/share}/godot/export_templates/4.5.2.stable.mono"
```

Expected: `dotnet --version` reports **10.0.112**, dotnet resolves to a Linux binary, Godot reports **4.5.2.stable.mono** with any build suffix, and matching .NET templates are installed. Adjust DOTNET_ROOT/PATH if another machine uses a different Linux SDK location. Launch the Linux editor from this configured session so it inherits SDK discovery. SDK inventory/selection were checked; Godot setup and editor launch remain unperformed.

Initial NuGet restore runs in WSL during P02/P06; later offline commands require the restore/import cache. Record actual package pins and restore policy when projects exist. No extra Windows development tooling is required.

## Export in WSL; run the exported game in Windows

P02 creates a credential-free **Windows Desktop x86_64** export preset and attempts C# cross-export from Linux Godot inside WSL. Build/export support is documented for desktop platforms in [Godot's C# guide](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html), but this specific WSL-to-Windows export path still needs P02 evidence. No build or export has been attempted.

Keep import and export output in WSL first. After a successful export, copy its runnable output into a dedicated Windows folder via `/mnt/c/...`. Windows runs the resulting .exe directly. Only game runtime output crosses this boundary; the source, editor, SDK, conversion tools and datasets remain in WSL.

“Only executable” means the exported playable game, including any required companion runtime files. A Godot C# export may need a PCK plus assemblies/runtime files alongside the .exe. Preserve the exporter-produced layout; do not promise that copying one .exe alone works. [Godot's export guide](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_projects.html) distinguishes runnable exports from resource-only packages. P02 will inventory and test the actual output.

Illustrative transfer command **after P02 defines and produces these paths** (not runnable yet):

```bash
# Replace WINDOWS_USER with the intended Windows account.
# Copy only the contents of the validated Windows export directory.
mkdir -p /mnt/c/Users/WINDOWS_USER/Games/DrivingTime
cp -a build/windows/. /mnt/c/Users/WINDOWS_USER/Games/DrivingTime/
```

Use a separate output folder per tested build to avoid mixing stale files. Open the exported .exe from Windows Explorer and retain every required sibling file/directory. P02 must record WSL C# build, Linux editor scene inspection, Windows cross-export, transfer and native Windows launch as separate results; expected first scene is a visible floor and cube without startup errors. [Windows export documentation](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_windows.html) remains the target-platform reference.

No export preset, output folder, copy operation or executable is created by this documentation change. Keep unsigned local export as the first test; packaging a literal single-file executable would need a separate verified packaging decision.

## First driving slice and performance

| Option | Fit / cost | Decision |
|---|---|---|
| C# CharacterBody3D arcade controller | Scripted velocity/heading; wall and slope support through MoveAndSlide; gravity must be applied in code. Few tuning parameters and predictable resets. | Select for P03; behavior still needs P03–P05 evidence |
| VehicleBody3D with VehicleWheel3D | Built-in raycast vehicle, but adds wheel setup, suspension/traction tuning and known limitations before the simple driving experiment. | Defer |

The API descriptions come from [CharacterBody3D](https://docs.godotengine.org/en/4.5/classes/class_characterbody3d.html) and [VehicleBody3D](https://docs.godotengine.org/en/4.5/classes/class_vehiclebody3d.html). Choice is an engineering judgment, not a tested driving result. Plan C# `_PhysicsProcess`, acceleration/braking/reverse, speed-dependent steering and a simple collision shape; tune only a few parameters. Collision/slope/reset checks in P05 can trigger a focused fix without redesigning the controller. No physics code is implemented now.

Provisional target: **1920 × 1080 at 60 FPS** (16.67 ms frame budget), single-player daylight prototype on a still-unknown reference machine. Use a provisional 60 Hz physics tick and check rendering-frame-rate independence in P03. Renderer choice and actual graphics settings remain unverified; record them with P02/P26 hardware evidence. This is a target, not a measured or promised result.
