using System;

namespace TRAMADE.ModelsBitacora
{
    public class clsBitacoraModel
    {
        public int ID { get; set; }
        public string Usuario { get; set; }
        public string Rol { get; set; }
        public string Accion { get; set; }
        public string Tabla { get; set; }
        public DateTime FechaDeAccion { get; set; }
    }
}