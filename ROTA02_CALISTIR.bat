@echo off
chcp 65001 >nul
cd /d "%~dp0"
set UNITY=C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe
if not exist "%UNITY%" (echo Unity bulunamadi: %UNITY% & pause & exit /b 1)
echo [1/2] Proje kuruluyor ve Rota02 sahnesi olusturuluyor. Ilk acilis 3-10 dakika surebilir, pencereyi kapatma...
"%UNITY%" -batchmode -quit -projectPath "%~dp0." -executeMethod Rota02Kurulum.KomutSatirindanKur -logFile "%~dp0kurulum_log.txt"
echo Cikis kodu: %ERRORLEVEL% > "%~dp0kurulum_sonuc.txt"
if errorlevel 1 (echo Kurulum hata verdi, kurulum_log.txt dosyasina bak. & pause & exit /b 1)
echo [2/2] Unity aciliyor, sahne Play modunda baslayacak...
start "" "%UNITY%" -projectPath "%~dp0." -executeMethod Rota02Kurulum.AcVeOyna
