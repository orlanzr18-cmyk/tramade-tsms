/*using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace TRAMADE.ClasesInventario
{
    /// <summary>
    /// Capa de Acceso a Datos (DAL) para la entidad Producto en el módulo de Inventario.
    /// Actúa como intermediario entre la lógica de negocio y la capa de consultas SQL
    /// (<see cref="clsConsultasInv"/>), proporcionando una interfaz limpia para las
    /// operaciones CRUD y de consulta sobre productos.
    /// </summary>
    /// <remarks>
    /// Todos los métodos son estáticos. Las operaciones de eliminación utilizan
    /// una única conexión compartida (<see cref="clsConexion"/>) para garantizar
    /// la integridad transaccional al eliminar registros relacionados en múltiples tablas.
    /// </remarks>
    internal class clsProductoDAL
    {
        /// <summary>
        /// Obtiene una página de productos del inventario, opcionalmente filtrados,
        /// utilizando paginación del lado del servidor.
        /// </summary>
        /// <param name="filtro">
        /// Cadena de texto para filtrar los productos por nombre u otros criterios.
        /// Pase una cadena vacía para obtener todos los productos sin filtro.
        /// </param>
        /// <param name="pagina">
        /// Número de página solicitado (base 1). Se usa para calcular el offset en la consulta.
        /// </param>
        /// <param name="registrosPorPagina">
        /// Cantidad de registros a retornar por página.
        /// </param>
        /// <returns>
        /// Un <see cref="DataTable"/> con los productos correspondientes a la página solicitada.
        /// </returns>
        /// <remarks>
        /// Delega la ejecución al procedimiento almacenado <c>PA_OBTENER_PRODUCTOS</c>
        /// a través de <see cref="clsConsultasInv.ObtenerProductos"/>.
        /// </remarks>
        /// 
        /// <summary>
        /// Inserta un producto y lo asigna automáticamente a la sucursal del usuario en sesión.
        /// </summary>
        public static int InsertarProducto(string nombre, int idCategoria,
     decimal precio, decimal costo, byte[] imagen, int stock,
     List<int> idsProveedores)
        {
            clsConexion obj = new clsConexion();
            obj.Abrir();
            try
            {
                int nuevoId = clsConsultasInv.InsertarProducto(
                    obj.SqlC, nombre, idCategoria, precio, costo, imagen);

                clsConsultasInv.InsertarProductoSucursal(
                    obj.SqlC, nuevoId, clsSesion.id_sucursal, stock);

                foreach (int idProveedor in idsProveedores)
                {
                    clsConsultasInv.AgregarProveedorAProducto(
                        obj.SqlC, nuevoId, idProveedor,
                        precio, costo, stock, clsSesion.id_sucursal); // ← agregar parámetros
                }

                return nuevoId;
            }
            finally
            {
                obj.Cerrar();
            }
        }
        public static DataTable ObtenerProductos(string filtro, int pagina, int registrosPorPagina)
        {
            return clsConsultasInv.ObtenerProductos(filtro, pagina, registrosPorPagina);
        }

        /// <summary>
        /// Retorna el número total de productos registrados en el inventario.
        /// </summary>
        /// <returns>
        /// Entero con la cantidad total de productos en la tabla <c>PRODUCTO</c>.
        /// </returns>
        /// <remarks>
        /// Se utiliza principalmente para calcular el total de páginas en controles
        /// de paginación. Delega en <see cref="clsConsultasInv.ContarProductos"/>.
        /// </remarks>
        public static int ContarProductos()
        {
            return clsConsultasInv.ContarProductos();
        }

        /// <summary>
        /// Busca productos por ID numérico o por nombre según el texto ingresado.
        /// </summary>
        /// <param name="busqueda">
        /// Texto de búsqueda. Si el valor es un número entero válido, busca por ID;
        /// de lo contrario, realiza búsqueda por nombre (coincidencia parcial).
        /// </param>
        /// <returns>
        /// Un <see cref="DataTable"/> con los productos que coinciden con el criterio de búsqueda.
        /// Puede retornar un resultado vacío si no hay coincidencias.
        /// </returns>
        /// <remarks>
        /// Delega en <see cref="clsConsultasInv.BuscarProductos"/>, que internamente
        /// decide entre los procedimientos <c>PA_BUSCAR_PRODUCTO_POR_ID</c> y
        /// <c>PA_BUSCAR_PRODUCTO_POR_NOMBRE</c> según el tipo de entrada.
        /// </remarks>
        public static DataTable BuscarProductos(string busqueda)
        {
            return clsConsultasInv.BuscarProductos(busqueda);
        }

        /// <summary>
        /// Elimina un producto del sistema junto con todos sus registros relacionados
        /// en otras tablas, garantizando la integridad referencial.
        /// </summary>
        /// <param name="idProducto">
        /// Identificador único del producto a eliminar.
        /// </param>
        /// <remarks>
        /// La eliminación se realiza en el siguiente orden para respetar las
        /// claves foráneas de la base de datos:
        /// <list type="number">
        ///   <item><description>Detalle de compras (<c>PA_ELIMINAR_DETALLE_COMPRA_PRODUCTO</c>)</description></item>
        ///   <item><description>Facturas relacionadas (<c>PA_ELIMINAR_FACTURA_PRODUCTO</c>)</description></item>
        ///   <item><description>Relación producto-proveedor (<c>PA_ELIMINAR_PRODUCTO_PROVEEDOR</c>)</description></item>
        ///   <item><description>Relación producto-sucursal (<c>PA_ELIMINAR_PRODUCTO_SUCURSAL</c>)</description></item>
        ///   <item><description>Registro del producto (<c>PA_ELIMINAR_PRODUCTO</c>)</description></item>
        /// </list>
        /// Usa una única conexión <see cref="clsConexion"/> abierta durante toda la operación
        /// y la cierra en el bloque <c>finally</c> para asegurar su liberación ante cualquier error.
        /// </remarks>
        public static void EliminarProducto(int idProducto)
        {
            clsConexion obj = new clsConexion();
            obj.Abrir();
            try
            {
                clsConsultasInv.EliminarDetalleCompra(obj.SqlC, idProducto);
                clsConsultasInv.EliminarFacturaProducto(obj.SqlC, idProducto);
                clsConsultasInv.EliminarProductoProveedor(obj.SqlC, idProducto);
                clsConsultasInv.EliminarProductoSucursal(obj.SqlC, idProducto);
                clsConsultasInv.EliminarProducto(obj.SqlC, idProducto);
            }
            finally
            {
                obj.Cerrar();
            }
        }

        /// <summary>
        /// Obtiene el identificador de una categoría a partir de su nombre.
        /// </summary>
        /// <param name="con">Conexión SQL activa a utilizar en la consulta.</param>
        /// <param name="nombre">Nombre exacto de la categoría a buscar.</param>
        /// <returns>
        /// Entero con el <c>id_categoria</c> correspondiente al nombre proporcionado.
        /// </returns>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado <c>PA_OBTENER_ID_CATEGORIA</c>.
        /// Requiere que la conexión <paramref name="con"/> esté previamente abierta.
        /// </remarks>
        public static int ObtenerIdCategoria(SqlConnection con, string nombre)
        {
            return clsConsultasInv.ObtenerIdCategoria(con, nombre);
        }

        /// <summary>
        /// Obtiene el identificador de un proveedor a partir de su nombre.
        /// </summary>
        /// <param name="con">Conexión SQL activa a utilizar en la consulta.</param>
        /// <param name="nombre">Nombre exacto del proveedor a buscar.</param>
        /// <returns>
        /// Entero con el <c>id_proveedor</c> correspondiente al nombre proporcionado.
        /// </returns>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado <c>PA_OBTENER_ID_PROVEEDOR</c>.
        /// Requiere que la conexión <paramref name="con"/> esté previamente abierta.
        /// </remarks>
        public static int ObtenerIdProveedor(SqlConnection con, string nombre)
        {
            return clsConsultasInv.ObtenerIdProveedor(con, nombre);
        }

        /// <summary>
        /// Obtiene el identificador de una sucursal a partir de su nombre.
        /// </summary>
        /// <param name="con">Conexión SQL activa a utilizar en la consulta.</param>
        /// <param name="nombre">Nombre exacto de la sucursal a buscar.</param>
        /// <returns>
        /// Entero con el <c>id_sucursal</c> correspondiente al nombre proporcionado.
        /// </returns>
        /// <remarks>
        /// Ejecuta el procedimiento almacenado <c>PA_OBTENER_ID_SUCURSAL</c>.
        /// Requiere que la conexión <paramref name="con"/> esté previamente abierta.
        /// </remarks>
        public static int ObtenerIdSucursal(SqlConnection con, string nombre)
        {
            return clsConsultasInv.ObtenerIdSucursal(con, nombre);
        }
    }
}
*/