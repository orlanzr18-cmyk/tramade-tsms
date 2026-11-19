using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TRAMADE.Repositories
{
    /// <summary>
    /// Todo lo que "clsCodigoBarras" tocaba de base de datos (antes tenía SQL
    /// embebido directamente). El dibujo del PDF / bitmap del código de barras
    /// NO es DAL ni regla de negocio — sigue viviendo en clsCodigoBarras como
    /// utilidad de impresión, tal como estaba.
    /// </summary>
    public interface ICodigoBarrasRepository
    {
        void GuardarCodigo(SqlConnection con, int idProducto, string codigo);
        List<(string Codigo, string Nombre, decimal Precio)> ObtenerProductosConCodigoBarras();
    }

    public class CodigoBarrasRepository : ICodigoBarrasRepository
    {
        public void GuardarCodigo(SqlConnection con, int idProducto, string codigo)
        {
            using (var cmd = new SqlCommand(
                "UPDATE dbo.PRODUCTO SET codigo_barras = @cod WHERE id_producto = @id", con))
            {
                cmd.Parameters.AddWithValue("@cod", codigo);
                cmd.Parameters.AddWithValue("@id", idProducto);
                cmd.ExecuteNonQuery();
            }
        }

        public List<(string Codigo, string Nombre, decimal Precio)> ObtenerProductosConCodigoBarras()
        {
            var resultado = new List<(string, string, decimal)>();
            var conexion = new TRAMADE.clsConexion();
            conexion.Abrir();
            try
            {
                using (var cmd = new SqlCommand(
                    @"SELECT codigo_barras, nombre_producto, precio_unitario
                      FROM   dbo.PRODUCTO
                      WHERE  codigo_barras IS NOT NULL
                      ORDER  BY nombre_producto", conexion.SqlC))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        resultado.Add((reader.GetString(0), reader.GetString(1), reader.GetDecimal(2)));
                }
            }
            finally { conexion.Cerrar(); }
            return resultado;
        }
    }
}
