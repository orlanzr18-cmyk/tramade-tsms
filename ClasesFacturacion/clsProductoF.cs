/*using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TRAMADE
{
    /// <summary>
    /// Clase que gestiona la lógica de negocios y acceso a datos para los productos.
    /// Permite consultas filtradas y búsquedas específicas por código de barras.
    /// </summary>
    public class clsProductoF
    {
        /// <summary>
        /// Identificador único del producto en la base de datos.
        /// </summary>
        public int id_producto { get; set; }

        /// <summary>
        /// Nombre descriptivo del producto.
        /// </summary>
        public string nombre_producto { get; set; }

        /// <summary>
        /// Precio de venta por unidad.
        /// </summary>
        public decimal precio_unitario { get; set; }

        /// <summary>
        /// Cantidad total disponible sumando todas las sucursales.
        /// </summary>
        public int existencia_producto { get; set; }

        /// <summary>
        /// Código de barras único asociado al producto para identificación rápida.
        /// </summary>
        public string codigo_barras { get; set; }

        // ─── Obtener productos con filtro por nombre O código de barras ───────
        /// <summary>
        /// Obtiene una lista de productos de la base de datos. Permite filtrar por nombre o código de barras.
        /// Realiza una suma (SUM) de las existencias en todas las sucursales vinculadas.
        /// </summary>
        /// <param name="filtro">Cadena de texto para buscar coincidencias parciales en nombre o código.</param>
        /// <returns>Una lista de objetos <see cref="clsProductoF"/>.</returns>
        /// <exception cref="Exception">Lanza una excepción si ocurre un error en la conexión o la consulta SQL.</exception>
        public List<clsProductoF> ObtenerProductos(string filtro = "")
        {
            List<clsProductoF> lista = new List<clsProductoF>();
            clsConexion conexion = new clsConexion();
            conexion.Abrir();

            try
            {
                // GROUP BY para evitar duplicados cuando el producto
                // está en varias sucursales — el stock se suma entre todas
                string query = @"
                    SELECT   P.id_producto,
                             P.nombre_producto,
                             P.precio_unitario,
                             P.codigo_barras,
                             SUM(PS.cantidad_stock) AS cantidad_stock
                    FROM     PRODUCTO P
                    INNER    JOIN PRODUCTO_SUCURSAL PS
                             ON P.id_producto = PS.id_producto
                    WHERE    PS.id_sucursal = @idSucursal";

                // El filtro busca por nombre O por código de barras
                if (!string.IsNullOrEmpty(filtro))
                    query += @" AND (P.nombre_producto LIKE @filtro
                                 OR  P.codigo_barras   LIKE @filtro)";

                query += @" GROUP BY P.id_producto, P.nombre_producto,
                                     P.precio_unitario, P.codigo_barras";

                SqlCommand cmd = new SqlCommand(query, conexion.SqlC);
                cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);
                if (!string.IsNullOrEmpty(filtro))
                    cmd.Parameters.AddWithValue("@filtro", "%" + filtro + "%");

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var prod = new clsProductoF
                    {
                        id_producto = Convert.ToInt32(dr["id_producto"]),
                        nombre_producto = dr["nombre_producto"].ToString(),
                        precio_unitario = Convert.ToDecimal(dr["precio_unitario"]),
                        existencia_producto = Convert.ToInt32(dr["cantidad_stock"]),
                        codigo_barras = dr["codigo_barras"] == DBNull.Value
                                             ? "" : dr["codigo_barras"].ToString()
                    };
                    lista.Add(prod);
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar productos: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
            return lista;
        }

        // ─── Buscar un producto EXACTO por código de barras (para el escáner) ─
        /// <summary>
        /// Busca y devuelve un producto específico mediante una coincidencia exacta de su código de barras.
        /// Ideal para procesos de venta con escáner de mano.
        /// </summary>
        /// <param name="codigo">El código de barras exacto a buscar.</param>
        /// <returns>Un objeto <see cref="clsProductoF"/> si se encuentra; de lo contrario, <c>null</c>.</returns>
        public clsProductoF ObtenerPorCodigoBarras(string codigo)
        {
            clsConexion conexion = new clsConexion();
            conexion.Abrir();
            try
            {
                const string query = @"
                    SELECT P.id_producto,
                           P.nombre_producto,
                           P.precio_unitario,
                           P.codigo_barras,
                           PS.cantidad_stock
                    FROM   PRODUCTO P
                    INNER  JOIN PRODUCTO_SUCURSAL PS
                           ON P.id_producto = PS.id_producto
                    WHERE  P.codigo_barras = @codigo
                    AND    PS.id_sucursal = @idSucursal";

                SqlCommand cmd = new SqlCommand(query, conexion.SqlC);
                cmd.Parameters.AddWithValue("@codigo", codigo);
                cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);

                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    return new clsProductoF
                    {
                        id_producto = Convert.ToInt32(dr["id_producto"]),
                        nombre_producto = dr["nombre_producto"].ToString(),
                        precio_unitario = Convert.ToDecimal(dr["precio_unitario"]),
                        existencia_producto = Convert.ToInt32(dr["cantidad_stock"]),
                        codigo_barras = dr["codigo_barras"].ToString()
                    };
                }
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }
    }
}
*/