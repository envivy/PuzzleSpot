@echo off
setlocal

set WORKSPACE=%~dp0..
set CONF_ROOT=%WORKSPACE%\DataTables
set OUTPUT_CODE_DIR=%WORKSPACE%\Assets\Scripts\Gen\Luban
set OUTPUT_DATA_DIR=%WORKSPACE%\Assets\Resources\LubanData
set LOG_FILE=%CONF_ROOT%\gen_client.log
set EXIT_CODE=0
set PREPARE_EXIT=0
set LUBAN_DLL=
set PREPARE_SCRIPT=%CONF_ROOT%\prepare_luban_tables.ps1

if exist "%WORKSPACE%\Tools\Luban\Luban.dll" (
    set LUBAN_DLL=%WORKSPACE%\Tools\Luban\Luban.dll
)

if "%LUBAN_DLL%"=="" if exist "%CONF_ROOT%\Tools\Luban\Luban.dll" (
    set LUBAN_DLL=%CONF_ROOT%\Tools\Luban\Luban.dll
)

> "%LOG_FILE%" echo [Luban] Generate started
>> "%LOG_FILE%" echo [Luban] Workspace=%WORKSPACE%
>> "%LOG_FILE%" echo [Luban] ConfRoot=%CONF_ROOT%
>> "%LOG_FILE%" echo [Luban] OutputCodeDir=%OUTPUT_CODE_DIR%
>> "%LOG_FILE%" echo [Luban] OutputDataDir=%OUTPUT_DATA_DIR%

if "%LUBAN_DLL%"=="" (
    echo [Luban] Missing Luban.dll
    echo [Luban] Checked:
    echo   %WORKSPACE%\Tools\Luban\Luban.dll
    echo   %CONF_ROOT%\Tools\Luban\Luban.dll
    echo [Luban] Copy the Luban tool files into one of those folders.
    >> "%LOG_FILE%" echo [Luban] ERROR Missing Luban.dll
    set EXIT_CODE=1
    goto :finish
)

echo [Luban] Using %LUBAN_DLL%
>> "%LOG_FILE%" echo [Luban] Using %LUBAN_DLL%

powershell -NoProfile -ExecutionPolicy Bypass -File "%PREPARE_SCRIPT%" -Workspace "%WORKSPACE%" -ConfRoot "%CONF_ROOT%" >> "%LOG_FILE%" 2>&1

set EXIT_CODE=%ERRORLEVEL%

if not "%EXIT_CODE%"=="0" (
    echo [Luban] Prepare tables failed. See %LOG_FILE%
    goto :finish
)

dotnet "%LUBAN_DLL%" ^
    -t client ^
    -c cs-simple-json ^
    -d json ^
    --conf "%CONF_ROOT%\luban.conf" ^
    -x outputCodeDir="%OUTPUT_CODE_DIR%" ^
    -x outputDataDir="%OUTPUT_DATA_DIR%" >> "%LOG_FILE%" 2>&1

set EXIT_CODE=%ERRORLEVEL%

if not "%EXIT_CODE%"=="0" (
    echo [Luban] Generate failed. See %LOG_FILE%
    goto :finish
)

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$files = Get-ChildItem -Path '%OUTPUT_CODE_DIR%' -Recurse -Filter *.cs; " ^
    "foreach ($file in $files) { " ^
    "  $text = Get-Content -LiteralPath $file.FullName -Raw; " ^
    "  $updated = $text.Replace('using SimpleJSON;', 'using Luban.SimpleJSON;'); " ^
    "  if ($updated -ne $text) { Set-Content -LiteralPath $file.FullName -Value $updated; } " ^
    "}" >> "%LOG_FILE%" 2>&1

set EXIT_CODE=%ERRORLEVEL%

if not "%EXIT_CODE%"=="0" (
    echo [Luban] Post-process failed. See %LOG_FILE%
    goto :finish
)

echo [Luban] Generate succeeded.
echo [Luban] Log file: %LOG_FILE%
>> "%LOG_FILE%" echo [Luban] Generate succeeded.

:finish
powershell -NoProfile -ExecutionPolicy Bypass -File "%PREPARE_SCRIPT%" -Workspace "%WORKSPACE%" -ConfRoot "%CONF_ROOT%" -Mode restore >> "%LOG_FILE%" 2>&1
set PREPARE_EXIT=%ERRORLEVEL%
if not "%PREPARE_EXIT%"=="0" (
    echo [Luban] Restore level.xlsx failed. See %LOG_FILE%
    if "%EXIT_CODE%"=="0" set EXIT_CODE=%PREPARE_EXIT%
)
if not "%CMDCMDLINE%"=="%CMDCMDLINE:/c=%" pause
exit /b %EXIT_CODE%
