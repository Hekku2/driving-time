# P11: Normalize a small road graph

Status: done
Type: implementation
Dependencies: [P10](P10-road-sample.md), [P07](P07-coordinates.md)
Goal: A reproducible offline versioned EPSG:3067 road graph from the bounded P10 sample.
Inputs: Verified P10 raw JSON/provenance, configured bounds and P07 projection.
Scope: Preserve source IDs/geometry/tags/metadata/provenance; split shared OSM nodes; directional edges; rectangular clipping with distinct boundary endpoints; diagnostics and SVG inspection.
Out of scope: AI lanes, intersection surfaces, road meshes, elevations, game integration, retrieval or route suitability.
Deliverables: C# converter/CLI, graph config, focused tests, ignored graph JSON and debug SVG, schema/workflow documentation.
Acceptance:
- [x] Fixtures prove two-way/reversed one-way direction, shared junction splitting and separation of geometric crossings at different levels.
- [x] Clipping preserves source references and never creates junctions at artificial cuts; isolated and unsupported situations reported.
- [x] Offline real conversion verifies input checksum/provenance and preserves all source attributes and geometry in projected metres.
- [x] Deterministic graph and labeled debug plot make the real sample inspectable.
Evidence: 2026-10-04: 65 pipeline tests pass (15 P11 cases plus 50 regressions); full solution Debug build passes with zero warnings/errors. Offline real conversion and repeated JSON/SVG byte comparison pass: 11 source ways, 12 graph nodes, 11 segments, 22 arcs, one component, no isolated segments, one synthetic western boundary cut, 12 diagnostics. Raw checksum unchanged. Schema/workflow/source checks: [PIPELINE.md](../PIPELINE.md#p11-offline-intermediate-road-graph); actual commands/checksums: [TESTING.md](../TESTING.md#p11--offline-road-graph). Optional SVG appearance inspection not performed; acceptance requires an inspectable plot, not a visual driving check. No game/editor/manual driving check required for this intermediate-data task.
Risks / blockers: Name-filtered sample is incomplete context; no elevations or restriction relations. Approximate WGS84 datum policy must remain explicit.
Follow-up tasks: P12.
