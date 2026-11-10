@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo === Hangar01 Git kurulumu ===
where git >nul 2>nul || (echo Git yuklu degil. https://git-scm.com/download/win adresinden kurup bu dosyayi tekrar calistir. & pause & exit /b 1)
if not exist "Assets" (echo Bu dosya Unity projesinin KOK klasorunde olmali ^(Assets klasorunun yaninda^). & pause & exit /b 1)
for /f "delims=" %%i in ('git config --global user.name') do set GN=%%i
if "%GN%"=="" set /p GN=GitHub kullanici adin: 
git config --global user.name "%GN%"
for /f "delims=" %%i in ('git config --global user.email') do set GE=%%i
if "%GE%"=="" set /p GE=GitHub e-postan: 
git config --global user.email "%GE%"
if not exist ".git" git init
git add .
git commit -m "Rota 02: hedefe yonelme, radar, Gizmos"
git branch -M main
echo.
echo Simdi github.com/new adresinde BOS bir depo olustur (README ekleme).
set /p URL=Deponun adresini yapistir (https://github.com/.../....git): 
git remote remove origin >nul 2>nul
git remote add origin %URL%
git push -u origin main
echo.
echo Bitti. Depo baglantisi: %URL%
pause
