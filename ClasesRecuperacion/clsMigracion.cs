using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace TRAMADE
{
    internal class clsMigracion
    {
        clsConexion cn = new clsConexion();

        public void MigrarPasswordsAHash()
        {
            cn.Abrir();
            List<(int id, string passActual)> usuarios = new List<(int, string)>();

            try
            {
                string sql = "SELECT id_usuario, password_usuario FROM USUARIO";
                using (SqlCommand cmd = new SqlCommand(sql, cn.SqlC))
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        usuarios.Add((Convert.ToInt32(dr["id_usuario"]), dr["password_usuario"].ToString()));
                    }
                }
            }
            finally { cn.Cerrar(); }

            int migrados = 0;
            foreach (var u in usuarios)
            {
                if (clsSeguridad.EsHashBCrypt(u.passActual)) continue; // ya migrado, lo saltamos

                string hash = clsSeguridad.HashPassword(u.passActual);

                cn.Abrir();
                try
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "UPDATE USUARIO SET password_usuario = @hash WHERE id_usuario = @id", cn.SqlC))
                    {
                        cmd.Parameters.AddWithValue("@hash", hash);
                        cmd.Parameters.AddWithValue("@id", u.id);
                        cmd.ExecuteNonQuery();
                    }
                    migrados++;
                }
                finally { cn.Cerrar(); }
            }

            MessageBox.Show($"Migración completada. {migrados} contraseñas actualizadas a hash.",
                "Migración", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}