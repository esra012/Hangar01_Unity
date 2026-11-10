using UnityEngine;
using UnityEngine.SceneManagement;

// Çakılma ya da süre dolması: ikisini de dinler, kısa bir bekleyişten sonra sahneyi baştan yükler.
// Uçuş kodu sahne yönetiminden habersiz kalır.
public class YenidenBasla : MonoBehaviour
{
    [SerializeField] private float bekleme = 2.5f;
    private float sayac = -1f;

    private void OnEnable()
    {
        UcusKontrol.Carpisti += Basarisizlik;
        GorevYoneticisi.SortieBasarisiz += Basarisizlik;
    }

    private void OnDisable()
    {
        UcusKontrol.Carpisti -= Basarisizlik;
        GorevYoneticisi.SortieBasarisiz -= Basarisizlik;
    }

    private void Basarisizlik(string neden)
    {
        if (sayac < 0f) sayac = bekleme;     // ilk bildirim sayacı başlatır
    }

    private void Update()
    {
        if (sayac < 0f) return;
        sayac -= Time.deltaTime;
        if (sayac > 0f) return;

        sayac = -1f;
        var sahne = SceneManager.GetActiveScene();
        SceneManager.LoadScene(sahne.buildIndex >= 0 ? sahne.buildIndex : 0);
    }
}
