using System;
using System.Data;
using System.Data.SqlClient;
using TRAMADE.Models;

namespace TRAMADE.Repositories
{
    public class ClienteOcasionalRepository
    {
        private readonly clsConexion conexion = new clsConexion();

        public bool Insertar(clsClienteOcasionalModel cliente)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_INSERTAR_CLIENTE_OCASIONAL", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_factura", cliente.IdFactura);
                    cmd.Parameters.AddWithValue("@nombre_cliente", cliente.Nombre);
                    cmd.Parameters.AddWithValue("@dni_cliente", cliente.Dni);
                    cmd.Parameters.AddWithValue("@direccion_cliente", cliente.Direccion);

                    // El SP devuelve @@ROWCOUNT vía SELECT
                    object resultado = cmd.ExecuteScalar();
                    return resultado != null && Convert.ToInt32(resultado) > 0;
                }
            }
            finally { conexion.Cerrar(); }
        }

        public clsClienteOcasionalModel ObtenerPorFactura(int idFactura)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_CLIENTE_OCASIONAL_POR_FACTURA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_factura", idFactura);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new clsClienteOcasionalModel
                            {
                                Nombre = dr["nombre_cliente"].ToString(),
                                Dni = dr["dni_cliente"].ToString(),
                                Direccion = dr["direccion_cliente"].ToString()
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