# Project state

Goal: Small drivable Helsinki desktop prototype  
Current gate: G0 — open; bootstrap and toolchain research complete<br>
Last completed task: P01<br>
Next recommended task: P02<br>
Updated: 2026-10-03

## Verified environment

- Agent session: Ubuntu 24.04.4 LTS under WSL2, x86_64. Linux SDK 10.0.112/runtime 10.0.12 and Git 2.43.0 observed.
- Windows interop inspection: Windows NT 10.0.26200.0, PowerShell 5.1.26100.9444; SDKs 3.0.100/3.1.100/5.0.102/5.0.200, runtime 8.0.4, Git 2.27.0.windows.1. Physical architecture/product name not confirmed.
- Selected setup: Godot 4.5.2-stable .NET and matching .NET templates; installed WSL SDK 10.0.112 pinned via global.json (D013). Pipeline/tests planned net10.0; P02 verifies Godot-generated game target, runtime loading and export. One root solution when projects exist. Official documentation checked; build compatibility not exercised.
- Selected SDK is available in WSL; SDK inventory/selection reconfirmed 2026-10-03. Godot absent from PATH/common inspected locations and default template directories absent; portable/custom copies elsewhere are unverified.
- Windows desktop x64 remains the first runtime target. Fixed workflow requirement (D012): develop/edit/build/test/convert/export in WSL2, then transfer only the runnable exported game to Windows. Use the Linux Godot .NET editor and Linux SDK; Windows development tools are not required. Local setup not performed.
- Required language: C#/.NET for game, offline pipeline, retrieval tools and automated tests.
- Reference hardware: unknown.

## Selected world configuration

- Region bounds / source CRS: not selected; approximately 1 × 1 km near Töölönlahti is provisional.
- Vertical datum: unverified.
- Geographic origin / engine axis mapping: not selected; P07 must verify.
- Tile size: provisional 500 m.
- Supported structures: ground-level roads provisionally; stacked structures deferred.

## Evidence

- P00: plan read; execution instructions, README, state, decisions, testing record, index and P01–P08 cards created. Original `HELSINKI_DRIVING_PLAN.md.md` filename normalized to `HELSINKI_DRIVING_PLAN.md`; content preserved.
- Documentation validation: see [TESTING.md](TESTING.md).
- Local setup helper (2026-10-03): created executable `scripts/launch-godot-wsl.sh` for SETUP.md step 4 and ignored that exact path in Git at the user's request. `bash -n scripts/launch-godot-wsl.sh` passed; `git check-ignore -v scripts/launch-godot-wsl.sh` confirmed exclusion. Running the helper exited 1 with the expected missing-editor path and step 3 guidance. DISPLAY=:0 and WAYLAND_DISPLAY=wayland-0 were present; actual WSLg rendering/GPU and editor launch remain unverified. No task or gate status changed.
- Build/export: not run.
- Local setup follow-up (2026-10-03): created executable, Git-ignored `scripts/setup-godot-wsl.sh` for SETUP.md step 3. It caches the selected editor/templates, records local SHA-256/retrieval evidence, and copies editor/templates to WSL user directories with path overrides. `bash -n scripts/setup-godot-wsl.sh`, `./scripts/setup-godot-wsl.sh --help`, `git check-ignore -v scripts/setup-godot-wsl.sh` and `git diff --check` passed. Download/install execution and archive-layout verification remain unperformed. SETUP.md now uses the .NET template directory `4.5.2.stable.mono`, following the tagged official installer. No task or gate status changed.
- Runtime automated checks: not run.
- Editor, exported application and manual driving checks: not run.
- Performance baseline: not measured.
- P01: inventory and official documentation research completed; [SETUP.md](SETUP.md) and D011 record original setup, controller choice and provisional target. D013 updates the SDK pin to installed 10.0.112 after user approval; `dotnet --version` succeeds. Historical SDK rejection is superseded.
- No game project, pipeline project, dependencies or downloads created. P02–P27 remain todo; P09–P27 are deferred backlog entries.

## Current blockers and limits

P01 research is complete; P02 can be selected but build/export needs the missing Linux editor/templates, WSLg checks and SDK 10/Godot runtime/export validation. The SDK is available and the existing WSL checkout remains the development workspace. No implementation or phase gate is validated. Reference hardware, data access and world configuration remain unresolved. Review any retained .NET 8 game target before its 2026-11-10 support end.

## Awaiting manual tests

None requiring a task status transition. P01 setup instructions received agent review; full user setup execution is not run. Optional local check: follow SETUP.md; expect SDK 10.0.112 and, after editor/template setup, Godot 4.5.2.stable.mono version output and matching .NET templates. No game build exists to test.

## Decisions and source registry

- [Decisions](DECISIONS.md): D001 fixed language requirement; D012 fixed WSL workflow; D013 user-approved .NET 10 SDK selection; D002/D004 selected engine/controller baseline, runtime unverified; D011 historical setup rationale; remaining assumptions provisional.
- [Setup](SETUP.md): official sources checked 2026-10-02/03; WSL setup, cross-export and exported-game transfer steps.
- [Plan](../HELSINKI_DRIVING_PLAN.md): authoritative prompts and phase gates.
- [Task index](tasks/INDEX.md): statuses and dependencies.
- Source registry and `docs/DATA_SOURCES.md`: not created; P09 will verify actual products and terms.

## Next session

Read the repository instructions, [setup](SETUP.md) and [P02 card](tasks/P02-empty-scene.md). Execute P02 only: resolve required setup under task authorization, create the minimal scene, build and attempt Windows export inside WSL, then transfer the runtime output and test it in Windows. Preserve separate evidence for export, editor run and local executable run. Do not implement the car. P06 is an independent ready task if selected instead; it also needs the pinned SDK.
