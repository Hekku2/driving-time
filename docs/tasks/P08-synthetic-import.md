# P08: Import one generated GLB and manifest

Status: todo
Type: implementation
Dependencies: P02, P07
Goal: One generated synthetic GLB and manifest imported at correct scale and orientation.
Inputs: P02 Godot project/export; P07 contract/fixtures; P06 CLI.
Scope: C# GLB writer verified with building-like cube and direction markers; versioned bounds/units/origin/path manifest; reproducible engine-readable staging and inspection scene; single axis conversion.
Out of scope: Real datasets, streaming, Blender and extra scripting runtimes.
Deliverables: Small synthetic GLB/manifest, writer/staging workflow and inspection scene.
Acceptance:
- [ ] Automated / research: Run targeted .NET manifest/bounds/mesh checks and verified Godot import/export commands; validate winding, normals, materials and exactly one axis conversion.
- [ ] Manual / review: Inspect markers and 10 m cube in Godot and export: correct scale/directions/materials; no mirroring or double rotation.
- [ ] Plan completion: Inspect the markers and a 10 m cube in Godot and an export. Automated bounds and manifest validation pass. Import has no unexplained mirroring or double rotation.
Evidence: Automated / research: not run. Manual / review: not run. Build/export: not run. Commands above are planned, not completed.
Risks / blockers: Writer compatibility, winding/normals and exported asset availability require evidence.
Follow-up tasks: P12, P18; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P08. Generate one small synthetic GLB containing a building-like cube and direction markers through the C# pipeline. Verify a suitable .NET glTF/GLB writer with this fixture; avoid adding Blender or a scripting runtime to the required build. Define a minimal versioned manifest including bounds, units, coordinate origin and asset paths. Choose a reproducible staging/import workflow into the Godot project that also works in exports. Display the generated scene in a separate inspection scene. Verify winding, normals, materials, scale and axis conversion happen exactly once. Keep fixtures small; do not add real data or streaming.
```

