using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Brillo ámbar debajo de la bandeja de frascos del nivel 1.
    /// </summary>
    public class BrilloBandeja : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Preparar()
        {
            new GameObject("BrilloBandeja").AddComponent<BrilloBandeja>();
        }

        IEnumerator Start()
        {
            MesaDeAlquimia mesa = null;
            for (int i = 0; i < 40 && mesa == null; i++)
            {
                mesa = FindFirstObjectByType<MesaDeAlquimia>();
                if (mesa != null && mesa.isActiveAndEnabled && AjustesJuego.NivelSeleccionado == 1)
                    break;
                mesa = null;
                yield return null;
            }

            if (mesa == null)
            {
                Destroy(gameObject);
                yield break;
            }

            
            var bandeja = GameObject.Find("Bandeja");
            if (bandeja == null)
            {
                Destroy(gameObject);
                yield break;
            }

            var origen = bandeja.GetComponent<RectTransform>();
            brillo = Imagen(origen.parent, "BrilloBandeja", Disco(64), new Vector2(origen.sizeDelta.x * 0.92f, 46f), new Color(1f, 0.72f, 0.32f, 0.28f));
            brillo.anchoredPosition = origen.anchoredPosition + new Vector2(0f, -origen.sizeDelta.y * 0.42f);
            brillo.SetSiblingIndex(origen.GetSiblingIndex());

        }

        RectTransform brillo;

        void Update()
        {
            if (brillo == null)
                return;

            var imagen = brillo.GetComponent<Image>();
            var color = imagen.color;
            color.a = 0.18f + Mathf.Sin(Time.time * 2.1f) * 0.08f;
            imagen.color = color;
        }

        static RectTransform Imagen(Transform padre, string nombre, Sprite sprite, Vector2 tamano, Color color)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(padre, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = tamano;
            var imagen = go.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.color = color;
            imagen.raycastTarget = false;
            return rect;
        }

        static Sprite Disco(int n)
        {
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            var pixeles = new Color[n * n];
            float radio = (n - 1) * 0.5f;
            var centro = new Vector2(radio, radio);
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    float lejos = Vector2.Distance(new Vector2(x, y), centro) / radio;
                    float alfa = Mathf.Clamp01(1f - lejos);
                    pixeles[y * n + x] = new Color(1f, 1f, 1f, alfa * alfa);
                }
            }

            tex.SetPixels(pixeles);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, n, n), new Vector2(0.5f, 0.5f), 100f);
        }

    }
}
