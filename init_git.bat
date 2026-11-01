@echo off
setlocal enabledelayedexpansion
title AVRoad - Day len GitHub

if exist "C:\Program Files\Git\cmd\git.exe" (
    set "PATH=C:\Program Files\Git\cmd;%PATH%"
)
if exist "C:\Program Files (x86)\Git\cmd\git.exe" (
    set "PATH=C:\Program Files (x86)\Git\cmd;%PATH%"
)

cls
echo =====================================================================
echo           AVRoad - KET NOI VA DAY DU AN LEN GITHUB
echo =====================================================================
echo.

cd /d "%~dp0"

echo [1/3] Kiem tra Git va commit tep tin...
git add -A
git commit -m "Cap nhat AVRoad v1.0" >nul 2>nul
git branch -M main

echo [2/3] Ket noi toi kho luu tru:
echo https://github.com/tienhoaa/AVRoad.git
git remote remove origin >nul 2>nul
git remote add origin https://github.com/tienhoaa/AVRoad.git

echo.
echo [3/3] DANG DAY DU LIEU LEN GITHUB (git push)...
echo (Trinh duyet se tu dong bat len mot hop thoai, hay bam 'Sign in with browser' hoac 'Authorize')
echo.

git push -u origin main

echo.
if %errorlevel% equ 0 (
    echo =====================================================================
    echo            THANH CONG RUC RO! DU AN DA LEN GITHUB!
    echo =====================================================================
    echo Xem du an tai: https://github.com/tienhoaa/AVRoad
    echo.
    echo Ban hay vao muc Releases tren GitHub de tao Release v1.0 va dinh kem file patch_1.0.zip nhe!
) else (
    echo =====================================================================
    echo [THONG BAO] Neu bao loi dang nhap:
    echo Hay dam bao ban da bam nut 'Create repository' tren https://github.com/tienhoaa/AVRoad
    echo =====================================================================
)

echo.
pause
