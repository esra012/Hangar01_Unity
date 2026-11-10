using UnityEngine;
using UnityEngine.InputSystem;

// Geçici test: Ucus tablosundaki HasarTest eylemi (H tuşu) gelince uçağa 10 hasar verir.
// Tuş adı yine kodda değil, girdi tablosunda.
[RequireComponent(typeof(Saglik))]
public class HasarTesti : MonoBehaviour
{
    [SerializeField] private int miktar = 10;
    private Saglik saglik;

    private void Awake() => saglik = GetComponent<Saglik>();

    public void OnHasarTest(InputValue v)
    {
        if (v.isPressed) saglik.Vur(miktar);
    }
}
