"""Hafta 4 · Arayüz yer tutucu görselleri (föydeki 10 prompt'un yapısına uygun).
Palet ve kurallar föyden: düz vektör, düşük ayrıntı, yazı yok, bayrak/amblem yok.
Kullanım: python arayuz_gorselleri.py <cikti_klasoru>
Aynı adlı PNG'leri görsel modelinden ürettiğinde bu dosyaların yerine koyabilirsin.
"""
import math, sys, os, json
from PIL import Image, ImageDraw, ImageFilter

OUT = sys.argv[1] if len(sys.argv) > 1 else "."
os.makedirs(OUT, exist_ok=True)
SS = 2  # süper örnekleme

ZEYTIN = (0x6B, 0x72, 0x47); TOPRAK = (0x9A, 0x7B, 0x4F); KIRLI_BEYAZ = (0xE8, 0xE2, 0xD0)
PAS = (0x9E, 0x4B, 0x38); PIRINC = (0xB0, 0x8D, 0x57); KOYU = (0x2B, 0x2A, 0x26)
def a(c, al=255): return (*c, al)
def koyulas(c, f): return tuple(max(0, int(v * f)) for v in c)

def tuval(w, h, saydam=True, renk=(0, 0, 0, 0)):
    im = Image.new("RGBA", (w * SS, h * SS), renk if saydam else a(renk[:3]))
    return im, ImageDraw.Draw(im)

def kaydet(im, ad, w, h):
    im.resize((w, h), Image.LANCZOS).save(os.path.join(OUT, ad))

def S(*v): return [x * SS for x in v]

def percin(d, x, y, r=5):
    d.ellipse(S(x - r, y - r, x + r, y + r), fill=a(koyulas(PIRINC, .75)))
    d.ellipse(S(x - r + 2, y - r + 1, x + r - 3, y + r - 3), fill=a(PIRINC))

def vida(d, x, y, r=9):
    d.ellipse(S(x - r, y - r, x + r, y + r), fill=a(koyulas(PIRINC, .7)))
    d.line(S(x - r + 3, y, x + r - 3, y), fill=a(KOYU), width=2 * SS)

# 01 · HUD çerçevesi 1920x1080: alt şerit, sol alt yuvarlak yuva, sağ alt dikdörtgen pano
def cerceve():
    W, H = 1920, 1080
    im, d = tuval(W, H)
    serit = koyulas(ZEYTIN, .8)
    d.rectangle(S(0, 1020, W, H), fill=a(serit))
    d.rectangle(S(0, 1020, W, 1026), fill=a(PIRINC))
    for x in range(30, W, 60): percin(d, x, 1050, 4)
    # sol alt: yuvarlak yuva (merkez 200,895 yarıçap 175)
    cx, cy, r = 200, 895, 178
    d.polygon(S(0, 680, 260, 680, 420, 840, 420, 1080, 0, 1080), fill=a(serit))
    d.ellipse(S(cx - r, cy - r, cx + r, cy + r), fill=a(KOYU))
    d.ellipse(S(cx - r, cy - r, cx + r, cy + r), outline=a(PIRINC), width=10 * SS)
    for k in range(8):
        t = k * math.pi / 4; percin(d, cx + (r + 22) * math.cos(t), cy + (r + 22) * math.sin(t), 5)
    # sağ alt: dikdörtgen pano (1530,740)-(1890,1050)
    d.polygon(S(1920, 690, 1500, 690, 1490, 1080, 1920, 1080), fill=a(serit))
    d.rounded_rectangle(S(1530, 740, 1890, 1050), radius=18 * SS, fill=a(KOYU), outline=a(PIRINC), width=10 * SS)
    for (x, y) in [(1545, 755), (1875, 755), (1545, 1035), (1875, 1035)]: percin(d, x, y, 6)
    # aşınmış boya lekeleri
    for (x, y, w) in [(330, 1040, 40), (800, 1060, 60), (1300, 1035, 30), (60, 720, 25)]:
        d.ellipse(S(x, y, x + w, y + w * .4), fill=a(TOPRAK, 160))
    kaydet(im, "hud-cerceve.png", W, H)

# 02 · Kadranlar (ibresiz) — iki ayrı dosya 512x512
def kadran(ad, iki_halka):
    W = 512; c = W / 2
    im, d = tuval(W, W)
    d.ellipse(S(10, 10, W - 10, W - 10), fill=a(PIRINC))
    d.ellipse(S(28, 28, W - 28, W - 28), fill=a(koyulas(PIRINC, .75)))
    d.ellipse(S(40, 40, W - 40, W - 40), fill=a(KIRLI_BEYAZ))
    for k in range(6):
        t = k * math.pi / 3 + math.pi / 6; vida(d, c + 230 * math.cos(t) * .93, c + 230 * math.sin(t) * .93, 8)
    halkalar = [(205, 40, 2.5)] if not iki_halka else [(205, 50, 2.5), (150, 10, 1.5)]
    for (R, n, kal) in halkalar:
        for i in range(n + 1):
            aci = math.radians(-140 + 280 * i / n - 90)      # -140..+140, 12 yönü sıfır
            uzun = i % 5 == 0
            r1 = R - (34 if uzun else 18)
            d.line(S(c + R * math.cos(aci), c + R * math.sin(aci), c + r1 * math.cos(aci), c + r1 * math.sin(aci)),
                   fill=a(KOYU), width=int(kal * (2 if uzun else 1) * SS))
    # kırmızı bölge (hız kadranında üst sınır)
    if not iki_halka:
        d.arc(S(c - 200, c - 200, c + 200, c + 200), start=-90 + 112, end=-90 + 140, fill=a(PAS), width=10 * SS)
    d.rounded_rectangle(S(c - 60, c + 120, c + 60, c + 160), radius=6 * SS, fill=a(TOPRAK))
    d.line(S(c - 150, c - 160, c - 40, c - 200), fill=(255, 255, 255, 90), width=8 * SS)   # cam yansıması
    kaydet(im, ad, W, W)

# 03 · İbre: kare tuval, göbek tam merkezde, yukarı bakar
def ibre():
    W = 512; c = W / 2
    im, d = tuval(W, W)
    d.polygon(S(c, 40, c + 12, c, c, c + 60, c - 12, c), fill=a(PAS))
    d.polygon(S(c, 40, c + 4, c, c - 4, c), fill=a(koyulas(PAS, 1.25)))
    d.ellipse(S(c - 26, c - 26, c + 26, c + 26), fill=a(PIRINC))
    d.ellipse(S(c - 9, c - 9, c + 9, c + 9), fill=a(KOYU))
    kaydet(im, "gosterge-ibre.png", W, W)

# 04 · Gaz kolu plakası 256x1024 (kanal boş, sağda bölmeli şerit, eşik = kalkış hızı 26/42)
GAZ = {"kanal": [40, 80, 100, 944], "serit": [150, 80, 210, 944], "esik": 26 / 42}
def gaz_plaka():
    W, H = 256, 1024
    im, d = tuval(W, H)
    d.rounded_rectangle(S(8, 20, W - 8, H - 20), radius=24 * SS, fill=a(PIRINC))
    d.rounded_rectangle(S(18, 30, W - 18, H - 30), radius=18 * SS, fill=a(koyulas(PIRINC, .85)))
    x0, y0, x1, y1 = GAZ["kanal"]
    d.rounded_rectangle(S(x0, y0, x1, y1), radius=20 * SS, fill=a(KOYU))
    x0, y0, x1, y1 = GAZ["serit"]
    n = 18; h = (y1 - y0) / n; esik_y = y1 - GAZ["esik"] * (y1 - y0)
    for i in range(n):
        ya, yb = y0 + i * h + 3, y0 + (i + 1) * h - 3
        renk = KIRLI_BEYAZ if (ya + yb) / 2 < esik_y else ZEYTIN
        d.rectangle(S(x0, ya, x1, yb), fill=a(renk))
    d.rectangle(S(x0 - 8, esik_y - 4, x1 + 8, esik_y + 4), fill=a(PAS))
    for (x, y) in [(30, 44), (W - 30, 44), (30, H - 44), (W - 30, H - 44)]: percin(d, x, y, 7)
    kaydet(im, "gaz-kolu-plaka.png", W, H)

# 05 · Gaz kolu topuzu 512x512
def topuz():
    W = 512; c = W / 2
    im, d = tuval(W, W)
    d.rectangle(S(c - 22, c + 40, c + 22, W - 70), fill=a(koyulas(PIRINC, .7)))
    d.rounded_rectangle(S(c - 120, c - 90, c + 120, c + 60), radius=40 * SS, fill=a(PIRINC))
    for x in range(int(c - 100), int(c + 110), 22):
        d.line(S(x, c - 80, x, c + 50), fill=a(koyulas(PIRINC, .7)), width=4 * SS)
    d.line(S(c - 100, c - 70, c + 60, c - 70), fill=(255, 255, 255, 80), width=6 * SS)
    kaydet(im, "gaz-kolu-topuz.png", W, W)

# 06 · Pusula şeridi 1536x128, soldan sağa döşenebilir. Genişlik = 180°, 60 çentik (3°),
#      uzun çentik her 15°, boş plaka her 30° (u=0 -> 0°). Dizin üçgeni bandın ÜSTÜNDE ve ortada.
PUSULA = {"bantUst": 52 / 128, "bantAlt": 120 / 128, "ucgen": [728 / 1536, 0, 808 / 1536, 46 / 128], "derecePerGenislik": 180}
def pusula():
    W, H = 1536, 128
    im, d = tuval(W, H)
    yu, ya = 52, 120
    d.rectangle(S(0, yu, W, ya), fill=a(KIRLI_BEYAZ))
    d.rectangle(S(0, yu, W, yu + 5), fill=a(PIRINC)); d.rectangle(S(0, ya - 5, W, ya), fill=a(PIRINC))
    for i in range(60):
        x = i * W / 60
        uzun = i % 5 == 0
        d.line(S(x, yu + 5, x, yu + (40 if uzun else 20)), fill=a(KOYU), width=(4 if uzun else 2) * SS)
        if i % 10 == 0:   # 30° plakası (boş)
            d.rectangle(S(x - 22 if x > 0 else 0, ya - 26, x + 22, ya - 8), fill=a(TOPRAK))
    # x=0 plakasının sağ yarısı başta; sol yarısı sona (döşenince birleşsin)
    d.rectangle(S(W - 22, ya - 26, W, ya - 8), fill=a(TOPRAK))
    d.polygon(S(768 - 30, 6, 768 + 30, 6, 768, 44), fill=a(PAS))
    kaydet(im, "pusula-serit.png", W, H)

# 07 · Hedef işaretleri atlası 1024x1024 (parçalar arasında boşluk). Kutular sol üst kökenli, 0-1.
ATLAS = {
    "anaNisan": [64, 64, 448, 448], "yanNisan": [576, 96, 896, 416], "yonOku": [96, 576, 352, 832],
    "kamera": [432, 616, 624, 776], "liste": [688, 520, 992, 992]}
def atlas():
    W = 1024
    im, d = tuval(W, W)
    def kose(b, kal, renk, boy):
        x0, y0, x1, y1 = b
        for (x, y, dx, dy) in [(x0, y0, 1, 1), (x1, y0, -1, 1), (x0, y1, 1, -1), (x1, y1, -1, -1)]:
            d.line(S(x, y, x + dx * boy, y), fill=a(renk), width=kal * SS)
            d.line(S(x, y, x, y + dy * boy), fill=a(renk), width=kal * SS)
    b = ATLAS["anaNisan"]; kose([b[0] + 12, b[1] + 12, b[2] - 12, b[3] - 12], 26, PAS, 110)
    b = ATLAS["yanNisan"]; kose([b[0] + 10, b[1] + 10, b[2] - 10, b[3] - 10], 12, KIRLI_BEYAZ, 80)
    x0, y0, x1, y1 = ATLAS["yonOku"]   # sağ üste bakan ok
    d.polygon(S(x1 - 10, y0 + 10, x1 - 10, y0 + 130, x1 - 130, y0 + 10), fill=a(PAS))
    d.line(S(x0 + 30, y1 - 30, x1 - 60, y0 + 60), fill=a(PAS), width=34 * SS)
    x0, y0, x1, y1 = ATLAS["kamera"]
    d.rounded_rectangle(S(x0 + 10, y0 + 40, x1 - 10, y1 - 10), radius=12 * SS, fill=a(KOYU))
    d.rectangle(S(x0 + 30, y0 + 15, x0 + 80, y0 + 45), fill=a(KOYU))
    d.ellipse(S((x0 + x1) / 2 - 42, y0 + 55, (x0 + x1) / 2 + 42, y0 + 139), fill=a(PIRINC))
    d.ellipse(S((x0 + x1) / 2 - 22, y0 + 75, (x0 + x1) / 2 + 22, y0 + 119), fill=a(KOYU))
    x0, y0, x1, y1 = ATLAS["liste"]
    d.rectangle(S(x0 + 8, y0 + 30, x1 - 8, y1 - 8), fill=a(KIRLI_BEYAZ))
    d.rectangle(S(x0 + 30, y0 + 22, x0 + 90, y0 + 50), fill=(0x9A, 0x7B, 0x4F, 200))   # bant
    d.rectangle(S(x1 - 90, y0 + 22, x1 - 30, y0 + 50), fill=(0x9A, 0x7B, 0x4F, 200))
    d.rounded_rectangle(S((x0 + x1) / 2 - 50, y0, (x0 + x1) / 2 + 50, y0 + 42), radius=10 * SS, fill=a(PIRINC))
    for i in range(3):
        y = y0 + 150 + i * 100
        d.line(S(x0 + 40, y, x1 - 40, y), fill=a(TOPRAK), width=4 * SS)
    kaydet(im, "hedef-isaretleri.png", W, W)

# 08 · Düğme seti 768x1024: 4 durum (boşta, üstünde, basılı, pasif) tek sütunda + kare ikon düğmesi
DUGME = {"plakalar": [[84, 40 + i * 180, 684, 190 + i * 180] for i in range(4)],
         "kare": [[84 + i * 160, 790, 224 + i * 160, 930] for i in range(4)]}
def dugmeler():
    W, H = 768, 1024
    im, d = tuval(W, H)
    stiller = [(KIRLI_BEYAZ, None, 1.0), (tuple(min(255, int(v * 1.08)) for v in KIRLI_BEYAZ), PIRINC, 1.0),
               (koyulas(KIRLI_BEYAZ, .78), None, .8), ((0xBD, 0xB9, 0xAE), None, .6)]
    for kutular in (DUGME["plakalar"], DUGME["kare"]):
        for (b, (renk, kenar, pf)) in zip(kutular, stiller):
            d.rounded_rectangle(S(*b), radius=18 * SS, fill=a(renk), outline=a(kenar) if kenar else None, width=6 * SS)
            if pf < .7: continue
            for (x, y) in [(b[0] + 22, (b[1] + b[3]) / 2 - 18), (b[0] + 22, (b[1] + b[3]) / 2 + 18),
                           (b[2] - 22, (b[1] + b[3]) / 2 - 18), (b[2] - 22, (b[1] + b[3]) / 2 + 18)]:
                percin(d, x, y, 6)
    kaydet(im, "dugme-seti.png", W, H)

# 09 · Görev sonu puan tablosu 1024x1280: 6 satır, 2 sütun, son satır (toplam) üstünde kalın çizgi
TABLO = {"satirlar": [[170, 330 + i * 120, 900, 450 + i * 120] for i in range(6)], "bolme": 680}
def tablo():
    W, H = 1024, 1280
    im, d = tuval(W, H)
    d.rounded_rectangle(S(40, 40, W - 40, H - 30), radius=30 * SS, fill=a(koyulas(TOPRAK, .8)))
    d.rectangle(S(100, 120, W - 100, H - 90), fill=a(KIRLI_BEYAZ))
    d.rounded_rectangle(S(W / 2 - 120, 70, W / 2 + 120, 150), radius=14 * SS, fill=a(PIRINC))
    for y in range(250, H - 150, 140): d.ellipse(S(122, y, 146, y + 24), fill=a(koyulas(TOPRAK, .8)))
    x0, x1, bol = 170, 900, TABLO["bolme"]
    ys = [330 + i * 120 for i in range(7)]
    for i, y in enumerate(ys):
        d.line(S(x0, y, x1, y), fill=a(KOYU), width=(6 if i == 5 else 2) * SS)
    d.line(S(bol, ys[0], bol, ys[-1]), fill=a(KOYU), width=2 * SS)
    d.ellipse(S(760, 170, 890, 300), outline=a(PAS), width=8 * SS)
    d.ellipse(S(220, 1010, 360, 1150), outline=(0x9A, 0x7B, 0x4F, 90), width=10 * SS)   # kahve lekesi
    kaydet(im, "puan-tablosu.png", W, H)

# 10 · Ana menü arka planı 1920x1080, OPAK: şafakta kıyı pisti, iç deniz, burunda deniz feneri
def menu():
    W, H = 1920, 1080
    im = Image.new("RGBA", (W * SS, H * SS), a(KIRLI_BEYAZ)); d = ImageDraw.Draw(im)
    for i in range(12):   # gökyüzü katmanları
        y = i * 45
        renk = tuple(int(KIRLI_BEYAZ[k] * (1 - i / 30) + TOPRAK[k] * (i / 30)) for k in range(3))
        d.rectangle(S(0, y, W, y + 45), fill=a(renk))
    deniz = (0x7E, 0x85, 0x6A)
    d.rectangle(S(0, 540, W, H), fill=a(deniz))
    d.polygon(S(1400, 560, 1920, 520, 1920, 640, 1500, 610), fill=a(koyulas(TOPRAK, .8)))   # kayalık burun
    d.rectangle(S(1700, 360, 1726, 540), fill=a(KIRLI_BEYAZ)); d.rectangle(S(1700, 400, 1726, 420), fill=a(PAS))
    d.polygon(S(1694, 360, 1732, 360, 1713, 335), fill=a(PAS))
    d.polygon(S(0, 700, 1920, 640, 1920, H, 0, H), fill=a(ZEYTIN))                            # kıyı
    d.polygon(S(500, H, 900, H, 1180, 690, 1080, 690), fill=a(TOPRAK))                       # pist
    d.rectangle(S(1260, 600, 1360, 680), fill=a(koyulas(ZEYTIN, .7)))                        # kulübe
    d.line(S(1420, 690, 1420, 580), fill=a(KOYU), width=4 * SS)
    d.polygon(S(1420, 580, 1480, 590, 1420, 605), fill=a(PAS))                                # rüzgâr tulumu
    d.polygon(S(980, 760, 1120, 760, 1050, 740), fill=a(KOYU)); d.rectangle(S(960, 735, 1140, 742), fill=a(KOYU))
    d.rectangle(S(975, 755, 1125, 761), fill=a(KOYU))                                         # çift kanatlı silüeti
    sis = Image.new("RGBA", im.size, (0, 0, 0, 0)); sd = ImageDraw.Draw(sis)
    sd.rectangle(S(0, 480, W, 620), fill=a(KIRLI_BEYAZ, 110))
    im = Image.alpha_composite(im, sis.filter(ImageFilter.GaussianBlur(30 * SS)))
    im.convert("RGB").resize((W, H), Image.LANCZOS).save(os.path.join(OUT, "menu-arkaplan.png"))

for f in (cerceve, ibre, gaz_plaka, topuz, pusula, atlas, dugmeler, tablo, menu): f()
kadran("gosterge-hiz.png", False); kadran("gosterge-yukseklik.png", True)
json.dump({"gaz": GAZ, "pusula": PUSULA, "atlas": ATLAS, "dugme": DUGME, "tablo": TABLO},
          open(os.path.join(OUT, "_olculer.json"), "w"), indent=1)
print("10 görsel yazıldı:", sorted(f for f in os.listdir(OUT) if f.endswith(".png")))
