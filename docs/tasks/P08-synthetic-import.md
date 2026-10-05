# P08: Import one generated GLB and manifest

Status: awaiting-manual-test
Type: implementation
Dependencies: P02, P07
Goal: One generated synthetic GLB and manifest imported at correct scale and orientation.
Inputs: P02 Godot project/export; P07 contract/fixtures; P06 CLI.
Scope: C# GLB writer verified with building-like cube and direction markers; versioned bounds/units/origin/path manifest; reproducible engine-readable staging and inspection scene; single axis conversion.
Out of scope: Real datasets, streaming, Blender and extra scripting runtimes.
Deliverables: Small synthetic GLB/manifest, writer/staging workflow and inspection scene.
Acceptance:
- [x] Automated / research: Run targeted .NET manifest/bounds/mesh checks and verified Godot import/export commands; validate winding, normals, materials and exactly one axis conversion.
- [ ] Manual / review: Inspect markers and 10 m cube in Godot and export: correct scale/directions/materials; no mirroring or double rotation.
- [ ] Plan completion: Inspect the markers and a 10 m cube in Godot and an export. Automated bounds and manifest validation pass. Import has no unexplained mirroring or double rotation.
Evidence: 2026-10-04: SharpGLTF.Toolkit 1.0.7 MIT/net10.0 writer verified; 6,664-byte deterministic GLB with 10 m cube, E/N/up markers and ground slab. Locked restore, Debug/ExportRelease builds (0 warnings/errors), 33 .NET tests, Godot 4.5.2 import and inspection check pass. Nonzero origin test confirms localization once. Native Windows exported inspection check also passes (identity transforms, bounds/axes, single-sided materials, outward normals and clockwise imported winding). Export/transfer artifact `artifacts/windows/20261003-204544-dc05f013`, 188 runtime files SHA-256 verified at W:\projektit\driving-time. [Commands/results](../TESTING.md#synthetic-glb-import), [workflow/schema/sources](../PIPELINE.md#synthetic-glb-staging), [manual instructions](../RUN.md#synthetic-asset-inspection). Manual / review: not run; no visual acceptance inferred.
Risks / blockers: Human appearance/scale/directions in editor and native Windows remain unverified. Synthetic only, no collisions/textures/real data/streaming; game net8.0 lifecycle limit unchanged. Generated files are ignored and require staging before import; default pinned Godot importer settings are used.
Follow-up tasks: P12, P18; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P08. Generate one small synthetic GLB containing a building-like cube and direction markers through the C# pipeline. Verify a suitable .NET glTF/GLB writer with this fixture; avoid adding Blender or a scripting runtime to the required build. Define a minimal versioned manifest including bounds, units, coordinate origin and asset paths. Choose a reproducible staging/import workflow into the Godot project that also works in exports. Display the generated scene in a separate inspection scene. Verify winding, normals, materials, scale and axis conversion happen exactly once. Keep fixtures small; do not add real data or streaming.
```
