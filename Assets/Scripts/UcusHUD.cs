using System.Collections.Generic;
using UnityEngine;

// Uçuş arayüzü (OnGUI): çerçeve, hız/yükseklik kadranları, gaz kolu, kayan pusula,
// hedef nişanları + ekran dışı yön oku, sortie panosu, uçuşan "+100", banner ve kalkış uyarısı.
// Her şey 1920x1080 referansında çizilir ve ekran yüksekliğine göre ölçeklenir.
public class UcusHUD : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private UcusKontrol ucak;
    [SerializeField] private GorevYoneticisi gorev;
    [SerializeField] private PuanCuzdani cuzdan;

    [Header("Görseller (Assets/UI)")]
    [SerializeField] private Texture2D cerceve;
    [SerializeField] private Texture2D hizKadrani;
    [SerializeField] private Texture2D yukseklikKadrani;
    [SerializeField] private Texture2D ibre;
    [SerializeField] private Texture2D gazPlaka;
    [SerializeField] private Texture2D gazTopuz;
    [SerializeField] private Texture2D pusulaSerit;     // Wrap Mode = Repeat!
    [SerializeField] private Texture2D isaretler;       // atlas

    [Header("Kadranlar")]
    [SerializeField] private float ibreBaslangic = -140f;   // sıfır değerdeki açı
    [SerializeField] private float ibreBitis     =  140f;   // tam değerdeki açı
    [SerializeField] private float enCokYukseklik = 300f;

    // Atlas kutuları: görsel dosyasında piksel olarak ÖLÇÜLDÜ (sol üst kökenli, 0-1)
    static readonly Rect AnaNisan = Rect.MinMaxRect(64 / 1024f, 64 / 1024f, 448 / 1024f, 448 / 1024f);
    static readonly Rect YanNisan = Rect.MinMaxRect(576 / 1024f, 96 / 1024f, 896 / 1024f, 416 / 1024f);
    static readonly Rect YonOku   = Rect.MinMaxRect(96 / 1024f, 576 / 1024f, 352 / 1024f, 832 / 1024f);
    static readonly Rect Kamera   = Rect.MinMaxRect(432 / 1024f, 616 / 1024f, 624 / 1024f, 776 / 1024f);
    static readonly Rect Liste    = Rect.MinMaxRect(688 / 1024f, 520 / 1024f, 992 / 1024f, 992 / 1024f);

    // Pusula şeridi: bandın kaynak satırları ve dizin üçgeni (ölçülen), şerit genişliği = 180°
    const float BantUst = 52 / 128f, BantAlt = 120 / 128f;
    static readonly Rect Ucgen = Rect.MinMaxRect(728 / 1536f, 0f, 808 / 1536f, 46 / 128f);
    // Gaz plakası (256x1024): kanal ve hız şeridi
    static readonly Rect GazKanal = Rect.MinMaxRect(40 / 256f, 80 / 1024f, 100 / 256f, 944 / 1024f);
    static readonly Rect GazSerit = Rect.MinMaxRect(150 / 256f, 80 / 1024f, 210 / 256f, 944 / 1024f);

    static readonly Color KirliBeyaz = new Color32(0xE8, 0xE2, 0xD0, 255);
    static readonly Color Koyu       = new Color32(0x2B, 0x2A, 0x26, 255);
    static readonly Color Pas        = new Color32(0x9E, 0x4B, 0x38, 255);
    static readonly Color Pirinc     = new Color32(0xB0, 0x8D, 0x57, 255);
    static readonly Color Zeytin     = new Color32(0x6B, 0x72, 0x47, 255);

    private class UcusanYazi { public string metin; public float t0; public Vector2 konum; }
    private readonly List<UcusanYazi> yazilar = new List<UcusanYazi>();
    private string banner, bannerAlt;
    private float olcek = 1f, genislik = 1920f;
    private GUIStyle stil;
    private Camera kam;

    private void OnEnable()
    {
        if (cuzdan != null) cuzdan.PuanEklendi += PuanEklendi;
        UcusKontrol.Carpisti += Cakildi;
        GorevYoneticisi.SortieBasarisiz += SureDoldu;
    }

    private void OnDisable()
    {
        if (cuzdan != null) cuzdan.PuanEklendi -= PuanEklendi;
        UcusKontrol.Carpisti -= Cakildi;
        GorevYoneticisi.SortieBasarisiz -= SureDoldu;
    }

    // Toplamdan fark hesaplamıyoruz: aynı karede iki kazanç gelirse fark yanlış çıkar
    private void PuanEklendi(int miktar, string kaynak) =>
        yazilar.Add(new UcusanYazi { metin = "+" + miktar, t0 = Time.time, konum = new Vector2(genislik / 2f, 470f) });

    private void Cakildi(string neden) { banner = "ÇAKILDIN"; bannerAlt = neden; }
    private void SureDoldu(string neden) { banner = "SÜRE DOLDU"; bannerAlt = "Sortie başarısız"; }

    private void OnGUI()
    {
        if (ucak == null) return;
        if (kam == null) kam = Camera.main;
        if (stil == null) stil = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

        olcek = Screen.height / 1080f;
        genislik = Screen.width / olcek;
        Matrix4x4 eskiMatris = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(olcek, olcek, 1f));

        HedefNisanlari();
        if (cerceve != null) GUI.DrawTexture(new Rect((genislik - 1920f) / 2f, 0f, 1920f, 1080f), cerceve);

        // 5 · Hız kadranı (sol yuvarlak yuva) · 7 · Yükseklik kadranı (sağ pano)
        float sol = (genislik - 1920f) / 2f;
        KadranCiz(new Rect(sol + 45f, 740f, 310f, 310f), hizKadrani, ucak.Hiz / ucak.EnCokHiz);
        Yaz(new Rect(sol + 140f, 965f, 120f, 30f), Mathf.RoundToInt(ucak.Hiz) + " m/s", 22, Koyu);
        KadranCiz(new Rect(sol + 1565f, 750f, 290f, 290f), yukseklikKadrani, ucak.Yukseklik / enCokYukseklik);
        Yaz(new Rect(sol + 1650f, 965f, 120f, 30f), Mathf.RoundToInt(Mathf.Max(0f, ucak.Yukseklik)) + " m", 22, Koyu);

        GazKolu(new Rect(genislik / 2f - 37.5f, 765f, 75f, 300f));
        Pusula(new Rect(genislik / 2f - 450f, 12f, 900f, 75f), ucak.transform.eulerAngles.y);
        Pano();
        UcusanYazilar();
        Uyarilar();

        GUI.matrix = eskiMatris;
    }

    // ---------------------------------------------------------------- kadran + dönen ibre
    private void KadranCiz(Rect yuva, Texture2D yuz, float oran)
    {
        if (yuz != null) GUI.DrawTexture(yuva, yuz, ScaleMode.ScaleToFit);
        if (ibre == null) return;

        float aci = Mathf.Lerp(ibreBaslangic, ibreBitis, Mathf.Clamp01(oran));
        Matrix4x4 eski = GUI.matrix;                       // mevcut dönüşümü SAKLA
        // RotateAroundPivot ölçeklenmiş matrisin ÜSTÜNE uygulanır: pivotu ekran pikseline çeviriyoruz
        GUIUtility.RotateAroundPivot(aci, yuva.center * olcek);

        float b = yuva.width * 0.63f;                      // ibre kadrandan küçük
        GUI.DrawTexture(new Rect(yuva.center.x - b / 2f, yuva.center.y - b / 2f, b, b), ibre, ScaleMode.ScaleToFit);
        GUI.matrix = eski;                                 // GERİ YÜKLE
    }

    // ---------------------------------------------------------------- 6 · gaz kolu
    private void GazKolu(Rect r)
    {
        if (gazPlaka != null) GUI.DrawTexture(r, gazPlaka);

        // Sağdaki şerit hızla dolar (kırmızı çizgi = kalkış hızı)
        Rect s = Alt(r, GazSerit);
        float dolu = Mathf.Clamp01(ucak.Hiz / ucak.EnCokHiz) * s.height;
        Renkli(new Rect(s.x, s.yMax - dolu, s.width, dolu),
               ucak.Hiz >= ucak.KalkisHizi ? new Color(Pas.r, Pas.g, Pas.b, .55f) : new Color(Zeytin.r, Zeytin.g, Zeytin.b, .6f));

        // Soldaki kanalda topuz gazla kayar
        if (gazTopuz == null) return;
        Rect k = Alt(r, GazKanal);
        float y = Mathf.Lerp(k.yMax - 14f, k.yMin + 14f, ucak.GazOrani);
        float boy = 58f;
        GUI.DrawTexture(new Rect(k.center.x - boy / 2f, y - boy / 2f, boy, boy), gazTopuz, ScaleMode.ScaleToFit);
    }

    // ---------------------------------------------------------------- 2 · kayan pusula
    private void Pusula(Rect r, float rota)
    {
        if (pusulaSerit == null) return;
        // Şeridi kopyalamıyoruz, doku koordinatını KAYDIRIYORUZ. Ortadaki üçgen = rota.
        float u0 = (rota - 90f) / 180f;
        Rect bant = new Rect(r.x, r.y + r.height * 0.35f, r.width, r.height * 0.65f);
        GUI.DrawTextureWithTexCoords(bant, pusulaSerit, new Rect(u0, 1f - BantAlt, 1f, BantAlt - BantUst));
        Ciz(new Rect(r.center.x - 18f, r.y, 36f, r.height * 0.4f), pusulaSerit, Ucgen);

        // Boş plakalara yön adları (her 30°)
        int ilk = Mathf.CeilToInt((rota - 90f) / 30f) * 30;
        for (int d = ilk; d <= rota + 90f; d += 30)
        {
            float x = r.x + (d - (rota - 90f)) / 180f * r.width;
            int n = ((d % 360) + 360) % 360;
            string ad = n == 0 ? "K" : n == 90 ? "D" : n == 180 ? "G" : n == 270 ? "B" : (n / 10).ToString();
            Yaz(new Rect(x - 20f, bant.y + bant.height * 0.42f, 40f, bant.height * 0.6f), ad, 15, Koyu);
        }
        Yaz(new Rect(r.center.x - 60f, r.yMax + 2f, 120f, 26f), Mathf.RoundToInt(rota).ToString("000") + "°", 18, KirliBeyaz, true);
    }

    // ---------------------------------------------------------------- 3 · 4 · hedef nişanları ve yön oku
    private void HedefNisanlari()
    {
        if (kam == null || isaretler == null) return;
        foreach (var h in KesifHedefi.Etkinler)
            Nisan(h.transform.position + Vector3.up * 25f, h.YanHedef ? YanNisan : AnaNisan, h.GorunenAd, h.YanHedef);

        if (gorev != null && gorev.DonusFazi && gorev.Us != null)
            Nisan(gorev.Us.position + Vector3.up * 10f, AnaNisan, "ÜS", false);
    }

    private void Nisan(Vector3 dunya, Rect kutu, string ad, bool yan)
    {
        Vector3 e = kam.WorldToScreenPoint(dunya);
        float mesafe = Vector3.Distance(ucak.transform.position, dunya);
        Vector2 p = new Vector2(e.x / olcek, (Screen.height - e.y) / olcek);   // GUI: y aşağı
        bool onde = e.z > 0f;
        float pay = 70f;

        if (onde && p.x > pay && p.x < genislik - pay && p.y > 110f && p.y < 720f)
        {
            float boy = Mathf.Clamp(9000f / Mathf.Max(mesafe, 1f), yan ? 34f : 46f, yan ? 70f : 110f);
            Ciz(new Rect(p.x - boy / 2f, p.y - boy / 2f, boy, boy), isaretler, kutu);
            Yaz(new Rect(p.x - 100f, p.y + boy / 2f, 200f, 24f), ad + " · " + Mathf.RoundToInt(mesafe) + " m", yan ? 14 : 17,
                yan ? KirliBeyaz : new Color(1f, .92f, .85f), true);
            return;
        }

        // Ekran dışı: kenarda yön oku
        if (yan) return;
        Vector2 merkez = new Vector2(genislik / 2f, 420f);
        Vector2 yon = onde ? (p - merkez) : (merkez - p);
        if (yon.sqrMagnitude < 1f) yon = Vector2.up;
        yon.Normalize();
        float t = Mathf.Min((genislik / 2f - pay) / Mathf.Max(Mathf.Abs(yon.x), 0.001f), (330f) / Mathf.Max(Mathf.Abs(yon.y), 0.001f));
        Vector2 q = merkez + yon * t;
        float aci = Mathf.Atan2(yon.y, yon.x) * Mathf.Rad2Deg + 45f;   // atlastaki ok sağ üste bakıyor
        Matrix4x4 eski = GUI.matrix;
        GUIUtility.RotateAroundPivot(aci, q * olcek);
        Ciz(new Rect(q.x - 26f, q.y - 26f, 52f, 52f), isaretler, YonOku);
        GUI.matrix = eski;
        Yaz(new Rect(q.x - 80f, q.y + 26f, 160f, 22f), ad + " " + Mathf.RoundToInt(mesafe) + " m", 14, KirliBeyaz, true);
    }

    // ---------------------------------------------------------------- 1 · sortie panosu
    private void Pano()
    {
        Rect r = new Rect(20f, 20f, 270f, 300f);
        if (isaretler != null) Ciz(r, isaretler, Liste);
        if (gorev == null) return;

        Yaz(new Rect(r.x, r.y + 44f, r.width, 30f), "SORTIE", 22, Koyu);
        int alinan = gorev.ToplamAna - gorev.KalanAna;
        string sure = gorev.SureSiniri > 0f ? Mathf.FloorToInt(gorev.KalanSure / 60f) + ":" + Mathf.FloorToInt(gorev.KalanSure % 60f).ToString("00") : "--:--";
        Satir(r, 0, "Hedef", alinan + " / " + gorev.ToplamAna, Koyu);
        Satir(r, 1, "Yan hedef", gorev.AlinanYan + " / " + gorev.ToplamYan, Koyu);
        Satir(r, 2, "Puan", cuzdan != null ? cuzdan.Puan.ToString() : "0", Koyu);
        Satir(r, 3, "Süre", sure, gorev.SureSiniri > 0f && gorev.KalanSure <= 10f ? Pas : Koyu);   // son 10 sn kırmızı
        if (isaretler != null) Ciz(new Rect(r.xMax - 58f, r.y + 46f, 34f, 28f), isaretler, Kamera);
    }

    private void Satir(Rect r, int i, string ad, string deger, Color renk)
    {
        float y = r.y + 92f + i * 46f;
        var s = Stil(18, renk); s.alignment = TextAnchor.MiddleLeft;
        GUI.Label(new Rect(r.x + 34f, y, 130f, 36f), ad, s);
        s.alignment = TextAnchor.MiddleRight;
        GUI.Label(new Rect(r.x + 120f, y, 118f, 36f), deger, s);
    }

    // ---------------------------------------------------------------- uçuşan "+100": başta hızlı, sonra yavaş
    private void UcusanYazilar()
    {
        for (int i = yazilar.Count - 1; i >= 0; i--)
        {
            float t = (Time.time - yazilar[i].t0) / 1.4f;
            if (t >= 1f) { yazilar.RemoveAt(i); continue; }
            float kolay = 1f - Mathf.Pow(1f - t, 3f);              // ease-out cubic
            Vector2 p = yazilar[i].konum + Vector2.up * (-90f * kolay);
            Color c = new Color(1f, .95f, .8f, 1f - t * t);
            Yaz(new Rect(p.x - 100f, p.y - 30f, 200f, 60f), yazilar[i].metin, 44, c, true);
        }
    }

    // ---------------------------------------------------------------- 8 · banner, kalkış uyarısı, kumanda şeması
    private void Uyarilar()
    {
        if (!string.IsNullOrEmpty(banner))
        {
            Renkli(new Rect(0f, 380f, genislik, 150f), new Color(Koyu.r, Koyu.g, Koyu.b, .75f));
            Yaz(new Rect(0f, 390f, genislik, 90f), banner, 72, Pas, true);
            Yaz(new Rect(0f, 470f, genislik, 40f), bannerAlt, 26, KirliBeyaz);
            return;
        }

        if (gorev != null && gorev.DonusFazi)
            Yaz(new Rect(0f, 118f, genislik, 36f), "TÜM HEDEFLER FOTOĞRAFLANDI — ÜSSE DÖN", 24, new Color(1f, .9f, .75f), true);

        if (ucak.Durum == UcusKontrol.Faz.Yerde && (gorev == null || !gorev.GorevTamam))
        {
            string m = ucak.Hiz < ucak.KalkisHizi
                ? "KALKIŞ: ↑ ile gaz ver · hız " + Mathf.RoundToInt(ucak.KalkisHizi) + " m/s'yi geçince S ile burnu kaldır"
                : "KALKIŞA HAZIR — S ile burnu kaldır";
            Yaz(new Rect(0f, 690f, genislik, 34f), m, 22, KirliBeyaz, true);
            Yaz(new Rect(genislik - 520f, 20f, 500f, 120f),
                "W / S  yunuslama\nA / D  yatış\nQ / E  sapma\n↑ / ↓  gaz", 17, KirliBeyaz, true);
        }
    }

    // ---------------------------------------------------------------- yardımcılar
    // Doku koordinatı sol ALT kökenli: y'yi çeviriyoruz
    private static void Ciz(Rect ekran, Texture2D sayfa, Rect kutu) =>
        GUI.DrawTextureWithTexCoords(ekran, sayfa, new Rect(kutu.xMin, 1f - kutu.yMax, kutu.width, kutu.height));

    private static Rect Alt(Rect r, Rect oran) =>
        new Rect(r.x + oran.xMin * r.width, r.y + oran.yMin * r.height, oran.width * r.width, oran.height * r.height);

    private static void Renkli(Rect r, Color c)
    {
        Color eski = GUI.color; GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = eski;
    }

    private GUIStyle Stil(int boyut, Color renk)
    {
        var s = new GUIStyle(stil) { fontSize = boyut };
        s.normal.textColor = renk;
        return s;
    }

    private void Yaz(Rect r, string metin, int boyut, Color renk, bool golge = false)
    {
        if (golge)
        {
            var g = Stil(boyut, new Color(0f, 0f, 0f, renk.a * .7f));
            GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), metin, g);
        }
        GUI.Label(r, metin, Stil(boyut, renk));
    }
}
