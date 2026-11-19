using System;
using System.Collections.Generic;

namespace TRAMADE.Models.ModelsCompras
{
    public class CompraDetalleModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioCosto { get; set; }
    }

    public class CompraSolicitudModel
    {
        public CompraSolicitudModel()
        {
            Detalles = new List<CompraDetalleModel>();
        }

        public int IdCompra { get; set; }
        public int IdProveedor { get; set; }
        public int IdFormaPago { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaEntrega { get; set; }
        public IList<CompraDetalleModel> Detalles { get; private set; }
    }

    public class IndicadoresCompraModel
    {
        public string SolicitudesRegistradas { get; set; }
        public string ValorTotalPendiente { get; set; }
        public string TiempoRespuestaPromedio { get; set; }
    }
}
