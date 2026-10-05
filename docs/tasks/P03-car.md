# P03: Move a simple car on flat ground

Status: awaiting-manual-test
Type: implementation
Dependencies: P02
Goal: A controllable placeholder car on a primitive flat test loop.
Inputs: P02 game scene; P01 controller choice.
Scope: Physics-step keyboard acceleration/brake/reverse/steering, tuning parameters, speed km/h and controls.
Out of scope: Realistic tires/suspension, traffic and detailed art.
Deliverables: Car/controller and obstacle-free primitive loop with controls guide.
Acceptance:
- [x] Automated / research: Run the verified C# build; record frame-rate behavior checks where possible and unavailable comparisons explicitly.
- [X] Manual / review: Drive, steer, stop and reverse for two minutes; compare rendering frame rates where possible.
- [X] Plan completion: Drive, steer, stop and reverse for two minutes. Movement remains controllable and does not depend materially on rendering frame rate.
Evidence: 2026-10-03: Debug and ExportRelease builds passed with 0 warnings/errors; headless import passed outside sandbox. C# integration scene passed 720 real physics ticks at headless 30/60/144 rendering caps (358/713/1709 frame callbacks), with identical sampled position, signed speed and yaw to four decimal places. Rendered WSLg checks at 30/60 caps passed (281/308 frame callbacks); llvmpipe cannot establish sustained target FPS. Windows export published and packed successfully to artifacts/windows-p03; no P03 transfer or native Windows run performed. Full commands/results and official sources: [TESTING.md](../TESTING.md#flat-driving-loop). Manual two-minute drive/appearance/feel: unverified; [RUN.md](../RUN.md) has steps. User explicitly selected P03 while P02 visual acceptance remained pending; P02 and G0 stay open.
Risks / blockers: Driving feel and appearance await manual testing. Fixed overview only; leaving the 60 m ground has no recovery until P05. Basic floor contact only; impacts/slopes untested. Six tuning values are provisional. No realistic tires/suspension, traffic, detailed art or datasets.
Transfer follow-up (P02a, 2026-10-03): current P03 scene re-exported to artifacts/windows/20261003-155717-ba7ee883/runtime and all 188 runtime files transferred to W:\projektit\driving-time with matching SHA-256 hashes. Earlier no-transfer evidence above describes the initial P03 session. Native launch and driving acceptance remain unverified; status unchanged.
Follow-up tasks: P04; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P03. Add a visible placeholder car with keyboard acceleration, braking, reverse and steering on the existing flat ground. Keep the implementation deliberately simple and physics-step based; expose a few tuning parameters. Build a small obstacle-free test loop using primitives. Show speed in km/h and document controls. Do not implement realistic tires, suspension, traffic or detailed art. Verify behavior at different rendering frame rates where possible.
```
