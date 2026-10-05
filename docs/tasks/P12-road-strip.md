# P12: Generate one flat road strip

Status: awaiting-manual-test
Type: implementation
Dependencies: [P11](P11-road-graph.md), [P08](P08-synthetic-import.md) implemented writer/staging; visual acceptance remains open
Goal: One short real curved road section with independently inspectable collision.
Inputs: Cached P11 graph, P09 origin, pinned P08 SharpGLTF writer.
Scope: Configured single-segment interior cut; width policy/assumptions; robust capped joins; flat asphalt and separate collision GLB; deterministic manifest and geometry tests.
Out of scope: Districts, junction surfaces, lane markings, terrain, car/spawn/scene integration (P13), retrieval/export.
Deliverables: C# offline command/config, GLBs/manifest under ignored staging, tests and reproducible instructions.
Acceptance:
- [x] One real interior curved section generated offline with width, flat-height and provenance report.
- [x] Duplicate/short edges and bends handled; join extent capped; bounds, winding and nondegenerate serialized triangles tested.
- [x] GLBs/manifest deterministic, collision separate and import verified at metre scale.
- [ ] Manual editor inspection: smooth bounded asphalt strip, no visible holes/spikes; collision independently inspectable.
Evidence: 2026-10-04: 78 tests pass (13 P12 cases, 65 regressions), full Debug solution build zero warnings/errors. Cached way 310376967 has an interior 100 m cut, eight points and 14 triangles, tagged 8.8 m width (ARCore source, not independently surveyed), synthetic flat Y=0. GLBs/manifest replay byte-identical; Godot imports visual and separate StaticBody3D/ConcavePolygonShape3D collision at identity/metre bounds, with valid winding and rays at every edge midpoint. [Commands/results](../TESTING.md#single-flat-road-strip), [schema/policy/source checks](../PIPELINE.md#single-flat-road-strip). Manual editor appearance/collision-gizmo inspection **not run**; [steps and expected result](../RUN.md#road-strip-inspection). No Windows export, drive or P13 scene changes.
Risks / blockers: P10 name-filtered graph lacks neighboring street/node context; ground-level suitability remains provisional. No surveyed width/elevation. Capped tight bends narrow intentionally; complex loops/hairpins/global self-intersection repair unsupported. Collision is an open static surface with no sides/thickness.
Follow-up tasks: P13.
