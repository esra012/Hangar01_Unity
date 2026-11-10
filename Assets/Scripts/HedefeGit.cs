using UnityEngine;

// Uçağı hedefe doğru döndürür, sabit hızla ilerletir ve dönüş yönüne kanat yatırır.
// En üstteki Nu_D36 nesnesine eklenir; Hedef alanına Balon sürüklenir.
public class HedefeGit : MonoBehaviour
{
    [SerializeField] private Transform hedef;
    [SerializeField] private float hiz = 14f;          // metre / saniye
    [SerializeField] private float donusHizi = 60f;    // derece / saniye
    [SerializeField] private float varisMesafesi = 8f;

    [Header("06 · Kanat yatışı (dış çarpım)")]
    [SerializeField] private float enFazlaYatis = 45f; // derece
    [SerializeField] private float yatisYumusakligi = 3f;
    [SerializeField] private bool yatisiTersCevir = false; // yatış ters yöndeyse işaretle

    // Yatışsız yönelme. Yatışı bunun üstüne her karede yeniden "atıyoruz";
    // böylece föydeki Rotate satırının birikip takla attırma hatası oluşmaz.
    private Quaternion yonelme;
    private float yatis;

    public float Yan { get; private set; }   // Inspector / Console için

    private void Start()
    {
        yonelme = transform.rotation;
    }

    private void Update()
    {
        if (hedef == null) return;

        // 1) Çıkarma: hedefe giden vektör
        Vector3 fark = hedef.position - transform.position;

        // 2) Uzunluk: hedefe olan uzaklık
        float mesafe = fark.magnitude;
        if (mesafe < varisMesafesi) return;

        // 3) Normalize: yalnızca yön kalsın
        //    Deney: ".normalized" yerine "fark" yaz; uçak uzaktayken fırlar.
        Vector3 yon = fark.normalized;

        // 4) Burnu hedefe çevir (kuaterniyonla yumuşak dönüş)
        Quaternion hedefDonusu = Quaternion.LookRotation(yon);
        yonelme = Quaternion.RotateTowards(yonelme, hedefDonusu, donusHizi * Time.deltaTime);

        // 6) Dış çarpımın yukarı bileşeni: pozitifse hedef sağda
        Vector3 ileri = yonelme * Vector3.forward;
        Yan = Vector3.Cross(ileri, yon).y;

        // Kanadı dönüş yönüne yatır (en çok enFazlaYatis derece), yumuşak geçişle
        float isaret = yatisiTersCevir ? 1f : -1f;
        float hedefYatis = Mathf.Clamp(isaret * Yan * enFazlaYatis, -enFazlaYatis, enFazlaYatis);
        yatis = Mathf.Lerp(yatis, hedefYatis, yatisYumusakligi * Time.deltaTime);
        transform.rotation = yonelme * Quaternion.AngleAxis(yatis, Vector3.forward);

        // 5) Skalerle çarpma: burnu yönünde ilerle (yatıştan bağımsız)
        transform.position += ileri * hiz * Time.deltaTime;
    }

    // Bonus "Nişangah" betiği hedefi buradan değiştirir.
    public void HedefAta(Transform yeniHedef) => hedef = yeniHedef;
}
