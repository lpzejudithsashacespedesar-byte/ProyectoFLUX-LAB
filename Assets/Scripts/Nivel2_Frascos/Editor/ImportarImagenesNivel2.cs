using UnityEditor;
using UnityEngine;

namespace FluxLab.Nivel2.EditorTools
{
    /// <summary>
    /// Importa las imágenes de Resources/FluxLab/NIVEL2-IMAGENES (y NIVEL4-IMAGENES, que usa la misma mesa) sin deformarlas
    /// (el proyecto está en modo 3D y Unity las estiraría a 256x512) y sin compresión,
    /// para que los frascos se vean nítidos.
    /// </summary>
    class ImportarImagenesNivel2 : AssetPostprocessor
    {
        static readonly string[] Carpetas = { "/NIVEL2-IMAGENES/", "/NIVEL4-IMAGENES/" };

        public override uint GetVersion() => 2;

        void OnPreprocessTexture()
        {
            if (System.Array.FindIndex(Carpetas, c => assetPath.Contains(c)) < 0)
                return;

            var importador = (TextureImporter)assetImporter;
            importador.textureType = TextureImporterType.Default;
            importador.npotScale = TextureImporterNPOTScale.None;
            importador.mipmapEnabled = false;
            importador.isReadable = true;
            importador.textureCompression = TextureImporterCompression.Uncompressed;
            importador.wrapMode = TextureWrapMode.Clamp;
            importador.filterMode = FilterMode.Bilinear;
        }
    }
}
