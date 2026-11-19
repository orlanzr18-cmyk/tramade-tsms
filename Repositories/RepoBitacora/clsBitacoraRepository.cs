using System;
using System.Data;
using System.Data.SqlClient;

namespace TRAMADE.Repositories.RepoBitacora
{
    public class clsBitacoraRepository
    {
        private readonly clsConexion conexion = new clsConexion();

        public DataTable ObtenerRegistros(DateTime? desde = null, DateTime? hasta = null, string rol = null, string nombre = null)
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_REGISTROS_BITACORA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@desde", (object)desde ?? DBNull.Value);

                    // Ajustamos el "hasta" para que cubra todo el día (23:59:59)
                    if (hasta.HasValue)
                        cmd.Parameters.AddWithValue("@hasta", hasta.Value.Date.AddDays(1).AddSeconds(-1));
                    else
                        cmd.Parameters.AddWithValue("@hasta", DBNull.Value);

                    cmd.Parameters.AddWithValue("@rol", string.IsNullOrWhiteSpace(rol) ? (object)DBNull.Value : rol);
                    cmd.Parameters.AddWithValue("@nombre", string.IsNullOrWhiteSpace(nombre) ? (object)DBNull.Value : nombre);

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        public DataTable ObtenerRoles()
        {
            DataTable dt = new DataTable();
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_ROLES_BITACORA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dt);
                }
            }
            finally { conexion.Cerrar(); }
            return dt;
        }
    }
}