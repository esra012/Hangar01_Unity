using UnityEngine;

// Hedef görüş konisinin içinde mi, sağda mı solda mı?
// Uçaksavar nesnesine eklenir; Hedef alanına uçak sürüklenir.
public class Radar : MonoBehaviour
{
    [SerializeField] private Transform hedef;
    [SerializeField] private float yarimAci = 35f;   // koninin yarı açısı, derece
    [SerializeField] private float menzil = 120f;
    [SerializeField] private bool konsolaYaz = false; // işareti doğrulamak için

    public bool Goruyor { get; private set; }
    public float YanTaraf { get; private set; }
    public Transform Hedef => hedef;

    private void Update()
    {
        Hesapla();
        if (konsolaYaz)
            Debug.Log($"Goruyor={Goruyor}  YanTaraf={YanTaraf:F2} ({(YanTaraf > 0 ? "sağda" : "solda")})");
    }

    private void Hesapla()
    {
        if (hedef == null) return;

        Vector3 fark = hedef.position - transform.position;
        Vector3 yon = fark.normalized;

        // İç çarpım: 1 tam önümde, 0 yanımda, negatif arkamda
        float onde = Vector3.Dot(transform.forward, yon);
        float esik = Mathf.Cos(yarimAci * Mathf.Deg2Rad);
        Goruyor = onde > esik && fark.magnitude < menzil;

        // Dış çarpım: yukarı bileşen pozitifse hedef sağda
        YanTaraf = Vector3.Cross(transform.forward, yon).y;
    }

    // 07 · Gizmos: yalnızca editörde çizilir
    private void OnDrawGizmos()
    {
        // Oyun çalışmıyorken de renk doğru olsun (Scene'de döndürme deneyi için)
        if (!Application.isPlaying) Hesapla();

        // İleri yön: her zaman mavi
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * 30f);

        // Bonus: görüş konisinin iki kenarı (sarı)
        Gizmos.color = Color.yellow;
        Vector3 sol = Quaternion.AngleAxis(-yarimAci, Vector3.up) * transform.forward;
        Vector3 sag = Quaternion.AngleAxis(yarimAci, Vector3.up) * transform.forward;
        Gizmos.DrawRay(transform.position, sol * menzil);
        Gizmos.DrawRay(transform.position, sag * menzil);

        if (hedef == null) return;

        // Hedefe giden vektör: görüyorsa yeşil, görmüyorsa kırmızı
        Gizmos.color = Goruyor ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, hedef.position);
        Gizmos.DrawWireSphere(hedef.position, 4f);
    }
}
