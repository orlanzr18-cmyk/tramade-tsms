using System;
using System.Data;
using TRAMADE.Models.ModelsCompras;

namespace TRAMADE.Services.ServCompras
{
    public interface ICompraService
    {
        DataTable ObtenerProveedores();
        DataTable ObtenerFormasPago();
        DataTable ObtenerProductosPorSucursal(int idSucursal);
        DataTable ObtenerCompraPorId(int idCompra);
        DataTable ObtenerCompras();
        DataTable ObtenerComprasSeguimiento(int? idCompra, DateTime desde, DateTime hasta);
        DataTable ObtenerComprasRecientes(int cantidad);
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
