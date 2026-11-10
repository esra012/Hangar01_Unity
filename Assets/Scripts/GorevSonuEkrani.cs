using UnityEngine;

// Görev bitince puan dökümünü gösterir. Geçici çözüm: OnGUI (12. haftada düzgün arayüze çevrilecek).
// Görsel varsa puan tablosu kâğıdına ve metal düğmeye çizer; yoksa düz kutuya.
public class GorevSonuEkrani : MonoBehaviour
{
    [SerializeField] private GorevYoneticisi gorev;
    [SerializeField] private PuanCuzdani cuzdan;
    [SerializeField] private Texture2D puanTablosu;   // 1024x1280, 6 satır
    [SerializeField] private Texture2D dugmeSeti;     // 768x1024, 4 durum

    private bool gorunur;
    private int toplam;

    // puan-tablosu.png'de ÖLÇÜLEN satırlar (piksel) ve sütun bölmesi
    const float TabloG = 1024f, TabloY = 1280f, SatirUst = 330f, SatirBoy = 120f, SolX = 170f, BolmeX = 680f, SagX = 900f;
    // dugme-seti.png'de ÖLÇÜLEN plakalar (piksel, sol üst kökenli): boşta, üstünde, basılı
    static readonly Rect[] Plakalar = { Rect.MinMaxRect(84, 40, 684, 190), Rect.MinMaxRect(84, 220, 684, 370), Rect.MinMaxRect(84, 400, 684, 550) };

    private void OnEnable()  { if (gorev != null) gorev.GorevBitti += Goster; }
    private void OnDisable() { if (gorev != null) gorev.GorevBitti -= Goster; }

    private void Goster(int puan)
    {
        toplam = puan;
        gorunur = true;
    }

    private void OnGUI()
    {
        if (!gorunur) return;

        float s = Screen.height / 1080f;
        Matrix4x4 eski = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(s, s, 1f));
        float w = Screen.width / s;

        Color c = GUI.color; GUI.color = new Color(0f, 0f, 0f, .55f);
        GUI.DrawTexture(new Rect(0, 0, w, 1080), Texture2D.whiteTexture);
        GUI.color = c;

        var baslik = new GUIStyle(GUI.skin.label) { fontSize = 40, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        var hucre = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleLeft };
        Color koyu = new Color32(0x2B, 0x2A, 0x26, 255);
        baslik.normal.textColor = koyu; hucre.normal.textColor = koyu;

        // Kâğıt: 1024x1280 -> 640x800
        float k = 640f / TabloG;
        Rect kagit = new Rect(w / 2f - 320f, 90f, 640f, TabloY * k);
        if (puanTablosu != null) GUI.DrawTexture(kagit, puanTablosu);
        else GUI.Box(kagit, "");

        GUI.Label(new Rect(kagit.x, kagit.y + 150f * k, kagit.width, 60f), "SORTIE TAMAM", baslik);

        // 6 satır: döküm kalemleri + son satırda toplam
        var dokum = cuzdan != null ? cuzdan.Dokum : null;
        for (int i = 0; i < 6; i++)
        {
            string ad = "", deger = "";
            if (i == 5) { ad = "TOPLAM"; deger = toplam.ToString(); hucre.fontStyle = FontStyle.Bold; }
            else if (dokum != null && i < dokum.Count) { ad = dokum[i].Key; deger = "+" + dokum[i].Value; }
            float y = kagit.y + (SatirUst + i * SatirBoy) * k;
            GUI.Label(new Rect(kagit.x + (SolX + 14f) * k, y, (BolmeX - SolX) * k, SatirBoy * k), ad, hucre);
            var sag = new GUIStyle(hucre) { alignment = TextAnchor.MiddleRight };
            GUI.Label(new Rect(kagit.x + BolmeX * k, y, (SagX - BolmeX - 14f) * k, SatirBoy * k), deger, sag);
        }

        if (Dugme(new Rect(w / 2f - 160f, kagit.yMax + 16f, 320f, 80f), "YENİ SORTIE"))
        {
            gorunur = false;
            // Geçici: sahneyi baştan yükle
            var sahne = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.SceneManager.LoadScene(sahne.name);
        }
        GUI.matrix = eski;
    }

    // Metal plaka düğme: boşta / üstünde / basılı durumları atlastan kesilir
    private bool Dugme(Rect r, string metin)
    {
        if (dugmeSeti == null) return GUI.Button(r, metin);

        Vector2 fare = Event.current.mousePosition;
        bool ustunde = r.Contains(fare);
        bool basili = ustunde && Input.GetMouseButton(0);
        Rect p = Plakalar[basili ? 2 : ustunde ? 1 : 0];
        GUI.DrawTextureWithTexCoords(r, dugmeSeti, new Rect(p.xMin / 768f, 1f - p.yMax / 1024f, p.width / 768f, p.height / 1024f));

        var s = new GUIStyle(GUI.skin.label) { fontSize = 28, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        s.normal.textColor = new Color32(0x2B, 0x2A, 0x26, 255);
        GUI.Label(r, metin, s);
        return GUI.Button(r, GUIContent.none, GUIStyle.none);
    }
}
