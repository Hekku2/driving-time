# Testing and evidence

## P05a — VehicleBody3D comparison

Checked 2026-10-03 in WSL2 using SDK 10.0.112/Godot 4.5.2 .NET. User reports slight movement jitter and requests VehicleBody3D testing. Original jitter cause remains undiagnosed. P05a is `awaiting-manual-test`; no prior manual acceptance or gate is closed. No dependency/tool/data changes. Synthetic geometry only; no dataset provenance or redistribution introduced.

`GODOT4` below denotes `/home/hekku2/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64`. Installed-tool checks: `command -v dotnet`, `dotnet --version` (10.0.112), editor directory inspection; runtime prints 4.5.2.stable.mono. C# Godot API names compiled successfully against the pinned SDK. Official sources checked on this date: [VehicleBody3D](https://docs.godotengine.org/en/4.5/classes/class_vehiclebody3d.html), [VehicleWheel3D](https://docs.godotengine.org/en/4.5/classes/class_vehiclewheel3d.html) and [RigidBody3D](https://docs.godotengine.org/en/4.5/classes/class_rigidbody3d.html). These document raycast wheels, force/brake/steering inputs, +Z vehicle front, wheel suspension/contact and rigid-body direct-state integration. Godot documents known vehicle realism limitations; no realistic simulation claim.

| Actual command / check | Result |
|---|---|
| `dotnet build DrivingTime.sln --no-restore` | Passed on initial implementation and after tuning/check changes; 0 warnings/errors |
| `"$GODOT4" --headless --path game --editor --import` in sandbox | Exit 0 but socket/IDE permission errors and editor-settings save errors: not counted as successful import |
| Initial sandbox VehicleChecks launch | Failed user-log access, signal 11/exit 134 before checks; retried with approved escalation |
| Import command outside sandbox | Exit 0, no reported errors |
| First VehicleChecks, 60 cap | Brake 35 left 1.471 m/s after one second; check failed. Brake increased to 55; subsequent stopping check passes |
| Second VehicleChecks, 60 cap | Forward/brake/reverse/right turn/impact pass. Ramp check ended at its entrance (Y=0.397 m, Z=5.457 m); increased approach/climb check duration by 40 ticks, without changing ramp/controller |
| `"$GODOT4" --headless --path game res://Checks/VehicleChecks.tscn --max-fps 60 --disable-vsync --quit-after 1400` | Final pass: 1150 physics ticks, five recoveries, exit 0; PASS printed |
| Same VehicleChecks command with `--max-fps 30` | Pass: 1150 ticks, five recoveries, exit 0; identical sampled physical traces |
| `"$GODOT4" --headless --path game res://Checks/DrivingChecks.tscn --max-fps 60 --disable-vsync --quit-after 1000` | Baseline regression passed at 720 ticks/713 frame callbacks; original trace unchanged |
| `"$GODOT4" --headless --path game res://Checks/CameraChecks.tscn --max-fps 60 --disable-vsync --quit-after 600` | Baseline camera regression passed at 420 ticks, exit 0 |
| `"$GODOT4" --headless --path game res://Checks/RecoveryChecks.tscn --max-fps 60 --disable-vsync --quit-after 1800` | Baseline collision/recovery regression passed at 1411 ticks/three recoveries, exit 0 |
| `dotnet run --file scripts/export-windows.cs` outside sandbox | ExportRelease build 0 warnings/errors; import/publish/packing successful; 188 runtime files (168.2 MiB) copied and SHA-256 verified at W:\projektit\driving-time |
| `git diff --check` | Passed |

Final vehicle check exercises actual input, all four physical wheel contacts and settled chassis clearance, nose-first -Z acceleration, Space overriding throttle, reverse, right-turn sign, chassis contact with ImpactBlock, ramp ascent, held R, full reset pose, zero linear/angular velocity and control forces, camera placement behind heading, and natural fall recovery with a -3 m override. Five callbacks comprise manual R, three test placements through the same reset operation, and natural fall. Placement goes through rigid-body integration rather than continuous transform manipulation. Tests are explicit scenes, not invoked by normal gameplay. Test callback indentation only was adjusted afterward; runtime behavior is unchanged.

Identical 30/60-cap sampled results: forward signed speed 9.986 m/s; brake 0.054 m/s; reverse -3.801 m/s; right yaw -0.915 rad; ramp position (-14,1.285,0.409); final settled position (14,0.306,8.000). Caps are not measurements of achieved native frame rate or visual smoothness. No rendered/manual run or Windows startup performed for P05a.

Current artifact/logs: `artifacts/windows/20261003-184950-a7195a8f` (`build.log`, `import.log`, `export.log`, `runtime/`). Export logs confirm both Main and VehicleExperiment scenes packed. C# vehicle defaults and scene match the tested implementation; original arcade controller remains unchanged. W: now launches the vehicle experiment by default. Manual same-route comparison, including straight/turn/ramp jitter and controller preference: [RUN.md](RUN.md#p05a-manual-comparison). Both scenes identify their body type in the HUD. Tuning, slope drift, tipping, appearance, reset settling and camera/rendering smoothness need native playtesting; P05a and G1 remain open.

## P05 — Collision and safe recovery

Checked 2026-10-03 in WSL2 with pinned SDK 10.0.112 and installed Godot 4.5.2 .NET. P05 selected explicitly; prior manual evidence/statuses were not closed. Existing local P03/P04 acceptance-checkbox edits preserved. No dependencies, tools or datasets added. Status: `awaiting-manual-test`; G1 remains open.

`GODOT4` denotes `$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64`; every Godot command below uses `DOTNET_ROOT=/usr/lib/dotnet`. Godot checks ran outside the sandbox after approval because its normal user-log directory is outside the writable roots.

| Actual command/check | Outcome |
|---|---|
| `dotnet --version`; tool directory inventory | SDK 10.0.112; existing portable Godot 4.5.2 .NET installation present |
| `dotnet build DrivingTime.sln --no-restore` | Passed after implementation and final check edit, 0 warnings/errors |
| `dotnet build DrivingTime.sln -c ExportRelease --no-restore` | Passed, 0 warnings/errors; export helper later rebuilt release successfully |
| Initial sandbox RecoveryChecks startup | User-log write denied; Godot crashed with signal 11 before tests. Escalated retry ran normally |
| Initial escalated RecoveryChecks | Failed reverse-escape criterion: actual impact speed 0, but SignedSpeed remained 12 m/s. Corrected wall-contact speed reconciliation in Car.cs |
| Intermediate checks | Passed impacts/climb; test used exact zero at the brake boundary and then released/repressed R instead of holding it. Corrected floating-point tolerance and input injection, without further movement changes |
| `"$GODOT4" --headless --path game --editor --import` | Passed, exit 0, C# resources registered without reported errors |
| `"$GODOT4" --headless --path game res://Checks/RecoveryChecks.tscn --max-fps 60 --disable-vsync --quit-after 1800` | Passed at 1411 physics ticks, 3 recoveries; PASS printed, exit 0 |
| Same RecoveryChecks command at `--max-fps 30` | Final check version passed at 1411 ticks, 3 recoveries; PASS printed, exit 0 |
| `"$GODOT4" --headless --path game res://Checks/DrivingChecks.tscn --max-fps 60 --disable-vsync --quit-after 1000` | P03 regression passed at 720 ticks / 713 frame callbacks; all 12 original checkpoints unchanged |
| `"$GODOT4" --headless --path game res://Checks/CameraChecks.tscn --max-fps 60 --disable-vsync --quit-after 600` | P04 regression passed at 420 ticks, exit 0 |
| `dotnet run --file scripts/export-windows.cs` | ExportRelease build 0 warnings/errors; headless import and Windows publish/packing passed; 188 runtime files (168.2 MiB) copied to W:\projektit\driving-time and SHA-256 verified |
| `git diff --check` | Passed |

Artifact/logs: `artifacts/windows/20261003-164321-f5563ae3`. Gameplay matches the tested source; the final RecoveryChecks failure-exit guard was added afterward and passed at 30 cap. Tests are explicit scenes, never part of the playable Main scene. Native Windows startup/manual drive, appearance, camera interpolation quality and human driving feel: **unverified**. Manual steps and expected results: [RUN.md](RUN.md#p05-manual-test). No rendered run or performance baseline claimed this session.

RecoveryChecks exercises actual Main.tscn, physical solids and real input actions. Each tick independently queries a slightly inset chassis box for overlap with solid interiors. It verifies blocking by all three obstacles, heading speed removed on impact, reversing away, climbing/braking/holding/descending the 10-degree ramp, R while throttle/steering are held, held R versus a second press, and a natural fall off the bounded ground. That fall overrides RecoveryHeight to -3 m to prove the threshold is configurable. Recovery callbacks inspect the full spawn transform, zero horizontal/vertical/controller speed, and camera transform matching initial spawn even after wall compression and a yaw change. A failure sets exit 1; frame caps are not measured performance or manual acceptance.

Collision model: grounded CharacterBody3D, explicit gravity, MoveAndSlide translation and default 0.1 m floor snap/45-degree maximum floor angle. Its 1.8 × 0.7 × 3.4 m upright chassis box is sufficient for these scripted impacts and 10-degree slope. Cabin/wheels are cosmetic; no suspension, pitching, tipping or physical bounce. Rotation is a direct yaw change, not an angular sweep; sharp corner turning and close-range camera occlusion need manual coverage. Spawn at (14,0,8), yaw 0, must remain clear and above RecoveryHeight (default -10 m). Reset clears motion on that tick; held driving input can accelerate on the next tick. Ramp high end is a drop. No focused controller child is required by the passing modest checks; the verified narrow defect was corrected without a redesign.

Synthetic provenance: repository-authored boxes in meters; no external data, download date/checksum, geographic CRS or vertical datum applies. Sources checked 2026-10-03: [Godot 4.5 CharacterBody3D](https://docs.godotengine.org/en/4.5/classes/class_characterbody3d.html) for MoveAndSlide velocity modification, grounded walls/floors, floor snap and slope stop; [Godot 4.5 Node](https://docs.godotengine.org/en/4.5/classes/class_node.html#class-node-method-reset-physics-interpolation) for resetting interpolation after teleportation. Behavior claims are backed by the runs above. Recommended next task: P06, independently ready while manual P05 feedback is pending.

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
## P06 — Offline synthetic manifest CLI

Date: 2026-10-03. Environment: Ubuntu 24.04 WSL2, Linux SDK 10.0.112/runtime 10.0.12 as reported by `dotnet --info`; root global.json unchanged. Existing staged P05a changes were preserved. No game export, native Windows launch or geographic download performed in P06.

| Actual command/check | Outcome |
|---|---|
| `dotnet restore tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj` | Restore completed from available packages but sandbox reported NU1900 vulnerability-service network failure; not accepted as final restore evidence |
| `dotnet restore tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --locked-mode --source https://api.nuget.org/v3/index.json --force` (approved outside sandbox) | Exit 0, both projects restored, no warnings; exact test references/transitive hashes recorded in lock files |
| `dotnet build pipeline/DrivingTime.Pipeline.csproj --no-restore` | Exit 0, 0 warnings/errors |
| `dotnet test tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --no-restore` | Exit 0, 15 passed, 0 failed/skipped; actual test execution, not just project build |
| `dotnet build DrivingTime.sln --no-restore` | Exit 0, all three projects built, 0 warnings/errors |
| `dotnet build DrivingTime.sln -c ExportRelease --no-restore` | Exit 0, game ExportRelease and pipeline/tests Release built, 0 warnings/errors |
| `dotnet run --project pipeline/DrivingTime.Pipeline.csproj --no-restore -- --config config/synthetic-scene.json` twice | Both exit 0; copy first to `/tmp/p06-first.manifest.json`, `cmp` with second exits 0 |
| Same run command with `/tmp/p06-invalid-config.json` (sample copied with tile size 0) | Exit 2; stderr: `Pipeline error: tileSizeMetres must be a finite positive number.` plus documentation/permissions guidance |
| Same run command with `/tmp/p06-missing-config.json` | Exit 2; missing input named, useful guidance, no unhandled exception |

The two CLI runs used no restore and the program has no network APIs or external processes. Offline execution here means the local cached SDK/build/input/output path; OS network namespace isolation was not used. NuGet access was confined to the earlier restore. No deterministic metadata exclusions: manifest bytes include no generation timestamp, machine path or random identifier.

SHA-256 (`sha256sum`): input `config/synthetic-scene.json`: `8f35618a0b9db8bc2e57caf284b4dc0a9b99fa86e00e0088f088d2d1d1c49ca5`; output `config/generated/synthetic-scene.manifest.json`: `e9cc6948c388e32f241725a924a2e5a69a49b868b2e6923357e8ba253dbdba6f`.

Targeted tests exercise missing/malformed/null/unknown/duplicate JSON fields, schema/coordinate rejection, region inversion/zero extent/overflow, origin outside bounds and on an accepted boundary, nonfinite numbers, invalid tile size, escaping/absolute output paths, culture-independent JSON, stable ordinal IDs, expected ground dimensions, repeat CLI byte identity, config-relative output location and preserving previous output when validation fails. Missing paths/invalid arguments are covered through the CLI entry point.

Manual/review: agent followed the documented initial-restore/build/test/run steps and inspected the output version, two primitive descriptions, input checksum and synthetic provenance. User replay not performed. No visual check is required for P06. Short optional review: [PIPELINE.md](PIPELINE.md#short-review), run twice and expect identical bytes; copy config and set tile size 0, expect exit 2 and no newly created output.

Limitations: synthetic source descriptions only, no chosen Helsinki CRS/origin/datum, engine transforms, GLB, tile lifecycle or game staging. Valid output writes overwrite and are not atomic. P07/P08 remain todo; P06 alone does not pass G2. Package/API sources and checked date are recorded in [PIPELINE.md](PIPELINE.md); this task changes no existing game restore policy.

## P07 — Coordinate contract and transform evaluation

Executed 2026-10-03 in WSL with pinned SDK 10.0.112/runtime 10.0.12. Existing staged/unrelated work preserved. No editor, game export, manual drive or third-party dataset retrieval performed. [Contract, fixtures, sources and limits](COORDINATES.md).

```bash
dotnet --info
dotnet restore tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj
dotnet restore tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --locked-mode
dotnet build DrivingTime.sln --no-restore
dotnet test tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --no-restore --logger 'console;verbosity=normal'
dotnet test tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --no-build --no-restore --filter FullyQualifiedName~ProjNetAgainstPublishedNlsExample10 --logger 'console;verbosity=detailed'
```

Restore and locked restore: pass, ProjNET 2.1.0 with content hashes in both lock files. Root Debug build: pass, 0 warnings/errors. Initial sandbox test run built projects but aborted on local VSTest socket creation (SocketException 13); escalated same test command passed all 31 cases (16 new P07 cases, 15 P06 regressions). Targeted detailed replay printed E=214141.42267341953, N=7016196.144977237; residual E=-0.000026580470148473978 m, N=-0.00002276245504617691 m against published NLS example 10. Tolerances: 0.01 m reference projection; 1e-7 degrees inverse/projection round trip; 1e-9 m local horizontal round trip. Residuals below reference rounding are numerical evidence, not real-world survey accuracy.

Coverage: four cardinal directions; +37.25 m elevation above nonzero synthetic height origin; independent 60/80/100 m synthetic segment; right-handed east-cross-north basis; millimetre preservation after origin subtraction; explicit E,N / N,E and lat,lon / lon,lat adapters; datum mismatch/nonfinite rejection; exact/negative tile edges and neighbor recomposition; GRS80 TM35FIN forward/inverse reference; central-meridian easting; geographic round trips around the Finland envelope; swapped axes rejected; WGS84 approximation requires opt-in.

Manual review performed by agent: axis/units table, fixed origin requirement, vertical validation process, approximation limitations and no invented real origin. User replay not performed; no visual acceptance is required for this task. Optional review: read COORDINATES.md and rerun the detailed reference command; expect a passing result and north toward engine -Z. Real height controls and WGS84 realization/epoch accuracy remain unverified; P08 must verify geometry import/appearance before G2 passes. Recommended next task: P08.

## P08 — Synthetic GLB import

Session 2026-10-03–04, WSL2 SDK 10.0.112, Godot 4.5.2 .NET. Official sources/checked dates and format rationale: [PIPELINE.md](PIPELINE.md#p08-synthetic-glb-staging), D018. Manual editor/Windows appearance: **unverified**.

| Actual command / check | Outcome |
|---|---|
| `dotnet restore DrivingTime.sln`, then `dotnet restore DrivingTime.sln --locked-mode` | Both pass; SharpGLTF.Toolkit/Core/Runtime 1.0.7 locked; installed nuspec MIT/net10.0 verified |
| `dotnet build DrivingTime.sln --no-restore` | Sandbox build returned failure without diagnostics; approved retry passed with 0 warnings/errors |
| `dotnet test tests/DrivingTime.Pipeline.Tests --no-build --no-restore` outside sandbox | 33 passed: 31 prior plus two GLB/manifest geometry and nonzero-origin tests |
| `dotnet run --project pipeline --no-build --no-restore -- --config config/synthetic-scene.json --stage-fixture game/Generated/P08` | Exit 0, 6,664-byte GLB and manifest; repeat bytes identical; bounds (-15,-0.2,-15)–(15,10,15) m |
| Godot `--headless --path game --editor --import` outside sandbox | Exit 0; synthetic GLB imported without errors; default root scale=1, no import script |
| Godot `--headless --path game res://AssetInspection.tscn --quit-after 120 -- --check-fixture` | First run failed material comparison; checked tagged importer linear-to-sRGB handling and corrected expected colors. After `dotnet build game --no-restore`, final run exit 0, `P08 PASS` |
| `dotnet run --file scripts/export-windows.cs` | Exit 0; ExportRelease build 0 warnings/errors; fixture regeneration, import, publish/packing pass; manifest, imported mesh and inspection scene visibly listed in export log |
| Native PowerShell `& "W:\projektit\driving-time\DrivingTime.exe" --headless res://AssetInspection.tscn --quit-after 120 --log-file "W:\projektit\driving-time\p08-check.log" -- --check-fixture; exit $LASTEXITCODE` | Exit 0, `P08 PASS` from native exported package |

Artifact/logs: `artifacts/windows/20261003-204544-dc05f013` (UTC artifact timestamp); 188 runtime files, 168.2 MiB transferred and SHA-256 verified. Windows headless log is `W:\projektit\driving-time\p08-check.log`. Synthetic GLB SHA-256: `f64be72dacbed42167efdb13fb3e4ef59f3b2867b32ce296c4c04504db6f75e7`; input configuration SHA-256: `8f35618a0b9db8bc2e57caf284b4dc0a9b99fa86e00e0088f088d2d1d1c49ca5`.

GLB readback verifies five meshes, 60 triangles, each object bound, identity nodes, no images, single-sided colored material, CCW triangles and outward flat unit normals. Nonzero (1000,2000,40) source origin produces identical localized bytes; too-small region rejected. Manifest path/hash/units/version/origin/bounds checks pass. Independent Godot inspection verifies all five imported bounds, identity global transforms, metallic/roughness/sRGB factors, outward unit normals and CW indices. Same check in the native Windows export proves resource remapping and manifest inclusion. No human visual result or performance measurement inferred. [Manual steps](RUN.md#p08-manual-inspection); P08 remains awaiting-manual-test and G2 open.

## P09 — bounded source research

Date: 2026-10-04, Europe/Helsinki. Research task done; no conversion or game change. Official sources/checked dates, exact source-request URLs, measured bounds/origin, licence limits and downstream fallbacks are in [DATA_SOURCES.md](DATA_SOURCES.md) and [source-registry.json](../config/source-registry.json). Successful raw responses are ignored in `artifacts/p09`; retained locally, not redistributed.

| Actual command / request | Outcome |
|---|---|
| `dotnet --version`, `command -v curl jq dotnet`, existing pipeline/tool inspection | SDK 10.0.112, curl and dotnet available; jq absent; existing ProjNET 2.1.0 available; no install |
| `curl --fail --location --max-time 30 --max-filesize 3000000 -o /tmp/p09-wfs.xml '<WFS GetCapabilities URL>'` in sandbox | Exit 6, DNS resolution failed; approved escalated retry exit 0 |
| Same bounded curl form against city WFS/WCS/CityGML capabilities and DescribeCoverage, exact URLs in registry | Exit 0; 305680 / 641372 / 11136 / 3333 bytes respectively; live terrain coverage is 1m, not catalogue 50cm |
| City address WFS, street filter count=20; same reference feature count=1 in EPSG:3067 and EPSG:4326 | Exit 0; 20 of 23 addresses returned; reference `Helsinki_osoiteluettelo.45899`; native GK25 and projected/geographic coordinates agree by feature ID/property values; no local transform claim |
| CityGML `bldg:Building`, count=1 in 200m GK25 box | N,E request returned zero (829-byte empty collection); corrected physical E,N request exit 0, 20 matched / 1 returned, 26638 bytes; one LoD1/2 building with compound GK25/N2000 CRS |
| WCS GetCoverage, 4m square, image/tiff, compression=NONE | Swapped physical axes returned HTTP 404 / curl exit 22. Corrected E,N request exit 0, 1429-byte GeoTIFF; 4x4 valid Float32 samples, EPSG:3879, pixel-is-area, 1m steps, matrix georeferencing, uncompressed tiled layout |
| Footprint WFS count=2 in 200m EPSG:3067 box | Exit 0, 1907 bytes; two 2D polygons / 21 matched; no actual heights |
| Overpass GET, street name in bbox (60.18,24.90,60.20,24.93), timeout=20/maxsize=16777216 | Main endpoint HTTP 406 / curl exit 22; ASCII-name regex variant same result; alternate kumi endpoint exit 28 after 30s, zero bytes; no OSM payload or road measurements |
| Helsinki 3D HTML, public start script; exploratory `config.json` path | HTML/script loaded (2631/158 bytes); guessed config path HTTP 404. Graphical viewer/export not run |
| Street-line DescribeFeatureType and count=5 street-name GetFeature | Exit 0; schema 3905 bytes; returned 1211-byte bicycle-rack feature, not centreline; reject layer as road-graph fallback |
| `dotnet run --file scripts/p09-inspect.cs -- artifacts/p09` in sandbox | Build-cache path outside workspace denied; retried with approved escalation |
| Same offline .NET command, final version | Exit 0; strict TIFF readback, building dates, measured origin/height, 1000m bounds, JSON and all ten registered payload byte counts/SHA-256 pass. Initial inspector revisions exposed TIFF Rational tags, tiling and matrix layout; final supports the observed tiny probe only, not a generic reader |
| `git diff --check` | Exit 0 |

The exact successful probe URLs can be replayed with the recorded bounded curl command; each response hash is in the registry. Service timestamps/content can change, so keep replay snapshots separate. GeoServer response UTC timestamps fell on 2026-10-03 evening; local retrieval date was 2026-10-04. CityGML timestamps omit timezone and must not be assumed UTC. HTTP response ETag/Last-Modified headers were not captured; future retrieval cache must record them where supplied.

Measured origin: EPSG:3067 E=384236.0361374, N=6674210.89320437; native GK25 E=25495142, N=6675054. Containing DEM cell row=2,col=2 gives H=13.52536678314209m, provider-declared N2000; Float32 value expanded to double. Independent vertical datum/control is not verified. Terrain range about 13.410841–13.583823m; no NoData in this tiny probe. Full route/area NoData and building completeness unverified.

Manual check (not performed): open the city map at lon=24.91244103, lat=60.18828858 and inspect the eastern Stenbäckinkatu interior run. Expected: a short surface street segment within the recorded square, with endpoints away from the named junction/tram/hospital structures; compare construction/access with the official 2026 street plan. P10 must obtain the centreline and validate actual conditions before accepting the route. P17 must check independent heights. No manual game drive, graphical download, export, performance run or earlier gate acceptance was performed in P09.

Recommended next task: P10. Missing OSM access has a manual Overpass export fallback or named P10 acquisition blocker; P14 must resolve footprint fallback terms; P17 must validate a managed TIFF reader and defective band-unit/axis metadata. These do not close or implement later tasks.

## P10 — bounded road retrieval and offline inspection

Date: **2026-10-04**, WSL2, pinned SDK10.0.112/net10.0. No new dependencies, game changes, export or manual drive.

Commands and actual outcomes:

- `dotnet --info`: Linux SDK10.0.112/runtime10.0.12 available.
- `dotnet build pipeline/DrivingTime.Pipeline.csproj --no-restore`: passed, 0 warnings/errors.
- `dotnet run --project pipeline --no-build -- road-sample --config config/road-sample.json`: sandbox first returned socket permission error (CLI exit2). Approved network rerun returned HTTP200 on **one POST**, no retries/mirrors; validated/cache write exit0. Retrieval UTC `2026-10-04T06:08:03.6563316+00:00`; OSM base `2026-10-04T06:06:51Z`; generator `Overpass API 0.7.62.11 87bfad18`. Element update timestamps span 2024-07-07..2026-09-15, not survey dates.
- `dotnet run --project pipeline --no-restore -- road-sample --config config/road-sample.json`: final code default **verified offline cache** exit0 inside network-restricted sandbox. Counts/bounds unchanged; no network needed.
- `dotnet run --project pipeline --no-build -- road-sample --config config/road-sample.json --input data/raw/osm-p10/df20f1363a2a44336af47c5f7692cc97d60afa2b6dde0cf9b98618ace9ccdd6d/41616d85bf87502df9322df0b18cd74341fb116c814a38a6009723b14ab24142/raw.json`: real cached local-file import exit0, same counts; separate import metadata, network retrieval metadata retained.
- Same command with `--input tests/fixtures/osm-road-sample.json`: exit0, 2 ways/3 unique nodes/4 vertices. Authored synthetic fixture, no real data redistribution.
- `sha256sum` on real raw cache after import: `41616d85bf87502df9322df0b18cd74341fb116c814a38a6009723b14ab24142`, **11930 bytes**. Request SHA256 `df20f1363a2a44336af47c5f7692cc97d60afa2b6dde0cf9b98618ace9ccdd6d`.
- `dotnet test tests/DrivingTime.Pipeline.Tests --no-restore`: initial sandbox test communication socket denied; approved final run **50 passed, 0 failed/skipped** (33 existing +17 P10 cases). Raw tags/missing values, measured bounds, shared IDs, truncation/HTML/missing metadata/geometry, duplicate IDs/fields, bad coordinates, node-coordinate conflicts, region cap, local raw-byte preservation, import retrieval-time unknown, cache replay/tamper detection and streamed byte cap covered.
- `dotnet build DrivingTime.sln --no-restore`: sandbox parallel build returned exit1 without diagnostics. Approved `dotnet build DrivingTime.sln --no-restore -m:1` passed, **0 warnings/errors**. No source change was needed for the build.
- `git check-ignore data/raw/osm-p10/request.lock`: ignored. `git diff --check`: passed.

Exact query (recorded in provenance alongside actual POST form body/status/response headers):

```text
[out:json][timeout:20][maxsize:16777216];way["highway"]["name"="Stenbäckinkatu"](60.18694260169827,24.912355544171,60.189747841761815,24.91973380573806);out meta geom;
```

Projected input rectangle is E384236.0361374..384636.0361374, N6674060.89320437..6674360.89320437, 400×300m inside the original 1km square. Four inverse-projected corners derive the request envelope with P07's approximate datum policy. Actual complete-way geographic bounds: **lat60.1881866..60.189673, lon24.9122881..24.9162629**. There are **11 ways, 26 unique nodes, 36 geometry vertices**, **1 vertex outside request envelope**. Full-way overflow is retained/reported, not hidden or clipped.

| Tag | Present / missing ways | Exact values/counts |
|---|---|---|
| highway | 11 / 0 | residential:11 |
| oneway | 0 / 11 | none supplied |
| lanes | 2 / 9 | `2`:2 |
| maxspeed | 10 / 1 | `30`:7, `40`:3 |
| layer | 0 / 11 | none supplied |
| bridge | 0 / 11 | none supplied |
| tunnel | 0 / 11 | none supplied |
| access, motor_vehicle, construction | each 0 / 11 | none supplied |

Way79499715 has `maxspeed:forward=30` and `maxspeed:backward=40` but no base maxspeed. Full tags and original strings are retained; no inferred defaults/direction/ground level. Raw data CRS EPSG:4326, lat/lon degrees; no elevation or vertical datum. © OpenStreetMap contributors, ODbL1.0; source licence link/check date recorded in PIPELINE.md; no distribution review or publication.

P10 is **done**: its acceptance is CLI/parser evidence, without a visual criterion. Optional manual replay: run the default command again; expect "Verified offline cache", 11 ways/26 unique nodes and the same actual bounds. User replay not performed. Construction/access/physical reference, route endpoints and 100–150m ground-level segment suitability remain unverified downstream; no graph, generated roads or gate closure. Next task **P11**. Official documentation links and checked date are in [PIPELINE.md](PIPELINE.md#p10-bounded-osm-road-sample).

## P11 — offline road graph

Date: 2026-10-04. WSL Linux SDK 10.0.112, existing net10.0 pipeline/tests and ProjNET 2.1.0. No dependency installation/retrieval/game/editor/export or manual drive in P11.

Commands and outcomes:

```bash
dotnet --info
dotnet build pipeline/DrivingTime.Pipeline.csproj --no-restore
dotnet test tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --no-restore
dotnet build DrivingTime.sln --no-restore
dotnet run --project pipeline --no-restore -- road-graph --config config/road-graph.json
cp data/intermediate/p11/road-graph.json /tmp/p11-first-graph.json
cp data/intermediate/p11/road-graph.svg /tmp/p11-first-graph.svg
dotnet run --project pipeline --no-restore -- road-graph --config config/road-graph.json
cmp /tmp/p11-first-graph.json data/intermediate/p11/road-graph.json
cmp /tmp/p11-first-graph.svg data/intermediate/p11/road-graph.svg
sha256sum data/intermediate/p11/road-graph.json data/intermediate/p11/road-graph.svg
dotnet run --project pipeline --no-restore -- --help
git check-ignore data/intermediate/p11/road-graph.json data/intermediate/p11/road-graph.svg
git diff --check
```

Pipeline build passes, zero warnings/errors. First fixture compile exposed the existing object-initializer Bounds contract; corrected. Sandbox VSTest aborted on local socket permissions; approved retry outside sandbox ran tests. An initial preservation assertion incorrectly compared JSON whitespace rather than contents; changed to structural deep equality. Final test run passes all 65 tests (15 P11 cases, 50 earlier regressions), no skipped tests. Root sandbox build failed without diagnostics; approved retry passes full Debug solution, zero warnings/errors. No unsuccessful attempt is counted as a pass.

P11 cases cover original/intermediate geometry/IDs/attributes/projected scale, two-way arcs, forward and reversed one-way, implied direction/explicit override, four-arm shared junction splitting/connectivity, grade-separated geometric crossing, coincident distinct IDs, outside-to-outside clipping with nonmerged synthetic cuts, original shared node exactly on boundary (identity retained, junction false), exit/reentry disconnection, unsupported/reversible/conditional directions, bridge/tunnel/grade preservation and transition warning, datum opt-in/invalid bounds, repeated/zero edges, structurally complete source preservation, deterministic offline output, valid SVG XML and checksum-tampering rejection. Tests author tiny C# fixtures using the already independently evaluated P07 projector; they are not real OSM exports.

Real offline output: 11 complete source ways preserved, 12 graph endpoint nodes, 11 clipped segments, 22 directed arcs, one undirected component, zero isolated segments, twelve diagnostics. Eleven `assumed-two-way` reports (missing oneway on all source ways), one incomplete sample-context report. The original name-filtered context supplies no neighboring street connectivity. One synthetic cut `cut/78393519/0.6057237965289157` is at E=384236.0361374, N=6674209.062150154, the western clip edge; OSM ID null, boundary=true, junction=false. Original outside geometry remains in sourceWays/sourceDocument. Real sample contains no grade tags; grade separation is proved by authored fixtures, not real elevation evidence.

Repeated JSON/SVG comparisons exit 0; help and ignore checks pass. SHA-256:

- Raw source unchanged: `41616d85bf87502df9322df0b18cd74341fb116c814a38a6009723b14ab24142` (11930 bytes).
- Graph JSON: `bef76ac1d78167a8f680c5e245b511adf3e327c8313ff6e3b910c96393d84d46`.
- Debug SVG: `5d07601fdb5ef7557ed5001e212aece7c766b1e0a6cc80d3fba5d59c0bc4e006`.

Provenance uses P10's network sidecar: retrieval 2026-10-04T06:08:03.6563316+00:00, OSM base 2026-10-04T06:06:51Z, ODbL source declaration/OSM attribution retained. EPSG:4326 raw input is projected to EPSG:3067 horizontal metres with explicit P07 WGS84/EUREF-FIN approximation. No vertical datum/heights introduced. Official OSM sources and failed layer/tunnel fetches are recorded in [PIPELINE.md](PIPELINE.md#p11-offline-intermediate-road-graph).

Optional manual check (not run): open `data/intermediate/p11/road-graph.svg` in a browser; expect a north-up/east-right labeled bounded street chain, base-direction arrows, shared-node markers and one orange western boundary endpoint. Hover segment tags and inspect JSON diagnostics. This intermediate-data task requires an inspectable artifact rather than a manual drive/appearance gate; P11 is done, earlier visual gates remain open. Next P12.

## P12 — single flat road strip

Date: 2026-10-04. SDK `dotnet --version`: 10.0.112. Godot reports 4.5.2.stable.mono.official.6ce3de25a. No dependencies installed/downloads attempted for generation.

Commands and final outcomes:

```sh
dotnet test tests/DrivingTime.Pipeline.Tests --no-restore
dotnet build DrivingTime.sln --no-restore
dotnet run --project pipeline --no-build -- road-strip --config config/road-strip.json
"$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64" --headless --path game --editor --import
"$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64" --headless --path game res://Checks/RoadAssetChecks.tscn
```

- All 78 tests pass (13 P12 cases, 65 regressions). New cases check independent straight bounds/width, nonzero origin, duplicate/near-zero vertices, ordinary/short/extreme capped bends, supported/invalid width tags, region/settings/grade/reversal rejection, serialized finite upward nondegenerate triangles/bounds/identity, identical collision geometry and deterministic offline CLI/hash manifest with failure preserving output. Initial sharp/short bend cases exposed the fixed-diagonal assumption; corrected triangulation chooses the interior diagonal for concave capped quads.
- Full Debug solution build passes, zero warnings/errors. Initial sandbox test runner/local socket denial and empty-error sandbox build failed; approved retries passed. Sandbox Godot import returned 0 but logged socket/settings errors; approved retry cleanly passed. These failures are not counted as successful checks.
- Real cached P11 conversion: one 100 m interior section of OSM 310376967, eight retained centreline points, width tag 8.8 m (`source:width=ARCore`), no removed points/capped joins, 14 triangles each. Configured width/height/limited-context assumptions printed and stored. No other roads or markings generated.
- Copied all three outputs to `/tmp/p12-{road.glb,collision.glb,manifest.json}`, replayed the exact CLI, and ran `cmp` for each: all exit 0. GLB lengths 1516/1524 bytes, manifest 6460 bytes. SHA-256 road `a1e43fcc0cf77aa64d25e7e30804b4a2d54a6dc3d5043466349c08db19caf10e`; collision `37962c7151aaeed482edc6db1663ac8aa4bc609dd2609411a45ecfdb47bce9ce`; manifest `fd3ee990a27513c76ec557e3b136bd7df599b9e03206e3820fd0603cefb4eb54`.
- Godot C# check passes: one visual mesh, separate non-rendered StaticBody3D/ConcavePolygonShape3D, 14 triangles each, identity transforms, matching manifest metre bounds, clockwise nondegenerate imported visual triangles, downward collision rays at every retained edge midpoint. Initial endpoint rays missed at open-surface edges where double endpoints round relative to float mesh; probes now use edge interiors. Endpoints are not padded, and this is not a drive test.
- A temporary C# run-file inventory measured all eleven cached P11 segment lengths, establishing the selected source length ~115.579 m. First run-file launch was blocked by read-only home cache; `XDG_DATA_HOME=/tmp/p12-dotnet` retry passed with NuGet audit network/compiler-server warnings. The generated run-file bookkeeping is removed from the checkout; no additional tool/runtime is required.

Manual appearance/collision gizmo inspection: **not run**, [steps](RUN.md#p12-manual-road-inspection). No editor graphical inspection, Windows export/native playtest or car scene integration performed. P12 awaits manual testing; prior gates remain open. Sources/checked dates and schema: [PIPELINE.md](PIPELINE.md#p12-single-flat-road-strip).
