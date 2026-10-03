# driving-time

A planned non-commercial desktop 3D driving game on recognizable Helsinki streets. Start with a synthetic driving scene, then one real road and nearby buildings; an approximately 1 × 1 km area near Töölönlahti is provisional. C#/.NET is required for both the game and offline data pipeline. The selected first target is Windows desktop x64, using Godot 4.5.2 .NET and the installed SDK 10.0.112. The pipeline will target net10.0; P02 verifies the Godot-generated game target and build/export compatibility. All development, editing, builds, pipeline work and export run in WSL; only the exported playable game (with required runtime files) is transferred to Windows. Builds and exports remain unverified.

Only the planning/research workspace exists. No game project, pipeline or dependencies have been created. See [WSL setup and Windows export](docs/SETUP.md) for exact required tools, inventory and installation steps. global.json pins the installed .NET 10 SDK; editor/templates remain unavailable in inspected locations. Any retained net8.0 game target needs review before its November 10, 2026 support end (D013).

Read [AGENTS.md](AGENTS.md), the [plan](HELSINKI_DRIVING_PLAN.md), [project state](docs/PROJECT_STATE.md), [decisions](docs/DECISIONS.md), [testing record](docs/TESTING.md), and [task index](docs/tasks/INDEX.md). P00–P01 are complete; P02–P27 are todo, with P09–P27 deferred. Execute one selected task in dependency order.

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

Next: [P02 — Launch and export an empty 3D scene](docs/tasks/P02-empty-scene.md). Setup/version checks are documented; game build/run commands await that project task.
