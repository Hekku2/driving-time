# Testing and evidence

## P00 documentation checks

2026-10-02: the plan and repository contents were inspected using `rg --files`, `cat`, `sed`, and `git status --short`. The supplied plan filename had an extra `.md`; `mv HELSINKI_DRIVING_PLAN.md.md HELSINKI_DRIVING_PLAN.md` normalized it without content edits.

Documentation validation passed: read all 14 generated/updated documentation files with `cat`; checked 44 relative file-link targets, all 28 index rows and statuses, all eight task cards' required fields, and their execution prompts against the source plan in a temporary in-session JavaScript check. P00 is done; P01–P27 are todo. `rg --files docs` confirmed the documentation inventory. `git diff --check` exited 0 (covering the tracked README change; new files are untracked). No game/pipeline build, restore, test, export, editor launch, dataset retrieval or performance check has run.

Optional manual review: read AGENTS.md, PROJECT_STATE.md and tasks/INDEX.md. Expected: only P00 is done, P01–P27 are todo, P09–P27 are deferred, C#/.NET is fixed for both projects, other assumptions are provisional, one selected task is permitted, and unperformed manual checks cannot pass.

## Future checks

SDK and engine selections are recorded in SETUP.md and D013; actual project paths and build/run commands await P02/P06. Commands in task cards are planned checks, not successful results. Do not install or create projects during a documentation update.

| Tasks | Automated / command evidence | Manual evidence |
|---|---|---|
| P01 | Tool/version inspection and official compatibility sources with checked dates | Review setup instructions; no runtime pass implied |
| P02 | C# build and Windows export, separately recorded | Open editor scene and exported application; floor/cube visible without startup errors |
| P03–P05 | C# build; frame-rate checks where available | Two-minute drive, chase camera wall/turn checks, collision/slope/fall/reset checks |
| P06 | Initial restore, build/test, two offline CLI runs with deterministic comparison, invalid-input failure | Follow the documented WSL CLI steps |
| P07 | Cardinal points, 100 m length, nonzero elevation, round trips, trusted-reference comparison | Review axis/CRS/datum contract; no visual substitute for numeric evidence |
| P08 | Manifest/bounds/mesh validation and export | Inspect direction markers and 10 m cube in Godot and export |
| P09–P27 | Defined when their cards are expanded from actual inputs | Follow each plan completion check; record G3–G6 only with evidence |

Use targeted .NET tests for nontrivial parsing, coordinates, geometry and tile lifecycle behavior. Use manual checks for appearance and feel. Store command, environment/build identifier, actual result, limitations and source checked dates where relevant. A build passing does not prove a manual criterion; use `awaiting-manual-test` until the required feedback arrives. Parent tasks need evidence for every required child and their own acceptance criteria.

## P01 — actual research evidence

Checked 2026-10-02. No tools installed, packages restored, projects created or editor/export/run tests attempted.

| Actual command / inspection | Outcome |
|---|---|
| `uname -a`; `cat /etc/os-release` | Ubuntu 24.04.4, x86_64, WSL2 kernel 6.6.87.2-microsoft-standard-WSL2 |
| `command -v dotnet godot godot4 godot-mono git pwsh powershell.exe cmd.exe wsl.exe` | Linux dotnet/git/pwsh and Windows PowerShell/cmd/wsl found; Godot names absent |
| `dotnet --info` before global.json | SDK 10.0.112; .NET runtime 10.0.12; no workloads; no global.json |
| `git --version` | 2.43.0 |
| `rg --files --hidden` filtered for global.json, solution, csproj and project.godot before changes | No matching repository projects or SDK pin |
| Windows PowerShell inventory shown below | Sandbox attempt failed with `UtilBindVsockAnyPort:307: socket failed 1`; escalated read-only retry produced Windows/.NET/Git inventory |
| `ls -ld` of common Linux Godot/template and Windows Program Files/Godot directories | Inspected Godot/template locations absent; Windows dotnet/sdk exists |
| `ls -d /mnt/c/Users/*/AppData/Roaming/Godot/export_templates /mnt/c/Users/*/Downloads/*[Gg]odot* /mnt/c/Users/*/Desktop/*[Gg]odot* /mnt/c/Tools/*[Gg]odot* /mnt/c/Godot` | No matches; exit 2 from missing paths, not proof of absence on every drive |
| Additional bounded Windows PowerShell CIM/common-location probe | Exit 0 but blank output; no reliable extra evidence, architecture/product name left unverified |
| Broader `rg --files` search under common user locations for Godot names | Only unrelated game logs returned; permission errors occurred. No editor found; not an exhaustive installation inventory |
| `dotnet --version` after global.json | Expected SDK-not-found failure requesting 8.0.425, standalone exit 155; installed 10.0.112 cannot satisfy exact pin |
| `dotnet --list-sdks` after pin | Still lists 10.0.112; inventory works without compatible selected SDK |
| `git diff --check` | Initial final check found Markdown hard-break whitespace in changed state lines; converted those breaks to HTML br tags. Final check passed, exit 0 |

Final in-session documentation validation passed: parsed global.json and checked exact version/roll-forward/prerelease values; checked all 28 ledger rows (P00/P01 done, P02–P27 todo), P01 card status and 50 relative file-link targets in the changed documents. Commands were read-only `cat` / `test -f` checks plus temporary JavaScript assertions in the agent tool session; no test scripts or extra runtime dependencies were added to the repository.

The Windows inventory command was `powershell.exe -NoProfile -NonInteractive -Command` with the following read-only body:

```powershell
$PSVersionTable.PSVersion.ToString()
[System.Environment]::OSVersion.VersionString
Get-Command dotnet,git,godot,godot4 -ErrorAction SilentlyContinue | Select-Object Name,Source
if (Get-Command dotnet -ErrorAction SilentlyContinue) { dotnet --info }
if (Get-Command git -ErrorAction SilentlyContinue) { git --version }
Get-ChildItem "$env:APPDATA\Godot\export_templates" -ErrorAction SilentlyContinue | Select-Object Name
```

Actual output: PowerShell 5.1.26100.9444; Windows NT 10.0.26200.0; Windows SDKs 3.0.100, 3.1.100, 5.0.102 and 5.0.200; default SDK 5.0.200; x64 host/runtime 8.0.4; Git 2.27.0.windows.1. Godot commands/templates did not appear. The combined command exited 1 after the silent missing-template lookup; earlier inventory output remains valid. OS version wording alone is not a Windows product-name measurement.

Official sources and checked dates are linked in [SETUP.md](SETUP.md) and D011. Read Godot's 4.5 C# prerequisites, 4.5 Windows/general export docs, 4.5 body API docs, 4.5.2 archive, Microsoft SDK/TFM/SDK-selection/support pages. Tagged Godot project-generator source fetches failed; no generated-project source verification claimed. Archive download redirects were unavailable in the web tool; no binary retrieval claimed. The newer 4.7.2 download listing was observed, but was not enough to establish its exact framework baseline.

Agent review: setup steps contain exact selections, bounded local setup, expected outputs and missing-tool paths; the SDK pin and task ledger were reviewed. This satisfies P01's document-review criterion. User execution of those steps: **not run**. Original setup check superseded by D012 on 2026-10-03. Optional local manual check: after supplying the selected Linux tools, follow SETUP.md's WSL Bash version checks; expect SDK 8.0.425, Godot 4.5.2.stable.mono and matching .NET templates. Editor launch, export and exported-app verification belong to P02 and remain unperformed. G0 remains open.

## 2026-10-03 — WSL workflow documentation update

Updated setup, instructions, plan, decisions, state and affected task cards to honor WSL development and Windows runtime-only transfer. P00/P01 remain done; P02–P27 remain todo. Earlier inventory is historical; no tool installation, editor launch, export, transfer or gameplay implementation was performed.

Official Linux .NET archive, C# desktop/export documentation and Microsoft's WSLg guidance checked on this date; see SETUP.md and D012. The direct Microsoft Linux SDK thank-you URL and an attempted C# platform-specific documentation URL were unavailable in the browsing tool; setup links to the main SDK download page instead.

Manual check for this documentation change: read SETUP.md and D012. Expect Linux tools and WSL Bash instructions, source/pipeline remaining in WSL, and only exported runtime files copied to Windows. Actual WSLg/editor and Windows export/run checks remain P02 acceptance work.

Documentation validation passed: `git diff --check` exited 0; `rg` found no remaining active Windows-development/optional-WSL setup instructions; in-session assertions checked all 28 task statuses unchanged and verified P02/P06 execution prompts match the revised plan. Setup and D012 were reviewed for WSL development and runtime-only transfer; no implementation acceptance criterion was marked complete.

## 2026-10-03 — .NET 10 setup update

User approved .NET 10. Updated global.json to 10.0.112, setup, README, plan, decisions (D013), project state and affected task cards/index. Historical .NET 8 SDK evidence above remains a record of the earlier selection, superseded by D013. P00/P01 remain done; P02–P27 remain todo. Pipeline/tests planned net10.0; Godot-generated game target and runtime/export compatibility await P02.

Actual checks: `dotnet --info` reconfirmed installed Linux SDK 10.0.112 and runtime 10.0.12; after the pin change, `dotnet --version` returned 10.0.112 and `dotnet --list-sdks` listed 10.0.112 at /usr/lib/dotnet/sdk. `git diff --check` passed. Official Godot 4.5 C# and Microsoft SDK/version-selection/global.json/support pages rechecked; see SETUP.md and D013. No tool installation, project creation, restore, build, export or editor launch performed.

Manual check: run `dotnet --version` from the WSL repository root and read SETUP.md; expect 10.0.112, no .NET 8 SDK download requirement, and separate Godot runtime/export verification in P02. User execution: not run. Game compatibility and visual checks remain unverified; G0 remains open.
