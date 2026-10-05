# Helsinki driving game: expandable Codex plan

Version: 1.2 · Created: 2026-10-01 · Updated: 2026-10-05 · Status: P13 first Helsinki road scene implemented, awaiting manual drive; P08/P12 and G0/G1/G2 manual evidence remain open

## Goal

Build a non-commercial desktop 3D game in which one player can drive through recognizable Helsinki streets. Develop it through small, independently reviewable tasks. The plan is a living backlog: early prompts implement narrow features; later prompts generate more detailed plans as evidence becomes available.

The first useful result is a car driving around a small synthetic test scene. The first Helsinki milestone is a short real-world road segment with surrounding buildings. The initial area suggestion was approximately 1 × 1 km near Töölönlahti. On 2026-10-04 the user selected Stenbäckinkatu instead; P09 records its provisional 1 km square, measured origin and source evidence in [DATA_SOURCES.md](docs/DATA_SOURCES.md) and [source-registry.json](config/source-registry.json). Whole-city coverage is a later ambition.

Selected language requirement and initial assumptions, to confirm in P01:

- Godot 4 .NET edition with C# for a Windows desktop game.
- C#/.NET for the offline geospatial pipeline as well as the game. This is a user requirement, not a provisional language choice. Use a .NET console CLI and NuGet dependencies; the running game must not depend on the pipeline or live map services.
- Development in WSL2 is a fixed user workflow requirement (2026-10-03, D012): use the Linux Godot .NET editor and Linux .NET SDK for editing, builds, tests, pipeline work and Windows export. Keep the repository and development tools in WSL. Transfer only the exported playable game, including required companion runtime files, to Windows for native execution.
- .NET 10 is user-approved (2026-10-03, D013). Use the installed WSL SDK 10.0.112 pinned in global.json; plan net10.0 for the independent pipeline/tests. P02 verifies the Godot-generated game target, runtime loading and Windows export with this SDK before claiming compatibility.
- Keyboard controls, third-person camera, simple arcade handling, daylight, single player.
- Use semantic road data, building geometry and terrain as separate inputs. Photogrammetry is optional reference or distant scenery.
- Synthetic fixtures remain available even when downloads fail.
- No traffic, pedestrians, multiplayer, missions, interiors or realistic simulation in the first prototype.

C# for both game and pipeline is fixed by the current user instruction. The WSL-development/Windows-runtime workflow is also fixed by the 2026-10-03 user instruction. Other defaults can evolve as evidence changes. Pin actual tool versions after verifying compatibility; do not assume the newest engine and newest .NET SDK are compatible. Aim for one installed .NET SDK that supports both projects, and verify target-framework compatibility rather than forcing identical target frameworks. Test export early. A web target requires a fresh engine/export-support decision while preserving the C# requirement unless the user changes it.

## How to use this file with Codex

Permanent project documents use feature names and current behavior rather than historical task IDs. Task IDs belong in this backlog and the task ledger/cards for proposed changes and outstanding acceptance work. Completed cards are historical records, not required usage documentation. Preserve literal paths, commands and recorded log markers; update incoming anchors when headings change. AGENTS.md defines this documentation rule.

User-selected follow-up P01a (2026-10-05): [Check development environment prerequisites](docs/tasks/P01a-environment-preflight.md). One inventory command with installation guidance; no installs, gameplay or later task execution. Current missing editor/templates are recorded separately from historical export evidence.

User-selected follow-up P05a (2026-10-03): [Compare VehicleBody3D driving](docs/tasks/P05a-vehicle-experiment.md). Following the built-in vehicle experiment, the user chose VehicleBody3D (D016). Keep the arcade baseline runnable. Detailed loop/ramp/turning manual evidence remains open. Status and evidence live in the card/ledger. This does not execute P06 or close G1.

User-selected follow-up P02a (2026-10-03): [Script the current Windows export and transfer](docs/tasks/P02a-windows-export-script.md). Status: done. This narrow helper builds/imports/exports the existing game in WSL and copies verified runtime files to W:\projektit\driving-time. It does not execute P25 or close P02/P03 manual acceptance. Dependencies and acceptance evidence are in the task card and ledger.

1. Put this file in the root of a new repository as `HELSINKI_DRIVING_PLAN.md`.
2. Run **P00** by pasting its prompt into Codex. This creates the working instructions and task ledger; it does not build the game.
3. Run one prompt at a time, in dependency order. Copy its entire code block. Every numbered prompt inherits the execution rules below.
4. After an implementation prompt, perform the stated manual check when required. Tell Codex the result using the feedback prompt. The agent can continue independent preparation, but must not treat an unperformed manual check as passed.
5. Use the resume prompt for a new session. Use the expansion prompt when a task is too broad or unknown.
6. Keep this plan, generated task cards, decisions and progress in Git. Keep bulk source data and generated assets out of ordinary Git unless there is a deliberate storage decision.

Do not run the entire plan in one Codex invocation. A phase is a collection of small tasks, not permission to implement them all. Prompts can be edited when the evidence changes; preserve IDs and record why.

## Execution contract for all prompts

P00 will copy these rules into `AGENTS.md`. Until then, include this file in Codex context.

- Read repository instructions, this plan, `docs/PROJECT_STATE.md`, and the selected task card before changing code. Inspect existing work and preserve unrelated changes.
- Execute only the requested task. If it needs more than one independently testable feature, split it into child tasks before implementation. Aim for one focused Codex session and one useful test outcome; do not invent time estimates.
- Prefer a working vertical slice over a broad framework. Do not introduce dependency injection containers, plugin systems, generic orchestration frameworks or cloud infrastructure without a concrete need.
- Inspect installed tools. Consult official documentation for uncertain APIs, current export support, data access and dependency compatibility. Record sources and checked dates. If browsing is unavailable, record what remains unverified.
- Never pretend a dataset was fetched, the editor launched, an export succeeded or a manual drive test passed. Record actual commands and results. Use `unverified` or `blocked` when appropriate.
- Use targeted automated tests for coordinate transforms, parsing, geometry and tile lifecycle behavior. Use manual checks for appearance and driving feel. Avoid tests that merely duplicate code.
- A task is `done` only when its acceptance criteria have evidence. A working feature awaiting the user's visual test is `awaiting-manual-test`.
- Keep changes reproducible. Configuration controls paths, region, origin and tile size. Downloads are bounded, cached and separate from offline conversion. Fail with actionable messages.
- Write game logic, offline conversion, retrieval tools and automated tests in C#/.NET. Prefer NuGet libraries and `dotnet` commands. Do not add Python, Node.js, containers or an external GIS CLI to the required workflow. If a dataset needs a native dependency, investigate a managed alternative or a simpler source format first, document the smallest necessary dependency and its packaging before adding it. Godot remains the game editor/import/export tool.
- Use one repository-level .NET solution and a compatible pinned SDK where practical. Keep pipeline code independent of Godot. Share a small contract or coordinate library only when both projects actually need it; verify it targets a framework supported by both. Do not build a shared framework in advance.
- Document data provenance, license, retrieval date, checksum, coordinate reference system, vertical datum and known limitations. Non-commercial use does not remove license obligations. Do not redistribute unreviewed source data.
- End with: what changed, commands/tests and outcomes, a short manual test with expected result, known limitations, and one recommended next task ID.
- Update `docs/PROJECT_STATE.md` and the task ledger. Do not mark later tasks complete simply because some scaffolding exists. Do not commit, push or publish unless requested or already authorized in repository instructions.
- Keep permanent project documentation independent of task IDs: use feature names, current behavior and dated evidence. Tasks describe upcoming changes and outstanding acceptance work in the backlog and task ledger/cards; completed cards may remain as historical records. Reference a task in project state only for the next proposed change. Preserve literal paths, commands and historical output, and update incoming Markdown anchors when headings change.
- Stop at the selected task boundary. Planning prompts produce plans and TODOs, not implementation. Raise a blocker only after useful independent work has been completed.

### Completion statuses

`todo` → `in-progress` → `awaiting-manual-test` → `done`

Also allow `blocked` and `superseded`. If an automated test is sufficient, `in-progress` can go directly to `done`. Track separate automated and manual results rather than collapsing them into a single checkbox.

## Repository layout to grow into

Create directories only when used. P00 creates documentation; later tasks create code.

```text
HELSINKI_DRIVING_PLAN.md
AGENTS.md
README.md
game/                         # Godot project and C# runtime
pipeline/                     # C#/.NET offline conversion CLI and libraries
tests/fixtures/               # Small synthetic and redistributable fixtures
config/                       # Region, coordinate origin, conversion settings
data/raw/                     # Ignored downloads
data/intermediate/            # Ignored normalized datasets
data/generated/               # Ignored game assets
scripts/                      # Build/run helpers when needed
docs/PROJECT_STATE.md          # Current truth and next task
docs/DECISIONS.md              # Short decision records
docs/DATA_SOURCES.md           # Verified data access and licenses
docs/TESTING.md                # Commands and manual checks
docs/tasks/INDEX.md            # Status and dependencies
docs/tasks/Pxx-description.md  # Task cards and future child tasks
```

Generated assets must be staged into an engine-readable location for import/export. Decide how in P08; the diagram does not imply that Godot automatically imports files outside its project.

## Stable task-card format

Every generated task, including child tasks, uses this format. Keep IDs stable, such as `P17a` or `E02-01`. Add dependency links to the index.

```markdown
# <ID>: <short title>
Status: todo
Type: research | planning | implementation | validation
Dependencies: <IDs or none>
Goal: <one observable result>
Inputs: <existing files, fixtures, decisions>
Scope: <included behavior>
Out of scope: <limits relevant to this task>
Deliverables: <files or runnable behavior>
Acceptance:
- [ ] <automated criterion with command or planned command>
- [ ] <manual criterion with steps and expected result, if needed>
Evidence: <actual results; initially not run>
Risks / blockers: <specific uncertainties>
Follow-up tasks: <IDs; no implementation implied>
```

A child plan should normally contain 2–6 tasks. Do not split a simple change into artificial subtasks. Parent tasks remain open until their required child tasks and acceptance criteria are complete.

## Phase gates

| Gate | Result | Required tasks | User check |
|---|---|---|---|
| G0 | Repository instructions and viable toolchain | P00–P02 | Run the blank exported Windows game |
| G1 | Car, camera and resets in a synthetic scene | P03–P05 | Drive a loop, stop and reset |
| G2 | Offline pipeline produces correctly aligned synthetic assets | P06–P08 | Inspect scale, directions and landmarks |
| G3 | Real road and simple buildings are drivable | P09–P15 | Compare a recognizable street with reference data |
| G4 | Terrain and road elevations work in a bounded area | P16–P21 | Drive a route with a slope and an intersection |
| G5 | Adjacent tiles load without holes or collision loss | P22–P24 | Cross tile boundaries repeatedly |
| G6 | Repeatable prototype export and measured performance | P25–P27 | Run exported game and inspect recorded metrics |

Do not expand the area to compensate for an unsolved alignment, road topology or driving problem. G3 may use temporary flat ground; G4 replaces that assumption deliberately. Bridges and tunnels can be excluded from the first route with a documented reason.

## Prompts: foundation and driving

### P00 — Bootstrap the planning workspace

Type: **planning** · Depends on: **none**

```text
Read HELSINKI_DRIVING_PLAN.md. Create AGENTS.md from its execution contract, a short README explaining the project and how to resume, docs/PROJECT_STATE.md, docs/DECISIONS.md, docs/TESTING.md, and docs/tasks/INDEX.md. Create task cards for P01–P08 from this plan; list P09–P27 as deferred tasks with their dependencies. Record all tasks as todo except this bootstrap task. Record C#/.NET for both game and pipeline as a fixed user requirement, and the remaining assumptions as provisional. Do not create a game project or install dependencies. End by recommending P01.
```

Completion check: Read the generated instructions and confirm they constrain Codex to one selected task and distinguish manual checks from completed work.

### P01 — Verify the toolchain and choose the first slice

Type: **research** · Depends on: **P00**

```text
Execute P01 under the plan's execution contract. Inspect available operating systems, Godot .NET edition, .NET SDK and Git. Verify the engine/SDK/export-template compatibility using official documentation; select and record exact versions, or document required installs if unavailable. Confirm Windows desktop as the first target and record the required C#/.NET stack for both game and pipeline. Prefer one SDK selected through global.json and a single .NET solution; verify supported target frameworks and document any unavoidable differences. No second language runtime is part of the required setup. Choose a simple arcade controller for the first driving experiment; compare only the minimum relevant options. Set a provisional 1080p performance target and record that the reference hardware is still unknown if it cannot be measured. Produce setup instructions and a decision record. Do not implement gameplay or install system-wide tools without a concrete need.
```

Completion check: A developer can follow the setup instructions; versions and missing tools are explicit. No engine API or support claim is presented as verified without evidence.

### P02 — Launch and export an empty 3D scene

Type: **implementation** · Depends on: **P01**

```text
Execute P02. Create the smallest Godot .NET project with a main 3D scene, visible floor, light, camera and simple reference cube. Include a run guide and a Windows export preset without credentials. Build the C# project and attempt a Windows desktop export inside WSL using the Linux .NET/Godot toolchain. Keep development in WSL and transfer only the exported runnable game, including required companion files, to Windows for native testing. If the environment cannot run the editor or Windows executable, record that limitation and give exact local steps. Do not add a vehicle or map data.
```

Completion check: Open the scene and exported application: the floor and cube are visible, with no startup errors. Actual export success and local run success are recorded separately.

### P03 — Move a simple car on flat ground

Type: **implementation** · Depends on: **P02**

```text
Execute P03. Add a visible placeholder car with keyboard acceleration, braking, reverse and steering on the existing flat ground. Keep the implementation deliberately simple and physics-step based; expose a few tuning parameters. Build a small obstacle-free test loop using primitives. Show speed in km/h and document controls. Do not implement realistic tires, suspension, traffic or detailed art. Verify behavior at different rendering frame rates where possible.
```

Completion check: Drive, steer, stop and reverse for two minutes. Movement remains controllable and does not depend materially on rendering frame rate.

### P04 — Add a usable chase camera

Type: **implementation** · Depends on: **P03**

```text
Execute P04. Add a third-person chase camera with bounded smoothing and a sensible view of the road ahead. Handle basic camera obstruction in the primitive test scene. Keep camera logic separate from driving input. Do not add multiple camera modes or cinematic effects. Provide a short manual test for turning and driving near a wall.
```

Completion check: Drive a tight loop and past a wall; the camera follows smoothly and does not leave the player viewing from inside an obstacle.

### P05 — Add collision and safe recovery

Type: **implementation** · Depends on: **P04**

```text
Execute P05. Add several simple static obstacles and a slope to the test scene. Verify the car's chosen collision model. Add a reset action to a defined spawn and automatic recovery below a configurable world limit. Clear velocity and camera state on reset. If the current movement approach cannot satisfy these modest tests, describe the narrow defect and propose a focused child task. Do not redesign the whole controller.
```

Completion check: Hit an obstacle, drive the slope, fall out of bounds and reset. The vehicle returns safely without retained motion. Record G1 results.

### P06 — Create the offline pipeline CLI

Type: **implementation** · Depends on: **P01**

Status: **done** (2026-10-03). [Task card](docs/tasks/P06-pipeline-cli.md), [WSL workflow/restore policy](docs/PIPELINE.md), [command evidence](docs/TESTING.md#offline-synthetic-manifest-cli). SDK 10.0.112, independent net10.0 CLI/tests in root solution, locked restore, Debug/ExportRelease builds and 15 tests pass. Two no-restore runs produce identical versioned manifests; invalid configuration exits 2. Synthetic CLI only; P07 now verifies its separate coordinate contract, P08 remains todo and G2 stays open.

```text
Execute P06. Create a minimal C#/.NET console CLI in pipeline/ and add it to the repository solution alongside the game when present. Use the SDK verified in P01, explicit NuGet versions and a documented restore policy. Read validated region/origin configuration and write a versioned manifest for a synthetic scene, using built-in .NET facilities where sufficient. Include one documented dotnet run command, deterministic output ordering, clear input errors and a small .NET test project for nontrivial validation. Start with no geospatial downloads. Document a WSL-development workflow that requires no Python runtime, Node.js, container or external GIS CLI; the pipeline stays in WSL and only exported game runtime files go to Windows. Add only dependencies already needed for this task.
```

Completion check: After the documented initial restore, run the command twice offline and compare outputs; they match except explicitly excluded diagnostic metadata. Invalid configuration fails clearly. The CLI builds and its targeted checks run with dotnet build / dotnet test.

### P07 — Specify and verify coordinate transforms

Type: **implementation** · Depends on: **P06**

Status: **done** (2026-10-03). [Task card](docs/tasks/P07-coordinates.md), [coordinate contract](docs/COORDINATES.md), [command evidence](docs/TESTING.md#coordinate-contract-and-transform-evaluation). Double-precision cardinal/100 m/elevation/round-trip/tile checks and ProjNET 2.1.0 comparison with independent NLS example pass; 31 total tests, locked restore and root Debug build pass. EPSG:3067/N2000 target selected, real origin unselected. Accurate WGS84 datum/vertical conversions unsupported; explicit approximation policy documented. P08/G2 visual import remains open.

```text
Execute P07. Define an explicit coordinate contract: source axis order and units, projected east/north coordinates, vertical datum, fixed origin, engine axes, handedness and tile-local coordinates. Use projected meters and local engine coordinates; keep source coordinates in double precision. A possible mapping is X=east-originEast, Y=height-originHeight, Z=-(north-originNorth), but verify it rather than assuming source X means east. Implement and test synthetic cardinal points, a measured 100 m segment, round trips and a nonzero elevation. Record how EPSG:4326 to the chosen projected CRS will be handled in C# and how vertical data will be validated. Evaluate a .NET coordinate-transform library against a trusted reference and verify its required CRS support and axis semantics; do not assume a package name proves compatibility. Do not invent origin coordinates.
```

Completion check: Automated fixtures prove direction, 100 m scale, elevation and round-trip tolerance. The coordinate contract includes source CRS axis-order handling.

### P08 — Import one generated GLB and manifest

Status: **awaiting-manual-test** (2026-10-04). [Task card](docs/tasks/P08-synthetic-import.md), [staging/schema/workflow](docs/PIPELINE.md#synthetic-glb-staging), [evidence](docs/TESTING.md#synthetic-glb-import). SharpGLTF.Toolkit 1.0.7 verified with a 6,664-byte synthetic GLB; 33 tests, locked restore, builds, Godot import and native Windows exported geometry/material checks pass. Editor/Windows visual inspection remains open; G2 stays open. Next recommended task: P09.

Type: **implementation** · Depends on: **P02, P07**

```text
Execute P08. Generate one small synthetic GLB containing a building-like cube and direction markers through the C# pipeline. Verify a suitable .NET glTF/GLB writer with this fixture; avoid adding Blender or a scripting runtime to the required build. Define a minimal versioned manifest including bounds, units, coordinate origin and asset paths. Choose a reproducible staging/import workflow into the Godot project that also works in exports. Display the generated scene in a separate inspection scene. Verify winding, normals, materials, scale and axis conversion happen exactly once. Keep fixtures small; do not add real data or streaming.
```

Completion check: Inspect the markers and a 10 m cube in Godot and an export. Automated bounds and manifest validation pass. Import has no unexplained mirroring or double rotation.

## Prompts: real Helsinki data and first street

### P09 — Verify data sources and select a bounded area

Type: **research** · Depends on: **P07**

```text
Execute P09. Consult current official Helsinki documentation and official OpenStreetMap access/licensing guidance. Verify practical access to building data and terrain, actual file formats, coverage, source CRS, vertical datum, update dates and license terms for the selected products. Define a provisional approximately 1 × 1 km area near Töölönlahti with a short simple drive route; avoid multi-level road structures initially. Record exact bounds and a measured origin derived from data. Produce docs/DATA_SOURCES.md and a machine-readable source registry, with provider links and checked dates. Distinguish provider claims from measurements. Plan bounded cached retrieval, service limits, storage estimates and manual-download fallback. This is research; do not download city-wide data or make legal conclusions from non-commercial status.
```

Completion check: The selected area, origin, access route and dataset-specific terms are traceable. Missing data has a named fallback or blocked task.

P09 executed 2026-10-04: [research card](docs/tasks/P09-data-sources.md) done with the user's Stenbäckinkatu area selection. Building/terrain probes and registry inspection pass. OSM access failed; manual fallback and P10 route validation are recorded. Footprint fallback terms and independent heights remain P14/P17 checks. P09 does not close any playable-area gate or execute acquisition/conversion tasks.

### P10 — Acquire and inspect a small road sample

Type: **implementation** · Depends on: **P09**

```text
Execute P10. Implement one explicit retrieval/import command for a bounded OpenStreetMap road sample using the access method verified in P09. Cache raw bytes with provenance, request parameters and checksum; allow an offline local-file input. Limit requests and provide clear failures. Summarize counts, geographic bounds and available highway/oneway/lanes/maxspeed/layer/bridge/tunnel tags without guessing missing values. Add a tiny synthetic parser fixture. Do not generate roads yet.
```

Completion check: A cached road sample can be parsed offline. Reported bounds match the selected area; malformed or incomplete input is detected.

### P11 — Normalize a small road graph

Type: **implementation** · Depends on: **P10, P07**

```text
Execute P11. Convert the road sample into a minimal versioned intermediate graph in projected meters. Preserve OSM IDs, geometry, attributes and provenance. Split at shared junction nodes; distinguish one-way direction including oneway=-1, and preserve grade-separation tags. Handle clipping without treating a boundary cut as a genuine intersection. Report isolated segments and unsupported situations. Test a two-way street, reversed one-way street, shared intersection and crossing roads on different levels. Do not create AI lanes or intersection surfaces yet.
```

Completion check: Small automated fixtures prove graph connectivity, direction and level separation. A debug plot or engine overlay makes the real graph inspectable.

P11 execution evidence (2026-10-04): [task card](docs/tasks/P11-road-graph.md) done. Offline graph v1/SVG conversion, provenance validation, 65 pipeline tests and root Debug build pass. Real sample yields 11 segments/22 arcs and one component; western cut is not a junction. No lane/surface/game work; next P12.

### P12 — Generate one flat road strip

Type: **implementation** · Depends on: **P11, P08**

```text
Execute P12. Generate a simple flat asphalt mesh for one short real road segment without junctions. Use configured width fallbacks with a report identifying assumptions. Handle bends, duplicate points and near-zero-length segments; cap extreme joins. Produce a separate simple collision representation and deterministic GLB/manifest output. Test bounds and mesh validity. Do not generate a whole district or lane markings.
```

Completion check: One curved segment imports at the correct scale, with no degenerate triangles or spikes. Its collision is present and independently inspectable.

2026-10-04: [P12 card](docs/tasks/P12-road-strip.md) awaiting-manual-test. One 100 m interior strip and separate collision GLB/manifest; 78 tests, full Debug build, deterministic replay and Godot metre-bounds/collision-ray checks pass. Manual appearance remains open; next P13, without closing prior gates.

### P13 — Drive on the first Helsinki road

Type: **implementation** · Depends on: **P05, P12**

```text
Execute P13. Create a dedicated Helsinki prototype scene that loads the generated single-road asset through the staging workflow. Place the spawn using the shared coordinate contract. Add temporary flat ground and a few labeled reference markers so placement can be checked. Drive the existing car on the road and collision surface. Provide a short manual route. Do not add real buildings or expand the road area.
```

Completion check: The car spawns correctly and drives the segment; reference positions align with the pipeline output. Real elevation remains explicitly deferred.

2026-10-05: [P13 card](docs/tasks/P13-first-road-drive.md) awaiting-manual-test. Manifest-based spawn/reference markers, existing vehicle/camera, temporary ground and default dedicated scene. Debug build/78 tests/import and 1,376-tick drive/collision/recovery check pass; maximum lateral error 0.200 m. Windows export skipped at user request. Prior manual gates stay open; next P14 after manual feedback.

### P14 — Acquire and inspect one building sample

Type: **implementation** · Depends on: **P09**

```text
Execute P14. Fetch or import one bounded official building sample using the access route verified in P09. Cache bytes and provenance. Inspect actual schema/version, geometry representations, CRS, vertical datum, geometry counts and external references. Choose one C#/.NET parser strategy for the observed input, documenting its supported subset. Prefer built-in streaming XML facilities for XML input where sufficient; validate namespace, geometry and reference handling against the actual file. Do not introduce a second-language converter. Add a small synthetic building fixture; keep any real test fixture only if its redistribution terms are recorded. Do not write a universal CityGML parser.
```

Completion check: The sample can be inspected offline; unsupported geometry and external references are reported, and the selected buildings overlap the road region.

### P15 — Convert and display a few real buildings

Type: **implementation** · Depends on: **P14, P13**

```text
Execute P15. Convert a handful of nearby buildings into local-coordinate meshes with simple materials. Support the observed geometry subset; validate triangulation including holes where present, normals, winding and bounds. Report failed objects by source ID rather than silently dropping them. Use a documented footprint extrusion fallback only when needed, visibly labeled as approximate. Add simple building collision if cheap; avoid detailed concave collision. Place them beside the existing road using the shared origin. Do not add facade textures or process the whole sample.
```

Completion check: Compare at least three building positions against source/reference data. Road and buildings align at correct scale; unsupported objects are listed. Record G3 results.

## Prompts: terrain and intersection

### P16 — Plan the terrain and elevation increment

Type: **planning** · Depends on: **P15**

```text
Execute P16 as a planning task. Inspect the real road and building results and current terrain data access. Create a bounded plan for P17–P19: terrain retrieval, terrain generation and road elevation fitting. Identify horizontal/vertical datum compatibility, resolution, missing-data handling and building base elevations. Explain why a heightfield cannot represent stacked roads, bridges or tunnels. Select a first route without those structures, documenting exclusions. Expand any broad task into 2–6 child cards with runnable acceptance criteria. Implement nothing.
```

Completion check: The plan contains explicit alignment checks, supported terrain format and a bounded route; it does not hide grade-separated roads by flattening them.

### P17 — Acquire one terrain patch

Type: **implementation** · Depends on: **P16**

```text
Execute P17 or its next approved child task. Acquire/import one terrain patch that covers the route with a small buffer. Record resolution, bounds, NoData semantics, CRS, vertical datum, retrieval date and checksum. Provide offline reading in the C# pipeline and a height query at documented sample points. Verify a .NET reader against the actual source format; prefer a managed library or a supported simpler format. If native raster bindings are unavoidable, create a separate bounded packaging/compatibility task instead of silently requiring an external GIS toolchain. Validate overlap with roads/buildings and report datum mismatches. Do not convert missing cells to zero silently or generate a city-scale terrain.
```

Completion check: Sample heights and units are plausible and traceable. Missing values and incompatible datums cause explicit warnings or a blocked conversion.

### P18 — Generate terrain mesh and collision

Type: **implementation** · Depends on: **P17, P08**

```text
Execute P18 or its next child task. Generate one modest-resolution terrain patch with normals, a simple material and bounded collision. Handle NoData explicitly and preserve the shared coordinate contract. Keep temporary flat ground behind a debug switch so alignment can be compared. Test known grid heights, bounds and seam-ready edge samples. Import the patch without road elevation fitting yet.
```

Completion check: Terrain scale and slopes look plausible. Sample locations match automated height checks; the car can stand on terrain without an unexplained offset.

### P19 — Fit the supported road segment to terrain

Type: **implementation** · Depends on: **P18, P12**

```text
Execute P19 or its next child task. Fit the supported ground-level road segment to terrain with controlled sampling and smoothing. Define how road collision relates to the render mesh and terrain collision to prevent fighting surfaces. Preserve cross-section consistency and limit implausible slopes; report where source data is insufficient. Verify nearby building bases independently rather than forcing every building to the same ground height. Keep bridges/tunnels out of this first fit.
```

Completion check: Drive the sloped segment without wheel/body penetration, excessive bumps or abrupt jumps. Debug views show terrain, road render surface and collision.

### P20 — Plan one real intersection

Type: **planning** · Depends on: **P19, P11**

```text
Execute P20 as a planning task. Select one simple same-level intersection on the route and inspect its graph and terrain. Design the smallest intersection patch that joins road strips without gaps or overlapping collision surfaces. Create 2–6 child implementation cards covering polygon construction, width transitions, elevation and collision checks as needed. Include a synthetic T-junction and a real-junction manual check. Defer lane-level AI, traffic rules, complex interchanges and curb detail. Implement nothing.
```

Completion check: Child tasks describe a watertight drivable intersection and a reproducible test, with clear limits on supported junction shapes.

### P21 — Implement and drive one intersection

Type: **implementation** · Depends on: **P20**

```text
Execute only the next ready child task from P20. Implement its bounded geometry or integration change and run its stated tests. Preserve source IDs and report unsupported junction cases. Do not implement other children in the same session. Once all required children have evidence, validate the parent with a drive across the real intersection in each direction and record G4 results.
```

Completion check: Each child has separate evidence. The parent passes only after the real junction is drivable without visible holes or collision steps.

## Prompts: tiles, performance and checkpoint

### P22 — Define tiles and generate two neighbors

Type: **implementation** · Depends on: **P21**

```text
Execute P22. Introduce a configurable fixed tile grid, provisionally 500 m squares, using the established projected origin. Generate only two adjacent tiles. Define object ownership, cross-boundary road/terrain clipping, border samples and stable object IDs so buildings are neither duplicated nor cut unexpectedly. Define shared edges consistently to prevent terrain cracks. Include a manifest with bounds, neighbors, checksums and asset version. Keep runtime loading static for now.
```

Completion check: Two statically loaded neighbors align. Automated boundary tests prove consistent edge heights and object ownership; a road crossing the border has no gap.

### P23 — Plan a bounded streaming lifecycle

Type: **planning** · Depends on: **P22**

```text
Execute P23 as a planning task. Plan a minimal runtime tile lifecycle: request, load, attach, active, detach and release. Keep data conversion offline and engine object creation on a supported thread. Define near-player collision retention, load-before-unload, a safety policy when destination tiles are unavailable, cancellation, resource cleanup and a small bounded memory budget. Create 2–6 implementation cards with a two-tile crossing fixture. Defer large LOD systems and floating origin until measurements justify them. Implement nothing.
```

Completion check: The plan explicitly prevents driving into an unloaded collision area and includes a repeated-crossing resource test.

### P24 — Implement and test tile crossing

Type: **implementation** · Depends on: **P23**

```text
Execute only the next ready child task from P23. Add the narrow lifecycle behavior described by that card and verify it using the two-tile fixture. Report actual load times and memory observations where available. After all children are complete, cross the border repeatedly at low and higher speeds, pause on the border, force a missing-tile error and verify the fallback. Record G5 only with evidence. Do not expand to all Helsinki.
```

Completion check: Repeated crossings have no missing collision or uncontrolled resource growth. A missing tile causes the documented safe fallback and useful diagnostics.

### P25 — Make the prototype build reproducible

Type: **implementation** · Depends on: **P24**

```text
Execute P25. Add a documented command through the C# pipeline CLI, or a small .NET build entry point, that converts the selected cached sources, validates manifests, stages/imports assets and invokes Godot to export the current prototype. Keep orchestration in C# so no extra language runtime is required. Respect the engine import step and verified .NET export requirements. Verify from a clean output directory; separate download from offline build. Add visible dataset/asset attribution using recorded terms and produce a concise build manifest. Do not redistribute raw data without review or publish the game.
```

Completion check: A cached-source rebuild creates a runnable export with matching asset checksums where deterministic. Credits identify actual sources and approximate/fallback content.

### P26 — Measure the playable prototype

Type: **validation** · Depends on: **P25**

```text
Execute P26. Define a repeatable short drive route with fixed resolution and settings. Measure on the actual target machine where accessible: frame-time distribution, physics behavior, draw calls, geometry counts, memory and tile load stalls. Record hardware, export mode and sample duration. Treat 60 FPS at 1080p as a provisional target, not a guaranteed result. Select the largest observed bottleneck and create one focused optimization task; do not optimize unmeasured systems.
```

Completion check: The report separates measured results from unavailable metrics. The proposed optimization has before/after acceptance criteria.

### P27 — Package the first playable checkpoint

Type: **validation** · Depends on: **P26**

```text
Execute P27. Review G0–G6 evidence, unresolved limitations, data provenance and attribution. Write short run instructions, known issues and the tested route. Produce a local prototype package only when the export and applicable redistribution review are complete; otherwise describe the exact blocker and retain a reproducible build recipe. Update PROJECT_STATE with the next expansion choices. Do not publish, upload to third-party stores or claim full Helsinki coverage.
```

Completion check: The local package or build recipe is usable by another developer. Open risks and unperformed checks remain visible. G6 reflects measured evidence.

## Reusable prompts for expansion and continuation

### Resume in a fresh Codex session

```text
Read AGENTS.md, HELSINKI_DRIVING_PLAN.md, docs/PROJECT_STATE.md,
docs/DECISIONS.md and docs/tasks/INDEX.md. Inspect the working tree and
relevant existing code. Summarize current evidence, open manual checks and
blockers. Select the next ready task in dependency order and state its ID.
Execute only that task under the execution contract. If the next task awaits
my manual check, work only on an independent ready task; do not assume it passed.
Update the ledger and state, and end with one recommended next task ID.
```

### Expand a task before implementation

Replace `<TASK_ID>` and `<CONSTRAINTS>` before sending.

```text
Expand <TASK_ID> using current repository evidence and <CONSTRAINTS>.
This is planning only. Inspect actual inputs and dependencies first.
Produce 2–6 child task cards with stable IDs, one observable goal each,
specific outputs, explicit limits and automated/manual acceptance criteria.
Use synthetic fixtures for geometry and lifecycle behavior. Identify the
smallest end-to-end experiment first. Include a dependency list and the
parent completion rule. Record unresolved questions and sensible defaults.
Update the index and state. Do not implement children or enlarge project scope.
End with the exact prompt for the first ready child task.
```

### Execute a generated child task

```text
Execute <CHILD_TASK_ID> only. Read its parent card and repository instructions.
Preserve unrelated work. Implement the smallest change satisfying its scope,
run the relevant checks and update its evidence. If implementation reveals a
new independent feature, create a follow-up card rather than including it.
Do not close the parent until all required children and parent checks pass.
Finish with the manual check, if needed, and one recommended next task ID.
```

### Record my manual test feedback

```text
I tested <TASK_ID> using <BUILD_OR_COMMIT_IDENTIFIER>.
Actions: <what I did>.
Observed result: <what happened>.
Expected result: <what I expected>.
Environment: <OS, hardware/settings if relevant>.
Record this feedback accurately. If it passes the task's manual criteria,
update status and recommend the next ready task. If it fails, investigate
only this defect and create or execute one focused fix task. Keep unrelated
features deferred. Do not treat incomplete observations as a complete pass.
```

### Diagnose a failed checkpoint

```text
Investigate the failure in <TASK_ID>: <error or observation>.
Reproduce it with the smallest existing fixture or add one targeted fixture.
Separate likely causes from verified causes. Prioritize coordinate/units,
input validity and collision/import settings where relevant. Fix the
smallest verified cause, rerun the failed check, and update evidence.
If this environment cannot reproduce it, add actionable diagnostics and
exact local reproduction steps. Do not redesign the pipeline speculatively.
```

### Change a prior decision without losing progress

```text
Review decision <DECISION_ID> in light of <new requirement or evidence>.
Inspect affected tasks and code. Write a short migration proposal with the
smallest validating spike, expected affected files and acceptance criteria.
Preserve reusable data contracts and fixtures where practical. Mark obsolete
tasks superseded, never silently done. This is planning only; do not perform
the migration until its task is selected.
```

### Plan the next phase after a gate

```text
Review evidence for <GATE_ID> and current PROJECT_STATE. Identify the most
important unsolved risk and propose at most three possible next increments.
Recommend one based on observed limitations and player value. Expand only
that increment into 2–6 child cards, keeping its area and features bounded.
Do not implement them. Provide a copyable prompt for the first experiment.
```

## Later expansion modules

These are planning entry points, not required work for the first prototype. Select one at a time after G6. Each prompt must produce child cards and a first-experiment prompt, rather than implementing the feature immediately.

| ID | Module | Select when |
|---|---|---|
| E01 | Larger district and LOD | Current route works and measurements show how much more geometry fits |
| E02 | Better vehicle handling | Driving feel is the limiting factor |
| E03 | Road markings and curbs | Street alignment and junction geometry are stable |
| E04 | Tram infrastructure | The chosen route needs recognizable Helsinki detail |
| E05 | Bridges and tunnels | Ground-level roads are reliable and a specific structure is selected |
| E06 | Minimal AI traffic | Stable lane topology and signals can be added to one junction |
| E07 | Floating origin | Measured precision issues appear as the playable area grows |
| E08 | Visual landmarks | A recognizable route already plays well |
| E09 | Public distribution | A tested package and source/license inventory exist |

### E01 — Expand area and add measured LOD

```text
Plan E01 only. Inspect measured rendering and tile-load limits. Choose one
adjacent expansion, not the whole city. Create child tasks for source
coverage checks, a low-detail distant representation and a repeatable drive
benchmark. Define switching distances from evidence and include visual pop,
collision retention and asset memory tests. Keep full-city acquisition deferred.
```

### E02 — Improve driving feel

```text
Plan E02 only. Inspect the current controller and recorded driving feedback.
Choose one handling problem, such as cornering or slope response. Compare a
small tuning change with a bounded physics spike on the existing test track.
Create child tasks with repeatable acceleration, braking, turning and reset
checks. Do not add realistic tire simulation solely for technical interest.
```

### E03 — Add road markings and sidewalks

```text
Plan E03 only for one short route. Audit which widths, lane counts and curb
positions are known and which are inferred. Create small tasks for a single
road cross-section, one marking style and one junction transition. Verify
terrain alignment and prevent z-fighting. Label generated assumptions; do
not claim lane-level accuracy from road centerlines alone.
```

### E04 — Add tram infrastructure

```text
Plan E04 only for one visible tram corridor. Verify available track geometry,
source terms and relationship to the road surface. Create child tasks for
static tracks and a small number of poles or wires. Use a bounded manual
comparison. Moving trams and their traffic interaction remain a later module.
```

### E05 — Handle one bridge or tunnel

```text
Plan E05 around one named structure. Inspect actual elevation and semantic
data before choosing geometry. Separate road topology from render/collision
surfaces; do not derive stacked roads solely by draping a heightmap. Create
child tasks for one geometry spike, connections to ground-level roads and a
safe drive-through. Document missing survey/elevation information and fallback.
```

### E06 — Introduce minimal traffic

```text
Plan E06 only for one junction. Audit whether the current graph contains the
lane-level paths and priority rules needed; centerlines alone are insufficient.
Create a bounded plan for lane paths, one AI vehicle and one simple control
rule. Include wrong-direction, conflict, deadlock and tile-boundary tests.
Defer crowds, city-wide traffic and realistic traffic simulation.
```

### E07 — Evaluate a floating origin

```text
Plan E07 only if a reproducible precision problem exists. Measure it first
and compare a smaller local coordinate range with a floating-origin shift.
Create child tasks covering physics, camera, tile positions, persistent
geographic position and reset/save semantics. Test repeated shifts without
velocity changes or tile misalignment. Do not shift every system speculatively.
```

### E08 — Improve a landmark area

```text
Plan E08 for one named landmark or street corner already reachable in game.
Choose the smallest visual improvement with high recognition value. Verify
asset rights and source provenance. Create child tasks for one replaceable
asset, its LOD/collision and reference comparison. Keep custom work separate
from generated geometry so the offline build does not overwrite it.
```

### E09 — Prepare for public distribution

```text
Plan E09 only. Inventory code, source datasets, derivative outputs, libraries
and art actually included in the package. Verify current provider/license
requirements using primary sources, and record unresolved questions about
database derivatives and redistribution. Create tasks for attribution,
source/asset separation, a clean-machine run and a release checklist. Seek
specific clarification when interpretation is unresolved. Non-commercial
status is not a blanket exemption. Do not publish anything in this task.
```

## Persistent state template

P00 should create this as `docs/PROJECT_STATE.md` and maintain it after each task.

```markdown
# Project state
Goal: Small drivable Helsinki desktop prototype
Current gate: G0
Last completed task: P00
Next recommended task: P01

## Verified environment
- Godot .NET edition / .NET SDK / game and pipeline target frameworks: not yet verified
- Editor OS / pipeline OS: WSL2 Linux by user requirement; target OS: Windows desktop; tool execution not yet verified
- Required language: C# for game, offline pipeline and automated tests
- Reference hardware: unknown

## Selected world configuration
- Region bounds / source CRS: not selected
- Vertical datum: unverified
- Geographic origin / engine axis mapping: not selected
- Tile size: provisional 500 m
- Supported structures: ground-level roads initially

## Evidence
- Build/export: not run
- Automated checks: not run
- Manual checks: not run
- Performance baseline: not measured

## Current blockers and limits
- <specific issue or none>

## Awaiting manual tests
- <task ID, tested build ID, steps, expected result>

## Decisions and source registry
- <links to current authoritative files>

## Next session
- <selected task ID and exact command/prompt reference>
```

## Important technical risks to keep visible

- **Alignment:** source axis order, geographic/projected coordinates, engine handedness and vertical datum must agree. A plausible-looking scene can still be mirrored or vertically offset.
- **Data parsing:** real building files may contain holes, complex surfaces, XLinks or unsupported geometry. Support observed input incrementally and count rejected objects.
- **Road geometry:** widths and lanes are often incomplete. Intersections require their own mesh work; buffering every centerline independently creates overlaps.
- **Elevation:** a heightmap cannot represent stacked surfaces. Bridges/tunnels need explicit geometry and connectivity.
- **Collision:** attractive rendering does not prove safe driving. Keep collision meshes simple and test surface transitions.
- **Tile boundaries:** render geometry, terrain samples and collision retention must agree. Never unload the only collision under the car.
- **Performance:** data conversion cost, editor import cost and runtime cost are different. Measure each before expanding.
- **Reproducibility:** service endpoints and source datasets can change. Keep checksums, bounded cached samples and synthetic fixtures.
- **Licensing:** verify each actual product and asset. Record attribution and investigate redistribution questions before packaging public datasets.

## Reference starting points

These are discovery links, not guarantees that a particular dataset/version or endpoint will remain available. P09 must verify the selected product and terms. Do not copy undocumented assumptions from the earlier conversation into the source registry.

- Helsinki 3D: https://www.hel.fi/en/decision-making/information-on-helsinki/maps-and-geospatial-data/helsinki-3d
- Helsinki open geographic data: https://www.hel.fi/en/decision-making/information-on-helsinki/maps-and-geospatial-data/make-better-use-of-geospatial-data/open-geographic-data
- Godot stable documentation: https://docs.godotengine.org/en/stable/
- Godot stable C# documentation: https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html
- Godot stable export documentation: https://docs.godotengine.org/en/stable/tutorials/export/index.html
- OpenStreetMap copyright and license overview: https://www.openstreetmap.org/copyright
- OpenStreetMap Foundation licensing guidance: https://osmfoundation.org/wiki/Licence

The plan intentionally leaves detailed .NET libraries, exact region coordinates and terrain-product choice to early verified research tasks. C#/.NET is the required implementation stack for both game and pipeline. Its completion criteria are fixed around observable behavior, while implementation choices can evolve.
