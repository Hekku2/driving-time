# Bounded building sample

The independent C# pipeline retrieves and inspects one official Helsinki building response. It does not generate meshes or load buildings into the game. Source access and terms are described in [DATA_SOURCES.md](DATA_SOURCES.md); dated results are in [TESTING.md](TESTING.md#bounded-building-retrieval-and-xml-inspection).

## Commands and cache

Run from the repository root, after restoring/building the solution:

```bash
dotnet build DrivingTime.sln --no-restore
dotnet run --project pipeline --no-build --no-restore -- building-sample --config config/building-sample.json
# Explicitly acquire a new snapshot:
dotnet run --project pipeline --no-build --no-restore -- building-sample --config config/building-sample.json --refresh
# Inspect an existing WFS response without network access:
dotnet run --project pipeline --no-build --no-restore -- building-sample --config config/building-sample.json --input /path/to/sample.xml
```

Default execution verifies the cached response or performs one HTTP GET when no cache exists. A corrupt cache fails; it is not silently replaced. Configuration controls native GK25 E,N request bounds, count, cache path and projected road discovery region. Requests are confined to a positive box of at most 200 × 200 m within the selected native square, with 1–5 buildings, a 4 MiB response cap and a 30 s timeout including body reading. The supplied configuration requests one building. The official endpoint is fixed. An identifying User-Agent, cache-root lock and persisted two-second request spacing bound retrieval. There are no automatic retries, redirects, page traversal or external-reference downloads.

Ignored `data/raw/buildings/<request-sha256>/<payload-sha256>/` holds untouched `raw.xml`, `network-provenance.json` or `import-provenance.json`, and `inspection.json`. The request hash covers `GET\n` plus the exact URL; the payload hash covers exact bytes. Network provenance records UTC retrieval, status/headers, provider, licence declaration/link/check date, axes/CRS/datum, size and checksum. Local imports record import time with original retrieval/status unknown. `latest.json` selects only a validated network snapshot; imports do not change it. Offline replay verifies its request identity, source checksum/size and agreement with the stored provenance, then re-inspects. Writes are atomic per file, not a multi-file transaction. Raw responses are not committed or redistributed.

## Selected XML subset

Use built-in `XmlReader` to stream WFS members and `XElement` to inspect one bounded feature at a time. No CityGML package, native GIS dependency or second-language converter is needed for the observed input. DTDs are prohibited, the XML resolver is disabled and document characters are capped. Namespace URIs are matched independently of prefixes. Schema locations are recorded, not fetched; this is structural inspection rather than XSD validation.

- Require WFS 2.0 `FeatureCollection` with direct `member` children containing CityGML building 2.0 `Building`, unique GML IDs and a consistent, nonempty `numberReturned` within the configured cap. Standalone `CityModel`, other versions and service exception documents are rejected.
- Inspect the observed CityGML core/building 2.0 and legacy GML `http://www.opengis.net/gml` namespace used by its GML 3.1 geometry model. Recognize LoD1/LoD2 `Solid` → `CompositeSurface` → `surfaceMember` → `Polygon`, and LoD2 semantic boundary `MultiSurface` polygons. Inventory counts retain expanded namespace names.
- Require the observed three-dimensional compound envelope CRS `urn:ogc:def:crs,crs:EPSG::3879,crs:EPSG::3900`. Physical tuple order is E,N,H. Positions without their own CRS use this explicit feature-envelope fallback; conflicting CRS/dimensions fail. Parse finite invariant-culture `posList` triples; rings with one inline list must contain at least four closed triples. Other ring encodings and unknown GML/LoD/building-part constructs produce diagnostics. Interior rings are counted; there is no triangulation, watertightness or topology validation.
- Inventory inline `pos` points and separate CityGML address positions from building geometry bounds. The real address has H=0; it is not a base/roof elevation control. Coordinate tuples are capped at 100,000 per feature.
- Resolve same-building fragment `surfaceMember` references to `Polygon` IDs after reading features, including forward references. Report unresolved fragments, external URLs, cross-building references, unsupported properties and wrong target types. Never dereference them over the network. Capture `core:externalReference` metadata separately from geometry XLinks.
- Transform four envelope corners with the existing managed ProjNET projection to assess overlap with the configured EPSG:3067 road region. This is a horizontal bounds check, not mesh conversion or vertical-datum validation. Preserve native bounds/heights and raw creation dates; do not infer survey dates or a timezone for the service timestamp.

The repository-authored [synthetic fixture](../tests/fixtures/citygml-building.xml) uses arbitrary namespace prefixes, inline and forward-referenced polygons, an address point and external metadata. Its geometry is intentionally small and is not a watertight solid. No real test fixture is retained in Git. Tests exercise malformed namespaces, dimensions, CRS, XML/DTD, tuples/rings, IDs/references, holes, unknown geometry, projection, bounds/caps, local imports and cache tampering. An inspection with diagnostics can succeed; those diagnostics must be reviewed before conversion.

## Observed official sample and limits

On 2026-10-05 the configured 200 × 200 m request returned one of 20 matched buildings, ID `BID_4c477677-cdf6-494a-a84b-6472939eb448`, in 26,638 bytes. There are two solids, 28 polygons/closed rings (8 LoD1, 20 LoD2), 156 building coordinate triples, no polygon holes and 20 same-building references, all resolved. One separate address point and one external metadata record occur. No external geometry or textures were fetched. LoD2 solid references polygons defined later in its semantic surfaces; selecting both LoDs would duplicate geometry.

Native geometry/envelope bounds agree: E=25495240.4..25495284.898, N=6675037.176..6675058.723, H=13.719..40.13 m. EPSG:3067 envelope bounds are E=384333.85835647007..384378.97784813977, N=6674189.754109229..6674212.633481642. These overlap the configured discovery road region, but sit south of the existing 100 m strip (approximately N=6674260..6674318). This establishes local source availability, not alignment beside the playable strip. A later conversion needs a bounded selection near that strip and independent base-height checks. Do not enlarge the world to conceal this placement limit.

Helsinki declares GK25/N2000 and CC BY 4.0 for this semantic model; the compound CRS is also present in the response. Heights remain provider-declared, without independent elevation controls. The sample is partial and service ordering/content can change. Roof/base precision, acquisition age, planar validity, normals, holes and conversion remain unverified.
