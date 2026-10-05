# P01a: Check development environment prerequisites

Status: done
Type: implementation / validation
Dependencies: [P01](P01-toolchain.md)
Goal: One command reports required installed tools and actionable missing-item guidance.
Inputs: User request, global.json, game/DrivingTime.csproj, SETUP.md and existing setup/launch helpers.
Scope: Read-only C# inventory with a small Bash bootstrap for missing SDKs; WSL2, Linux x64 tools, pinned .NET 10 SDK, Git, selected .NET editor, Windows export templates, display and setup prerequisites.
Out of scope: Installs/downloads, gameplay, solution build/restore, editor launch, export or closure of visual gates.
Deliverables: scripts/check-environment.cs, scripts/check-environment.sh, setup usage and current evidence.
Acceptance:
- [x] One documented command runs and reports the actual installed/missing prerequisites with exit status and remedies.
- [x] Missing SDK has bootstrap guidance; custom editor override and help/invalid arguments are documented.
- [x] Current inventory is recorded separately from historical toolchain evidence; no install/launch/build/export success is inferred.
Evidence: 2026-10-05 `bash scripts/check-environment.sh`: exit 1 as expected; seven required checks pass, three Godot/editor/templates checks fail. SDK 10.0.112, Git 2.43.0, WSL2/x64 present. Godot/default templates absent; unzip missing; curl/sha256sum/display configuration present. `--help` exit 0; Bash syntax and whitespace checks pass. Initial run could not write the sandbox-restricted standard .NET file-app cache; approved retry compiled and ran successfully. See docs/TESTING.md for additional bootstrap checks. Manual GUI checks: not run or required for this inventory-only task.
Risks / blockers: Does not scan all disks or inspect template archive provenance/completeness beyond required version/Windows x64 files. Inventory is not proof of functional GUI, restore/build/export or Windows gameplay. Compiler requires the pinned SDK; Bash bootstrap covers its absence.
Follow-up tasks: Restore road cache and regenerate assets, P12 manual inspection, then P13.

## Road source-cache check — 2026-10-05

User-requested extension: read the payload/provenance paths from `config/road-graph.json`; require nonempty files bounded to 10 MiB and verify schema v1, byte count and SHA-256. Report missing/corrupt data with the road-sample download command and path-update guidance for changed payload checksums. No download, parser/converter or generated-asset checks occur. Actual checker compiles and exits 1: all ten toolchain checks pass, only the absent configured road files fail. Manual replay: run the checker and expect missing-file paths and download guidance followed by the prompt. Status remains done; no visual gate changes.

## Terminal-preservation correction — 2026-10-05

The launcher previously used `exec`, top-level `exit` and shell option changes, which could replace/terminate the user's Bash shell when sourced. It now runs in a subshell and invokes dotnet normally. Sourced missing-SDK and actual full preflight failure probes return exit 1 with the parent shell alive, cwd and options unchanged; executed help returns 0 with parent shell alive. `bash -n` and whitespace checks pass. No manual terminal window test performed; process survival is verified by subsequent commands in the calling Bash process. A caller using `set -e` must explicitly handle nonzero statuses; a terminal launched only to run a single command may close according to its terminal settings. Manual replay: run `bash scripts/check-environment.sh` in an existing terminal; expect output and the prompt to return. Task remains done, no gate changes; recommended next task P13 after prerequisites/P12 manual inspection.
