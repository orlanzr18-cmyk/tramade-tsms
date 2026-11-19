using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using TRAMADE.Models;

namespace TRAMADE.Repositories
{
    public interface IProductoRepository
    {
        List<clsProductoModel> ObtenerProductos(string filtro, int pagina, int registrosPorPagina);
        int ContarProductos();
        DataTable ObtenerTodosLosProductos(string filtro);
        List<clsProductoModel> BuscarProductos(string busqueda);
        clsProductoModel ObtenerProductoPorId(int idProducto);
        clsProductoModel ObtenerProductoPorIdYProveedor(int idProducto, string proveedor);
        List<string> ObtenerProveedoresPorProducto(int idProducto);
        int VerificarProductoExiste(string nombreProducto);
        string ObtenerCodigoBarras(int idProducto);

        int InsertarProducto(SqlConnection con, string nombre, int idCategoria, decimal precio, decimal costo, byte[] imagen);

        
        void ActualizarProducto(SqlConnection con, int idProducto, string nombre, int idCategoria, byte[] imagen);

        void InsertarProductoSucursal(SqlConnection con, int idProducto, int idSucursal, int stock);

        
        void ActualizarProductoSucursal(SqlConnection con, int idProducto, int idSucursal, int stock);

        bool ExisteEnProductoSucursal(SqlConnection con, int idProducto);
        void AgregarProveedorAProducto(SqlConnection con, int idProducto, int idProveedor, decimal precioUnitario, decimal precioCosto, int stock, int idSucursal);
        void QuitarProveedorDeProducto(SqlConnection con, int idProducto, int idProveedor);
        void EliminarProductoProveedorEspecifico(SqlConnection con, int idProducto, int idProveedor);
        int ContarProveedoresDeProducto(SqlConnection con, int idProducto);
        void ActualizarPrecioCostoStockProveedor(SqlConnection con, int idProducto, int idProveedor, decimal precio, decimal costo, int stock, int idSucursal);
        void EliminarProducto(SqlConnection con, int idProducto);
    }
}