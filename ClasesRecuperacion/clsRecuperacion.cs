using System;
using System.Data;
using System.Data.SqlClient;

namespace TRAMADE
{
    internal class clsRecuperacion
    {
        clsConexion cn = new clsConexion();

        public string GenerarCodigo()
        {
            Random r = new Random();
            return r.Next(0, 999999).ToString("D6");
        }

        // Busca el id_usuario a partir del correo. Devuelve null si no existe.
        public int? ObtenerIdUsuarioPorCorreo(string correo)
        {
            cn.Abrir();
            try
            {
                string sql = "SELECT id_usuario FROM USUARIO WHERE correo_usuario = @correo";
                using (SqlCommand cmd = new SqlCommand(sql, cn.SqlC))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    object res = cmd.ExecuteScalar();
                    return res != null ? Convert.ToInt32(res) : (int?)null;
                }
            }
            finally { cn.Cerrar(); }
        }

        // Crea una nueva solicitud de recuperación con su código y expiración
        public void CrearSolicitud(int idUsuario, string codigo, int minutosExpiracion = 1)
        {
            cn.Abrir();
            try
            {
                string sql = @"INSERT INTO RECUPERACION 
                               (id_usuario, codigo_recuperacion, fecha_envio_recuperacion, 
                                fecha_expiracion_recuperacion, intentos_recuperacion, exitosa_recuperacion)
                               VALUES (@id, @codigo, @envio, @expira, 0, 0)";
                using (SqlCommand cmd = new SqlCommand(sql, cn.SqlC))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    cmd.Parameters.AddWithValue("@codigo", codigo);
                    cmd.Parameters.AddWithValue("@envio", DateTime.Now);
                    cmd.Parameters.AddWithValue("@expira", DateTime.Now.AddMinutes(minutosExpiracion));
                    cmd.ExecuteNonQuery();
                }
            }
            finally { cn.Cerrar(); }
        }

        // Resultados posibles: 0 = correcto, 1 = código incorrecto, 2 = expirado,
        // 3 = demasiados intentos, 4 = no hay solicitud activa
        public int ValidarCodigo(int idUsuario, string codigoIngresado, out int idRecuperacion)
        {
            idRecuperacion = 0;
            cn.Abrir();
            try
            {
                string sql = @"SELECT TOP 1 id_recuperacion, codigo_recuperacion, 
                               fecha_expiracion_recuperacion, intentos_recuperacion
                               FROM RECUPERACION
                               WHERE id_usuario = @id AND exitosa_recuperacion = 0
                               ORDER BY id_recuperacion DESC";

                string codigoReal;
                DateTime expira;
                int intentos;

                using (SqlCommand cmd = new SqlCommand(sql, cn.SqlC))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (!dr.Read()) return 4;

                        idRecuperacion = (int)dr["id_recuperacion"];
                        codigoReal = dr["codigo_recuperacion"].ToString();
                        expira = (DateTime)dr["fecha_expiracion_recuperacion"];
                        intentos = (int)dr["intentos_recuperacion"];
                    }
                }

                if (intentos >= 3) return 3;
                if (DateTime.Now > expira) return 2;

                if (codigoReal != codigoIngresado)
                {
                    using (SqlCommand cmdU = new SqlCommand(
                        "UPDATE RECUPERACION SET intentos_recuperacion = intentos_recuperacion + 1 WHERE id_recuperacion = @idr",
                        cn.SqlC))
                    {
                        cmdU.Parameters.AddWithValue("@idr", idRecuperacion);
                        cmdU.ExecuteNonQuery();
                    }
                    return 1;
                }

                return 0; 
            }
            finally { cn.Cerrar(); }
        }

        // Marca la solicitud como exitosa y cambia la contraseña del usuario
        public void CambiarPassword(int idUsuario, int idRecuperacion, string nuevaPassword)
        {
            string hash = clsSeguridad.HashPassword(nuevaPassword); 

            cn.Abrir();
            try
            {
                using (SqlCommand cmd1 = new SqlCommand(
                    "UPDATE USUARIO SET password_usuario = @pass WHERE id_usuario = @id", cn.SqlC))
                {
                    cmd1.Parameters.AddWithValue("@pass", hash); 
                    cmd1.Parameters.AddWithValue("@id", idUsuario);
                    cmd1.ExecuteNonQuery();
                }

                using (SqlCommand cmd2 = new SqlCommand(
                    "UPDATE RECUPERACION SET exitosa_recuperacion = 1 WHERE id_recuperacion = @idr", cn.SqlC))
                {
                    cmd2.Parameters.AddWithValue("@idr", idRecuperacion);
                    cmd2.ExecuteNonQuery();
                }
            }
            finally { cn.Cerrar(); }
        }
    }
}