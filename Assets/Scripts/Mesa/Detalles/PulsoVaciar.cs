using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Halo que late detrás del botón Vaciar del nivel 1.
    /// </summary>
    public class PulsoVaciar : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Preparar()
        {
            new GameObject("PulsoVaciar").AddComponent<PulsoVaciar>();
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

            
            var boton = GameObject.Find("Boton_Vaciar");
            if (boton == null)
            {
                Destroy(gameObject);
                yield break;
            }

            var origen = boton.GetComponent<RectTransform>();
            halo = Imagen(origen.parent, "HaloVaciar", Disco(48), origen.sizeDelta + new Vector2(36f, 28f), new Color(1f, 0.78f, 0.35f, 0.35f));
            halo.anchoredPosition = origen.anchoredPosition;
            halo.SetSiblingIndex(origen.GetSiblingIndex());

        }

        RectTransform halo;

        void Update()
        {
            if (halo == null)
                return;

            float pulso = 0.9f + Mathf.Sin(Time.time * 3.2f) * 0.08f;
            halo.localScale = new Vector3(pulso, pulso, 1f);
            var imagen = halo.GetComponent<Image>();
            var color = imagen.color;
            color.a = 0.22f + Mathf.Sin(Time.time * 3.2f) * 0.12f;
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
