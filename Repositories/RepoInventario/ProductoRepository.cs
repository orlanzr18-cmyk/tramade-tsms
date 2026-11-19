using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TRAMADE.Models;

namespace TRAMADE.Repositories
{
    /// <summary>
    /// Repositorio de Productos. Es el reemplazo directo de "clsConsultasInv"
    /// para todo lo relacionado a PRODUCTO / PRODUCTO_SUCURSAL / PRODUCTO_PROVEEDOR.
    /// Mismos Stored Procedures, mismos parámetros — solo se reorganiza el código
    /// y se mapea a Models en vez de DataTable "a mano" en la UI.
    /// </summary>
    public class ProductoRepository : IProductoRepository
    {
        // clsConexion vive en el namespace raíz TRAMADE y la usan todos los módulos.
        // No se modifica su contrato: se sigue usando Abrir()/Cerrar()/SqlC tal cual.
        private clsConexion NuevaConexion() => new clsConexion();

        public List<clsProductoModel> ObtenerProductos(string filtro, int pagina, int registrosPorPagina)
        {
            var obj = NuevaConexion();
            obj.Abrir();
            try
            {
                int offset = (pagina - 1) * registrosPorPagina;
                var cmd = new SqlCommand("PA_OBTENER_PRODUCTOS", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@filtro", filtro);
                cmd.Parameters.AddWithValue("@offset", offset);
                cmd.Parameters.AddWithValue("@cantidad", registrosPorPagina);
                cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);

                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().Select(MapearDesdeFila).ToList();
            }
            finally { obj.Cerrar(); }
        }

        public int ContarProductos()
        {
            var obj = NuevaConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_CONTAR_PRODUCTOS", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);
                return (int)cmd.ExecuteScalar();
            }
            finally { obj.Cerrar(); }
        }

        public DataTable ObtenerTodosLosProductos(string filtro)
        {
            var obj = NuevaConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_OBTENER_TODOS_PRODUCTOS", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@filtro", filtro);
                cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
            finally { obj.Cerrar(); }
        }

        public List<clsProductoModel> BuscarProductos(string busqueda)
        {
            var obj = NuevaConexion();
            obj.Abrir();
            try
            {
                SqlCommand cmd;
                if (int.TryParse(busqueda, out int idBuscar))
                {
                    cmd = new SqlCommand("PA_BUSCAR_PRODUCTO_POR_ID", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@id", idBuscar);
                    cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);
                }
                else
                {
                    cmd = new SqlCommand("PA_BUSCAR_PRODUCTO_POR_NOMBRE", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@busqueda", busqueda);
                    cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);
                }
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable().Select(MapearDesdeFila).ToList();
            }
            finally { obj.Cerrar(); }
        }

        public clsProductoModel ObtenerProductoPorId(int idProducto)
        {
            var obj = NuevaConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_OBTENER_PRODUCTO_POR_ID", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@id", idProducto);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.Rows.Count > 0 ? MapearDesdeFilaEdicion(dt.Rows[0]) : null;
            }
            finally { obj.Cerrar(); }
        }

        public clsProductoModel ObtenerProductoPorIdYProveedor(int idProducto, string proveedor)
        {
            var obj = NuevaConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_OBTENER_PRODUCTO_POR_ID_PROVEEDOR", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@id", idProducto);
                cmd.Parameters.AddWithValue("@proveedor", proveedor);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.Rows.Count > 0 ? MapearDesdeFilaEdicion(dt.Rows[0]) : null;
            }
            finally { obj.Cerrar(); }
        }

        public List<string> ObtenerProveedoresPorProducto(int idProducto)
        {
            var obj = NuevaConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_OBTENER_PROVEEDORES_POR_PRODUCTO", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@idProducto", idProducto);
                var dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.AsEnumerable()
                    .Select(r => r["nombre_comercial_proveedor"].ToString())
                    .ToList();
            }
            finally { obj.Cerrar(); }
        }

        public int VerificarProductoExiste(string nombreProducto)
        {
            var obj = NuevaConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_VERIFICAR_PRODUCTO_EXISTE", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@nombre", nombreProducto);
                return (int)cmd.ExecuteScalar();
            }
            finally { obj.Cerrar(); }
        }

        public string ObtenerCodigoBarras(int idProducto)
        {
            var obj = NuevaConexion();
            obj.Abrir();
            try
            {
                var cmd = new SqlCommand("PA_OBTENER_CODIGO_BARRAS", obj.SqlC) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@idProducto", idProducto);
                object resultado = cmd.ExecuteScalar();
                return resultado != null && resultado != DBNull.Value ? resultado.ToString() : string.Empty;
            }
            finally { obj.Cerrar(); }
        }

        // ── Transaccionales (conexión ya abierta, provista por el Service) ───

        public int InsertarProducto(SqlConnection con, string nombre, int idCategoria, decimal precio, decimal costo, byte[] imagen)
        {
            var cmd = new SqlCommand("PA_INSERTAR_PRODUCTO", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@id_categoria", idCategoria);
            cmd.Parameters.AddWithValue("@nombre_producto", nombre);
            cmd.Parameters.AddWithValue("@precio_unitario", precio);
            cmd.Parameters.AddWithValue("@precio_costo", costo);
            cmd.Parameters.AddWithValue("@id_usuario", clsSesion.id_usuario);
            // Nunca usar AddWithValue para varbinary (igual que en el original)
            cmd.Parameters.Add("@imagen_producto", SqlDbType.VarBinary, -1).Value = (object)imagen ?? DBNull.Value;
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void ActualizarProducto(SqlConnection con, int idProducto, string nombre, int idCategoria, byte[] imagen)
        {
            var cmd = new SqlCommand("PA_ACTUALIZAR_TABLA_PRODUCTO", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@id_producto", idProducto);
            cmd.Parameters.AddWithValue("@id_categoria", idCategoria);
            cmd.Parameters.AddWithValue("@nombre_producto", nombre);
            cmd.Parameters.AddWithValue("@id_usuario", clsSesion.id_usuario);
            cmd.Parameters.Add("@imagen_producto", SqlDbType.VarBinary).Value = (object)imagen ?? DBNull.Value;
            cmd.ExecuteNonQuery();
        }

        public void InsertarProductoSucursal(SqlConnection con, int idProducto, int idSucursal, int stock)
        {
            var cmd = new SqlCommand("PA_INSERTAR_PRODUCTO_SUCURSAL", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmd.Parameters.AddWithValue("@stock", stock);
            cmd.ExecuteNonQuery();
        }

        public void ActualizarProductoSucursal(SqlConnection con, int idProducto, int idSucursal, int stock)
        {
            var cmd = new SqlCommand("PA_ACTUALIZAR_PRODUCTO_SUCURSAL", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmd.Parameters.AddWithValue("@stock", stock);
            cmd.ExecuteNonQuery();
        }

        public bool ExisteEnProductoSucursal(SqlConnection con, int idProducto)
        {
            // Misma verificación puntual que había embebida en EditarInvControl,
            // ahora vive en el Repository (que es quien debe tocar SQL).
            var cmd = new SqlCommand("SELECT COUNT(*) FROM PRODUCTO_SUCURSAL WHERE id_producto = @id", con);
            cmd.Parameters.AddWithValue("@id", idProducto);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public void AgregarProveedorAProducto(SqlConnection con, int idProducto, int idProveedor, decimal precioUnitario, decimal precioCosto, int stock, int idSucursal)
        {
            var cmd = new SqlCommand("PA_AGREGAR_PROVEEDOR_PRODUCTO", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.Parameters.AddWithValue("@idProveedor", idProveedor);
            cmd.Parameters.AddWithValue("@precioUnitario", precioUnitario);
            cmd.Parameters.AddWithValue("@precioCosto", precioCosto);
            cmd.Parameters.AddWithValue("@stock", stock);
            cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmd.ExecuteNonQuery();
        }

        public void QuitarProveedorDeProducto(SqlConnection con, int idProducto, int idProveedor)
        {
            var cmd = new SqlCommand("PA_QUITAR_PROVEEDOR_PRODUCTO", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.Parameters.AddWithValue("@idProveedor", idProveedor);
            cmd.ExecuteNonQuery();
        }

        public void EliminarProductoProveedorEspecifico(SqlConnection con, int idProducto, int idProveedor)
        {
            var cmd = new SqlCommand("PA_ELIMINAR_PRODUCTO_PROVEEDOR_ESPECIFICO", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.Parameters.AddWithValue("@idProveedor", idProveedor);
            cmd.ExecuteNonQuery();
        }

        public int ContarProveedoresDeProducto(SqlConnection con, int idProducto)
        {
            var cmd = new SqlCommand("SELECT COUNT(*) FROM PRODUCTO_PROVEEDOR WHERE id_producto = @id", con);
            cmd.Parameters.AddWithValue("@id", idProducto);
            return (int)cmd.ExecuteScalar();
        }

        public void ActualizarPrecioCostoStockProveedor(SqlConnection con, int idProducto, int idProveedor, decimal precio, decimal costo, int stock, int idSucursal)
        {
            
            var cmd = new SqlCommand(
                "UPDATE PRODUCTO_PROVEEDOR SET precio_unitario=@precio, precio_costo=@costo, " +
                "cantidad_stock=@stock, id_sucursal=@idSuc " +
                "WHERE id_producto=@idProd AND id_proveedor=@idProv", con);
            cmd.Parameters.AddWithValue("@precio", precio);
            cmd.Parameters.AddWithValue("@costo", costo);
            cmd.Parameters.AddWithValue("@stock", stock);
            cmd.Parameters.AddWithValue("@idSuc", idSucursal);
            cmd.Parameters.AddWithValue("@idProd", idProducto);
            cmd.Parameters.AddWithValue("@idProv", idProveedor);
            cmd.ExecuteNonQuery();
        }

        public void EliminarDetalleCompra(SqlConnection con, int idProducto)
        {
            var cmd = new SqlCommand("PA_ELIMINAR_DETALLE_COMPRA_PRODUCTO", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.ExecuteNonQuery();
        }

        public void EliminarFacturaProducto(SqlConnection con, int idProducto)
        {
            var cmd = new SqlCommand("PA_ELIMINAR_FACTURA_PRODUCTO", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.ExecuteNonQuery();
        }

        public void EliminarProductoProveedor(SqlConnection con, int idProducto)
        {
            var cmd = new SqlCommand("PA_ELIMINAR_PRODUCTO_PROVEEDOR", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.ExecuteNonQuery();
        }

        public void EliminarProductoSucursal(SqlConnection con, int idProducto)
        {
            var cmd = new SqlCommand("PA_ELIMINAR_PRODUCTO_SUCURSAL", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.ExecuteNonQuery();
        }

        public void EliminarProducto(SqlConnection con, int idProducto)
        {
            var cmd = new SqlCommand("PA_ELIMINAR_PRODUCTO", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.ExecuteNonQuery();
        }

        // ── Mapeo DataRow → Model (reemplaza clsProductoVM.DesdeFila) ────────
        private static clsProductoModel MapearDesdeFila(DataRow row) => new clsProductoModel
        {
            Id = ToInt(row, "ID"),
            Nombre = ToStr(row, "Nombre"),
            Proveedor = ToStr(row, "Proveedor"),
            Categoria = ToStr(row, "Categoría"),
            Sucursal = ToStr(row, "Sucursal"),
            PrecioUnitario = ToDecimal(row, "Precio"),
            PrecioCosto = ToDecimal(row, "Costo"),
            Stock = ToInt(row, "Stock"),
            Imagen = row.Table.Columns.Contains("imagen_producto") && row["imagen_producto"] != DBNull.Value
                ? (byte[])row["imagen_producto"] : null
        };

        // Las filas de "ObtenerProductoPorId"/"...PorIdYProveedor" usan otros
        // nombres de columna (Nombre, Precio, Costo, Stock, Categoría) según
        // el .cs original de EditarInvControl — se mapean igual.
        private static clsProductoModel MapearDesdeFilaEdicion(DataRow row) => new clsProductoModel
        {
            Nombre = ToStr(row, "Nombre"),
            PrecioUnitario = ToDecimal(row, "Precio"),
            PrecioCosto = ToDecimal(row, "Costo"),
            Stock = ToInt(row, "Stock"),
            Categoria = ToStr(row, "Categoría"),
            Imagen = row.Table.Columns.Contains("imagen_producto") && row["imagen_producto"] != DBNull.Value
                ? (byte[])row["imagen_producto"] : null
        };

        private static int ToInt(DataRow r, string col) =>
            r.Table.Columns.Contains(col) && r[col] != DBNull.Value ? Convert.ToInt32(r[col]) : 0;

        private static decimal ToDecimal(DataRow r, string col) =>
            r.Table.Columns.Contains(col) && r[col] != DBNull.Value ? Convert.ToDecimal(r[col]) : 0m;

        private static string ToStr(DataRow r, string col) =>
            r.Table.Columns.Contains(col) && r[col] != DBNull.Value ? r[col].ToString() : string.Empty;
    }
}
