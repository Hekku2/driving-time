# P04: Add a usable chase camera

Status: todo
Type: implementation
Dependencies: P03
Goal: A chase camera that follows turns and avoids basic obstruction.
Inputs: P03 car and primitive scene.
Scope: Bounded smoothing, road-ahead view, basic obstruction handling and camera logic separate from input.
Out of scope: Multiple camera modes and cinematic effects.
Deliverables: Camera implementation and turning/wall manual guide.
Acceptance:
- [ ] Automated / research: Run the verified C# build and record results; do not equate build success with camera acceptance.
- [ ] Manual / review: Drive a tight loop and past a wall: smooth follow and no view from inside obstacles.
- [ ] Plan completion: Drive a tight loop and past a wall; the camera follows smoothly and does not leave the player viewing from inside an obstacle.
Evidence: Automated / research: not run. Manual / review: not run. Build/export: not run. Commands above are planned, not completed.
Risks / blockers: Obstruction behavior and tuning need visual evidence.
Follow-up tasks: P05; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P04. Add a third-person chase camera with bounded smoothing and a sensible view of the road ahead. Handle basic camera obstruction in the primitive test scene. Keep camera logic separate from driving input. Do not add multiple camera modes or cinematic effects. Provide a short manual test for turning and driving near a wall.
```

