param(
    [string]$Workspace,
    [string]$ConfRoot,
    [ValidateSet("prepare", "restore")]
    [string]$Mode = "prepare"
)

$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([Parameter(Mandatory = $true)][string]$Path)

    $executionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
}

function Normalize-HeaderName {
    param([string]$Name)

    if ([string]::IsNullOrWhiteSpace($Name)) {
        return ""
    }

    return (($Name -replace "[^A-Za-z0-9]", "").ToLowerInvariant())
}

function Get-ColumnMap {
    param($Sheet, [int]$LastColumn)

    $map = @{}
    for ($column = 1; $column -le $LastColumn; $column++) {
        $header = [string]$Sheet.Cells.Item(1, $column).Text
        $normalized = Normalize-HeaderName $header
        if (-not [string]::IsNullOrEmpty($normalized) -and -not $map.ContainsKey($normalized)) {
            $map[$normalized] = $column
        }
    }
    return $map
}

function Get-CellText {
    param($Sheet, [int]$Row, [hashtable]$ColumnMap, [string[]]$Aliases)

    foreach ($alias in $Aliases) {
        $normalized = Normalize-HeaderName $alias
        if ($ColumnMap.ContainsKey($normalized)) {
            return [string]$Sheet.Cells.Item($Row, $ColumnMap[$normalized]).Text
        }
    }

    return ""
}

function Convert-ListValue {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return ""
    }

    $trimmed = $Value.Trim()

    if ($trimmed.Contains("|")) {
        $parts = $trimmed.Split("|")
    } elseif ($trimmed.Contains(",")) {
        $parts = $trimmed.Split(",")
    } elseif ($trimmed.Contains([char]0xFF0C)) {
        $parts = $trimmed.Split([char]0xFF0C)
    } elseif ($trimmed.Contains([Environment]::NewLine)) {
        $parts = $trimmed.Split([Environment]::NewLine)
    } else {
        $parts = @($trimmed)
    }

    $cleanParts = @()
    foreach ($part in $parts) {
        $item = $part.Trim()
        if (-not [string]::IsNullOrWhiteSpace($item)) {
            $cleanParts += $item
        }
    }

    return ($cleanParts -join "|")
}

function Release-ComObject {
    param($ComObject)

    if ($null -ne $ComObject) {
        [void][System.Runtime.InteropServices.Marshal]::ReleaseComObject($ComObject)
    }
}

function Remove-PathWithRetry {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [int]$Retries = 10,
        [int]$DelayMilliseconds = 300
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return
    }

    for ($attempt = 1; $attempt -le $Retries; $attempt++) {
        try {
            $item = Get-Item -LiteralPath $Path -Force -ErrorAction SilentlyContinue
            if ($item) {
                $item.Attributes = "Normal"
            }
            Remove-Item -LiteralPath $Path -Force
            return
        } catch {
            if ($attempt -eq $Retries) {
                throw
            }

            Start-Sleep -Milliseconds $DelayMilliseconds
            [GC]::Collect()
            [GC]::WaitForPendingFinalizers()
        }
    }
}

function Try-RemovePathWithRetry {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [int]$Retries = 10,
        [int]$DelayMilliseconds = 300
    )

    try {
        Remove-PathWithRetry -Path $Path -Retries $Retries -DelayMilliseconds $DelayMilliseconds
        return $true
    } catch {
        Write-Warning "[Luban] Could not remove ${Path}: $($_.Exception.Message)"
        return $false
    }
}

function Remove-ExcelLockFiles {
    param([string]$Directory)

    Get-ChildItem -LiteralPath $Directory -Force -Filter "~`$*.xlsx*" -ErrorAction SilentlyContinue |
        ForEach-Object {
            [void](Try-RemovePathWithRetry -Path $_.FullName -Retries 5 -DelayMilliseconds 200)
        }
}

function Copy-CellStyle {
    param(
        $SourceCell,
        $TargetCell
    )

    $TargetCell.Interior.Color = $SourceCell.Interior.Color
    $TargetCell.Font.Color = $SourceCell.Font.Color
    $TargetCell.Font.Bold = $SourceCell.Font.Bold
    $TargetCell.Font.Italic = $SourceCell.Font.Italic
    $TargetCell.Font.Name = $SourceCell.Font.Name
    $TargetCell.Font.Size = $SourceCell.Font.Size
    $TargetCell.HorizontalAlignment = $SourceCell.HorizontalAlignment
    $TargetCell.VerticalAlignment = $SourceCell.VerticalAlignment
}

function Wait-FileReadable {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [int]$Retries = 20,
        [int]$DelayMilliseconds = 100
    )

    for ($attempt = 1; $attempt -le $Retries; $attempt++) {
        try {
            $stream = [System.IO.File]::Open($Path, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
            $stream.Close()
            return
        } catch {
            if ($attempt -eq $Retries) {
                throw
            }
            Start-Sleep -Milliseconds $DelayMilliseconds
        }
    }
}

$Workspace = Resolve-FullPath $Workspace
$ConfRoot = Resolve-FullPath $ConfRoot
$dataDir = Resolve-FullPath (Join-Path $ConfRoot "Datas")
$tempDir = Resolve-FullPath (Join-Path $ConfRoot ".tmp")
$sourcePath = Join-Path $dataDir "level.xlsx"
$backupPath = Join-Path $tempDir "level.xlsx"
$legacyBackupPath = Join-Path $dataDir "level.xlsx.bak"

if ($Mode -eq "restore") {
    $restoreBackup = Get-ChildItem -LiteralPath $tempDir -Force -Filter "level.*.xlsx" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($restoreBackup) {
        Copy-Item -LiteralPath $restoreBackup.FullName -Destination $sourcePath -Force
        [void](Try-RemovePathWithRetry -Path $restoreBackup.FullName)
        Remove-ExcelLockFiles -Directory $tempDir
        Remove-ExcelLockFiles -Directory $dataDir
        Write-Host "[Luban] Restored $sourcePath"
    } elseif (Test-Path $backupPath) {
        Copy-Item -LiteralPath $backupPath -Destination $sourcePath -Force
        [void](Try-RemovePathWithRetry -Path $backupPath)
        Remove-ExcelLockFiles -Directory $tempDir
        Remove-ExcelLockFiles -Directory $dataDir
        Write-Host "[Luban] Restored $sourcePath from legacy temp backup"
    } elseif (Test-Path $legacyBackupPath) {
        Copy-Item -LiteralPath $legacyBackupPath -Destination $sourcePath -Force
        [void](Try-RemovePathWithRetry -Path $legacyBackupPath)
        Remove-ExcelLockFiles -Directory $tempDir
        Remove-ExcelLockFiles -Directory $dataDir
        Write-Host "[Luban] Restored $sourcePath from legacy backup"
    } else {
        Remove-ExcelLockFiles -Directory $tempDir
        Remove-ExcelLockFiles -Directory $dataDir
        Write-Host "[Luban] No backup found, skip restore."
    }
    return
}

if (-not (Test-Path $sourcePath)) {
    throw "Missing source table: $sourcePath"
}

New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
Remove-ExcelLockFiles -Directory $tempDir
Remove-ExcelLockFiles -Directory $dataDir
$backupPath = Join-Path $tempDir ("level.{0}.xlsx" -f ([Guid]::NewGuid().ToString("N")))
Copy-Item -LiteralPath $sourcePath -Destination $backupPath -Force

$excel = $null
$sourceWorkbook = $null
$sourceSheet = $null
$targetWorkbook = $null
$targetSheet = $null

try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false

    $sourceWorkbook = $excel.Workbooks.Open($backupPath)
    $sourceSheet = $sourceWorkbook.Worksheets.Item(1)

    $usedRange = $sourceSheet.UsedRange
    $lastRow = $usedRange.Rows.Count
    $lastColumn = $usedRange.Columns.Count
    $columnMap = Get-ColumnMap -Sheet $sourceSheet -LastColumn $lastColumn

    $targetWorkbook = $excel.Workbooks.Add()
    $targetSheet = $targetWorkbook.Worksheets.Item(1)
    $targetSheet.Name = "level"

    $headers = @(
        @("##var", "id", "title", "tip", "time", "target_score", "image_path1", "image_path2", "image_path3", "tip_infos"),
        @("##type", "int", "string", "string", "int", "int", "string", "string", "string", "(list#sep=|),string"),
        @("##group", "c", "c", "c", "c", "c", "c", "c", "c", "c"),
        @("##", "唯一字段", "标题", "提示", "时间", "分数", "图片1", "图片2", "图片3", "提示列表")
    )

    for ($row = 0; $row -lt $headers.Count; $row++) {
        for ($column = 0; $column -lt $headers[$row].Count; $column++) {
            $targetSheet.Cells.Item($row + 1, $column + 1) = $headers[$row][$column]
        }
    }

    for ($row = 1; $row -le 4; $row++) {
        for ($column = 1; $column -le $headers[0].Count; $column++) {
            $sourceCell = $sourceSheet.Cells.Item($row, [Math]::Min($column, $lastColumn))
            $targetCell = $targetSheet.Cells.Item($row, $column)
            Copy-CellStyle -SourceCell $sourceCell -TargetCell $targetCell
            Release-ComObject $sourceCell
            Release-ComObject $targetCell
        }
    }

    for ($column = 1; $column -le $headers[0].Count; $column++) {
        $targetSheet.Columns.Item($column).ColumnWidth = $sourceSheet.Columns.Item([Math]::Min($column, $lastColumn)).ColumnWidth
    }

    $targetRow = 5
    for ($sourceRow = 5; $sourceRow -le $lastRow; $sourceRow++) {
        $id = Get-CellText -Sheet $sourceSheet -Row $sourceRow -ColumnMap $columnMap -Aliases @("id", "Id", "ID")
        if ([string]::IsNullOrWhiteSpace($id)) {
            continue
        }

        $title = Get-CellText -Sheet $sourceSheet -Row $sourceRow -ColumnMap $columnMap -Aliases @("title", "Title")
        $tip = Get-CellText -Sheet $sourceSheet -Row $sourceRow -ColumnMap $columnMap -Aliases @("tip", "Tip")
        $time = Get-CellText -Sheet $sourceSheet -Row $sourceRow -ColumnMap $columnMap -Aliases @("time", "Time")
        $targetScore = Get-CellText -Sheet $sourceSheet -Row $sourceRow -ColumnMap $columnMap -Aliases @("target_score", "targetScore", "TargetScore")
        $imagePath1 = Get-CellText -Sheet $sourceSheet -Row $sourceRow -ColumnMap $columnMap -Aliases @("image_path1", "imagePath1", "ImagePath1", "imagePath", "ImagePath")
        $imagePath2 = Get-CellText -Sheet $sourceSheet -Row $sourceRow -ColumnMap $columnMap -Aliases @("image_path2", "imagePath2", "ImagePath2")
        $imagePath3 = Get-CellText -Sheet $sourceSheet -Row $sourceRow -ColumnMap $columnMap -Aliases @("image_path3", "imagePath3", "ImagePath3")
        $tipInfos = Convert-ListValue (Get-CellText -Sheet $sourceSheet -Row $sourceRow -ColumnMap $columnMap -Aliases @("tip_infos", "tipInfos", "TipInfos"))

        $values = @($id, $title, $tip, $time, $targetScore, $imagePath1, $imagePath2, $imagePath3, $tipInfos)
        for ($column = 0; $column -lt $values.Count; $column++) {
            $targetSheet.Cells.Item($targetRow, $column + 2) = $values[$column]
        }

        $targetRow++
    }

    $xlOpenXmlWorkbook = 51
    $targetWorkbook.SaveAs($sourcePath, $xlOpenXmlWorkbook)
    Write-Host "[Luban] Prepared $sourcePath"
}
finally {
    if ($targetWorkbook) {
        $targetWorkbook.Close($false) | Out-Null
    }
    if ($sourceWorkbook) {
        $sourceWorkbook.Close($false) | Out-Null
    }
    if ($excel) {
        $excel.Quit()
    }

    Release-ComObject $targetSheet
    Release-ComObject $targetWorkbook
    Release-ComObject $sourceSheet
    Release-ComObject $sourceWorkbook
    Release-ComObject $excel

    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()

    Remove-ExcelLockFiles -Directory $dataDir
    Remove-ExcelLockFiles -Directory $tempDir
    if ($Mode -eq "prepare") {
        Wait-FileReadable -Path $sourcePath
    }
}
