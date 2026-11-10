using UnityEngine;

// Gözlemci: Saglik'in olaylarını dinler. Saglik bu betiğin varlığından habersizdir.
// Ayrı bir nesneye ("Arayuz") eklenir; Saglik alanına uçak sürüklenir.
public class CanYazisi : MonoBehaviour
{
    [SerializeField] private Saglik saglik;

    private void OnEnable()
    {
        if (saglik == null) return;
        saglik.CanDegisti += CanDegistiginde;   // abone ol
        saglik.Oldu += OldugUnde;
    }

    private void OnDisable()
    {
        if (saglik == null) return;
        saglik.CanDegisti -= CanDegistiginde;   // aboneliği bırak
        saglik.Oldu -= OldugUnde;
    }

    private void CanDegistiginde(int kalan, int enCok) => Debug.Log($"[CanYazisi] Can: {kalan} / {enCok}");
    private void OldugUnde() => Debug.Log("[CanYazisi] Uçak düştü!");
}
