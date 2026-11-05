@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: ============================================================================
:: Fix_CleanPC_CAD7.bat
:: Script độc lập bổ sung toàn bộ môi trường nền AutoCAD 2007 cho máy Windows sạch
:: (Những phần mà trình gỡ cài đặt CAD7 thường để sót lại trên máy cũ)
:: ============================================================================

:: 1. Tự động xin quyền Administrator nếu chưa có
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [AVRoad] Đang yêu cầu quyền Administrator...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    exit /b
)

set "APP_DIR=C:\AVRoad_2026"
if exist "%~dp0acad.exe" set "APP_DIR=%~dp0"
if "%APP_DIR:~-1%"=="\" set "APP_DIR=%APP_DIR:~0,-1%"

echo ============================================================================
echo   AVROAD 2.1 RC2 - BỔ SUNG MÔI TRƯỜNG MÁY SẠCH (CHƯA TỪNG CÀI CAD 2007)
echo   Thư mục đích: %APP_DIR%
echo ============================================================================

:: 2. Tạo các thư mục AppData / LocalAppData / ProgramData (Uninstaller CAD7 không xóa các thư mục này)
set "LOCAL_ENU=%LOCALAPPDATA%\Autodesk\AutoCAD 2007\R17.0\enu"
set "ROAM_ENU=%APPDATA%\Autodesk\AutoCAD 2007\R17.0\enu"
set "PDATA_ENU=C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\enu"
set "PDATA_ADLM=C:\ProgramData\Autodesk\AutoCAD 2007\R17.0\Adlm"

mkdir "%LOCAL_ENU%" 2>nul
mkdir "%LOCAL_ENU%\Template" 2>nul
mkdir "%LOCAL_ENU%\WebServices" 2>nul
mkdir "%ROAM_ENU%" 2>nul
mkdir "%ROAM_ENU%\Support\Profiles\AVRoad" 2>nul
mkdir "%ROAM_ENU%\Data Links" 2>nul
mkdir "%ROAM_ENU%\support\icons" 2>nul
mkdir "%PDATA_ENU%" 2>nul
mkdir "%PDATA_ADLM%" 2>nul

echo [1/6] Đã tạo đầy đủ thư mục cấu hình UserData (LocalAppData / Roaming / ProgramData).

:: 3. Tạo acad.exe.config hỗ trợ cả .NET 2.0 và .NET 4.0 (để máy Win 10/11 sạch nạp được SinglePanel21.dll)
(
echo ^<?xml version="1.0" encoding="utf-8"?^>
echo ^<configuration^>
echo   ^<startup useLegacyV2RuntimeActivationPolicy="true"^>
echo     ^<supportedRuntime version="v2.0.50727"/^>
echo     ^<supportedRuntime version="v4.0"/^>
echo   ^</startup^>
echo   ^<runtime^>
echo     ^<generatePublisherEvidence enabled="false"/^>
echo   ^</runtime^>
echo ^</configuration^>
) > "%APP_DIR%\acad.exe.config"

echo [2/6] Đã cấu hình acad.exe.config (.NET CLR 2.0/4.0 cho UI21 SinglePanel21).

:: 4. Thiết lập khóa Registry HKLM 32-bit & chặn userdata.dll xóa Profile khi không có MSI CAD7
set "HKLM_ACAD=HKLM\SOFTWARE\WOW6432Node\Autodesk\AutoCAD\R17.0\ACAD-5001:409"
set "HKCU_ACAD=HKCU\Software\Autodesk\AutoCAD\R17.0\ACAD-5001:409"

reg add "HKLM\SOFTWARE\WOW6432Node\Autodesk" /v "CurProd" /t REG_SZ /d "" /f >nul
reg add "HKLM\SOFTWARE\WOW6432Node\Autodesk\AutoCAD" /v "CurVer" /t REG_SZ /d "R17.0" /f >nul
reg add "HKLM\SOFTWARE\WOW6432Node\Autodesk\AutoCAD\R17.0" /v "CurVer" /t REG_SZ /d "ACAD-5001:409" /f >nul

reg add "%HKLM_ACAD%" /v "AcadLocation" /t REG_SZ /d "%APP_DIR%" /f >nul
reg add "%HKLM_ACAD%" /v "Location" /t REG_SZ /d "%APP_DIR%" /f >nul
reg add "%HKLM_ACAD%" /v "ProductName" /t REG_SZ /d "AutoCAD 2007" /f >nul
reg add "%HKLM_ACAD%" /v "Release" /t REG_SZ /d "17.0.54.110" /f >nul
reg add "%HKLM_ACAD%" /v "SerialNumber" /t REG_SZ /d "000-00000000" /f >nul
reg add "%HKLM_ACAD%" /v "StandaloneNetworkType" /t REG_DWORD /d 1 /f >nul
reg add "%HKLM_ACAD%" /v "Roaming" /t REG_DWORD /d 1 /f >nul
reg add "%HKLM_ACAD%" /v "RoamableRootFolder" /t REG_SZ /d "%ROAM_ENU%\" /f >nul
reg add "%HKLM_ACAD%" /v "LocalRootFolder" /t REG_SZ /d "%LOCAL_ENU%\" /f >nul
reg add "%HKLM_ACAD%" /v "ExtensionRootFolder" /t REG_SZ /d "%PDATA_ENU%\" /f >nul
reg add "%HKLM_ACAD%" /v "AdLMInfoPath" /t REG_SZ /d "%PDATA_ADLM%\" /f >nul
reg add "%HKLM_ACAD%" /v "LocaleId" /t REG_DWORD /d 1033 /f >nul
reg add "%HKLM_ACAD%" /v "LangAbbrev" /t REG_SZ /d "enu" /f >nul
reg add "%HKLM_ACAD%" /v "Language" /t REG_SZ /d "English" /f >nul
reg add "%HKLM_ACAD%" /v "ProductId" /t REG_SZ /d "5001" /f >nul
reg add "%HKLM_ACAD%" /v "ProductCode" /t REG_SZ /d "{5783F2D7-5001-0409-0002-0060B0CE6BBA}" /f >nul
reg add "%HKLM_ACAD%" /v "AutoMigrate" /t REG_DWORD /d 0 /f >nul
reg add "%HKLM_ACAD%\AdLM" /v "Type" /t REG_DWORD /d 42 /f >nul

:: Quan trọng: Tạo khóa UserData rỗng (xóa giá trị MSI {5783F2D7-...} trong HKLM) để userdata.dll không gọi MsiConfigureProductExW
reg add "%HKLM_ACAD%\UserData" /f >nul
reg delete "%HKLM_ACAD%\UserData" /v "{5783F2D7-5001-0409-0002-0060B0CE6BBA}" /f >nul 2>&1

:: Thiết lập HKCU tương ứng và khóa .dwt (để acad.exe 0x408030 trả về 1, không hiện cảnh báo "not installed properly")
reg add "HKCU\Software\Classes\.dwt" /ve /t REG_SZ /d "AutoCADTemplate.17" /f >nul
reg add "HKCU\Software\Autodesk" /v "CurProd" /t REG_SZ /d "AutoCAD" /f >nul
reg add "HKCU\Software\Autodesk\AutoCAD" /v "CurVer" /t REG_SZ /d "R17.0" /f >nul
reg add "HKCU\Software\Autodesk\AutoCAD\R17.0" /v "CurVer" /t REG_SZ /d "ACAD-5001:409" /f >nul
reg add "%HKCU_ACAD%" /v "RoamableRootFolder" /t REG_SZ /d "%ROAM_ENU%\" /f >nul
reg add "%HKCU_ACAD%" /v "LocalRootFolder" /t REG_SZ /d "%LOCAL_ENU%\" /f >nul
reg add "%HKCU_ACAD%" /v "AcadLocation" /t REG_SZ /d "%APP_DIR%" /f >nul
reg add "%HKCU_ACAD%" /v "Location" /t REG_SZ /d "%APP_DIR%" /f >nul
reg add "%HKCU_ACAD%\UserData" /v "Version" /t REG_SZ /d "17.0.54.0" /f >nul
reg add "%HKCU_ACAD%\UserData\{5783F2D7-5001-0409-0002-0060B0CE6BBA}" /v "Version" /t REG_SZ /d "17.0.54.0" /f >nul
reg add "%HKCU_ACAD%\Profiles" /ve /t REG_SZ /d "AVRoad" /f >nul

echo [3/6] Đã cấu hình khóa Registry HKLM/HKCU & tắt kiểm tra MSI của userdata.dll.

:: 5. Đăng ký 3 CLSID COM axdb.dll (sửa lỗi Properties / QSELECT theo chuẩn Codex)
for %%G in (
    "{B53CEF4B-1A13-49DE-BBC5-A7100FB2F38C}"
    "{F78DCF7C-043D-45FC-9D21-676FC307BA3F}"
    "{4BD03680-3C0F-4501-AFF7-3D008586917F}"
) do (
    reg add "HKCU\Software\Classes\CLSID\%%~G\InprocServer32" /ve /t REG_SZ /d "%APP_DIR%\axdb.dll" /f >nul
    reg add "HKCU\Software\Classes\CLSID\%%~G\InprocServer32" /v "ThreadingModel" /t REG_SZ /d "Both" /f >nul
)

echo [4/6] Đã đăng ký 3 khóa COM axdb.dll cho Properties và QSELECT.

:: 6. Nạp lại AVRoad_Profile.arg (thay thế đường dẫn máy cũ sang user hiện tại)
if exist "%APP_DIR%\AVRoad_Profile.arg" (
    powershell -NoProfile -ExecutionPolicy Bypass -Command ^
        "$arg = Get-Content -Raw '%APP_DIR%\AVRoad_Profile.arg';" ^
        "$u = $env:USERPROFILE.Replace('\','\\');" ^
        "$arg = [regex]::Replace($arg, 'C:\\\\Users\\\\[^\\\\\"]+', $u, 'IgnoreCase');" ^
        "$tmp = Join-Path $env:TEMP 'avroad_clean_profile.reg';" ^
        "[IO.File]::WriteAllText($tmp, $arg, [Text.Encoding]::Unicode);" ^
        "reg import $tmp | Out-Null; Remove-Item $tmp -Force -ErrorAction SilentlyContinue"
    echo [5/6] Đã nạp lại Profile AVRoad (SinglePanel21 + Menu) theo đường dẫn máy hiện tại.
) else (
    echo [5/6] Bỏ qua nạp AVRoad_Profile.arg (không tìm thấy file trong %APP_DIR%).
)

:: 7. Đăng ký Uninstall vào Windows Installed Apps / Control Panel
reg add "HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026" /v "DisplayName" /t REG_SZ /d "AVRoad 2026 Solution" /f >nul
reg add "HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026" /v "DisplayVersion" /t REG_SZ /d "2.1.0.2" /f >nul
reg add "HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026" /v "Publisher" /t REG_SZ /d "AV Technology CDE" /f >nul
reg add "HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026" /v "UninstallString" /t REG_SZ /d "\"%APP_DIR%\Uninstall.exe\"" /f >nul
reg add "HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026" /v "DisplayIcon" /t REG_SZ /d "%APP_DIR%\AVRoad.ico" /f >nul
reg add "HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026" /v "InstallLocation" /t REG_SZ /d "%APP_DIR%" /f >nul
reg add "HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026" /v "NoModify" /t REG_DWORD /d 1 /f >nul
reg add "HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\AVRoad2026" /v "NoRepair" /t REG_DWORD /d 1 /f >nul

echo [6/6] Đã đăng ký bộ gỡ cài đặt vào Windows Installed Apps.
echo ============================================================================
echo   HOÀN TẤT! Bạn có thể mở AVRoad ngay bây giờ.
echo ============================================================================
pause
