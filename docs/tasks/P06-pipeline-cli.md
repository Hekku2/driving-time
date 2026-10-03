# P06: Create the offline pipeline CLI

Status: todo
Type: implementation
Dependencies: P01
Goal: A deterministic offline synthetic manifest CLI with validated configuration.
Inputs: P01 setup guidance and D013 installed SDK 10.0.112; planned net10.0 pipeline/tests; game solution only if already present.
Scope: Minimal independent C# pipeline CLI, explicit NuGet versions if needed, region/origin validation, versioned manifest, deterministic order, useful errors and targeted .NET validation tests.
Out of scope: Downloads, geospatial conversion frameworks, extra language/runtime/GIS setup and speculative dependencies.
Deliverables: Pipeline project in repository solution, documented dotnet run command and small .NET test project.
Acceptance:
- [ ] Automated / research: Planned: dotnet build / dotnet test on actual project paths; after documented initial restore, run documented dotnet run command twice offline and compare outputs; invalid configuration fails with actionable errors.
- [ ] Manual / review: Follow documented WSL CLI steps after initial restore; offline execution requires no extra runtime.
- [ ] Plan completion: After the documented initial restore, run the command twice offline and compare outputs; they match except explicitly excluded diagnostic metadata. Invalid configuration fails clearly. The CLI builds and its targeted checks run with dotnet build / dotnet test.
Evidence: Automated / research: not run. Manual / review: not run. Build/export: not run. Commands above are planned, not completed.
Risks / blockers: Pinned Linux SDK 10.0.112 is available; project/dependency restore and build remain untested. Deterministic metadata exclusions must be explicit.
Follow-up tasks: P07; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P06. Create a minimal C#/.NET console CLI in pipeline/ and add it to the repository solution alongside the game when present. Use the SDK verified in P01, explicit NuGet versions and a documented restore policy. Read validated region/origin configuration and write a versioned manifest for a synthetic scene, using built-in .NET facilities where sufficient. Include one documented dotnet run command, deterministic output ordering, clear input errors and a small .NET test project for nontrivial validation. Start with no geospatial downloads. Document a WSL-development workflow that requires no Python runtime, Node.js, container or external GIS CLI; the pipeline stays in WSL and only exported game runtime files go to Windows. Add only dependencies already needed for this task.
```
