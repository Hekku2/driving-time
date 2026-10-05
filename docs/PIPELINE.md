# Offline pipeline

Keep the checkout, .NET SDK, NuGet cache, pipeline and generated manifests in WSL2. Use the root `global.json` SDK 10.0.112. Pipeline and tests target net10.0; the independent Godot game retains net8.0. No shared library is required. The workflow requires no Python runtime, Node.js, container or external GIS CLI. Only the exported game EXE, PCK and complete companion runtime directory go to Windows; see [RUN.md](RUN.md) for the existing export helper.

## Restore and run

From the repository root in WSL, initially with network access:

```bash
dotnet --version
dotnet restore tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --locked-mode --source https://api.nuget.org/v3/index.json
dotnet build pipeline/DrivingTime.Pipeline.csproj --no-restore
dotnet test tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --no-restore
```

The test restore also restores its pipeline reference. The original manifest command uses built-in .NET JSON, validation, SHA-256 and file operations. Coordinate and GLB commands also use the pinned managed dependencies described below. Tests need only Microsoft.NET.Test.Sdk 17.14.1, xunit 2.9.3 and xunit.runner.visualstudio 3.1.4. Exact version ranges are in the test project; checked-in `packages.lock.json` files pin transitive versions/content hashes. Normal restores use `--locked-mode`; after a deliberate package change, regenerate with `dotnet restore ... --force-evaluate` and review the lock-file diff before returning to locked mode. Initial restore performs NuGet auditing; network failures must be resolved, not silently disabled. This policy applies to the new pipeline/tests; the existing game SDK/restore policy is unchanged.

After restore, the documented command is:

```bash
dotnet run --project pipeline/DrivingTime.Pipeline.csproj --no-restore -- --config config/synthetic-scene.json
```

Expect exit 0 and `Wrote synthetic manifest v1:` with the output path. This command builds locally without restore. It reads one local configuration and writes one local file; there are no network clients or download steps. To run the compiled result directly, add `--no-build` after the initial build. Building the whole solution also needs the game's already documented initial restore.

## Configuration and manifest v1

[synthetic-scene.json](../config/synthetic-scene.json) is a repository-authored 60 × 60 m fixture, not a selected Helsinki region or geographic origin. All fields are required and case sensitive; unknown fields, duplicate fields, missing/null values and malformed JSON fail. Comments and trailing commas are unsupported.

| Field | Rule |
|---|---|
| `schemaVersion` | Exactly 1 |
| `sceneId` | 1–64 ASCII letters, digits, hyphens or underscores |
| `coordinateSystem` | Exactly `synthetic-local-metres`; no EPSG support yet |
| `region` | Finite `minEast`, `minNorth`, `maxEast`, `maxNorth`; finite strictly positive extents |
| `origin` | Finite `east`, `north`, `height`; east/north inside closed region bounds |
| `tileSizeMetres` | Finite positive number; metadata only, no tiles generated |
| `manifestPath` | Relative `.json` path, resolved from the configuration directory, using `/`; no `.`/`..`, empty segments, control characters, backslashes or colon; cannot overwrite configuration |

The output contains schema version, scene ID, region, origin, tile size, provenance and two primitive descriptions: a ground plane spanning the region at origin height, and a 2 m reference box centred at origin east/north, with its bottom at origin height. Source coordinates and sizes use east/north/height metres; no engine mapping is implied. The reference box is a fixed fixture and may extend outside very small regions or an edge origin. This descriptive manifest contains no mesh/collision assets or tile enumeration. Separate asset-generation commands below use the coordinate contract and engine staging.

Provenance states repository-authored synthetic content, no retrieval date, no geographic CRS/vertical datum and the SHA-256 of the exact input bytes. No third-party dataset license applies; this statement grants no new rights to repository code. Configuration whitespace changes its checksum. No source dataset is downloaded or redistributed.

Objects are ordered by ordinal ID; property order follows the fixed record declarations. JSON uses invariant numeric formatting, UTF-8 without BOM, and a final LF. There are no timestamps, absolute paths or random IDs in the manifest and no excluded diagnostic metadata. Same input bytes with the pinned SDK produce identical manifest bytes. The absolute output path appears only in console diagnostics.

Errors go to stderr with exit 2: check the named field, configuration path or write permissions. Validation finishes before output creation; invalid input leaves any existing output unchanged. Valid runs overwrite the configured manifest. File writes are not atomic; rerun after an interrupted write. Keep generated output outside ordinary Git; the sample's `config/generated/` is ignored.

## Short review

Run the documented command twice without restore/network access, copying the first manifest aside:

```bash
cp config/generated/synthetic-scene.manifest.json /tmp/p06-first.manifest.json
dotnet run --project pipeline/DrivingTime.Pipeline.csproj --no-restore -- --config config/synthetic-scene.json
cmp /tmp/p06-first.manifest.json config/generated/synthetic-scene.manifest.json
```

Expect `cmp` exit 0. In a temporary copy of the config, change `tileSizeMetres` to 0; expect exit 2 naming `tileSizeMetres` and no output created. Full automated evidence is in [TESTING.md](TESTING.md#offline-synthetic-manifest-cli). The descriptive manifest needs no visual game check.

Official sources checked 2026-10-03: [NuGet lock files and locked restores](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies), [System.Text.Json unmapped-member rejection](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/missing-members), and package metadata for [xunit 2.9.3](https://www.nuget.org/packages/xunit/2.9.3), [VS runner 3.1.4](https://www.nuget.org/packages/xunit.runner.visualstudio/3.1.4), [test SDK 17.14.1](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/17.14.1). Local restore/build/test establishes compatibility with net10.0 and SDK 10.0.112; versions were selected for this small harness, not as a claim they are latest.

Coordinate contract adds the tested [coordinate contract](COORDINATES.md) and bounded ProjNET 2.1.0 horizontal projection API. The v1 synthetic CLI/config/output above retain their existing behavior; Asset generation consumes this mapping. Real-source bounds/origin are recorded separately in the source registry; the synthetic fixture remains unchanged. Restore uses updated lock files; tests include independent NLS reference evidence and all pipeline CLI regressions.

## Synthetic GLB staging

The separate fixture manifest v1 is an asset manifest, distinct from the original descriptive manifest v1. The original `--config` command remains available. Run from the repository root in WSL:

```bash
dotnet restore DrivingTime.sln --locked-mode
dotnet build DrivingTime.sln --no-restore
dotnet run --project pipeline --no-build --no-restore -- --config config/synthetic-scene.json --stage-fixture game/Generated/P08
"$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64" --headless --path game --editor --import
"$HOME/.local/share/driving-time/godot-4.5.2/Godot_v4.5.2-stable_mono_linux.x86_64" --headless --path game res://AssetInspection.tscn --quit-after 120 -- --check-fixture
```

The explicitly supplied staging directory resolves from the working directory. It receives only `synthetic.glb` (6,664 bytes) and `manifest.json`. Treat this directory as owned generated output; choose `game/Generated/P08` for the inspection scene. `game/Generated/`, import sidecars and `.godot/` caches are ignored and reproducible, never ordinary Git assets. The C# source and configuration are the fixture. A clean checkout must generate before import. Do not use symlinks to outside-project assets. Godot's pinned default importer uses root scale 1 with no custom script; runtime instantiation applies no transform. Import precedes export.

The existing `dotnet run --file scripts/export-windows.cs` helper now generates the same fixture after its ExportRelease build and before import/export. The preset exports all resources and explicitly includes `Generated/P08/manifest.json`; runtime uses Godot `FileAccess` and `ResourceLoader` remapping rather than OS paths or a runtime glTF parser. Raw GLB bytes are not required in the exported game. Full runtime transfer remains required. No pipeline/writer assembly becomes a game dependency.

Asset manifest fields: `schemaVersion=1`, scene ID, `units=metres`, source coordinate-system label, fixed source E/N/H origin, axes/handedness, synthetic vertical datum, configured source region, local engine-space `min`/`max`, asset paths relative to the manifest plus SHA-256, object IDs/local bounds/linear RGBA colors, and configuration checksum/provenance. Retrieval date is null because nothing was downloaded. Aggregate bounds are (-15,-0.2,-15)–(15,10,15) m. The orange building is exactly 10 m on each axis; red east marker occupies X=8..12, green north marker Z=-12..-8, blue up marker rises 6 m near (-9.5,0,8.5). Ground is a thin 30 m slab. Configuration must contain the fixture's 15 m horizontal radius or generation fails before writes. Shape sizes are intentionally fixed for this test; configuration controls source origin, region and output staging.

Source corners use E,N,H doubles and coordinate contract's `CoordinateFrame.ToEngine` once before float serialization: X=E-originE, Y=H-originH, Z=-(N-originN). GLB vertices are already in Godot's right-handed metre space; all nodes are identity. Pipeline triangles use glTF counterclockwise fronts and flat outward unit normals. Godot reverses triangle winding to its clockwise fronts without rotating or scaling these coordinates. Linear material factors are converted by the importer to Godot sRGB albedo; there is no pipeline gamma conversion. Materials are opaque, single-sided, metallic=0, roughness=0.85. No textures, collision, real data or streaming.

SharpGLTF.Toolkit 1.0.7 (MIT; net10.0 package inspected locally), with locked Core/Runtime 1.0.7, is verified by serialized GLB readback tests and the independent Godot importer. No native dependency or additional scripting runtime is required. Official sources checked 2026-10-03: [SharpGLTF package](https://www.nuget.org/packages/SharpGLTF.Toolkit/1.0.7), [writer example](https://github.com/vpenades/SharpGLTF/blob/master/examples/Example1/Program.cs), [Khronos specification](https://github.com/KhronosGroup/glTF/blob/main/specification/2.0/Specification.adoc), [Godot 4.5.2 importer source](https://github.com/godotengine/godot/blob/4.5.2-stable/modules/gltf/gltf_document.cpp) (baseColorFactor linear-to-sRGB handling), and [Godot export resource filters](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_projects.html). These support format/API choices; actual fixture evidence is in [TESTING.md](TESTING.md#synthetic-glb-import).

## Bounded OSM road sample

Run in WSL with pinned .NET 10.0.112. No new dependencies or game-service access.

The download helper restores locked pipeline dependencies and then runs the bounded retrieval command below. It reuses a verified cache when present; otherwise it downloads one sample. Use `--refresh` only to explicitly request fresh data. Configuration remains in `config/road-sample.json`; the helper locates the repository from its own file and stops on failure without retries. After successful retrieval it verifies the reported payload/provenance and atomically updates only the graph input/provenance path fields, preserving other configuration values. It performs no conversion.

```bash
dotnet run --file scripts/download-road-source.cs
```

The successful output identifies the cache directory. The helper automatically selects its `raw.json` and `network-provenance.json` in `config/road-graph.json`, so readiness and offline conversion use the verified payload even when its checksum changes. Direct pipeline invocation below still requires manually selecting these paths after a fresh response. This retrieves current source data; it does not recover historical response bytes.

For direct pipeline invocation after restore:

```bash
dotnet run --project pipeline --no-restore -- road-sample --config config/road-sample.json
```

Default: verify and parse a network cache if present; otherwise send **one POST** to `https://overpass-api.de/api/interpreter`. Explicit `--refresh` makes one new request. No retries, paging, automatic mirrors or redirects. Identifying User-Agent is `DrivingTime-P10/1.0 (bounded offline Helsinki prototype)`. A shared cache-root file lock prevents concurrent requests and a persisted timestamp enforces at least two seconds between attempts. Separate cache roots/processes are not a global rate limiter. HTTP errors (including 403/406/429/504), Retry-After, timeout or malformed/error bodies stop with exit 2; respect operator waiting guidance before manually retrying. Timeout is 30 seconds for headers and streamed body; query timeout20s/maxsize16MiB; raw body cap10MiB, 1000 ways and 100000 total vertices. No automatic retry means provider cooldowns cannot be bypassed by this command's retry logic.

Configuration v1 has `streetName`, EPSG:3067 `region` and `cachePath` (relative to the config directory). All fields are required; unknown/duplicate fields fail. Region must stay inside source research's selected square. The supplied discovery rectangle is **400 × 300 m**: E384236.0361374..384636.0361374, N6674060.89320437..6674360.89320437. Coordinate contract inverse-projects its four corners to a geographic envelope using the documented approximate EUREF-FIN/WGS84 policy. The query selects only named highway ways, then `out meta geom`. This is discovery of the selected street, not a clipped route corridor. Full ways can cross the rectangle and adjacent junction patches. Geographic envelope is slightly larger than the projected rectangle; no extent expansion or road generation is implied.

The summary separates **request bounds** and **actual full-way bounds**, reports outside vertices, unique node/way/vertex counts, `osm_base`, element timestamps and exact raw tag strings. It includes present/missing/value counts for highway, oneway, lanes, maxspeed, layer, bridge, tunnel, access, motor_vehicle and construction. Every supplied way tag remains in the summary/raw cache, including directional speed/lane tags. Missing values have no defaults; `maxspeed:forward` is not guessed into `maxspeed`. Heights/vertical datum are absent. Geometry is validated for matching node-reference lengths, valid coordinates/IDs, duplicate ways, conflicting shared-node coordinates, matching name and at least one vertex in the configured geographic envelope. Empty output, `remark` error/partial output, HTML/XML/GeoJSON and truncated/incomplete JSON fail before a raw cache is created. This cannot detect omitted entire ways or stale mapping.

Cache layout: ignored `data/raw/osm-p10/<request-sha256>/<payload-sha256>/`. Request hash covers endpoint, POST method and exact form body; payload hash covers exact unmodified response bytes. Files: `raw.json`, `network-provenance.json` (or `import-provenance.json`) and `summary.json`. Metadata stores query/body/bounds, source/mode, UTC retrieval or import time, HTTP status/headers if actually retrieved, checksum/byte count, generator/OSM database timestamp, CRS/datum, attribution/terms and limitations. `latest.json` points to the last valid network payload via its metadata. Default offline replay checks request, size, checksum and parser validation; corrupt cache fails and needs repair or explicit refresh. Writes use unique `.partial` files and atomic per-file rename; interruption can leave an unreferenced valid payload, never a new pointer to an unvalidated response. No multi-file transaction is claimed.

For offline input, export **raw JSON** with the exact query stored in network provenance at [Overpass Turbo](https://overpass-turbo.eu/), or save a bounded response separately:

```bash
dotnet run --project pipeline --no-restore -- road-sample --config config/road-sample.json --input /absolute/path/sample.json
```

Local import makes no request and records the absolute input path/import time, with retrieval time/status/headers unknown. Its original source, licence and query remain unverified declarations; retain original download/query evidence alongside it. It does not change the network-cache pointer. JSON with complete way geometry is the only supported local format; choose raw JSON rather than XML/GeoJSON. Re-importing `raw.json` verifies and parses offline without modifying its bytes.

Tiny repository-authored parser fixture (synthetic, not real OSM or evidence of real tags):

```bash
dotnet run --project pipeline --no-restore -- road-sample --config config/road-sample.json --input tests/fixtures/osm-road-sample.json
```

Expected: 2 ways, 3 unique nodes; raw oneway `-1` on one way and missing on the other; maxspeed `30`/`walk` preserved. Manual review is optional for road retrieval's CLI acceptance; no graphical/driving check is required. No roads, graph, clipping, route endpoints or spawn are produced; road graph is next. Physical construction/access and the proposed 100–150m route remain unverified downstream checks.

Official sources checked **2026-10-04**: [Overpass QL](https://wiki.openstreetmap.org/wiki/Overpass_API/Overpass_QL) (bbox order south/west/north/east, JSON, geometry/meta output and resource settings); [operator guidance](https://dev.overpass-api.de/overpass-doc/en/preface/commons.html) (load-dependent slots/429/504, no guaranteed allocation); [OSM copyright/licence](https://www.openstreetmap.org/copyright) (© OpenStreetMap contributors, ODbL1.0). These are access/terms references, not a licence review of future derivatives. Keep raw sources ignored and do not redistribute without review. Actual retrieval evidence is in [TESTING.md](TESTING.md#bounded-road-retrieval-and-offline-inspection).

## Offline intermediate road graph

Run in WSL with the existing pinned SDK, after road retrieval has populated the cache:

```bash
dotnet run --project pipeline --no-restore -- road-graph --config config/road-graph.json
```

This command reads only local files. Configuration v1 controls raw/provenance input paths, graph/plot output paths, EPSG:3067 clip rectangle and explicit `allowApproximateDatum`. All paths resolve relative to the config. The checked-in paths select the exact road retrieval network payload rather than a mutable latest pointer. Missing files require road retrieval acquisition/import or updated paths; this command never downloads. Distinct input/output/config paths are required. Provenance schema, payload SHA-256, byte count, source timestamp/generator and required attribution/source fields are checked before conversion. The command trusts the road retrieval sidecar's source declaration; a checksum is not independent proof of upstream authenticity.

Graph schema v1 (JSON, camelCase):

| Field | Meaning |
|---|---|
| `horizontalCrs`, `units`, `verticalDatum`, `datumPolicy`, `clipRegion` | Absolute double EPSG:3067 E,N metres, explicit coordinate contract approximate WGS84 policy, configured rectangle; no heights or vertical datum inferred |
| `sourceDocument`, `provenance` | Complete original JSON content including OSM version/changeset/user metadata, geographic geometry, timestamps; complete road retrieval sidecar including retrieval date/checksum/license/attribution/query/headers. JSON formatting may change; exact raw bytes stay in the hash-verified cache |
| `sourceWays` | Unclipped source IDs, ordered node IDs, geographic geometry, raw string tags and full projected geometry; no original vertex discarded here |
| `nodes` | Segment endpoints: `osm/<id>` or unique `cut/<way>/<fractional-source-index>`; nullable OSM ID, projected coordinates, boundary/shared-junction flags |
| `segments` | Stable `way/<id>/segment/<ordinal>` in ascending way ID/source order; source way ID, endpoints, ordered clipped geometry and all raw tags; direction and its basis |
| `arcs` | Segment/from/to references: two for both directions, one for forward/reverse, none for unsupported direction. Base direction only, not legal routability |
| `components`, `isolatedSegments`, `diagnostics` | Undirected segment components; singleton components are isolated segments. Structured assumption/unsupported/degenerate/context reports |

Geometry vertices record `osmNodeId` and `sourceIndex`: integer original node index, or fractional index along the original straight projected edge for a cut (null OSM ID). Geometry always follows original OSM order. `oneway=-1` reverses the arc, preserving geometry and directional tags. `yes/true/1`, `no/false/0` are supported; roundabout/motorway imply forward unless explicitly overridden. Missing ordinary-highway oneway uses a reported two-way default; unsupported values emit no arcs. All raw tags remain available, including directional speed/access and `layer`, `bridge`, `tunnel`. Layer numbers are not heights.

Split points are shared or repeated OSM nodes, plus original way ends and rectangular cuts. No geometric welding occurs: distinct IDs at identical positions remain disconnected; crossing roads on different levels remain separate. Differing grade tags at a shared OSM node retain explicit connectivity and emit a transition-review diagnostic, allowing bridge approaches without inventing vertical geometry. Conditional/modal/turn restrictions and access/construction tags emit diagnostics; node tags and restriction relations are absent from the way-only input. Closed/repeated ways and excluded near-zero edges are reported. An empty clipped way is reported. Unsupported data is preserved for later review.

A cut is local to its way/source position, never merged by coordinates and never flagged as a junction. An original OSM node exactly on the clip boundary retains its identity and boundary flag, with junction=false because context outside the rectangle is incomplete. Shared interior OSM endpoints are flagged as junctions even where they are only continuations between two ways; this is sample connectivity, not a claim that a physical intersection exists. Leaving/re-entering the rectangle produces separate pieces. Zero-length boundary touches are discarded. The request's geographic envelope is not the clipping rectangle; conversion clips in projected metres.

Generated JSON and SVG stay under ignored `data/intermediate/p11`; they include unreviewed OSM source metadata and must not be redistributed without license review. SVG is a standalone debug plot with north up/east right, bounds, OSM node/way labels, directional arrows, orange boundary endpoints, red shared interior endpoints and a grade-tag color hint. Hover a segment for its tags. It provides inspectability, not surveyed alignment/elevation or proof of physical route suitability. No Godot import/export, game edits, AI lanes or intersection surfaces are part of road graph.

Checked 2026-10-04: official OSM [oneway semantics](https://wiki.openstreetmap.org/wiki/Key:oneway) (direction relative to ordered ways, reverse and implied values), [elements](https://wiki.openstreetmap.org/wiki/Elements) (node identity and ordered ways), [bridge tagging](https://wiki.openstreetmap.org/wiki/Key:bridge) (bridge sections/approaches and layer tags). Direct layer/tunnel wiki fetches failed; this task preserves those strings and does not infer elevations or introduce additional tag semantics. Projection/dependencies remain coordinate contract's verified implementation; no new packages or runtimes.

Optional manual inspection: open `data/intermediate/p11/road-graph.svg` in a browser. Expect the bounded labeled Stenbäckinkatu chain, arrowheads for both base directions, and an orange endpoint at the clipped western edge. Review the JSON diagnostics beside it. Appearance/user inspection has not been performed; no manual driving acceptance is required for this data-conversion task. The road-strip command below consumes the graph.

## Single flat road strip

The [dedicated first-road scene](RUN.md#first-helsinki-road-drive) loads the staged manifest and imported visual/collision resources at identity. `pipeline/Coordinates.cs` is shared as linked source with the game and compiles independently for net10.0 and net8.0; no pipeline/writer/projection assembly dependency enters the game. Heights are explicitly synthetic for spawn/reference mapping. Export helper regenerates graph/strip offline before import, records `road-graph.log`/`road-strip.log` and includes road manifest/game config in resource filters. Export execution was skipped by user request; packaged road availability remains unverified.

```sh
dotnet run --project pipeline --no-restore -- road-graph --config config/road-graph.json
dotnet run --project pipeline --no-restore -- road-strip --config config/road-strip.json
```

Both commands are offline. The first replays the existing road retrieval cache; if absent, restore that cache using the documented road retrieval workflow. The second reads the bounded road graph JSON and generates only `way/310376967/segment/0`, from distance 8 m to 108 m in source geometry order. The original segment is about 115.579 m long and contains eight points. The cut keeps eight points and seven edges along gentle bends; it ends about 7.579 m before the source endpoint. Shared-node junctions were split in road graph; cutting inside one segment excludes observed endpoint junctions. The name-filtered sample cannot prove absence of neighboring streets, driveways or unobserved junctions. Physical access/construction remains unverified.

`config/road-strip.json` controls graph/stage paths (relative to config), segment ID, start/length (maximum 200 m), origin/region, flat local Y, width fallbacks, short-edge tolerance and join cap. Origin/region preserve D019; horizontal mapping is coordinate contract X=east−originEast, Z=−(north−originNorth), calculated in doubles before float serialization. Y=0 is deliberately synthetic and is not a surveyed N2000 road height, despite retaining the source research origin height. Tile size is irrelevant to this single untiled asset.

Width policy: positive plain decimal OSM `width` in metres up to 20 m; otherwise configured `highway` fallback, otherwise default. Unsupported tagged units/ranges/lists are reported, not parsed as measurements. Defaults are residential 7 m, service 4 m, otherwise 6 m; lanes and parking tags do not infer width. The real way has `width=8.8` and `source:width=ARCore`; this is a provider tag, not an independently surveyed carriageway or clear driving width. Parking/sidewalk details do not add mesh surfaces. The manifest reports the selected policy and all assumptions.

Consecutive duplicate/near-zero points (<0.01 m by default) are removed before arc-distance cutting; tiny cut remnants are collapsed while keeping the endpoints. Cross-section bisectors use miter offsets with a maximum two half-widths by default. Tangential offsets are also bounded to 45% of either adjacent edge length. Extreme joins deliberately narrow rather than produce spikes. Concave capped quads use their interior diagonal; inverted/degenerate triangles after float conversion fail before writing. Near reversals, grade/junction/construction tags, out-of-region surfaces and endpoint-inclusive cuts fail with actionable errors. This is a narrow simple-strip algorithm, not a polygon union/intersection or global self-intersection repairer; complex hairpins/loops are unsupported.

Ignored `game/Generated/P12` contains:

- `road.glb`: one untextured dark rough asphalt surface with upward normals and identity transform, no markings.
- `collision.glb`: separate low-detail triangle surface with identical geometry. Its `RoadCollision-colonly` node imports in Godot as a StaticBody3D with ConcavePolygonShape3D and no rendered mesh. Other viewers can show its blue inspection material. Static open surface only; no thickness, curbs, sides or dynamic concave body.
- `manifest.json`: independent schema v1 `kind=flat-road-strip` (distinct from synthetic asset import fixture schema), local bounds, metre axes, origin/region, selected source references/tags, width/flat-height policy, assumptions, triangle count, relative asset roles/paths/hashes, exact configuration and graph SHA-256, upstream provenance and datum policy. No current timestamp is added, so offline replay remains deterministic. Road retrieval provenance retains raw checksum, retrieval date, source/attribution and ODbL license. Generated real assets remain local/unreviewed for redistribution.

No new dependency was added; the synthetic asset import pinned SharpGLTF.Toolkit 1.0.7 remains the writer. Source checks on 2026-10-04: [Khronos glTF 2.0 specification](https://registry.khronos.org/glTF/specs/2.0/glTF-2.0.html) for metre/right-handed geometry and triangle fronts; [Godot 4.5 import suffixes](https://docs.godotengine.org/en/4.5/tutorials/assets_pipeline/importing_3d_scenes/node_type_customization.html) for `-colonly` static collision generation. Actual import/physics evidence is separate in [TESTING.md](TESTING.md#single-flat-road-strip); playable integration evidence is in [the road-driving checks](TESTING.md#first-helsinki-road-integration).
