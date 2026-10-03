# Task index

Updated: 2026-10-03. P00 and P01 are complete. Deferred is a backlog disposition, not a completion status: all P09–P27 remain `todo`. Dependencies are copied from the plan; acceptance evidence is required before a dependency counts as complete. P01 research completion documents required installs; it does not certify a runnable toolchain.

| ID | Task | Type | Status | Dependencies | Disposition |
|---|---|---|---|---|---|
| <a id="p00"></a>P00 | [Bootstrap the planning workspace](../../HELSINKI_DRIVING_PLAN.md#p00--bootstrap-the-planning-workspace) | planning | done | none | Bootstrap complete |
| <a id="p01"></a>P01 | [Verify the toolchain and choose the first slice](P01-toolchain.md) | research | done | [P00](#p00) | Research complete; selected tools exercised by P02 |
| <a id="p02"></a>P02 | [Launch and export an empty 3D scene](P02-empty-scene.md) | implementation | awaiting-manual-test | [P01](#p01) | WSL build/export and Linux/Windows startup pass; visual confirmation pending |
| <a id="p02a"></a>P02a | [Script the current Windows export and transfer](P02a-windows-export-script.md) | implementation | done | [P01](#p01), verified [P02](#p02) export configuration | User-requested helper; 188 runtime files exported/transferred to W: with SHA-256 verification; P25 remains todo |
| <a id="p03"></a>P03 | [Move a simple car on flat ground](P03-car.md) | implementation | awaiting-manual-test | [P02](#p02) | Build/import/export and 30/60/144-cap integration checks pass; two-minute manual drive pending |
| <a id="p04"></a>P04 | [Add a usable chase camera](P04-chase-camera.md) | implementation | awaiting-manual-test | [P03](#p03) | Build/import/export/transfer and camera checks pass; turning/wall visual check pending |
| <a id="p05"></a>P05 | [Add collision and safe recovery](P05-recovery.md) | implementation | awaiting-manual-test | [P04](#p04) | Obstacles/ramp, R/fall recovery, camera reset and focused impact-speed correction; build/import/checks/export/transfer pass; manual acceptance pending |
| <a id="p06"></a>P06 | [Create the offline pipeline CLI](P06-pipeline-cli.md) | implementation | todo | [P01](#p01) | Card prepared |
| <a id="p07"></a>P07 | [Specify and verify coordinate transforms](P07-coordinates.md) | implementation | todo | [P06](#p06) | Card prepared |
| <a id="p08"></a>P08 | [Import one generated GLB and manifest](P08-synthetic-import.md) | implementation | todo | [P02](#p02), [P07](#p07) | Card prepared |
| <a id="p09"></a>P09 | [Verify data sources and select a bounded area](../../HELSINKI_DRIVING_PLAN.md#p09--verify-data-sources-and-select-a-bounded-area) | research | todo | [P07](#p07) | Deferred; expand before execution |
| <a id="p10"></a>P10 | [Acquire and inspect a small road sample](../../HELSINKI_DRIVING_PLAN.md#p10--acquire-and-inspect-a-small-road-sample) | implementation | todo | [P09](#p09) | Deferred; expand before execution |
| <a id="p11"></a>P11 | [Normalize a small road graph](../../HELSINKI_DRIVING_PLAN.md#p11--normalize-a-small-road-graph) | implementation | todo | [P10](#p10), [P07](#p07) | Deferred; expand before execution |
| <a id="p12"></a>P12 | [Generate one flat road strip](../../HELSINKI_DRIVING_PLAN.md#p12--generate-one-flat-road-strip) | implementation | todo | [P11](#p11), [P08](#p08) | Deferred; expand before execution |
| <a id="p13"></a>P13 | [Drive on the first Helsinki road](../../HELSINKI_DRIVING_PLAN.md#p13--drive-on-the-first-helsinki-road) | implementation | todo | [P05](#p05), [P12](#p12) | Deferred; expand before execution |
| <a id="p14"></a>P14 | [Acquire and inspect one building sample](../../HELSINKI_DRIVING_PLAN.md#p14--acquire-and-inspect-one-building-sample) | implementation | todo | [P09](#p09) | Deferred; expand before execution |
| <a id="p15"></a>P15 | [Convert and display a few real buildings](../../HELSINKI_DRIVING_PLAN.md#p15--convert-and-display-a-few-real-buildings) | implementation | todo | [P14](#p14), [P13](#p13) | Deferred; expand before execution |
| <a id="p16"></a>P16 | [Plan the terrain and elevation increment](../../HELSINKI_DRIVING_PLAN.md#p16--plan-the-terrain-and-elevation-increment) | planning | todo | [P15](#p15) | Deferred; expand before execution |
| <a id="p17"></a>P17 | [Acquire one terrain patch](../../HELSINKI_DRIVING_PLAN.md#p17--acquire-one-terrain-patch) | implementation | todo | [P16](#p16) | Deferred; expand before execution |
| <a id="p18"></a>P18 | [Generate terrain mesh and collision](../../HELSINKI_DRIVING_PLAN.md#p18--generate-terrain-mesh-and-collision) | implementation | todo | [P17](#p17), [P08](#p08) | Deferred; expand before execution |
| <a id="p19"></a>P19 | [Fit the supported road segment to terrain](../../HELSINKI_DRIVING_PLAN.md#p19--fit-the-supported-road-segment-to-terrain) | implementation | todo | [P18](#p18), [P12](#p12) | Deferred; expand before execution |
| <a id="p20"></a>P20 | [Plan one real intersection](../../HELSINKI_DRIVING_PLAN.md#p20--plan-one-real-intersection) | planning | todo | [P19](#p19), [P11](#p11) | Deferred; expand before execution |
| <a id="p21"></a>P21 | [Implement and drive one intersection](../../HELSINKI_DRIVING_PLAN.md#p21--implement-and-drive-one-intersection) | implementation | todo | [P20](#p20) | Deferred; expand before execution |
| <a id="p22"></a>P22 | [Define tiles and generate two neighbors](../../HELSINKI_DRIVING_PLAN.md#p22--define-tiles-and-generate-two-neighbors) | implementation | todo | [P21](#p21) | Deferred; expand before execution |
| <a id="p23"></a>P23 | [Plan a bounded streaming lifecycle](../../HELSINKI_DRIVING_PLAN.md#p23--plan-a-bounded-streaming-lifecycle) | planning | todo | [P22](#p22) | Deferred; expand before execution |
| <a id="p24"></a>P24 | [Implement and test tile crossing](../../HELSINKI_DRIVING_PLAN.md#p24--implement-and-test-tile-crossing) | implementation | todo | [P23](#p23) | Deferred; expand before execution |
| <a id="p25"></a>P25 | [Make the prototype build reproducible](../../HELSINKI_DRIVING_PLAN.md#p25--make-the-prototype-build-reproducible) | implementation | todo | [P24](#p24) | Deferred; expand before execution |
| <a id="p26"></a>P26 | [Measure the playable prototype](../../HELSINKI_DRIVING_PLAN.md#p26--measure-the-playable-prototype) | validation | todo | [P25](#p25) | Deferred; expand before execution |
| <a id="p27"></a>P27 | [Package the first playable checkpoint](../../HELSINKI_DRIVING_PLAN.md#p27--package-the-first-playable-checkpoint) | validation | todo | [P26](#p26) | Deferred; expand before execution |

## Bootstrap evidence

P00 created AGENTS.md, README.md, PROJECT_STATE.md, DECISIONS.md, TESTING.md, this index and P01–P08 task cards from the supplied plan. Documentation checks are recorded in [TESTING.md](../TESTING.md). No code project or dependencies were created. Manual game/editor checks remain not run. The optional instruction review is documented separately from runtime acceptance.

## Selecting work

Local setup follow-up (2026-10-03): SETUP.md step 3 now documents executable, Git-ignored `scripts/setup-godot-wsl.sh` for editor/template download and installation. Syntax, help, ignore and whitespace checks passed; actual downloads, archive layout, installation and editor execution remain unverified. P01 remains research-complete; P02 remains todo.

Local setup follow-up (2026-10-03): SETUP.md step 4 now documents the executable, Git-ignored `scripts/launch-godot-wsl.sh` helper. Bash syntax and ignore checks passed; execution stopped with actionable missing-editor guidance (exit 1). Actual editor launch remains unverified; P01 remains research-complete and P02 remains todo.

P02 implementation and command checks completed 2026-10-03; its visual acceptance remains open. The user explicitly requested P03 on 2026-10-03, authorizing implementation before that check. P03 now has automated driving evidence and a Windows export; its manual two-minute drive is pending. This does not close P02 or G0. Follow [RUN.md](../RUN.md), then select P04 after manual acceptance. P06 remains independently ready if selected. SDK 10.0.112, Godot 4.5.2 .NET, game net8.0 build/runtime and Windows cross-export are exercised (D014); P06 still plans net10.0. Execute one task per session.

P09–P27 retain the plan's prompts, dependencies and completion checks; create detailed cards from actual evidence before execution. P16 expands P17–P19 where necessary; P20 and P23 each plan 2–6 children. P21 and P24 execute only the next ready child per session, and their parents remain open until all required child and parent checks pass.

## Gates

P05 was explicitly selected on 2026-10-03 while prior manual evidence remained open. Its static obstacles/ramp and unified R/fall recovery are implemented; C# collision/recovery and existing driving/camera regression checks, build/import and Windows export/transfer pass. P05 awaits [manual testing](../RUN.md#p05-manual-test); G0/G1 remain open. Existing local P03/P04 card checkbox edits were preserved; this session adds no manual results for those tasks. Next recommended task: P06 (independently ready).

| Gate | Required tasks | Current evidence |
|---|---|---|
| G0: instructions and viable toolchain | P00–P02 | Open: build/import/export and native Windows startup pass; scene/export visual confirmation pending |
| G1: car, camera and recovery | P03–P05 | P03/P04 regressions and P05 collision/ramp/recovery checks pass; P05 implemented, manual gate acceptance remains open |
| G2: aligned synthetic pipeline assets | P06–P08 | Not run |
| G3: real road and buildings | P09–P15 | Not run |
| G4: terrain and intersection | P16–P21 | Not run |
| G5: tile crossing | P22–P24 | Not run |
| G6: repeatable export and performance | P25–P27 | Not run |

Statuses: `todo`, `in-progress`, `awaiting-manual-test`, `done`, `blocked`, `superseded`. Record automated and manual evidence separately. Do not close later tasks because scaffolding exists. Later E01–E09 expansion modules remain unselected planning entry points in the plan.
