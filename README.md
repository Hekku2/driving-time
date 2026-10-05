# driving-time

A non-commercial desktop prototype for driving through recognizable Helsinki streets. The selected area is a provisional 1 × 1 km square around Stenbäckinkatu. C#/.NET is required for both the Godot game and offline data pipeline. Development, editing, builds, tests and Windows export run in WSL2; only exported runtime files transfer to Windows for playtesting.

The default game scene drives a VehicleBody3D car on a flat 100 m Stenbäckinkatu strip, with a chase camera, reference markers, temporary ground and recovery. Separate synthetic loop/obstacle/ramp scenes remain available. The offline pipeline retrieves a bounded road sample, normalizes its graph and stages separate visual/collision assets. Automated road driving and build/import checks pass; manual appearance/driving and the new scene's Windows export remain unverified.

- [Setup and environment checker](docs/SETUP.md)
- [Run, export and manual checks](docs/RUN.md)
- [Offline pipeline](docs/PIPELINE.md)
- [Coordinates](docs/COORDINATES.md) and [data sources](docs/DATA_SOURCES.md)
- [Current state](docs/PROJECT_STATE.md), [decisions](docs/DECISIONS.md) and [test evidence](docs/TESTING.md)
- [Repository instructions](AGENTS.md), [backlog](HELSINKI_DRIVING_PLAN.md) and [task ledger](docs/tasks/INDEX.md) for upcoming changes

To resume work, read the repository instructions, current state, decisions and task ledger. Inspect existing code and select one ready change from the backlog. Record actual checks and preserve outstanding manual acceptance.
