using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Vapor blanco que sale de la boca del caldero del nivel 1.
    /// </summary>
    public class VaporCaldero : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Preparar()
        {
            new GameObject("VaporCaldero").AddComponent<VaporCaldero>();
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

            
            var caldero = GameObject.Find("CalderoVivo");
            if (caldero == null)
            {
                Destroy(gameObject);
                yield break;
            }

            nubes = new RectTransform[5];
            fases = new float[5];
            for (int i = 0; i < nubes.Length; i++)
            {
                fases[i] = i * 0.37f;
                nubes[i] = Imagen(caldero.transform, "Vapor", Disco(32), new Vector2(28f, 16f), new Color(0.95f, 0.95f, 0.98f, 0f));
            }

        }

        RectTransform[] nubes;
        float[] fases;

        void Update()
        {
            if (nubes == null)
                return;

            for (int i = 0; i < nubes.Length; i++)
            {
                float ciclo = (Time.time * 0.22f + fases[i]) % 1f;
                float x = Mathf.Sin((Time.time + fases[i]) * 1.3f) * 16f;
                nubes[i].anchoredPosition = new Vector2(x, Mathf.Lerp(8f, 78f, ciclo));
                nubes[i].localScale = Vector3.one * Mathf.Lerp(0.6f, 1.5f, ciclo);
                var imagen = nubes[i].GetComponent<Image>();
                var color = imagen.color;
                color.a = Mathf.Sin(ciclo * Mathf.PI) * 0.45f;
                imagen.color = color;
            }
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
