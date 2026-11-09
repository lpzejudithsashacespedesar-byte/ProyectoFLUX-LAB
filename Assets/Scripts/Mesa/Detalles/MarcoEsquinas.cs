using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Cuatro esquinas doradas en el borde del taller del nivel 1.
    /// </summary>
    public class MarcoEsquinas : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Preparar()
        {
            new GameObject("MarcoEsquinas").AddComponent<MarcoEsquinas>();
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

            
            var lienzo = GameObject.Find("Canvas");
            if (lienzo == null)
            {
                Destroy(gameObject);
                yield break;
            }

            var padre = lienzo.transform;
            Esquina(padre, new Vector2(0f, 0f));
            Esquina(padre, new Vector2(1f, 0f));
            Esquina(padre, new Vector2(0f, 1f));
            Esquina(padre, new Vector2(1f, 1f));

        }

        
        static void Esquina(Transform padre, Vector2 ancla)
        {
            Barra(padre, ancla, new Vector2(64f, 6f));
            Barra(padre, ancla, new Vector2(6f, 64f));
        }

        static void Barra(Transform padre, Vector2 ancla, Vector2 tamano)
        {
            var go = new GameObject("Esquina", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(padre, false);
            go.transform.SetAsLastSibling();
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = ancla;
            rect.pivot = ancla;
            rect.anchoredPosition = new Vector2(ancla.x < 0.5f ? 18f : -18f, ancla.y < 0.5f ? 18f : -18f);
            rect.sizeDelta = tamano;
            var imagen = go.GetComponent<Image>();
            imagen.sprite = Cuadrado();
            imagen.color = new Color(0.93f, 0.76f, 0.38f, 0.9f);
            imagen.raycastTarget = false;
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
