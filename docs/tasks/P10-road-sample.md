# P10: Acquire and inspect a small road sample

Cache-selection correction (2026-10-05): the helper now observes the existing retrieval command's verified-cache output, independently verifies schema/size/SHA-256 and atomically selects its raw/provenance paths in graph configuration, preserving other values. Offline replay passed using the user's new payload; readiness exits 0. Fresh-download and failure paths remain unexercised in this correction. No generated-asset or gate changes.

Download-helper follow-up (2026-10-05): user requested `scripts/download-road-source.cs`. This C# wrapper locates the repository, restores locked pipeline dependencies and invokes the existing bounded cache-aware retrieval command, forwarding `--refresh` and child failures. It does not convert or rewrite graph configuration. Compile/help succeeds; live retrieval through the helper remains unverified. The environment checker now directs missing-cache users to it. Historical task acceptance/status is unchanged.

Status: done
Type: implementation
Dependencies: [P09](P09-data-sources.md)
Goal: One bounded explicit OSM retrieval/import command producing a verified raw cache and factual offline summary.
Inputs: P09 source registry/access failures, P07 projection, pinned .NET SDK.
Scope: Name-filtered Overpass JSON with complete way geometry; local JSON input; bounded requests, cache/provenance, raw tag summaries and synthetic parser tests.
Out of scope: Graph normalization, road generation, clipping, route/spawn selection, physical construction verification, game changes.
Deliverables: C# road-sample command, configuration, tiny authored fixture, targeted tests and usage/evidence.
Acceptance:
- [x] Bounded real sample retrieved or imported and parsed offline with checksum/provenance.
- [x] Request bounds are derived from the selected area; actual complete-way bounds and any outside vertices reported separately.
- [x] Counts and requested tag availability preserve strings and missing values.
- [x] Malformed/incomplete input fails clearly; synthetic parser and request/cache tests pass.
Evidence: 2026-10-04 one approved bounded Overpass POST returned HTTP200, 11930 bytes, 11 ways/26 unique nodes/36 vertices. Default cache replay and explicit real-file import passed without network; raw SHA-256 unchanged. All 50 tests pass; root Debug build passes with 0 warnings/errors. Full query/bounds/checksum and commands: [TESTING.md](../TESTING.md#bounded-road-retrieval-and-offline-inspection). Optional user CLI replay not performed; no visual acceptance is required for this acquisition/parser task. No driving/manual route evidence added.
Risks / blockers: Live access now verified for this request, not guaranteed future availability. JSON geometry cannot prove missing entire ways or real driving suitability; timestamps are not construction surveys. One full-way vertex outside request envelope is reported; no clipping or route endpoints selected. No roads generated. Prior visual gates remain open.
Follow-up tasks: P11.
