using System;
using System.Data;
using System.Data.SqlClient;

namespace TRAMADE.Formularios_Login__Menú
{
    /// <summary>
    /// Clase de acceso a datos para las últimas compras realizadas.
    /// Solo se encarga de obtener los datos — el estilo lo maneja la UI.
    /// </summary>
    public class clsUltimasCompras
    {
        private readonly int _cantidad;

        /// <summary>
        /// Inicializa la clase indicando cuántos registros obtener.
        /// </summary>
        /// <param name="cantidad">Número de compras a recuperar. Por defecto 5.</param>
        public clsUltimasCompras(int cantidad = 5)
        {
            _cantidad = cantidad;
        }

        /// <summary>
        /// Obtiene las últimas compras como DataTable listo para bindear a cualquier control.
        /// Columnas: Proveedor, Monto, Fecha.
        /// </summary>
        public DataTable ObtenerDatos()
        {
            var cn = new clsConexion();
            try
            {
                cn.Abrir();
                string query = $@"
                    SELECT TOP {_cantidad}
                        p.nombre_comercial_proveedor        AS Proveedor,
                        SUM(dc.cantidad * pr.precio_costo)  AS Monto,
                        CONVERT(varchar, c.fecha_pedido, 103) AS Fecha
                    FROM COMPRAS c
                    INNER JOIN PROVEEDOR p       ON c.id_proveedor = p.id_proveedor
                    INNER JOIN DETALLE_COMPRA dc ON c.id_compra    = dc.id_compra
                    INNER JOIN PRODUCTO pr       ON dc.id_producto = pr.id_producto
                    GROUP BY c.id_compra, p.nombre_comercial_proveedor, c.fecha_pedido
                    ORDER BY c.fecha_pedido DESC";

                var da = new SqlDataAdapter(query, cn.SqlC);
                var dt = new DataTable();
                da.Fill(dt);

                // Formatear monto como moneda
                var montoFmt = new DataColumn("MontoFmt", typeof(string));
                dt.Columns.Add(montoFmt);
                foreach (DataRow row in dt.Rows)
                    row["MontoFmt"] = $"L. {Convert.ToDecimal(row["Monto"]):N2}";
                dt.Columns.Remove("Monto");
                montoFmt.ColumnName = "Monto";

                return dt;
            }
            catch { return new DataTable(); }
            finally { cn.Cerrar(); }
        }
    }
}