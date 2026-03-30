@echo off
chcp 65001 >nul
echo ===================================================
echo     ATAS i18n Fallback Synchronization Tool
echo ===================================================
echo.

echo [1/3] Exporting strings from ATAS installations...
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "%~dp0export_strings.ps1"
if %errorlevel% neq 0 goto :error

echo.
echo [2/3] Comparing Alpha vs Stable and generating fallbacks...
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "%~dp0generate_fallbacks_and_log.ps1"
if %errorlevel% neq 0 goto :error

echo.
echo [3/3] Synchronizing fallbacks to local .resx files...
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "%~dp0sync_fallbacks_to_resx.ps1"
if %errorlevel% neq 0 goto :error

echo.
echo ===================================================
echo SUCCESS: Process completed successfully!
echo Check the fallbacks-data folder for the Graduation Log.
echo ===================================================
pause
exit /b 0

:error
echo.
echo ===================================================
echo ERROR: An error occurred during the process.
echo ===================================================
pause
exit /b 1