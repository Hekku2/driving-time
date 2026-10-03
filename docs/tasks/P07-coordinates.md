# P07: Specify and verify coordinate transforms

Status: done
Type: implementation
Dependencies: P06
Goal: A tested explicit coordinate contract in projected meters and local engine space.
Inputs: P06 CLI/config/fixtures; trusted coordinate reference; selected CRS evidence.
Scope: Axis order/units/handedness/datum/origin/tile coordinates; double-precision source coordinates; cardinal/100 m/elevation/round-trip tests; evaluate .NET transforms and EPSG:4326 handling.
Out of scope: Invented origin, assumed library support and unverified source-axis mapping.
Deliverables: Coordinate contract, implementation, synthetic fixtures and trusted-reference evaluation.
Acceptance:
- [x] Automated / research: 31 tests pass (16 P07 cases plus 15 P06 regressions): cardinal direction, synthetic measured 100 m segment, elevation, round trips, tile boundaries and independent NLS reference; ProjNET 2.1.0 WKT/GRS80/TM35FIN and actual library lon/lat semantics verified.
- [x] Manual / review: Agent reviewed [coordinate contract](../COORDINATES.md): explicit source axes, N2000 validation, fixed caller-supplied origin; no real origin invented. User review not run; no runtime visual acceptance required.
- [x] Plan completion: Fixtures and documented tolerances cover required behavior. Source CRS/format axis handling and EPSG:4326 approximation policy are explicit.
Evidence: 2026-10-03: restore and locked restore pass; root Debug build 0 warnings/errors; tests pass after sandbox-local test socket permission failure was retried with escalation. NLS example 10 residual E=-0.0000265805 m, N=-0.0000227625 m (tolerance 0.01 m). [Commands/results](../TESTING.md#p07--coordinate-contract-and-transform-evaluation). No editor/export/manual drive run. Existing unrelated staged work preserved.
Risks / blockers: Real origin/bounds/control points remain P09; real height validation not performed. ProjNET verifies horizontal projection only; exact epoch-aware WGS84 datum transforms and vertical/grid transforms unsupported. P06 CLI remains synthetic-only; P08 owns asset mapping/import and visual scale checks. G2 stays open.
Follow-up tasks: P08, P09, P11; no implementation implied.

## Execution prompt

Read AGENTS.md, the plan, project state and this card first. Execute only this task; update evidence and the ledger afterward.

```text
Execute P07. Define an explicit coordinate contract: source axis order and units, projected east/north coordinates, vertical datum, fixed origin, engine axes, handedness and tile-local coordinates. Use projected meters and local engine coordinates; keep source coordinates in double precision. A possible mapping is X=east-originEast, Y=height-originHeight, Z=-(north-originNorth), but verify it rather than assuming source X means east. Implement and test synthetic cardinal points, a measured 100 m segment, round trips and a nonzero elevation. Record how EPSG:4326 to the chosen projected CRS will be handled in C# and how vertical data will be validated. Evaluate a .NET coordinate-transform library against a trusted reference and verify its required CRS support and axis semantics; do not assume a package name proves compatibility. Do not invent origin coordinates.
```
