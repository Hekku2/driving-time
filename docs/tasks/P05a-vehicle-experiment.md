# P05a: Compare VehicleBody3D driving

Status: awaiting-manual-test
Type: implementation / validation
Dependencies: P03, P04, P05 implemented baseline (manual acceptance remains open)
Goal: A playable VehicleBody3D alternative in the same synthetic loop for comparing reported jitter and driving feel.
Inputs: Current arcade scene, chase camera and recovery; user feedback 2026-10-03: movement feels a little jittery, requests VehicleBody3D test.
Scope: Four raycast wheels, force/brake/steering controls, speed HUD, R/fall recovery, targeted physical checks and Windows export/transfer.
Out of scope: Diagnosing the original jitter, realistic tires/gears, terrain/data pipeline, permanent controller selection.
Deliverables: Alternative C# controller and scene; comparison run guide; automated evidence; exported Windows runtime.
Acceptance:
- [x] Build/import and physical checks demonstrate wheel support, forward/brake/reverse/steering, collision, ramp and reset/fall recovery.
- [x] Existing arcade checks still pass; baseline remains runnable.
- [x] Windows export/transfer succeeds with runtime hashes checked.
- [x] User reports preferred controller: VehicleBody3D chosen on 2026-10-03 (D016).
- [ ] User reports loop/ramp/turning smoothness observations; no jitter fix inferred from automated checks or controller selection.
Evidence: 2026-10-03: SDK 10.0.112/Godot 4.5.2 .NET in WSL; Debug and ExportRelease builds passed, 0 warnings/errors. Escalated headless import passed after sandbox socket/settings/log failures. VehicleChecks passed 1150 physical ticks/five recoveries at 60 and 30 rendering caps with identical sampled traces. Recorded forward speed 9.986 m/s; after braking 0.054 m/s; reverse -3.801 m/s; right yaw -0.915 rad; ramp Y=1.285 m. Checks use real wheels, input actions, chassis contact and integration; clear pose/linear/angular/control state on resets and verify camera heading. Preserved arcade DrivingChecks/CameraChecks/RecoveryChecks passed at 60 caps (720/420/1411 ticks). Export/transfer helper passed: artifacts/windows/20261003-184950-a7195a8f, 188 runtime files (168.2 MiB) SHA-256 verified at W:\projektit\driving-time. Both scenes packed. Test callback indentation only changed after the successful tests; gameplay matches the exported artifact. Native startup and manual comparison remain unverified. Sources/commands: [TESTING.md](../TESTING.md#p05a--vehiclebody3d-comparison); comparison: [RUN.md](../RUN.md#p05a-manual-comparison).
Manual / decision evidence: 2026-10-03 user instruction: “Record VehicleBody3d as chosen”. Recorded as D016; the existing vehicle scene remains the default. No tested artifact/platform or detailed driving observations were supplied, so manual smoothness acceptance and status remain open. This documentation-only follow-up changes no gameplay and reruns no runtime checks.
Risks / blockers: Built-in vehicle physics has documented limitations. Original jitter may involve camera/interpolation/rendering rather than body type. VehicleBody3D is chosen; tuning is provisional and visual evidence remains required.
Follow-up tasks: P06; further controller work only from comparison feedback.
