using System;
using System.Collections.Generic;

namespace TRAMADE.Models
{
    public class clsFacturaModel
    {
        public int NumeroFactura { get; set; }
        public int IdUsuario { get; set; }
        public int IdFormaPago { get; set; }
        public int? IdCliente { get; set; } // Nullable para clientes ocasionales
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }

        // Datos para cliente ocasional
        public bool EsOcasional { get; set; }
        public string NombreOcasional { get; set; }
        public string DireccionOcasional { get; set; }
        public string DniOcasional { get; set; }

        public List<clsFacturaDetalleModel> Detalles { get; set; } = new List<clsFacturaDetalleModel>();
    }
}