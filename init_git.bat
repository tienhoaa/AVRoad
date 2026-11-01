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
echo [1/4] Kiem tra Git...
where git >nul 2>nul
if %errorlevel% neq 0 (
    echo [LOI] Khong tim thay Git! Vui long cai Git hoac khoi dong lai may.
    pause
    exit /b
)
git --version
echo.

cd /d "%~dp0"

echo [2/4] Khoi tao Repository va Commit...
if not exist ".git" (
    git init
)

git config user.name >nul 2>nul
if %errorlevel% neq 0 (
    git config --global user.name "tienhoaa"
    git config --global user.email "tienhoaa@github.com"
)

git add .
git commit -m "Phat hanh AVRoad v1.0 - Ban va Hotfix GitHub" >nul 2>nul
git branch -M main
echo Da san sang tep tin.
echo.

echo =====================================================================
echo           NHAP THONG TIN TAI KHOAN GITHUB CUA BAN
echo =====================================================================
echo.
echo Chu y ve Mat khau:
echo - Neu ban co Personal Access Token: hay dan vao dong mat khau.
echo - Neu KHONG co Token: CU DE TRONG VA NHAN ENTER, trinh duyet web
echo   se tu dong mo len de ban bam 1-click 'Authorize' dang nhap!
echo =====================================================================
echo.

set /p gh_user="1. Nhap Username GitHub [Mac dinh: tienhoaa]: "
if "!gh_user!"=="" set gh_user=tienhoaa

set /p gh_repo="2. Nhap Ten Repo [Mac dinh: AVRoad]: "
if "!gh_repo!"=="" set gh_repo=AVRoad

set /p gh_token="3. Nhap Token/Mat khau (hoac de trong roi Enter de dung Web): "

echo.
echo [3/4] Dang ket noi voi GitHub...
git remote remove origin >nul 2>nul

if "!gh_token!"=="" (
    git remote add origin https://github.com/!gh_user!/!gh_repo!.git
) else (
    git remote add origin https://!gh_user!:!gh_token!@github.com/!gh_user!/!gh_repo!.git
)

echo Da ket noi: https://github.com/!gh_user!/!gh_repo!
echo.
echo [4/4] Dang day du lieu len GitHub (git push)...
echo Vui long doi... Neu co hop thoai trinh duyet hien len, hay bam 'Sign in with browser'.
echo.

git push -u origin main

echo.
if %errorlevel% equ 0 (
    echo =====================================================================
    echo            THANH CONG! DU AN DA LEN GITHUB!
    echo =====================================================================
    echo Xem du an tai: https://github.com/!gh_user!/!gh_repo!
) else (
    echo =====================================================================
    echo [THONG BAO] Neu gap loi ket noi:
    echo 1. Hay dam bao ban da bam nut 'Create repository' tren GitHub.
    echo 2. Kiem tra dung ten tai khoan va repo.
    echo =====================================================================
)

echo.
pause
