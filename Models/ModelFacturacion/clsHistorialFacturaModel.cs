using System;

namespace TRAMADE.Models
{
    public class clsHistorialFacturaModel
    {
        public int Id { get; set; }
        public int NumeroFactura { get; set; } // Solo el número, sin el "INV/..."
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string RtnCliente { get; set; }
        public string DniCliente { get; set; }
        public DateTime FechaEmision { get; set; }

        // Propiedad calculada (Solo lectura) para cumplir con TS-51-H24 
        // de forma elegante sin ensuciar la base de datos
        public string DocumentoCliente
        {
            get
            {
                if (!string.IsNullOrEmpty(RtnCliente) && !RtnCliente.StartsWith("SN-")) return RtnCliente;
                if (!string.IsNullOrEmpty(DniCliente) && !DniCliente.StartsWith("SN-")) return DniCliente;
                return "S/N";
            }
        }

        // Formato visual solo para la UI
        public string NumeroFormateado => $"INV/2026/{NumeroFactura:D4}";
    }
}