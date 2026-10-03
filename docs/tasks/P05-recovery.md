# P05: Add collision and safe recovery

Status: todo
Type: implementation
Dependencies: P04
Goal: A vehicle that collides and recovers safely in the test scene.
Inputs: P04 car/camera, defined spawn and primitive scene.
Scope: Static obstacles and slope; verify chosen collision model; reset/fall recovery with configurable world limit; clear motion and camera state.
Out of scope: Whole-controller redesign; split any verified independent defect.
Deliverables: Obstacles/slope, recovery action and G1 evidence.
Acceptance:
- [ ] Automated / research: Run the verified C# build and any targeted recovery checks justified by the implementation; record actual results.
- [ ] Manual / review: Hit obstacle, drive slope, fall out of bounds and reset: safe spawn without retained motion; record G1.
- [ ] Plan completion: Hit an obstacle, drive the slope, fall out of bounds and reset. The vehicle returns safely without retained motion. Record G1 results.
Evidence: Automated / research: not run. Manual / review: not run. Build/export: not run. Commands above are planned, not completed.
Risks / blockers: Chosen controller may fail modest collision/slope checks; create focused child if needed.
Follow-up tasks: P13; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P05. Add several simple static obstacles and a slope to the test scene. Verify the car's chosen collision model. Add a reset action to a defined spawn and automatic recovery below a configurable world limit. Clear velocity and camera state on reset. If the current movement approach cannot satisfy these modest tests, describe the narrow defect and propose a focused child task. Do not redesign the whole controller.
```

