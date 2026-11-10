using System.Collections;
using System.Text;
using UnityEngine;

// 01 · deltaTime deneyi, otomatik: dört denemeyi sırayla yapar ve süreleri ölçer.
//   1) deltaTime YOK, 30 FPS    2) deltaTime YOK, 144 FPS
//   3) deltaTime VAR, 30 FPS    4) deltaTime VAR, 144 FPS
// Sonuçlar Console'a ve proje kökündeki Teslim/H03_deltaTime_sonuclari.txt dosyasına yazılır.
public class DeltaTimeDeneyi : MonoBehaviour
{
    [SerializeField] private float mesafe = 50f;            // "sahanın ucu"
    [SerializeField] private float kareBasinaAdim = 0.2f;   // föydeki deltaTime'sız değer
    [SerializeField] private float hiz = 12f;               // föydeki deltaTime'lı değer (m/s)

    public bool Tamamlandi { get; private set; }

    private IEnumerator Start()
    {
        // targetFrameRate yalnızca VSync kapalıyken etkili (Project Settings > Quality > VSync Count)
        QualitySettings.vSyncCount = 0;
        Vector3 baslangic = transform.position;

        var rapor = new StringBuilder("deltaTime deneyi (mesafe " + mesafe + " m)\n");
        (bool dt, int fps)[] denemeler = { (false, 30), (false, 144), (true, 30), (true, 144) };

        foreach (var d in denemeler)
        {
            Application.targetFrameRate = d.fps;
            transform.position = baslangic;
            yield return new WaitForSecondsRealtime(0.5f);   // kare hızı otursun

            float t0 = Time.time, yol = 0f;
            int kare = 0;
            while (yol < mesafe)
            {
                float adim = d.dt ? hiz * Time.deltaTime : kareBasinaAdim;
                transform.position += transform.forward * adim;
                yol += adim;
                kare++;
                yield return null;
            }
            float sure = Time.time - t0;
            string satir = $"deltaTime {(d.dt ? "VAR" : "YOK")}, hedef {d.fps} FPS (ölçülen ~{kare / sure:F0} FPS): {sure:F2} s";
            Debug.Log("[DeltaTimeDeneyi] " + satir);
            rapor.AppendLine(satir);
        }

        Application.targetFrameRate = -1;
        rapor.AppendLine("Sonuç: deltaTime yokken süre kare hızına bağlı; varken iki kare hızında da ~" + (mesafe / hiz).ToString("F2") + " s.");
        try
        {
            System.IO.Directory.CreateDirectory("Teslim");
            System.IO.File.WriteAllText("Teslim/H03_deltaTime_sonuclari.txt", rapor.ToString());
        }
        catch (System.Exception e) { Debug.LogWarning(e.Message); }
        Tamamlandi = true;
    }
}
