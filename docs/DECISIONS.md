# Decisions

Updated: 2026-10-03. P00 recorded assumptions; P01 verified tool inventory and official compatibility documentation. Actual builds/exports and dataset claims remain unverified.

| ID | Status | Decision / assumption | Review |
|---|---|---|---|
| D001 | Fixed user requirement | C#/.NET for the game, offline geospatial pipeline, retrieval tools and automated tests; prefer NuGet and dotnet commands. The game must run independently of the pipeline and live map services. | Language changes require an explicit user change. |
| D002 | Selected baseline; runtime unverified | Windows desktop x64 first; Godot 4.5.2-stable .NET with matching 4.5.2-stable .NET export templates. Linux .NET editor/SDK and pipeline in WSL; only exported game files go to Windows (D012). | P02 verifies native architecture, setup, blank-scene build/export/run. See D011. |
| D003 | Selected baseline; SDK available | One installed SDK 10.0.112 pinned in root global.json; planned net10.0 pipeline/tests, Godot-generated game target verified in P02, and one DrivingTime.sln when projects exist. | D013 supersedes D011's SDK/framework selection; P02/P06 verify actual builds and runtime requirements. |
| D004 | Selected experiment; feel provisional | C# CharacterBody3D scripted arcade movement for P03; keyboard, third-person camera, daylight, single player. Existing first-prototype exclusions remain. | Compare only VehicleBody3D alternative; P03–P05 validate driving/collision/slope/reset behavior. |
| D005 | Provisional | Semantic roads, building geometry and terrain are separate inputs; photogrammetry is optional reference or distant scenery. Retain synthetic fixtures when downloads fail. | P09 verifies sources and terms before retrieval. |
| D006 | Provisional | Synthetic scene first, then one real street; approximately 1 × 1 km near Töölönlahti later. Ground-level roads first; bridges and tunnels may be excluded with reasons. | P09 selects exact bounds and measured origin; P16 checks terrain exclusions. |
| D007 | Provisional | Projected meters and local engine coordinates, double-precision source coordinates. Candidate mapping X=east−originEast, Y=height−originHeight, Z=−(north−originNorth). No CRS, datum or origin selected. | P07 verifies axes, handedness, transforms and vertical data. |
| D008 | Provisional | 500 m square tiles, only two neighbors initially; bounded lifecycle before larger coverage. | P22–P24 verify borders, ownership and collision retention. |
| D009 | Provisional | 60 FPS at 1080p as an initial performance target; reference hardware unknown. | P01 records target; P26 measures actual hardware. |
| D010 | Provisional | GLB assets and versioned manifests with reproducible staging into Godot's project for import/export. No specific writer, layout or shared library chosen. | P08 verifies writer and staging; share code only when needed. |

Remaining initial defaults are assumptions, not evidence. No data library, dataset format, license, origin or native dependency is selected. Future records must include rationale, sources and checked dates, affected tasks and acceptance evidence.

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
