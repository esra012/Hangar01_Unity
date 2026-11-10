using UnityEngine;
using UnityEngine.InputSystem;

// Uçağı klavyeyle (ya da oyun koluyla) uçurur. Fizik yok; yalnızca Transform.
// Kodda hiçbir tuş adı geçmez: tuşlar Assets/Input/Ucus.inputactions tablosunda.
public class UcusKontrol : MonoBehaviour
{
    [SerializeField] private float enAzHiz = 8f;
    [SerializeField] private float enCokHiz = 42f;
    [SerializeField] private float yunuslamaHizi = 55f;   // derece / saniye
    [SerializeField] private float yatisHizi = 90f;
    [SerializeField] private float sapmaHizi = 30f;
    [SerializeField] private float yumusatma = 6f;        // deney: 1 ve 20
    [SerializeField] private float gazDegisimHizi = 0.5f; // saniyede gaz oranı
    [SerializeField] private float oluBolge = 0.15f;      // bonus: ölü bölge

    private float yunuslama, yatis, sapma, gazGirdisi, gaz = 0.5f;
    private float hYunuslama, hYatis, hSapma;            // yumuşatılmış hâlleri

    public float Hiz { get; private set; }
    public float GazOrani => gaz;

    // Metot adları eylem adlarından türer: Yunuslama -> OnYunuslama
    public void OnYunuslama(InputValue v) => yunuslama = OluBolge(v.Get<float>());
    public void OnYatis(InputValue v)     => yatis     = OluBolge(v.Get<float>());
    public void OnSapma(InputValue v)     => sapma     = OluBolge(v.Get<float>());

    // Föydeki "gaz += v * 0.02f" tuşa basılı tutulduğunda yalnızca bir kez çalışır
    // (mesaj değer değiştiğinde gelir). Girdiyi saklayıp Update'te zamanla biriktiriyoruz.
    public void OnGaz(InputValue v)       => gazGirdisi = OluBolge(v.Get<float>());

    private float OluBolge(float deger) => Mathf.Abs(deger) < oluBolge ? 0f : deger;

    private void Update()
    {
        float dt = Time.deltaTime;

        // Girdiyi yumuşat: ani sıçrama yerine kademeli yaklaşma
        float k = 1f - Mathf.Exp(-yumusatma * dt);
        hYunuslama = Mathf.Lerp(hYunuslama, yunuslama, k);
        hYatis     = Mathf.Lerp(hYatis,     yatis,     k);
        hSapma     = Mathf.Lerp(hSapma,     sapma,     k);

        transform.Rotate(Vector3.right,   hYunuslama * yunuslamaHizi * dt, Space.Self);
        transform.Rotate(Vector3.forward, -hYatis    * yatisHizi     * dt, Space.Self);
        transform.Rotate(Vector3.up,       hSapma    * sapmaHizi     * dt, Space.World);

        gaz = Mathf.Clamp01(gaz + gazGirdisi * gazDegisimHizi * dt);
        Hiz = Mathf.Lerp(enAzHiz, enCokHiz, gaz);
        transform.position += transform.forward * Hiz * dt;
    }
}
