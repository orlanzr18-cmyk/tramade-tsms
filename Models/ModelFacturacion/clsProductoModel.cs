using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRAMADE.Models
{
    public class clsProductoModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string CodigoBarras { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioCosto { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; }
        public string Proveedor { get; set; }
        public string Sucursal { get; set; }
        public byte[] Imagen { get; set; }
    }
}
