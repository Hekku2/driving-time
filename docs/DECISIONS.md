# Current decisions

Updated: 2026-10-05. This document records the choices that apply now. Command results and checked sources live in [testing](TESTING.md), [setup](SETUP.md), [coordinates](COORDINATES.md) and [data sources](DATA_SOURCES.md).

## Language and workflow

C#/.NET is fixed for the game, offline conversion, retrieval tools and automated tests. Prefer NuGet and dotnet commands. The runnable game has no pipeline or live map-service dependency.

Development, Godot editing/import, builds, tests, conversion and Windows export run in WSL2 with Linux tools and checkout. Transfer only the complete exported game (EXE, PCK and companion runtime directory) to Windows for native playtesting. Windows desktop x64 is the first target. No Python, Node.js, containers or external GIS CLI is required.

## Toolchain

Use Godot 4.5.2 .NET and matching .NET export templates with SDK 10.0.112 pinned in root global.json. One root DrivingTime.sln contains the net8.0 game and net10.0 pipeline/tests. Coordinates are linked source compiled by both consumers; the game does not reference the pipeline assembly.

Build/import and historical Windows export/startup are verified. Current environment inventory passes; the Helsinki road scene's Windows export was explicitly skipped and remains unverified. Review the game target before its recorded .NET 8 support end on 2026-11-10. See [setup](SETUP.md) and [dated evidence](TESTING.md).

## Driving

Use VehicleBody3D with VehicleCar, four suspension wheels, rear-wheel propulsion, front steering, chase camera and R/fall recovery. HelsinkiRoad.tscn is the default real-road prototype; VehicleExperiment.tscn and Main.tscn retain the synthetic vehicle and CharacterBody3D comparison scenes. Keyboard, third-person view, daylight and single player remain the scope. No traffic, pedestrians, multiplayer, missions or detailed vehicle simulation.

Automated road driving passes; appearance, driving feel and the reported jitter remain unverified. See [manual route](RUN.md#first-helsinki-road-drive).

## Coordinates and area

Use EPSG:3067 projected E,N metres, double precision until localization, and X=east−originEast, Y=height−originHeight, Z=−(north−originNorth), right-handed. Validated real elevated assets use N2000; the current road uses explicitly synthetic local Y=0.

The provisional 1 km Stenbäckinkatu square has origin E=384236.0361374, N=6674210.89320437, H=13.52536678314209 m; bounds E=383736.0361374..384736.0361374 and N=6673710.89320437..6674710.89320437. The address/terrain anchor is not a vehicle spawn or independent vertical control. Keep it fixed and version any change. Synthetic fixtures retain their separate local origin.

ProjNET 2.1.0 supplies bounded managed horizontal projection. EPSG:4326 needs explicit approximate-datum opt-in; exact epoch/grid/geoid conversion is unsupported. Independent heights remain unvalidated. See [coordinate contract](COORDINATES.md) and [source registry](../config/source-registry.json).

## Data and assets

Semantic roads, buildings and terrain are separate inputs. Use bounded OSM JSON road retrieval; CityGML WFS is the preferred building source, initially LoD1; the served 1 m WCS terrain is the provisional terrain source. Production building/terrain readers remain to be selected. Terrain metadata inconsistencies and footprint fallback terms need resolution before use. Physical access/construction and surveyed road width/elevation are unverified.

Use SharpGLTF.Toolkit 1.0.7 for deterministic GLB/manifest output. Stage ignored assets under game/Generated before Godot import/export; load imported resources through Godot APIs. Keep bulk sources and generated assets out of ordinary Git. Retain provenance, license, retrieval date, checksum, CRS/datum, source links and limitations; no unreviewed redistribution is authorized. Current source/format/terms evidence: [data sources](DATA_SOURCES.md); output workflows: [pipeline](PIPELINE.md).

## Provisional expansion and performance

500 m square tiles and two initial neighbors are provisional; tile ownership, borders, collision retention and bounded streaming must be verified before expansion. Ground-level roads come first; no stacked structures, bridges or tunnels in the initial route. Photogrammetry is optional reference/scenery.

60 FPS at 1080p is a target, not a result. Reference hardware and frame-time measurements remain unverified. No area expansion or generic infrastructure is justified by scaffolding alone.
