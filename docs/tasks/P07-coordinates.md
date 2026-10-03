# P07: Specify and verify coordinate transforms

Status: todo
Type: implementation
Dependencies: P06
Goal: A tested explicit coordinate contract in projected meters and local engine space.
Inputs: P06 CLI/config/fixtures; trusted coordinate reference; selected CRS evidence.
Scope: Axis order/units/handedness/datum/origin/tile coordinates; double-precision source coordinates; cardinal/100 m/elevation/round-trip tests; evaluate .NET transforms and EPSG:4326 handling.
Out of scope: Invented origin, assumed library support and unverified source-axis mapping.
Deliverables: Coordinate contract, implementation, synthetic fixtures and trusted-reference evaluation.
Acceptance:
- [ ] Automated / research: Planned: dotnet test on actual test project; prove cardinal direction, 100 m scale, nonzero elevation and round-trip tolerance; compare chosen transform library against trusted reference with required CRS/axis support.
- [ ] Manual / review: Review contract: source axis semantics and vertical-data validation are explicit; no runtime visual acceptance required yet.
- [ ] Plan completion: Automated fixtures prove direction, 100 m scale, elevation and round-trip tolerance. The coordinate contract includes source CRS axis-order handling.
Evidence: Automated / research: not run. Manual / review: not run. Build/export: not run. Commands above are planned, not completed.
Risks / blockers: CRS/library support, axis semantics and vertical datum need verification.
Follow-up tasks: P08, P09, P11; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P07. Define an explicit coordinate contract: source axis order and units, projected east/north coordinates, vertical datum, fixed origin, engine axes, handedness and tile-local coordinates. Use projected meters and local engine coordinates; keep source coordinates in double precision. A possible mapping is X=east-originEast, Y=height-originHeight, Z=-(north-originNorth), but verify it rather than assuming source X means east. Implement and test synthetic cardinal points, a measured 100 m segment, round trips and a nonzero elevation. Record how EPSG:4326 to the chosen projected CRS will be handled in C# and how vertical data will be validated. Evaluate a .NET coordinate-transform library against a trusted reference and verify its required CRS support and axis semantics; do not assume a package name proves compatibility. Do not invent origin coordinates.
```

