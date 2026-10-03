# P05: Add collision and safe recovery

Status: awaiting-manual-test
Type: implementation
Dependencies: P04
Goal: A vehicle that collides and recovers safely in the test scene.
Inputs: P04 car/camera, defined spawn and primitive scene.
Scope: Static obstacles and slope; verify chosen collision model; reset/fall recovery with configurable world limit; clear motion and camera state.
Out of scope: Whole-controller redesign; split any verified independent defect.
Deliverables: Three StaticBody3D blocks and 10-degree ramp in Main.cs; Spawn marker in Main.tscn; R action and configurable RecoveryHeight in Car.cs; camera reset in ChaseCamera.cs; C# RecoveryChecks scene; updated run guide and G1 evidence.
Acceptance:
- [x] Automated / research: Run the verified C# build and any targeted recovery checks justified by the implementation; record actual results.
- X ] Manual / review: Hit obstacle, drive slope, fall out of bounds and reset: safe spawn without retained motion; record G1.
- [X] Plan completion: Hit an obstacle, drive the slope, fall out of bounds and reset. The vehicle returns safely without retained motion. Record G1 results.
Evidence: 2026-10-03, SDK 10.0.112/Godot 4.5.2 .NET in WSL2. Debug/ExportRelease builds: 0 warnings/errors. Headless import passed. RecoveryChecks passed 1411 physics ticks at 60 FPS cap, including all three obstacles, independent collider-interior overlap queries, reverse escape, slope climb/brake/idle/descent, R with held driving input, held/repeated R, camera reset from a compressed/turned state, and natural fall recovery with threshold overridden to -3 m. P03 DrivingChecks passed 720 ticks with unchanged checkpoint trace; P04 CameraChecks passed 420 ticks. Windows export/transfer passed: artifact artifacts/windows/20261003-164321-f5563ae3, 188 SHA-256-verified runtime files (168.2 MiB) at W:\projektit\driving-time. Native startup and manual drive: unverified. Full commands, initial failures and sources: [TESTING.md](../TESTING.md#p05--collision-and-safe-recovery). Steps: [RUN.md](../RUN.md#p05-manual-test). G1 remains open pending manual evidence; earlier tasks are not closed by this session.
Risks / blockers: Upright arcade box collider with scripted gravity; cabin/wheels are cosmetic, no suspension, bounce or tipping. Angular motion is not swept by MoveAndSlide; sharp turns against corners remain manual coverage. Spawn marker must be clear of solids and above RecoveryHeight (default -10 m). R clears motion immediately; driving input still held on the next tick starts fresh acceleration. Ramp's high end is an intentional drop. Appearance/feel/interpolation and native performance await manual testing.
Follow-up tasks: P06 (recommended independent next task), P13 after required dependencies; no implementation implied. No controller child needed for the tested modest checks: the narrow impact-speed defect was corrected within P05 without changing the movement model.

Collision finding: First impact run stopped actual velocity but retained SignedSpeed=12 m/s, delaying reverse by braking invisible forward speed. Car.cs now reduces SignedSpeed to remaining heading velocity only on wall contact. Subsequent impact/reverse checks pass. No whole-controller redesign.

Final automated follow-up: RecoveryChecks also passed at 30 FPS cap (1411 ticks, 3 recoveries), including the final failure-exit guard; final Debug build and git diff --check passed. Frame caps do not imply achieved native FPS or manual acceptance.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P05. Add several simple static obstacles and a slope to the test scene. Verify the car's chosen collision model. Add a reset action to a defined spawn and automatic recovery below a configurable world limit. Clear velocity and camera state on reset. If the current movement approach cannot satisfy these modest tests, describe the narrow defect and propose a focused child task. Do not redesign the whole controller.
```
