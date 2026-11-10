# Varlık lisans kaydı

Projedeki her dış ya da üretilmiş varlığın kaynağı. Kaynağı belirsiz dosya bırakılmaz.

## Assets/UI — Hafta 4 arayüz görselleri

| Dosya | Föy prompt'u | Nasıl üretildi | Tarih | Durum |
| --- | --- | --- | --- | --- |
| hud-cerceve.png | 01 · HUD çerçevesi | Python/Pillow betiği (`Belgeler/arayuz_gorselleri.py`), Claude ile yazıldı | 10.11.2026 | Yer tutucu |
| gosterge-hiz.png | 02 · Kadranlar (ibresiz) | aynı betik | 10.11.2026 | Yer tutucu |
| gosterge-yukseklik.png | 02 · Kadranlar (ibresiz) | aynı betik | 10.11.2026 | Yer tutucu |
| gosterge-ibre.png | 03 · İbre | aynı betik | 10.11.2026 | Yer tutucu |
| gaz-kolu-plaka.png | 04 · Gaz kolu plakası | aynı betik | 10.11.2026 | Yer tutucu |
| gaz-kolu-topuz.png | 05 · Gaz kolu topuzu | aynı betik | 10.11.2026 | Yer tutucu |
| pusula-serit.png | 06 · Pusula şeridi | aynı betik | 10.11.2026 | Yer tutucu |
| hedef-isaretleri.png | 07 · Hedef işaretleri sayfası | aynı betik | 10.11.2026 | Yer tutucu |
| dugme-seti.png | 08 · Düğme seti | aynı betik | 10.11.2026 | Yer tutucu |
| puan-tablosu.png | 09 · Görev sonu puan tablosu | aynı betik | 10.11.2026 | Yer tutucu |
| menu-arkaplan.png | 10 · Ana menü arka planı | aynı betik | 10.11.2026 | Yer tutucu |

Bu görseller bir görsel üretim modeliyle değil, föydeki palet (#6B7247, #9A7B4F, #E8E2D0, #9E4B38,
#B08D57, #2B2A26) ve yapı kurallarına göre kodla çizildi. Dış kaynak, yazı tipi ya da hazır görsel
kullanılmadı; telif sorunu yok.

Görsel modeliyle üretilen sürümler aynı dosya adlarıyla `Assets/UI/` altına konduğunda bu satırlar
şu bilgilerle güncellenecek: kullanılan araç ve sürümü, üretim tarihi, kullanılan prompt (föydeki
tam metin) ve aracın kullanım koşullarının ticari olmayan ders projesine izin verdiği.

Not: Kodla çizilen görsellerin ölçüleri (atlas kutuları, pusula bandı, gaz plakası) UcusHUD.cs içinde
sabit olarak duruyor. Modelden gelen görsellerde bu kutuları resim düzenleyicide piksel olarak
yeniden ölçüp UcusHUD.cs'deki sabitleri güncellemek gerekir.

## Diğer varlıklar

| Dosya | Kaynak |
| --- | --- |
| Assets/Models/Nu_D36.fbx | Blender 5.2 betiğiyle (build_nud36.py) üretildi, Hafta 1 |
| Packages: com.unity.inputsystem 1.19.0 | Unity Technologies, Unity Companion License |
