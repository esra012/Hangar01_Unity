using UnityEngine;

// Yalnızca canı tutar ve değiştiğinde haber verir. Kimin dinlediğini bilmez.
public class Saglik : MonoBehaviour
{
    [SerializeField] private int enCokCan = 100;

    public int Can { get; private set; }
    public event System.Action<int, int> CanDegisti;   // (kalan, en çok)
    public event System.Action Oldu;

    private void Awake() => Can = enCokCan;

    public void Vur(int miktar)
    {
        if (Can <= 0) return;
        Can = Mathf.Max(0, Can - miktar);
        CanDegisti?.Invoke(Can, enCokCan);
        if (Can == 0) Oldu?.Invoke();
    }
}
