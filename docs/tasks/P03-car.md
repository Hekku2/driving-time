# P03: Move a simple car on flat ground

Status: todo
Type: implementation
Dependencies: P02
Goal: A controllable placeholder car on a primitive flat test loop.
Inputs: P02 game scene; P01 controller choice.
Scope: Physics-step keyboard acceleration/brake/reverse/steering, tuning parameters, speed km/h and controls.
Out of scope: Realistic tires/suspension, traffic and detailed art.
Deliverables: Car/controller and obstacle-free primitive loop with controls guide.
Acceptance:
- [ ] Automated / research: Run the verified C# build; record frame-rate behavior checks where possible and unavailable comparisons explicitly.
- [ ] Manual / review: Drive, steer, stop and reverse for two minutes; compare rendering frame rates where possible.
- [ ] Plan completion: Drive, steer, stop and reverse for two minutes. Movement remains controllable and does not depend materially on rendering frame rate.
Evidence: Automated / research: not run. Manual / review: not run. Build/export: not run. Commands above are planned, not completed.
Risks / blockers: Controller/frame-rate behavior is unverified.
Follow-up tasks: P04; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P03. Add a visible placeholder car with keyboard acceleration, braking, reverse and steering on the existing flat ground. Keep the implementation deliberately simple and physics-step based; expose a few tuning parameters. Build a small obstacle-free test loop using primitives. Show speed in km/h and document controls. Do not implement realistic tires, suspension, traffic or detailed art. Verify behavior at different rendering frame rates where possible.
```

