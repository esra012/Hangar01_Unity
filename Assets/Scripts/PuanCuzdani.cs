using System.Collections.Generic;
using UnityEngine;

// Puanı tutar ve değiştiğinde duyurur. Puanın nereden geldiğini bilmez;
// kaynağın adını yalnızca döküm için saklar.
public class PuanCuzdani : MonoBehaviour
{
    [Header("Ödüller")]
    [SerializeField] private int inisPuani = 30;
    [SerializeField] private int hasarsizPuani = 40;

    [Header("Kilit eşiği")]
    [Range(100, 2000)]
    [SerializeField] private int hurricaneEsigi = 400;

    public int Puan { get; private set; }
    public event System.Action<int> PuanDegisti;            // yeni toplam
    public event System.Action<int, string> PuanEklendi;    // kazanç + kaynak ("+100" yazısı için)
    public event System.Action<string> UcakAcildi;

    // Görev sonu ekranı için kaynak bazında döküm (eklenme sırasıyla)
    private readonly List<KeyValuePair<string, int>> dokum = new List<KeyValuePair<string, int>>();
    public IReadOnlyList<KeyValuePair<string, int>> Dokum => dokum;

    private bool hurricaneAcik;

    private void OnEnable()  { KesifHedefi.Fotograflandi += HedefAlindi; }
    private void OnDisable() { KesifHedefi.Fotograflandi -= HedefAlindi; }

    private void HedefAlindi(int puan, bool yanHedef)
    {
        Ekle(puan, yanHedef ? "Yan hedefler" : "Ana hedefler");
    }

    public void Ekle(int miktar) => Ekle(miktar, "Diğer");

    public void Ekle(int miktar, string kaynak)
    {
        Puan += miktar;
        DokumeYaz(kaynak, miktar);
        PuanEklendi?.Invoke(miktar, kaynak);
        PuanDegisti?.Invoke(Puan);

        if (!hurricaneAcik && Puan >= hurricaneEsigi)
        {
            hurricaneAcik = true;
            UcakAcildi?.Invoke("Hawker Hurricane");
        }
    }

    private void DokumeYaz(string kaynak, int miktar)
    {
        for (int i = 0; i < dokum.Count; i++)
        {
            if (dokum[i].Key != kaynak) continue;
            dokum[i] = new KeyValuePair<string, int>(kaynak, dokum[i].Value + miktar);
            return;
        }
        dokum.Add(new KeyValuePair<string, int>(kaynak, miktar));
    }

    public int InisPuani { get { return inisPuani; } }
    public int HasarsizPuani { get { return hasarsizPuani; } }
    public int HurricaneEsigi { get { return hurricaneEsigi; } }
}
