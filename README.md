# driving-time

A non-commercial desktop prototype for driving through recognizable Helsinki streets. The selected area is a provisional 1 × 1 km square around Stenbäckinkatu. C#/.NET is required for both the Godot game and offline data pipeline. Development, editing, builds, tests and Windows export run in WSL2; only exported runtime files transfer to Windows for playtesting.

The game has a VehicleBody3D car, chase camera, synthetic loop, obstacles, ramp and recovery. The pipeline can retrieve a bounded road sample, normalize its graph and generate a flat 100 m road strip with separate collision geometry. The real strip is not yet integrated into a playable driving scene. Automated build/import/export evidence exists; visual and driving checks remain open.

- [Setup and environment checker](docs/SETUP.md)
- [Run, export and manual checks](docs/RUN.md)
- [Offline pipeline](docs/PIPELINE.md)
- [Coordinates](docs/COORDINATES.md) and [data sources](docs/DATA_SOURCES.md)
- [Current state](docs/PROJECT_STATE.md), [decisions](docs/DECISIONS.md) and [test evidence](docs/TESTING.md)
- [Repository instructions](AGENTS.md), [backlog](HELSINKI_DRIVING_PLAN.md) and [task ledger](docs/tasks/INDEX.md) for upcoming changes

To resume work, read the repository instructions, current state, decisions and task ledger. Inspect existing code and select one ready change from the backlog. Record actual checks and preserve outstanding manual acceptance.
