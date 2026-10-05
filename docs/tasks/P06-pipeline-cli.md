# P06: Create the offline pipeline CLI

Status: done
Type: implementation
Dependencies: P01
Goal: A deterministic offline synthetic manifest CLI with validated configuration.
Inputs: P01 setup guidance and D013 installed SDK 10.0.112; planned net10.0 pipeline/tests; game solution only if already present.
Scope: Minimal independent C# pipeline CLI, explicit NuGet versions if needed, region/origin validation, versioned manifest, deterministic order, useful errors and targeted .NET validation tests.
Out of scope: Downloads, geospatial conversion frameworks, extra language/runtime/GIS setup and speculative dependencies.
Deliverables: Pipeline project in repository solution, documented dotnet run command and small .NET test project.
Acceptance:
- [x] Automated / research: SDK 10.0.112; locked restore, pipeline build, root Debug/ExportRelease builds and 15 .NET tests pass. Two documented local no-restore runs produce byte-identical manifests. Invalid tile size and missing configuration exit 2 with actionable stderr.
- [x] Manual / review: Agent followed/reviewed documented WSL CLI steps and inspected v1 manifest. No additional runtime or download calls in pipeline; no visual acceptance criterion. User replay not performed.
- [x] Plan completion: After initial restore, documented no-restore command twice and cmp exit 0; no diagnostic metadata exclusions. Build/test pass. No OS network isolation claimed; local offline path uses cached SDK/assets without restore or network APIs.
Evidence: 2026-10-03; [PIPELINE.md](../PIPELINE.md), [TESTING.md](../TESTING.md#offline-synthetic-manifest-cli). Pipeline and test projects target net10.0 in DrivingTime.sln alongside independent net8.0 game. Built-in JSON/SHA-256/files only in CLI; three exact test PackageReferences and reviewed lock files. Sample configuration validates synthetic region/origin/tile size/relative manifest path. Agent CLI/document review performed; no user manual run, game export or geographic retrieval in P06.
Risks / blockers: No blocker. Geographic CRS/origin/datum and engine transforms are unselected; fixture is synthetic only. Primitive descriptions, no meshes or tiles. Initial restore requires network; sandbox vulnerability-service failure was resolved by approved restore retry. Writes are not atomic. No P07/P08 or gate completion inferred.
Follow-up tasks: P07; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P06. Create a minimal C#/.NET console CLI in pipeline/ and add it to the repository solution alongside the game when present. Use the SDK verified in P01, explicit NuGet versions and a documented restore policy. Read validated region/origin configuration and write a versioned manifest for a synthetic scene, using built-in .NET facilities where sufficient. Include one documented dotnet run command, deterministic output ordering, clear input errors and a small .NET test project for nontrivial validation. Start with no geospatial downloads. Document a WSL-development workflow that requires no Python runtime, Node.js, container or external GIS CLI; the pipeline stays in WSL and only exported game runtime files go to Windows. Add only dependencies already needed for this task.
```
