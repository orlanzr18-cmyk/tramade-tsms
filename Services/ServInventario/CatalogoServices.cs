using System.Collections.Generic;
using TRAMADE.Models;
using TRAMADE.Repositories;

namespace TRAMADE.Services
{
    public interface ICategoriaService
    {
        List<clsCategoriaModel> ListarCategorias();
    }

    public interface IProveedorService
    {
        List<clsProveedorModel> ListarProveedores();
    }

    public interface ISucursalService
    {
        List<clsSucursalModel> ListarSucursales();
    }

    /// <summary>
    /// Por ahora son "pass-through" hacia el Repository (no hay reglas de negocio
    /// adicionales sobre categorías todavía), pero mantenerlas como Service
    /// permite agregar reglas después (ej. cache, orden, permisos) sin tocar la UI.
    /// </summary>
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _repo;
        public CategoriaService(ICategoriaRepository repo = null) => _repo = repo ?? new CategoriaRepository();
        public List<clsCategoriaModel> ListarCategorias() => _repo.ObtenerCategorias();
    }

    public class ProveedorService : IProveedorService
    {
        private readonly IProveedorRepository _repo;
        public ProveedorService(IProveedorRepository repo = null) => _repo = repo ?? new ProveedorRepository();
        public List<clsProveedorModel> ListarProveedores() => _repo.ObtenerProveedores();
    }

    public class SucursalService : ISucursalService
    {
        private readonly ISucursalRepository _repo;
        public SucursalService(ISucursalRepository repo = null) => _repo = repo ?? new SucursalRepository();
        public List<clsSucursalModel> ListarSucursales() => _repo.ObtenerSucursales();
    }
}
