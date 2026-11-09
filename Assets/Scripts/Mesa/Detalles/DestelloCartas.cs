using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Un destello recorre las cartas de ingredientes del nivel 1.
    /// </summary>
    public class DestelloCartas : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Preparar()
        {
            new GameObject("DestelloCartas").AddComponent<DestelloCartas>();
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

            
            yield return null;
            var rects = FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var lista = new System.Collections.Generic.List<RectTransform>();
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i] != null && rects[i].name.StartsWith("Carta_"))
                    lista.Add(rects[i]);
            }

            if (lista.Count == 0)
            {
                Destroy(gameObject);
                yield break;
            }

            cartas = lista.ToArray();
            System.Array.Sort(cartas, (a, b) => a.anchoredPosition.x.CompareTo(b.anchoredPosition.x));
            destellos = new Image[cartas.Length];
            for (int i = 0; i < cartas.Length; i++)
            {
                var luz = Imagen(cartas[i], "DestelloCarta", Disco(32), cartas[i].sizeDelta + new Vector2(18f, 18f), new Color(1f, 0.9f, 0.55f, 0f));
                luz.SetAsFirstSibling();
                destellos[i] = luz.GetComponent<Image>();
            }

        }

        RectTransform[] cartas;
        Image[] destellos;

        void Update()
        {
            if (destellos == null)
                return;

            float turno = Time.time * 0.55f;
            for (int i = 0; i < destellos.Length; i++)
            {
                float distancia = Mathf.Repeat(turno - i, destellos.Length);
                if (distancia > destellos.Length * 0.5f)
                    distancia = destellos.Length - distancia;
                float encendido = Mathf.Clamp01(1f - distancia);
                var color = destellos[i].color;
                color.a = encendido * 0.55f;
                destellos[i].color = color;
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
