# P14: Acquire and inspect one building sample

Status: in-progress
Type: implementation
Dependencies: Existing verified CityGML access guidance/source registry.
Goal: One bounded official building response cached with provenance and inspectable offline.
Inputs: docs/DATA_SOURCES.md, config/source-registry.json, selected road region and independent .NET pipeline.
Scope: Bounded cache-aware retrieval/local import, observed XML namespace/geometry/CRS/reference report, supported parser subset and synthetic test fixture.
Out of scope: Mesh conversion/game buildings, universal CityGML support, new native dependencies, full-area downloads and redistribution.
Deliverables: C# command/config, ignored raw cache/provenance/summary, synthetic XML fixture/tests and current usage/source documentation.
Acceptance:
- [ ] Actual bounded official sample and network provenance captured; checksum-verified offline inspection passes.
- [ ] Namespace/schema, geometry/coordinates, horizontal/vertical CRS, counts, external/internal references and unsupported constructs reported.
- [ ] Supported C# XML subset tested against synthetic fixture and observed response; selected sample overlap with road region assessed explicitly.
Evidence: Not run.
Risks / blockers: GK25 localization is not yet implemented; N2000 is a provider declaration, not independent survey. Service/schema may change; source bytes remain ignored.
Follow-up tasks: P15, bounded building conversion after inspection evidence.
