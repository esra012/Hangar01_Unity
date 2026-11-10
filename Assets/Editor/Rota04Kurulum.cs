#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Hafta 4 · İlk Sortie
//   KomutSatirindanKur : Rota03 -> Rota04, hedef prefab'ları, üs, oyun nesnesi, HUD, Build Settings (-batchmode)
//   TestCek            : Play modunda döngüyü otomatik dener (3 hedef, yan hedef, üsse dönüş, çakılma,
//                        süre dolması), ekran görüntülerini Teslim/ klasörüne yazar.
public static class Rota04Kurulum
{
    private const string Rota03 = "Assets/Scenes/Rota03.unity";
    private const string Rota04 = "Assets/Scenes/Rota04.unity";
    private const string Teslim = "Teslim";

    [MenuItem("Hangar/5 · Rota04 Sahnesini Kur")]
    private static void MenudenKur()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Kur();
    }

    [MenuItem("Hangar/6 · Rota04 Otomatik Test")]
    public static void TestCek() => Test.Baslat();

    public static void KomutSatirindanKur()
    {
        Kur();
        Debug.Log("ROTA04_TAMAM");
    }

    // ---------------------------------------------------------------- kurulum
    private static void Kur()
    {
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset("Assets/UI", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);

        var sahne = EditorSceneManager.OpenScene(Rota03, OpenSceneMode.Single);
        EditorSceneManager.SaveScene(sahne, Rota04);
        sahne = EditorSceneManager.OpenScene(Rota04, OpenSceneMode.Single);

        // Zemin büyüsün (3 km), üsse pist
        GameObject zemin = GameObject.Find("Zemin");
        if (zemin != null) zemin.transform.localScale = new Vector3(300f, 1f, 300f);
        Kutu("Pist", new Vector3(0f, 0.02f, -30f), new Vector3(16f, 0.04f, 180f), new Color(0.6f, 0.48f, 0.31f), carpisma: false);
        Kutu("Kule", new Vector3(28f, 4f, -70f), new Vector3(8f, 8f, 8f), new Color(0.42f, 0.45f, 0.28f), carpisma: true);

        // Uçaksavarı üssün yanından al
        var ucaksavar = GameObject.Find("Uçaksavar");
        if (ucaksavar != null) ucaksavar.transform.position = new Vector3(70f, 0f, 120f);

        // Uçak: pistte, burnu +Z. Föy 01: Sphere Collider (r=3, trigger değil) + kinematik Rigidbody
        GameObject ucak = UcagiBul();
        ucak.transform.SetPositionAndRotation(new Vector3(0f, 0f, -100f), Quaternion.identity);
        var kure = Bilesen<SphereCollider>(ucak);
        kure.radius = 3f; kure.center = new Vector3(0f, 1.2f, 0f); kure.isTrigger = false;
        var rb = Bilesen<Rigidbody>(ucak);
        rb.isKinematic = true; rb.useGravity = false;

        // 01 · Hedef prefab'ları
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        GameObject anaPrefab = HedefPrefab("Assets/Prefabs/Hedef.prefab", 100, false, new Vector3(12f, 20f, 12f), new Color(0.55f, 0.5f, 0.42f));
        GameObject yanPrefab = HedefPrefab("Assets/Prefabs/YanHedef.prefab", 25, true, new Vector3(6f, 8f, 6f), new Color(0.62f, 0.55f, 0.4f));

        foreach (var eski in Object.FindObjectsByType<KesifHedefi>(FindObjectsSortMode.None)) Object.DestroyImmediate(eski.gameObject);
        Yerlestir(anaPrefab, "Deniz Feneri", new Vector3(380f, 0f, 520f), new Vector3(6f, 34f, 6f));
        Yerlestir(anaPrefab, "Köprübaşı", new Vector3(-420f, 0f, 640f), Vector3.zero);
        Yerlestir(anaPrefab, "Depo", new Vector3(-300f, 0f, -380f), Vector3.zero);
        Yerlestir(yanPrefab, "Telsiz Direği", new Vector3(150f, 0f, 250f), Vector3.zero);
        Yerlestir(yanPrefab, "Çoban Kulübesi", new Vector3(-520f, 0f, 150f), Vector3.zero);

        // 03 · Üs bölgesi (0,0,0): 55 x 220 x 55 tetikleyici
        GameObject us = GameObject.Find("UsBolgesi");
        if (us == null) us = new GameObject("UsBolgesi");
        us.transform.position = Vector3.zero;
        var usKutu = Bilesen<BoxCollider>(us);
        usKutu.isTrigger = true; usKutu.size = new Vector3(55f, 220f, 55f); usKutu.center = new Vector3(0f, 110f, 0f);

        // 02-04 · Oyun nesnesi
        GameObject oyun = GameObject.Find("Oyun");
        if (oyun == null) oyun = new GameObject("Oyun");
        var cuzdan = Bilesen<PuanCuzdani>(oyun);
        var gorev = Bilesen<GorevYoneticisi>(oyun);
        Ata(gorev, "usBolgesi", us);
        Ata(gorev, "cuzdan", cuzdan);
        Ata(gorev, "saglik", ucak.GetComponent<Saglik>());
        Ata(Bilesen<UsBolgesi>(us), "gorev", gorev);
        var son = Bilesen<GorevSonuEkrani>(oyun);
        Ata(son, "gorev", gorev); Ata(son, "cuzdan", cuzdan);
        Ata(son, "puanTablosu", Doku("puan-tablosu")); Ata(son, "dugmeSeti", Doku("dugme-seti"));
        Bilesen<YenidenBasla>(oyun);

        // 07 · HUD
        var hud = Bilesen<UcusHUD>(oyun);
        Ata(hud, "ucak", ucak.GetComponent<UcusKontrol>()); Ata(hud, "gorev", gorev); Ata(hud, "cuzdan", cuzdan);
        Ata(hud, "cerceve", Doku("hud-cerceve")); Ata(hud, "hizKadrani", Doku("gosterge-hiz"));
        Ata(hud, "yukseklikKadrani", Doku("gosterge-yukseklik")); Ata(hud, "ibre", Doku("gosterge-ibre"));
        Ata(hud, "gazPlaka", Doku("gaz-kolu-plaka")); Ata(hud, "gazTopuz", Doku("gaz-kolu-topuz"));
        Ata(hud, "pusulaSerit", Doku("pusula-serit")); Ata(hud, "isaretler", Doku("hedef-isaretleri"));

        Camera.main.farClipPlane = 2500f;
        EditorSceneManager.MarkSceneDirty(sahne);
        EditorSceneManager.SaveScene(sahne);

        // Yeniden başlatma için sahneler Build Settings'te olmalı
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Rota04, true),
            new EditorBuildSettingsScene(Rota03, true),
            new EditorBuildSettingsScene("Assets/Scenes/Rota02.unity", true),
        };
    }

    private static GameObject HedefPrefab(string yol, int puan, bool yan, Vector3 bina, Color renk)
    {
        var kok = new GameObject(yan ? "YanHedef" : "Hedef");
        var tetik = kok.AddComponent<BoxCollider>();
        tetik.isTrigger = true; tetik.size = new Vector3(30f, 180f, 30f); tetik.center = new Vector3(0f, 90f, 0f);
        var h = kok.AddComponent<KesifHedefi>();
        var so = new SerializedObject(h);
        so.FindProperty("puan").intValue = puan;
        so.FindProperty("yanHedef").boolValue = yan;
        so.ApplyModifiedPropertiesWithoutUndo();

        var kutu = GameObject.CreatePrimitive(PrimitiveType.Cube);   // görünür bina; çarpıştırıcısı engel
        kutu.name = "Bina";
        kutu.transform.SetParent(kok.transform, false);
        kutu.transform.localScale = bina;
        kutu.transform.localPosition = new Vector3(0f, bina.y / 2f, 0f);
        Boya(kutu, renk, yan ? "R04_YanHedef" : "R04_Hedef");

        var prefab = PrefabUtility.SaveAsPrefabAsset(kok, yol);
        Object.DestroyImmediate(kok);
        return prefab;
    }

    private static void Yerlestir(GameObject prefab, string ad, Vector3 konum, Vector3 binaOlcegi)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.name = "Hedef_" + ad;
        go.transform.position = konum;
        var so = new SerializedObject(go.GetComponent<KesifHedefi>());
        so.FindProperty("gorunenAd").stringValue = ad;
        so.ApplyModifiedPropertiesWithoutUndo();
        if (binaOlcegi != Vector3.zero)
        {
            var b = go.transform.Find("Bina");
            b.localScale = binaOlcegi;
            b.localPosition = new Vector3(0f, binaOlcegi.y / 2f, 0f);
        }
    }

    private static void Kutu(string ad, Vector3 konum, Vector3 olcek, Color renk, bool carpisma)
    {
        var go = GameObject.Find(ad);
        if (go == null) { go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = ad; }
        go.transform.position = konum;
        go.transform.localScale = olcek;
        if (!carpisma) { var c = go.GetComponent<Collider>(); if (c != null) Object.DestroyImmediate(c); }
        Boya(go, renk, "R04_" + ad);
    }

    private static void Boya(GameObject go, Color renk, string ad)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        string yol = "Assets/Materials/" + ad + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(yol);
        if (m == null) { m = new Material(shader); AssetDatabase.CreateAsset(m, yol); }
        m.color = renk;
        go.GetComponent<Renderer>().sharedMaterial = m;
    }

    private static Texture2D Doku(string ad) => AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/UI/" + ad + ".png");

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

    // ---------------------------------------------------------------- Play modunda otomatik test
    private static class Test
    {
        private static readonly StringBuilder rapor = new StringBuilder();
        private class Adim { public float bekle; public System.Action is_; }
        private static readonly Queue<Adim> adimlar = new Queue<Adim>();
        private static double sonraki;
        private static bool eskiAyar;
        private static EnterPlayModeOptions eskiSecenek;

        private static UcusKontrol Ucak => Object.FindFirstObjectByType<UcusKontrol>();
        private static GorevYoneticisi Gorev => Object.FindFirstObjectByType<GorevYoneticisi>();
        private static PuanCuzdani Cuzdan => Object.FindFirstObjectByType<PuanCuzdani>();
        private static void Not(string s) { rapor.AppendLine(s); Debug.Log("[Rota04 Test] " + s); }
        private static void Foto(string ad) => ScreenCapture.CaptureScreenshot(Path.Combine(Teslim, ad));
        private static Vector3 Hedef(string ad) => GameObject.Find("Hedef_" + ad).transform.position;

        private static void Isinla(Vector3 hedef, float y, Vector3 nereden)
        {
            Vector3 p = new Vector3(hedef.x, y, hedef.z);
            Vector3 yon = p - new Vector3(nereden.x, y, nereden.z);
            Ucak.Isinla(p - yon.normalized * 2f, Quaternion.LookRotation(yon.normalized), 30f);
        }

        private static void Ekle(float bekle, System.Action is_) => adimlar.Enqueue(new Adim { bekle = bekle, is_ = is_ });

        public static void Baslat()
        {
            Directory.CreateDirectory(Teslim);
            rapor.Clear(); adimlar.Clear();
            eskiAyar = EditorSettings.enterPlayModeOptionsEnabled;
            eskiSecenek = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true;
            EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

            Ekle(0f, () => { EditorSceneManager.OpenScene(Rota04); EditorApplication.ExecuteMenuItem("Window/General/Game"); EditorApplication.EnterPlaymode(); });
            Ekle(4f, () =>
            {
                Not($"Başlangıç: faz={Ucak.Durum}, hız={Ucak.Hiz:F0}, ana hedef={Gorev.ToplamAna}, yan hedef={Gorev.ToplamYan}, süre={Gorev.KalanSure:F0} s, üs açık mı={Gorev.Us.gameObject.activeSelf}");
                Foto("H04_1_pistte.png");
            });
            Ekle(1f, () => Ucak.Isinla(new Vector3(0f, 60f, -260f), Quaternion.LookRotation(new Vector3(380f, 0f, 780f)), 34f));
            Ekle(2.5f, () => Foto("H04_2_ucus_HUD.png"));
            Ekle(0.5f, () => Isinla(Hedef("Deniz Feneri"), 60f, Ucak.transform.position));
            Ekle(0.25f, () => Foto("H04_3_puan_kazanildi.png"));
            Ekle(1f, () => { Not($"Deniz Feneri sonrası: puan={Cuzdan.Puan}, kalan ana={Gorev.KalanAna}"); Isinla(Hedef("Telsiz Direği"), 60f, Ucak.transform.position); });
            Ekle(1f, () => { Not($"Yan hedef sonrası: puan={Cuzdan.Puan}, yan={Gorev.AlinanYan}/{Gorev.ToplamYan}, kalan ana={Gorev.KalanAna}"); Isinla(Hedef("Köprübaşı"), 60f, Ucak.transform.position); });
            Ekle(1f, () => Isinla(Hedef("Depo"), 60f, Ucak.transform.position));
            Ekle(1.5f, () =>
            {
                Not($"Üç ana hedef sonrası: puan={Cuzdan.Puan}, kalan ana={Gorev.KalanAna}, dönüş fazı={Gorev.DonusFazi}, üs açık mı={Gorev.Us.gameObject.activeSelf}");
                Ucak.Isinla(new Vector3(0f, 80f, -160f), Quaternion.identity, 30f);
            });
            Ekle(1.5f, () => Foto("H04_4_usse_don.png"));
            Ekle(0.5f, () => Ucak.Isinla(new Vector3(0f, 60f, -5f), Quaternion.identity, 30f));
            Ekle(1.5f, () =>
            {
                Not($"Üsse dönüş: görev tamam={Gorev.GorevTamam}, toplam puan={Cuzdan.Puan}");
                foreach (var k in Cuzdan.Dokum) Not($"   döküm: {k.Key} = {k.Value}");
                Foto("H04_5_gorev_sonu.png");
            });
            Ekle(1.5f, () => EditorApplication.ExitPlaymode());

            // Çakılma: burun aşağı, yere yakın
            Ekle(1.5f, () => EditorApplication.EnterPlaymode());
            Ekle(3f, () => Ucak.Isinla(new Vector3(0f, 8f, -40f), Quaternion.Euler(40f, 0f, 0f), 30f));
            Ekle(0.8f, () => { Not($"Çakılma testi: çarptı mı={Ucak.Carpti}, neden='{Ucak.CarpmaNedeni}', görev başarısız={Gorev.Basarisiz}"); Foto("H04_6_cakildin.png"); });
            Ekle(3.5f, () => Not($"2,5 sn sonra sahne baştan: çarptı mı={Ucak.Carpti}, faz={Ucak.Durum}, puan={Cuzdan.Puan}, süre={Gorev.KalanSure:F0} s"));

            // Düzgün iniş: yavaş, burun ve kanat düz
            Ekle(0.5f, () => Ucak.Isinla(new Vector3(0f, 1.2f, -120f), Quaternion.Euler(3f, 0f, 0f), 18f));
            Ekle(1.5f, () => Not($"Yumuşak iniş testi: faz={Ucak.Durum}, çarptı mı={Ucak.Carpti}"));

            // Süre dolması
            Ekle(0.5f, () =>
            {
                var alan = typeof(GorevYoneticisi).GetField("kalanSure", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                alan.SetValue(Gorev, 8f);
            });
            Ekle(2f, () => Foto("H04_7_son_10_saniye.png"));
            Ekle(7f, () => { Not($"Süre testi: başarısız={Gorev.Basarisiz}, kalan süre={Gorev.KalanSure:F1}"); Foto("H04_8_sure_doldu.png"); });
            Ekle(3.5f, () => Not($"Süre dolduktan sonra sahne baştan: başarısız={Gorev.Basarisiz}, süre={Gorev.KalanSure:F0} s"));
            Ekle(0.5f, () => EditorApplication.ExitPlaymode());
            Ekle(1f, () =>
            {
                EditorSettings.enterPlayModeOptionsEnabled = eskiAyar;
                EditorSettings.enterPlayModeOptions = eskiSecenek;
                File.WriteAllText(Path.Combine(Teslim, "H04_test_sonuclari.txt"), rapor.ToString());
                Debug.Log("ROTA04_TEST_TAMAM");
            });

            sonraki = EditorApplication.timeSinceStartup + 1.0;
            EditorApplication.update -= Guncelle;
            EditorApplication.update += Guncelle;
        }

        private static void Guncelle()
        {
            if (adimlar.Count == 0) { EditorApplication.update -= Guncelle; return; }
            if (EditorApplication.isPlayingOrWillChangePlaymode != EditorApplication.isPlaying) return;   // geçiş sürüyor
            if (EditorApplication.timeSinceStartup < sonraki) return;

            var adim = adimlar.Dequeue();
            try { adim.is_(); }
            catch (System.Exception e) { Not("HATA: " + e.Message); }
            sonraki = EditorApplication.timeSinceStartup + (adimlar.Count > 0 ? adimlar.Peek().bekle : 0f);
        }
    }
}

// Föy 07 · içe aktarma ayarları: Assets/UI altındaki her görsele otomatik uygulanır
public class UIIceAktarma : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/UI/")) return;
        var t = (TextureImporter)assetImporter;
        t.textureType = TextureImporterType.Sprite;
        t.spriteImportMode = SpriteImportMode.Single;
        t.textureCompression = TextureImporterCompression.Uncompressed;
        t.mipmapEnabled = false;
        t.alphaIsTransparency = true;
        t.npotScale = TextureImporterNPOTScale.None;
        t.maxTextureSize = 2048;
        // Pusula şeridi Repeat: doku koordinatını kaydırarak döşüyoruz
        t.wrapMode = assetPath.EndsWith("pusula-serit.png") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
    }
}
#endif
