using System.Collections.Generic;
using System.Data;
using TRAMADE.Models;

namespace TRAMADE.Services
{
    /// <summary>Resultado de una eliminación, para que la UI muestre el mensaje correcto.</summary>
    public enum ResultadoEliminacionProducto
    {
        RelacionProveedorEliminada,
        ProductoEliminadoCompleto
    }

    public interface IProductoService
    {
        // ── Listado / búsqueda ────────────────────────────────────────────
        List<clsProductoModel> ObtenerProductosPaginados(string filtro, int pagina, int registrosPorPagina, out int totalPaginas);
        List<clsProductoModel> BuscarProductos(string texto);
        DataTable ObtenerTodosParaExportar(string filtro);
        clsProductoModel ObtenerProductoPorId(int idProducto);
        clsProductoModel ObtenerProductoPorIdYProveedor(int idProducto, string proveedor);
        List<string> ObtenerProveedoresDelProducto(int idProducto);

        // ── Validaciones (idénticas a las que ya existían en la UI) ────────
        bool ValidarRegistro(string nombre, string precioTxt, string costoTxt, string stockTxt,
            object proveedorSeleccionado, object categoriaSeleccionada, out string mensajeError);

        bool ValidarEdicion(string nombre, string precioTxt, string costoTxt, string stockTxt,
            object categoriaSeleccionada, int cantidadProveedoresEnLista, out string mensajeError);

        bool ValidarPrecioCostoStock(string precioTxt, string costoTxt, string stockTxt,
            bool permitirStockCero, out string mensajeError);

        // ── Alta completa (Producto + Sucursal + Proveedor + Código de barras) ─
        (int NuevoId, string CodigoBarras) RegistrarProducto(
            string nombre, string nombreCategoria, string nombreProveedor,
            decimal precio, decimal costo, int stock, byte[] imagen, int idSucursal);

        // ── Edición completa ────────────────────────────────────────────────
        void ActualizarProducto(int idProducto, string nombre, string nombreCategoria,
            decimal precio, decimal costo, int stock, byte[] imagen, int idSucursal,
            string proveedorSeleccionado);

        // ── Gestión de proveedores de un producto ya existente ─────────────
        void AgregarProveedorAProducto(int idProducto, string nombreProveedor, decimal precio, decimal costo, int stock, int idSucursal);
        void QuitarProveedorDeProducto(int idProducto, string nombreProveedor);

        // ── Eliminación (con la regla "si tiene más de un proveedor, solo se quita esa relación") ─
        ResultadoEliminacionProducto EliminarProducto(int idProducto, string proveedorSeleccionado);

        int VerificarProductoExiste(string nombreProducto);
    }
}
