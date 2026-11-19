using System;
using System.Data;
using TRAMADE.Repositories.RepoBitacora;

namespace TRAMADE.Services.ServBitacora
{
    public class clsBitacoraService
    {
        private readonly clsBitacoraRepository repo = new clsBitacoraRepository();

        public DataTable ListarHistorial() => repo.ObtenerRegistros();

        public DataTable ListarRoles() => repo.ObtenerRoles();

        public DataTable FiltrarBitacora(DateTime? desde, DateTime? hasta, string rol, string nombre)
        {
            if (desde.HasValue && hasta.HasValue && desde > hasta)
                throw new Exception("La fecha 'Desde' no puede ser mayor a 'Hasta'.");

            return repo.ObtenerRegistros(desde, hasta, rol, nombre);
        }
    }
}