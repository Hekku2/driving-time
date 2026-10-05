# P09: Verify data sources and select a bounded Stenbäckinkatu area

Status: done
Type: research
Dependencies: [P07](P07-coordinates.md)
Goal: Traceable product choices, exact provisional bounds and a data-derived origin for one small Helsinki slice.
Inputs: AGENTS.md, HELSINKI_DRIVING_PLAN.md P09, PROJECT_STATE.md, DECISIONS.md, COORDINATES.md; user selects Stenbäckinkatu.
Scope: Official documentation and bounded access probes; source registry; route and retrieval/storage/fallback planning.
Out of scope: P10/P14/P17 acquisition or conversion implementations, city-wide downloads, game changes, redistribution or publication.
Deliverables: docs/DATA_SOURCES.md; config/source-registry.json; reproducible offline inspection of tiny research probes.
Acceptance:
- [x] Official Helsinki and OSM access/licensing guidance checked with URLs and dates.
- [x] Building and terrain formats, coverage, CRS, vertical datum, dates and terms distinguish metadata from actual probes.
- [x] Approximately 1 km square at Stenbäckinkatu, measured origin and short provisional ground-level route are traceable.
- [x] Bounded cached retrieval, service limits, storage estimates and manual fallbacks are planned; unknowns have named follow-ups.
- [x] Registry parses and measurement/whitespace checks pass.
Evidence: 2026-10-04 research complete. [DATA_SOURCES.md](../DATA_SOURCES.md), [registry](../../config/source-registry.json), [commands](../TESTING.md#bounded-source-research). Approved bounded probes returned one CityGML building with LoD1/2, two GeoJSON footprints, address origin in three CRS outputs and 4x4 Float32 GeoTIFF. Offline C# inspection passes: 1000m square, recorded origin/height and ten registered byte counts/hashes. No manual drive, graphical viewer download or route/vertical-control acceptance implied. No manual acceptance is required to close this research; future reference checks belong to P10/P17. P07 dependency is done; P08 and prior manual gates remain open.
Risks / blockers: Historical 50cm catalogue differs from served 1m terrain; axis labels and band units are inconsistent. Overpass 406/timeout: P10 manual export fallback or acquisition blocker. Footprint terms conflict: P14 resolution before fallback adoption. Construction state requires P10 inspection; independent N2000 control requires P17. These limit downstream acquisition, not the completed research record.
Follow-up tasks: P10, P14, P16/P17; no implementation authorized by this card.
