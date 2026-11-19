/*using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.ClasesAdministrador
{
    /// <summary>
    /// Provee métodos para consultar y filtrar los registros de la bitácora del sistema.
    /// Devuelve resultados en forma de <see cref="DataTable"/> para su uso en grillas o reportes.
    /// </summary>
    internal class clsBitacora
    {
        // ─── OBTENER TODA LA BITÁCORA ─────────────────────────────────
        /// <summary>
        /// Recupera todos los registros de la vista <c>VistaBitacora</c> y los devuelve en un DataTable.
        /// </summary>
        /// <param name="conexion">Instancia de <see cref="clsConexion"/> usada para ejecutar la consulta.</param>
        /// <returns>Un <see cref="DataTable"/> con los registros de la bitácora o <c>null</c> en caso de error.</returns>
        public DataTable obtenerBitacoraTabla(clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT * FROM VistaBitacora";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la bitácora: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        // ─── FILTRAR BITÁCORA POR FECHA Y ROL ────────────────────────
        /// <summary>
        /// Filtra los registros de la bitácora entre dos fechas y opcionalmente por rol.
        /// </summary>
        /// <param name="conexion">Instancia de <see cref="clsConexion"/> para la consulta.</param>
        /// <param name="desde">Fecha inicial del rango (se considera desde las 00:00:00).</param>
        /// <param name="hasta">Fecha final del rango (se considera hasta las 23:59:59).</param>
        /// <param name="rol">Nombre del rol por el que filtrar (opcional).</param>
        /// <returns>Un <see cref="DataTable"/> con los registros filtrados o <c>null</c> en caso de error.</returns>
        public DataTable filtrarBitacora(clsConexion conexion, DateTime desde, DateTime hasta, string rol)
        {
            try
            {
                conexion.Abrir();

                string consulta = @"SELECT * FROM VistaBitacora 
                            WHERE [Fecha de acción] >= @desde 
                              AND [Fecha de acción] <= @hasta";

                if (!string.IsNullOrWhiteSpace(rol))
                    consulta += " AND Rol = @rol";

                SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC);
                cmd.Parameters.AddWithValue("@desde", desde.Date);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1).AddSeconds(-1));

                if (!string.IsNullOrWhiteSpace(rol))
                    cmd.Parameters.AddWithValue("@rol", rol);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al filtrar la bitácora: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Filtra la bitácora por rango de fechas, rol y nombre de usuario (LIKE).
        /// </summary>
        public DataTable buscarBitacoraPorNombre(clsConexion conexion, DateTime desde,
            DateTime hasta, string rol, string nombreUsuario)
        {
            try
            {
                conexion.Abrir();
                string consulta = @"SELECT * FROM VistaBitacora 
                            WHERE [Fecha de acción] >= @desde 
                              AND [Fecha de acción] <= @hasta
                              AND Usuario LIKE @nombre";

                if (!string.IsNullOrWhiteSpace(rol))
                    consulta += " AND Rol = @rol";

                SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC);
                cmd.Parameters.AddWithValue("@desde", desde.Date);
                cmd.Parameters.AddWithValue("@hasta", hasta.Date.AddDays(1).AddSeconds(-1));
                cmd.Parameters.AddWithValue("@nombre", "%" + nombreUsuario.Trim() + "%");

                if (!string.IsNullOrWhiteSpace(rol))
                    cmd.Parameters.AddWithValue("@rol", rol);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar en la bitácora: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }
    }
}
*/