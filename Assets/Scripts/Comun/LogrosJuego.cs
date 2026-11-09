using UnityEngine;

namespace FluxLab
{
    /// <summary>
    /// Recompensas del laboratorio. Cada logro abre una habilidad para la mesa,
    /// al estilo de un refuerzo: se desbloquea jugando y se gasta una vez por visita.
    /// </summary>
    public static class LogrosJuego
    {
        public enum Poder
        {
            Mirada,
            Lento,
            Pista,
            Deshacer,
            Escudo
        }

        public struct Definicion
        {
            public Poder poder;
            public string nombre;
            public string premio;
            public string como;
        }

        public static readonly Definicion[] Todos =
        {
            new Definicion
            {
                poder = Poder.Mirada,
                nombre = "Primera chispa",
                premio = "Otra mirada: la receta vuelve a mostrarse un momento.",
                como = "Acierta 1 poción."
            },
            new Definicion
            {
                poder = Poder.Lento,
                nombre = "Paciencia",
                premio = "Tiempo lento: la receta se queda más tiempo en el pergamino.",
                como = "Acierta 3 pociones."
            },
            new Definicion
            {
                poder = Poder.Pista,
                nombre = "Ojo de búho",
                premio = "Una pista: se enciende un ingrediente que faltaba recordar.",
                como = "Acierta 5 pociones."
            },
            new Definicion
            {
                poder = Poder.Deshacer,
                nombre = "Mano firme",
                premio = "Deshacer: quita el último ingrediente que echaste.",
                como = "Acierta 8 pociones o abre el segundo taller."
            },
            new Definicion
            {
                poder = Poder.Escudo,
                nombre = "Escudo del caldero",
                premio = "Escudo: el primer error de la mezcla no cuenta.",
                como = "Acierta 12 pociones o abre el tercer taller."
            }
        };

        public static bool Desbloqueado(Poder poder)
        {
            int pociones = AjustesJuego.PocionesHechas;
            int nivel = AjustesJuego.NivelAlcanzado;
            switch (poder)
            {
                case Poder.Mirada: return pociones >= 1;
                case Poder.Lento: return pociones >= 3;
                case Poder.Pista: return pociones >= 5;
                case Poder.Deshacer: return pociones >= 8 || nivel >= 2;
                case Poder.Escudo: return pociones >= 12 || nivel >= 3;
                default: return false;
            }
        }

        public static int Mascara()
        {
            int bits = 0;
            for (int i = 0; i < Todos.Length; i++)
            {
                if (Desbloqueado(Todos[i].poder))
                    bits |= 1 << i;
            }

            return bits;
        }

        public static string NombresNuevos(int antes, int despues)
        {
            string texto = "";
            for (int i = 0; i < Todos.Length; i++)
            {
                int bit = 1 << i;
                if ((antes & bit) != 0 || (despues & bit) == 0)
                    continue;
                if (texto.Length > 0)
                    texto += "\n";
                texto += "Nuevo logro: " + Todos[i].nombre;
            }

            return texto;
        }

        /// <summary>
        /// Usos que trae la visita. El primer taller da uno de cada habilidad.
        /// Los demás traen una cantidad fija.
        /// </summary>
        public static int UsosAlEntrar(Poder poder)
        {
            int nivel = Mathf.Clamp(AjustesJuego.NivelSeleccionado, 1, 5);
            if (nivel <= 1)
                return 1;

            int[,] usos =
            {
                { 2, 1, 1, 1, 1 },
                { 2, 2, 1, 2, 1 },
                { 3, 2, 2, 2, 1 },
                { 3, 2, 2, 2, 2 }
            };
            return usos[nivel - 2, (int)poder];
        }

        public static void PrepararVisita()
        {
            for (int i = 0; i < Todos.Length; i++)
            {
                Poder poder = Todos[i].poder;
                int usos = Desbloqueado(poder) ? UsosAlEntrar(poder) : 0;
                PlayerPrefs.SetInt(Clave(poder), usos);
            }

            PlayerPrefs.Save();
        }

        public static int Cargas(Poder poder)
        {
            if (!Desbloqueado(poder))
                return 0;
            return PlayerPrefs.GetInt(Clave(poder), 0);
        }

        public static bool Gastar(Poder poder)
        {
            if (Cargas(poder) <= 0)
                return false;
            PlayerPrefs.SetInt(Clave(poder), Cargas(poder) - 1);
            PlayerPrefs.Save();
            return true;
        }

        static string Clave(Poder poder)
        {
            return "flux.carga." + (int)poder;
        }
    }
}
