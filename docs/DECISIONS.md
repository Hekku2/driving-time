# Decisions

Updated: 2026-10-03. P00 recorded assumptions; P01 verified tool inventory and official compatibility documentation. P02 verifies the minimal game build/export/startup (D014); visual acceptance and datasets remain unverified.

| ID | Status | Decision / assumption | Review |
|---|---|---|---|
| D001 | Fixed user requirement | C#/.NET for the game, offline geospatial pipeline, retrieval tools and automated tests; prefer NuGet and dotnet commands. The game must run independently of the pipeline and live map services. | Language changes require an explicit user change. |
| D002 | Build/export/startup verified; appearance pending | Windows desktop x64 first; Godot 4.5.2-stable .NET with matching 4.5.2-stable .NET export templates. Linux .NET editor/SDK and pipeline in WSL; only exported game files go to Windows (D012). | P02 evidence in D014; user visual check remains open. |
| D003 | Game and pipeline compatibility verified | One installed SDK 10.0.112 pinned in root global.json; net10.0 pipeline/tests verified in P06, game net8.0 verified in P02, and one root DrivingTime.sln. | D014 records game evidence; P06 records locked restore/build/test evidence. |
| D004 | Controller choice superseded by D016 | Historical C# CharacterBody3D scripted arcade movement for P03; keyboard, third-person camera, daylight, single player. Existing first-prototype exclusions remain. | Retain the runnable baseline; VehicleBody3D is now chosen (D016). |
| D005 | Provisional | Semantic roads, building geometry and terrain are separate inputs; photogrammetry is optional reference or distant scenery. Retain synthetic fixtures when downloads fail. | P09 verifies sources and terms before retrieval. |
| D006 | Provisional | Synthetic scene first, then one real street; approximately 1 × 1 km near Töölönlahti later. Ground-level roads first; bridges and tunnels may be excluded with reasons. | P09 selects exact bounds and measured origin; P16 checks terrain exclusions. |
| D007 | Contract/projection verified; real data validation pending | EPSG:3067 E,N projected metres, N2000 real heights, double source/normalized coordinates; X=east−originEast, Y=height−originHeight, Z=−(north−originNorth), right-handed. Real origin/bounds unselected. | P07 contract/tests and D017; P09 selects traceable origin and verifies source axes/datums; P17 validates heights. |
| D008 | Provisional | 500 m square tiles, only two neighbors initially; bounded lifecycle before larger coverage. | P22–P24 verify borders, ownership and collision retention. |
| D009 | Provisional | 60 FPS at 1080p as an initial performance target; reference hardware unknown. | P01 records target; P26 measures actual hardware. |
| D010 | Provisional | GLB assets and versioned manifests with reproducible staging into Godot's project for import/export. No specific writer, layout or shared library chosen. | P08 verifies writer and staging; share code only when needed. |

Remaining initial defaults are assumptions, not evidence. P07 selects ProjNET for bounded horizontal projection (D017); no dataset reader/format, source-data license, real origin or native dependency is selected. Future records must include rationale, sources and checked dates, affected tasks and acceptance evidence.

## D016 — VehicleBody3D chosen

Date: 2026-10-03. Status: chosen by the user. Supersedes D004's controller choice and resolves D015's pending selection.

User instruction: “Record VehicleBody3d as chosen”. Use the existing C# VehicleCar / VehicleBody3D implementation for subsequent driving work. VehicleExperiment.tscn is already the default scene; retain Main.tscn / Car.cs as the runnable CharacterBody3D comparison baseline. No code, scene, tuning or export change is needed to record this choice.

Evidence: explicit user selection alongside existing P05a automated/export evidence. No tested artifact/platform or detailed driving observations were supplied; smoothness and a jitter fix remain unverified. P05a remains awaiting-manual-test, and G0/G1 remain open. Affected records: P05a, project state, task ledger and plan. No new API claims or external source checks are needed for this decision-only update.

## D011 — P01 toolchain and first experiment

Date / checked date: 2026-10-02. Status: historical version/controller baseline; its original Windows-development assumption was superseded by D012 and SDK/pipeline framework selection by D013 on 2026-10-03. The paragraphs below record the original rationale, not current setup instructions. Installation/build/export not performed.

Confirm Windows desktop as the first target and preserve D001. Use Godot 4.5.2 .NET and its matching .NET templates with SDK 8.0.425. Versioned [Godot C# docs](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html) document .NET 8 and desktop export; the [release archive](https://godotengine.org/download/archive/4.5.2-stable/) provides the editor/templates, and [Microsoft SDK downloads](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) identify the exact SDK. Prefer this explicitly documented pair over assuming the installed Linux SDK 10.0.112 proves compatibility with the newer engine. Windows currently has no usable selected SDK. The exact pin intentionally fails until 8.0.425 is supplied; it does not install anything.

Use net8.0 for the proposed desktop game and independent pipeline; one SDK suffices without a framework split. This is documentation-backed compatibility, not build evidence. Defer root solution creation to the first selected project task and any shared library until actual consumers need it. [Microsoft TFM guidance](https://learn.microsoft.com/en-us/dotnet/standard/frameworks) and [global.json rules](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) support this arrangement.

Known tradeoff: .NET 8 support ends 2026-11-10 ([official policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)). Revisit the complete toolchain before that date or later implementation; use a focused compatibility/build/export task to validate a supported upgrade. No long-term commitment to the 4.5/.NET 8 baseline is implied.

Choose CharacterBody3D with scripted C# movement for the flat arcade experiment. [CharacterBody3D](https://docs.godotengine.org/en/4.5/classes/class_characterbody3d.html) supplies wall/slope movement support; [VehicleBody3D](https://docs.godotengine.org/en/4.5/classes/class_vehiclebody3d.html) adds wheel configuration and documented limitations. CharacterBody3D reduces the first experiment's tuning scope; it needs explicit gravity and will not provide realistic suspension or impacts. Validate in P03–P05 before extending it.

Keep D009's 1920 × 1080, 60 FPS target provisional; use 16.67 ms as the frame budget, with a provisional 60 Hz physics tick. Reference hardware remains unknown and no performance claim is made. See [SETUP.md](SETUP.md) for reproducible steps, exact observations and verification limitations. Affected tasks: P02, P03, P05, P06, P07 and P26.

## D012 — Develop in WSL, transfer the exported game to Windows

Date: 2026-10-03. Status: fixed user workflow requirement. Supersedes the Windows-development/optional-WSL assumption in D002/D011 and the original plan defaults.

User instruction: development will be made in WSL; only the executable is transferred to Windows. Keep code, Godot editing/import, C# builds/tests, pipeline conversion and Windows export in WSL2. Use the Linux x64 Godot .NET editor and Linux SDK at the existing selected versions. Windows is the native game runtime/test destination; it requires no development checkout, editor, SDK or pipeline.

Interpret the transfer as the exported runnable game with any companion files the C# export actually needs. A lone .exe is not yet a verified packaging result. P02 must inventory the export and test native Windows launch after transfer; preserve separate evidence for WSL editing/build, cross-export and Windows run. WSLg/GPU and Linux-to-Windows export execution remain unverified.

Checked official sources on 2026-10-03: [Linux .NET editor/templates archive](https://godotengine.org/download/archive/4.5.2-stable/), [WSLg prerequisites](https://learn.microsoft.com/en-us/windows/wsl/tutorials/gui-apps), [C# desktop support](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html) and [export workflow](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_projects.html). These establish setup routes, not successful local execution. No versions, gameplay choices or task statuses changed. Affected tasks: P02, P06 and later build/export tasks. Setup instructions and plan prompts now reflect this requirement.

## D013 — Use the installed .NET 10 SDK

Date / checked date: 2026-10-03. Status: user-approved SDK selection; game build/export unverified. Supersedes the SDK and pipeline framework selection in D003/D011; preserves D012.

User instruction: “dotnet 10 is fine, update setupmd and other documents”. Pin the installed Linux x64 SDK 10.0.112 in global.json, retaining disabled roll-forward and prereleases. `dotnet --info` reconfirmed SDK 10.0.112 and runtime 10.0.12; `dotnet --version` after the change confirms selection. No .NET 8 SDK install is required. Plan net10.0 for the independent pipeline and its tests in P06.

The [Godot 4.5 C# guide](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html) identifies .NET 8 as its baseline and recommends the latest stable SDK. [Microsoft version selection](https://learn.microsoft.com/en-us/dotnet/core/versions/selection) distinguishes SDK, target framework and runtime selection; [global.json](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) controls the SDK. These support selecting SDK 10 without assuming that the game must target net10.0. Keep the existing Godot version/templates selection; P02 inspects its generated project and verifies build, editor loading and Windows export before claiming compatibility. A standalone net8.0 test/CLI may need a runtime absent from the current .NET 10-only Linux inventory. Any future shared library must support both actual consumers.

[Microsoft support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) lists .NET 10 as LTS; any retained .NET 8 game target still needs review before 2026-11-10. Sources checked on the date above. No engine install, package restore, project creation, build, export or manual runtime check was performed. Affected tasks: P01 evidence, P02 toolchain validation, P06 pipeline baseline; statuses unchanged.

## D014 — P02 verified desktop slice

Date / checked date: 2026-10-03. Status: build/import/export/startup verified; manual appearance pending. Preserves D001/D012/D013 and confirms D002/D003 for this slice.

Use the installed Linux Godot `4.5.2.stable.mono.official.6ce3de25a` and matching 4.5.2.stable.mono templates with Linux SDK 10.0.112. The minimal csproj is explicitly authored using Godot.NET.Sdk/4.5.2, net8.0 and EnableDynamicLoading. It was not generated by the editor: tagged project-generator source fetches remained unavailable. The installed editor plugin runtimeconfig has net8.0/LatestMajor roll-forward. Actual editor loading and game startup on the installed .NET 10 runtime passed; Windows export includes self-contained Microsoft.NETCore.App 8.0.31. SDK 10 approval does not change the game target. Review net8.0 before 2026-11-10.

Keep one root DrivingTime.sln, with Debug and ExportRelease mapped to the Godot SDK's actual configurations. Set `dotnet/project/solution_directory=".."` so editor/export uses it. The first export exposed the missing default game-local solution and returned exit 0 with errors and incomplete output; corrected export successfully published and packed the game. Check logs and runtime output as well as exit status.

Use GL Compatibility for the minimal desktop scene. WSLg editor/game launches work through llvmpipe software rendering, with V-Sync warning and missing-audio-library fallback. Native Windows graphical startup reports RTX 3060; no performance conclusion. Only runtime output was transferred to C:\TEMP\driving-time-P02 (188 files, approximately 169 MiB). EXE, PCK and full data_DrivingTime_windows_x86_64 folder are required. The preset disables signing and resource modification, with no credentials or extra packaging tool required.

Official sources checked on this date: [Godot C# prerequisites/desktop support](https://docs.godotengine.org/en/4.5/tutorials/scripting/c_sharp/c_sharp_basics.html), [command-line import/export](https://docs.godotengine.org/en/4.5/tutorials/editor/command_line_tutorial.html), [Windows export](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_for_windows.html) and [solution directory setting](https://docs.godotengine.org/en/4.5/classes/class_projectsettings.html#class-projectsettings-property-dotnet-project-solution-directory). Local commands/results: [TESTING.md](TESTING.md#p02--minimal-scene-build-export-and-native-startup). User steps: [RUN.md](RUN.md). Affected tasks: P02; informs P03/P06/P08 without completing them. G0 stays open pending visual confirmation.

## D015 — P05a built-in vehicle comparison

Date / checked date: 2026-10-03. Status: historical user-selected experiment; controller selection resolved by D016. Smoothness remains unverified. D004 records the original arcade baseline choice.

User feedback: movement feels a little jittery; requests testing VehicleBody3D. Add a separate VehicleExperiment.tscn/VehicleCar.cs and make it the current default for playtesting, retaining Main.tscn/Car.cs as the comparison baseline. Reuse the test loop and unchanged chase camera settings. No claim that body type caused or fixes the reported jitter; camera/interpolation/rendering remain possible causes. Compare before selecting a permanent movement model.

Use rear-wheel propulsion, front-wheel steering, four suspension wheels and low custom center of mass. Preserve the scene's -Z nose/camera convention by negating engine force; physical checks confirm W forward and D right. Use force/brake/steering properties for normal driving; exceptional recovery runs through the rigid-body integration callback to clear linear/angular velocity and notify the camera. Wheel and body interpolation are explicitly enabled; global interpolation and 60 Hz ticks are unchanged. No realistic tire model, gears or external dependencies.

Official sources checked: [VehicleBody3D](https://docs.godotengine.org/en/4.5/classes/class_vehiclebody3d.html), [VehicleWheel3D](https://docs.godotengine.org/en/4.5/classes/class_vehiclewheel3d.html), [RigidBody3D](https://docs.godotengine.org/en/4.5/classes/class_rigidbody3d.html). The docs identify raycast wheels, +Z vehicle front, force controls and known realism limitations, suspension parameters and direct-state integration. Local tuning is experimental rather than documentation-certified behavior. Build/import, 30/60-cap physics checks, baseline regressions and Windows export/transfer pass; native appearance/feel pending. Actual results: [TESTING.md](TESTING.md#p05a--vehiclebody3d-comparison). Affected task: P05a only; G1 remains open.

## D017 — P07 coordinate contract and bounded managed projection

Date / checked date: 2026-10-03. Status: synthetic contract and horizontal projection verified; real origin/data/elevation validation pending. Resolves D007's mapping/CRS choice; preserves D001/D012/D013.

Adopt EPSG:3067 projected E,N metres, N2000 heights for real elevated assets, immutable configured origin and right-handed X=east offset, Y=height offset, Z=negative north offset. Keep source/projected calculations double; cast only after localization. Tile anchors share origin/height and use floor indices including negatives. Origin remains unselected until P09; P06 synthetic values are not Helsinki coordinates.

Use pinned ProjNET 2.1.0 for the offline horizontal TM35FIN projection with explicit GRS80 WKT parameters. Package netstandard2.0/2.1 metadata and actual net10.0 restore/build/tests establish compatibility; no shared library or native dependency added. LGPL-2.1-or-later package metadata recorded; no redistribution performed. Actual library lon/lat input semantics verified against the independent NLS/FGI example even with latitude-first WKT. EPSG:4326 conversion rejects by default; explicit approximate datum opt-in treats WGS84 angles as EUREF-FIN and must be recorded/accepted during source review. No accurate epoch-aware datum/grid/geoid support is claimed.

31 tests pass including P06 regressions, cardinal/100 m/elevation/precision/round-trip/tile checks and NLS example 10 (residual magnitudes under 0.000027 m, acceptance 0.01 m). Contract review complete; no editor/import/export or manual drive. Required vertical-control validation and sources/checked dates are in [COORDINATES.md](COORDINATES.md); actual commands in [TESTING.md](TESTING.md#p07--coordinate-contract-and-transform-evaluation). Affected tasks: P07, P08/P09/P11 and later elevation/tile work; no later task completed.
