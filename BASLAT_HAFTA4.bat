@echo off
cd /d "%~dp0"
set UNITY=C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe
echo [1/2] Rota04 kuruluyor, pencereyi kapatma...
"%UNITY%" -batchmode -quit -projectPath "%~dp0." -executeMethod Rota04Kurulum.KomutSatirindanKur -logFile "%~dp0kurulum4_log.txt"
echo Cikis kodu: %ERRORLEVEL% > "%~dp0kurulum4_sonuc.txt"
if errorlevel 1 (echo Kurulum hata verdi. Claude kurulum4_log.txt dosyasina bakacak. & pause & exit /b 1)
echo [2/2] Unity aciliyor. Oyun 1 dakika boyunca kendi kendine test edilecek, dokunma.
start "" "%UNITY%" -projectPath "%~dp0." -executeMethod Rota04Kurulum.TestCek
