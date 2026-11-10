using UnityEngine;

// Bonus · Uydu: uçağın çocuğu olan küçük küreyi ebeveynin etrafında döndürür.
// Yerel konumu döndürdüğümüz için uçak nereye giderse uydu da onunla gider.
public class Uydu : MonoBehaviour
{
    [SerializeField] private float yaricap = 6f;
    [SerializeField] private float acisalHiz = 180f; // derece / saniye
    private float aci;

    private void Update()
    {
        aci += acisalHiz * Time.deltaTime;
        transform.localPosition = Quaternion.AngleAxis(aci, Vector3.forward) * (Vector3.up * yaricap);
    }
}
