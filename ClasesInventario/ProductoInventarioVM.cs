using TRAMADE.Models;

namespace TRAMADE.ClasesInventario
{
    /// <summary>
    /// Mismo rol que la "clsProductoVM" original: envuelve los datos del
    /// producto y agrega las propiedades calculadas que necesita el DataGrid
    /// WPF (alertas de stock, colores, strings formateados) sin tocar el XAML.
    ///
    /// Se mantiene el mismo nombre de clase y las mismas propiedades públicas
    /// ("ID", "Categoría", "AlertaColor", etc.) para que el XAML existente
    /// (bindings de dgInventario y del panel de detalle) siga funcionando
    /// sin cambios. La diferencia es que ahora se construye a partir del
    /// Model (TRAMADE.Models.clsProductoModel) que entrega el Service,
    /// en vez de leer un DataRow crudo.
    /// </summary>
    internal class clsProductoVM
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Proveedor { get; set; }
        public string Categoría { get; set; }
        public string Sucursal { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioCosto { get; set; }
        public int Stock { get; set; }
        public byte[] Imagen { get; set; }

        // ── Propiedades calculadas para el DataGrid (idénticas a las originales) ─
        // Este bloque dice que no hace referencia a nada, pero en realidad hace que se muestren 
        // los colores y alertas de stock en el DataGrid y en el panel de detalle.
        public string AlertaTexto =>
            Stock == 0 ? "● Agotado" :
            Stock <= 10 ? "● Crítico" :
            Stock <= 50 ? "● Bajo" :
                          "● Normal";

        public string AlertaColor =>
            Stock == 0 ? "#c0392b" :
            Stock <= 10 ? "#e74c3c" :
            Stock <= 50 ? "#e67e22" :
                          "#27ae60";

        public string FilaBg =>
            Stock == 0 ? "#fff0f0" :
            Stock <= 10 ? "#fff0f0" :
            Stock <= 50 ? "#fffde8" :
                          "White";

        public string PrecioStr => $"L. {PrecioUnitario:N2}";
        public string CostoStr => $"L. {PrecioCosto:N2}";

        // ── Factory: construye el VM a partir del Model que entrega el Service ──
        public static clsProductoVM DesdeModel(clsProductoModel m) => new clsProductoVM
        {
            ID = m.Id,
            Nombre = m.Nombre,
            Proveedor = m.Proveedor,
            Categoría = m.Categoria,
            Sucursal = m.Sucursal,
            PrecioUnitario = m.PrecioUnitario,
            PrecioCosto = m.PrecioCosto,
            Stock = m.Stock,
            Imagen = m.Imagen
        };
    }
}
