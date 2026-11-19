using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRAMADE.Models
{
    public class clsClienteOcasionalModel
    {
        public int Id { get; set; }
        public int IdFactura { get; set; } // Relación con la factura
        public string Nombre { get; set; }
        public string Dni { get; set; }
        public string Direccion { get; set; }
    }
}
