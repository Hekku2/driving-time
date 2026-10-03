# P01: Verify the toolchain and choose the first slice

Status: done
Type: research
Dependencies: P00
Goal: One evidence-backed toolchain and first-slice decision.
Inputs: Plan, D001–D004/D009, installed tools and official documentation.
Scope: Inspect OS/Godot/.NET/Git; verify exact versions, framework/export compatibility; document missing tools, Windows target, minimal controller comparison and provisional performance target.
Out of scope: Gameplay, dependency installation, project scaffolding and system-wide installs.
Deliverables: docs/SETUP.md, D011 and updated D002–D004, root global.json justified by official compatibility documentation, inventory and evidence updates. No project or install.
Acceptance:
- [x] Automated / research: Document installed-tool/version inspection commands and outcomes; link official compatibility sources with checked dates, or label unavailable verification explicitly.
- [x] Manual / review: Review setup steps: exact versions or missing tools and reproducible verification steps are explicit.
- [x] Plan completion: A developer can follow the setup instructions; versions and missing tools are explicit. No engine API or support claim is presented as verified without evidence.
Evidence: Research completed 2026-10-02; see docs/SETUP.md, docs/TESTING.md and D011. Agent reviewed setup instructions for explicit versions/missing tools and separately labeled future local checks. Original Windows setup execution by user: not run; superseded by D012 WSL setup, also not run. Build/export/editor: not run. P01 permits documenting required installs; research completion does not certify a ready machine or pass G0.
Risks / blockers: D013 supersedes the original missing SDK baseline: installed WSL SDK 10.0.112 is now pinned and verified; pipeline/tests planned net10.0. Godot/editor location and matching templates remain unavailable in inspected locations. P02 verifies the Godot-generated game target, SDK 10 build/runtime/export compatibility, Windows target architecture, WSLg and runtime-output handoff. Any retained net8.0 target needs support review before 2026-11-10. No blocker to completion of P01 research.
Follow-up tasks: P02, P06; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P01 under the plan's execution contract. Inspect available operating systems, Godot .NET edition, .NET SDK and Git. Verify the engine/SDK/export-template compatibility using official documentation; select and record exact versions, or document required installs if unavailable. Confirm Windows desktop as the first target and record the required C#/.NET stack for both game and pipeline. Prefer one SDK selected through global.json and a single .NET solution; verify supported target frameworks and document any unavoidable differences. No second language runtime is part of the required setup. Choose a simple arcade controller for the first driving experiment; compare only the minimum relevant options. Set a provisional 1080p performance target and record that the reference hardware is still unknown if it cannot be measured. Produce setup instructions and a decision record. Do not implement gameplay or install system-wide tools without a concrete need.
```
