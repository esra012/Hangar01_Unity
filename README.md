# Hangar01 · Rota 02

BMU1421 Dijital Oyun Tasarımı dersinin haftalık laboratuvar projesi.

- **Motor:** Unity 6.3 LTS (Universal 3D)
- **Model:** Blender 5.2 LTS

Nu.D.36'dan esinlenen çift kanatlı uçak, vektörlerle gözlem balonuna yöneliyor; uçaksavar iç çarpımla görüş konisi, dış çarpımla dönüş yönü hesaplıyor.

## Sahneler
- `Assets/Scenes/Rota02.unity`: Hafta 2 · yön vektörü, iç çarpım, dış çarpım, Gizmos

## Betikler
| Betik | Nesne | Görev |
| --- | --- | --- |
| `PervaneDondur.cs` | Pervane | Pervaneyi yerel Z ekseninde döndürür |
| `HedefeGit.cs` | Nu_D36 | Hedefe döner, normalize yönle ilerler, kanat yatırır |
| `Radar.cs` | Uçaksavar | Görüş konisi (iç çarpım), sağ/sol (dış çarpım), Gizmos |
| `Uydu.cs`, `Nisangah.cs`, `UcaksavarAtes.cs`, `Mermi.cs` | — | Bonus görevler |
