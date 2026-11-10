#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

// Hafta 3 · Pilot Koltuğu
//   KomutSatirindanKur : Rota02'yi Rota03 olarak kaydeder, bileşenleri bağlar, deney sahnesini kurar,
//                        H01 pervane görüntüsünü render eder.  (-batchmode)
//   TeslimCek          : Editör açıkken deltaTime deneyini çalıştırır, H02 Gizmos ve H03 takip
//                        kamerası görüntülerini çeker, sonunda Rota03'ü Play'de bırakır.
public static class Rota03Kurulum
{
    private const string Rota02 = "Assets/Scenes/Rota02.unity";
    private const string Rota03 = "Assets/Scenes/Rota03.unity";
    private const string DeneySahnesi = "Assets/Scenes/DeltaTimeDeneyi.unity";
    private const string Girdi = "Assets/Input/Ucus.inputactions";
    private const string TeslimKlasoru = "Teslim";

    [MenuItem("Hangar/3 · Rota03 Sahnesini Kur")]
    private static void MenudenKur()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Kur();
        EditorUtility.DisplayDialog("Rota03", "Rota03 sahnesi hazır. Play'e bas: W/S yunuslama, A/D yatış, Q/E sapma, ↑/↓ gaz, H hasar testi.", "Tamam");
    }

    [MenuItem("Hangar/4 · Teslim Görüntülerini Çek")]
    public static void TeslimCek() => Orkestra.Baslat();

    public static void KomutSatirindanKur()
    {
        Kur();
        Debug.Log("ROTA03_TAMAM");
    }

    // ---------------------------------------------------------------- kurulum
    private static void Kur()
    {
        Directory.CreateDirectory(TeslimKlasoru);
        AssetDatabase.Refresh();

        // Başlangıç: Rota02'yi aç, Rota03 olarak kaydet (Rota02 bozulmaz)
        var sahne = EditorSceneManager.OpenScene(Rota02, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(sahne, Rota03);
        sahne = EditorSceneManager.OpenScene(Rota03, OpenSceneMode.Single);

        GameObject ucak = UcagiBul();
        ucak.transform.SetPositionAndRotation(new Vector3(0f, 20f, -40f), Quaternion.identity);

        // HedefeGit'i kapat (kaldırma)
        var hedefeGit = ucak.GetComponent<HedefeGit>();
        if (hedefeGit != null) hedefeGit.enabled = false;

        // 02 · PlayerInput + girdi tablosu
        var girdi = AssetDatabase.LoadAssetAtPath<InputActionAsset>(Girdi);
        var pi = Bilesen<PlayerInput>(ucak);
        pi.actions = girdi;
        pi.defaultActionMap = "Ucus";
        pi.notificationBehavior = PlayerNotifications.SendMessages;

        // 03 · Uçuş, 04 · Sağlık + test
        Bilesen<UcusKontrol>(ucak);
        Bilesen<Saglik>(ucak);
        Bilesen<HasarTesti>(ucak);

        // 04 · Ayrı dinleyici nesne
        GameObject arayuz = GameObject.Find("Arayuz");
        if (arayuz == null) arayuz = new GameObject("Arayuz");
        var canYazisi = Bilesen<CanYazisi>(arayuz);
        Ata(canYazisi, "saglik", ucak.GetComponent<Saglik>());

        // 05 · Takip kamerası
        Camera kamera = Camera.main;
        var takip = Bilesen<TakipKamerasi>(kamera.gameObject);
        Ata(takip, "hedef", ucak.transform);
        kamera.transform.position = ucak.transform.TransformPoint(new Vector3(0f, 3.2f, -11f));
        kamera.transform.LookAt(ucak.transform.position + Vector3.up * 1.2f);
        kamera.fieldOfView = 60f;

        EditorSceneManager.MarkSceneDirty(sahne);
        EditorSceneManager.SaveScene(sahne);

        // H01 · pervane görüntüsü (yakın çekim, ayrı geçici kamera)
        var gecici = new GameObject("GeciciKamera").AddComponent<Camera>();
        gecici.transform.position = ucak.transform.TransformPoint(new Vector3(5.5f, 1.2f, 8f));
        gecici.transform.LookAt(ucak.transform.TransformPoint(new Vector3(0f, 1.1f, 0.5f)));
        gecici.fieldOfView = 50f;
        Render(gecici, Path.Combine(TeslimKlasoru, "H01_Nu_D36_pervane.png"));
        Object.DestroyImmediate(gecici.gameObject);
        EditorSceneManager.SaveScene(sahne);

        // 01 · deltaTime deney sahnesi
        var deney = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var zemin = GameObject.CreatePrimitive(PrimitiveType.Plane);
        zemin.transform.position = new Vector3(0f, 0f, 25f);
        zemin.transform.localScale = new Vector3(2f, 1f, 7f);
        var kup = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kup.name = "DeneyKupu";
        kup.transform.position = new Vector3(0f, 0.5f, 0f);
        kup.AddComponent<DeltaTimeDeneyi>();
        Camera.main.transform.SetPositionAndRotation(new Vector3(-30f, 20f, 25f), Quaternion.Euler(30f, 90f, 0f));
        EditorSceneManager.SaveScene(deney, DeneySahnesi);

        EditorSceneManager.OpenScene(Rota03, OpenSceneMode.Single);
    }

    // ---------------------------------------------------------------- Play modu otomasyonu
    private static class Orkestra
    {
        private enum Adim { DeneyAc, DeneyBekle, Rota02Ac, Rota02Bekle, GizmoCek, Rota03Ac, Rota03Bekle, Bitti }
        private static Adim adim;
        private static double zaman;
        private static bool eskiAyar;
        private static EnterPlayModeOptions eskiSecenek;

        public static void Baslat()
        {
            Directory.CreateDirectory(TeslimKlasoru);
            // Sahne geçişlerinde betik yeniden yüklenmesin (orkestranın durumu kaybolmasın)
            eskiAyar = EditorSettings.enterPlayModeOptionsEnabled;
            eskiSecenek = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            adim = Adim.DeneyAc;
            EditorApplication.update -= Guncelle;
            EditorApplication.update += Guncelle;
        }

        private static double Gecen => EditorApplication.timeSinceStartup - zaman;
        private static void Gec(Adim yeni) { adim = yeni; zaman = EditorApplication.timeSinceStartup; }
        private static bool Durgun => !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode;

        private static void Guncelle()
        {
            switch (adim)
            {
                case Adim.DeneyAc:
                    if (!Durgun) { EditorApplication.isPlaying = false; return; }
                    EditorSceneManager.OpenScene(DeneySahnesi);
                    EditorApplication.EnterPlaymode();
                    Gec(Adim.DeneyBekle);
                    break;

                case Adim.DeneyBekle:
                    var deney = Object.FindFirstObjectByType<DeltaTimeDeneyi>();
                    if ((deney != null && deney.Tamamlandi) || Gecen > 90)
                    {
                        EditorApplication.ExitPlaymode();
                        Gec(Adim.Rota02Ac);
                    }
                    break;

                case Adim.Rota02Ac:
                    if (!Durgun) return;
                    EditorSceneManager.OpenScene(Rota02);
                    EditorApplication.EnterPlaymode();
                    Gec(Adim.Rota02Bekle);
                    break;

                case Adim.Rota02Bekle:
                    if (Gecen < 9) return;   // uçak balona yaklaşsın
                    // Föy 07: uçaksavarı Y'de döndürüp hedefi koniye al -> çizgi yeşil
                    var ucaksavar = GameObject.Find("Uçaksavar");
                    var ucak = GameObject.Find("Nu_D36");
                    if (ucaksavar != null && ucak != null)
                    {
                        Vector3 yatay = ucak.transform.position - ucaksavar.transform.position;
                        yatay.y = 0f;
                        ucaksavar.transform.rotation = Quaternion.LookRotation(yatay.normalized);
                    }
                    var sv = SceneView.lastActiveSceneView;
                    if (sv == null) sv = EditorWindow.GetWindow<SceneView>();
                    sv.Focus();
                    sv.drawGizmos = true;
                    sv.LookAt(new Vector3(18f, 14f, 28f), Quaternion.Euler(24f, 40f, 0f), 42f, false, true);
                    sv.Repaint();
                    Gec(Adim.GizmoCek);
                    break;

                case Adim.GizmoCek:
                    if (Gecen < 1.5) return;
                    PencereyiKaydet(SceneView.lastActiveSceneView, Path.Combine(TeslimKlasoru, "H02_Gizmos_Scene.png"));
                    EditorApplication.ExitPlaymode();
                    Gec(Adim.Rota03Ac);
                    break;

                case Adim.Rota03Ac:
                    if (!Durgun) return;
                    EditorSceneManager.OpenScene(Rota03);
                    EditorApplication.ExecuteMenuItem("Window/General/Game");
                    EditorApplication.EnterPlaymode();
                    Gec(Adim.Rota03Bekle);
                    break;

                case Adim.Rota03Bekle:
                    if (Gecen < 3) return;
                    // 04 · gözlemciyi sına: iki kez 10 hasar -> CanYazisi Console'a yazar
                    var saglik = Object.FindFirstObjectByType<Saglik>();
                    if (saglik != null) { saglik.Vur(10); saglik.Vur(10); }
                    ScreenCapture.CaptureScreenshot(Path.Combine(TeslimKlasoru, "H03_TakipKamerasi_Game.png"));
                    Gec(Adim.Bitti);
                    break;

                case Adim.Bitti:
                    if (Gecen < 1) return;
                    EditorSettings.enterPlayModeOptionsEnabled = eskiAyar;
                    EditorSettings.enterPlayModeOptions = eskiSecenek;
                    File.WriteAllText(Path.Combine(TeslimKlasoru, "_tamam.txt"), System.DateTime.Now.ToString("s"));
                    Debug.Log("TESLIM_GORUNTULERI_TAMAM — Rota03 Play modunda, uçmaya hazır.");
                    EditorApplication.update -= Guncelle;
                    break;
            }
        }
    }

    // ---------------------------------------------------------------- yardımcılar
    private static void PencereyiKaydet(EditorWindow pencere, string dosya)
    {
        if (pencere == null) return;
        float ppp = EditorGUIUtility.pixelsPerPoint;
        Rect r = pencere.position;
        int w = Mathf.RoundToInt(r.width * ppp), h = Mathf.RoundToInt(r.height * ppp);
        Color[] piksel = InternalEditorUtility.ReadScreenPixel(new Vector2(r.x * ppp, r.y * ppp), w, h);
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.SetPixels(piksel);
        tex.Apply();
        File.WriteAllBytes(dosya, tex.EncodeToPNG());
    }

    private static void Render(Camera kamera, string dosya)
    {
        var rt = new RenderTexture(1600, 900, 24);
        kamera.targetTexture = rt;
        kamera.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
        tex.Apply();
        kamera.targetTexture = null;
        RenderTexture.active = null;
        File.WriteAllBytes(dosya, tex.EncodeToPNG());
    }

    private static GameObject UcagiBul()
    {
        GameObject ucak = GameObject.Find("Nu_D36");
        while (ucak.transform.parent != null) ucak = ucak.transform.parent.gameObject;
        return ucak;
    }

    private static T Bilesen<T>(GameObject go) where T : Component
    {
        T b = go.GetComponent<T>();
        return b != null ? b : go.AddComponent<T>();
    }

    private static void Ata(Object bilesen, string alan, Object deger)
    {
        var so = new SerializedObject(bilesen);
        so.FindProperty(alan).objectReferenceValue = deger;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
#endif
