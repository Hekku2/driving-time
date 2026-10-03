# P04: Add a usable chase camera

Status: awaiting-manual-test
Type: implementation
Dependencies: P03
Goal: A chase camera that follows turns and avoids basic obstruction.
Inputs: P03 car and primitive scene.
Scope: Bounded smoothing, road-ahead view, basic obstruction handling and camera logic separate from input.
Out of scope: Multiple camera modes and cinematic effects.
Deliverables: game/ChaseCamera.cs, perspective rig in Main.tscn, wall outside the loop, C# CameraChecks scene and turning/wall guide in docs/RUN.md.
Acceptance:
- [x] Automated / research: Run the verified C# build and record results; do not equate build success with camera acceptance.
- [ ] Manual / review: Drive a tight loop and past a wall: smooth follow and no view from inside obstacles.
- [ ] Plan completion: Drive a tight loop and past a wall; the camera follows smoothly and does not leave the player viewing from inside an obstacle.
Evidence: 2026-10-03: Debug/ExportRelease builds passed with 0 warnings/errors; headless import passed. CameraChecks passed at 30/60/144 caps (420 physics ticks) and in WSLg at 60 cap. P03 DrivingChecks passed at 60 cap (720 ticks). Windows export/transfer passed: 188 SHA-256-verified runtime files, artifact 20261003-161322-f7a5a660. See [TESTING.md](../TESTING.md#p04--chase-camera). Manual / review: unverified; native launch/drive not run. User explicitly selected P04 while P03 manual acceptance remained open.
Risks / blockers: Obstruction behavior and tuning need visual evidence. Basic convex-wall coverage only; camera pivot must remain outside solids. Close-up car occlusion is possible. No recovery until P05; no mode switching or cinematic effects.
Follow-up tasks: P05; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P04. Add a third-person chase camera with bounded smoothing and a sensible view of the road ahead. Handle basic camera obstruction in the primitive test scene. Keep camera logic separate from driving input. Do not add multiple camera modes or cinematic effects. Provide a short manual test for turning and driving near a wall.
```

