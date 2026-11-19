using System;
using System.Data.SqlClient;

namespace TRAMADE.Formularios_Login__Menú
{
    public class clsStatCards
    {
        public int TotalProductos { get; private set; }
        public int Agotados { get; private set; }
        public decimal IngresosMes { get; private set; }
        public int DeudoresPendientes { get; private set; }
        public int TotalClientes { get; private set; }
        public int ClientesActivos { get; private set; }

        public void Cargar()
        {
            var cn = new clsConexion();
            try
            {
                cn.Abrir();

                TotalProductos = Scalar(cn, "SELECT COUNT(*) FROM PRODUCTO");
                Agotados = Scalar(cn, @"
                SELECT COUNT(*) FROM PRODUCTO P
                WHERE NOT EXISTS (
                    SELECT 1 FROM PRODUCTO_PROVEEDOR PP
                    WHERE PP.id_producto = P.id_producto AND PP.cantidad_stock > 0
                )");
                IngresosMes = ScalarDec(cn,
                    @"SELECT ISNULL(SUM(fp.cantidad * p.precio_unitario), 0)
                      FROM FACTURA f
                      INNER JOIN FACTURA_PRODUCTO fp ON f.id_factura = fp.id_factura
                      INNER JOIN PRODUCTO p ON fp.id_producto = p.id_producto
                      WHERE MONTH(f.fecha_emision) = MONTH(GETDATE())
                        AND YEAR(f.fecha_emision)  = YEAR(GETDATE())");
                DeudoresPendientes = Scalar(cn, "SELECT COUNT(*) FROM VistaDeudores");
                TotalClientes = Scalar(cn, "SELECT COUNT(*) FROM CLIENTE");
                ClientesActivos = Scalar(cn,
                    @"SELECT COUNT(*) FROM CLIENTE c
                      INNER JOIN ESTADO e ON c.id_estado = e.id_estado
                      WHERE UPPER(e.descripcion_estado) = 'ACTIVO'");
            }
            finally { cn.Cerrar(); }
        }

        private int Scalar(clsConexion cn, string sql)
        {
            var r = new SqlCommand(sql, cn.SqlC).ExecuteScalar();
            return (r == null || r == DBNull.Value) ? 0 : Convert.ToInt32(r);
        }

        private decimal ScalarDec(clsConexion cn, string sql)
        {
            var r = new SqlCommand(sql, cn.SqlC).ExecuteScalar();
            return (r == null || r == DBNull.Value) ? 0m : Convert.ToDecimal(r);
        }
    }
}