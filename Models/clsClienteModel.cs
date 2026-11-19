using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRAMADE.Models
{
    public class clsClienteModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string RazonSocial { get; set; }
        public string TipoCliente { get; set; } // Puede ser el ID de clasificación
        public string Contacto { get; set; }
        public string Dni { get; set; }
        public string Rtn { get; set; }
        public string Correo { get; set; }
        public string Direccion { get; set; }
        public int IdCiudad { get; set; }
        public int IdDepartamento { get; set; }
        public string Telefono { get; set; }
        public int IdUsuario { get; set; }
        public int Estado { get; set; } = 1; // Por defecto Activo
        public DateTime FechaRegistro { get; set; }
    }
}
