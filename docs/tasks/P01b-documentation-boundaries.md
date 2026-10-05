# P01b: Separate permanent documentation from task history

Status: done
Type: documentation
Dependencies: [P01a](P01a-environment-preflight.md)
Goal: Permanent project documentation is understandable without historical task IDs.
Inputs: User instruction, AGENTS.md, permanent docs and existing task/evidence records.
Scope: Feature-based headings/prose/links, current-state summary, incoming anchor updates and a repository documentation rule.
Out of scope: Runtime changes, path/schema/asset renames, deletion of historical cards, acceptance of manual checks or implementation of later tasks.
Deliverables: Updated AGENTS.md, README/permanent docs, consistent links and task ledger.
Acceptance:
- [x] Permanent prose/headings do not reference historical task IDs; literal commands/paths/log evidence remain unchanged.
- [x] Local Markdown document links and changed heading anchors resolve.
- [x] Current state/open limitations are preserved; tasks remain in planning records for changes and acceptance work.
Evidence: 2026-10-05, C# Markdown checker verified task-free permanent prose (allowing the next proposed change in state), local document targets and heading anchors; final check covered 28 documents/214 local links with zero errors. README/state/setup now describe actual capabilities and fresh versus historical setup evidence; other permanent docs use descriptive headings and feature references. Command/source/license/numeric evidence remains dated; existing generated paths and startup markers retained. `git diff --check` passes. No code/build/drive changes or gate acceptance.
Risks / blockers: Literal filenames and log markers contain task IDs and must remain reproducible.
Follow-up tasks: P13 after environment setup and road-strip visual acceptance.
