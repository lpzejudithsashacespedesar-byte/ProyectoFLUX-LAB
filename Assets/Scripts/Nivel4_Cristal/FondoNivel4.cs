using FluxLab.Nivel2;
using UnityEngine;

namespace FluxLab.Nivel4
{
    /// <summary>
    /// El fondo de la Torre de Cristal (Resources/FluxLab/NIVEL4-IMAGENES/fondo_cristal.png) y todas sus medidas.
    /// El juego es la mesa de frascos (Nivel2_Frascos/MesaDeFrascos): este archivo solo le dice
    /// dónde está el caldero pintado, de qué color es el líquido, dónde se para Magnus
    /// y qué animación tiene el fondo (CristalesVivos).
    /// Las medidas están en píxeles del dibujo (1622 x 969), contando desde arriba a la izquierda.
    /// Si se cambia el fondo, solo hay que volver a medir y cambiar los números de este archivo.
    /// </summary>
    public static class FondoNivel4
    {
        public const int Nivel = 4;
        public const string Ruta = "FluxLab/NIVEL4-IMAGENES/fondo_cristal";
        public const float Ancho = 1622f;
        public const float Alto = 969f;

        public static readonly FondoFrascos Datos = new FondoFrascos
        {
            nivel = Nivel,
            ruta = Ruta,
            ancho = Ancho,
            alto = Alto,

            // ── Caldero pintado en el centro, sobre el círculo mágico ──
            zonaCaldero = new RectInt(640, 488, 390, 307),
            centroCalderoX = 838f,
            piesCalderoY = 792f,
            siluetaCaldero = new[]
            {
                new Vector4(840f, 520f, 160f, 30f),  // borde de arriba
                new Vector4(838f, 560f, 157f, 14f),  // cuello
                new Vector4(838f, 650f, 168f, 90f),  // panza
                new Vector4(670f, 578f, 24f, 44f),   // asa izquierda
                new Vector4(1004f, 578f, 24f, 44f),  // asa derecha
                new Vector4(718f, 730f, 18f, 24f),   // pata izquierda
                new Vector4(958f, 730f, 18f, 24f),   // pata derecha
                new Vector4(837f, 752f, 26f, 42f)    // pata del medio
            },

            // ── Boca: el líquido azul con la estrella ──
            bocaDibujo = new Vector2(840f, 517f),
            radioBocaDibujo = 142f,
            achatarBoca = 0.16f,
            liquido = new Color(0.30f, 0.55f, 1f, 1f),

            // ── Magnus de pie en el suelo, a la izquierda del caldero ──
            piesMagnusDibujo = new Vector2(360f, 810f),
            altoMagnus = 480f,

            bienvenida = "¡Bienvenido a la Torre de Cristal!",
            subtitulo = "Nivel 4  ·  ¡más difícil que el Jardín de Luna!",
            superaste = "¡Superaste la Torre de Cristal!",
            pocion = "Elixir de cristal",

            // Conteo de cristal: morado, azul y cian (como los cristales del fondo), con un tintineo de cristal sutil.
            // Es Nivel2/cristal recortado (sin el segundo de silencio que traía al principio) para que suene a tiempo.
            fraseConteo = "¡A brillar, atentooo!",
            numerosDeCristal = true,
            coloresConteo = new[]
            {
                new Color(0.72f, 0.42f, 1f, 1f),
                new Color(0.38f, 0.62f, 1f, 1f),
                new Color(0.42f, 0.95f, 1f, 1f)
            },
            sonidoConteo = "Nivel4/cristal_conteo",
            adelantoSonidoConteo = 0.03f,

            pasilloDeFrascos = false,
            construirVida = (capa, escena, config) => capa.gameObject.AddComponent<CristalesVivos>().Construir(escena, config)
        };

        // ── Para la animación del fondo (CristalesVivos) ──

        /// <summary>Cristales pintados que brillan: (x, y, tamaño del resplandor).</summary>
        public static readonly Vector3[] Cristales =
        {
            new Vector3(600f, 440f, 170f),   // racimo grande de la izquierda, detrás del caldero
            new Vector3(1060f, 400f, 160f),  // racimo de la derecha, detrás del caldero
            new Vector3(880f, 270f, 140f),   // cristales de la cueva
            new Vector3(555f, 185f, 60f),    // cristal colgante de la izquierda
            new Vector3(658f, 320f, 55f),    // cristal colgante del medio
            new Vector3(1128f, 230f, 55f),   // cristal colgante de la derecha
            new Vector3(1528f, 185f, 85f),   // repisa de arriba a la derecha
            new Vector3(1272f, 430f, 80f),   // cómoda de la derecha
            new Vector3(1575f, 590f, 115f),  // copa grande de la derecha
            new Vector3(1500f, 860f, 75f),   // abajo a la derecha
            new Vector3(115f, 560f, 70f),    // farol de cristal de la izquierda
            new Vector3(175f, 720f, 50f),    // cristalito sobre los libros
            new Vector3(60f, 445f, 65f)      // bola de cristal
        };

        /// <summary>Faroles y velas: (x, y, tamaño del resplandor).</summary>
        public static readonly Vector3[] Faroles =
        {
            new Vector3(615f, 95f, 75f),
            new Vector3(727f, 290f, 75f),
            new Vector3(1243f, 205f, 75f),
            new Vector3(1060f, 280f, 50f),
            new Vector3(462f, 525f, 65f),
            new Vector3(110f, 655f, 60f),
            new Vector3(140f, 485f, 45f),
            new Vector3(1450f, 488f, 45f)
        };

        /// <summary>El círculo mágico del suelo: centro (x, y) y radios (ancho, alto).</summary>
        public static readonly Vector4 Circulo = new Vector4(857f, 775f, 592f, 130f);

        /// <summary>Donde se posa la mariposa (el punto donde apoya sus patitas), además del borde del caldero.</summary>
        public static readonly Vector2[] PosaderosMariposa =
        {
            new Vector2(1272f, 400f),   // cristales de la cómoda de la derecha
            new Vector2(1560f, 540f),   // copa de cristales de la derecha
            new Vector2(520f, 555f),    // mueble de la izquierda, junto al farol
            new Vector2(220f, 497f)     // repisa de las velas, a la izquierda
        };

        /// <summary>Donde puede estar Mishi: (izquierda, derecha, altura del piso) en píxeles del dibujo.</summary>
        public static readonly Vector3[] PlataformasMishi =
        {
            new Vector3(1370f, 1540f, 503f),   // encima de la cómoda grande de la derecha
            new Vector3(1250f, 1330f, 488f)    // encima de la cómoda pequeña, junto a los cristales
        };

        /// <summary>De dónde salen los rayos de luz: arriba, en la boca de la cueva.</summary>
        public static readonly Vector2 OrigenRayos = new Vector2(880f, 110f);

        /// <summary>La cueva del fondo, donde titilan destellos.</summary>
        public static readonly Rect Cueva = new Rect(700f, 150f, 400f, 280f);

        public static Vector2 ACanvas(float x, float y) => Datos.ACanvas(x, y);

        public static float Escala => Datos.Escala;

        // Se registra antes de cargar la primera escena, así MesaDeFrascos (con Nivel = 4) lo encuentra.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Registrar()
        {
            FondoFrascos.Registrar(Nivel, Datos);
        }
    }
}
