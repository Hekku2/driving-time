# Project state

Goal: Small drivable Helsinki desktop prototype<br>
Updated: 2026-10-05<br>
Current gate: G0 open; historical command checks pass, visual acceptance pending<br>
Next proposed change: P13 — drive on the first Helsinki road, after road-strip visual inspection and environment setup

## Current environment

The environment checker runs with `bash scripts/check-environment.sh`. Latest inspection confirms Ubuntu WSL2/x64, Linux SDK 10.0.112 selected by global.json and Git 2.43.0. curl, unzip and sha256sum are available. Godot 4.5.2 .NET and matching export templates are absent from the checked default/PATH locations; set GODOT4 for a custom complete editor or follow [setup](SETUP.md). Display variables are configured, but graphical operation was not retested. The launcher preserves the calling shell, working directory and options when sourced under normal interactive Bash settings.

Historical checks on 2026-10-03/04 used Godot 4.5.2 .NET, matching templates and SDK 10.0.112. The game uses Godot.NET.Sdk/4.5.2 and net8.0; the independent pipeline/tests use net10.0. Build/import/runtime and Windows cross-export passed. The editor plugin allows LatestMajor runtime roll-forward; the Windows export includes .NET 8.0.31. These historical successes do not certify the current missing editor installation. Review the game framework before its recorded 2026-11-10 support end.

Development and export remain in WSL; only the exported EXE, PCK and complete companion runtime directory go to Windows. C#/.NET is fixed for game, pipeline, retrieval and tests. No Python, Node.js, containers or external GIS CLI is required.

## Implemented capabilities and evidence

- **Driving:** VehicleExperiment.tscn is the default VehicleBody3D scene; Main.tscn retains the CharacterBody3D baseline. Four suspension wheels, rear-wheel propulsion, front steering, chase camera, obstacles/ramp, manual R reset and configurable fall recovery are implemented. Vehicle checks passed 1,150 physics ticks/five recoveries at 30/60 rendering caps with identical sampled traces; baseline driving/camera/recovery checks passed. The user's controller choice is recorded in D016; jitter cause and visual improvement remain unverified.
- **Export:** The C# helper builds/imports/exports in WSL, transfers runtime files and verifies SHA-256. Historical vehicle artifact `artifacts/windows/20261003-184950-a7195a8f` contains 188 runtime files transferred to `W:\projektit\driving-time`. A later synthetic-inspection export `artifacts/windows/20261003-204544-dc05f013` passed native headless mesh checks. Do not assume the Windows destination still contains either artifact without checking.
- **Synthetic assets:** Locked SharpGLTF.Toolkit 1.0.7 writes a deterministic 6,664-byte GLB with a 10 m cube, cardinal/up markers and ground. Manifest hashes, localized bounds, winding/normals, import transforms/materials and exported resource availability were verified. AssetInspection.tscn provides an orbit view; generated assets stay ignored under `game/Generated/P08`.
- **Coordinates:** Double-precision EPSG:3067 east/north mapping, explicit source-axis adapters, synthetic/N2000 height checks and tile-local math are implemented. ProjNET 2.1.0 matches the published NLS example within 0.000027 m; EPSG:4326 needs explicit approximate-datum opt-in. Exact epoch/grid/geoid operations remain unsupported. See [coordinate contract](COORDINATES.md).
- **Source research:** The [registry](../config/source-registry.json) records Stenbäckinkatu bounds/origin, provider declarations, bounded measurements, retrieval details and ten response checksums. CityGML, footprints and a 4×4 Float32 terrain probe were inspected. Terrain resolution/axis/unit inconsistencies, footprint terms and independent height controls remain open. See [data sources](DATA_SOURCES.md).
- **Road retrieval:** A bounded named-highway request returned HTTP200, 11,930 bytes, 11 ways and 26 unique nodes on 2026-10-04. Raw bytes/query/request and payload hashes, retrieval metadata and ODbL provenance are cached under ignored `data/raw/osm-p10`. Offline replay/local-file import passed; source bytes remain unchanged.
- **Road graph:** Offline conversion preserves source IDs/geometry/tags/provenance, splits shared/repeated nodes and clips in projected metres. Reverse-oneway arcs, separate boundary cuts, grade tags and disconnected crossings were tested. Real output has 12 graph nodes, 11 segments, 22 arcs, one component and twelve diagnostics; repeated JSON/SVG bytes match.
- **Road strip:** One 100 m interior section of Stenbäckinkatu way 310376967 has visual/collision GLBs and a deterministic assumption/provenance manifest. Width 8.8 m comes from a source tag; Y=0 is explicitly synthetic. Cleanup, bounded joins and concave triangulation are implemented. Latest full Debug build passed with zero warnings/errors and all 78 pipeline tests passed. Godot import verified 14 triangles per asset, metre bounds, winding and collision rays. No playable real-road scene or drive test exists yet.

Full dated commands, package/source checks and historical outcomes live in [TESTING.md](TESTING.md). User instructions are in [RUN.md](RUN.md); conversion commands are in [PIPELINE.md](PIPELINE.md). No gate closes through scaffolding or automated evidence alone where visual acceptance is required.

## Selected world configuration

Use EPSG:3067 projected metres and N2000 heights for validated real elevated assets. Engine mapping is X=east−originEast, Y=height−originHeight, Z=−(north−originNorth), right-handed. The provisional anchor is E=384236.0361374, N=6674210.89320437, H=13.52536678314209 m. Bounds are E=383736.0361374..384736.0361374, N=6673710.89320437..6674710.89320437; tile size is provisionally 500 m. The address/terrain anchor is not a vehicle spawn or independent vertical control.

`config/synthetic-scene.json` remains a separate synthetic-local-metres fixture: region −30..30 m, origin (0,0,0), 500 m tile metadata. No geographic datum is implied by the driving test scene. Changes to the real origin require explicit versioning.

## Outstanding manual checks and limits

- Inspect the road asphalt and separate collision gizmo using [the road inspection](RUN.md#road-strip-inspection): expect a flat, continuous 100 m strip without spikes, holes or duplicate rendering. Actual physical access/construction and width accuracy remain unverified.
- Inspect synthetic cube/cardinal/up markers in the editor and native Windows export using [asset inspection](RUN.md#synthetic-asset-inspection). G2 stays open until scale, direction and appearance pass.
- Drive the vehicle loop, obstacles and ramp; check steering/braking, jitter, R/fall recovery and camera wall behavior using [driving comparison](RUN.md#vehicle-driving-comparison). G1 remains open. Controller selection alone does not pass these checks.
- The original floor/cube export at `C:\TEMP\driving-time-P02\DrivingTime.exe` historically started in Windows; visual acceptance remains unverified. Current Main.tscn contains the arcade baseline, so record the scene/artifact tested. G0 remains open.

Historical WSLg used llvmpipe software rendering, warned about V-Sync and lacked audio libraries. Native Windows startup reported RTX 3060/OpenGL 3.3.0/driver 591.86; no performance baseline or achieved-FPS claim exists. No buildings/terrain/intersections/streaming are implemented; G3–G6 remain unpassed. No publication or raw-data redistribution is authorized or performed.

## Next session

Permanent documentation now uses feature names and descriptive links independently of historical task IDs. Repository instructions enforce this boundary; existing path/log identifiers remain literal for reproducibility. The backlog and task cards retain change planning/history; this cleanup changes no runtime behavior or acceptance status.

Resolve current editor/template setup, record visual feedback against a specific artifact, then prepare the proposed first-road driving change from the backlog. Keep the area bounded and stop at one selected change. Completed implementation history belongs in dated evidence; planning IDs belong in the [task ledger](tasks/INDEX.md).
