using UnityEngine;

// Bonus · Mermi: normalize yön vektörüyle düz giden küre (fizik yok).
public class Mermi : MonoBehaviour
{
    private Vector3 yon;
    private float hiz;

    public void Baslat(Vector3 y, float h)
    {
        yon = y; hiz = h;
        Destroy(gameObject, 4f);
    }

    private void Update() => transform.position += yon * hiz * Time.deltaTime;
}
