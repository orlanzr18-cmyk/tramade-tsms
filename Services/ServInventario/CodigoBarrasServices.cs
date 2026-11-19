using System.Data.SqlClient;
using TRAMADE.Repositories;

namespace TRAMADE.Services
{
    public interface ICodigoBarrasService
    {
        /// <summary>Regla de negocio: formato único de código de barras del sistema.</summary>
        string GenerarCodigo(int idProducto);
        void GuardarCodigo(SqlConnection con, int idProducto, string codigo);
    }

    public class CodigoBarrasService : ICodigoBarrasService
    {
        private readonly ICodigoBarrasRepository _repo;
        public CodigoBarrasService(ICodigoBarrasRepository repo = null) => _repo = repo ?? new CodigoBarrasRepository();

        // Misma regla que tenías en clsCodigoBarras.GenerarCodigo: PRD-00012
        public string GenerarCodigo(int idProducto) => $"PRD-{idProducto:D5}";

        public void GuardarCodigo(SqlConnection con, int idProducto, string codigo) =>
            _repo.GuardarCodigo(con, idProducto, codigo);
    }
}
