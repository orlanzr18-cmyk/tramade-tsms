using TRAMADE.Models;
using TRAMADE.Repositories;

namespace TRAMADE.Services
{
    public class ClienteOcasionalService
    {
        private readonly ClienteOcasionalRepository _repo = new ClienteOcasionalRepository();

        public bool GuardarClienteOcasional(clsClienteOcasionalModel cliente)
        {
            // Aquí podrías poner validaciones (ej: nombre no vacío)
            if (string.IsNullOrWhiteSpace(cliente.Nombre)) return false;

            return _repo.Insertar(cliente);
        }

        public clsClienteOcasionalModel BuscarPorFactura(int idFactura)
        {
            return _repo.ObtenerPorFactura(idFactura);
        }
    }
}