using System;
using System.Collections.Generic;

namespace TRAMADE.Models
{
    public class clsFacturaConsultaModel
    {
        public int NumeroFactura { get; set; }
        public string NumeroFormateado => $"INV/2026/{NumeroFactura:D4}";
        public string DniRtnCliente { get; set; }
        public string NombreCliente { get; set; }
        public string DireccionCliente { get; set; }
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Total { get; set; }
        public int IdFormaPago { get; set; }

        // Relación con los detalles
        public List<clsFacturaDetalleConsultaModel> Detalles { get; set; } = new List<clsFacturaDetalleConsultaModel>();
    }

    public class clsFacturaDetalleConsultaModel
    {
        public string Producto { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}