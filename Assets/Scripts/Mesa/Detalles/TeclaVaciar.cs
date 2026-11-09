using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// La tecla V vacía el caldero del nivel 1.
    /// </summary>
    public class TeclaVaciar : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Preparar()
        {
            new GameObject("TeclaVaciar").AddComponent<TeclaVaciar>();
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

            
            boton = GameObject.Find("Boton_Vaciar");
            if (boton == null)
            {
                Destroy(gameObject);
                yield break;
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go = new GameObject("LetraV", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(boton.transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-12f, -8f);
            rect.sizeDelta = new Vector2(36f, 32f);
            var letra = go.GetComponent<Text>();
            letra.font = font;
            letra.text = "V";
            letra.fontSize = 22;
            letra.alignment = TextAnchor.MiddleCenter;
            letra.color = new Color(1f, 0.9f, 0.65f, 1f);
            letra.raycastTarget = false;
            listo = true;

        }

        GameObject boton;
        bool listo;

        void Update()
        {
            if (!listo || boton == null)
                return;
            if (!Input.GetKeyDown(KeyCode.V))
                return;

            var click = boton.GetComponent<Button>();
            if (click != null)
                click.onClick.Invoke();
        }

    }
}
