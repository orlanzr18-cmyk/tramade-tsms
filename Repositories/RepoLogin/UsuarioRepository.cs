using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using TRAMADE.Models;

namespace TRAMADE.Repositories
{
    public interface IUsuarioRepository
    {
        Task<clsUsuarioLoginModel> ObtenerPorCorreoAsync(string correo);
        Task ActualizarIntentosFallidosAsync(int idUsuario, int nuevosIntentos);
        Task ActualizarBloqueoAsync(int idUsuario, int nuevoNivel, DateTime bloqueadoHasta);
        Task DesactivarCuentaAsync(int idUsuario, int nuevoNivel);
        Task ResetearBloqueoAsync(int idUsuario);
    }

    /// <summary>
    /// Reemplaza el SQL que estaba embebido directamente en frmLogin.login()
    /// y en RegistrarIntentoFallido()/ResetearBloqueo(). Mismas consultas,
    /// misma vista (VistaUsuariosLoginCompleta), mismas columnas.
    /// </summary>
    public class UsuarioRepository : IUsuarioRepository
    {
        public async Task<clsUsuarioLoginModel> ObtenerPorCorreoAsync(string correo)
        {
            var conexion = new clsConexion();
            await conexion.AbrirAsync();
            try
            {
                const string consulta = @"SELECT id_usuario, nombre_usuario, id_rol, id_sucursal, nombre_sucursal,
                            correo_usuario, password_usuario, intentos_fallidos, nivel_bloqueo,
                            bloqueado_hasta, id_estado
                     FROM VistaUsuariosLoginCompleta WHERE correo_usuario = @usuario";

                var cmd = new SqlCommand(consulta, conexion.SqlC);
                cmd.Parameters.AddWithValue("@usuario", correo);

                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    if (!await dr.ReadAsync()) return null;

                    object bloqueadoHastaObj = dr["bloqueado_hasta"];

                    return new clsUsuarioLoginModel
                    {
                        IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                        NombreUsuario = dr["nombre_usuario"].ToString(),
                        IdRol = Convert.ToInt32(dr["id_rol"]),
                        IdSucursal = Convert.ToInt32(dr["id_sucursal"]),
                        NombreSucursal = dr["nombre_sucursal"].ToString(),
                        CorreoUsuario = dr["correo_usuario"].ToString(),
                        PasswordHash = dr["password_usuario"].ToString(),
                        IntentosFallidos = Convert.ToInt32(dr["intentos_fallidos"]),
                        NivelBloqueo = Convert.ToInt32(dr["nivel_bloqueo"]),
                        BloqueadoHasta = bloqueadoHastaObj == DBNull.Value ? null : (DateTime?)bloqueadoHastaObj,
                        IdEstado = Convert.ToInt32(dr["id_estado"])
                    };
                }
            }
            finally { conexion.Cerrar(); }
        }

        public async Task ActualizarIntentosFallidosAsync(int idUsuario, int nuevosIntentos)
        {
            var conexion = new clsConexion();
            await conexion.AbrirAsync();
            try
            {
                using (var cmd = new SqlCommand(
                    "UPDATE USUARIO SET intentos_fallidos = @intentos WHERE id_usuario = @id", conexion.SqlC))
                {
                    cmd.Parameters.AddWithValue("@intentos", nuevosIntentos);
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally { conexion.Cerrar(); }
        }

        public async Task ActualizarBloqueoAsync(int idUsuario, int nuevoNivel, DateTime bloqueadoHasta)
        {
            var conexion = new clsConexion();
            await conexion.AbrirAsync();
            try
            {
                using (var cmd = new SqlCommand(
                    @"UPDATE USUARIO SET intentos_fallidos = 0, nivel_bloqueo = @nivel,
                      bloqueado_hasta = @hasta WHERE id_usuario = @id", conexion.SqlC))
                {
                    cmd.Parameters.AddWithValue("@nivel", nuevoNivel);
                    cmd.Parameters.AddWithValue("@hasta", bloqueadoHasta);
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally { conexion.Cerrar(); }
        }

        public async Task DesactivarCuentaAsync(int idUsuario, int nuevoNivel)
        {
            var conexion = new clsConexion();
            await conexion.AbrirAsync();
            try
            {
                using (var cmd = new SqlCommand(
                    @"UPDATE USUARIO SET intentos_fallidos = 0, nivel_bloqueo = @nivel,
                      bloqueado_hasta = NULL, id_estado = 2 WHERE id_usuario = @id", conexion.SqlC))
                {
                    cmd.Parameters.AddWithValue("@nivel", nuevoNivel);
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally { conexion.Cerrar(); }
        }

        public async Task ResetearBloqueoAsync(int idUsuario)
        {
            var conexion = new clsConexion();
            await conexion.AbrirAsync();
            try
            {
                using (var cmd = new SqlCommand(
                    @"UPDATE USUARIO SET intentos_fallidos = 0, nivel_bloqueo = 0, bloqueado_hasta = NULL
                      WHERE id_usuario = @id", conexion.SqlC))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            finally { conexion.Cerrar(); }
        }
    }
}
