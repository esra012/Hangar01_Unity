#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Üst menüye "Hangar" ekler:
//   Hangar > 1 · Rota02 Sahnesini Kur   (föyün 01. adımındaki Save As + 03. adım + betikleri bağlama)
//   Hangar > 2 · Bonusları Ekle         (Uydu, Nişangah, Mermi)
// Bu dosya mutlaka Assets/Editor klasöründe durmalı.
public static class Rota02Kurulum
{
    private const string SahneYolu = "Assets/Scenes/Rota02.unity";

    [MenuItem("Hangar/1 · Rota02 Sahnesini Kur")]
    private static void SahneyiKur()
    {
        // 01 · Geçen haftanın sahnesini bozmadan Rota02 olarak kaydet
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        GameObject ucaksavar = Kur();
        Selection.activeGameObject = ucaksavar;
        EditorUtility.DisplayDialog("Rota02",
            "Sahne hazır ve Assets/Scenes/Rota02.unity olarak kaydedildi.\n\n" +
            "Uçak (0,20,0) · HedefeGit → Balon\nBalon (40,30,60), ölçek 6\nUçaksavar (0,0,0) · Radar → Uçak\n\n" +
            "Play'e bas. Gizmos çizgileri için Scene görünümünde Gizmos düğmesi açık olmalı.", "Tamam");
    }

    // Komut satırından: Unity.exe -batchmode -projectPath ... -executeMethod Rota02Kurulum.KomutSatirindanKur -quit
    public static void KomutSatirindanKur()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Kur();
        OnizlemeKaydet("Rota02_onizleme.png");
        Debug.Log("ROTA02_TAMAM");
    }

    // Komut satırından: sahneyi aç ve Play'e bas
    public static void AcVeOyna()
    {
        EditorSceneManager.OpenScene(SahneYolu);
        EditorApplication.EnterPlaymode();
    }

    private static GameObject Kur()
    {
        var sahne = EditorSceneManager.GetActiveScene();
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        if (sahne.path != SahneYolu) EditorSceneManager.SaveScene(sahne, SahneYolu);
        // 03 · Uçak (0, 20, 0)
        GameObject ucak = UcagiBul();
        Undo.RecordObject(ucak.transform, "Rota02");
        ucak.transform.SetPositionAndRotation(new Vector3(0f, 20f, 0f), Quaternion.identity);

        // Balon: çap 6, konum (40, 30, 60)
        GameObject balon = BulYaDaYap("Balon", PrimitiveType.Sphere);
        balon.transform.SetParent(null);
        balon.transform.SetPositionAndRotation(new Vector3(40f, 30f, 60f), Quaternion.identity);
        balon.transform.localScale = Vector3.one * 6f;
        Renk(balon, new Color(0.95f, 0.85f, 0.3f));

        // Uçaksavar (0, 0, 0) ve +Z'ye uzanan namlu
        GameObject ucaksavar = BulYaDaYap("Uçaksavar", PrimitiveType.Cylinder);
        ucaksavar.transform.SetParent(null);
        ucaksavar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        ucaksavar.transform.localScale = new Vector3(2f, 1f, 2f);
        Renk(ucaksavar, new Color(0.25f, 0.3f, 0.25f));
        if (ucaksavar.transform.Find("Namlu") == null)
        {
            GameObject namlu = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(namlu, "Rota02");
            namlu.name = "Namlu";
            namlu.transform.SetParent(ucaksavar.transform, false);
            namlu.transform.localPosition = new Vector3(0f, 0.6f, 1.2f);
            namlu.transform.localScale = new Vector3(0.12f, 0.25f, 1.4f);
            Renk(namlu, new Color(0.15f, 0.15f, 0.15f));
        }

        // Zemin
        if (GameObject.Find("Zemin") == null && GameObject.Find("Plane") == null)
        {
            GameObject zemin = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Undo.RegisterCreatedObjectUndo(zemin, "Rota02");
            zemin.name = "Zemin";
            zemin.transform.localScale = new Vector3(20f, 1f, 20f);
            Renk(zemin, new Color(0.45f, 0.55f, 0.35f));
        }

        // Hafta 1 · pervane dönsün
        Transform pervane = ucak.transform.Find("Pervane");
        if (pervane != null && pervane.GetComponent<PervaneDondur>() == null)
            Undo.AddComponent<PervaneDondur>(pervane.gameObject);

        // 04 · HedefeGit uçağa, hedef = Balon
        BagliBilesen<HedefeGit>(ucak, "hedef", balon.transform);
        // 05 · Radar uçaksavara, hedef = uçak
        BagliBilesen<Radar>(ucaksavar, "hedef", ucak.transform);

        // Kamera her şeyi görsün
        Camera kamera = Camera.main;
        if (kamera != null)
        {
            Undo.RecordObject(kamera.transform, "Rota02");
            kamera.transform.position = new Vector3(-45f, 45f, -35f);
            kamera.transform.LookAt(new Vector3(20f, 18f, 35f));
            kamera.farClipPlane = 1000f;
        }

        EditorSceneManager.MarkSceneDirty(sahne);
        EditorSceneManager.SaveScene(sahne);
        return ucaksavar;
    }

    private static void OnizlemeKaydet(string dosya)
    {
        Camera kamera = Camera.main;
        if (kamera == null) return;
        var rt = new RenderTexture(1280, 720, 24);
        kamera.targetTexture = rt;
        kamera.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        tex.Apply();
        kamera.targetTexture = null;
        RenderTexture.active = null;
        System.IO.File.WriteAllBytes(dosya, tex.EncodeToPNG());
    }

    [MenuItem("Hangar/2 · Bonusları Ekle")]
    private static void BonuslariEkle()
    {
        GameObject ucak = UcagiBul();

        // Uydu: uçağın çocuğu küçük küre
        if (ucak.transform.Find("Uydu") == null)
        {
            GameObject uydu = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Undo.RegisterCreatedObjectUndo(uydu, "Bonus");
            uydu.name = "Uydu";
            Object.DestroyImmediate(uydu.GetComponent<Collider>());
            uydu.transform.SetParent(ucak.transform, false);
            uydu.transform.localScale = Vector3.one * 0.8f;
            uydu.AddComponent<Uydu>();
        }

        // Nişangah: kameraya
        if (Camera.main != null)
            BagliBilesen<Nisangah>(Camera.main.gameObject, "ucak", ucak.GetComponent<HedefeGit>());

        // Mermi: uçaksavara
        GameObject ucaksavar = GameObject.Find("Uçaksavar");
        if (ucaksavar != null && ucaksavar.GetComponent<UcaksavarAtes>() == null)
            Undo.AddComponent<UcaksavarAtes>(ucaksavar);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Bonus", "Uydu, Nişangah (sol tık = yeni hedef) ve Mermi eklendi.", "Tamam");
    }

    // --- yardımcılar ---

    private static GameObject UcagiBul()
    {
        GameObject ucak = GameObject.Find("Nu_D36");
        if (ucak != null)
        {
            // FBX iki kök nesneli: Nu_D36 > Nu_D36 ve Nu_D36 > Pervane. En üsttekini al.
            while (ucak.transform.parent != null) ucak = ucak.transform.parent.gameObject;
            return ucak;
        }

        string[] bulunan = AssetDatabase.FindAssets("Nu_D36 t:Model");
        if (bulunan.Length > 0)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(bulunan[0]));
            ucak = (GameObject)PrefabUtility.InstantiatePrefab(model);
            ucak.name = "Nu_D36";
            Undo.RegisterCreatedObjectUndo(ucak, "Rota02");
            return ucak;
        }

        // Föydeki yedek plan: küp + burnu belli eden ince dikdörtgen
        ucak = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ucak.name = "Nu_D36";
        ucak.transform.localScale = new Vector3(2f, 1f, 4f);
        GameObject burun = GameObject.CreatePrimitive(PrimitiveType.Cube);
        burun.name = "Burun";
        burun.transform.SetParent(ucak.transform, false);
        burun.transform.localPosition = new Vector3(0f, 0f, 0.6f);
        burun.transform.localScale = new Vector3(0.2f, 0.2f, 0.4f);
        Renk(ucak, new Color(0.6f, 0.05f, 0.04f));
        Undo.RegisterCreatedObjectUndo(ucak, "Rota02");
        Debug.LogWarning("Nu_D36 modeli bulunamadı; yerine küp kullanıldı.");
        return ucak;
    }

    private static GameObject BulYaDaYap(string ad, PrimitiveType tur)
    {
        GameObject go = GameObject.Find(ad);
        if (go != null) { Undo.RecordObject(go.transform, "Rota02"); return go; }
        go = GameObject.CreatePrimitive(tur);
        go.name = ad;
        Undo.RegisterCreatedObjectUndo(go, "Rota02");
        return go;
    }

    private static void BagliBilesen<T>(GameObject go, string alan, Object deger) where T : Component
    {
        T bilesen = go.GetComponent<T>();
        if (bilesen == null) bilesen = Undo.AddComponent<T>(go);
        var so = new SerializedObject(bilesen);
        so.FindProperty(alan).objectReferenceValue = deger;
        so.ApplyModifiedProperties();
    }

    private static void Renk(GameObject go, Color renk)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(shader) { color = renk };
        string klasor = "Assets/Materials";
        if (!AssetDatabase.IsValidFolder(klasor)) AssetDatabase.CreateFolder("Assets", "Materials");
        string yol = $"{klasor}/R02_{go.name}.mat";
        var mevcut = AssetDatabase.LoadAssetAtPath<Material>(yol);
        if (mevcut == null) { AssetDatabase.CreateAsset(m, yol); mevcut = m; }
        else mevcut.color = renk;
        r.sharedMaterial = mevcut;
    }
}
#endif
