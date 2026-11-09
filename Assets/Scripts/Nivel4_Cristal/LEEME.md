# Nivel 4 — Torre de Cristal

Trabajo de **Nicol (rama `nikki`)**. Es el nivel más difícil que el Jardín de Luna: tiene la dificultad que antes era del Pasillo de Frascos (nivel 2) más las **sombras** del nivel 3.

## No repite código: llama al del nivel 2

El juego entero es la mesa de frascos de `Nivel2_Frascos/`. Esta carpeta solo trae lo propio del nivel:

```text
Nivel4_Cristal/
├── LEEME.md
├── FondoNivel4.cs     medidas de fondo_cristal.png: caldero, boca, Magnus, cristales, faroles, círculo
├── TorreDeCristal.cs  va en la escena y le avisa a la mesa cuál es su fondo (antes de que empiece)
└── CristalesVivos.cs  la animación del fondo
```

- Escena: `Assets/Scenes/Nivel4_Cristal.unity`. Tiene un `MesaDeFrascos` con **Nivel = 4** y un `TorreDeCristal`.
- Fondo: `Assets/Resources/FluxLab/NIVEL4-IMAGENES/fondo_cristal.png`.

**Lo que se llama del nivel 2:**
- `MesaDeFrascos` y `ConfigFrascos`: el juego y la dificultad.
- `EscenaFrascos`: la receta, la bandeja, el HUD, la bienvenida y los resultados.
- `CalderoVivo` y `CuerpoCaldero`: el caldero que se mueve y su líquido, aquí **azul**.
- `MagnusAnimado`: Magnus con su personalización.
- `ParticulasUI` y `FormasUI`: chispas y destellos.
- Del nivel 3: `MariposaAnimada`. De Sasha (`Gatitos`): `MishiAnimado` y `BolaMagica`.

**Lo que no se usa:** `FrascosDelFondo` y `FondoVivo` (frascos, luna y faroles del pasillo), porque son del Pasillo de Frascos.

## Dificultad

Está en el Inspector de `MesaDeFrascos` (sección Config):
- 4 ingredientes, 4 rondas.
- Se enciende **uno por uno**.
- Puede repetir ingredientes.
- La bandeja **se baraja** en cada ronda.
- 3 frascos de vida.

**Tipos De Ronda**, en orden:
1. Normal
2. **Sombras:** el ingrediente se ve como silueta oscura, sin color ni nombre, y se reconoce por su forma.
3. **Espejo:** la receta se pone al revés.
4. **Espejo en sombras:** al revés y en silueta.

## Animación del fondo (CristalesVivos)

- Los cristales laten con luz morada y azul, y a veces uno suelta un destello.
- Faroles y velas titilan.
- El círculo mágico del suelo respira. Al acertar brilla cian, con un anillo que sale del caldero; al fallar parpadea en rojo.
- Motas de luz suben del suelo y titilan destellos en la cueva.
- **Rayos de luz** bajan desde la cueva y se mecen despacio, y una **niebla mágica** pasa por el lago y el suelo.
- **Fragmentos de cristal** con colores de prisma suben girando desde los racimos (forma `FormasUI.Cristal`).
- De vez en cuando, una punta de cristal suelta una **estrella grande** con un anillo de luz.
- **Al acertar**, los cristales se encienden **en cadena** desde el caldero hacia afuera.
- **Al fallar:**
  - los cristales se **apagan en cadena**, de afuera hacia el caldero, y vuelven solos;
  - caen **fragmentos de cristal** que se rompen en el suelo;
  - **Mishi**, que descansa en la cómoda de la derecha (`Gatitos/MishiAnimado`), maúlla y lanza su **bola mágica** hasta la carta del ingrediente que iba, que brilla y late (`BolaMagica.LanzarHacia`).
- La **mariposa** del Jardín de Luna (`Nivel3/MariposaAnimada`) vuela por la torre y se posa en los cristales y en el borde del caldero. Al acertar, va volando al caldero. Vuela detrás de la receta, así que nunca la tapa. Dónde se posa está en `FondoNivel4.PosaderosMariposa`.

Se apaga con **Fondo Vivo** en el Inspector. Si se cambia la imagen del fondo, solo se vuelven a medir los números de `FondoNivel4.cs`.

## Conteo 3, 2, 1

Lo dibuja la bienvenida del nivel 2 (`BienvenidaFrascos.NumeroDeCristal`), con los datos de `FondoNivel4`:
- Colores **morado, azul y cian**, con un tintineo de cristal sutil (`Resources/Nivel4/cristal_conteo.wav`, el `cristal.mp3` del nivel 2 recortado sin su segundo de silencio), que suena justo cuando se juntan los fragmentos.
- Cada número se forma con **fragmentos que vuelan al centro** y hacen un cristal grande con el número grabado; al final **se rompe**.
- Con cada número, el círculo del suelo y los cristales del fondo **laten de ese color**.
- Al final sale **"¡A brillar, atentooo!"** y toda la torre se enciende en cadena.
