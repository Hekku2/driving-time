# Project state

Goal: Small drivable Helsinki desktop prototype<br>
Current gate: G0 — open; command checks pass, visual acceptance pending<br>
Last completed task: P01<br>
Selected task: P02 — awaiting-manual-test<br>
Next recommended task: P03, after P02 visual confirmation<br>
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
- P02: single root DrivingTime.sln, game/project.godot, Main.tscn, startup-marker Main.cs, credential-free Windows export preset and [RUN.md](RUN.md). The static scene has a 20 m floor, 2 m orange cube, directional light and current camera. No vehicle, controls, map data or collision behavior.
- `dotnet build DrivingTime.sln`: passed, 0 warnings/errors. Headless import and C# runtime: exit 0, startup marker printed.
- First export left incomplete output because it could not find a game-local solution, despite exit 0. Corrected with `dotnet/project/solution_directory=".."`; subsequent export successfully published and packed the game without reported errors.
- Export: artifacts/windows/DrivingTime.exe, DrivingTime.pck and complete data_DrivingTime_windows_x86_64 directory; 188 files, approximately 169 MiB before test logs. Runtime files copied to `C:\TEMP\driving-time-P02`; source/tools remain in WSL.
- Bounded native Windows headless and graphical runs exited 0 and printed `P02: main 3D scene ready (C#).` without startup errors. Startup logs are in the Windows test folder. Bounded Linux editor and game launch also exited 0.
- Generated caches/builds/exports ignored. No datasets downloaded, no task beyond P02 implemented, no commit/push/publish.
- Full commands, checksums, sources and limitations: [TESTING.md](TESTING.md#p02--minimal-scene-build-export-and-native-startup), D014 and [RUN.md](RUN.md).

## Selected world configuration

Region bounds, CRS, vertical datum and geographic origin: not selected. Approximately 1 × 1 km near Töölönlahti and 500 m tiles remain provisional. Candidate axis mapping belongs to P07; none is implied by the synthetic scene. Data sources and license registry remain P09 work.

## Awaiting manual tests and limits

P02 awaits the user's visual confirmation: open Main.tscn in the Linux editor through WSLg, run it, then run `C:\TEMP\driving-time-P02\DrivingTime.exe` natively in Windows. Expect the orange cube resting on the blue-gray floor, lit and visible from the angled camera, without startup errors. Record both results separately. Exact commands: [RUN.md](RUN.md).

G0 remains open until these visual criteria pass. WSL software rendering is not representative of native Windows performance; no FPS claim or driving test. Review the retained .NET 8 game target before its 2026-11-10 support end. The tagged project-generator source fetch was unavailable; no claim that the editor generated the csproj.

## Next session

Read AGENTS.md, the plan, this state, [decisions](DECISIONS.md) and [task index](tasks/INDEX.md). Record P02 manual feedback before closing it or executing dependent P03. Recommended next task: P03 after P02 acceptance. P06 is independently ready if selected while visual confirmation is pending. Stop at one selected task.
