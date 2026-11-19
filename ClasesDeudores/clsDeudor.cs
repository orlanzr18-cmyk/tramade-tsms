using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.ClasesDeudores
{
    internal class clsDeudor
    {

        private int idCliente, idUsuario;
        private decimal montoPago;

        public void setIdCliente(int valor) { idCliente = valor; }
        public void setIdUsuario(int valor) { idUsuario = valor; }
        public void setMontoPago(decimal valor) { montoPago = valor; }

        // ─── OBTENER DEUDORES ─────────────────────────────────────────
        public DataTable obtenerDeudores(clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT * FROM VistaDeudores";

                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar deudores: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        // ─── REGISTRAR PAGO ─────────────────────────────────────────
        public bool registrarPago(clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd = new SqlCommand("PA_REGISTRAR_PAGO_DEUDOR", conexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                cmd.Parameters.AddWithValue("@monto_pago", montoPago);
                cmd.ExecuteNonQuery();
                /*int filas = cmd.ExecuteNonQuery();
                 return filas > 0;*/
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al registrar pago: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        // ─── BUSCAR DEUDORES ─────────────────────────────────────────

        public DataTable buscarDeudores(clsConexion conexion, string criterio)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd = new SqlCommand(@"
                SELECT * FROM VistaDeudores
                WHERE [Cliente] COLLATE SQL_Latin1_General_CP1_CI_AI LIKE @criterio
                OR [DNI]     COLLATE SQL_Latin1_General_CP1_CI_AI LIKE @criterio
                OR [RTN]     COLLATE SQL_Latin1_General_CP1_CI_AI LIKE @criterio",
                    conexion.SqlC);
                cmd.Parameters.AddWithValue("@criterio", "%" + criterio.Trim() + "%");
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar: " + ex.Message, "Error",
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
