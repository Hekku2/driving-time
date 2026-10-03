# Offline synthetic pipeline (P06)

Keep the checkout, .NET SDK, NuGet cache, pipeline and generated manifests in WSL2. Use the root `global.json` SDK 10.0.112. Pipeline and tests target net10.0; the independent Godot game retains net8.0. No shared library is required. The workflow requires no Python runtime, Node.js, container or external GIS CLI. Only the exported game EXE, PCK and complete companion runtime directory go to Windows; see [RUN.md](RUN.md) for the existing export helper.

## Restore and run

From the repository root in WSL, initially with network access:

```bash
dotnet --version
dotnet restore tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --locked-mode --source https://api.nuget.org/v3/index.json
dotnet build pipeline/DrivingTime.Pipeline.csproj --no-restore
dotnet test tests/DrivingTime.Pipeline.Tests/DrivingTime.Pipeline.Tests.csproj --no-restore
```

The test restore also restores its pipeline reference. The CLI has no PackageReferences; JSON, validation, SHA-256 and file operations use built-in .NET facilities. Tests need only Microsoft.NET.Test.Sdk 17.14.1, xunit 2.9.3 and xunit.runner.visualstudio 3.1.4. Exact version ranges are in the test project; checked-in `packages.lock.json` files pin transitive versions/content hashes. Normal restores use `--locked-mode`; after a deliberate package change, regenerate with `dotnet restore ... --force-evaluate` and review the lock-file diff before returning to locked mode. Initial restore performs NuGet auditing; network failures must be resolved, not silently disabled. This policy applies to the new pipeline/tests; the existing game SDK/restore policy is unchanged.

After restore, the documented command is:

```bash
dotnet run --project pipeline/DrivingTime.Pipeline.csproj --no-restore -- --config config/synthetic-scene.json
```

Expect exit 0 and `Wrote synthetic manifest v1:` with the output path. This command builds locally without restore. It reads one local configuration and writes one local file; there are no network clients or download steps. To run the compiled result directly, add `--no-build` after the initial build. Building the whole solution also needs the game's already documented initial restore.

## Configuration and manifest v1

[synthetic-scene.json](../config/synthetic-scene.json) is a repository-authored 60 × 60 m fixture, not a selected Helsinki region or geographic origin. All fields are required and case sensitive; unknown fields, duplicate fields, missing/null values and malformed JSON fail. Comments and trailing commas are unsupported.

| Field | Rule |
|---|---|
| `schemaVersion` | Exactly 1 |
| `sceneId` | 1–64 ASCII letters, digits, hyphens or underscores |
| `coordinateSystem` | Exactly `synthetic-local-metres`; no EPSG support yet |
| `region` | Finite `minEast`, `minNorth`, `maxEast`, `maxNorth`; finite strictly positive extents |
| `origin` | Finite `east`, `north`, `height`; east/north inside closed region bounds |
| `tileSizeMetres` | Finite positive number; metadata only, no tiles generated |
| `manifestPath` | Relative `.json` path, resolved from the configuration directory, using `/`; no `.`/`..`, empty segments, control characters, backslashes or colon; cannot overwrite configuration |

The output contains schema version, scene ID, region, origin, tile size, provenance and two primitive descriptions: a ground plane spanning the region at origin height, and a 2 m reference box centred at origin east/north, with its bottom at origin height. Source coordinates and sizes use east/north/height metres; no engine mapping is implied. The reference box is a fixed fixture and may extend outside very small regions or an edge origin. There are no GLBs, collision meshes, engine-readable staging or tile enumeration. P07/P08 own those later contracts.

Provenance states repository-authored synthetic content, no retrieval date, no geographic CRS/vertical datum and the SHA-256 of the exact input bytes. No third-party dataset license applies; this statement grants no new rights to repository code. Configuration whitespace changes its checksum. No source dataset is downloaded or redistributed.

Objects are ordered by ordinal ID; property order follows the fixed record declarations. JSON uses invariant numeric formatting, UTF-8 without BOM, and a final LF. There are no timestamps, absolute paths or random IDs in the manifest and no excluded diagnostic metadata. Same input bytes with the pinned SDK produce identical manifest bytes. The absolute output path appears only in console diagnostics.

Errors go to stderr with exit 2: check the named field, configuration path or write permissions. Validation finishes before output creation; invalid input leaves any existing output unchanged. Valid runs overwrite the configured manifest. File writes are not atomic; rerun after an interrupted write. Keep generated output outside ordinary Git; the sample's `config/generated/` is ignored.

## Short review

Run the documented command twice without restore/network access, copying the first manifest aside:

```bash
cp config/generated/synthetic-scene.manifest.json /tmp/p06-first.manifest.json
dotnet run --project pipeline/DrivingTime.Pipeline.csproj --no-restore -- --config config/synthetic-scene.json
cmp /tmp/p06-first.manifest.json config/generated/synthetic-scene.manifest.json
```

Expect `cmp` exit 0. In a temporary copy of the config, change `tileSizeMetres` to 0; expect exit 2 naming `tileSizeMetres` and no output created. Full automated evidence is in [TESTING.md](TESTING.md#p06--offline-synthetic-manifest-cli). No visual game test is needed for this manifest-only task.

Official sources checked 2026-10-03: [NuGet lock files and locked restores](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies), [System.Text.Json unmapped-member rejection](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/missing-members), and package metadata for [xunit 2.9.3](https://www.nuget.org/packages/xunit/2.9.3), [VS runner 3.1.4](https://www.nuget.org/packages/xunit.runner.visualstudio/3.1.4), [test SDK 17.14.1](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/17.14.1). Local restore/build/test establishes compatibility with net10.0 and SDK 10.0.112; versions were selected for this small harness, not as a claim they are latest.

P07 adds the tested [coordinate contract](COORDINATES.md) and bounded ProjNET 2.1.0 horizontal projection API. The v1 synthetic CLI/config/output above retain their existing behavior; P08 will consume the mapping when generating assets. No real geographic origin or height data is selected. Restore uses updated lock files; tests include independent NLS reference evidence and all P06 regressions.
