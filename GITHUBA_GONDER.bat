@echo off
cd /d "%~dp0"
echo === Hangar01_Unity projesi GitHub'a gonderiliyor ===
echo.
echo kurulum*_log.txt>> .gitignore
echo kurulum*_sonuc.txt>> .gitignore
git rm -r -q --cached --ignore-unmatch kurulum_log.txt kurulum_sonuc.txt kurulum3_log.txt kurulum3_sonuc.txt
git add -A
git commit -q -m "Hafta 1-3: Unity projesi (Rota02, Rota03, betikler, girdi tablosu)"
git remote remove origin 2>nul
git remote add origin https://github.com/esra012/Hangar01_Unity.git
echo [1/3] Depodaki mevcut commit aliniyor...
git fetch origin
git merge origin/main --allow-unrelated-histories --no-edit -m "Depodaki ilk commit ile birlestir"
echo [2/3] Zip dosyasi depodan kaldiriliyor (bilgisayarindaki kopyaya dokunulmaz)...
git rm -q --ignore-unmatch 24080410202_EsraBaran_H01-H03.zip
git commit -q -m "Zip yerine acik proje dosyalari"
echo [3/3] Gonderiliyor. Tarayicida GitHub girisi acilirsa onayla...
git push -u origin main
echo.
if errorlevel 1 (echo GONDERIM BASARISIZ. Bu pencerenin ekran goruntusunu Claude'a at.) else (echo TAMAM: https://github.com/esra012/Hangar01_Unity)
pause
