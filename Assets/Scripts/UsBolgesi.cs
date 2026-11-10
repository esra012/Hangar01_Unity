using UnityEngine;

// Üs bölgesindeki tetikleyici: oyuncu girince görev yöneticisine haber verir.
// UsBolgesi nesnesinde: Box Collider (Is Trigger, 55 x 220 x 55, merkez Y = 110).
public class UsBolgesi : MonoBehaviour
{
    [SerializeField] private GorevYoneticisi gorev;

    private void OnTriggerEnter(Collider other)
    {
        if (gorev == null || other.GetComponentInParent<UcusKontrol>() == null) return;
        gorev.UsseVarildi();
    }
}
