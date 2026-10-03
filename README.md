# driving-time

A non-commercial desktop 3D driving game prototype on recognizable Helsinki streets. Start with a synthetic scene, then one real road and nearby buildings; an approximately 1 × 1 km area near Töölönlahti is provisional. C#/.NET is required for both the game and offline data pipeline. The first target is Windows desktop x64, using Godot 4.5.2 .NET and SDK 10.0.112. The game targets net8.0; the pipeline will target net10.0. All development, editing, builds, pipeline work and export run in WSL; only the exported playable game (with required runtime files) is transferred to Windows.

P02 provides a minimal Godot .NET scene with a floor, light, camera and reference cube. [Run and export guide](docs/RUN.md) gives exact WSL build/editor/export and native Windows test commands. Build, Windows export, Linux startup and native Windows startup pass; visual acceptance remains pending. The net8.0 game target needs review before its November 10, 2026 support end (D014). No vehicle, map data or pipeline exists.

Read [AGENTS.md](AGENTS.md), the [plan](HELSINKI_DRIVING_PLAN.md), [project state](docs/PROJECT_STATE.md), [decisions](docs/DECISIONS.md), [testing record](docs/TESTING.md), and [task index](docs/tasks/INDEX.md). P00–P01 are complete; P02 awaits manual testing; P03–P27 remain todo, with P09–P27 deferred. Execute one selected task in dependency order.

To resume in Codex, paste:

```text
Read AGENTS.md, HELSINKI_DRIVING_PLAN.md, docs/PROJECT_STATE.md,
docs/DECISIONS.md and docs/tasks/INDEX.md. Inspect the working tree and
relevant existing code. Summarize current evidence, open manual checks and
blockers. Select the next ready task in dependency order and state its ID.
Execute only that task under the execution contract. If the next task awaits
my manual check, work only on an independent ready task; do not assume it passed.
Update the ledger and state, and end with one recommended next task ID.
```

Next: confirm P02 visually using [RUN.md](docs/RUN.md), then select [P03](docs/tasks/P03-car.md).
