# Project state

Goal: Small drivable Helsinki desktop prototype<br>
Current gate: G0 — open; command checks pass, visual acceptance pending<br>
Last completed task: P02a (Windows export/transfer helper)<br>
Selected task: P03 — awaiting-manual-test<br>
Next recommended task: P04, after P03 manual drive acceptance<br>
Updated: 2026-10-03

## Verified environment

- Ubuntu 24.04 WSL2, x86_64, kernel 6.6.87.2-microsoft-standard-WSL2. Linux SDK 10.0.112/runtime 10.0.12; global.json selects that SDK. Git 2.43.0 observed in P01.
- Installed portable Linux editor: `$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64`, reporting `4.5.2.stable.mono.official.6ce3de25a`. Matching templates present at `$HOME/.local/share/godot/export_templates/4.5.2.stable.mono`. P02 discovered existing installations; it did not install them.
- Game: Godot.NET.Sdk/4.5.2, explicitly authored net8.0 project; build/import/runtime and Windows cross-export verified with SDK 10.0.112. Editor plugin runtimeconfig permits LatestMajor roll-forward. Windows export includes .NET 8.0.31. Pipeline/tests remain planned net10.0; no shared library or pipeline exists.
- WSLg editor and graphical game launch passed with llvmpipe software rendering, V-Sync warning, and missing Linux audio libraries (editor dummy-audio fallback). Visual appearance not inspected.
- Native Windows x64 headless and graphical export startup passed. Graphics log reports NVIDIA GeForce RTX 3060, OpenGL 3.3.0, driver 591.86. This is startup evidence, not a performance baseline.
- Fixed requirements: C#/.NET for game/pipeline/tools/tests; development and export in WSL; only exported runtime files go to Windows (D001/D012).

## Current implementation and evidence

- P00/P01: instructions, planning ledger, tool research and SDK selection completed. Historical setup helper checks and inventories remain in [TESTING.md](TESTING.md).
- P02 baseline: single root DrivingTime.sln, game/project.godot, Main.tscn, startup-marker Main.cs, credential-free Windows export preset and [RUN.md](RUN.md). Its original 20 m floor/reference cube scene was exported to Windows; P03 replaces that cube with the car in the working scene.
- `dotnet build DrivingTime.sln`: passed, 0 warnings/errors. Headless import and C# runtime: exit 0, startup marker printed.
- First export left incomplete output because it could not find a game-local solution, despite exit 0. Corrected with `dotnet/project/solution_directory=".."`; subsequent export successfully published and packed the game without reported errors.
- Export: artifacts/windows/DrivingTime.exe, DrivingTime.pck and complete data_DrivingTime_windows_x86_64 directory; 188 files, approximately 169 MiB before test logs. Runtime files copied to `C:\TEMP\driving-time-P02`; source/tools remain in WSL.
- Bounded native Windows headless and graphical runs exited 0 and printed `P02: main 3D scene ready (C#).` without startup errors. Startup logs are in the Windows test folder. Bounded Linux editor and game launch also exited 0.
- P03: orange primitive CharacterBody3D car, fixed 60 Hz `_PhysicsProcess` movement, W/Up acceleration, S/Down brake then reverse, A/D/arrows steering, Space brake priority, coasting and six Inspector tuning values. Floor expanded to a bounded 60 m square to fit a 36 m outer/20 m inner primitive loop, with no lane obstacles or raised curb. Fixed overview camera and km/h/FWD/REV HUD with controls. No chase camera or recovery implemented.
- P03 Debug and ExportRelease builds: 0 warnings/errors. Headless editor import: passed outside sandbox. C# integration scene checks real input actions and MoveAndSlide over 720 ticks. Headless frame caps 30/60/144: passed with 358/713/1709 frame callbacks; all 12 checkpoint positions/speeds/yaws identical to four decimals. WSLg rendered checks at 30/60 caps: passed with 281/308 frame callbacks; llvmpipe did not sustain cap rates. These establish scripted physics behavior, not human driving feel or native Windows performance.
- P03 Windows export: publish/packing passed; artifacts/windows-p03 has EXE/PCK/full runtime directory, 188 files, approximately 169 MiB. P02a subsequently exported the current scene and transferred it to W:\projektit\driving-time; native launch of this artifact remains unverified. User steps in [RUN.md](RUN.md); automated checks and sources in [TESTING.md](TESTING.md#p03--flat-driving-loop).
- P02a (user-requested helper) done: `dotnet run --file scripts/export-windows.cs` builds/imports/exports inside WSL and copies the complete runtime to W:\projektit\driving-time. Default execution passed with 0 build warnings/errors and 188 files (168.1 MiB) verified by SHA-256 after copying. Artifact and build/import/export logs: `artifacts/windows/20261003-155717-ba7ee883`. Help and invalid-argument checks passed. No native launch/drive performed. Copy is not atomic and preserves stale/unrelated files; close the game before rerunning. This completes only the narrow export helper, not P25 or any game gate. [Card](tasks/P02a-windows-export-script.md), [evidence](TESTING.md#p02a--windows-export-and-transfer-script).
- Generated caches/builds/exports ignored. No datasets downloaded, no task beyond P03 implemented, no commit/push/publish. Primitive geometry is repository-authored synthetic meter-space content; no geographic CRS or vertical datum.
- Full commands, checksums, sources and limitations: [TESTING.md](TESTING.md#p02--minimal-scene-build-export-and-native-startup), D014 and [RUN.md](RUN.md).

## Selected world configuration

Region bounds, CRS, vertical datum and geographic origin: not selected. Approximately 1 × 1 km near Töölönlahti and 500 m tiles remain provisional. Candidate axis mapping belongs to P07; none is implied by the synthetic scene. Data sources and license registry remain P09 work.

## Awaiting manual tests and limits

P02 awaits the user's visual confirmation. Its original cube/floor export remains at `C:\TEMP\driving-time-P02\DrivingTime.exe`: expect the orange cube on blue-gray ground without startup errors. The current Main.tscn now contains the P03 driving scene, so its WSLg appearance check expects a car and loop instead; record this separately and identify the tested version. Exact commands: [RUN.md](RUN.md).

G0 remains open until these visual criteria pass. WSL software rendering is not representative of native Windows performance; no FPS performance claim or human driving test. Review the retained .NET 8 game target before its 2026-11-10 support end. The tagged project-generator source fetch was unavailable; no claim that the editor generated the csproj.

The user explicitly selected P03 while P02 visual acceptance was pending; implementation proceeded under that instruction without marking the dependency complete. P03 now awaits a two-minute manual drive, visible car/loop/HUD confirmation and rendering-cap feel comparisons where possible. Follow [RUN.md](RUN.md): drive slowly through corners, stop with Space, hold S to reverse and try both steering directions. Expected: stable upright car, speed falls to zero under braking, REV indicator when backing and comparable response at different caps. Basic floor contact is checked; impacts/slopes are untested. The fixed camera cannot follow a car leaving the loop; driving beyond the ground has no recovery until P05. P04/P05 and G1 remain open.

## Next session

Read AGENTS.md, the plan, this state, [decisions](DECISIONS.md) and [task index](tasks/INDEX.md). Record P02/P03 manual feedback separately before closing either. Recommended next task: P04 after P03 manual acceptance. P06 is independently ready if selected while visual/driving confirmation is pending. Stop at one selected task.
