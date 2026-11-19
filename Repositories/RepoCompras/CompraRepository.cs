using System;
using System.Data;
using System.Data.SqlClient;
using TRAMADE.Models.ModelsCompras;

namespace TRAMADE.Repositories.RepoCompras
{
    public class CompraRepository : ICompraRepository
    {
        public DataTable ObtenerProveedores()
        {
            return Consultar("SELECT * FROM VistaProveedor");
        }

        public DataTable ObtenerFormasPago()
        {
            return Consultar("SELECT * FROM VistaFormaPago");
        }

        public DataTable ObtenerProductosPorSucursal(int idSucursal)
        {
            return Consultar(
                "SELECT * FROM VistaProducto WHERE id_sucursal = @idSucursal",
                cmd => cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = idSucursal);
        }

        public DataTable ObtenerCompraPorId(int idCompra)
        {
            return Consultar(
                "SELECT * FROM VistaBuscarCompra WHERE id_compra = @id",
                cmd => cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCompra);
        }

        public DataTable ObtenerCompras()
        {
            return EjecutarProcedimientoTabla("PA_OBTENER_COMPRAS");
        }

        public DataTable ObtenerComprasPorId(int idCompra)
        {
            return Consultar(
                "SELECT * FROM VistaComprasTabla WHERE [ID compra] = @id ORDER BY [Fecha pedido] DESC",
                cmd => cmd.Parameters.Add("@id", SqlDbType.Int).Value = idCompra);
        }

        public DataTable ObtenerComprasPorFecha(DateTime desde, DateTime hastaExclusiva)
        {
            return Consultar(
                @"SELECT * FROM VistaComprasTabla
                  WHERE [Fecha pedido] >= @desde
                    AND [Fecha pedido] < @hasta
                  ORDER BY [Fecha pedido] DESC",
                cmd =>
                {
                    cmd.Parameters.Add("@desde", SqlDbType.DateTime).Value = desde;
                    cmd.Parameters.Add("@hasta", SqlDbType.DateTime).Value = hastaExclusiva;
                });
        }

        public DataTable ObtenerComprasRecientes()
        {
            return Consultar("SELECT * FROM VistaComprasRecientes");
        }

        public DataTable ObtenerTopProductos()
        {
            return Consultar("SELECT Producto, TotalComprado FROM VistaTop5ProductosComprados");
        }

        public DataTable ObtenerTopProveedores()
        {
            return Consultar("SELECT Proveedor, TotalOrdenes FROM VistaTop5ProveedoresMasUsados");
        }

        public DataTable ObtenerEstadoCompras()
        {
            return Consultar(
                "SELECT ComprasPendientes, ComprasAutorizadas FROM VistaGraficoComprasPendientes");
        }

        public IndicadoresCompraModel ObtenerIndicadores()
        {
            return new IndicadoresCompraModel
            {
                SolicitudesRegistradas = EjecutarEscalar("PA_TOTAL_SOLICITUDES", "0"),
                ValorTotalPendiente = EjecutarEscalar("PA_VALOR_PENDIENTE", "0.00"),
                TiempoRespuestaPromedio = EjecutarEscalar("PA_TIEMPO_ESPERA_PROMEDIO", "0")
            };
        }

        public int RegistrarCompra(CompraSolicitudModel compra)
        {
            clsConexion conexion = new clsConexion();
            SqlTransaction transaccion = null;
            try
            {
                conexion.Abrir();
                transaccion = conexion.SqlC.BeginTransaction();
                int idCompra;
                using (SqlCommand cmd = new SqlCommand("PA_INSERTAR_COMPRA", conexion.SqlC, transaccion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AgregarParametrosCompra(cmd, compra, false);
                    idCompra = Convert.ToInt32(cmd.ExecuteScalar());
                }

                GuardarDetalles(conexion.SqlC, transaccion, idCompra, compra.IdProveedor, compra);
                transaccion.Commit();
                return idCompra;
            }
            catch
            {
                if (transaccion != null) transaccion.Rollback();
                throw;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        public void ActualizarCompra(CompraSolicitudModel compra)
        {
            clsConexion conexion = new clsConexion();
            SqlTransaction transaccion = null;
            try
            {
                conexion.Abrir();
                transaccion = conexion.SqlC.BeginTransaction();
                using (SqlCommand cmd = new SqlCommand("PA_ACTUALIZAR_COMPRA", conexion.SqlC, transaccion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    AgregarParametrosCompra(cmd, compra, true);
                    cmd.ExecuteScalar();
                }

                using (SqlCommand cmd = new SqlCommand("PA_ELIMINAR_DETALLE_COMPRA", conexion.SqlC, transaccion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id_compra", SqlDbType.Int).Value = compra.IdCompra;
                    cmd.ExecuteNonQuery();
                }

                GuardarDetalles(conexion.SqlC, transaccion, compra.IdCompra, compra.IdProveedor, compra);
                transaccion.Commit();
            }
            catch
            {
                if (transaccion != null) transaccion.Rollback();
                throw;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        public bool AutorizarCompra(int idCompra)
        {
            return EjecutarEstadoCompra("PA_AUTORIZAR_COMPRA", idCompra);
        }

        public bool DenegarCompra(int idCompra)
        {
            return EjecutarEstadoCompra("PA_DENEGAR_COMPRA", idCompra);
        }

        public bool RecibirCompra(int idCompra, int idUsuario, int idSucursal)
        {
            clsConexion conexion = new clsConexion();
            try
            {
                conexion.Abrir();
                using (SqlCommand cmd = new SqlCommand("PA_RECIBIR_COMPRA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id_compra", SqlDbType.Int).Value = idCompra;
                    cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = idUsuario;
                    cmd.Parameters.Add("@id_sucursal", SqlDbType.Int).Value = idSucursal;
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        private static void AgregarParametrosCompra(
            SqlCommand cmd, CompraSolicitudModel compra, bool incluirId)
        {
            if (incluirId)
                cmd.Parameters.Add("@id_compra", SqlDbType.Int).Value = compra.IdCompra;
            cmd.Parameters.Add("@id_proveedor", SqlDbType.Int).Value = compra.IdProveedor;
            cmd.Parameters.Add("@id_forma_pago", SqlDbType.Int).Value = compra.IdFormaPago;
            if (!incluirId)
                cmd.Parameters.Add("@id_estado", SqlDbType.Int).Value = 7;
            cmd.Parameters.Add("@id_usuario", SqlDbType.Int).Value = compra.IdUsuario;
            cmd.Parameters.Add("@fecha_entrega", SqlDbType.DateTime).Value = compra.FechaEntrega;
        }

        private static void GuardarDetalles(
        SqlConnection conexion,
        SqlTransaction transaccion,
        int idCompra,
        int idProveedor,
        CompraSolicitudModel compra)
            {
                foreach (CompraDetalleModel detalle in compra.Detalles)
                {
                    using (SqlCommand cmd = new SqlCommand("PA_INSERTAR_DETALLE_COMPRA", conexion, transaccion))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@id_compra", SqlDbType.Int).Value = idCompra;
                        cmd.Parameters.Add("@id_producto", SqlDbType.Int).Value = detalle.IdProducto;
                        cmd.Parameters.Add("@cantidad", SqlDbType.Int).Value = detalle.Cantidad;
                        cmd.ExecuteNonQuery();
                    }

                    AsegurarProductoProveedor(
                        conexion, transaccion, detalle.IdProducto, idProveedor, detalle.PrecioCosto);
                }
        }

        private static void AsegurarProductoProveedor(
    SqlConnection conexion, SqlTransaction transaccion, int idProducto, int idProveedor, decimal precioCosto)
        {
            using (SqlCommand verificar = new SqlCommand(
                @"SELECT COUNT(*) FROM Vista_Producto_Proveedor
          WHERE id_producto = @id_producto AND id_proveedor = @id_proveedor",
                conexion, transaccion))
            {
                verificar.Parameters.Add("@id_producto", SqlDbType.Int).Value = idProducto;
                verificar.Parameters.Add("@id_proveedor", SqlDbType.Int).Value = idProveedor;
                if (Convert.ToInt32(verificar.ExecuteScalar()) > 0) return;
            }

            using (SqlCommand insertar = new SqlCommand(
                "PA_INSERTAR_PRODUCTO_PROVEEDOR", conexion, transaccion))
            {
                insertar.CommandType = CommandType.StoredProcedure;
                insertar.Parameters.Add("@id_producto", SqlDbType.Int).Value = idProducto;
                insertar.Parameters.Add("@id_proveedor", SqlDbType.Int).Value = idProveedor;
                insertar.Parameters.Add("@precio_costo", SqlDbType.Decimal).Value = precioCosto;
                insertar.Parameters.Add("@cantidad_stock", SqlDbType.Int).Value = 0;
                insertar.ExecuteNonQuery();
            }
        }

        private static DataTable Consultar(string sql, Action<SqlCommand> configurar = null)
        {
            clsConexion conexion = new clsConexion();
            try
            {
                conexion.Abrir();
                using (SqlCommand cmd = new SqlCommand(sql, conexion.SqlC))
                {
                    if (configurar != null) configurar(cmd);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable tabla = new DataTable();
                        da.Fill(tabla);
                        return tabla;
                    }
                }
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        private static DataTable EjecutarProcedimientoTabla(string procedimiento)
        {
            clsConexion conexion = new clsConexion();
            try
            {
                conexion.Abrir();
                using (SqlCommand cmd = new SqlCommand(procedimiento, conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        DataTable tabla = new DataTable();
                        da.Fill(tabla);
                        return tabla;
                    }
                }
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        private static string EjecutarEscalar(string procedimiento, string predeterminado)
        {
            clsConexion conexion = new clsConexion();
            try
            {
                conexion.Abrir();
                using (SqlCommand cmd = new SqlCommand(procedimiento, conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    object valor = cmd.ExecuteScalar();
                    return valor == null || valor == DBNull.Value
                        ? predeterminado
                        : valor.ToString();
                }
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        private static bool EjecutarEstadoCompra(string procedimiento, int idCompra)
        {
            clsConexion conexion = new clsConexion();
            try
            {
                conexion.Abrir();
                using (SqlCommand cmd = new SqlCommand(procedimiento, conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@id_compra", SqlDbType.Int).Value = idCompra;
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally
            {
                conexion.Cerrar();
            }
        }
    }
}
