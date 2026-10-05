# P13: Drive on the first Helsinki road

Status: awaiting-manual-test
Type: implementation
Dependencies: [P05](P05-recovery.md), [P12](P12-road-strip.md); implemented foundations, manual acceptance remains open. User explicitly selected implementation before those checks closed.
Goal: Drive the existing vehicle on the single staged Stenbäckinkatu road.
Inputs: Road GLBs/manifest, selected coordinate origin, VehicleCar and ChaseCamera, offline conversion/export workflow.
Scope: One dedicated scene, manifest-derived coordinate spawn and labeled reference positions, bounded temporary ground, staging/export integration and targeted physics checks.
Out of scope: Buildings, terrain/elevation, junctions, road expansion, controller redesign and closing prior visual gates.
Deliverables: Scene/C# setup, configuration, shared coordinate source, headless integration check, export staging, short manual route and evidence.
Acceptance:
- [x] Imported single visual/collision road retains identity placement; spawn/markers agree with projected manifest positions.
- [x] Vehicle wheels are supported by the road collision and drive/brake/reset checks pass.
- [x] Offline staging/build/import has evidence; export staging and resource filters prepared. Windows export explicitly skipped at user's request; packaged availability unverified.
- [ ] Manual route: correct spawn, aligned markers, continuous asphalt/collision, usable driving/camera and safe R reset.
Evidence: 2026-10-05: Debug solution build passes with zero warnings/errors; all 78 pipeline tests pass. Offline graph/strip regeneration passes (11 segments, one component, 100 m strip/14 triangles). Godot headless import passes after approved retry; road asset checks pass. HelsinkiRoadChecks passes 1,376 ticks, driving 6..90 m with maximum lateral error 0.200 m; imported road wheel contacts, reference/spawn alignment, braking, held R, fall recovery and camera verified. Shared coordinate source compiles for net8.0 game and net10.0 pipeline without a pipeline assembly dependency. Windows export skipped as requested; no native playtest or visual acceptance. See [dated results](../TESTING.md#first-helsinki-road-integration) and [manual route](../RUN.md#first-helsinki-road-drive).
Risks / blockers: Flat local Y is synthetic, width is source-tag-derived, road access/construction unverified. Prior manual checks remain open. No measured native performance.
Follow-up tasks: P14 after manual feedback; do not execute it in this session.
