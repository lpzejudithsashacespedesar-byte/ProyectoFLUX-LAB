using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Fondo animado con cuadros sueltos (StreamingAssets/Menu/FondoAnimado/fondo_00.jpg, fondo_01.jpg...).
    /// El primer cuadro se pone al instante, para no mostrar el fondo estático viejo mientras carga el resto.
    /// </summary>
    public class FondoAnimado : MonoBehaviour
    {
        public float segundosPorCuadro = 0.09f;

        RawImage imagen;
        string[] archivos;
        Texture2D[] cuadros;
        bool listo;
        float reloj;

        /// <summary>Convierte un fondo en animado. Devuelve false si no hay cuadros.</summary>
        public static bool Aplicar(RawImage imagen)
        {
            string carpeta = Path.Combine(Application.streamingAssetsPath, "Menu", "FondoAnimado");
            if (!Directory.Exists(carpeta))
                return false;

            string[] archivos = Directory.GetFiles(carpeta, "*.jpg");
            if (archivos.Length < 2)
                return false;

            Array.Sort(archivos);

            var animado = imagen.gameObject.AddComponent<FondoAnimado>();
            animado.imagen = imagen;
            animado.archivos = archivos;
            animado.cuadros = new Texture2D[archivos.Length];
            animado.cuadros[0] = Cargar(archivos[0]);
            imagen.texture = animado.cuadros[0];
            return true;
        }

        static Texture2D Cargar(string archivo)
        {
            var textura = new Texture2D(2, 2, TextureFormat.RGB24, false);
            textura.LoadImage(File.ReadAllBytes(archivo));
            textura.filterMode = FilterMode.Bilinear;
            return textura;
        }

        IEnumerator Start()
        {
            for (int i = 1; i < archivos.Length; i++)
            {
                cuadros[i] = Cargar(archivos[i]);
                yield return null; // un cuadro por frame para no congelar el menú al abrir
            }

            listo = true;
        }

        void Update()
        {
            if (!listo)
                return;

            reloj += Time.deltaTime;
            int cuadro = (int)(reloj / segundosPorCuadro) % cuadros.Length;
            if (imagen.texture != cuadros[cuadro])
                imagen.texture = cuadros[cuadro];
        }

        void OnDestroy()
        {
            if (cuadros == null)
                return;
            foreach (var textura in cuadros)
                if (textura != null)
                    Destroy(textura);
        }
    }
}
