@echo off
cd /d "%~dp0"
echo === Hangar01_Unity GitHub'a gonderiliyor ===
set /p MESAJ=Bu guncellemenin kisa aciklamasi (bos birakirsan "Haftalik guncelleme"): 
if "%MESAJ%"=="" set MESAJ=Haftalik guncelleme
git add -A
git commit -m "%MESAJ%"
git push
echo.
if errorlevel 1 (echo GONDERIM BASARISIZ. Bu pencerenin ekran goruntusunu Claude'a at.) else (echo TAMAM: https://github.com/esra012/Hangar01_Unity)
pause
