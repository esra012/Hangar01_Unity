#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Proje açıldığında sahne boşsa (Untitled) Rota03'ü otomatik açar.
// Depodan indirilen projede "son açılan sahne" bilgisi olmadığı için gerekli.
[InitializeOnLoad]
public static class ProjeAcilis
{
    private const string Sahne = "Assets/Scenes/Rota04.unity";

    static ProjeAcilis()
    {
        if (Application.isBatchMode || SessionState.GetBool("Hangar_AcilisYapildi", false)) return;
        SessionState.SetBool("Hangar_AcilisYapildi", true);

        EditorApplication.delayCall += () =>
        {
            var aktif = EditorSceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(aktif.path) && !aktif.isDirty && File.Exists(Sahne))
                EditorSceneManager.OpenScene(Sahne);
        };
    }
}
#endif
