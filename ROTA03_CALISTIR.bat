@echo off
cd /d "%~dp0"
set UNITY=C:\Program Files\Unity\Hub\Editor\6000.6.7f1\Editor\Unity.exe
echo [1/2] Rota03 kuruluyor. Paket indirilecegi icin birkac dakika surebilir, pencereyi kapatma...
"%UNITY%" -batchmode -quit -projectPath "%~dp0." -executeMethod Rota03Kurulum.KomutSatirindanKur -logFile "%~dp0kurulum3_log.txt"
echo Cikis kodu: %ERRORLEVEL% > "%~dp0kurulum3_sonuc.txt"
if errorlevel 1 (echo Kurulum hata verdi. Claude kurulum3_log.txt dosyasina bakacak. & pause & exit /b 1)
echo [2/2] Unity aciliyor. Goruntuler otomatik cekilecek, 1 dakika dokunma.
start "" "%UNITY%" -projectPath "%~dp0." -executeMethod Rota03Kurulum.TeslimCek
