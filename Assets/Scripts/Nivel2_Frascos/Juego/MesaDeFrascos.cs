using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FluxLab.Nivel2
{
    /// <summary>
    /// La mesa de frascos: la receta se enciende de a un ingrediente, puede repetir ingredientes,
    /// cada error rompe un frasco de vida, la bandeja se baraja y hay rondas de sombras y espejo.
    /// Nació en el nivel 2 (Pasillo de Frascos); hoy la usa la Torre de Cristal (Nivel4_Cristal),
    /// que pone este componente en su escena con "Nivel" = 4.
    /// Los números se cambian en el Inspector (sección "Config").
    /// </summary>
    public class MesaDeFrascos : MonoBehaviour
    {
        /// <summary>El nivel que se está jugando (lo dice el fondo elegido en Awake).</summary>
        public static int Nivel => FondoFrascos.Actual.nivel;
        const string SonidoRomper = "Nivel2/romper";

        [Tooltip("Qué nivel es esta escena: elige su fondo, su caldero y su animación (2 = Pasillo de Frascos, 4 = Torre de Cristal).")]
        [SerializeField] int nivel = 2;

        [SerializeField] ConfigFrascos config = new ConfigFrascos();

        EscenaFrascos escena;
        IngredientKind[] ordenBandeja = RecetaFrascos.Todos;
        List<IngredientKind> receta = new List<IngredientKind>();
        List<IngredientKind> esperada = new List<IngredientKind>();
        readonly List<IngredientKind> puestos = new List<IngredientKind>();
        readonly List<float> tiemposRespuesta = new List<float>();

        int ronda;
        int puntos;
        int aciertos;
        int racha;
        int rachaMaxima;
        bool turnoAbierto;
        bool vertiendo;
        bool enPausa;
        bool preguntandoSalir;
        bool bienvenidaVista;
        int partida;
        float inicioTurno;

        bool falloEnRonda;
        bool pistaUsada;
        int posicionFallo = -1;   // en qué lugar de la receta se equivocó (para seguir desde ahí)
        int errores;
        float ultimaJugada;

        bool Detenido => enPausa || preguntandoSalir;

        void Awake()
        {
            Time.timeScale = 1f;
            FondoFrascos.Actual = FondoFrascos.De(nivel);
            if (FondoFrascos.Actual.nivel != nivel)
                Debug.LogWarning("MesaDeFrascos: no hay fondo registrado para el nivel " + nivel + "; se usa el del nivel 2. " +
                                 "¿Falta el componente del nivel en la escena (por ejemplo TorreDeCristal)?");
            escena = new EscenaFrascos(this);
            escena.Construir(config, new EscenaFrascos.Acciones
            {
                elegir = Elegir,
                vaciar = Vaciar,
                pausar = Pausar,
                continuar = Continuar,
                pedirSalir = PedirSalir,
                confirmarSalir = ConfirmarSalir,
                cancelarSalir = CancelarSalir,
                irAlMapa = IrAlMapa,
                irAlMenu = IrAlMenu,
                reintentar = Empezar,
                siguiente = IrAlSiguiente
            });

            // Sin vidas (como en el nivel 1): la placa de frascos de vida no se ve.
            if (!config.usarVidas)
                escena.Vidas.gameObject.AddComponent<CanvasGroup>().alpha = 0f;
        }

        void Start()
        {
            Empezar();
        }

        // Si la escena se cierra en pausa, el resto del juego no debe quedar congelado.
        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void Update()
        {
            escena.AnimarBandeja();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (preguntandoSalir) CancelarSalir();
                else if (enPausa) Continuar();
                else Pausar();
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                if (enPausa) Continuar();
                else Pausar();
            }

            if (!turnoAbierto || Detenido)
                return;

            for (int i = 0; i < ordenBandeja.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                    Elegir(ordenBandeja[i]);
            }

            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Space))
                Vaciar();
        }

        // ───────────────────────── Flujo del nivel ─────────────────────────

        public void Empezar()
        {
            StopAllCoroutines();
            Time.timeScale = 1f;
            enPausa = false;
            preguntandoSalir = false;
            escena.MostrarPausa(false);
            escena.MostrarConfirmarSalir(false);
            partida++;
            vertiendo = false;
            puntos = 0;
            aciertos = 0;
            errores = 0;
            racha = 0;
            rachaMaxima = 0;
            ronda = 0;
            tiemposRespuesta.Clear();
            turnoAbierto = false;
            escena.OcultarResultado();
            escena.EnfocarReceta(false);
            escena.LimpiarEfectos();
            escena.Caldero.Reiniciar();
            escena.MostrarEspejo(false);
            escena.Vidas.Restaurar();
            escena.PonerAnimo(AnimoMagnus.Quieto);
            StartCoroutine(Sesion());
        }

        IEnumerator Sesion()
        {
            escena.Marcador(1, config.rondas, puntos);
            escena.Mensaje("", "#5a3b24");
            escena.Temporizador("", 1f);

            // Nuevo: bienvenida (solo la primera vez) y conteo 3, 2, 1.
            if (config.mostrarBienvenida && !bienvenidaVista)
            {
                bienvenidaVista = true;
                yield return escena.Bienvenida.Mostrar();
            }

            if (config.conteoInicial)
                yield return escena.Bienvenida.Conteo();
            escena.Bienvenida.gameObject.SetActive(false);
            escena.SoltarMaderas();

            for (ronda = 1; ronda <= config.rondas; ronda++)
            {
                escena.Marcador(ronda, config.rondas, puntos);
                yield return Ronda();
                if (escena.Vidas.Quedan <= 0)
                    break;
            }

            // Nuevo: mensaje de motivación al terminar (o de ánimo si se rompieron los frascos).
            if (config.mensajesAnimo)
            {
                bool seRompieron = escena.Vidas.Quedan <= 0;
                int estrellas = Estrellas(seRompieron);
                yield return escena.Bienvenida.Final(estrellas, config.estrellasParaAvanzar, seRompieron);
            }

            Terminar();
        }

        IEnumerator Ronda()
        {
            ConfigFrascos.TipoRonda tipo = config.TipoDe(ronda);
            bool espejo = ConfigFrascos.EsEspejo(tipo);
            bool sombras = ConfigFrascos.EsSombra(tipo);
            receta = RecetaFrascos.Generar(Mathf.Clamp(config.largoReceta, 1, EscenaFrascos.MaxReceta), config.permitirRepetidos);
            esperada = RecetaFrascos.Esperada(receta, espejo);
            pistaUsada = false;   // la pista por duda es una sola por ronda (aunque se repita la receta)

            // Nuevo: aviso gigante antes de la ronda al revés.
            if (espejo && config.anunciarEspejo)
                yield return escena.Bienvenida.AnuncioEspejo();

            // La bandeja se baraja una vez por ronda: si se repite la receta, las cartas se quedan donde estaban
            // (así la carta que señaló Mishi sigue en su lugar).
            if (config.barajarBandeja)
            {
                ordenBandeja = RecetaFrascos.Barajar(RecetaFrascos.Todos);
                escena.AcomodarBandeja(ordenBandeja, true);
            }

            bool sigueDesdeElError = false;
            puestos.Clear();
            while (true)
            {
                turnoAbierto = false;
                falloEnRonda = false;

                if (sigueDesdeElError)
                {
                    // Sigue desde donde falló: lo bien puesto se queda en el caldero y la receta NO se vuelve a mostrar.
                    // Para lo que falta, sirve la pista de Mishi (la carta del ingrediente que iba ahí) y la memoria.
                    escena.Mensaje("Sigue desde el ingrediente " + (puestos.Count + 1) + ". ¡Usa la pista de Mishi!", "#2f6a8a");
                }
                else
                {
                    puestos.Clear();
                    escena.Caldero.Reiniciar();
                    escena.MostrarEspejo(espejo);

                    escena.RecetaEnSombras = sombras;
                    escena.Mensaje(sombras && espejo ? "¡Sombras al revés! Reconoce las formas y recuerda el orden."
                        : sombras ? "¡Solo sombras! Reconoce cada ingrediente por su forma."
                        : espejo ? "Mira bien: esta receta va al revés."
                        : config.recetaCompleta ? "Memoriza el orden, de izquierda a derecha." : "Mira cómo se encienden los frascos.", "#5a3b24");
                    escena.PonerAnimo(AnimoMagnus.Observa);
                    escena.EncenderIngrediente(receta, -1);
                    escena.Temporizador("Prepárate...", 1f);
                    escena.EnfocarReceta(true);   // nuevo: el pergamino crece mientras se dicta la receta
                    yield return new WaitForSeconds(0.6f);

                    if (config.recetaCompleta)
                        yield return MostrarRecetaCompleta();
                    else
                        yield return EncenderReceta();
                    escena.RecetaEnSombras = false;
                    escena.EnfocarReceta(false);  // y vuelve a su lugar cuando te toca

                    escena.Mensaje(espejo ? "Ahora ponla en el caldero AL REVÉS." : "Ahora repítela en el caldero.", espejo ? "#8d3d36" : "#5a3b24");
                }

                escena.Avance(puestos.Count, esperada.Count);
                escena.PonerAnimo(AnimoMagnus.Quieto);
                turnoAbierto = true;
                inicioTurno = Time.time;
                ultimaJugada = Time.time;

                while (turnoAbierto)
                {
                    // Pista por duda: si pasa un rato sin poner nada, una sola vez por ronda.
                    // (Time.time no avanza en pausa, así que la pausa no cuenta.)
                    if (config.pistaPorDuda && !pistaUsada && !vertiendo && !Detenido
                        && puestos.Count < esperada.Count && Time.time - ultimaJugada >= config.segundosParaPista)
                    {
                        pistaUsada = true;
                        escena.Mensaje("¡Mishi te da una pista!", "#2f6a8a");
                        escena.AvisarDuda(esperada[puestos.Count]);
                    }
                    yield return null;
                }

                // Se equivocó y quedan frascos: sigue con la misma receta desde donde falló (no cuenta como otra ronda).
                // Se quita lo que puso desde el error en adelante; lo de antes estaba bien y se queda.
                if (falloEnRonda && config.repetirRecetaAlFallar && escena.Vidas.Quedan > 0)
                {
                    sigueDesdeElError = true;
                    if (posicionFallo >= 0 && posicionFallo < puestos.Count)
                        puestos.RemoveRange(posicionFallo, puestos.Count - posicionFallo);
                    yield return new WaitForSeconds(Mathf.Max(1.5f, config.pausaTrasError));
                    continue;
                }

                break;
            }

            // Después de un error se espera más: así se alcanza a ver cómo era la receta antes de la siguiente.
            yield return new WaitForSeconds(falloEnRonda ? Mathf.Max(1.5f, config.pausaTrasError) : 1.5f);
            escena.MostrarEspejo(false);
        }

        // Como en el nivel 1: la receta se ve entera unos segundos y después se tapa.
        IEnumerator MostrarRecetaCompleta()
        {
            float total = Mathf.Max(0.5f, config.segundosRecetaCompleta);
            escena.MostrarRecetaCompleta(receta);
            escena.Sonar("FluxLab/Audio/click");
            float restante = total;
            while (restante > 0f)
            {
                restante -= Time.deltaTime;
                escena.Temporizador("Recuerda: " + Mathf.Max(0f, restante).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + "s",
                    Mathf.Clamp01(restante / total));
                yield return null;
            }

            escena.EncenderIngrediente(receta, -1);
        }

        // Nuevo en el nivel 2: la receta aparece ingrediente por ingrediente.
        IEnumerator EncenderReceta()
        {
            float porIngrediente = Mathf.Max(0.1f, config.segundosPorIngrediente);
            float pausa = Mathf.Max(0f, config.pausaEntreIngredientes);
            float total = receta.Count * (porIngrediente + pausa);
            float transcurrido = 0f;

            for (int i = 0; i < receta.Count; i++)
            {
                escena.EncenderIngrediente(receta, i);
                escena.Sonar("FluxLab/Audio/click");
                float reloj = 0f;
                while (reloj < porIngrediente)
                {
                    reloj += Time.deltaTime;
                    transcurrido += Time.deltaTime;
                    escena.Temporizador("Frasco " + (i + 1) + " de " + receta.Count, 1f - transcurrido / total);
                    yield return null;
                }

                escena.EncenderIngrediente(receta, -1);
                reloj = 0f;
                while (reloj < pausa)
                {
                    reloj += Time.deltaTime;
                    transcurrido += Time.deltaTime;
                    escena.Temporizador("Frasco " + (i + 1) + " de " + receta.Count, 1f - transcurrido / total);
                    yield return null;
                }
            }
        }

        // ───────────────────────── Jugadas ─────────────────────────

        // Elegir lanza el frasco; la jugada se revisa cuando el chorro cae (AlCaer).
        public void Elegir(IngredientKind tipo)
        {
            if (!turnoAbierto || vertiendo || Detenido || puestos.Count >= esperada.Count)
                return;

            vertiendo = true;
            int esta = partida;
            IngredientKind? anterior = puestos.Count > 0 ? puestos[puestos.Count - 1] : (IngredientKind?)null;
            escena.Sonar("FluxLab/Audio/click");
            escena.PonerAnimo(AnimoMagnus.Remueve);
            escena.LanzarFrasco(tipo, config.duracionVertido, () =>
            {
                if (esta == partida)
                    AlCaer(tipo, anterior);
            });
        }

        void AlCaer(IngredientKind tipo, IngredientKind? anterior)
        {
            vertiendo = false;
            if (!turnoAbierto)
                return;

            ultimaJugada = Time.time;
            int posicion = puestos.Count;
            puestos.Add(tipo);
            escena.Avance(puestos.Count, esperada.Count);
            escena.Caldero.Verter(tipo, escena.ColorDe(tipo), anterior);

            // Como en el nivel 1: se revisa al final, cuando ya están todos.
            if (config.revisarAlFinal)
            {
                if (puestos.Count < esperada.Count)
                {
                    escena.Magnus.Reir(0.6f);
                    return;
                }

                int malo = -1;
                for (int i = 0; i < puestos.Count && malo < 0; i++)
                    if (!RecetaFrascos.AciertaEn(esperada, i, puestos[i]))
                        malo = i;
                if (malo < 0)
                {
                    Acertar();
                }
                else
                {
                    escena.IngredienteCorrecto = esperada[malo];
                    posicionFallo = malo;
                    Fallar();
                }
                return;
            }

            // Nuevo en el nivel 2: se revisa en el momento, no al final.
            if (!RecetaFrascos.AciertaEn(esperada, posicion, tipo))
            {
                escena.IngredienteCorrecto = esperada[posicion];
                posicionFallo = posicion;
                Fallar();
                return;
            }

            if (puestos.Count == esperada.Count)
            {
                Acertar();
                return;
            }

            escena.Magnus.Reir(0.9f);   // ingrediente correcto: Magnus se ríe
            if (config.mensajesAnimo)
                escena.Elogio();
        }

        public void Vaciar()
        {
            if (!turnoAbierto || vertiendo || Detenido || puestos.Count == 0)
                return;
            puestos.Clear();
            ultimaJugada = Time.time;
            escena.Avance(0, esperada.Count);
            escena.Caldero.Vaciar();
            escena.Sonar("FluxLab/Audio/click");
        }

        void Acertar()
        {
            CerrarTurno();
            aciertos++;
            puntos += config.puntosPorAcierto;
            racha++;
            rachaMaxima = Mathf.Max(rachaMaxima, racha);
            escena.Mensaje("Correcto. La mezcla cuajó.", "#2f7a4a");
            escena.Temporizador("Bien hecho", 1f);
            escena.Sonar("FluxLab/Audio/acierto");
            escena.PonerAnimo(AnimoMagnus.Festeja);
            escena.Caldero.Exito();
            escena.FrascosFondo?.Festejar();   // ¡los frascos del fondo festejan!
            escena.AvisarAcierto();            // y la animación del fondo de cada nivel
            if (config.mensajesAnimo)
                escena.Felicitar();
            escena.Marcador(ronda, config.rondas, puntos);
        }

        void Fallar()
        {
            falloEnRonda = true;
            errores++;
            CerrarTurno();
            racha = 0;
            int quedan = escena.Vidas.Quedan;
            if (config.usarVidas)
            {
                quedan = escena.Vidas.Romper();
                // Si se repite la receta no se enseña entera (sería regalarla): solo la pista de Mishi.
                bool seRepite = config.repetirRecetaAlFallar && quedan > 0;
                if (!seRepite)
                    escena.MostrarRecetaCompleta(receta);
                escena.Mensaje(quedan <= 0 ? "Se rompió el último frasco."
                    : seRepite ? "¡Crack! Se rompió un frasco. Mira cuál iba ahí: ¡Mishi te lo muestra!"
                    : "¡Crack! Se rompió un frasco. Así era la receta.", "#8d3d36");
                escena.Temporizador("Quedan " + quedan + " frascos", 0f);
                escena.Sonar(SonidoRomper);
            }
            else
            {
                escena.MostrarRecetaCompleta(receta);
                escena.Mensaje("Esa no era la receta. Así era:", "#8d3d36");
                escena.Temporizador("¡La próxima sale!", 0f);
            }
            escena.PonerAnimo(AnimoMagnus.SeEquivoca);
            escena.Caldero.Error();
            escena.FrascosFondo?.RomperVarios();  // se caen y se rompen varios frascos del fondo
            escena.AvisarFallo();
            if (config.mensajesAnimo)
                escena.Lamentar(quedan);
        }

        // Sin repetir recetas: como el nivel 1 (según cuántas rondas salieron bien).
        // Repitiendo la receta al fallar, todas las rondas terminan bien, así que cuentan los errores:
        // 3 estrellas sin errores, 2 con uno, 1 con más. Si se rompen todos los frascos, como máximo 1.
        int Estrellas(bool seRompieron)
        {
            if (!config.repetirRecetaAlFallar || !config.usarVidas)
                return RecetaFrascos.Estrellas(aciertos, config.rondas, seRompieron);
            if (seRompieron)
                return aciertos > 0 ? 1 : 0;
            return errores == 0 ? 3 : errores == 1 ? 2 : 1;
        }

        void CerrarTurno()
        {
            turnoAbierto = false;
            tiemposRespuesta.Add(Mathf.Max(0f, Time.time - inicioTurno));
        }

        void Terminar()
        {
            turnoAbierto = false;
            bool seRompieron = escena.Vidas.Quedan <= 0;
            int recordAnterior = AjustesJuego.MejorPuntaje;
            AjustesJuego.RegistrarPuntaje(puntos, aciertos);

            // Nuevo: solo se abre el siguiente nivel con las estrellas mínimas (y sin romper todos los frascos).
            int estrellas = Estrellas(seRompieron);
            bool pasa = !seRompieron && estrellas >= config.estrellasParaAvanzar;
            string siguiente = NivelesTaller.De(Nivel + 1).nombre;
            string aviso = seRompieron
                ? "Se rompieron los frascos. ¡Inténtalo otra vez!"
                : pasa ? "" : "Necesitas " + config.estrellasParaAvanzar + " estrellas para abrir " + siguiente + ".";
            if (pasa && AjustesJuego.NivelAlcanzado == Nivel)
            {
                AjustesJuego.AvanzarNivel();
                aviso = "Se abrió " + siguiente + ".";
            }

            float promedio = 0f;
            for (int i = 0; i < tiemposRespuesta.Count; i++)
                promedio += tiemposRespuesta[i];
            if (tiemposRespuesta.Count > 0)
                promedio /= tiemposRespuesta.Count;

            escena.MostrarResultado(new EscenaFrascos.Resultado
            {
                puntos = puntos,
                aciertos = aciertos,
                rondas = config.rondas,
                rachaMaxima = rachaMaxima,
                tiempoMedio = promedio,
                vidasRestantes = escena.Vidas.Quedan,
                vidasTotales = escena.Vidas.Total,
                seRompieron = seRompieron,
                aviso = aviso,
                haySiguiente = AjustesJuego.NivelAlcanzado > Nivel,
                pasa = pasa,
                mejorPuntaje = AjustesJuego.MejorPuntaje,
                nuevoRecord = puntos > 0 && puntos > recordAnterior
            });
        }

        // ───────────────────────── Pausa y salir ─────────────────────────

        // Time.timeScale = 0 congela todo lo que usa Time.deltaTime: receta, frascos, partículas.
        void Pausar()
        {
            if (enPausa || preguntandoSalir || escena.ResultadoVisible)
                return;
            enPausa = true;
            Time.timeScale = 0f;
            escena.MostrarPausa(true);
            escena.Sonar("FluxLab/Audio/click");
        }

        void Continuar()
        {
            enPausa = false;
            preguntandoSalir = false;
            escena.MostrarPausa(false);
            escena.MostrarConfirmarSalir(false);
            Time.timeScale = 1f;
        }

        void PedirSalir()
        {
            if (preguntandoSalir)
                return;
            preguntandoSalir = true;
            Time.timeScale = 0f;
            escena.MostrarPausa(false);
            escena.MostrarConfirmarSalir(true);
            escena.Sonar("FluxLab/Audio/click");
        }

        // "No, seguir": vuelve a la pausa si venía de ahí, si no, sigue jugando.
        void CancelarSalir()
        {
            preguntandoSalir = false;
            escena.MostrarConfirmarSalir(false);
            if (enPausa)
                escena.MostrarPausa(true);
            else
                Time.timeScale = 1f;
        }

        void ConfirmarSalir() => Cargar("MapaNiveles");

        // ───────────────────────── Navegación ─────────────────────────

        void IrAlSiguiente()
        {
            if (AjustesJuego.NivelAlcanzado <= Nivel)
                return;
            AjustesJuego.ElegirNivel(Nivel + 1);
            Cargar(NivelesTaller.EscenaDe(Nivel + 1));
        }

        void IrAlMapa() => Cargar("MapaNiveles");

        void IrAlMenu() => Cargar("MenuPrincipal");

        static void Cargar(string escenaDestino)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(escenaDestino);
        }
    }
}
