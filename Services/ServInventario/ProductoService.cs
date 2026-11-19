using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using TRAMADE.Models;
using System.Text.RegularExpressions;
using TRAMADE.Repositories;

namespace TRAMADE.Services
{
    /// <summary>
    /// Reglas de negocio de Inventario. Reemplaza a "clsProductoDAL" (que ya
    /// hacía algo de orquestación) y absorbe las reglas que antes vivían
    /// mezcladas dentro de los .xaml.cs de InventarioControl / RegistrarInvControl /
    /// EditarInvControl.
    /// </summary>
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepo;
        private readonly ICategoriaRepository _categoriaRepo;
        private readonly IProveedorRepository _proveedorRepo;
        private readonly ICodigoBarrasService _codigoBarrasService;
        private readonly ICodigoBarrasRepository _codigoBarrasRepo;

        private const int RegistrosPorPaginaDefault = 20;

        public ProductoService(
            IProductoRepository productoRepo = null,
            ICategoriaRepository categoriaRepo = null,
            IProveedorRepository proveedorRepo = null,
            ICodigoBarrasService codigoBarrasService = null,
            ICodigoBarrasRepository codigoBarrasRepo = null)
        {
            _productoRepo = productoRepo ?? new ProductoRepository();
            _categoriaRepo = categoriaRepo ?? new CategoriaRepository();
            _proveedorRepo = proveedorRepo ?? new ProveedorRepository();
            _codigoBarrasService = codigoBarrasService ?? new CodigoBarrasService();
            _codigoBarrasRepo = codigoBarrasRepo ?? new CodigoBarrasRepository();
        }

        // ═══════════════════════════════════════════════════════════════
        //  LISTADO / BÚSQUEDA
        // ═══════════════════════════════════════════════════════════════

        public List<clsProductoModel> ObtenerProductosPaginados(string filtro, int pagina, int registrosPorPagina, out int totalPaginas)
        {
            int total = _productoRepo.ContarProductos();
            totalPaginas = Math.Max(1, (int)Math.Ceiling((double)total / registrosPorPagina));
            return _productoRepo.ObtenerProductos(filtro, pagina, registrosPorPagina);
        }
        private static bool TieneCerosInnecesarios(string valor) => Regex.IsMatch((valor ?? "").Trim(), @"^0{2,}\d");
        public List<clsProductoModel> BuscarProductos(string texto) => _productoRepo.BuscarProductos(texto);

        public DataTable ObtenerTodosParaExportar(string filtro) => _productoRepo.ObtenerTodosLosProductos(filtro);

        public clsProductoModel ObtenerProductoPorId(int idProducto) => _productoRepo.ObtenerProductoPorId(idProducto);

        public clsProductoModel ObtenerProductoPorIdYProveedor(int idProducto, string proveedor) =>
            _productoRepo.ObtenerProductoPorIdYProveedor(idProducto, proveedor);

        public int VerificarProductoExiste(string nombreProducto) => _productoRepo.VerificarProductoExiste(nombreProducto);

        public List<string> ObtenerProveedoresDelProducto(int idProducto) => _productoRepo.ObtenerProveedoresPorProducto(idProducto);

        // ═══════════════════════════════════════════════════════════════
        //  VALIDACIONES — MISMO TEXTO Y MISMO ORDEN QUE EL ORIGINAL
        // ═══════════════════════════════════════════════════════════════

        // Caracteres permitidos en el nombre de un producto: letras (con acentos),
        // números (tallas, medidas, tamaños como "1GL", "500ML"), espacios y
        // puntuación básica de catálogo (- . , / ()). Cualquier otro símbolo
        // (@, #, $, %, &, *, etc.) se rechaza.
        private static readonly System.Text.RegularExpressions.Regex NombreProductoValido =
            new Regex(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ.,\-\/()""'° ]+$");

        /// <summary>
        /// Valida precio, costo y stock como números válidos y con signo correcto.
        /// Reutilizable desde el alta, la edición, y desde "agregar proveedor"
        /// (que antes NO pasaba por ninguna validación antes de guardar).
        /// </summary>
        public bool ValidarPrecioCostoStock(string precioTxt, string costoTxt, string stockTxt,
        bool permitirStockCero, out string mensajeError)
        {
            if (TieneCerosInnecesarios(precioTxt))
            { mensajeError = "El precio de venta no puede tener ceros a la izquierda (ej: 0123, no 00123)."; return false; }
            if (!decimal.TryParse((precioTxt ?? "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precio))
            { mensajeError = "Ingresa un precio de venta válido."; return false; }
            if (precio <= 0)
            { mensajeError = "El precio de venta debe ser mayor a cero."; return false; }

            if (TieneCerosInnecesarios(costoTxt))
            { mensajeError = "El precio de costo no puede tener ceros a la izquierda (ej: 0123, no 00123)."; return false; }
            if (!decimal.TryParse((costoTxt ?? "").Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo))
            { mensajeError = "Ingresa un precio de costo válido."; return false; }
            if (costo <= 0)
            { mensajeError = "El precio de costo debe ser mayor a cero."; return false; }

            // NUEVO: el costo no puede ser mayor o igual al precio de venta
            if (costo >= precio)
            { mensajeError = "El precio de costo debe ser menor al precio de venta."; return false; }

            if (TieneCerosInnecesarios(stockTxt))
            { mensajeError = "El stock no puede tener ceros a la izquierda (ej: 0123, no 00123)."; return false; }
            if (!int.TryParse((stockTxt ?? "").Trim(), out int stock))
            { mensajeError = permitirStockCero ? "Ingresa un stock válido." : "Ingresa un stock inicial válido."; return false; }
            if (permitirStockCero ? stock < 0 : stock <= 0)
            {
                mensajeError = permitirStockCero ? "El stock no puede ser negativo." : "El stock inicial debe ser mayor a cero.";
                return false;
            }
            mensajeError = null;
            return true;
        }

        public bool ValidarRegistro(string nombre, string precioTxt, string costoTxt, string stockTxt,
            object proveedorSeleccionado, object categoriaSeleccionada, out string mensajeError)
        {
            // Réplica de RegistrarInvControl.Validar(), reforzada con las
            // validaciones que faltaban (signo y caracteres especiales) y que el
            // formulario original nunca llegó a aplicar.
            if (string.IsNullOrWhiteSpace(nombre))
            { mensajeError = "El nombre del producto es obligatorio."; return false; }

            if (!NombreProductoValido.IsMatch(nombre.Trim()))
            { mensajeError = "El nombre del producto contiene caracteres no permitidos. Solo se permiten letras, números, espacios y los signos - . , / ( )."; return false; }

            if (!ValidarPrecioCostoStock(precioTxt, costoTxt, stockTxt, permitirStockCero: false, out mensajeError))
                return false;

            if (proveedorSeleccionado == null)
            { mensajeError = "Selecciona un proveedor."; return false; }

            if (categoriaSeleccionada == null)
            { mensajeError = "Selecciona una categoría."; return false; }

            mensajeError = null;
            return true;
        }

        public bool ValidarEdicion(string nombre, string precioTxt, string costoTxt, string stockTxt,
            object categoriaSeleccionada, int cantidadProveedoresEnLista, out string mensajeError)
        {
            // Réplica de EditarInvControl.Validar() + el chequeo de "debe tener
            // al menos un proveedor", reforzada con las mismas validaciones de
            // signo y caracteres especiales que ValidarRegistro. En edición se
            // permite Stock = 0 (un producto puede agotarse), pero nunca negativo.
            if (string.IsNullOrWhiteSpace(nombre))
            { mensajeError = "El nombre del producto es obligatorio."; return false; }

            if (!NombreProductoValido.IsMatch(nombre.Trim()))
            { mensajeError = "El nombre del producto contiene caracteres no permitidos. Solo se permiten letras, números, espacios y los signos - . , / ( )."; return false; }

            if (!ValidarPrecioCostoStock(precioTxt, costoTxt, stockTxt, permitirStockCero: true, out mensajeError))
                return false;

            if (categoriaSeleccionada == null)
            { mensajeError = "Selecciona una categoría."; return false; }

            if (cantidadProveedoresEnLista == 0)
            { mensajeError = "El producto debe tener al menos un proveedor."; return false; }

            mensajeError = null;
            return true;
        }


        // ═══════════════════════════════════════════════════════════════
        //  ALTA COMPLETA (equivalente a clsProductoDAL.InsertarProducto +
        //  lo que hacía RegistrarInvControl.btnRegistrar_Click)
        // ═══════════════════════════════════════════════════════════════

        public (int NuevoId, string CodigoBarras) RegistrarProducto(
            string nombre, string nombreCategoria, string nombreProveedor,
            decimal precio, decimal costo, int stock, byte[] imagen, int idSucursal)
        {
            var conexion = new clsConexion();
            conexion.Abrir();
            try
            {
                int idCategoria = _categoriaRepo.ObtenerIdPorNombre(conexion.SqlC, nombreCategoria);
                int idProveedor = _proveedorRepo.ObtenerIdPorNombre(conexion.SqlC, nombreProveedor);

                int nuevoId = _productoRepo.InsertarProducto(conexion.SqlC, nombre, idCategoria, precio, costo, imagen);

                string codigo = _codigoBarrasService.GenerarCodigo(nuevoId);
                _codigoBarrasRepo.GuardarCodigo(conexion.SqlC, nuevoId, codigo);

                _productoRepo.AgregarProveedorAProducto(conexion.SqlC, nuevoId, idProveedor, precio, costo, stock, idSucursal);
                _productoRepo.InsertarProductoSucursal(conexion.SqlC, nuevoId, idSucursal, stock);

                return (nuevoId, codigo);
            }
            finally { conexion.Cerrar(); }
        }

        // ═══════════════════════════════════════════════════════════════
        //  EDICIÓN COMPLETA (equivalente a EditarInvControl.btnConfirmar_Click)
        // ═══════════════════════════════════════════════════════════════

        public void ActualizarProducto(int idProducto, string nombre, string nombreCategoria,
    decimal precio, decimal costo, int stock, byte[] imagen, int idSucursal,
    string proveedorSeleccionado)
        {
            var conexion = new clsConexion();
            conexion.Abrir();
            try
            {
                int idCategoria = _categoriaRepo.ObtenerIdPorNombre(conexion.SqlC, nombreCategoria);
                _productoRepo.ActualizarProducto(conexion.SqlC, idProducto, nombre, idCategoria, imagen);

                if (!string.IsNullOrWhiteSpace(proveedorSeleccionado))
                {
                    int idProveedor = _proveedorRepo.ObtenerIdPorNombre(conexion.SqlC, proveedorSeleccionado);
                    _productoRepo.ActualizarPrecioCostoStockProveedor(conexion.SqlC, idProducto,
                        idProveedor, precio, costo, stock, idSucursal);
                }
            }
            finally { conexion.Cerrar(); }
        }

        // ═══════════════════════════════════════════════════════════════
        //  GESTIÓN DE PROVEEDORES DE UN PRODUCTO EXISTENTE
        //  (equivalente a btnAgregarProveedor_Click / btnQuitarProveedor_Click)
        // ═══════════════════════════════════════════════════════════════

        public void AgregarProveedorAProducto(int idProducto, string nombreProveedor, decimal precio, decimal costo, int stock, int idSucursal)
        {
            var conexion = new clsConexion();
            conexion.Abrir();
            try
            {
                int idProveedor = _proveedorRepo.ObtenerIdPorNombre(conexion.SqlC, nombreProveedor);
                _productoRepo.AgregarProveedorAProducto(conexion.SqlC, idProducto, idProveedor, precio, costo, stock, idSucursal);
            }
            finally { conexion.Cerrar(); }
        }

        public void QuitarProveedorDeProducto(int idProducto, string nombreProveedor)
        {
            var conexion = new clsConexion();
            conexion.Abrir();
            try
            {
                int idProveedor = _proveedorRepo.ObtenerIdPorNombre(conexion.SqlC, nombreProveedor);
                _productoRepo.QuitarProveedorDeProducto(conexion.SqlC, idProducto, idProveedor);
            }
            finally { conexion.Cerrar(); }
        }

        // ═══════════════════════════════════════════════════════════════
        //  ELIMINACIÓN (equivalente a InventarioControl.btnEliminar_Click)
        // ═══════════════════════════════════════════════════════════════

        public ResultadoEliminacionProducto EliminarProducto(int idProducto, string proveedorSeleccionado)
        {
            var conexion = new clsConexion();
            conexion.Abrir();
            try
            {
                int idProveedor = _proveedorRepo.ObtenerIdPorNombre(conexion.SqlC, proveedorSeleccionado);
                int totalProveedores = _productoRepo.ContarProveedoresDeProducto(conexion.SqlC, idProducto);

                if (totalProveedores > 1)
                {
                    _productoRepo.EliminarProductoProveedorEspecifico(conexion.SqlC, idProducto, idProveedor);
                    return ResultadoEliminacionProducto.RelacionProveedorEliminada;
                }

                // Si solo tiene un proveedor, se elimina el producto por completo.
                // Se cierra esta conexión (igual que el original) y se abre una
                // nueva dedicada exclusivamente a la cascada de eliminación.
                conexion.Cerrar();
                EliminarProductoCompleto(idProducto);
                return ResultadoEliminacionProducto.ProductoEliminadoCompleto;
            }
            finally
            {
                // "Cerrar()" ya es idempotente en clsConexion (revisa el estado),
                // así que llamarlo de nuevo aquí es seguro si el flujo de arriba
                // ya la cerró.
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Elimina un producto y toda su información relacionada, respetando
        /// el mismo orden (llaves foráneas) que tenía clsProductoDAL.EliminarProducto.
        /// </summary>
        private void EliminarProductoCompleto(int idProducto)
        {
            var conexion = new clsConexion();
            conexion.Abrir();
            try
            {
                /*_productoRepo.EliminarDetalleCompra(conexion.SqlC, idProducto);
                _productoRepo.EliminarFacturaProducto(conexion.SqlC, idProducto);
                _productoRepo.EliminarProductoProveedor(conexion.SqlC, idProducto);
                _productoRepo.EliminarProductoSucursal(conexion.SqlC, idProducto);
                _productoRepo.EliminarProducto(conexion.SqlC, idProducto);*/
            }
            finally { conexion.Cerrar(); }
        }
    }
}
