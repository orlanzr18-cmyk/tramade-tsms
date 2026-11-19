using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TRAMADE.Models;

namespace TRAMADE.Repositories
{
    public class clsHistorialFacturaRepository
    {
        private readonly clsConexion conexion = new clsConexion();

        public int ContarTotal()
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_CONTAR_FACTURAS", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    return (int)cmd.ExecuteScalar();
                }
            }
            finally { conexion.Cerrar(); }
        }

        public List<clsHistorialFacturaModel> ObtenerHistorial(string filtro, DateTime? fecha, bool ordenAsc)
        {
            List<clsHistorialFacturaModel> lista = new List<clsHistorialFacturaModel>();
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_HISTORIAL_FACTURAS", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@filtro", string.IsNullOrEmpty(filtro) ? (object)DBNull.Value : filtro);
                    cmd.Parameters.AddWithValue("@fecha", fecha.HasValue ? (object)fecha.Value.Date : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ordenAsc", ordenAsc);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new clsHistorialFacturaModel
                            {
                                Id = Convert.ToInt32(dr["id_factura"]),
                                NumeroFactura = Convert.ToInt32(dr["numero_factura"]),
                                IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                                RtnCliente = dr["rtn_cliente"]?.ToString(),
                                DniCliente = dr["dni_cliente"]?.ToString(),
                                FechaEmision = Convert.ToDateTime(dr["fecha_emision"])
                            });
                        }
                    }
                }
            }
            finally { conexion.Cerrar(); }
            return lista;
        }
    }
}