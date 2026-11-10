using UnityEngine;
using UnityEngine.InputSystem;

// Uçağı klavyeyle (ya da oyun koluyla) uçurur. Fizik yok; yalnızca Transform.
// Kodda hiçbir tuş adı geçmez: tuşlar Assets/Input/Ucus.inputactions tablosunda.
// Hafta 4: pistten kalkış, düzgün iniş ve çakılma eklendi.
public class UcusKontrol : MonoBehaviour
{
    public enum Faz { Yerde, Havada }

    [SerializeField] private float enAzHiz = 8f;
    [SerializeField] private float enCokHiz = 42f;
    [SerializeField] private float yunuslamaHizi = 55f;   // derece / saniye
    [SerializeField] private float yatisHizi = 90f;
    [SerializeField] private float sapmaHizi = 30f;
    [SerializeField] private float yumusatma = 6f;        // deney: 1 ve 20
    [SerializeField] private float gazDegisimHizi = 0.5f; // saniyede gaz oranı
    [SerializeField] private float oluBolge = 0.15f;      // bonus: ölü bölge

    [Header("Kalkış ve iniş")]
    [SerializeField] private bool yerdeBasla = true;
    [SerializeField] private float kalkisHizi = 26f;      // m/s; gaz kolundaki kırmızı çizgi
    [SerializeField] private float ivme = 7f;             // m/s²
    [SerializeField] private float yerPayi = 0f;          // zeminin yüksekliği
    [SerializeField] private float tekerYuksekligi = 0f;  // modelin merkezi tekerlerin altında
    [SerializeField] private float inisAcisi = 18f;       // iniş/çakılma sınırı (derece)
    [SerializeField] private float govdeYaricapi = 2.4f;  // engel taraması

    private float yunuslama, yatis, sapma, gazGirdisi, gaz = 0.5f;
    private float hYunuslama, hYatis, hSapma;            // yumuşatılmış hâlleri

    public float Hiz { get; private set; }
    public float GazOrani => gaz;
    public float EnCokHiz => enCokHiz;
    public float KalkisHizi => kalkisHizi;
    public Faz Durum { get; private set; } = Faz.Havada;
    public float YatisDerece => Mathf.DeltaAngle(0f, transform.eulerAngles.z);
    public float Yukseklik => transform.position.y - TabanY;

    public bool Carpti { get; private set; }
    public string CarpmaNedeni { get; private set; } = "";
    public static event System.Action<string> Carpisti;

    private float TabanY => yerPayi + tekerYuksekligi;

    // Metot adları eylem adlarından türer: Yunuslama -> OnYunuslama
    public void OnYunuslama(InputValue v) => yunuslama = OluBolge(v.Get<float>());
    public void OnYatis(InputValue v)     => yatis     = OluBolge(v.Get<float>());
    public void OnSapma(InputValue v)     => sapma     = OluBolge(v.Get<float>());

    // Föydeki "gaz += v * 0.02f" tuşa basılı tutulduğunda yalnızca bir kez çalışır
    // (mesaj değer değiştiğinde gelir). Girdiyi saklayıp Update'te zamanla biriktiriyoruz.
    public void OnGaz(InputValue v)       => gazGirdisi = OluBolge(v.Get<float>());

    private float OluBolge(float deger) => Mathf.Abs(deger) < oluBolge ? 0f : deger;

    private void Start()
    {
        if (yerdeBasla && transform.position.y <= TabanY + 0.5f)
        {
            Durum = Faz.Yerde;
            Hiz = 0f;
            gaz = 0.3f;
            YereOtur();
        }
        else
        {
            Durum = Faz.Havada;
            Hiz = Mathf.Lerp(enAzHiz, enCokHiz, gaz);
        }
    }

    private void Update()
    {
        if (Carpti) return;
        float dt = Time.deltaTime;

        // Girdiyi yumuşat: ani sıçrama yerine kademeli yaklaşma
        float k = 1f - Mathf.Exp(-yumusatma * dt);
        hYunuslama = Mathf.Lerp(hYunuslama, yunuslama, k);
        hYatis     = Mathf.Lerp(hYatis,     yatis,     k);
        hSapma     = Mathf.Lerp(hSapma,     sapma,     k);

        gaz = Mathf.Clamp01(gaz + gazGirdisi * gazDegisimHizi * dt);

        if (Durum == Faz.Yerde) Yerde(dt);
        else Havada(dt);
    }

    private void Yerde(float dt)
    {
        // Tekerlerde: gaz hızı belirler, Q/E dümenle yön verir; yunuslama ve yatış yok
        Hiz = Mathf.MoveTowards(Hiz, gaz * enCokHiz, ivme * dt);
        transform.Rotate(Vector3.up, hSapma * sapmaHizi * dt, Space.World);
        YereOtur();
        transform.position += transform.forward * Hiz * dt;

        // Kalkış: kalkış hızını geçtiysen ve burnu kaldırıyorsan (S) havalan
        if (Hiz >= kalkisHizi && hYunuslama < -0.2f)
        {
            Durum = Faz.Havada;
            transform.position += Vector3.up * 0.6f;
            transform.Rotate(Vector3.right, -6f, Space.Self);
        }
    }

    private void Havada(float dt)
    {
        transform.Rotate(Vector3.right,   hYunuslama * yunuslamaHizi * dt, Space.Self);
        transform.Rotate(Vector3.forward, -hYatis    * yatisHizi     * dt, Space.Self);
        transform.Rotate(Vector3.up,       hSapma    * sapmaHizi     * dt, Space.World);

        Hiz = Mathf.MoveTowards(Hiz, Mathf.Lerp(enAzHiz, enCokHiz, gaz), ivme * dt);
        transform.position += transform.forward * Hiz * dt;

        if (transform.position.y <= TabanY)
        {
            // Yere değmek bedava değil: düzgün inersen iniş, değilse çakılma
            float burun = Mathf.Abs(Mathf.DeltaAngle(0f, transform.eulerAngles.x));
            bool duzgun = Hiz < kalkisHizi
                          && burun < inisAcisi
                          && Mathf.Abs(YatisDerece) < inisAcisi * 1.5f;
            if (!duzgun) { Cakil("zemine çarptın"); return; }

            Durum = Faz.Yerde;                       // başarılı iniş
            YereOtur();
            return;
        }

        // Engeller: kalkış anında piste takılmasın diye yerden ayrılmayı bekliyoruz
        if (transform.position.y > TabanY + 1.5f) EngeleCarptiMi();
    }

    // Kinematik gövde ile statik çarpıştırıcı OnCollisionEnter üretmez; her kare küre taraması yapıyoruz.
    private void EngeleCarptiMi()
    {
        Vector3 merkez = transform.position + transform.up * 1.2f;
        foreach (var c in Physics.OverlapSphere(merkez, govdeYaricapi, ~0, QueryTriggerInteraction.Ignore))
        {
            if (c.transform.IsChildOf(transform)) continue;   // kendi çarpıştırıcımız
            Cakil(c.name + " engeline çarptın");
            return;
        }
    }

    private void YereOtur()
    {
        Vector3 ileri = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
        if (ileri.sqrMagnitude < 0.001f) ileri = Vector3.forward;
        transform.rotation = Quaternion.LookRotation(ileri.normalized, Vector3.up);
        Vector3 p = transform.position;
        p.y = TabanY;
        transform.position = p;
    }

    private void Cakil(string neden)
    {
        if (Carpti) return;
        Carpti = true;
        CarpmaNedeni = neden;
        Hiz = 0f;
        Carpisti?.Invoke(neden);
    }

    // Test ve hata ayıklama: uçağı havada verilen noktaya taşır
    public void Isinla(Vector3 konum, Quaternion yon, float hiz)
    {
        Durum = Faz.Havada;
        transform.SetPositionAndRotation(konum, yon);
        Hiz = hiz;
        gaz = Mathf.InverseLerp(enAzHiz, enCokHiz, hiz);
    }
}
