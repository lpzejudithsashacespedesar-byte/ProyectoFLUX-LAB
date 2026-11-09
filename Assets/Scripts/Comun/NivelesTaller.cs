using UnityEngine;

namespace FluxLab
{
    public struct Taller
    {
        public string nombre;
        public string corto;
        public string detalle;
        public int receta;
        public int rondas;
        public float segundos;
    }

    public static class NivelesTaller
    {
        public static readonly Taller[] Lista =
        {
            new Taller
            {
                nombre = "Mesa del Aprendiz",
                corto = "Aprendiz",
                detalle = "Cuatro ingredientes y cuatro segundos para mirar.",
                receta = 4,
                rondas = 3,
                segundos = 4f
            },
            new Taller
            {
                nombre = "Pasillo de Frascos",
                corto = "Frascos",
                // Por ahora con la dificultad de la Mesa del Aprendiz.
                // Sus números de verdad están en la escena Nivel2_Frascos (MesaDeFrascos > Config).
                detalle = "Tres ingredientes y cinco segundos para mirar.",
                receta = 3,
                rondas = 3,
                segundos = 5f
            },
            new Taller
            {
                nombre = "Jardín de Luna",
                corto = "Jardín",
                detalle = "Entran cuatro ingredientes en el caldero.",
                receta = 4,
                rondas = 3,
                segundos = 4f
            },
            new Taller
            {
                nombre = "Torre de Cristal",
                corto = "Cristal",
                // Usa la mesa de frascos (Nivel4_Cristal): uno por uno, bandeja barajada, sombras y espejo.
                // Sus números de verdad están en la escena Nivel4_Cristal (MesaDeFrascos > Config).
                detalle = "Uno por uno, sombras y recetas al revés.",
                receta = 4,
                rondas = 4,
                segundos = 3.2f
            },
            new Taller
            {
                nombre = "Caldero Mayor",
                corto = "Maestro",
                detalle = "Cinco ingredientes. La prueba del maestro.",
                receta = 5,
                rondas = 4,
                segundos = 2.5f
            }
        };

        /// <summary>Escena que abre cada nivel. El Pasillo de Frascos y la Torre de Cristal tienen la suya; los demás usan la mesa general.</summary>
        public static string EscenaDe(int nivel)
        {
            if (nivel == 2) return "Nivel2_Frascos";
            if (nivel == 4) return "Nivel4_Cristal";   // Torre de Cristal
            return "MesaDeAlquimia";
        }

        public static Taller De(int nivel)
        {
            int indice = Mathf.Clamp(nivel, 1, Lista.Length) - 1;
            return Lista[indice];
        }
    }
}
