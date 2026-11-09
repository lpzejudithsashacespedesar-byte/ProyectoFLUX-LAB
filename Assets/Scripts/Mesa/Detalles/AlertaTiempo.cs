using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// El texto del tiempo parpadea en rojo cuando queda poco para que se apague la receta.
    /// </summary>
    public class AlertaTiempo : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Preparar()
        {
            new GameObject("AlertaTiempo").AddComponent<AlertaTiempo>();
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
            for (int i = 0; i < textos.Length; i++)
            {
                if (textos[i] != null && textos[i].text.Contains("Recuerda"))
                {
                    aviso = textos[i];
                    colorQuieto = aviso.color;
                    break;
                }
            }

            if (aviso == null)
                Destroy(gameObject);

        }

        Text aviso;
        Color colorQuieto;
        bool marcado;

        void Update()
        {
            if (aviso == null)
                return;

            string texto = aviso.text;
            int dosPuntos = texto.IndexOf(':');
            float segundos = 99f;
            if (dosPuntos >= 0)
            {
                string numero = texto.Substring(dosPuntos + 1).Replace("s", "").Trim();
                float.TryParse(numero, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out segundos);
            }

            if (segundos > 0f && segundos < 1.5f)
            {
                aviso.color = Color.Lerp(colorQuieto, new Color(0.86f, 0.16f, 0.12f), Mathf.PingPong(Time.time * 4f, 1f));
                marcado = true;
            }
            else if (marcado)
            {
                aviso.color = colorQuieto;
                marcado = false;
            }
        }

    }
}
