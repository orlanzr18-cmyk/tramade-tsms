using System;
using System.Data;
using TRAMADE.Models.ModelsCompras;
using TRAMADE.Repositories.RepoCompras;

namespace TRAMADE.Services.ServCompras
{
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _repositorio;

        public CompraService(ICompraRepository repositorio = null)
        {
            _repositorio = repositorio ?? new CompraRepository();
        }

        public DataTable ObtenerProveedores()
        {
            return _repositorio.ObtenerProveedores();
        }

        public DataTable ObtenerFormasPago()
        {
            return _repositorio.ObtenerFormasPago();
        }

        public DataTable ObtenerProductosPorSucursal(int idSucursal)
        {
            return _repositorio.ObtenerProductosPorSucursal(idSucursal);
        }

        public DataTable ObtenerCompraPorId(int idCompra)
        {
            return _repositorio.ObtenerCompraPorId(idCompra);
        }

        public DataTable ObtenerCompras()
        {
            return _repositorio.ObtenerCompras();
        }

        public DataTable ObtenerComprasSeguimiento(int? idCompra, DateTime desde, DateTime hasta)
        {
            if (idCompra.HasValue)
                return _repositorio.ObtenerComprasPorId(idCompra.Value);
            return _repositorio.ObtenerComprasPorFecha(desde.Date, hasta.Date.AddDays(1));
        }

        public DataTable ObtenerComprasRecientes(int cantidad)
        {
            DataTable origen = _repositorio.ObtenerComprasRecientes();
            DataTable resultado = origen.Clone();
            DataView vista = origen.DefaultView;
            if (origen.Columns.Contains("Fecha pedido") && origen.Columns.Contains("ID compra"))
                vista.Sort = "[Fecha pedido] DESC, [ID compra] DESC";

            for (int i = 0; i < vista.Count && i < cantidad; i++)
                resultado.ImportRow(vista[i].Row);
            return resultado;
        }

        public DataTable ObtenerTopProductos()
        {
            return _repositorio.ObtenerTopProductos();
        }

        public DataTable ObtenerTopProveedores()
        {
            return _repositorio.ObtenerTopProveedores();
        }

        public DataTable ObtenerEstadoCompras()
        {
            return _repositorio.ObtenerEstadoCompras();
        }

        public IndicadoresCompraModel ObtenerIndicadores()
        {
            return _repositorio.ObtenerIndicadores();
        }

        public int RegistrarCompra(CompraSolicitudModel compra)
        {
            if (compra == null) throw new ArgumentNullException("compra");
            return _repositorio.RegistrarCompra(compra);
        }

        public void ActualizarCompra(CompraSolicitudModel compra)
        {
            if (compra == null) throw new ArgumentNullException("compra");
            _repositorio.ActualizarCompra(compra);
        }

        public bool AutorizarCompra(int idCompra)
        {
            return _repositorio.AutorizarCompra(idCompra);
        }

        public bool DenegarCompra(int idCompra)
        {
            return _repositorio.DenegarCompra(idCompra);
        }

        public bool RecibirCompra(int idCompra, int idUsuario, int idSucursal)
        {
            return _repositorio.RecibirCompra(idCompra, idUsuario, idSucursal);
        }
    }
}
