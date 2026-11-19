using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TRAMADE.Models;

namespace TRAMADE.Repositories
{
    public class ClienteRepository
    {
        private readonly clsConexion conexion = new clsConexion();

        public bool Insertar(clsClienteModel cliente)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_INSERTAR_CLIENTE", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre_cliente", cliente.Nombre);
                    cmd.Parameters.AddWithValue("@rtn_cliente", string.IsNullOrWhiteSpace(cliente.Rtn) ? (object)DBNull.Value : cliente.Rtn);
                    cmd.Parameters.AddWithValue("@razon_social", string.IsNullOrWhiteSpace(cliente.RazonSocial) ? (object)DBNull.Value : cliente.RazonSocial);
                    cmd.Parameters.AddWithValue("@dni_cliente", string.IsNullOrWhiteSpace(cliente.Dni) ? (object)DBNull.Value : cliente.Dni);
                    cmd.Parameters.AddWithValue("@telefono_cliente", cliente.Telefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@contacto_cliente", cliente.Contacto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@correo_electronico_cliente", cliente.Correo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@direccion_cliente", cliente.Direccion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@id_estado", cliente.Estado);
                    cmd.Parameters.AddWithValue("@id_clasificacion_cliente", Convert.ToInt32(cliente.TipoCliente));
                    cmd.Parameters.AddWithValue("@id_ciudad", cliente.IdCiudad);
                    cmd.Parameters.AddWithValue("@id_usuario", cliente.IdUsuario);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally { conexion.Cerrar(); }
        }

        public bool Actualizar(clsClienteModel cliente)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_ACTUALIZAR_CLIENTE", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_cliente", cliente.Id);
                    cmd.Parameters.AddWithValue("@nombre_cliente", cliente.Nombre);
                    cmd.Parameters.AddWithValue("@rtn_cliente", string.IsNullOrWhiteSpace(cliente.Rtn) ? (object)DBNull.Value : cliente.Rtn);
                    cmd.Parameters.AddWithValue("@razon_social", string.IsNullOrWhiteSpace(cliente.RazonSocial) ? (object)DBNull.Value : cliente.RazonSocial);
                    cmd.Parameters.AddWithValue("@dni_cliente", string.IsNullOrWhiteSpace(cliente.Dni) ? (object)DBNull.Value : cliente.Dni);
                    cmd.Parameters.AddWithValue("@telefono_cliente", cliente.Telefono ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@contacto_cliente", cliente.Contacto ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@correo_electronico_cliente", cliente.Correo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@direccion_cliente", cliente.Direccion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@id_clasificacion_cliente", Convert.ToInt32(cliente.TipoCliente));
                    cmd.Parameters.AddWithValue("@id_ciudad", cliente.IdCiudad);
                    cmd.Parameters.AddWithValue("@id_usuario", cliente.IdUsuario);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally { conexion.Cerrar(); }
        }

        public clsClienteModel BuscarPorDocumento(string documento)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_BUSCAR_CLIENTE_POR_DOCUMENTO", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@doc", documento);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new clsClienteModel
                            {
                                Id = Convert.ToInt32(reader["id_cliente"]),
                                Nombre = reader["nombre_cliente"].ToString(),
                                Direccion = reader["direccion_cliente"].ToString(),
                                Rtn = reader["rtn_cliente"]?.ToString(),
                                Dni = reader["dni_cliente"]?.ToString(),
                                IdCiudad = Convert.ToInt32(reader["id_ciudad"]),
                                IdDepartamento = Convert.ToInt32(reader["id_departamento"])
                            };
                        }
                    }
                }
            }
            finally { conexion.Cerrar(); }
            return null;
        }

        // Consultas para Combos y Catálogos
        public DataTable ObtenerDepartamentos()
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_DEPARTAMENTOS", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public DataTable ObtenerCiudadesPorDepartamento(int idDepartamento)
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_CIUDADES_POR_DEPARTAMENTO", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_departamento", idDepartamento);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public List<string> ObtenerSugerenciasAutoComplete()
        {
            List<string> sugerencias = new List<string>();
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_SUGERENCIAS_AUTOCOMPLETE", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string rtn = dr["rtn_cliente"]?.ToString() ?? "";
                            string dni = dr["dni_cliente"]?.ToString() ?? "";
                            string nombre = dr["nombre_cliente"]?.ToString() ?? "";

                            string documentoFinal = "";

                            // LÓGICA DE SELECCIÓN:
                            // 1. Si el RTN es válido (no empieza con SN y no está vacío), lo usamos.
                            if (!string.IsNullOrWhiteSpace(rtn) && !rtn.StartsWith("SN-"))
                            {
                                documentoFinal = rtn;
                            }
                            // 2. Si no, si el DNI es válido, lo usamos.
                            else if (!string.IsNullOrWhiteSpace(dni) && !dni.StartsWith("SN-"))
                            {
                                documentoFinal = dni;
                            }

                            if (!string.IsNullOrEmpty(documentoFinal))
                            {
                                sugerencias.Add($"{documentoFinal} — {nombre}");
                            }
                        }
                    }
                }
            }
            finally { conexion.Cerrar(); }
            return sugerencias;
        }

        public bool Autorizar(int idCliente)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_AUTORIZAR_CLIENTE", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally { conexion.Cerrar(); }
        }

        public bool Denegar(int idCliente)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_DENEGAR_CLIENTE", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            finally { conexion.Cerrar(); }
        }

        public DataTable ObtenerTiposCliente()
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                string consulta = "SELECT id_clasificacion_cliente, descripcion_clasificacion_cliente FROM CLASIFICACION_CLIENTE";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion.SqlC);
                adapter.Fill(dt);
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public DataTable ObtenerUltimosClientes()
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM Ultimos_Clientes", conexion.SqlC);
                adapter.Fill(dt);
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public DataTable ObtenerVistaAprobacion()
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM vista_aprobacion_clientes", conexion.SqlC);
                adapter.Fill(dt);
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public DataTable BuscarEnVistaAprobacionPorNombre(string textoBuscar)
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                string consulta = "SELECT * FROM vista_aprobacion_clientes WHERE [Nombre Cliente] LIKE @nombre";
                using (SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC))
                {
                    cmd.Parameters.AddWithValue("@nombre", "%" + textoBuscar.Trim() + "%");
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public DataTable ObtenerVistaSeguimiento()
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT * FROM VistaClientes", conexion.SqlC);
                adapter.Fill(dt);
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public DataTable ObtenerPorcentajeEstados()
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter("PA_PorcentajeEstados", conexion.SqlC);
                adapter.Fill(dt);
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public DataTable ObtenerTopDepartamentos()
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter("PA_TopDepartamentos", conexion.SqlC);
                adapter.Fill(dt);
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public dynamic ObtenerEstadisticas()
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_CONTADORES", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new
                            {
                                Total = reader["Total"].ToString(),
                                Activos = reader["Activos"].ToString(),
                                Inactivos = reader["Inactivos"].ToString()
                            };
                        }
                        return null;
                    }
                }
            }
            finally { conexion.Cerrar(); }
        }

        public DataTable ObtenerClienteCompletoPorDocumento(string documento)
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                string consulta = @"SELECT C.*, CI.id_departamento
                                     FROM CLIENTE C
                                     INNER JOIN CIUDAD CI ON C.id_ciudad = CI.id_ciudad
                                     WHERE C.rtn_cliente = @busqueda OR C.dni_cliente = @busqueda";
                using (SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC))
                {
                    cmd.Parameters.AddWithValue("@busqueda", documento);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public DataTable BuscarEnVistaSeguimientoPorNombre(string textoBuscar)
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                string consulta = "SELECT * FROM VistaClientes WHERE [Nombre cliente] LIKE @nombre";
                using (SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC))
                {
                    cmd.Parameters.AddWithValue("@nombre", "%" + textoBuscar.Trim() + "%");
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public bool TieneMora(int idCliente)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_CLIENTE_TIENE_MORA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                    var resultado = cmd.ExecuteScalar();
                    return resultado != null && Convert.ToInt32(resultado) == 1;
                }
            }
            finally { conexion.Cerrar(); }
        }
    }
}