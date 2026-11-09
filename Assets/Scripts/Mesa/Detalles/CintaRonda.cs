using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Línea dorada debajo del texto de la ronda del nivel 1.
    /// </summary>
    public class CintaRonda : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Preparar()
        {
            new GameObject("CintaRonda").AddComponent<CintaRonda>();
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
            var textos = FindObjectsByType<Text>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            Text ronda = null;
            for (int i = 0; i < textos.Length; i++)
            {
                if (textos[i] != null && textos[i].text.StartsWith("Ronda"))
                {
                    ronda = textos[i];
                    break;
                }
            }

            if (ronda == null)
            {
                Destroy(gameObject);
                yield break;
            }

            var origen = ronda.rectTransform;
            linea = Imagen(origen.parent, "CintaRonda", Cuadrado(), new Vector2(origen.sizeDelta.x * 0.62f, 4f), new Color(0.93f, 0.76f, 0.38f, 0.95f));
            linea.anchorMin = origen.anchorMin;
            linea.anchorMax = origen.anchorMax;
            linea.pivot = origen.pivot;
            linea.anchoredPosition = origen.anchoredPosition + new Vector2(0f, -origen.sizeDelta.y * 0.42f);

        }

        RectTransform linea;

        void Update()
        {
            if (linea == null)
                return;

            var imagen = linea.GetComponent<Image>();
            var color = imagen.color;
            color.a = 0.45f + Mathf.Sin(Time.time * 3f) * 0.4f;
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

        static Sprite Cuadrado()
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            return Sprite.Create(tex, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 100f);
        }

    }
}
