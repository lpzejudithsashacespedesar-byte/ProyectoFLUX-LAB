using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FluxLab
{
    /// <summary>
    /// Camino de cinco talleres. Solo se entra al que ya está abierto.
    /// El camino, las luces y el polvo se mueven como el resto del laboratorio.
    /// </summary>
    public class MapaDeNiveles : MonoBehaviour
    {
        static readonly Vector2[] Puntos =
        {
            new Vector2(-730f, -238f),
            new Vector2(-576f, 173f),
            new Vector2(-77f, -86f),
            new Vector2(211f, 108f),
            new Vector2(634f, 216f)
        };

        static readonly Color Oro = new Color(1f, 0.84f, 0.46f, 1f);
        static readonly Color OroSuave = new Color(1f, 0.9f, 0.62f, 1f);

        struct Destello
        {
            public RectTransform rect;
            public Image imagen;
            public Vector2 velocidad;
            public float vida;
            public float total;
        }

        Font font;
        AudioSource sfx;
        int elegido;
        float nacio;
        float elegirEn = -4f;
        float largoCamino;
        readonly float[] tramo = new float[5];
        readonly RectTransform[] marcas = new RectTransform[5];
        readonly Image[] placas = new Image[5];
        readonly Image[] luces = new Image[5];
        readonly Text[] etiquetas = new Text[5];
        Text nombreDetalle;
        Text cuerpoDetalle;
        Image botonEntrar;
        Text textoEntrar;
        Image anillo;
        Image anilloFino;
        RectTransform tituloRt;
        RectTransform puntosRt;
        RectTransform fichaRt;
        RectTransform volverRt;
        Vector2 tituloBase;
        Vector2 puntosBase;
        Vector2 fichaBase;
        Vector2 volverBase;
        CanvasGroup fichaGrupo;
        RectTransform[] cuentas;
        float[] cuentaDist;
        RectTransform[] motas;
        float[] motaFase;
        RectTransform orbita;
        RectTransform[] orbitas;
        Destello[] destellos;
        Transform lienzo;

        void Awake()
        {
            nacio = Time.time;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            MedirCamino();
            Construir();
            elegido = Mathf.Clamp(AjustesJuego.NivelAlcanzado, 1, 5) - 1;
            Refrescar();
        }

        void Update()
        {
            float ahora = Time.time;
            AnimarEntrada(ahora);
            AnimarCamino(ahora);
            AnimarMarcas(ahora);
            AnimarOrbita(ahora);
            AnimarMotas(ahora);
            AnimarDestellos();
            AnimarFicha();

            if (Input.GetKeyDown(KeyCode.Escape))
                Volver();
        }

        void MedirCamino()
        {
            tramo[0] = 0f;
            for (int i = 1; i < Puntos.Length; i++)
                tramo[i] = tramo[i - 1] + Vector2.Distance(Puntos[i - 1], Puntos[i]);
            largoCamino = tramo[Puntos.Length - 1];
        }

        Vector2 EnCamino(float distancia, out int segmento)
        {
            segmento = 0;
            for (int i = 1; i < Puntos.Length; i++)
            {
                if (distancia <= tramo[i] || i == Puntos.Length - 1)
                {
                    segmento = i - 1;
                    float largo = Mathf.Max(0.001f, tramo[i] - tramo[i - 1]);
                    float t = Mathf.Clamp01((distancia - tramo[i - 1]) / largo);
                    return Vector2.Lerp(Puntos[i - 1], Puntos[i], t);
                }
            }

            return Puntos[Puntos.Length - 1];
        }

        void Construir()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            lienzo = canvasGo.transform;

            var fondo = ImagenRaw(lienzo, "Mapa", Leer("Menu/mapa.jpg"), Vector2.zero, new Vector2(1920f, 1080f));
            Stretch(fondo.rectTransform);

            CrearCamino();
            CrearMotas();

            var titulo = Nivel3UI.Placa(lienzo, "placa_titulo", new Vector2(0f, 448f), new Vector2(680f, 175f));
            tituloRt = titulo.rectTransform;
            tituloBase = tituloRt.anchoredPosition;
            var tituloTexto = Texto(titulo.transform, "Camino del taller", 42, Hex("#f6efe2"), FontStyle.Normal);
            tituloTexto.font = Nivel3UI.Fuente;
            tituloTexto.rectTransform.sizeDelta = new Vector2(520f, 90f);
            Contorno(tituloTexto, new Color(0.15f, 0.07f, 0.03f, 0.95f), 2.2f);

            var puntos = PlacaMadera(lienzo, "Puntos", new Vector2(755f, 458f), new Vector2(310f, 108f));
            puntosRt = puntos.rectTransform;
            puntosBase = puntosRt.anchoredPosition;
            var puntosTexto = Texto(puntos.transform, "Puntos: " + AjustesJuego.MejorPuntaje + "\nAbiertos: " + AjustesJuego.NivelAlcanzado + " / 5", 22, Hex("#fff6ea"), FontStyle.Normal);
            puntosTexto.font = Nivel3UI.Fuente;
            puntosTexto.rectTransform.sizeDelta = new Vector2(270f, 78f);
            Contorno(puntosTexto, new Color(0.15f, 0.07f, 0.03f, 0.95f), 1.6f);

            anillo = Imagen(lienzo, "Anillo", Nivel3UI.LuzSuave(), Puntos[0], new Vector2(210f, 210f));
            anillo.color = new Color(1f, 0.82f, 0.38f, 0.42f);
            anilloFino = Imagen(lienzo, "AnilloFino", Disco(96), Puntos[0], new Vector2(128f, 128f));
            anilloFino.color = new Color(1f, 0.9f, 0.55f, 0.7f);

            for (int i = 0; i < NivelesTaller.Lista.Length; i++)
            {
                var marca = new GameObject("Taller" + (i + 1), typeof(RectTransform));
                marca.transform.SetParent(lienzo, false);
                marcas[i] = marca.GetComponent<RectTransform>();
                marcas[i].anchorMin = marcas[i].anchorMax = new Vector2(0.5f, 0.5f);
                marcas[i].pivot = new Vector2(0.5f, 0.5f);
                marcas[i].anchoredPosition = Puntos[i];
                marcas[i].sizeDelta = new Vector2(250f, 180f);

                luces[i] = Imagen(marca.transform, "Luz", Nivel3UI.LuzSuave(), new Vector2(0f, -8f), new Vector2(190f, 190f));
                luces[i].color = new Color(1f, 0.78f, 0.35f, 0.28f);
                luces[i].transform.SetAsFirstSibling();

                placas[i] = PlacaMadera(marca.transform, "Placa", new Vector2(0f, -64f), new Vector2(236f, 82f));
                placas[i].raycastTarget = true;
                var boton = placas[i].gameObject.AddComponent<Button>();
                boton.targetGraphic = placas[i];
                boton.navigation = new Navigation { mode = Navigation.Mode.None };
                int capturado = i;
                boton.onClick.AddListener(() => Elegir(capturado));
                placas[i].gameObject.AddComponent<HoverScale>().scale = 1.06f;

                etiquetas[i] = Texto(placas[i].transform, NivelesTaller.Lista[i].corto, 28, Hex("#fff6ea"), FontStyle.Normal);
                etiquetas[i].font = Nivel3UI.Fuente;
                etiquetas[i].rectTransform.sizeDelta = new Vector2(200f, 64f);
                Contorno(etiquetas[i], new Color(0.15f, 0.07f, 0.03f, 0.95f), 1.8f);
            }

            CrearOrbita();
            CrearDestellos();

            var ficha = Nivel3UI.Panel(lienzo, "Ficha", new Vector2(30f, -462f), new Vector2(980f, 190f));
            fichaRt = ficha.rectTransform;
            fichaBase = fichaRt.anchoredPosition;
            fichaGrupo = ficha.gameObject.AddComponent<CanvasGroup>();
            var sombraFicha = ficha.gameObject.AddComponent<Shadow>();
            sombraFicha.effectColor = new Color(0f, 0f, 0f, 0.4f);
            sombraFicha.effectDistance = new Vector2(0f, -10f);

            var placaNombre = PlacaMadera(ficha.transform, "Nombre", new Vector2(-175f, 42f), new Vector2(500f, 88f));
            nombreDetalle = Texto(placaNombre.transform, "", 32, Hex("#fff6ea"), FontStyle.Normal);
            nombreDetalle.font = Nivel3UI.Fuente;
            nombreDetalle.rectTransform.sizeDelta = new Vector2(450f, 70f);
            Contorno(nombreDetalle, new Color(0.15f, 0.07f, 0.03f, 0.95f), 1.8f);

            cuerpoDetalle = Texto(ficha.transform, "", 22, Hex("#5a3b24"), FontStyle.Normal);
            cuerpoDetalle.font = Nivel3UI.Fuente;
            cuerpoDetalle.alignment = TextAnchor.MiddleLeft;
            cuerpoDetalle.rectTransform.anchoredPosition = new Vector2(-175f, -52f);
            cuerpoDetalle.rectTransform.sizeDelta = new Vector2(500f, 72f);

            botonEntrar = PlacaMadera(ficha.transform, "Entrar", new Vector2(350f, 8f), new Vector2(230f, 90f));
            botonEntrar.raycastTarget = true;
            var entrar = botonEntrar.gameObject.AddComponent<Button>();
            entrar.targetGraphic = botonEntrar;
            entrar.navigation = new Navigation { mode = Navigation.Mode.None };
            entrar.onClick.AddListener(Entrar);
            botonEntrar.gameObject.AddComponent<HoverScale>().scale = 1.04f;
            textoEntrar = Texto(botonEntrar.transform, "Entrar", 30, Hex("#fff6ea"), FontStyle.Normal);
            textoEntrar.font = Nivel3UI.Fuente;
            textoEntrar.rectTransform.sizeDelta = new Vector2(190f, 70f);
            Contorno(textoEntrar, new Color(0.15f, 0.07f, 0.03f, 0.95f), 1.8f);

            var volver = PlacaMadera(lienzo, "Volver", new Vector2(-800f, -462f), new Vector2(220f, 88f));
            volverRt = volver.rectTransform;
            volverBase = volverRt.anchoredPosition;
            volver.raycastTarget = true;
            var volverBoton = volver.gameObject.AddComponent<Button>();
            volverBoton.targetGraphic = volver;
            volverBoton.navigation = new Navigation { mode = Navigation.Mode.None };
            volverBoton.onClick.AddListener(Volver);
            volver.gameObject.AddComponent<HoverScale>().scale = 1.04f;
            var volverTexto = Texto(volver.transform, "Volver", 28, Hex("#fff6ea"), FontStyle.Normal);
            volverTexto.font = Nivel3UI.Fuente;
            volverTexto.rectTransform.sizeDelta = new Vector2(180f, 68f);
            Contorno(volverTexto, new Color(0.15f, 0.07f, 0.03f, 0.95f), 1.8f);

            PrepararMusica();
        }

        void CrearCamino()
        {
            const int cantidad = 32;
            cuentas = new RectTransform[cantidad];
            cuentaDist = new float[cantidad];
            for (int i = 0; i < cantidad; i++)
            {
                float d = largoCamino * (i + 0.5f) / cantidad;
                cuentaDist[i] = d;
                int segmento;
                Vector2 punto = EnCamino(d, out segmento);
                var cuenta = Imagen(lienzo, "Cuenta" + i, Nivel3UI.LuzSuave(), punto, new Vector2(26f, 26f));
                cuenta.color = new Color(1f, 0.82f, 0.42f, 0.2f);
                cuentas[i] = cuenta.rectTransform;
            }
        }

        void CrearMotas()
        {
            const int cantidad = 16;
            motas = new RectTransform[cantidad];
            motaFase = new float[cantidad];
            for (int i = 0; i < cantidad; i++)
            {
                float x = Random.Range(-900f, 900f);
                float y = Random.Range(-500f, 500f);
                var mota = Imagen(lienzo, "Polvo" + i, Nivel3UI.LuzSuave(), new Vector2(x, y), Vector2.one * Random.Range(10f, 22f));
                mota.color = new Color(1f, 0.9f, 0.65f, 0.25f);
                motas[i] = mota.rectTransform;
                motaFase[i] = Random.Range(0f, 6.28f);
            }
        }

        void CrearOrbita()
        {
            var go = new GameObject("Orbita", typeof(RectTransform));
            go.transform.SetParent(lienzo, false);
            orbita = go.GetComponent<RectTransform>();
            orbita.anchorMin = orbita.anchorMax = new Vector2(0.5f, 0.5f);
            orbita.sizeDelta = Vector2.zero;
            orbitas = new RectTransform[5];
            for (int i = 0; i < orbitas.Length; i++)
            {
                var luz = Imagen(orbita, "Chispa" + i, Nivel3UI.LuzSuave(), Vector2.zero, new Vector2(18f, 18f));
                luz.color = OroSuave;
                orbitas[i] = luz.rectTransform;
            }
        }

        void CrearDestellos()
        {
            destellos = new Destello[18];
            for (int i = 0; i < destellos.Length; i++)
            {
                var imagen = Imagen(lienzo, "Destello" + i, Nivel3UI.LuzSuave(), new Vector2(4000f, 4000f), new Vector2(22f, 22f));
                imagen.color = new Color(1f, 0.9f, 0.55f, 0f);
                destellos[i].rect = imagen.rectTransform;
                destellos[i].imagen = imagen;
            }
        }

        void AnimarEntrada(float ahora)
        {
            float edad = ahora - nacio;
            float placa = SalidaSuave(edad / 0.55f);
            if (tituloRt != null)
            {
                float flotando = Mathf.Sin(ahora * 1.25f) * 5f;
                tituloRt.anchoredPosition = tituloBase + new Vector2(0f, (1f - placa) * 70f + flotando);
            }

            if (puntosRt != null)
                puntosRt.anchoredPosition = puntosBase + new Vector2((1f - SalidaSuave((edad - 0.08f) / 0.5f)) * 80f, 0f);
            if (volverRt != null)
                volverRt.anchoredPosition = volverBase + new Vector2((1f - SalidaSuave((edad - 0.12f) / 0.5f)) * -80f, 0f);
            if (fichaRt != null)
                fichaRt.anchoredPosition = fichaBase + new Vector2(0f, (1f - SalidaSuave((edad - 0.16f) / 0.55f)) * -90f);
        }

        void AnimarCamino(float ahora)
        {
            float avance = Mathf.Repeat(ahora * 0.16f, 1f);
            for (int i = 0; i < cuentas.Length; i++)
            {
                int segmento;
                EnCamino(cuentaDist[i], out segmento);
                bool vivo = AjustesJuego.NivelAlcanzado >= segmento + 2;
                float lugar = cuentaDist[i] / Mathf.Max(1f, largoCamino);
                float cerca = 1f - Mathf.Abs(Mathf.Repeat(lugar - avance + 0.5f, 1f) - 0.5f) * 7f;
                cerca = Mathf.Clamp01(cerca);
                cerca = cerca * cerca;
                float respirar = 0.55f + 0.45f * Mathf.Sin(ahora * 2.4f + i * 0.45f);
                var color = vivo ? Oro : new Color(0.55f, 0.42f, 0.28f, 1f);
                color.a = vivo ? 0.18f + 0.72f * cerca : 0.08f + 0.12f * respirar;
                var imagen = cuentas[i].GetComponent<Image>();
                imagen.color = color;
                float tam = vivo ? 18f + 16f * cerca : 14f;
                cuentas[i].sizeDelta = new Vector2(tam, tam);
                int ignorado;
                Vector2 camino = EnCamino(cuentaDist[i], out ignorado);
                cuentas[i].anchoredPosition = camino + new Vector2(0f, Mathf.Sin(ahora * 1.6f + i) * (vivo ? 3.5f : 1f));
            }
        }

        void AnimarMarcas(float ahora)
        {
            for (int i = 0; i < marcas.Length; i++)
            {
                if (marcas[i] == null)
                    continue;

                bool libre = i + 1 <= AjustesJuego.NivelAlcanzado;
                bool activo = i == elegido;
                float entrada = SalidaSuave((ahora - nacio - 0.05f - i * 0.07f) / 0.48f);
                float bob = activo ? Mathf.Sin(ahora * 2.1f + i) * 7f : Mathf.Sin(ahora * 1.15f + i * 1.3f) * (libre ? 3.5f : 1.4f);
                marcas[i].anchoredPosition = Puntos[i] + new Vector2(Mathf.Sin(ahora * 0.7f + i) * (activo ? 2.5f : 1f), bob);

                float pop = 1f;
                if (activo)
                {
                    float edad = ahora - elegirEn;
                    pop += 0.16f * Mathf.Sin(edad * 17f) * Mathf.Exp(-edad * 5.2f);
                    pop += Mathf.Sin(ahora * 2.3f) * 0.025f;
                }

                marcas[i].localScale = new Vector3(entrada * pop, entrada * pop, 1f);

                if (luces[i] == null)
                    continue;
                float brillo = libre
                    ? (activo ? 0.42f + Mathf.Sin(ahora * 2.6f) * 0.16f : 0.16f + Mathf.Sin(ahora * 1.4f + i) * 0.05f)
                    : 0.05f;
                luces[i].color = libre ? new Color(1f, 0.78f, 0.35f, brillo) : new Color(0.7f, 0.66f, 0.58f, brillo);
                float lado = activo ? 210f + Mathf.Sin(ahora * 2.6f) * 16f : 150f;
                luces[i].rectTransform.sizeDelta = new Vector2(lado, lado);
            }

            if (anillo == null || marcas[elegido] == null)
                return;

            anillo.transform.SetParent(marcas[elegido], false);
            anillo.rectTransform.anchoredPosition = new Vector2(0f, -6f);
            anillo.rectTransform.SetAsFirstSibling();
            float pulso = 1f + Mathf.Sin(ahora * 2.4f) * 0.08f;
            anillo.rectTransform.localScale = new Vector3(pulso, pulso, 1f);
            anillo.color = new Color(1f, 0.82f, 0.38f, 0.28f + Mathf.Sin(ahora * 2.4f) * 0.1f);

            anilloFino.transform.SetParent(marcas[elegido], false);
            anilloFino.rectTransform.anchoredPosition = new Vector2(0f, -6f);
            anilloFino.rectTransform.localRotation = Quaternion.Euler(0f, 0f, ahora * 24f);
            anilloFino.rectTransform.localScale = Vector3.one * (0.92f + Mathf.Sin(ahora * 2.4f) * 0.06f);
        }

        void AnimarOrbita(float ahora)
        {
            if (orbita == null || marcas[elegido] == null)
                return;

            bool libre = elegido + 1 <= AjustesJuego.NivelAlcanzado;
            orbita.SetParent(marcas[elegido], false);
            orbita.SetAsLastSibling();
            orbita.anchoredPosition = new Vector2(0f, -6f);
            orbita.localRotation = Quaternion.Euler(0f, 0f, ahora * (libre ? 26f : 8f));
            for (int i = 0; i < orbitas.Length; i++)
            {
                float ang = i * Mathf.PI * 2f / orbitas.Length;
                float radio = (libre ? 86f : 70f) + Mathf.Sin(ahora * 2f + i) * 5f;
                orbitas[i].anchoredPosition = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radio;
                var imagen = orbitas[i].GetComponent<Image>();
                float a = libre ? 0.55f + 0.35f * Mathf.Sin(ahora * 4f + i) : 0.12f;
                imagen.color = new Color(1f, 0.92f, 0.62f, Mathf.Abs(a));
            }
        }

        void AnimarMotas(float ahora)
        {
            for (int i = 0; i < motas.Length; i++)
            {
                var pos = motas[i].anchoredPosition;
                pos.y += (12f + (i % 4) * 4f) * Time.deltaTime;
                pos.x += Mathf.Sin(ahora * 0.85f + motaFase[i]) * 10f * Time.deltaTime;
                if (pos.y > 560f)
                {
                    pos.y = -540f;
                    pos.x = Random.Range(-900f, 900f);
                }

                motas[i].anchoredPosition = pos;
                var imagen = motas[i].GetComponent<Image>();
                float a = 0.08f + 0.22f * (0.5f + 0.5f * Mathf.Sin(ahora * 1.3f + motaFase[i]));
                imagen.color = new Color(1f, 0.9f, 0.66f, a);
            }
        }

        void AnimarDestellos()
        {
            for (int i = 0; i < destellos.Length; i++)
            {
                if (destellos[i].vida <= 0f)
                    continue;
                destellos[i].vida -= Time.deltaTime;
                destellos[i].velocidad.y -= 40f * Time.deltaTime;
                destellos[i].rect.anchoredPosition += destellos[i].velocidad * Time.deltaTime;
                float t = Mathf.Clamp01(destellos[i].vida / destellos[i].total);
                var color = destellos[i].imagen.color;
                color.a = t;
                destellos[i].imagen.color = color;
                float lado = 10f + 20f * t;
                destellos[i].rect.sizeDelta = new Vector2(lado, lado);
            }
        }

        void AnimarFicha()
        {
            if (fichaGrupo == null)
                return;
            fichaGrupo.alpha = Mathf.MoveTowards(fichaGrupo.alpha, 1f, Time.deltaTime * 3.2f);
        }

        void SoltarDestellos(Vector2 origen)
        {
            int lanzados = 0;
            for (int i = 0; i < destellos.Length && lanzados < 10; i++)
            {
                if (destellos[i].vida > 0f)
                    continue;
                float ang = Random.Range(0f, 6.28f);
                float rapidez = Random.Range(70f, 180f);
                destellos[i].velocidad = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * rapidez;
                destellos[i].vida = destellos[i].total = Random.Range(0.45f, 0.8f);
                destellos[i].rect.anchoredPosition = origen + Random.insideUnitCircle * 12f;
                destellos[i].imagen.color = Color.Lerp(Oro, Color.white, Random.value);
                destellos[i].rect.SetAsLastSibling();
                lanzados++;
            }
        }

        static float SalidaSuave(float t)
        {
            t = Mathf.Clamp01(t);
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        void Elegir(int indice)
        {
            if (indice == elegido)
                return;
            elegido = indice;
            elegirEn = Time.time;
            if (fichaGrupo != null)
                fichaGrupo.alpha = 0.15f;
            SoltarDestellos(Puntos[indice]);
            SonarClick();
            Refrescar();
        }

        void Refrescar()
        {
            Taller taller = NivelesTaller.Lista[elegido];
            bool abierto = elegido + 1 <= AjustesJuego.NivelAlcanzado;
            nombreDetalle.text = taller.nombre;
            cuerpoDetalle.text = taller.detalle + "\n" +
                taller.receta + " ingredientes  ·  " + taller.rondas + " rondas  ·  " +
                taller.segundos.ToString("0.0") + " s" +
                (abierto ? "" : "\nSe abre al terminar el taller anterior.");
            textoEntrar.text = abierto ? "Entrar" : "Cerrado";
            botonEntrar.color = abierto ? Color.white : new Color(0.65f, 0.65f, 0.65f, 1f);
            botonEntrar.GetComponent<Button>().interactable = abierto;

            for (int i = 0; i < placas.Length; i++)
            {
                bool libre = i + 1 <= AjustesJuego.NivelAlcanzado;
                bool activo = i == elegido;
                placas[i].color = !libre
                    ? new Color(0.55f, 0.48f, 0.4f, 1f)
                    : activo ? Oro : Color.white;
                etiquetas[i].text = libre ? NivelesTaller.Lista[i].corto : "Nv. " + (i + 1);
                etiquetas[i].color = libre ? Hex("#fff6ea") : new Color(0.82f, 0.76f, 0.68f, 1f);
            }
        }

        void Entrar()
        {
            if (elegido + 1 > AjustesJuego.NivelAlcanzado)
                return;
            SonarClick();
            AjustesJuego.ElegirNivel(elegido + 1);
            SceneManager.LoadScene(NivelesTaller.EscenaDe(elegido + 1));
        }

        void Volver()
        {
            SceneManager.LoadScene("MenuPrincipal");
        }

        void SonarClick()
        {
            if (sfx == null || sfx.clip == null || !AjustesJuego.EfectosActivos)
                return;
            sfx.Play();
        }

        void PrepararMusica()
        {
            var clip = Resources.Load<AudioClip>("FluxLab/Audio/menu");
            if (clip != null)
            {
                var fuente = gameObject.AddComponent<AudioSource>();
                fuente.clip = clip;
                fuente.loop = true;
                fuente.playOnAwake = false;
                fuente.volume = AjustesJuego.MusicaActiva ? 0.42f : 0f;
                fuente.Play();
            }

            var click = Resources.Load<AudioClip>("FluxLab/Audio/click");
            if (click == null)
                return;
            sfx = gameObject.AddComponent<AudioSource>();
            sfx.clip = click;
            sfx.playOnAwake = false;
            sfx.volume = 0.55f;
        }

        RawImage ImagenRaw(Transform padre, string nombre, Texture2D textura, Vector2 posicion, Vector2 tamano)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            go.transform.SetParent(padre, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = posicion;
            rect.sizeDelta = tamano;
            var imagen = go.GetComponent<RawImage>();
            imagen.texture = textura;
            imagen.raycastTarget = false;
            return imagen;
        }

        Image Imagen(Transform padre, string nombre, Sprite sprite, Vector2 posicion, Vector2 tamano)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(padre, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = posicion;
            rect.sizeDelta = tamano;
            var imagen = go.GetComponent<Image>();
            imagen.sprite = sprite;
            imagen.raycastTarget = false;
            return imagen;
        }

        Text Texto(Transform padre, string contenido, int tamano, Color color, FontStyle estilo)
        {
            var go = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(padre, false);
            var texto = go.GetComponent<Text>();
            texto.font = font;
            texto.text = contenido;
            texto.fontSize = tamano;
            texto.fontStyle = estilo;
            texto.color = color;
            texto.alignment = TextAnchor.MiddleCenter;
            texto.horizontalOverflow = HorizontalWrapMode.Wrap;
            texto.verticalOverflow = VerticalWrapMode.Overflow;
            texto.raycastTarget = false;
            texto.rectTransform.sizeDelta = new Vector2(300f, 40f);
            return texto;
        }

        Image PlacaMadera(Transform padre, string nombre, Vector2 posicion, Vector2 tamano)
        {
            Sprite madera = Nivel3UI.SpriteUI("boton_ancho");
            var imagen = Imagen(padre, nombre, madera != null ? madera : SpriteRecurso("button_brown"), posicion, tamano);
            imagen.type = Image.Type.Sliced;
            if (madera != null)
                imagen.pixelsPerUnitMultiplier = tamano.y >= 100f ? 1.2f : 1.45f;
            var sombra = imagen.gameObject.AddComponent<Shadow>();
            sombra.effectColor = new Color(0.05f, 0.02f, 0.01f, 0.42f);
            sombra.effectDistance = new Vector2(0f, -5f);
            return imagen;
        }

        static void Contorno(Text texto, Color color, float grosor)
        {
            var contorno = texto.gameObject.AddComponent<Outline>();
            contorno.effectColor = color;
            contorno.effectDistance = new Vector2(grosor, -grosor);
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        Texture2D Leer(string relativo)
        {
            string ruta = Path.Combine(Application.streamingAssetsPath, relativo);
            var textura = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (File.Exists(ruta))
                textura.LoadImage(File.ReadAllBytes(ruta));
            textura.filterMode = FilterMode.Bilinear;
            return textura;
        }

        static Sprite SpriteRecurso(string nombre)
        {
            return Resources.Load<Sprite>("FluxLab/" + nombre);
        }

        static Sprite Disco(int lado)
        {
            var textura = new Texture2D(lado, lado, TextureFormat.RGBA32, false);
            float radio = (lado - 1) * 0.5f;
            var centro = new Vector2(radio, radio);
            for (int y = 0; y < lado; y++)
            {
                for (int x = 0; x < lado; x++)
                {
                    float distancia = Vector2.Distance(new Vector2(x, y), centro) / radio;
                    float borde = 1f - Mathf.Abs(distancia - 0.72f) / 0.16f;
                    float alpha = Mathf.Clamp01(borde);
                    textura.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
                }
            }

            textura.Apply();
            return Sprite.Create(textura, new Rect(0f, 0f, lado, lado), new Vector2(0.5f, 0.5f), 100f);
        }

        static Color Hex(string valor)
        {
            ColorUtility.TryParseHtmlString(valor, out Color color);
            return color;
        }
    }
}
