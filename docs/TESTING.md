# Testing and evidence

## P04 — Chase Camera

Checked 2026-10-03 in WSL2 using SDK 10.0.112 and installed Godot 4.5.2 .NET. User explicitly selected P04 with P03 manual acceptance pending; P02/P03 and G0/G1 remain open. No tools, packages or datasets added. P04 status: `awaiting-manual-test`.

`GODOT4` denotes `$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64`; Godot commands use `DOTNET_ROOT=/usr/lib/dotnet`.

| Actual command/check | Outcome |
|---|---|
| `dotnet --version`; clean starting `git status --short` | SDK 10.0.112; no prior tracked/untracked changes |
| `dotnet build DrivingTime.sln --no-restore` | Passed before/after interpolation change, 0 warnings/errors |
| `"$GODOT4" --headless --path game --editor --import` | Passed; C# scripts registered, no reported errors |
| `"$GODOT4" --headless --path game res://Checks/CameraChecks.tscn --max-fps 60 --disable-vsync` | Passed, 420 ticks; rerun after enabling interpolation also passed |
| Same headless check with `--max-fps 30` and `144` | Both passed, 420 ticks each; final interpolation-enabled reruns also passed |
| `"$GODOT4" --path game res://Checks/CameraChecks.tscn --max-fps 60 --disable-vsync --audio-driver Dummy` | WSLg-rendered check passed, 420 ticks; llvmpipe and unsupported V-Sync warning |
| `"$GODOT4" --headless --path game res://Checks/DrivingChecks.tscn --max-fps 60 --disable-vsync` | P03 regression passed before/after interpolation, 720 ticks, 713 frame callbacks; checkpoint trace unchanged |
| `git diff --check` | Passed |
| Editor diagnostics for ChaseCamera.cs, CameraChecks.cs, Main.cs | No errors |
| `dotnet run --file scripts/export-windows.cs` | ExportRelease build 0 warnings/errors, import/export passed; 188 runtime files (168.1 MiB) copied and SHA-256 verified at W:\projektit\driving-time |

Artifact/logs: `artifacts/windows/20261003-161322-f7a5a660`. Native Windows startup/drive: **unverified**. Manual visual/feel acceptance: **unverified**. See [RUN.md](RUN.md#p04-manual-test) for tight-loop and wall steps. Rendered startup/test success does not prove appearance, interpolation quality or native performance.

CameraChecks is C# inside Godot, explicitly launched instead of Main.tscn. It moves/turns the target in physics ticks, checks finite/bounded transforms, small smooth turn steps and road-ahead framing, independently queries a sphere at the camera for solid overlap each tick, places the car with the wall behind it, and verifies retraction plus bounded outward return when clear. Teleports are test-only; there is no gameplay reset. P03 checks still exercise real input and MoveAndSlide. Inspector overrides must remain at defaults. Basic convex-wall behavior is covered; pivot-inside-solid and enclosed-space handling are not. Sphere sweep ignores initial overlaps per the engine API, so the pivot must remain clear. Camera tuning and close-range car occlusion need visual feedback.

Official sources checked 2026-10-03: [PhysicsDirectSpaceState3D](https://docs.godotengine.org/en/4.5/classes/class_physicsdirectspacestate3d.html) for CastMotion's safe fraction and initial-overlap limitation; [SpringArm3D](https://docs.godotengine.org/en/4.5/classes/class_springarm3d.html) for swept-volume/exclusion/margin camera behavior; [physics interpolation](https://docs.godotengine.org/en/4.5/tutorials/physics/interpolation/using_physics_interpolation.html) for project enablement, physics-step transforms and reset on discontinuities. A direct sphere sweep is used rather than SpringArm3D so retraction can be immediate while extension is rate-limited. No engine/version/dependency change. Recommended next task: P05 after manual P04 acceptance.

## P00 documentation checks

2026-10-02: the plan and repository contents were inspected using `rg --files`, `cat`, `sed`, and `git status --short`. The supplied plan filename had an extra `.md`; `mv HELSINKI_DRIVING_PLAN.md.md HELSINKI_DRIVING_PLAN.md` normalized it without content edits.

Documentation validation passed: read all 14 generated/updated documentation files with `cat`; checked 44 relative file-link targets, all 28 index rows and statuses, all eight task cards' required fields, and their execution prompts against the source plan in a temporary in-session JavaScript check. P00 is done; P01–P27 are todo. `rg --files docs` confirmed the documentation inventory. `git diff --check` exited 0 (covering the tracked README change; new files are untracked). No game/pipeline build, restore, test, export, editor launch, dataset retrieval or performance check has run.

Optional manual review: read AGENTS.md, PROJECT_STATE.md and tasks/INDEX.md. Expected: only P00 is done, P01–P27 are todo, P09–P27 are deferred, C#/.NET is fixed for both projects, other assumptions are provisional, one selected task is permitted, and unperformed manual checks cannot pass.

## Future checks

SDK and engine selections are recorded in SETUP.md and D013; actual project paths and build/run commands await P02/P06. Commands in task cards are planned checks, not successful results. Do not install or create projects during a documentation update.

| Tasks | Automated / command evidence | Manual evidence |
|---|---|---|
| P01 | Tool/version inspection and official compatibility sources with checked dates | Review setup instructions; no runtime pass implied |
| P02 | C# build and Windows export, separately recorded | Open editor scene and exported application; floor/cube visible without startup errors |
| P03–P05 | C# build; frame-rate checks where available | Two-minute drive, chase camera wall/turn checks, collision/slope/fall/reset checks |
| P06 | Initial restore, build/test, two offline CLI runs with deterministic comparison, invalid-input failure | Follow the documented WSL CLI steps |
| P07 | Cardinal points, 100 m length, nonzero elevation, round trips, trusted-reference comparison | Review axis/CRS/datum contract; no visual substitute for numeric evidence |
| P08 | Manifest/bounds/mesh validation and export | Inspect direction markers and 10 m cube in Godot and export |
| P09–P27 | Defined when their cards are expanded from actual inputs | Follow each plan completion check; record G3–G6 only with evidence |

Use targeted .NET tests for nontrivial parsing, coordinates, geometry and tile lifecycle behavior. Use manual checks for appearance and feel. Store command, environment/build identifier, actual result, limitations and source checked dates where relevant. A build passing does not prove a manual criterion; use `awaiting-manual-test` until the required feedback arrives. Parent tasks need evidence for every required child and their own acceptance criteria.

## P01 — actual research evidence

Checked 2026-10-02. No tools installed, packages restored, projects created or editor/export/run tests attempted.

| Actual command / inspection | Outcome |
|---|---|
| `uname -a`; `cat /etc/os-release` | Ubuntu 24.04.4, x86_64, WSL2 kernel 6.6.87.2-microsoft-standard-WSL2 |
| `command -v dotnet godot godot4 godot-mono git pwsh powershell.exe cmd.exe wsl.exe` | Linux dotnet/git/pwsh and Windows PowerShell/cmd/wsl found; Godot names absent |
| `dotnet --info` before global.json | SDK 10.0.112; .NET runtime 10.0.12; no workloads; no global.json |
| `git --version` | 2.43.0 |
| `rg --files --hidden` filtered for global.json, solution, csproj and project.godot before changes | No matching repository projects or SDK pin |
| Windows PowerShell inventory shown below | Sandbox attempt failed with `UtilBindVsockAnyPort:307: socket failed 1`; escalated read-only retry produced Windows/.NET/Git inventory |
| `ls -ld` of common Linux Godot/template and Windows Program Files/Godot directories | Inspected Godot/template locations absent; Windows dotnet/sdk exists |
| `ls -d /mnt/c/Users/*/AppData/Roaming/Godot/export_templates /mnt/c/Users/*/Downloads/*[Gg]odot* /mnt/c/Users/*/Desktop/*[Gg]odot* /mnt/c/Tools/*[Gg]odot* /mnt/c/Godot` | No matches; exit 2 from missing paths, not proof of absence on every drive |
| Additional bounded Windows PowerShell CIM/common-location probe | Exit 0 but blank output; no reliable extra evidence, architecture/product name left unverified |
| Broader `rg --files` search under common user locations for Godot names | Only unrelated game logs returned; permission errors occurred. No editor found; not an exhaustive installation inventory |
| `dotnet --version` after global.json | Expected SDK-not-found failure requesting 8.0.425, standalone exit 155; installed 10.0.112 cannot satisfy exact pin |
| `dotnet --list-sdks` after pin | Still lists 10.0.112; inventory works without compatible selected SDK |
| `git diff --check` | Initial final check found Markdown hard-break whitespace in changed state lines; converted those breaks to HTML br tags. Final check passed, exit 0 |

Final in-session documentation validation passed: parsed global.json and checked exact version/roll-forward/prerelease values; checked all 28 ledger rows (P00/P01 done, P02–P27 todo), P01 card status and 50 relative file-link targets in the changed documents. Commands were read-only `cat` / `test -f` checks plus temporary JavaScript assertions in the agent tool session; no test scripts or extra runtime dependencies were added to the repository.

The Windows inventory command was `powershell.exe -NoProfile -NonInteractive -Command` with the following read-only body:

```powershell
$PSVersionTable.PSVersion.ToString()
[System.Environment]::OSVersion.VersionString
Get-Command dotnet,git,godot,godot4 -ErrorAction SilentlyContinue | Select-Object Name,Source
if (Get-Command dotnet -ErrorAction SilentlyContinue) { dotnet --info }
if (Get-Command git -ErrorAction SilentlyContinue) { git --version }
Get-ChildItem "$env:APPDATA\Godot\export_templates" -ErrorAction SilentlyContinue | Select-Object Name
```

Actual output: PowerShell 5.1.26100.9444; Windows NT 10.0.26200.0; Windows SDKs 3.0.100, 3.1.100, 5.0.102 and 5.0.200; default SDK 5.0.200; x64 host/runtime 8.0.4; Git 2.27.0.windows.1. Godot commands/templates did not appear. The combined command exited 1 after the silent missing-template lookup; earlier inventory output remains valid. OS version wording alone is not a Windows product-name measurement.

Official sources and checked dates are linked in [SETUP.md](SETUP.md) and D011. Read Godot's 4.5 C# prerequisites, 4.5 Windows/general export docs, 4.5 body API docs, 4.5.2 archive, Microsoft SDK/TFM/SDK-selection/support pages. Tagged Godot project-generator source fetches failed; no generated-project source verification claimed. Archive download redirects were unavailable in the web tool; no binary retrieval claimed. The newer 4.7.2 download listing was observed, but was not enough to establish its exact framework baseline.

Agent review: setup steps contain exact selections, bounded local setup, expected outputs and missing-tool paths; the SDK pin and task ledger were reviewed. This satisfies P01's document-review criterion. User execution of those steps: **not run**. Original setup check superseded by D012 on 2026-10-03. Optional local manual check: after supplying the selected Linux tools, follow SETUP.md's WSL Bash version checks; expect SDK 8.0.425, Godot 4.5.2.stable.mono and matching .NET templates. Editor launch, export and exported-app verification belong to P02 and remain unperformed. G0 remains open.

## 2026-10-03 — WSL workflow documentation update

Updated setup, instructions, plan, decisions, state and affected task cards to honor WSL development and Windows runtime-only transfer. P00/P01 remain done; P02–P27 remain todo. Earlier inventory is historical; no tool installation, editor launch, export, transfer or gameplay implementation was performed.

Official Linux .NET archive, C# desktop/export documentation and Microsoft's WSLg guidance checked on this date; see SETUP.md and D012. The direct Microsoft Linux SDK thank-you URL and an attempted C# platform-specific documentation URL were unavailable in the browsing tool; setup links to the main SDK download page instead.

Manual check for this documentation change: read SETUP.md and D012. Expect Linux tools and WSL Bash instructions, source/pipeline remaining in WSL, and only exported runtime files copied to Windows. Actual WSLg/editor and Windows export/run checks remain P02 acceptance work.

Documentation validation passed: `git diff --check` exited 0; `rg` found no remaining active Windows-development/optional-WSL setup instructions; in-session assertions checked all 28 task statuses unchanged and verified P02/P06 execution prompts match the revised plan. Setup and D012 were reviewed for WSL development and runtime-only transfer; no implementation acceptance criterion was marked complete.

## 2026-10-03 — .NET 10 setup update

User approved .NET 10. Updated global.json to 10.0.112, setup, README, plan, decisions (D013), project state and affected task cards/index. Historical .NET 8 SDK evidence above remains a record of the earlier selection, superseded by D013. P00/P01 remain done; P02–P27 remain todo. Pipeline/tests planned net10.0; Godot-generated game target and runtime/export compatibility await P02.

Actual checks: `dotnet --info` reconfirmed installed Linux SDK 10.0.112 and runtime 10.0.12; after the pin change, `dotnet --version` returned 10.0.112 and `dotnet --list-sdks` listed 10.0.112 at /usr/lib/dotnet/sdk. `git diff --check` passed. Official Godot 4.5 C# and Microsoft SDK/version-selection/global.json/support pages rechecked; see SETUP.md and D013. No tool installation, project creation, restore, build, export or editor launch performed.

Manual check: run `dotnet --version` from the WSL repository root and read SETUP.md; expect 10.0.112, no .NET 8 SDK download requirement, and separate Godot runtime/export verification in P02. User execution: not run. Game compatibility and visual checks remain unverified; G0 remains open.

## P02 — minimal scene build, export and native startup

Checked 2026-10-03 in the WSL2 repository. Selected task only; no vehicle/map/pipeline added. `GODOT4` below denotes `/home/hekku2/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64`; Godot commands used `DOTNET_ROOT=/usr/lib/dotnet`. Tools were already installed when inspected; P02 did not download or install Godot.

| Actual command / check | Outcome |
|---|---|
| `git status --short`; repository/task/plan/state/decisions reads | Clean starting tracked tree; existing Git-ignored local setup helpers preserved |
| `dotnet --info`; `uname -a`; display variables; installed editor/template inventory | Linux SDK 10.0.112/runtime 10.0.12, WSL2 x64; DISPLAY=:0/WAYLAND_DISPLAY=wayland-0; editor/templates now present |
| `"$GODOT4" --headless --version` | `4.5.2.stable.mono.official.6ce3de25a` |
| Initial sandbox `dotnet new sln --name DrivingTime --format sln` | Failed: read-only user first-use sentinel; rerun with escalation succeeded |
| `dotnet new sln --name DrivingTime --format sln`; `dotnet sln DrivingTime.sln add game/DrivingTime.csproj`; `dotnet build DrivingTime.sln` (escalated) | Root solution created; restore/build passed, 0 warnings/errors, Debug assembly in game/.godot/mono/temp/bin/Debug |
| Initial sandbox `"$GODOT4" --headless --path game --editor --import` | Exit 0 with denied user cache/data/config writes and editor socket errors; not a valid successful import check |
| Same import command, escalated retry | Exit 0, completed initialization/editor load without reported errors |
| `"$GODOT4" --headless --path game --quit-after 10` (escalated) | Exit 0; `P02: main 3D scene ready (C#).`, no startup errors |
| First `"$GODOT4" --headless --path game --export-release 'Windows Desktop' ../artifacts/windows/DrivingTime.exe` (escalated) | Exit 0 but .NET export errors: game/DrivingTime.sln absent. Incomplete EXE/PCK were not treated as success or transferred |
| Same export after adding project/solution_directory=".." | Exit 0; .NET publish and packing completed without reported warnings/errors |
| Export inventory / runtimeconfig | 188 files, about 169 MiB: 96,723,968-byte DrivingTime.exe, 5,776-byte DrivingTime.pck and 186 files in data_DrivingTime_windows_x86_64, including DrivingTime.dll, GodotSharp.dll, hostfxr.dll, hostpolicy.dll, coreclr.dll and framework DLLs. Runtimeconfig: net8.0, included Microsoft.NETCore.App 8.0.31 |
| `"$GODOT4" --path game --editor --rendering-method gl_compatibility --quit-after 120` (escalated) | Editor launched through WSLg, exited 0. OpenGL 4.5 Mesa 25.2.8, llvmpipe software renderer; V-Sync unsupported warning, absent libasound/libpulse, dummy-audio fallback |
| `mkdir -p /mnt/c/TEMP/driving-time-P02`; `cp -a artifacts/windows/. /mnt/c/TEMP/driving-time-P02/` (escalated) | Complete exported runtime copied; no source/editor/SDK copied |
| `/mnt/c/TEMP/driving-time-P02/DrivingTime.exe --headless --quit-after 10 --log-file C:/TEMP/driving-time-P02/startup.log` (escalated) | Native Windows exit 0, startup marker, no startup errors; log read back |
| `/mnt/c/TEMP/driving-time-P02/DrivingTime.exe --quit-after 120 --log-file C:/TEMP/driving-time-P02/graphical-startup.log` (escalated) | Native Windows graphical startup, exit 0; OpenGL 3.3.0 NVIDIA 591.86, NVIDIA GeForce RTX 3060, C# marker; log read back, no startup errors |
| `"$GODOT4" --path game --audio-driver Dummy --quit-after 60` (escalated) | WSLg graphical game exit 0, C# marker, llvmpipe and V-Sync warning; audio warning avoided for this launch |

The solution initially generated generic Release mappings to Debug; it was reduced to Any CPU with Debug/ExportRelease matching installed Godot.NET.Sdk configurations. Final build/configuration and whitespace results are recorded below. No artificial unit tests added for static scene declarations; useful evidence is real build/import/runtime/export plus manual appearance checks.

Installed archive provenance evidence: existing cache download-record.txt says retrieved `2026-10-03T11:53:18Z` from `https://github.com/godotengine/godot-builds/releases/download/4.5.2-stable`. SHA256SUMS.local identifies editor ZIP `9197371d3c4ceebb3ce750f8f4274a1b6f3eb75f25f17eb94fea9c1746081154` and .NET templates TPZ `ed196ee69204a829176129878e48916fc30370941340558ba38db8210ff92dcb`. These are inspected local download records, not a fresh download or comparison against provider checksums. No map datasets or source assets retrieved; scene primitives are repository-authored and have no CRS/vertical datum.

Sources checked 2026-10-03: [Godot 4.5 C# basics](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html), [CLI tutorial](https://docs.godotengine.org/en/4.5/tutorials/editor/command_line_tutorial.html), [Windows export](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_windows.html) and [solution directory property](https://docs.godotengine.org/en/4.5/classes/class_projectsettings.html#class-projectsettings-property-dotnet-project-solution-directory). GitHub/tagged raw project-generator fetch unavailable through browsing; csproj manually authored against documented baseline, not claimed editor-generated. Installed SDK props and runtimeconfig inspected directly.

Manual evidence: **unverified**. No floor/cube visual confirmation was performed by the agent or reported by the user. Follow [RUN.md](RUN.md): open/run Main.tscn through the Linux editor, then launch C:\TEMP\driving-time-P02\DrivingTime.exe in Windows. Expected: lit orange cube resting on blue-gray floor, visible from the angled camera, with no startup errors. Record editor and native Windows appearance separately. P02 is awaiting-manual-test; G0 open. No driving/performance test claimed. WSL software rendering and missing audio libraries are known environment limits; Windows graphical startup succeeded without those warnings.

Final verification: `dotnet build DrivingTime.sln --no-restore` and `dotnet build DrivingTime.sln -c ExportRelease --no-restore` both exited 0 with 0 warnings/errors after solution configuration cleanup. `git diff --check` passed; `git check-ignore` confirmed Godot cache, Windows export and export credentials exclusion. New source/guide/solution whitespace checks also passed. No commits, pushes or publication performed. Recommended next task: P03 after P02 manual visual acceptance.

## P03 — Flat driving loop

Checked 2026-10-03 in WSL2, with the same installed SDK/editor/templates as P02. User explicitly selected P03 while P02 visual acceptance remained open. P03 is `awaiting-manual-test`; P02 and gates are not closed by these checks. No tools/packages/datasets installed or downloaded.

`GODOT4` denotes `/home/hekku2/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64`; Godot commands used `DOTNET_ROOT=/usr/lib/dotnet`.

| Actual command/check | Outcome |
|---|---|
| `dotnet --version` | 10.0.112 |
| `dotnet build DrivingTime.sln --no-restore` | Passed, 0 warnings/errors |
| `dotnet build DrivingTime.sln -c ExportRelease --no-restore` | Passed, 0 warnings/errors |
| `"$GODOT4" --headless --path game --editor --import` | Sandbox attempt exited 0 with denied sockets/settings; invalid evidence. Escalated retry exited 0 without errors |
| Initial sandbox integration startup | User log access denied, Godot crashed (signal 11); retried outside sandbox |
| First escalated integration check | Failed at tick 5: collision bottom was 0.2 m above the car origin, causing a small drop. Moved collision center down to 0.35 m; subsequent checks below pass |
| `"$GODOT4" --headless --path game res://Checks/DrivingChecks.tscn --max-fps 30 --disable-vsync` | Passed: 720 physics ticks, 358 frame callbacks |
| Same command, `--max-fps 60` | Passed: 720 physics ticks, 713 frame callbacks |
| Same command, `--max-fps 144` | Passed: 720 physics ticks, 1709 frame callbacks |
| `"$GODOT4" --path game res://Checks/DrivingChecks.tscn --max-fps 30 --disable-vsync --audio-driver Dummy` | Rendered WSLg run passed: 720 physics ticks, 281 frame callbacks |
| Same rendered command, `--max-fps 60` | Passed: 720 physics ticks, 308 frame callbacks |
| `mkdir -p artifacts/windows-p03`; `"$GODOT4" --headless --path game --export-release 'Windows Desktop' ../artifacts/windows-p03/DrivingTime.exe` | Exit 0, .NET publish and packing completed without reported errors |
| `du -sh artifacts/windows-p03`; file inventory | 169 MiB, 188 files; EXE, PCK and data_DrivingTime_windows_x86_64 directory present |
| `git diff --check` | Passed |

The integration scene is C# running inside Godot and uses the playable Main.tscn, real input actions, `_PhysicsProcess` and floor collision. It tests forward/reverse limits, km/h conversion, forward displacement, brake priority, stopping in both directions, reversed steering when backing, both forward steering directions, no turning at rest, opposite pedal braking before reverse, coasting and stable floor height. It exits 1 on failure and prints PASS plus 12 checkpoints on success. It needs no added test framework or runtime; explicitly launch the Checks scene. It does not run in the playable main scene. Scene Inspector tuning overrides must remain at defaults for this check.

All 12 checkpoint traces match to four decimal places across all five passing runs. Selected samples:

| Physics tick | Position X,Y,Z (m) | Signed speed (m/s) | Yaw (rad) |
|---|---|---|---|
| 120 | 14.0000, 0.0009, -4.1000 | 12.0000 | 0.0000 |
| 180 | 14.0000, 0.0009, -10.0000 | 0.0000 | 0.0000 |
| 240 | 15.0866, 0.0009, -7.6918 | -4.0000 | 0.8618 |
| 360 | 15.6091, 0.0009, -7.3356 | 0.0000 | 1.0690 |
| 720 | 7.1697, 0.0009, -15.1232 | 0.0000 | 1.0690 |

Physics is fixed at 60 Hz. Headless comparisons vary frame callback frequency and show deterministic sampled movement for the same physics-timed inputs. Rendered WSLg runs report llvmpipe software OpenGL, plus a warning that the driver cannot change V-Sync. Their frame counts do not establish sustained 30/60 FPS; native Windows rendering-cap/feel comparisons remain unverified. Dummy audio used for graphical checks. No FPS performance claim, visual inspection or human driving test is inferred from these commands.

P03 export has not been transferred or run natively. The P02 Windows folder is unchanged. Manual two-minute appearance/drive and native comparisons: **unverified**; exact controls, tuning, export/copy and test steps are in [RUN.md](RUN.md). No wall impacts, slope tests, recovery, chase camera, tires/suspension or traffic implemented. Synthetic primitives are authored here with meter-scale coordinates; geographic CRS, datum and external data provenance do not apply.

Official Godot 4.5 sources checked 2026-10-03: [physics processing](https://docs.godotengine.org/en/4.5/tutorials/scripting/idle_and_physics_processing.html) describes fixed-step callbacks; [CharacterBody3D](https://docs.godotengine.org/en/4.5/classes/class_characterbody3d.html) documents velocity and MoveAndSlide integration; [Input](https://docs.godotengine.org/en/4.5/classes/class_input.html) documents action axes and action injection used by the check. These support API use; behavior evidence comes from the actual runs above. Recommended next task: P04 after P03 manual acceptance.

## P02a — Windows export and transfer script

Checked 2026-10-03. User selected a narrow C# helper for the current game, independent of P25's future cached-source build. Existing P03 working changes were preserved. Linux SDK 10.0.112 and Godot 4.5.2 .NET used in WSL2.

| Command / check | Actual result |
|---|---|
| `dotnet run --help` | Installed SDK supports `--file` |
| `dotnet run --file scripts/export-windows.cs -- --help` (sandbox) | Failed before compilation: read-only SDK cache at ~/.local/share/dotnet/runfile |
| Same help command outside sandbox after approval | Exit 0; C# compiled and usage/default destination printed |
| `dotnet run --file scripts/export-windows.cs -- --unknown` (approved) | Exit 1 with actionable argument error; no build/export/copy |
| `dotnet run --file scripts/export-windows.cs` (approved for SDK cache, Godot and Windows destination writes) | Exit 0; Windows destination resolved through wslpath to /mnt/w/projektit/driving-time |
| Script's `dotnet build DrivingTime.sln -c ExportRelease` | Passed, 0 warnings/errors |
| Script's headless editor import | Exit 0, no reported errors |
| Script's Windows Desktop release export | Exit 0; .NET publish and packing completed, no reported errors; required EXE/PCK/game DLL/runtimeconfig/coreclr/hostfxr present and nonempty |
| Runtime copy and SHA-256 verification | All 188 files, 168.1 MiB, copied to W:\projektit\driving-time; every source/destination hash matched |
| `git diff --check` | Passed |

Fresh artifact: `artifacts/windows/20261003-155717-ba7ee883/runtime`. Build/import/export logs remain in its parent, outside the transferred runtime folder. No checkout/editor/SDK files copied. Native Windows startup, appearance and manual drive for this artifact: **unverified**. The historical P03 no-transfer entry above describes its original session and is superseded only for transfer by this evidence. P02/P03 acceptance and G0/G1 remain open.

Manual check: launch `W:\projektit\driving-time\DrivingTime.exe`, expect orange car, square loop and speed HUD. Drive with W/A/S/D, stop with Space, hold S to reverse. Record feedback against artifact `20261003-155717-ba7ee883`. Copy is not atomic, overwrites matching files, preserves unrelated/stale destination files, and retains prior WSL artifacts. Close the game before rerunning; rerun after an interrupted copy. No datasets, public packaging or publishing added. Recommended next task: P04 after P03 manual acceptance.

Official sources checked 2026-10-03: [Godot 4.5 command-line import/export](https://docs.godotengine.org/en/4.5/tutorials/editor/command_line_tutorial.html) for headless import, preset name, absolute export path and existing output directory; [Microsoft .NET file-based apps](https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps) for single-file C# execution with .NET 10. Local execution above verifies compatibility with the pinned installed SDK. No extra project or language runtime added.
