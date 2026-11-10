using UnityEngine;

// Hedefi yerel uzayında takip eder; hız arttıkça geri çekilir ve görüşü açar.
[RequireComponent(typeof(Camera))]
public class TakipKamerasi : MonoBehaviour
{
    [SerializeField] private Transform hedef;
    [SerializeField] private Vector3 uzaklik = new Vector3(0f, 3.2f, -11f);
    [SerializeField] private float yumusatma = 0.18f;     // deney: 0,02 ve 0,6
    [SerializeField] private float enAzGorus = 55f;
    [SerializeField] private float enCokGorus = 72f;

    private Camera kam;
    private Vector3 hiz;                  // SmoothDamp'in iç durumu
    private UcusKontrol ucus;

    private void Awake()
    {
        kam = GetComponent<Camera>();
        if (hedef != null) ucus = hedef.GetComponent<UcusKontrol>();
    }

    private void LateUpdate()            // Update DEĞİL: hedef hareketini bitirsin
    {
        if (hedef == null) return;

        Vector3 istenen = hedef.TransformPoint(uzaklik);
        transform.position = Vector3.SmoothDamp(transform.position, istenen, ref hiz, yumusatma);
        transform.LookAt(hedef.position + hedef.up * 1.2f, hedef.up);

        if (ucus != null)
            kam.fieldOfView = Mathf.Lerp(kam.fieldOfView,
                Mathf.Lerp(enAzGorus, enCokGorus, ucus.GazOrani), Time.deltaTime * 3f);
    }
}
