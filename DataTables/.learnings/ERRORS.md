# Errors

Command failures and integration errors.

---

## [ERR-20260514-007] powershell_xmlelement_descendants

**Logged**: 2026-05-14T14:18:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
PowerShell XML DOM nodes do not expose a LINQ-style `Descendants()` method.

### Error
```text
Method invocation failed because [System.Xml.XmlElement] does not contain a method named 'Descendants'.
```

### Context
- Operation attempted: quick `.xlsx` shared string inspection via `System.IO.Compression`.

### Suggested Fix
Use `GetElementsByTagName('t')` or Excel COM for quick workbook inspection in PowerShell.

### Metadata
- Reproducible: yes
- Related Files: Datas/levelStep.xlsx

---

## [ERR-20260514-006] missing_level_hint_xlsx_during_type_update

**Logged**: 2026-05-14T14:13:00+08:00
**Priority**: medium
**Status**: pending
**Area**: config

### Summary
Updating Excel type rows for string hint IDs failed because `Datas/levelHint.xlsx` was not present at command time.

### Error
```text
Resolve-Path : Cannot find path 'E:\TeamGames\PuzzleSpot\DataTables\Datas\levelHint.xlsx' because it does not exist.
```

### Context
- Operation attempted: update `levelStep.xlsx` `hint_id` and `levelHint.xlsx` `id` type cells to `string`.
- The schema still references `levelHint.xlsx`, so either the file needs to be restored or the table definition removed/renamed.

### Suggested Fix
Check `Datas` for the current hint table filename before batch editing, and skip missing workbooks.

### Metadata
- Reproducible: unknown
- Related Files: Datas/levelHint.xlsx, Defines/Rule.xml

---

## [ERR-20260514-005] powershell_nested_command_variable_expansion

**Logged**: 2026-05-14T12:22:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
Nested `powershell -Command` syntax check failed because the outer PowerShell expanded `$errors` before the inner command ran.

### Error
```text
Missing condition in if statement after 'if ('.
```

### Context
- Command attempted: inline PSParser syntax check with double-quoted inner command.

### Suggested Fix
Use single quotes around the inner `-Command` payload or escape `$` variables.

### Metadata
- Reproducible: yes
- Related Files: prepare_luban_tables.ps1

---

## [ERR-20260514-004] powershell_select_object_range

**Logged**: 2026-05-14T12:20:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
Using `Select-Object -Index 140..250` failed because PowerShell treats the range as a string in that parameter position.

### Error
```text
Cannot bind parameter 'Index'. Cannot convert value "140..250" to type "System.Int32".
```

### Context
- Command attempted while inspecting `prepare_luban_tables.ps1`.

### Suggested Fix
Wrap ranges in parentheses, e.g. `Select-Object -Index (140..250)`.

### Metadata
- Reproducible: yes
- Related Files: prepare_luban_tables.ps1

---

## [ERR-20260514-003] xlsx_read_locked

**Logged**: 2026-05-14T12:10:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
Attempting to inspect `levelOutcome.xlsx` directly as a zip package failed because the file was in use by another process.

### Error
```text
The process cannot access the file 'E:\TeamGames\PuzzleSpot\DataTables\Datas\levelOutcome.xlsx' because it is being used by another process.
```

### Context
- Operation attempted: quick read of Excel table rows for examples.
- The schema files were still enough to answer the configuration relationship.

### Suggested Fix
Close the workbook in Excel before direct file inspection, or inspect a copied temp file.

### Metadata
- Reproducible: unknown
- Related Files: Datas/levelOutcome.xlsx

---

## [ERR-20260514-002] sandbox_write_assets_denied

**Logged**: 2026-05-14T12:05:00+08:00
**Priority**: medium
**Status**: pending
**Area**: config

### Summary
Attempting to patch generated Luban C# files under `..\Assets` from the `DataTables` workspace failed because the sandbox writable root is limited to `DataTables`.

### Error
```text
Set-Content : Access to the path 'E:\TeamGames\PuzzleSpot\Assets\Scripts\Gen\Luban\Tables.cs' is denied.
```

### Context
- Needed to replace generated `using SimpleJSON;` with `using Luban.SimpleJSON;`.
- Files under `Assets` are outside the configured writable root for this session.

### Suggested Fix
Request escalated permission when editing generated Unity assets outside the `DataTables` writable root.

### Metadata
- Reproducible: yes
- Related Files: ../Assets/Scripts/Gen/Luban

---

## [ERR-20260514-001] prepare_luban_tables_relative_confroot

**Logged**: 2026-05-14T11:54:00+08:00
**Priority**: medium
**Status**: pending
**Area**: config

### Summary
Running `prepare_luban_tables.ps1` with a relative `-ConfRoot .` caused Excel COM to fail opening the temporary backup workbook.

### Error
```text
无法找到“.\.tmp\level.xlsx”。请检查文件名的拼写，并检查文件位置是否正确。
```

### Context
- Command attempted: `powershell -NoProfile -ExecutionPolicy Bypass -File .\prepare_luban_tables.ps1 -Workspace .. -ConfRoot .`
- The script uses `Join-Path` outputs directly with Excel COM; relative paths can be interpreted unexpectedly by Excel.

### Suggested Fix
Resolve `Workspace`, `ConfRoot`, and derived workbook paths to absolute provider paths before passing them to Excel COM.

### Metadata
- Reproducible: yes
- Related Files: prepare_luban_tables.ps1

---
