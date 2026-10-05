# P02a: Script the current Windows export and transfer

Status: done
Type: implementation
Dependencies: P01; P02 export configuration (verified, visual acceptance remains open)
Goal: One WSL command creates the current Windows artifact and copies its complete runtime to W:\projektit\driving-time.
Inputs: D012/D014, pinned SDK, existing Windows Desktop preset and current working game.
Scope: C# build/import/export script, fresh artifact directory, error and completeness checks, runtime transfer and checksum verification.
Out of scope: P25 cached-source pipeline orchestration, game changes, native driving acceptance, publishing.
Deliverables: scripts/export-windows.cs; run instructions and execution evidence.
Acceptance:
- [x] Script help compiles with SDK 10.0.112 and invalid arguments fail clearly.
- [x] A fresh Windows release export completes and all runtime files reach the requested destination with matching SHA-256 hashes.
- [x] Logs remain in WSL; only exported runtime files are copied to Windows.
Evidence: 2026-10-03: script help exit 0; invalid argument exit 1. Full default execution exit 0: build 0 warnings/errors, import and Windows release publish/pack passed, 188 runtime files (168.1 MiB) copied to W:\projektit\driving-time with matching SHA-256 hashes. Fresh artifact: artifacts/windows/20261003-155717-ba7ee883. Native launch/visual/drive unverified and not required to close this script task. See [TESTING.md](../TESTING.md#windows-export-and-transfer-script) for commands and official sources checked 2026-10-03.
Risks / blockers: W: must be mounted and writable; close the game before overwriting runtime files. Copy is not atomic and does not delete unrelated or stale destination files. Native appearance/drive acceptance remains P02/P03 work.
Follow-up tasks: P04 after P03 manual acceptance; P25 remains todo.
