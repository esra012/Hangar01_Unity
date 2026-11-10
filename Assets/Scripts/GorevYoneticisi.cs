using UnityEngine;

// Hedefleri sayar, ana hedeflerin hepsi bitince dönüş fazını açar, süreyi geri sayar.
// Bu betik puanı hesaplamaz; yalnızca görevin durumunu bilir.
public class GorevYoneticisi : MonoBehaviour
{
    [SerializeField] private GameObject usBolgesi;   // dönüş için tetikleyici
    [SerializeField] private PuanCuzdani cuzdan;
    [SerializeField] private Saglik saglik;          // bonus: hasarsız dönüş

    [Header("Süre")]
    [Tooltip("Sortie bu sürede bitmezse başarısız sayılır. 0 = süre sınırı yok.")]
    [SerializeField] private float sureSaniye = 150f;

    private int kalan, kalanYan, toplamAna, toplamYan;
    private float kalanSure;

    public bool DonusFazi { get; private set; }
    public bool GorevTamam { get; private set; }
    public bool Basarisiz { get; private set; }
    public float KalanSure { get { return kalanSure; } }
    public float SureSiniri { get { return sureSaniye; } }
    public int KalanAna => kalan;
    public int ToplamAna => toplamAna;
    public int AlinanYan => toplamYan - kalanYan;
    public int ToplamYan => toplamYan;
    public Transform Us => usBolgesi != null ? usBolgesi.transform : null;

    public event System.Action<int> KalanDegisti;
    public event System.Action<int> GorevBitti;      // toplam puan

    // Çakılma gibi: duyuran biziz, sahneyi kimin yükleyeceği bizi ilgilendirmez
    public static event System.Action<string> SortieBasarisiz;

    private void Start()
    {
        foreach (var h in FindObjectsByType<KesifHedefi>(FindObjectsSortMode.None))
        {
            if (h.YanHedef) toplamYan++;
            else toplamAna++;
        }
        kalan = toplamAna;
        kalanYan = toplamYan;
        kalanSure = sureSaniye;
        if (usBolgesi != null) usBolgesi.SetActive(false);
        KalanDegisti?.Invoke(kalan);
    }

    private void OnEnable()
    {
        KesifHedefi.Fotograflandi += Azalt;
        UcusKontrol.Carpisti += Cakildi;
    }

    private void OnDisable()
    {
        KesifHedefi.Fotograflandi -= Azalt;
        UcusKontrol.Carpisti -= Cakildi;
    }

    private void Update()
    {
        if (sureSaniye <= 0f || GorevTamam || Basarisiz) return;

        kalanSure -= Time.deltaTime;
        if (kalanSure > 0f) return;

        kalanSure = 0f;
        Basarisiz = true;
        SortieBasarisiz?.Invoke("Süre doldu");
    }

    // Yan hedefler dönüşü açmaz: oyuncu onları toplamak için rotadan sapıp sapmamaya kendi karar verir.
    private void Azalt(int puan, bool yanHedef)
    {
        if (yanHedef) { kalanYan--; return; }

        kalan--;
        KalanDegisti?.Invoke(kalan);
        if (kalan <= 0)
        {
            DonusFazi = true;
            if (usBolgesi != null) usBolgesi.SetActive(true);
        }
    }

    private void Cakildi(string neden) => Basarisiz = true;

    // Üs bölgesindeki tetikleyici burayı çağırır
    public void UsseVarildi()
    {
        if (!DonusFazi || Basarisiz) return;
        DonusFazi = false;
        GorevTamam = true;
        if (cuzdan != null)
        {
            cuzdan.Ekle(cuzdan.InisPuani, "Üsse dönüş");
            if (saglik != null && saglik.Can == saglik.EnCokCan)
                cuzdan.Ekle(cuzdan.HasarsizPuani, "Hasarsız dönüş");
        }
        GorevBitti?.Invoke(cuzdan != null ? cuzdan.Puan : 0);
    }
}
