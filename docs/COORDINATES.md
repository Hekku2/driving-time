# P07 coordinate contract

Version 1, checked 2026-10-03. Implementation: `pipeline/Coordinates.cs` and `pipeline/Tm35FinProjection.cs`; fixtures: `tests/DrivingTime.Pipeline.Tests/CoordinateTests.cs`. This contract prepares P08/P09; the P06 v1 CLI still accepts only synthetic configurations and emits its existing manifest. No game or mesh import change is implied.

## Source and normalized coordinates

Choose EPSG:3067 (ETRS-TM35FIN, EUREF-FIN / TM35FIN(E,N)) as the horizontal working CRS. Its current registry name is ETRS89-FIN [EUREF-FIN] / TM35FIN(E,N). Use N2000 (EPSG:3900) normal heights in metres for real elevated assets. EPSG:3067 alone is two-dimensional and specifies no vertical datum. P09 must verify actual datasets against this target; choosing this contract does not prove their compatibility.

| Input | Source axis order and units | Adapter behavior |
|---|---|---|
| EPSG:3067 native order | easting, northing; metres | `SourceAxes.Projected(..., EastNorth)` |
| EPSG:5048 native order | northing, easting; metres | `SourceAxes.Projected(..., NorthEast)`; reorder to E,N |
| EPSG:4326 native order | latitude, longitude; decimal degrees | `SourceAxes.Geographic(..., LatitudeLongitude)` |
| RFC 7946 GeoJSON / OGC CRS84 | longitude, latitude; decimal degrees | `SourceAxes.Geographic(..., LongitudeLatitude)` |
| EUREF-FIN geographic (EPSG:10690 native order) | latitude, longitude; decimal degrees, GRS80 | named `GeographicPoint`, then `FromEurefFin` |
| P06 synthetic | explicitly named east, north, height; metres | synthetic frame only; no geographic datum |

The importer must check CRS metadata, format/transport axis rules, units and a known control point before selecting an adapter. A field called X or a plausible coordinate magnitude is not evidence of east. A source format may override native CRS order: record the observed order and rule, not just the EPSG code. Geographic adapters do not infer a height from a third array element. Missing, ambiguous, unsupported CRS/axis/units are conversion errors until verified. Feet, radians, other projections and datum operations require a separately verified adapter. Preserve original source coordinates and normalized coordinates as C# doubles.

## Origin, axes and precision

No real origin or geographic bounds have been selected. P09 must record a traceable fixed E,N,H origin in EPSG:3067/N2000 from the selected bounded area, plus selection method and source/control evidence. It must be identical for all layers, tiles and manifests and persisted in configuration; do not recalculate it per object, tile or run. `CoordinateFrame` requires the caller to supply that origin and tile size and stores immutable values. P06's configured (0,0,0) is synthetic only. Large numbers in P07 fixtures deliberately exercise precision and are not origin recommendations. No Helsinki coordinates were invented or committed as configuration.

For normalized projected east, north and validated height:

```text
X = east   - originEast
Y = height - originHeight
Z = -(north - originNorth)

east   = originEast   + X
north  = originNorth  - Z
height = originHeight + Y
```

Engine units are metres: +X grid east, +Y up, -Z grid north (+Z south). These are projected grid directions, not true geodetic north/east; meridian convergence and projection scale remain. Godot uses a right-handed Y-up metre space. The matrix from ordered (east,north,up) to (X,Y,Z) is `[1,0,0; 0,0,1; 0,-1,0]`, determinant +1; east cross north maps to up. This is a rotation, so no handedness reflection is introduced. Vertex winding/normals still need P08's writer/import check.

Subtract origin in double precision before casting to engine/GLB floats. Keep projected/source coordinates out of float `Vector3`s. Tests show millimetre offsets survive localization while casting a multi-million-metre northing first loses centimetres. No floating-origin system or large-area float accuracy guarantee is added. Round-trip arithmetic tolerance is 1e-9 m horizontally for the fixtures; this is numerical precision, not survey accuracy. A known synthetic 60 m east / 80 m north segment measures 100 m in engine space. It verifies projected grid scale, not a surveyed ground/geodesic distance; no physical measurement was performed.

## Tile-local space

The tile grid is anchored at the same fixed projected origin; configured positive size S is in metres. Indices `(i,j)` use `floor((east-originEast)/S)` and `floor((north-originNorth)/S)` including negative values. Ownership uses half-open cells: the east/north edge belongs to the next tile. Southwest anchor is `(originEast+i*S, originNorth+j*S, originHeight)`.

Tile engine translation is `(i*S,0,-j*S)`. Local vertices subtract this translation from world-local engine coordinates; Y retains the common height origin. Thus local X lies in [0,S), local Z in (-S,0] for owned points. Recompose world position by adding tile translation. Border vertices may appear in both meshes and must recompose identically. P07 tests adjacent representations, exact edges, negative indices and height preservation; tile generation/streaming remains P22–P24.

## Managed transform evaluation and EPSG:4326 policy

Pin NuGet ProjNET 2.1.0 with lock-file content hashes. Package metadata supplies netstandard2.0/2.1 assemblies and LGPL-2.1-or-later licensing; local net10.0 restore/build/tests passed with SDK 10.0.112. It is used only by the offline pipeline, with no native GIS CLI/runtime dependency or shared game library. Preserve applicable license notices when distributing pipeline binaries; no pipeline redistribution occurred here.

This is a bounded adapter, not an EPSG database lookup. Explicit WKT1 definitions supply GRS80 (a=6378137 m, inverse flattening=298.257222101), Greenwich, degree units, Transverse_Mercator, latitude origin 0°, central meridian 27°, scale 0.9996, false easting 500000 m and false northing 0 m. Do not substitute a WGS84 UTM factory and claim EUREF-FIN equivalence. Adapter scope is latitude 59–71° and longitude 19–32°, a conservative rectangular prototype envelope, not the official area-of-use polygon.

The actual library interface takes longitude,latitude degrees and returns east,north metres even though the geographic WKT declares latitude first. The published asymmetric reference and inverse checks demonstrate this behavior for the pinned package. Normalize source order outside the library and explicitly pass longitude first; do not rely on WKT AXIS to reorder arrays. Tests verify the central meridian easting and inverse round trips at a Helsinki-like synthetic angle pair, the west edge beyond nominal UTM zone 35, and northern/eastern points. These synthetic pairs are not landmarks or origin selections.

Independent reference: FGI/NLS Tiedote 30, corrected 2009-12-10, example 10, printed pp.106–107 (PDF pages 109–110, zero-based). Published EUREF-FIN input latitude=1.102369021930 rad, longitude=0.372098448769 rad; E=214141.4227 m, N=7016196.1450 m. Executed ProjNET output E=214141.42267341953, N=7016196.144977237; residuals -0.0000265805/-0.0000227625 m. Forward acceptance is 0.01 m, inverse/angular round trips 1e-7 degree. This validates the required projection and axis semantics against published calculations, not just self-consistency or a package name. No reference GIS program was installed or run. Only these small cited numerical facts are embedded in tests; no PDF/source dataset is redistributed.

For EPSG:4326, C# normalizes source axes and calls `FromWgs84`. Default rejects conversion. `allowApproximateDatum: true` deliberately treats the WGS84 angles as EUREF-FIN before GRS80 projection, suitable only if the later source review explicitly accepts a metre-level prototype approximation and records that policy in provenance. This is not a precision/epoch-aware WGS84-to-EUREF-FIN operation; its real-world error has not been measured. The NLS reference uses EUREF-FIN and does not validate that approximation. Preserve realization/epoch/accuracy metadata when present. If stricter accuracy is required, prefer supplied EPSG:3067/EUREF-FIN data or evaluate a managed epoch-aware transform separately. ProjNET's projection support does not establish NKG2020, grid shifts, arbitrary EPSG support or vertical transformation support.

## Vertical validation before real conversion

`CoordinateFrame` allows only an explicitly synthetic frame or N2000 frame; mismatched, unknown, N60 and ellipsoidal heights fail rather than silently offsetting them. Height is a separate argument and is never transformed by the horizontal adapter. N2000 metadata is a necessary input assertion, not proof that supplied values are correct.

P09/P14/P17 must record height units, vertical CRS/datum, height type (normal, ellipsoidal, relative building height), NoData rules and provenance. Verify at independent known elevation controls/benchmarks, compare overlapping road/building/terrain locations, record residuals and define acceptable tolerance from source accuracy/resolution before accepting data. Missing/NaN/infinite elevations are errors; no zero-height fallback is implicit. A flat demonstration must be labeled synthetic/approximate and kept separate from validated real elevations. Building roof height above ground is a dimension, not an absolute N2000 elevation.

N60 or ellipsoidal heights require a verified conversion and separately tested grid/model interpolation, units, coverage and NoData handling before use. NLS documents FIN2023N2000 for EUREF-FIN ellipsoidal to N2000 heights; WGS84 heights first require compatible realization/epoch handling. Fetch/license/checksum/model packaging remain future bounded work; no geoid grid was downloaded or height transformation validated in P07. Reject unsupported sources rather than treating N2000, N60 and ellipsoidal heights as interchangeable. A RFC 7946 third coordinate is WGS84 ellipsoidal height, not N2000.

## Sources and review

Official sources checked 2026-10-03:

- [NLS EPSG codes and current datum/axis/height-model definitions](https://www.maanmittauslaitos.fi/kartat-ja-paikkatieto/koordinaatit-ja-paikannus/epsg-koodit-ja-proj-muunnosohjelma).
- [FGI/NLS Tiedote 30 reference calculations](https://www.maanmittauslaitos.fi/sites/maanmittauslaitos.fi/files/fgi/GLtiedote30.pdf), example 10.
- [ProjNET upstream scope and supported projections](https://github.com/NetTopologySuite/ProjNet4GeoAPI) and [2.1.0 package](https://www.nuget.org/packages/ProjNET/2.1.0); installed nuspec and local tests verify actual dependency compatibility. Tagged implementation-source URLs were unavailable through browsing; no claim of source-code verification.
- [Godot 4.5 axes and scale](https://docs.godotengine.org/en/4.5/tutorials/3d/introduction_to_3d.html).
- [RFC 7946 positions and height semantics](https://www.rfc-editor.org/rfc/rfc7946) and [OGC CRS/encoding rules](https://docs.ogc.org/is/18-058r1/18-058r1.html). Direct EPSG:4326 registry page retrieval was unavailable; transport axis order still requires actual-source review.

Manual review: read the axis table, origin rules and vertical validation checklist; expect no selected real origin, north toward -Z and explicit rejection of unsupported heights. Agent contract review completed; user replay not performed. No runtime visual check is required for P07. P08 must show cardinal markers, a 100 m scale and raised marker in Godot before G2 can pass.
