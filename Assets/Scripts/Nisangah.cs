using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Bonus · Nişangah: sol tıklanan noktayı uçağın yeni hedefi yapar.
// Kameradan fareye bir ışın atar ve yatay bir düzlemle kesiştirir.
public class Nisangah : MonoBehaviour
{
    [SerializeField] private HedefeGit ucak;
    [SerializeField] private float duzlemYuksekligi = 20f; // uçağın uçtuğu yükseklik

    private Transform isaret;

    private void Start()
    {
        isaret = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        isaret.name = "Nisangah_Isareti";
        isaret.localScale = Vector3.one * 2f;
        Destroy(isaret.GetComponent<Collider>());
        isaret.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (ucak == null || !TiklandiMi(out Vector2 fare)) return;

        Ray isin = Camera.main.ScreenPointToRay(fare);
        Plane duzlem = new Plane(Vector3.up, new Vector3(0f, duzlemYuksekligi, 0f));

        if (duzlem.Raycast(isin, out float uzaklik))
        {
            isaret.position = isin.GetPoint(uzaklik);
            isaret.gameObject.SetActive(true);
            ucak.HedefAta(isaret);
        }
    }

    // Projede hangi giriş sistemi açıksa onu kullanır
    private static bool TiklandiMi(out Vector2 konum)
    {
#if ENABLE_INPUT_SYSTEM
        konum = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
        konum = Input.mousePosition;
        return Input.GetMouseButtonDown(0);
#endif
    }
}
