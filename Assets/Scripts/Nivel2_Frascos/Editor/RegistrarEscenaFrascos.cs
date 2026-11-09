using System.Collections.Generic;
using UnityEditor;

namespace FluxLab.Nivel2.EditorTools
{
    /// <summary>
    /// Agrega a Build Settings, al abrir Unity, las escenas que usan la mesa de frascos
    /// (Pasillo de Frascos y Torre de Cristal), para que SceneManager.LoadScene las encuentre.
    /// </summary>
    [InitializeOnLoad]
    static class RegistrarEscenaFrascos
    {
        static readonly string[] Rutas =
        {
            "Assets/Scenes/Nivel2_Frascos.unity",
            "Assets/Scenes/Nivel4_Cristal.unity"
        };
        const string DespuesDe = "Assets/Scenes/MesaDeAlquimia.unity";

        static RegistrarEscenaFrascos()
        {
            var escenas = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool cambio = false;
            foreach (string ruta in Rutas)
            {
                if (escenas.Exists(e => e.path == ruta))
                    continue;

                int indice = escenas.Count;
                for (int i = 0; i < escenas.Count; i++)
                {
                    if (escenas[i].path == DespuesDe)
                        indice = i + 1;
                }

                escenas.Insert(indice, new EditorBuildSettingsScene(ruta, true));
                cambio = true;
            }

            if (cambio)
                EditorBuildSettings.scenes = escenas.ToArray();
        }
    }
}
