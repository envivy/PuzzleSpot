# Errors

Command failures and integration errors.

---


## [ERR-20260513-001] parallel_learnings_read

**Logged**: 2026-05-13T15:09:00+08:00
**Priority**: low
**Status**: resolved
**Area**: config

### Summary
A parallel read of .learnings ran before the initialization command completed.

### Error
```Cannot find path 'E:\TeamGames\PuzzleSpot\.learnings' because it does not exist.
```
### Context
- Command/operation attempted: listed .learnings concurrently with initialization.
- Impact: none; initialization completed in the parallel batch.

### Suggested Fix
Initialize .learnings before parallel reads that depend on it.

### Metadata
- Reproducible: yes
- Related Files: .learnings/ERRORS.md

### Resolution
- **Resolved**: 2026-05-13T15:09:00+08:00
- **Notes**: .learnings was initialized successfully immediately after the race.

---


## [ERR-20260513-002] git_clone_luban_examples

**Logged**: 2026-05-13T15:12:00+08:00
**Priority**: low
**Status**: pending
**Area**: infra

### Summary
Cloning luban_examples into .utmp failed because git could not write .git/config.

### Error
```warning: unable to unlink 'E:/TeamGames/PuzzleSpot/.utmp/luban_examples/.git/config.lock': Invalid argument
error: could not write config file E:/TeamGames/PuzzleSpot/.utmp/luban_examples/.git/config: Permission denied
fatal: could not set 'core.repositoryformatversion' to '0'
```
### Context
- Command attempted: git clone --depth 1 https://github.com/focus-creative-games/luban_examples.git .utmp/luban_examples
- Workaround: use official docs/raw files or clone elsewhere under the workspace if needed.

### Suggested Fix
Avoid using .utmp for git clones when it contains stale locked folders; use a normal temporary folder under the workspace or clean stale clone after confirming it is safe.

### Metadata
- Reproducible: unknown
- Related Files: .utmp/luban_examples

---


## [ERR-20260513-003] git_clone_workspace_tmp

**Logged**: 2026-05-13T15:14:00+08:00
**Priority**: low
**Status**: pending
**Area**: infra

### Summary
Cloning luban_examples into a normal workspace folder also failed when writing .git/config.

### Error
```warning: unable to unlink 'E:/TeamGames/PuzzleSpot/LubanExamplesTmp/.git/config.lock': Invalid argument
error: could not write config file E:/TeamGames/PuzzleSpot/LubanExamplesTmp/.git/config: Permission denied
fatal: could not set 'core.repositoryformatversion' to '0'
```
### Context
- Command attempted: git clone --depth 1 https://github.com/focus-creative-games/luban_examples.git LubanExamplesTmp
- Workaround: proceed using official docs and direct file templates instead of clone.

### Suggested Fix
Investigate workspace filesystem permissions or Git locking behavior before using git clone for temporary dependencies.

### Metadata
- Reproducible: yes
- Related Files: LubanExamplesTmp

---


## [ERR-20260513-004] powershell_fetch_raw_luban_template

**Logged**: 2026-05-13T15:16:00+08:00
**Priority**: low
**Status**: pending
**Area**: infra

### Summary
PowerShell failed to fetch a raw luban_examples template from GitHub.

### Error
```Invoke-WebRequest : The underlying connection was closed: An error occurred while receiving.
```
### Context
- Command attempted: Invoke-WebRequest raw.githubusercontent.com/focus-creative-games/luban_examples/main/MiniTemplate/luban.conf
- Workaround: use official docs and local conservative templates.

### Suggested Fix
Use browser/web tool, retry from a network with stable GitHub raw access, or vendor the desired template manually.

### Metadata
- Reproducible: unknown
- Related Files: DataTables/luban.conf

---



## [ERR-20260513-005] github_zip_download_luban_unity

**Logged**: 2026-05-13T15:49:00+08:00
**Priority**: medium
**Status**: pending
**Area**: infra

### Summary
Downloading luban_unity tag zip from GitHub failed because the connection closed during receive.

### Error
`Invoke-WebRequest : The underlying connection was closed: An error occurred while receiving.
``n### Context
- Command attempted: Invoke-WebRequest https://github.com/focus-creative-games/luban_unity/archive/refs/tags/1.2.0.zip
- Impact: local package was not downloaded by zip.

### Suggested Fix
Retry with git-based transport, browser download, or a network that can access GitHub more reliably.

### Metadata
- Reproducible: unknown
- Related Files: LocalPackages/luban_unity-1.2.0.zip

---


## [ERR-20260513-006] git_clone_luban_unity_rpc_reset

**Logged**: 2026-05-13T15:51:00+08:00
**Priority**: medium
**Status**: pending
**Area**: infra

### Summary
Cloning luban_unity from GitHub failed because the connection reset during pack transfer.

### Error
`error: RPC failed; curl 28 Recv failure: Connection was reset
fatal: expected flush after ref listing
``n### Context
- Command attempted: git clone --depth 1 --branch 1.2.0 https://github.com/focus-creative-games/luban_unity.git LocalPackages/luban_unity-1.2.0
- Impact: local Unity package could not be downloaded into the workspace.

### Suggested Fix
Use a browser/manual download on a stable network, a GitHub mirror, or a local shared copy of the package.

### Metadata
- Reproducible: yes
- Related Files: LocalPackages/luban_unity-1.2.0

---

## [ERR-20260513-007] powershell_add_content_escape

**Logged**: 2026-05-13T16:19:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
Appending a learning entry with inline escaped backticks and pipes caused a PowerShell parser error.

### Error
```text
Missing argument in parameter list.
An empty pipe element is not allowed.
```
### Context
- Command attempted: Add-Content with a double-quoted string containing backticks and `|`.
- Impact: learning entry was not appended; project files were unaffected.

### Suggested Fix
Use a single-quoted here-string when appending markdown that contains backticks, quotes, or pipe characters.

### Metadata
- Reproducible: yes
- Related Files: .learnings/LEARNINGS.md

---
## [ERR-20260513-008] excel_tmp_remove_denied

**Logged**: 2026-05-13T17:15:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
Temporary Excel files in DataTables\Datas could not be removed because the paths were locked.

### Error
```text
Access to the path '...level~59A08.tmp' is denied.
```
### Context
- Command attempted: remove generated `*.tmp` files after Excel automation.
- Impact: no functional change yet; files may indicate Excel still held handles briefly.

### Suggested Fix
Close workbook cleanly and tolerate transient `~*.tmp` files during table generation, or retry cleanup later.

### Metadata
- Reproducible: unknown
- Related Files: DataTables/Datas/*.tmp

---
## [ERR-20260513-009] excel_save_level_xlsx_hresult_80070002

**Logged**: 2026-05-13T18:08:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
Saving edits back into level.xlsx through Excel COM failed with HRESULT 0x80070002.

### Error
```text
The system cannot find the file specified. (Exception from HRESULT: 0x80070002)
```
### Context
- Command attempted: open and overwrite `DataTables/Datas/level.xlsx`, then `Save()`.
- Impact: the requested rollback was not yet applied.

### Suggested Fix
Use a fresh workbook and SaveAs to the target path instead of in-place Save when Excel file state is unstable.

### Metadata
- Reproducible: unknown
- Related Files: DataTables/Datas/level.xlsx

---

## [ERR-20260513-010] powershell_non_ascii_separator_parse

**Logged**: 2026-05-13T18:40:00+08:00
**Priority**: low
**Status**: resolved
**Area**: config

### Summary
PowerShell preprocessing script failed to parse when a non-ASCII full-width comma literal was embedded in the source file.

### Error
```text
Missing ')' in method call.
Unexpected token ...
```

### Context
- Command/operation attempted: run `DataTables/gen_client.bat` after adding `prepare_luban_tables.ps1`
- Impact: Luban preprocessing failed before generation started

### Suggested Fix
Prefer ASCII-only literals in project scripts; when matching full-width punctuation, use numeric char codes such as `[char]0xFF0C`.

### Metadata
- Reproducible: yes
- Related Files: DataTables/prepare_luban_tables.ps1

### Resolution
- **Resolved**: 2026-05-13T18:41:00+08:00
- **Notes**: Replaced the full-width comma string literal with `[char]0xFF0C`.

---

## [ERR-20260514-001] remove_level_xlsx_bak_denied

**Logged**: 2026-05-14T00:30:00+08:00
**Priority**: low
**Status**: resolved
**Area**: config

### Summary
Deleting `DataTables\\Datas\\level.xlsx.bak` failed because the file was locked or denied by the filesystem.

### Error
```text
Access to the path 'E:\TeamGames\PuzzleSpot\DataTables\Datas\level.xlsx.bak' is denied.
```

### Context
- Command/operation attempted: `Remove-Item -LiteralPath 'DataTables\\Datas\\level.xlsx.bak' -Force`
- Impact: temporary backup file remained in the workspace

### Suggested Fix
Retry deletion after closing Excel or any process holding the file, or rerun with elevated permissions if needed.

### Metadata
- Reproducible: unknown
- Related Files: DataTables/Datas/level.xlsx.bak

### Resolution
- **Resolved**: 2026-05-14T11:37:00+08:00
- **Notes**: Added retry-based cleanup for `level.xlsx.bak` and Excel lock files in `DataTables/prepare_luban_tables.ps1`.

---

## [ERR-20260514-002] powershell_unquoted_dollar_path

**Logged**: 2026-05-14T11:35:00+08:00
**Priority**: low
**Status**: resolved
**Area**: config

### Summary
PowerShell expanded `$level` inside an unquoted path for an Office lock file, changing `~$level.xlsx.bak` into `~.xlsx.bak`.

### Error
```text
Cannot find path 'DataTables\Datas\~.xlsx.bak' because it does not exist.
```

### Context
- Command/operation attempted: `Get-Item -LiteralPath DataTables\Datas\~$level.xlsx.bak -Force`
- Impact: The diagnostic command inspected the wrong path until the literal path was quoted.

### Suggested Fix
Quote paths containing `$` with single quotes in PowerShell, especially Office lock files named like `~$file.xlsx`.

### Metadata
- Reproducible: yes
- Related Files: DataTables/Datas/~$level.xlsx.bak

### Resolution
- **Resolved**: 2026-05-14T11:35:00+08:00
- **Notes**: Re-ran the command with `-LiteralPath 'DataTables\Datas\~$level.xlsx.bak'`.

---

## [ERR-20260514-003] powershell_colon_after_variable_parse

**Logged**: 2026-05-14T11:43:00+08:00
**Priority**: low
**Status**: resolved
**Area**: config

### Summary
PowerShell failed to parse a double-quoted string containing `$Path:` because the colon was treated as part of the variable reference.

### Error
```text
Variable reference is not valid. ':' was not followed by a valid variable name character.
```

### Context
- Command/operation attempted: run `DataTables/prepare_luban_tables.ps1 -Mode restore` after adding cleanup warnings.
- Impact: Restore script did not start until the warning string was fixed.

### Suggested Fix
Use `${Path}:` or string formatting when appending punctuation directly after a PowerShell variable in double-quoted strings.

### Metadata
- Reproducible: yes
- Related Files: DataTables/prepare_luban_tables.ps1

### Resolution
- **Resolved**: 2026-05-14T11:43:00+08:00
- **Notes**: Changed `$Path:` to `${Path}:`.

---

## [ERR-20260514-004] get_cim_process_access_denied

**Logged**: 2026-05-14T11:55:00+08:00
**Priority**: low
**Status**: pending
**Area**: config

### Summary
Reading process command lines with `Get-CimInstance Win32_Process` failed due to access restrictions.

### Error
```text
Get-CimInstance : Access denied
```

### Context
- Command/operation attempted: inspect command lines for lingering `powershell.exe`, `dotnet.exe`, `wps.exe`, and `et.exe` processes.
- Impact: Process names and start times were available via `Get-Process`, but exact command lines were not.

### Suggested Fix
Use elevated permissions when command-line inspection is required, or rely on process names/start times when enough for diagnosis.

### Metadata
- Reproducible: unknown
- Related Files: DataTables/gen_client.bat, DataTables/prepare_luban_tables.ps1

---
