using System;
using System.Data;
using TRAMADE.Models.ModelsCompras;

namespace TRAMADE.Repositories.RepoCompras
{
    public interface ICompraRepository
    {
        DataTable ObtenerProveedores();
        DataTable ObtenerFormasPago();
        DataTable ObtenerProductosPorSucursal(int idSucursal);
        DataTable ObtenerCompraPorId(int idCompra);
        DataTable ObtenerCompras();
        DataTable ObtenerComprasPorId(int idCompra);
        DataTable ObtenerComprasPorFecha(DateTime desde, DateTime hastaExclusiva);
        DataTable ObtenerComprasRecientes();
        DataTable ObtenerTopProductos();
        DataTable ObtenerTopProveedores();
        DataTable ObtenerEstadoCompras();
        IndicadoresCompraModel ObtenerIndicadores();
        int RegistrarCompra(CompraSolicitudModel compra);
        void ActualizarCompra(CompraSolicitudModel compra);
        bool AutorizarCompra(int idCompra);
        bool DenegarCompra(int idCompra);
        bool RecibirCompra(int idCompra, int idUsuario, int idSucursal);
    }
}
