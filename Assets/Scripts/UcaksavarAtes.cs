using UnityEngine;

// Bonus · Mermi: Radar hedefi konisine aldığında aralıklarla mermi üretir.
// Uçaksavar nesnesine Radar ile birlikte eklenir.
[RequireComponent(typeof(Radar))]
public class UcaksavarAtes : MonoBehaviour
{
    [SerializeField] private float atisAraligi = 0.5f; // saniye
    [SerializeField] private float mermiHizi = 60f;
    private Radar radar;
    private float sayac;

    private void Awake() => radar = GetComponent<Radar>();

    private void Update()
    {
        sayac -= Time.deltaTime;
        if (!radar.Goruyor || radar.Hedef == null || sayac > 0f) return;
        sayac = atisAraligi;

        Transform m = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        m.name = "Mermi";
        m.localScale = Vector3.one * 0.6f;
        Destroy(m.GetComponent<Collider>());
        m.position = transform.position + Vector3.up;

        // Ateş anındaki normalize yön; mermi sonra düz gider (fizik yok)
        Vector3 yon = (radar.Hedef.position - m.position).normalized;
        m.gameObject.AddComponent<Mermi>().Baslat(yon, mermiHizi);
    }
}
