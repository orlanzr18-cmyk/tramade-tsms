using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRAMADE.Models
{
    /// <summary>Categoría de producto — solo atributos.</summary>
    public class clsCategoriaModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    /// <summary>Proveedor — solo atributos.</summary>
    public class clsProveedorModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    /// <summary>Sucursal — solo atributos.</summary>
    public class clsSucursalModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
