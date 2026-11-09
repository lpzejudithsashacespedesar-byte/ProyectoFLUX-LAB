using UnityEngine;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Libro de logros. Las fichas entran con rebote, las abiertas respiran en oro
    /// y las cerradas se quedan quietas.
    /// </summary>
    public class PanelDeLogros : MonoBehaviour
    {
        RectTransform tarjeta;
        RectTransform[] fichas;
        Image[] maderas;
        Image[] medallas;
        Image[] candados;
        Image[] brillos;
        CanvasGroup grupo;
        float abiertaEn;
        bool visible;

        public bool EstaAbierto => visible;

        public static PanelDeLogros Crear(Transform padre)
        {
            var raiz = new GameObject("LibroDeLogros", typeof(RectTransform));
            raiz.transform.SetParent(padre, false);
            var libro = raiz.AddComponent<PanelDeLogros>();
            libro.Armar();
            raiz.SetActive(false);
            return libro;
        }

        void Armar()
        {
            var rect = GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            grupo = gameObject.AddComponent<CanvasGroup>();

            var velo = Nivel3UI.Imagen(transform, "Velo", Nivel3UI.Solido(), Vector2.zero, Vector2.zero);
            Nivel3UI.Estirar(velo.rectTransform);
            velo.color = new Color(0.05f, 0.03f, 0.02f, 0.74f);
            velo.raycastTarget = true;

            var panel = Nivel3UI.Panel(transform, "Tarjeta", Vector2.zero, new Vector2(1180f, 900f));
            tarjeta = panel.rectTransform;
            var sombra = panel.gameObject.AddComponent<Shadow>();
            sombra.effectColor = new Color(0f, 0f, 0f, 0.45f);
            sombra.effectDistance = new Vector2(0f, -16f);

            var placa = Nivel3UI.Placa(panel.transform, "placa_titulo", new Vector2(0f, 368f), new Vector2(620f, 140f));
            Nivel3UI.Texto(placa.transform, "Logros", 52, Hex("#f6efe2"), Vector2.zero, new Vector2(460f, 90f), TextAnchor.MiddleCenter, true);

            Nivel3UI.Texto(panel.transform, "En la Mesa del Aprendiz cada habilidad entra con un solo uso. Los otros talleres traen más.",
                22, Hex("#4a2c17"), new Vector2(0f, 286f), new Vector2(980f, 44f));

            Sprite madera = Nivel3UI.SpriteUI("ficha_logro");
            if (madera == null)
                madera = Nivel3UI.SpriteUI("placa_titulo");
            Sprite candado = Nivel3UI.SpriteUI("poder_cerrado");
            string[] artes = { "poder_mirada", "poder_lento", "poder_pista", "poder_deshacer", "poder_escudo" };

            int n = LogrosJuego.Todos.Length;
            fichas = new RectTransform[n];
            maderas = new Image[n];
            medallas = new Image[n];
            candados = new Image[n];
            brillos = new Image[n];
            for (int i = 0; i < n; i++)
            {
                float y = 168f - i * 112f;
                var ficha = Nivel3UI.Imagen(panel.transform, "Ficha" + i, madera, new Vector2(36f, y), new Vector2(900f, 128f), true);
                fichas[i] = ficha.rectTransform;
                maderas[i] = ficha;

                var luz = Nivel3UI.Imagen(ficha.transform, "Brillo", Nivel3UI.LuzSuave(), new Vector2(-392f, 0f), new Vector2(130f, 130f));
                luz.raycastTarget = false;
                brillos[i] = luz;

                Sprite medalla = Nivel3UI.SpriteUI(artes[i]);
                var icono = Nivel3UI.Imagen(ficha.transform, "Medalla", medalla, new Vector2(-392f, 4f), new Vector2(118f, 118f), true);
                medallas[i] = icono;

                LogrosJuego.Definicion def = LogrosJuego.Todos[i];
                var nombre = Nivel3UI.Texto(ficha.transform, def.nombre, 28, Hex("#fff6ea"), new Vector2(40f, 18f), new Vector2(480f, 36f), TextAnchor.MiddleLeft, true);
                nombre.horizontalOverflow = HorizontalWrapMode.Overflow;
                var detalle = Nivel3UI.Texto(ficha.transform, def.premio, 18, Hex("#f6efe2"), new Vector2(40f, -18f), new Vector2(560f, 32f), TextAnchor.MiddleLeft);
                detalle.horizontalOverflow = HorizontalWrapMode.Overflow;
                detalle.name = "Detalle";
                var como = Nivel3UI.Texto(ficha.transform, def.como, 18, Hex("#ffe08a"), new Vector2(40f, -18f), new Vector2(560f, 32f), TextAnchor.MiddleLeft);
                como.horizontalOverflow = HorizontalWrapMode.Overflow;
                como.name = "Como";

                if (candado != null)
                    candados[i] = Nivel3UI.Imagen(ficha.transform, "Candado", candado, new Vector2(390f, 0f), new Vector2(64f, 64f), true);
            }

            Nivel3UI.Boton(panel.transform, "Cerrar", new Vector2(0f, -400f), new Vector2(280f, 72f), () => Cerrar(), 30);
        }

        public void Abrir()
        {
            gameObject.SetActive(true);
            visible = true;
            abiertaEn = Time.unscaledTime;
            grupo.alpha = 0f;
            tarjeta.localScale = Vector3.one * 0.86f;
            for (int i = 0; i < fichas.Length; i++)
            {
                var pos = fichas[i].anchoredPosition;
                pos.x = 90f;
                fichas[i].anchoredPosition = pos;
            }
            Pintar();
        }

        public void Cerrar()
        {
            visible = false;
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (!visible)
                return;

            float edad = Time.unscaledTime - abiertaEn;
            float entrada = Salida(edad / 0.42f);
            grupo.alpha = Mathf.Clamp01(edad / 0.2f);
            tarjeta.localScale = Vector3.one * Mathf.Lerp(0.86f, 1f, entrada);

            for (int i = 0; i < fichas.Length; i++)
            {
                float propia = Salida((edad - 0.08f - i * 0.06f) / 0.4f);
                var pos = fichas[i].anchoredPosition;
                float baseX = 0f;
                pos.x = Mathf.Lerp(90f, baseX, propia);
                fichas[i].anchoredPosition = pos;

                bool abierto = LogrosJuego.Desbloqueado(LogrosJuego.Todos[i].poder);
                if (!abierto || brillos[i] == null)
                    continue;
                float pulso = 0.28f + Mathf.Sin(Time.unscaledTime * 2.4f + i) * 0.12f;
                var color = brillos[i].color;
                color.a = pulso;
                brillos[i].color = color;
                float lado = 110f + Mathf.Sin(Time.unscaledTime * 2.4f + i) * 10f;
                brillos[i].rectTransform.sizeDelta = new Vector2(lado, lado);
            }
        }

        void Pintar()
        {
            for (int i = 0; i < LogrosJuego.Todos.Length; i++)
            {
                bool abierto = LogrosJuego.Desbloqueado(LogrosJuego.Todos[i].poder);
                if (maderas[i] != null)
                    maderas[i].color = abierto ? Color.white : new Color(0.55f, 0.48f, 0.42f, 1f);
                if (medallas[i] != null)
                    medallas[i].color = abierto ? Color.white : new Color(0.32f, 0.28f, 0.24f, 1f);
                if (candados != null && candados[i] != null)
                    candados[i].gameObject.SetActive(!abierto);
                if (brillos[i] != null)
                    brillos[i].color = new Color(1f, 0.84f, 0.4f, abierto ? 0.4f : 0f);

                var como = fichas[i].Find("Como");
                if (como != null)
                    como.gameObject.SetActive(!abierto);
                var detalle = fichas[i].Find("Detalle");
                if (detalle != null)
                    detalle.gameObject.SetActive(abierto);
            }
        }

        static float Salida(float t)
        {
            t = Mathf.Clamp01(t);
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        static Color Hex(string valor)
        {
            ColorUtility.TryParseHtmlString(valor, out Color color);
            return color;
        }
    }
}
