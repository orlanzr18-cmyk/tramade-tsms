using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TRAMADE.Models;

namespace TRAMADE.Repositories
{
    public class clsProductoRepository
    {
        private readonly clsConexion conexion = new clsConexion();

        public List<clsProductoModel> ObtenerProductos(int idSucursal, string filtro = "")
        {
            List<clsProductoModel> lista = new List<clsProductoModel>();
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_PRODUCTOS_POR_SUCURSAL", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);
                    cmd.Parameters.AddWithValue("@filtro", string.IsNullOrEmpty(filtro) ? (object)DBNull.Value : filtro);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new clsProductoModel
                            {
                                Id = Convert.ToInt32(dr["id_producto"]),
                                Nombre = dr["nombre_producto"].ToString(),
                                PrecioUnitario = Convert.ToDecimal(dr["precio_unitario"]),
                                Stock = Convert.ToInt32(dr["cantidad_stock"]),
                                CodigoBarras = dr["codigo_barras"]?.ToString() ?? ""
                            });
                        }
                    }
                }
            }
            finally { conexion.Cerrar(); }
            return lista;
        }

        public clsProductoModel ObtenerPorCodigoBarras(int idSucursal, string codigo)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_PRODUCTO_POR_CODIGO_BARRAS", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new clsProductoModel
                            {
                                Id = Convert.ToInt32(dr["id_producto"]),
                                Nombre = dr["nombre_producto"].ToString(),
                                PrecioUnitario = Convert.ToDecimal(dr["precio_unitario"]),
                                Stock = Convert.ToInt32(dr["cantidad_stock"]),
                                CodigoBarras = dr["codigo_barras"].ToString()
                            };
                        }
                    }
                }
            }
            finally { conexion.Cerrar(); }
            return null;
        }
    }
}