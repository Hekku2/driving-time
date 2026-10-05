# Helsinki driving backlog

Updated: 2026-10-05. Implemented work is documented in [project state](docs/PROJECT_STATE.md), [run instructions](docs/RUN.md), [pipeline instructions](docs/PIPELINE.md) and [test results](docs/TESTING.md). This backlog contains only outstanding validation and future changes.

## Working rules

Follow [AGENTS.md](AGENTS.md), [current decisions](docs/DECISIONS.md) and the [task ledger](docs/tasks/INDEX.md). Execute one selected task per session; expand broad work into 2–6 useful children before implementation. Do not invent time estimates, close manual checks using automated evidence, or expand the area to conceal defects.

C#/.NET and WSL2 development/import/build/conversion/export are fixed requirements. Only exported runtime files transfer to Windows. The existing slice is one 100 m Stenbäckinkatu road; no buildings, terrain, junctions or streaming are implemented. Completed task prompts/cards and superseded decision history are removed. Retain unresolved acceptance work under stable IDs as validation tasks.

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

## Gates

| Gate | Current result / remaining check |
|---|---|
| G0 | Toolchain and historical startup/export checks pass; original exported scene appearance pending |
| G1 | Car/camera/collision/recovery implemented and automated checks pass; driving feel/manual checks pending |
| G2 | Synthetic asset generation/import/export checks pass; visual alignment pending |
| G3 | First real road implemented; manual road checks and nearby buildings pending |
| G4 | Terrain, elevation and intersection unimplemented |
| G5 | Tile generation and streaming unimplemented |
| G6 | Current reproducible export, target-machine measurements and package review pending |

## Outstanding validation

### P02 — Verify exported scene appearance

Status: **awaiting-manual-test** · Type: **validation** · Inputs: existing implementation

Launch the identified original floor/cube Windows artifact and confirm the scene is visible at the expected scale. Historical startup is automated evidence; visual acceptance remains open.

Steps: [manual instructions](docs/RUN.md#original-exported-scene-check). Record the actual scene/artifact/platform, actions and result. Implementation prompts have been removed; do not reimplement the feature.

### P03 — Check car driving feel

Status: **awaiting-manual-test** · Type: **validation** · Inputs: existing implementation

Drive, stop and reverse the synthetic vehicle/baseline scene. Record controls and jitter against the scene/platform tested.

Steps: [manual instructions](docs/RUN.md#vehicle-driving-comparison). Record the actual scene/artifact/platform, actions and result. Implementation prompts have been removed; do not reimplement the feature.

### P04 — Check chase camera appearance

Status: **awaiting-manual-test** · Type: **validation** · Inputs: existing implementation

Turn, stop, reverse and approach the camera wall. Record clipping, lag and heading behavior.

Steps: [manual instructions](docs/RUN.md#chase-camera-manual-test). Record the actual scene/artifact/platform, actions and result. Implementation prompts have been removed; do not reimplement the feature.

### P05 — Check collision and recovery manually

Status: **awaiting-manual-test** · Type: **validation** · Inputs: existing implementation

Hit obstacles, drive the ramp, leave the floor and press R. Confirm safe recovery without retained motion.

Steps: [manual instructions](docs/RUN.md#collision-and-recovery-manual-test). Record the actual scene/artifact/platform, actions and result. Implementation prompts have been removed; do not reimplement the feature.

### P05a — Check chosen vehicle behavior

Status: **awaiting-manual-test** · Type: **validation** · Inputs: existing implementation

Confirm VehicleBody3D steering, suspension and recovery on the loop/ramp. Controller choice is settled; detailed driving observations remain open.

Steps: [manual instructions](docs/RUN.md#vehicle-driving-comparison). Record the actual scene/artifact/platform, actions and result. Implementation prompts have been removed; do not reimplement the feature.

### P08 — Inspect synthetic asset alignment

Status: **awaiting-manual-test** · Type: **validation** · Inputs: existing implementation

Inspect the 10 m cube and cardinal/up markers in the editor and the identified Windows artifact. Confirm scale, directions and appearance.

Steps: [manual instructions](docs/RUN.md#synthetic-asset-inspection). Record the actual scene/artifact/platform, actions and result. Implementation prompts have been removed; do not reimplement the feature.

### P12 — Inspect road strip and collision

Status: **awaiting-manual-test** · Type: **validation** · Inputs: existing implementation

Inspect continuous flat asphalt without holes/spikes and matching independently inspectable collision. Width/elevation accuracy remains unverified.

Steps: [manual instructions](docs/RUN.md#road-strip-inspection). Record the actual scene/artifact/platform, actions and result. Implementation prompts have been removed; do not reimplement the feature.

### P13 — Manually drive the first Helsinki road

Status: **awaiting-manual-test** · Type: **validation** · Inputs: existing implementation

Drive START → MID → END, brake before the open end and press R. Confirm spawn, reference alignment, road support, camera and reset. Windows export was skipped by request.

Steps: [manual instructions](docs/RUN.md#first-helsinki-road-drive). Record the actual scene/artifact/platform, actions and result. Implementation prompts have been removed; do not reimplement the feature.

## Future building work

### P14 — Acquire and inspect one building sample

Type: **implementation** · Depends on: **verified source registry and access guidance**

```text
Execute P14. Fetch or import one bounded official building sample using the access route documented in docs/DATA_SOURCES.md. Cache bytes and provenance. Inspect actual schema/version, geometry representations, CRS, vertical datum, geometry counts and external references. Choose one C#/.NET parser strategy for the observed input, documenting its supported subset. Prefer built-in streaming XML facilities for XML input where sufficient; validate namespace, geometry and reference handling against the actual file. Do not introduce a second-language converter. Add a small synthetic building fixture; keep any real test fixture only if its redistribution terms are recorded. Do not write a universal CityGML parser.
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

Type: **implementation** · Depends on: **P17, existing GLB staging workflow**

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

Type: **planning** · Depends on: **P19, existing normalized road graph**

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
Review the current decision <DECISION_NAME> in light of <new requirement or evidence>.
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

Maintain `docs/PROJECT_STATE.md` as current results and open checks after each task.

```markdown
# Project state
Goal: Small drivable Helsinki desktop prototype
Current gate: G0
Current capabilities: <implemented results>
Next recommended task: <one pending task ID>

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

These are discovery links, not guarantees that a particular dataset/version or endpoint will remain available. Verify the selected product and current terms before retrieval. Do not copy undocumented assumptions from the earlier conversation into the source registry.

- Helsinki 3D: https://www.hel.fi/en/decision-making/information-on-helsinki/maps-and-geospatial-data/helsinki-3d
- Helsinki open geographic data: https://www.hel.fi/en/decision-making/information-on-helsinki/maps-and-geospatial-data/make-better-use-of-geospatial-data/open-geographic-data
- Godot stable documentation: https://docs.godotengine.org/en/stable/
- Godot stable C# documentation: https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html
- Godot stable export documentation: https://docs.godotengine.org/en/stable/tutorials/export/index.html
- OpenStreetMap copyright and license overview: https://www.openstreetmap.org/copyright
- OpenStreetMap Foundation licensing guidance: https://osmfoundation.org/wiki/Licence

The plan intentionally leaves detailed .NET libraries, exact region coordinates and terrain-product choice to early verified research tasks. C#/.NET is the required implementation stack for both game and pipeline. Its completion criteria are fixed around observable behavior, while implementation choices can evolve.
