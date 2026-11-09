using System.Collections.Generic;
using UnityEngine;

namespace FluxLab.Nivel2
{
    /// <summary>
    /// Un fondo con un caldero pintado sobre el que se juega la mesa de frascos (MesaDeFrascos).
    /// Guarda las medidas del dibujo (dónde está el caldero, su boca y dónde se para Magnus),
    /// el color del líquido, los textos del nivel y la animación propia de su fondo.
    ///
    /// El nivel 2 usa FondoNivel2.Datos. Otro nivel (por ejemplo Nivel4_Cristal) se registra con
    /// FondoFrascos.Registrar(nivel, datos) y en su escena pone MesaDeFrascos con ese número de nivel.
    /// Las medidas están en píxeles del dibujo, contando desde arriba a la izquierda.
    /// </summary>
    public class FondoFrascos
    {
        public int nivel = 2;
        public string ruta;
        public float ancho;
        public float alto;

        // ── Caldero pintado (CuerpoCaldero lo recorta y lo anima) ──
        public RectInt zonaCaldero;
        public float centroCalderoX;
        public float piesCalderoY;
        /// <summary>Silueta del caldero: elipses (centro x, centro y, radio x, radio y).</summary>
        public Vector4[] siluetaCaldero;

        // ── Boca del caldero (CalderoVivo pone ahí el líquido) ──
        public Vector2 bocaDibujo;
        /// <summary>Medio ancho del líquido, en píxeles del dibujo.</summary>
        public float radioBocaDibujo;
        /// <summary>Cuánto se aplasta el círculo del líquido para verse en perspectiva.</summary>
        public float achatarBoca = 0.13f;
        /// <summary>Color del líquido cuando el caldero está tranquilo.</summary>
        public Color liquido = new Color(0.50f, 0.86f, 0.30f, 1f);

        // ── Magnus ──
        public Vector2 piesMagnusDibujo;
        public float altoMagnus = 480f;

        // ── Textos del nivel ──
        public string bienvenida = "¡Bienvenido!";
        public string subtitulo = "";
        public string superaste = "¡Superaste el nivel!";
        public string pocion = "Poción mágica";

        // ── Conteo 3, 2, 1 ──
        public string fraseConteo = "¡Vamos, atentooo!";
        /// <summary>true: cada número se forma con fragmentos dentro de un cristal y se rompe (Torre de Cristal).</summary>
        public bool numerosDeCristal;
        /// <summary>Colores del 3, el 2 y el 1. Vacío: rojo, amarillo y verde.</summary>
        public Color[] coloresConteo;
        /// <summary>Sonido de cada número (Resources). Vacío: el clic de siempre.</summary>
        public string sonidoConteo;
        /// <summary>Segundos de silencio que trae el sonido al principio: se lanza antes para que el golpe caiga a tiempo.</summary>
        public float adelantoSonidoConteo;

        /// <summary>
        /// true solo en el Pasillo de Frascos: frascos pintados que reaccionan, luna y faroles (FondoVivo).
        /// </summary>
        public bool pasilloDeFrascos;

        /// <summary>
        /// La animación propia del fondo de este nivel. Se llama una vez, con la capa donde dibujar.
        /// </summary>
        public System.Action<Transform, EscenaFrascos, ConfigFrascos> construirVida;

        /// <summary>Unidades del canvas (1920 x 1080) por cada píxel del dibujo.</summary>
        public float Escala => 1920f / ancho;

        /// <summary>Pasa un punto del dibujo a coordenadas del canvas (centro = 0, 0).</summary>
        public Vector2 ACanvas(float x, float y)
        {
            return new Vector2((x / ancho - 0.5f) * 1920f, (0.5f - y / alto) * 1080f);
        }

        public Vector2 Boca => ACanvas(bocaDibujo.x, bocaDibujo.y);
        public float RadioBoca => radioBocaDibujo * Escala;
        public Vector2 PiesMagnus => ACanvas(piesMagnusDibujo.x, piesMagnusDibujo.y);

        public Texture2D Cargar()
        {
            var textura = Resources.Load<Texture2D>(ruta);
            if (textura == null)
                Debug.LogWarning("No se encontró el fondo " + ruta);
            return textura;
        }

        // ───────────────────────── Qué fondo se está jugando ─────────────────────────

        static readonly Dictionary<int, FondoFrascos> registrados = new Dictionary<int, FondoFrascos>();
        static FondoFrascos actual;

        /// <summary>El fondo del nivel que se está jugando. Lo elige MesaDeFrascos al empezar.</summary>
        public static FondoFrascos Actual
        {
            get => actual ?? FondoNivel2.Datos;
            set => actual = value;
        }

        public static void Registrar(int nivel, FondoFrascos datos)
        {
            datos.nivel = nivel;
            registrados[nivel] = datos;
        }

        /// <summary>El fondo registrado para ese nivel; si no hay ninguno, el del nivel 2.</summary>
        public static FondoFrascos De(int nivel)
        {
            return registrados.TryGetValue(nivel, out var datos) ? datos : FondoNivel2.Datos;
        }
    }
}
