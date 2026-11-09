using FluxLab.Nivel2;
using UnityEngine;

namespace FluxLab.Nivel4
{
    /// <summary>
    /// Va en la escena Nivel4_Cristal, junto a MesaDeFrascos (con Nivel = 4).
    /// Se ejecuta antes que la mesa y le avisa cuál es el fondo de la Torre de Cristal
    /// (FondoNivel4: medidas, caldero azul y la animación CristalesVivos).
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class TorreDeCristal : MonoBehaviour
    {
        void Awake()
        {
            FondoFrascos.Registrar(FondoNivel4.Nivel, FondoNivel4.Datos);
        }
    }
}
