using System.Collections.Generic;
using TRAMADE.Models;
using TRAMADE.Repositories;

namespace TRAMADE.Services
{
    public class clsProductoService
    {
        private readonly clsProductoRepository repo = new clsProductoRepository();

        public List<clsProductoModel> ListarProductos(string filtro = "")
        {
            // Tomamos la sucursal de la sesión y se la pasamos al repo
            return repo.ObtenerProductos(clsSesion.id_sucursal, filtro);
        }

        public clsProductoModel BuscarPorCodigo(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;
            return repo.ObtenerPorCodigoBarras(clsSesion.id_sucursal, codigo);
        }
    }
}