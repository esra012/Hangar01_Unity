using UnityEngine;

// Pervaneyi kendi ekseni etrafında sabit hızla döndürür.
public class PervaneDondur : MonoBehaviour
{
    // Inspector'dan değiştirilebilir: derece / saniye
    [SerializeField] private float donusHizi = 1200f;

    // Yerel Z ekseni: uçağın burun yönü
    [SerializeField] private Vector3 donusEkseni = Vector3.forward;

    private void Update()
    {
        // Time.deltaTime ile dönüş kare hızından bağımsız olur.
        transform.Rotate(donusEkseni, donusHizi * Time.deltaTime, Space.Self);
    }
}
