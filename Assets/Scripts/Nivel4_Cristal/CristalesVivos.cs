using System.Collections;
using System.Collections.Generic;
using FluxLab.Nivel2;
using UnityEngine;
using UnityEngine.UI;

namespace FluxLab.Nivel4
{
    /// <summary>
    /// La animación del fondo de la Torre de Cristal: va encima del dibujo y detrás del juego (Magnus, caldero, HUD).
    ///  - Los cristales laten con luz morada y azul; a veces uno suelta un destello.
    ///  - Faroles y velas titilan como una llama.
    ///  - El círculo mágico del suelo respira; brilla fuerte al acertar y parpadea en rojo al fallar.
    ///  - Motas de luz suben desde el suelo y titilan destellos en la cueva.
    ///  - Rayos de luz bajan desde la cueva y se mecen despacio; niebla mágica pasa por el suelo y el lago.
    ///  - Fragmentos de cristal con colores de prisma suben girando desde los racimos.
    ///  - De vez en cuando una punta de cristal suelta una estrella grande con un anillo de luz.
    ///  - Al acertar, los cristales se encienden en cadena desde el caldero hacia afuera.
    ///  - Al fallar, los cristales se apagan en cadena (de afuera hacia el caldero) y vuelven solos,
    ///    caen fragmentos de cristal que se rompen en el suelo, y Mishi (Gatitos/MishiAnimado), que descansa
    ///    en la cómoda, lanza su bola mágica (Gatitos/BolaMagica) hasta la carta del ingrediente que iba.
    ///  - La mariposa del Jardín de Luna (MariposaAnimada) vuela por la torre, se posa en los cristales
    ///    y en el borde del caldero, y acude al caldero cuando aciertas. Vuela detrás de la receta: nunca la tapa.
    /// Usa las piezas del nivel 2 (ParticulasUI, FormasUI, EscenaFrascos.Imagen) y del nivel 3 (MariposaAnimada);
    /// las posiciones están en FondoNivel4.cs.
    /// Se apaga con "Fondo Vivo" en el Inspector de MesaDeFrascos.
    /// </summary>
    public class CristalesVivos : MonoBehaviour
    {
        class Luz
        {
            public Image imagen;
            public float tamano;
            public float fase;
            public Color color;
            public float extra;     // luz de más cuando le llega la cadena al acertar (baja sola)
            public float apagado;   // se apaga cuando le llega la cadena al fallar (vuelve sola)
        }

        class Caida
        {
            public RectTransform rect;
            public Vector2 velocidad;
            public float giro;
            public float suelo;
        }

        static readonly Color Apagado = new Color(0.35f, 0.06f, 0.14f, 1f);

        class Rayo
        {
            public Image imagen;
            public float angulo;
            public float fase;
        }

        class Niebla
        {
            public Image imagen;
            public float y;
            public float velocidad;
            public float fase;
        }

        // Colores de prisma para los fragmentos de cristal.
        static readonly Color[] Prisma =
        {
            new Color(0.72f, 0.45f, 1f, 1f),
            new Color(0.45f, 0.65f, 1f, 1f),
            new Color(0.5f, 0.98f, 1f, 1f),
            new Color(1f, 0.6f, 0.95f, 1f),
            new Color(0.92f, 0.9f, 1f, 1f)
        };

        static readonly Color Morado = new Color(0.68f, 0.38f, 1f, 1f);
        static readonly Color Azul = new Color(0.35f, 0.6f, 1f, 1f);
        static readonly Color Calido = new Color(1f, 0.7f, 0.3f, 1f);
        static readonly Color Rojo = new Color(1f, 0.25f, 0.3f, 1f);
        static readonly Color Cian = new Color(0.45f, 0.95f, 1f, 1f);
        static readonly Color LuzRayo = new Color(0.78f, 0.72f, 1f, 1f);

        EscenaFrascos escena;
        ConfigFrascos config;
        ParticulasUI chispas;
        MariposaAnimada mariposa;
        MishiAnimado mishi;
        RectTransform capaBola;
        ParticulasUI chispasBola;
        readonly List<Caida> caidas = new List<Caida>();
        readonly List<Luz> cristales = new List<Luz>();
        readonly List<Luz> faroles = new List<Luz>();
        Image circulo;
        Image anilloCirculo;
        Vector2 tamanoCirculo;
        Color colorFlash = Cian;
        float flash;            // 1 justo al acertar o fallar, baja a 0
        float relojMota;
        float relojDestello;
        float relojCueva;
        float relojFragmento;
        float relojEstrella = 1.5f;
        readonly List<Rayo> rayos = new List<Rayo>();
        readonly List<Niebla> nieblas = new List<Niebla>();

        public void Construir(EscenaFrascos escena, ConfigFrascos config)
        {
            this.escena = escena;
            this.config = config;
            var raiz = GetComponent<RectTransform>();
            raiz.anchorMin = Vector2.zero;
            raiz.anchorMax = Vector2.one;
            raiz.offsetMin = Vector2.zero;
            raiz.offsetMax = Vector2.zero;

            // Rayos de luz que bajan desde la cueva, en abanico (van primero: quedan detrás de todo lo demás).
            Vector2 origen = FondoNivel4.ACanvas(FondoNivel4.OrigenRayos.x, FondoNivel4.OrigenRayos.y);
            float[] angulos = { -42f, -22f, -4f, 15f, 34f };
            for (int i = 0; i < angulos.Length; i++)
            {
                var rayo = escena.Imagen(transform, "Rayo", FormasUI.Humo, origen, new Vector2(Random.Range(100f, 150f), Random.Range(760f, 900f)), false);
                rayo.rectTransform.pivot = new Vector2(0.5f, 1f);
                rayo.color = Transparente(LuzRayo, 0f);
                rayos.Add(new Rayo { imagen = rayo, angulo = angulos[i], fase = i * 1.9f });
            }

            // Niebla mágica que pasa despacio por el lago de la cueva y por el suelo.
            float[] alturas = { FondoNivel4.ACanvas(0f, 450f).y, -170f, -290f };
            for (int i = 0; i < alturas.Length; i++)
            {
                var niebla = escena.Imagen(transform, "Niebla", FormasUI.Humo, new Vector2(Random.Range(-900f, 900f), alturas[i]), new Vector2(Random.Range(700f, 950f), Random.Range(150f, 220f)), false);
                niebla.color = Transparente(i % 2 == 0 ? Morado : Azul, 0f);
                nieblas.Add(new Niebla { imagen = niebla, y = alturas[i], velocidad = Random.Range(14f, 28f) * (i % 2 == 0 ? 1f : -1f), fase = i * 2.3f });
            }

            // El círculo mágico del suelo: un resplandor y un anillo aplastados en perspectiva.
            Vector4 c = FondoNivel4.Circulo;
            Vector2 centro = FondoNivel4.ACanvas(c.x, c.y);
            tamanoCirculo = new Vector2(c.z * 2f, c.w * 2f) * FondoNivel4.Escala;
            circulo = escena.Imagen(transform, "BrilloCirculo", FormasUI.Humo, centro, tamanoCirculo * 1.05f, false);
            circulo.color = Transparente(Azul, 0f);
            anilloCirculo = escena.Imagen(transform, "AnilloCirculo", FormasUI.Anillo, centro, tamanoCirculo, false);
            anilloCirculo.color = Transparente(Cian, 0f);

            for (int i = 0; i < FondoNivel4.Cristales.Length; i++)
                cristales.Add(NuevaLuz("BrilloCristal", FondoNivel4.Cristales[i], i % 2 == 0 ? Morado : Azul, i));
            for (int i = 0; i < FondoNivel4.Faroles.Length; i++)
                faroles.Add(NuevaLuz("BrilloFarol", FondoNivel4.Faroles[i], Calido, i));

            var capa = new GameObject("Chispas", typeof(RectTransform));
            capa.transform.SetParent(transform, false);
            chispas = capa.AddComponent<ParticulasUI>();

            escena.AlAcertar += Acerto;
            escena.AlFallar += Fallo;
            escena.AlDudar += Dudo;
            escena.AlContar += Conto;
        }

        // La mariposa se crea cuando ya está toda la pantalla armada, para ponerla en su capa:
        // delante del fondo, Magnus y el caldero, pero detrás de la receta, la bandeja y el HUD.
        void Start()
        {
            if (escena.Caldero == null)
                return;
            Transform lienzo = escena.Caldero.transform.parent;
            var capa = new GameObject("CapaMariposa", typeof(RectTransform)).GetComponent<RectTransform>();
            capa.SetParent(lienzo, false);
            capa.anchorMin = Vector2.zero;
            capa.anchorMax = Vector2.one;
            capa.offsetMin = Vector2.zero;
            capa.offsetMax = Vector2.zero;
            capa.SetSiblingIndex(escena.Caldero.transform.GetSiblingIndex() + 1);

            var posaderos = new Vector2[FondoNivel4.PosaderosMariposa.Length + 1];
            posaderos[0] = CalderoVivo.Boca + new Vector2(130f, 16f);   // borde del caldero
            for (int i = 0; i < FondoNivel4.PosaderosMariposa.Length; i++)
                posaderos[i + 1] = FondoNivel4.ACanvas(FondoNivel4.PosaderosMariposa[i].x, FondoNivel4.PosaderosMariposa[i].y);
            mariposa = MariposaAnimada.Crear(capa, posaderos);

            // Mishi descansa en la cómoda de la derecha (tranquila: no lanza bolas sola, solo cuando ayuda).
            var plataformas = new MishiAnimado.Plataforma[FondoNivel4.PlataformasMishi.Length];
            for (int i = 0; i < plataformas.Length; i++)
            {
                Vector3 p = FondoNivel4.PlataformasMishi[i];
                Vector2 izq = FondoNivel4.ACanvas(p.x, p.z);
                Vector2 der = FondoNivel4.ACanvas(p.y, p.z);
                plataformas[i] = new MishiAnimado.Plataforma(izq.x, der.x, izq.y);
            }
            mishi = MishiAnimado.Crear(capa, plataformas);
            mishi.escala = 1.2f;
            mishi.Tranquila = true;

            // La bola que señala la carta va por encima de la bandeja (pero debajo de las ventanas de pausa y resultados).
            int encimaDeLaBandeja = escena.Caldero.transform.GetSiblingIndex();
            foreach (IngredientKind tipo in RecetaFrascos.Todos)
            {
                RectTransform carta = escena.CartaDe(tipo);
                if (carta != null)
                    encimaDeLaBandeja = Mathf.Max(encimaDeLaBandeja, carta.GetSiblingIndex());
            }
            capaBola = new GameObject("CapaBolaMishi", typeof(RectTransform)).GetComponent<RectTransform>();
            capaBola.SetParent(lienzo, false);
            capaBola.anchorMin = Vector2.zero;
            capaBola.anchorMax = Vector2.one;
            capaBola.offsetMin = Vector2.zero;
            capaBola.offsetMax = Vector2.zero;
            capaBola.SetSiblingIndex(encimaDeLaBandeja + 1);
            chispasBola = capaBola.gameObject.AddComponent<ParticulasUI>();
        }

        void OnDestroy()
        {
            if (escena == null)
                return;
            escena.AlAcertar -= Acerto;
            escena.AlFallar -= Fallo;
            escena.AlDudar -= Dudo;
            escena.AlContar -= Conto;
        }

        Luz NuevaLuz(string nombre, Vector3 dato, Color color, int i)
        {
            float lado = dato.z * FondoNivel4.Escala * 2f;
            var imagen = escena.Imagen(transform, nombre, FormasUI.Humo, FondoNivel4.ACanvas(dato.x, dato.y), new Vector2(lado, lado), false);
            imagen.color = Transparente(color, 0f);
            return new Luz { imagen = imagen, tamano = lado, fase = i * 1.37f, color = color };
        }

        // ───────────────────────── Reacciones al juego ─────────────────────────

        void Acerto()
        {
            colorFlash = Cian;
            flash = 1f;
            // Un anillo de luz sale del caldero hacia el borde del círculo y los cristales se encienden en cadena.
            Vector2 centro = anilloCirculo.rectTransform.anchoredPosition;
            chispas.Una(FormasUI.Anillo, centro, tamanoCirculo * 0.3f, Transparente(Cian, 0.9f), Vector2.zero, 0.9f, 0f, 0f, 2.4f);
            StartCoroutine(Resonancia());

            // De premio, la mariposa acude a revolotear sobre el caldero (como en el Jardín de Luna).
            if (mariposa != null)
                mariposa.Atraer(CalderoVivo.Boca + new Vector2(0f, 90f), true);
        }

        void Fallo()
        {
            colorFlash = Rojo;
            flash = 1f;
            StartCoroutine(Apagon());
            StartCoroutine(LluviaDeFragmentos());
            if (escena.IngredienteCorrecto.HasValue)
                StartCoroutine(MishiAyuda(escena.IngredienteCorrecto.Value));
        }

        // Conteo 3, 2, 1: con cada número la torre late del color del número; en el "¡ya!" se enciende toda en cadena.
        void Conto(int numero)
        {
            if (numero > 0)
            {
                Color[] colores = FondoNivel4.Datos.coloresConteo;
                colorFlash = colores != null && colores.Length >= 3 ? colores[Mathf.Clamp(3 - numero, 0, 2)] : Cian;
                flash = 0.7f;
                chispas.Una(FormasUI.Anillo, anilloCirculo.rectTransform.anchoredPosition, tamanoCirculo * 0.5f, Transparente(colorFlash, 0.7f), Vector2.zero, 0.7f, 0f, 0f, 1.2f);
                return;
            }

            colorFlash = Cian;
            flash = 1f;
            chispas.Una(FormasUI.Anillo, anilloCirculo.rectTransform.anchoredPosition, tamanoCirculo * 0.3f, Transparente(Cian, 0.9f), Vector2.zero, 0.9f, 0f, 0f, 2.4f);
            StartCoroutine(Resonancia());
        }

        // Pista por duda: Mishi lanza su bola a la carta del siguiente ingrediente.
        void Dudo(IngredientKind siguiente)
        {
            StartCoroutine(MishiAyuda(siguiente));
        }

        // Los cristales se apagan uno tras otro, del más lejano al caldero al más cercano.
        IEnumerator Apagon()
        {
            Vector2 boca = CalderoVivo.Boca;
            var orden = new List<Luz>(cristales);
            orden.Sort((a, b) => Vector2.Distance(b.imagen.rectTransform.anchoredPosition, boca)
                .CompareTo(Vector2.Distance(a.imagen.rectTransform.anchoredPosition, boca)));
            foreach (Luz cristal in orden)
            {
                cristal.apagado = 1f;
                cristal.extra = 0f;
                yield return new WaitForSeconds(0.06f);
            }
        }

        // Caen fragmentos de cristal desde arriba y se rompen en chispitas al llegar al suelo.
        IEnumerator LluviaDeFragmentos()
        {
            for (int i = 0; i < 16; i++)
            {
                float ancho = Random.Range(12f, 22f);
                var imagen = escena.Imagen(chispas.transform, "Fragmento", FormasUI.Cristal,
                    new Vector2(Random.Range(-820f, 820f), Random.Range(560f, 640f)), new Vector2(ancho, ancho * 2.1f), false);
                imagen.color = Transparente(Prisma[Random.Range(0, Prisma.Length)], 0.85f);
                imagen.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                caidas.Add(new Caida
                {
                    rect = imagen.rectTransform,
                    velocidad = new Vector2(Random.Range(-40f, 40f), Random.Range(-80f, 0f)),
                    giro = Random.Range(-260f, 260f),
                    suelo = Random.Range(-330f, -150f)
                });
                yield return new WaitForSeconds(Random.Range(0.03f, 0.08f));
            }
        }

        void MoverCaidas(float dt)
        {
            for (int i = caidas.Count - 1; i >= 0; i--)
            {
                Caida c = caidas[i];
                if (c.rect == null)
                {
                    caidas.RemoveAt(i);
                    continue;
                }

                c.velocidad.y -= 900f * dt;
                c.rect.anchoredPosition += c.velocidad * dt;
                c.rect.localRotation = Quaternion.Euler(0f, 0f, c.rect.localEulerAngles.z + c.giro * dt);
                if (c.rect.anchoredPosition.y > c.suelo)
                    continue;

                // Se rompe: pedacitos que saltan y un brillo que se apaga.
                Vector2 punto = new Vector2(c.rect.anchoredPosition.x, c.suelo);
                Color color = c.rect.GetComponent<Image>().color;
                chispas.Emitir(new ParticulasUI.Rafaga
                {
                    forma = FormasUI.Cristal, cantidad = 5, centro = punto, dispersion = new Vector2(6f, 2f),
                    color = Transparente(color, 0.85f), angulo = new Vector2(25f, 155f), rapidez = new Vector2(90f, 200f),
                    vida = new Vector2(0.35f, 0.6f), tamano = new Vector2(5f, 9f), gravedad = 600f, giro = 300f
                });
                chispas.Una(FormasUI.Destello, punto, Vector2.one * 28f, new Color(1f, 0.95f, 1f, 0.8f), Vector2.zero, 0.35f, 0f, 90f, 0.4f);
                Destroy(c.rect.gameObject);
                caidas.RemoveAt(i);
            }
        }

        // Mishi maúlla y lanza su bola mágica hasta la carta del ingrediente que iba: ahí estalla y la carta brilla.
        // Sale enseguida y vuela rápido, para llegar mientras la receta todavía se ve (antes de barajar la bandeja).
        IEnumerator MishiAyuda(IngredientKind correcto)
        {
            yield return new WaitForSeconds(0.15f);
            RectTransform carta = escena.CartaDe(correcto);
            if (mishi == null || carta == null || capaBola == null)
                yield break;

            mishi.Maullar();
            Vector2 mano = new Vector2(mishi.X, mishi.PisoY + 70f * mishi.escala);
            Vector2 destino = carta.anchoredPosition + new Vector2(0f, 20f);
            Color tono = escena.ColorDe(correcto);
            BolaMagica.LanzarHacia(capaBola, mano, destino, 0.7f, 0.45f, () =>
            {
                chispasBola.Una(FormasUI.Humo, destino, Vector2.one * 230f, Transparente(Color.Lerp(tono, Color.white, 0.3f), 0.75f), Vector2.zero, 1.6f, 0f, 0f, 0.3f);
                chispasBola.Una(FormasUI.Anillo, destino, Vector2.one * 90f, Transparente(Cian, 0.9f), Vector2.zero, 0.7f, 0f, 0f, 2.2f);
                chispasBola.Emitir(new ParticulasUI.Rafaga
                {
                    forma = FormasUI.Destello, cantidad = 8, centro = destino, dispersion = new Vector2(40f, 30f),
                    color = new Color(1f, 1f, 1f, 0.95f), angulo = new Vector2(0f, 360f), rapidez = new Vector2(40f, 120f),
                    vida = new Vector2(0.5f, 0.9f), tamano = new Vector2(18f, 32f), gravedad = 0f, giro = 120f
                });
                StartCoroutine(Latir(carta));
            });
        }

        // La carta señalada late tres veces.
        static IEnumerator Latir(RectTransform carta)
        {
            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                if (carta == null)
                    yield break;
                carta.localScale = Vector3.one * (1f + Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2.5f)) * 0.12f);
                yield return null;
            }

            if (carta != null)
                carta.localScale = Vector3.one;
        }

        // Los cristales se encienden uno tras otro, del más cercano al caldero al más lejano.
        IEnumerator Resonancia()
        {
            Vector2 boca = CalderoVivo.Boca;
            var orden = new List<Luz>(cristales);
            orden.Sort((a, b) => Vector2.Distance(a.imagen.rectTransform.anchoredPosition, boca)
                .CompareTo(Vector2.Distance(b.imagen.rectTransform.anchoredPosition, boca)));
            foreach (Luz cristal in orden)
            {
                cristal.extra = 1f;
                Vector2 punto = cristal.imagen.rectTransform.anchoredPosition;
                chispas.Una(FormasUI.Destello, punto, Vector2.one * Random.Range(40f, 64f), new Color(1f, 1f, 1f, 0.95f), Vector2.zero, 0.8f, 0f, 120f, 0.6f);
                SoltarFragmentos(punto, cristal.tamano * 0.2f, 4);
                yield return new WaitForSeconds(0.07f);
            }
        }

        void SoltarFragmentos(Vector2 centro, float dispersion, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                float ancho = Random.Range(9f, 15f);
                Vector2 punto = centro + Random.insideUnitCircle * dispersion;
                Vector2 velocidad = new Vector2(Random.Range(-40f, 40f), Random.Range(30f, 90f));
                chispas.Una(FormasUI.Cristal, punto, new Vector2(ancho, ancho * 2f), Transparente(Prisma[Random.Range(0, Prisma.Length)], 0.9f),
                    velocidad, Random.Range(1f, 1.8f), 40f, Random.Range(-160f, 160f), -0.3f);
            }
        }

        // ───────────────────────── Animación ─────────────────────────

        void Update()
        {
            float dt = Time.deltaTime;
            MoverCaidas(dt);
            if (!config.fondoVivo)
                return;
            float t = Time.time;
            flash = Mathf.MoveTowards(flash, 0f, dt * 0.8f);

            // Rayos de la cueva: se mecen despacio y su luz va y viene.
            foreach (Rayo rayo in rayos)
            {
                float onda = (Mathf.Sin(t * 0.45f + rayo.fase) + 1f) * 0.5f;
                rayo.imagen.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rayo.angulo + Mathf.Sin(t * 0.25f + rayo.fase) * 4f);
                rayo.imagen.color = Transparente(Color.Lerp(LuzRayo, colorFlash, flash * 0.5f), 0.04f + onda * 0.08f + flash * 0.08f);
            }

            // Niebla: pasa de un lado al otro y vuelve a entrar por el otro borde.
            foreach (Niebla niebla in nieblas)
            {
                RectTransform r = niebla.imagen.rectTransform;
                float x = r.anchoredPosition.x + niebla.velocidad * dt;
                float borde = 960f + r.sizeDelta.x * 0.5f;
                if (x > borde) x = -borde;
                if (x < -borde) x = borde;
                r.anchoredPosition = new Vector2(x, niebla.y + Mathf.Sin(t * 0.3f + niebla.fase) * 12f);
                Color c = niebla.imagen.color;
                niebla.imagen.color = Transparente(c, 0.06f + (Mathf.Sin(t * 0.5f + niebla.fase) + 1f) * 0.03f);
            }

            // Cristales: laten despacio, cada uno a su ritmo; al acertar o fallar se encienden del color del aviso.
            foreach (Luz cristal in cristales)
            {
                cristal.extra = Mathf.MoveTowards(cristal.extra, 0f, dt * 1.2f);
                cristal.apagado = Mathf.MoveTowards(cristal.apagado, 0f, dt * 0.45f);
                float latido = (Mathf.Sin(t * 1.3f + cristal.fase) + 1f) * 0.5f;
                Color color = Color.Lerp(Color.Lerp(cristal.color, colorFlash, flash * 0.8f), Color.white, cristal.extra * 0.4f);
                color = Color.Lerp(color, Apagado, cristal.apagado);
                float alfa = 0.16f + latido * 0.22f + flash * 0.3f + cristal.extra * 0.4f;
                cristal.imagen.color = Transparente(color, Mathf.Lerp(alfa, 0.5f, cristal.apagado));
                cristal.imagen.rectTransform.sizeDelta = Vector2.one * cristal.tamano * (0.9f + latido * 0.18f + flash * 0.2f + cristal.extra * 0.35f);
            }

            // Fragmentos de cristal que suben girando desde los racimos grandes.
            relojFragmento -= dt;
            if (relojFragmento <= 0f)
            {
                relojFragmento = Random.Range(0.12f, 0.25f);
                Luz racimo = cristales[Random.Range(0, cristales.Count)];
                float ancho = Random.Range(8f, 14f);
                Vector2 punto = racimo.imagen.rectTransform.anchoredPosition + Random.insideUnitCircle * racimo.tamano * 0.3f;
                chispas.Una(FormasUI.Cristal, punto, new Vector2(ancho, ancho * 2.1f), Transparente(Prisma[Random.Range(0, Prisma.Length)], Random.Range(0.55f, 0.9f)),
                    new Vector2(Random.Range(-14f, 14f), Random.Range(22f, 55f)), Random.Range(3.5f, 6f), 0f, Random.Range(-70f, 70f), -0.25f);
            }

            // Una punta de cristal suelta una estrella grande con un anillo de luz.
            relojEstrella -= dt;
            if (relojEstrella <= 0f)
            {
                relojEstrella = Random.Range(2.2f, 4f);
                Luz cristal = cristales[Random.Range(0, cristales.Count)];
                Vector2 punto = cristal.imagen.rectTransform.anchoredPosition + Random.insideUnitCircle * cristal.tamano * 0.15f;
                Color tono = Prisma[Random.Range(0, Prisma.Length)];
                chispas.Una(FormasUI.Destello, punto, Vector2.one * Random.Range(80f, 130f), new Color(1f, 0.97f, 1f, 0.95f), Vector2.zero, 0.9f, 0f, 60f, 0.5f);
                chispas.Una(FormasUI.Anillo, punto, Vector2.one * 40f, Transparente(tono, 0.7f), Vector2.zero, 0.6f, 0f, 0f, 3f);
                SoltarFragmentos(punto, 10f, 5);
                cristal.extra = Mathf.Max(cristal.extra, 0.6f);
            }

            // Faroles y velas: la llama titila (ruido suave, no parpadeo brusco).
            foreach (Luz farol in faroles)
            {
                float llama = Mathf.PerlinNoise(t * 2.2f, farol.fase * 5.3f);
                farol.imagen.color = Transparente(farol.color, 0.2f + llama * 0.28f);
                farol.imagen.rectTransform.sizeDelta = Vector2.one * farol.tamano * (0.9f + llama * 0.2f);
            }

            // Círculo mágico: respira azul; al fallar parpadea en rojo y al acertar brilla cian.
            float respira = (Mathf.Sin(t * 0.9f) + 1f) * 0.5f;
            float parpadeo = colorFlash == Rojo ? (Mathf.Sin(t * 22f) + 1f) * 0.5f : 1f;
            circulo.color = Transparente(Color.Lerp(Azul, colorFlash, flash), 0.1f + respira * 0.1f + flash * 0.4f * parpadeo);
            anilloCirculo.color = Transparente(Color.Lerp(Cian, colorFlash, flash), 0.12f + respira * 0.12f + flash * 0.5f * parpadeo);
            anilloCirculo.rectTransform.localScale = Vector3.one * (1f + respira * 0.015f + flash * 0.03f);

            // Motas de luz que suben desde el círculo del suelo.
            relojMota -= dt;
            if (relojMota <= 0f)
            {
                relojMota = 0.14f;
                Vector2 centro = anilloCirculo.rectTransform.anchoredPosition;
                float angulo = Random.Range(0f, Mathf.PI * 2f);
                Vector2 punto = centro + new Vector2(Mathf.Cos(angulo) * tamanoCirculo.x * 0.5f, Mathf.Sin(angulo) * tamanoCirculo.y * 0.5f) * Random.Range(0.3f, 1f);
                chispas.Una(FormasUI.Punto, punto, Vector2.one * Random.Range(4f, 9f), Transparente(Random.value < 0.5f ? Morado : Cian, Random.Range(0.45f, 0.85f)),
                    new Vector2(Random.Range(-8f, 8f), Random.Range(20f, 45f)), Random.Range(2.5f, 4.5f), 0f, 0f, 0f);
            }

            // Un cristal suelta un destello de vez en cuando.
            relojDestello -= dt;
            if (relojDestello <= 0f)
            {
                relojDestello = Random.Range(0.35f, 0.8f);
                Luz cristal = cristales[Random.Range(0, cristales.Count)];
                Vector2 punto = cristal.imagen.rectTransform.anchoredPosition + Random.insideUnitCircle * cristal.tamano * 0.25f;
                chispas.Una(FormasUI.Destello, punto, Vector2.one * Random.Range(18f, 34f), new Color(0.95f, 0.9f, 1f, 0.9f), Vector2.zero, 0.7f, 0f, 90f, 0.6f);
            }

            // Destellos pequeños en la cueva del fondo.
            relojCueva -= dt;
            if (relojCueva <= 0f)
            {
                relojCueva = Random.Range(0.3f, 0.7f);
                Rect cueva = FondoNivel4.Cueva;
                Vector2 punto = FondoNivel4.ACanvas(Random.Range(cueva.xMin, cueva.xMax), Random.Range(cueva.yMin, cueva.yMax));
                chispas.Una(FormasUI.Destello, punto, Vector2.one * Random.Range(10f, 18f), new Color(0.8f, 0.9f, 1f, 1f), Vector2.zero, 1.2f, 0f, 40f, 0.3f);
            }
        }

        static Color Transparente(Color c, float alfa) => new Color(c.r, c.g, c.b, Mathf.Clamp01(alfa));
    }
}
