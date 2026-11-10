using System.Collections.Generic;
using UnityEngine;

// Üstünden geçilince bir kez fotoğraflanır. Puanı kendisi vermez,
// yalnızca "fotoğraflandım" diye haber verir.
// Hedef nesnesinde: Box Collider (Is Trigger, 30 x 180 x 30, merkez Y = 90).
public class KesifHedefi : MonoBehaviour
{
    [SerializeField] private int puan = 100;
    [SerializeField] private bool yanHedef = false;
    [SerializeField] private string gorunenAd = "Hedef";

    public static event System.Action<int, bool> Fotograflandi;

    // HUD'un nişan çizmesi için sahnedeki etkin hedefler
    public static readonly List<KesifHedefi> Etkinler = new List<KesifHedefi>();

    public int Puan => puan;
    public bool YanHedef => yanHedef;
    public string GorunenAd => gorunenAd;

    private bool alindi;

    private void OnEnable()  => Etkinler.Add(this);
    private void OnDisable() => Etkinler.Remove(this);

    private void OnTriggerEnter(Collider other)
    {
        if (alindi) return;
        if (other.GetComponentInParent<UcusKontrol>() == null) return;   // yalnızca oyuncu

        alindi = true;
        Fotograflandi?.Invoke(puan, yanHedef);
        // Görsel geri bildirim: şimdilik kapatmak yeterli
        gameObject.SetActive(false);
    }
}
