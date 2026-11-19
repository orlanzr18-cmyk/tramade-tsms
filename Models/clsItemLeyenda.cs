using System.Windows.Media;

namespace TRAMADE.Models
{
    /// <summary>
    /// Representa un elemento de leyenda (nombre + color) para dibujar
    /// manualmente en WPF junto a un gráfico OxyPlot.
    /// </summary>
    internal class clsItemLeyenda
    {
        public string Nombre { get; set; }
        public Color Color { get; set; }
    }
}