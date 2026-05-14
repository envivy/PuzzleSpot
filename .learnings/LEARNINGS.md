# Learnings

Corrections, insights, and knowledge gaps captured during development.

**Categories**: correction | insight | knowledge_gap | best_practice

---


## [LRN-20260513-001] best_practice

**Logged**: 2026-05-13T15:28:00+08:00
**Priority**: medium
**Status**: pending
**Area**: config

### Summary
Install com.code-philosophy.luban with a fixed git tag instead of tracking the repository default branch.

### Details
Using the repository URL without a version suffix can fail or become unstable when Unity Package Manager resolves a moving branch. Pinning https://github.com/focus-creative-games/luban_unity.git#1.2.0 is safer for this Unity 2022.3 project and makes package resolution reproducible.

### Suggested Action
Keep Luban pinned to a tested tag in Packages/manifest.json, and only upgrade deliberately after verification.

### Metadata
- Source: conversation
- Related Files: Packages/manifest.json, DataTables/README.md
- Tags: unity, upm, luban, package-management

---

## [LRN-20260513-002] best_practice

**Logged**: 2026-05-13T16:18:00+08:00
**Priority**: medium
**Status**: pending
**Area**: config

### Summary
For current Luban XML schemas, collection separators should be expressed in the table type header, not as a `sep` attribute on `<var>`.

### Details
`<var name="tip_infos" type="list,string" sep="|" />` fails with `unknown attribute attr:sep`. The working pattern is to keep XML as `type="list,string"` and encode the separator in CSV `##type`, for example `"(list#sep=|),string"`.

### Suggested Action
Keep XML schema fields minimal and move separator-specific parsing hints into the table header types.

### Metadata
- Source: conversation
- Related Files: DataTables/Defines/Level.xml, DataTables/Datas/level.csv
- Tags: luban, schema, csv, unity

---
## [LRN-20260513-003] best_practice

**Logged**: 2026-05-13T17:26:00+08:00
**Priority**: high
**Status**: pending
**Area**: config

### Summary
For this PuzzleSpot Luban workflow, prefer `.xlsx` authoring and fixed-slot fields over CSV/list-heavy schemas when defining gameplay rules.

### Details
The current Luban toolchain in this project parses `.xlsx` reliably, while CSV meta rows and list-valued cells were error-prone across separators and formats. Replacing list-heavy schema fields with small fixed-slot columns made generation stable and kept runtime logic straightforward.

### Suggested Action
Author gameplay rule tables in `.xlsx`, and model multi-reference relations with explicit numbered fields unless there is a confirmed need for a more generic relation table.

### Metadata
- Source: conversation
- Related Files: DataTables/Defines/Rule.xml, DataTables/Datas/level-rule-*.xlsx
- Tags: luban, xlsx, schema, gameplay-rules

---
## [LRN-20260513-004] best_practice

**Logged**: 2026-05-13T18:03:00+08:00
**Priority**: medium
**Status**: pending
**Area**: config

### Summary
Keep Luban Excel column names exactly aligned with schema names; avoid ad hoc tags like `##tag` in this project.

### Details
`level.xlsx` failed when its header was changed from `target_score` to `targetScore` and a custom `##tag` row was introduced. Current project tables should keep the known-good `##var / ##type / ##group / ##` layout and match XML field names exactly.

### Suggested Action
When editing rule or level tables, do not rename columns to camelCase and do not add unsupported meta tags unless the schema and parser expectations are updated together.

### Metadata
- Source: conversation
- Related Files: DataTables/Defines/Level.xml, DataTables/Datas/level.xlsx
- Tags: luban, excel, schema-alignment

---

## [LRN-20260514-001] best_practice

**Logged**: 2026-05-14T00:10:00+08:00
**Priority**: high
**Status**: pending
**Area**: config

### Summary
Rule-driven scene ids should use fixed per-level prefixes to prevent target and actor id collisions.

### Details
For this PuzzleSpot rule system, new scene ids should follow `Lxx_Txx` for `targetId` and `Lxx_Axx` for `actorId`. Example: the first target object in level 1 is `L01_T01`. This keeps ids short, unique across levels, and easy to wire in tables and prefabs.

### Suggested Action
Apply `Lxx_Txx` and `Lxx_Axx` naming to all new rule levels, and document the rule in the local `rule` skill so future changes stay consistent.

### Metadata
- Source: conversation
- Related Files: skills/rule/SKILL.md, Assets/Scripts/Game/GamePlay/Rule/RuleOperation.cs, Assets/Scripts/Game/GamePlay/Rule/RuleTarget.cs
- Tags: unity, luban, rule-system, naming

---
