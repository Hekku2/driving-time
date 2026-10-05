# Pending tasks

Updated: 2026-10-05. Completed implementation cards/prompts have been removed. Existing features and outcomes are in [project state](../PROJECT_STATE.md), [current decisions](../DECISIONS.md) and [test evidence](../TESTING.md). No manual result is inferred from this cleanup.

## Outstanding validation

These features are implemented. Only the listed acceptance checks remain; do not repeat their implementation.

| ID | Pending check | Status | Instructions | Automated evidence |
|---|---|---|---|---|

## Future changes

Select one task at a time. Create a detailed card before implementation and expand broad tasks before selecting a child. References to existing capabilities are dependencies on their documented result, not archived tasks.

| ID | Task | Type | Status | Dependencies |
|---|---|---|---|---|
| <a id="p14"></a>P14 | [Acquire and inspect one building sample](../../HELSINKI_DRIVING_PLAN.md#p14--acquire-and-inspect-one-building-sample) | implementation | todo | verified source registry and access guidance |
| <a id="p15"></a>P15 | [Convert and display a few real buildings](../../HELSINKI_DRIVING_PLAN.md#p15--convert-and-display-a-few-real-buildings) | implementation | todo | [P14](#p14), [P13](#p13) |
| <a id="p16"></a>P16 | [Plan the terrain and elevation increment](../../HELSINKI_DRIVING_PLAN.md#p16--plan-the-terrain-and-elevation-increment) | planning | todo | [P15](#p15) |
| <a id="p17"></a>P17 | [Acquire one terrain patch](../../HELSINKI_DRIVING_PLAN.md#p17--acquire-one-terrain-patch) | implementation | todo | [P16](#p16) |
| <a id="p18"></a>P18 | [Generate terrain mesh and collision](../../HELSINKI_DRIVING_PLAN.md#p18--generate-terrain-mesh-and-collision) | implementation | todo | [P17](#p17), existing GLB staging workflow |
| <a id="p19"></a>P19 | [Fit the supported road segment to terrain](../../HELSINKI_DRIVING_PLAN.md#p19--fit-the-supported-road-segment-to-terrain) | implementation | todo | [P18](#p18), [P12](#p12) |
| <a id="p20"></a>P20 | [Plan one real intersection](../../HELSINKI_DRIVING_PLAN.md#p20--plan-one-real-intersection) | planning | todo | [P19](#p19), existing normalized road graph |
| <a id="p21"></a>P21 | [Implement and drive one intersection](../../HELSINKI_DRIVING_PLAN.md#p21--implement-and-drive-one-intersection) | implementation | todo | [P20](#p20) |
| <a id="p22"></a>P22 | [Define tiles and generate two neighbors](../../HELSINKI_DRIVING_PLAN.md#p22--define-tiles-and-generate-two-neighbors) | implementation | todo | [P21](#p21) |
| <a id="p23"></a>P23 | [Plan a bounded streaming lifecycle](../../HELSINKI_DRIVING_PLAN.md#p23--plan-a-bounded-streaming-lifecycle) | planning | todo | [P22](#p22) |
| <a id="p24"></a>P24 | [Implement and test tile crossing](../../HELSINKI_DRIVING_PLAN.md#p24--implement-and-test-tile-crossing) | implementation | todo | [P23](#p23) |
| <a id="p25"></a>P25 | [Make the prototype build reproducible](../../HELSINKI_DRIVING_PLAN.md#p25--make-the-prototype-build-reproducible) | implementation | todo | [P24](#p24) |
| <a id="p26"></a>P26 | [Measure the playable prototype](../../HELSINKI_DRIVING_PLAN.md#p26--measure-the-playable-prototype) | validation | todo | [P25](#p25) |
| <a id="p27"></a>P27 | [Package the first playable checkpoint](../../HELSINKI_DRIVING_PLAN.md#p27--package-the-first-playable-checkpoint) | validation | todo | [P26](#p26) |

Next recommended implementation: **P14**, after first-road manual feedback. G0–G2 visual checks remain open; G3 awaits manual road acceptance and buildings, and G4–G6 remain unpassed. [Gate definitions](../../HELSINKI_DRIVING_PLAN.md#gates).
