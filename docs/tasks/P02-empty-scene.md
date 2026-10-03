# P02: Launch and export an empty 3D scene

Status: todo
Type: implementation
Dependencies: P01
Goal: A minimal 3D scene that builds and exports using the verified toolchain.
Inputs: P01 setup/versions, D012 WSL workflow, D013 installed SDK 10.0.112 selection and docs/SETUP.md; existing repository solution if present.
Scope: Main scene, floor, light, camera, reference cube, run guide and credential-free Windows export preset.
Out of scope: Vehicle and map data.
Deliverables: Minimal game project, scene, build/run guide and export preset.
Acceptance:
- [ ] Automated / research: Run the P01-defined C# build and attempt Windows export inside WSL; inventory required runtime files for transfer; record actual commands/results and environment limitations separately.
- [ ] Manual / review: Open the Linux editor scene through WSLg and run the transferred Windows export: floor/cube visible, no startup errors; record export and local run separately.
- [ ] Plan completion: Open the scene and exported application: the floor and cube are visible, with no startup errors. Actual export success and local run success are recorded separately.
Evidence: Automated / research: not run. Manual / review: not run. Build/export: not run. Commands above are planned, not completed.
Risks / blockers: Godot/templates and WSLg availability unresolved. Verify Godot-generated target, SDK 10 build, editor runtime loading and WSL-to-Windows cross-export; SDK discovery alone does not prove these checks.
Follow-up tasks: P03, P08; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P02. Create the smallest Godot .NET project with a main 3D scene, visible floor, light, camera and simple reference cube. Include a run guide and a Windows export preset without credentials. Build the C# project and attempt a Windows desktop export inside WSL using the Linux .NET/Godot toolchain. Keep development in WSL and transfer only the exported runnable game, including required companion files, to Windows for native testing. If the environment cannot run the editor or Windows executable, record that limitation and give exact local steps. Do not add a vehicle or map data.
```
