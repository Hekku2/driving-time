# Data sources: Stenbäckinkatu

Initial source research checked **2026-10-04 (Europe/Helsinki)**; building access/terms and the bounded reader were checked again **2026-10-05**. No city-wide download or publication. The machine-readable research record is [source-registry.json](../config/source-registry.json); subsequent retrievals have their own cache provenance. `providerClaims` records documentation; `observed` records bounded responses and measurements. `null` means unverified, not zero. This registry is not the synthetic pipeline configuration and does not change its origin.

## Provisional area and measured origin

The user selected **Stenbäckinkatu**, replacing the plan's initial Töölönlahti-centred suggestion. The selected square covers the Stenbäckinkatu neighbourhood northwest of Töölönlahti; it is not an expansion to include the lake. Its size is exactly **1,000 × 1,000 projected metres** in EPSG:3067 (1 km²). Bounds below are design choices centred on a measured source point, not provider dataset bounds.

| Quantity | Value / derivation |
|---|---|
| West / east | 383736.0361374 / 384736.0361374 m |
| South / north | 6673710.89320437 / 6674710.89320437 m |
| Horizontal origin | E=384236.0361374, N=6674210.89320437 m, EPSG:3067 |
| Source control | `Helsinki_osoiteluettelo.45899`, street name Stenbäckinkatu, no house number; native GK25 E=25495142, N=6675054 |
| Geographic locator | Longitude 24.91244103°, latitude 60.18828858°, same feature requested in EPSG:4326 |
| Height origin | 13.52536678314209 m, provider-declared N2000; Float32 DEM value expanded to double |
| Height sample | Containing 1 m pixel, centre GK25 E=25495142.5, N=6675053.5; row 2, column 2 in the 4 × 4 probe |
| Initial tile size | 500 m, from the coordinate contract; no tiles generated yet |

The city WFS performed the GK25 → TM35FIN and geographic projections; this does **not** validate a local GK25 adapter. The address reference is an anchor, not a road-centre or spawn measurement. Its `paivitetty_tietopalveluun` is 2026-10-02, a publication field, not a survey date. The height uses the containing pixel; because the source point lies on grid boundaries, the inspector deliberately chooses the east/south cell. No interpolation or zero-height substitution occurs. Binary precision is not survey accuracy. Independent vertical control and terrain/road alignment remain unverified. Keep this provisional origin fixed across sources; changing it requires a versioned region, not a silent resample.

The short first run is **100–150 m inside eastern Stenbäckinkatu**, travelling from the Mannerheimintie end toward Töölöntullinkatu, stopping before junctions and resetting rather than constructing a loop. Validate the actual centreline and place both route endpoints at least 20 m inside supported street geometry. Length is a target, not a measured road length. Exclude Mannerheimintie/tram junctions, Topeliuksenkatu, the Messeniuksenkatu and Töölöntullinkatu junction patches, hospital access roads, bridges, tunnels and stacked/nonzero-layer carriageways. No full-area road network is needed.

The city's [street-plan description, drawing 32394/1](https://ptp.hel.fi/DataForms/katu_ja_puistosuunnitelmat/Download.ashx?q=d0428056/Selostus_6.3.2026_Stenb%C3%A4ckinkatu_Stenb%C3%A4ckinkatu_v%C3%A4lill%C3%A4_Topeliuksenkatu_-_Mannerheimintie_Selostus.pdf) is dated 2026-02-27 internally (the filename says 6.3.2026). It describes the eastern street and proposes raised junctions and a through-traffic cut east of Topeliuksenkatu, retaining ambulance/cycle passage. These are **plan claims**, not evidence that construction is finished. Do not model the proposed 5.5 m carriageway width as a measured existing width. Reference inspection must reject incompatible access/construction conditions or shorten the interior run within this same square. Route appearance, exact endpoints and driving remain unverified.

## Product choices and actual access

### Buildings: city information model through CityGML WFS

Select the semantic model, initially using LoD1 solids where available. The current [Helsinki 3D guidance](https://www.hel.fi/en/decision-making/information-on-helsinki/maps-and-geospatial-data/helsinki-3d) identifies CityGML 2.0, ETRS-GK25/N2000 and CC BY 4.0. It describes continuous updates, bounded viewer exports (without building characteristics) and direct 4 km² CityGML sheets (with characteristics). The [product metadata, ID 457](https://kartta.hel.fi/paikkatietohakemisto/pth/?id=457) claims Helsinki coverage except outer archipelago; its currency date is 2021-01-01 and metadata update 2021-04-12. Continuous maintenance is not an exact acquisition date for a downloaded building.

The [official open-data page](https://www.hel.fi/en/decision-making/information-on-helsinki/maps-and-geospatial-data/make-better-use-of-geospatial-data/open-geographic-data) links the urban-model interface at `https://kartta.hel.fi/3d/citydb-wfs/wfs`. Actual unauthenticated GetCapabilities and count=1 GetFeature requests succeeded. In a **200 × 200 m** native GK25 box around the anchor, the service reported **20 matches and returned one building** (26,638 bytes). This is local presence evidence, not a complete inventory of the 1 km² square.

Measured response:

- WFS 2.0 FeatureCollection, CityGML building/core 2.0 namespaces and GML 3.1 geometry. Both `lod1Solid` and `lod2Solid` occur; ordinary GML is not automatically CityGML.
- ID `BID_4c477677-cdf6-494a-a84b-6472939eb448`, coordinates **E,N,H**, CRS `urn:ogc:def:crs,crs:EPSG::3879,crs:EPSG::3900`.
- Native bounds E=25495240.4..25495284.898, N=6675037.176..6675058.723, H=13.719..40.13 m. The reported `measuredHeight=26.4` is an attribute, not our survey.
- Building creation date 2016-05-22; surface creation dates include 2024-01-04. Neither establishes imagery/survey date. No textures were fetched.
- An N,E bbox returned zero; physical E,N returned the building. Capabilities advertise a world bounding box: **do not interpret that as worldwide building coverage**. Paging is advertised but not transaction-safe; response cache timeout is 300 s.

The [bounded building command](BUILDINGS.md) now uses built-in streaming `XmlReader` with per-feature `XElement` inspection and managed GK25 projection. A fresh official response and its network provenance are cached; namespaces, closed rings, IDs and forward references were checked against the actual bytes. This supports the documented inspection subset, not universal CityGML conversion. Manual fallback: open [Helsinki 3D](https://kartta.hel.fi/3d/), select a small polygon and non-textured export, capture file/terms/CRS. A single required direct 4 km² sheet is a secondary fallback only after checking its size. The HTML entry loaded; no graphical viewer/export was performed.

### Simpler building fallback: footprint WFS

`avoindata:Rakennukset_alue` returned two **2D GeoJSON Polygons** from 21 matches in a 200 m EPSG:3067 box. One is Stenbäckinkatu 16, `Rakennukset_alue.761064`, modification 2025-09-09; another lacks address/register ID. Publication field is 2026-10-02. No building heights or vertical CRS exist in these responses. This fallback requires clearly synthetic extrusion heights, not claimed real roof elevations.

[Product metadata 278](https://kartta.hel.fi/paikkatietohakemisto/pth/?id=278) identifies GK25, Helsinki coverage, undefined vertical datum, continuous maintenance and currency 2017-12-01, but retains older order/agreement terms. That conflicts with the current general open-publication policy. **Resolve and capture current product-specific terms before adopting this fallback**, using the map-service directory or provider clarification. Successful public access alone is not licence clearance. HRI building catalogue fetch returned 403 in browsing.

### Terrain: the served 2021 1 m elevation model

Select WCS coverage **`avoindata__Korkeusmalli_2021_1m`**, not the older 2012 model, surface model or photogrammetric mesh. [Terrain metadata 234](https://kartta.hel.fi/paikkatietohakemisto/pth/?id=234) declares laser-derived elevation, GK25/N2000 and Helsinki coverage, currency 2021-06-15 and metadata update 2022-11-11. It describes 50 cm pixels. The HRI search result likewise describes 50 cm; opening its page returned 403. However, the [live WCS product list](https://kartta.hel.fi/avoindata/dokumentit/Aineistolista_wcs_avoindata.html), printed 2026-09-24, lists the **1 m** product. Catalogue resolution is not the resolution retrieved here.

GetCapabilities/DescribeCoverage and a **4 × 4 m** GetCoverage succeeded. Measured TIFF: 4 × 4 Float32 cells, big-endian classic GeoTIFF, single band, uncompressed tile with 16-cell row stride, `ModelTransformationTag` matrix, PixelIsArea, EPSG:3879. Matrix top-left is E=25495140, N=6675056, pixel steps 1 m east / 1 m south. All 16 samples are valid, ranging about 13.410841–13.583823 m. The native coverage grid is 29,000 × 35,000; physical envelope E=25487000..25516000, N=6655000..6690000. This rectangular envelope contains the area but does not prove every cell is populated.

Important observed inconsistencies:

- DescribeCoverage labels the axes E,N but serializes corners/origin/vectors in N,E magnitude order. The successful GetCoverage uses **E=physical easting, N=physical northing**; swapped values returned HTTP 404.
- DescribeCoverage gives band unit `W.m-2.Sr-1`, inconsistent with the provider's elevation/N2000 documentation. Treat it as an unresolved metadata defect, not radiometric height units or an automatic datum conversion.
- NoData is -32767 in service metadata/TIFF. No vertical CRS is embedded in the TIFF keys. N2000/metres therefore remains a provider declaration; independent height checks are mandatory before accepting terrain. Product year 2021 is not an exact local scan date.

The city's [WCS guide](https://kartta.hel.fi/avoindata/dokumentit/Prosessi_Tyoohje_paikkatietopeitteen_kyselypalveluiden_kaytto_ulkoverkko.pdf) documents bounded E/N subsetting. Its old `50cm` coverage identifier must be replaced with the verified live identifier. Verify a managed .NET TIFF reader against tiled Float32, matrix georeferencing, endian order and NoData. The tiny offline probe inspector is deliberately not a production raster reader. No native GIS dependency is selected.

Manual fallback: save the exact bounded WCS URL response in a browser with provenance; otherwise use the Helsinki Map Service open-data directory. A WMS/JPEG/PNG shaded image is not terrain height data. If those fail, research NLS Elevation model 2 m as a separately verified fallback; its access/terms/formats are not claimed verified in source research.

### Roads: OpenStreetMap

Use semantic OSM ways/tags offline from the verified cached sample described below. The [OSMF API policy](https://operations.osmfoundation.org/policies/api/) directs read-only work to alternatives such as Overpass; the editing API is intended for editing. It requires an identifying User-Agent and limits editing-API downloads to two threads. Those are not Overpass quotas. No raster tile download or runtime map-service dependency is planned.

Actual bounded Overpass requests: `overpass-api.de` returned HTTP **406** for the exact street name and an ASCII regex variant; `overpass.kumi.systems` timed out after **30 s**, zero bytes. No OSM file, road length, timestamp, access tags or bridge/tunnel/layer claims were measured. The manual fallback is to run the registry's narrow query at [Overpass Turbo](https://overpass-turbo.eu/), export raw JSON/XML and record query, `osm_base`, timestamps and checksum. No manual export was performed. A city street-line probe returned a **bicycle rack**, not a centreline: `YLRE_Katuosat_viiva` is not a substitute road graph. City carriageway polygon access may be researched if both OSM routes fail; its semantic equivalence is not established.

OSM coordinates need the [approximate WGS84 policy](COORDINATES.md); they do not supply N2000 terrain heights. Inspect highway/access/oneway/bridge/tunnel/layer tags and topology; missing tags alone do not prove ground-level suitability. Record element/source update timestamps from the actual extract, not this documentation check date.

## Terms and attribution plan

City Survey Services' current [open-data policy](https://www.hel.fi/en/decision-making/information-on-helsinki/maps-and-geospatial-data/make-better-use-of-geospatial-data/open-geographic-data) states CC BY 4.0 unless dataset-specific terms say otherwise. The semantic building metadata explicitly agrees. Apply that policy to the served 2021 terrain; do not transfer its terms to the differently documented 2012 product. Retain **City of Helsinki, City Survey Services** credit, source/product links, licence link and a description of cropping, reprojection and simplification. The [CC BY 4.0 deed](https://creativecommons.org/licenses/by/4.0/deed.en) requires appropriate credit, a licence link, indication of changes and no added restrictions or implied endorsement. Capture exact terms with each real retrieval; footprint fallback's conflicting older terms remain open.

OSM is [ODbL 1.0](https://www.openstreetmap.org/copyright). Plan **© OpenStreetMap contributors** with the copyright/licence link in game credits/menu and distributed data notices; [OSMF game/simulation attribution guidance](https://osmfoundation.org/wiki/Licence/Attribution_Guidelines) identifies suitable display locations. [ODbL sections 4.2–4.6](https://opendatacommons.org/licenses/odbl/1-0/) address notices, public use of derivative databases and machine-readable access obligations. Classify the actual distributed road database and produced assets before publication; merely rendering geometry does not resolve that classification. Maintain source provenance separately so the review can identify transformations. **Non-commercial status is not an exemption**, and source research makes no legal conclusion about a future mixed-source asset package.

## Bounded retrieval and storage plan

These are project limits, not promised provider capacities. Implement only in selected later tasks, in C#/.NET, separate from offline conversion:

1. Start road retrieval with one 100–150 m route corridor; building acquisition with at most ten buildings in a bounded box; terrain acquisition with one terrain patch plus a small buffer. Preserve the exact 1 km² target. For native GK25 retrieval a provisional 1,100 m square is E=25494592..25495692, N=6674504..6675604. Verify transformed target corners fit before using it, then crop offline. Derive tighter OSM geographic bounds using the coordinate contract; the larger name-filtered discovery bbox in the registry is not permission to download all its roads.
2. Cache by source ID and SHA-256 of canonical request/query under ignored `data/raw/`. The raw cache is already ignored. Save exact URL/body, UTC retrieval time/status, response headers if present, byte count/checksum, CRS/axes/datum/units/bounds, update timestamps and terms snapshot. A request hash and a payload hash serve different purposes. Validate HTTP 200 bodies: XML exceptions/HTML errors are not data. Write `.partial`, validate, then atomically rename. Conversion uses only verified cache; refresh is explicit.
3. One request at a time, at least 2 s apart; at most two retries at 30/120 s, respecting Retry-After/status slots. Stop persistent 401/403/406, quota errors or byte/area/count violations with actionable manual fallback. No automatic mirror rotation or full-city paging. Identify the application/version in User-Agent. The game never accesses these services.
4. Caps: metadata 3 MB; research geometry probes 1–3 MB; road sample 10 MB; first ten buildings 10 MB and later bounded-area buildings 100 MB; terrain response 8 MB and at most 1.1 million pixels per request; total session downloads 150 MB. Inspect counts first and use small pages. A 1,100 m native crop has 1.21 million cells, so split it into bounded patches rather than exceeding the pixel cap. CityGML paging is not transaction-safe; save a single consistent small extract or explicitly reconcile changes.

The [Overpass operator guidance](https://dev.overpass-api.de/overpass-doc/en/preface/commons.html) gives broad safety margins around 10,000 requests/day and below 1 GB/day, with load-dependent slots and 429/504 responses. It is not a reserved allowance. Use query timeout 20 s / memory 16 MiB for the small street query, far below operator defaults (180 s / 512 MiB). No numeric Helsinki request/day or byte quota was found in checked guidance. The GeoServer default count of 1,000,000 is observed metadata, not a prudent download limit. Helsinki requests contact for continuous/large use or online integration; no service guarantee or contact was made.

Planning storage estimates: road raw JSON/XML 1–10 MiB, untextured buildings 10–100 MiB; geometry complexity can exceed these and should hit a cap, not silently broaden retrieval. A 1 km square Float32 raster is 4,000,000 bytes of sample payload; 1,100 m square is 4,840,000 bytes; 500 m square is 1,000,000 bytes, plus TIFF headers/tiling/padding. These are arithmetic estimates, not measured full downloads. Budget **512 MiB** for bounded raw/intermediate/generated working files initially; textures and photogrammetric mesh are excluded. Keep all raw data and generated assets outside ordinary Git. Manual downloads enter the same cache with untouched bytes, checksum and provenance.

## Commands, evidence and remaining checks

Installed tools observed: .NET SDK 10.0.112, curl and existing managed ProjNET 2.1.0; `jq` not found. Initial sandbox curl failed DNS, so bounded network checks were retried with approved escalation. Successful requests use the **exact URLs in `probes`** in the registry:

```bash
curl --fail --location --max-time 30 --max-filesize 1000000 -o <ignored-file> '<registry probe URL>'
# Building probe permits 3000000 bytes; capabilities permit 3000000 bytes.
dotnet run --file scripts/p09-inspect.cs -- artifacts/p09
git diff --check
```

The offline C# inspector reads only cached files, checks TIFF structure/samples and CityGML dates, compares the registry origin/1 km bounds and verifies every recorded byte count/SHA-256. Raw research responses are retained locally in ignored `artifacts/p09/`; committed metadata preserves URLs and hashes. Re-fetching may produce new timestamps/content and hashes; keep new snapshots separate rather than claiming identical bytes. Total ten registered successful responses: approximately 1 MB, including service metadata. Additional schema/street-line research was also small. No city-wide source product was fetched.

Automated outcome: see [source research evidence](TESTING.md#bounded-source-research). Manual reference check: open the city map at the geographic locator, find Stenbäckinkatu, and inspect the eastern interior run between the named junctions. Expected: the square surrounds the road, and a short surface segment can be isolated without entering tram/hospital/junction structures. Verify current construction/access against the 2026 plan before treating it as a drivable route. **Not performed**; this is a future road retrieval route-validation check, not missing game acceptance for the research task.

Remaining checks cover physical route access/construction and supported endpoints, footprint terms, building selection beside the playable strip and mesh conversion, production raster reading/NoData coverage/scan age, independent N2000 height controls and physical alignment. Cached OSM retrieval/graph conversion and bounded building XML inspection are implemented; no G3/G4 gate or earlier manual check is closed.

## Verified road sample — 2026-10-04

The initial failed requests above are historical. A tighter named-highway query now returned HTTP200 from the documented Overpass endpoint: 11930 bytes, 11 ways/26 unique nodes. Exact request, retrieval time, checksum, tag coverage and actual-vs-request bounds are in [TESTING.md](TESTING.md#bounded-road-retrieval-and-offline-inspection); the [command](PIPELINE.md#bounded-osm-road-sample) replays the verified cache offline or imports local raw JSON. The registry's initial probe observations remain unchanged. This removes the acquisition uncertainty for this cached sample, without confirming physical construction/access, ground level, a driveable route or current future service availability. One full-way vertex lies outside request bounds; Raw geometry remains unchanged for offline graph clipping. A graph and flat road strip are now generated separately; physical access and surveyed heights remain unverified.
