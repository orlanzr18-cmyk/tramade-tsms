/*using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.ClasesCompras
{
    internal class clsIndicadoresCompras
    {
        /// <summary>
        /// Actualiza la etiqueta proporcionada con el total de solicitudes de compra registradas.
        /// Realiza una consulta a la vista <c>VistaComprasTabla</c> para contar compras distintas.
        /// </summary>
        /// <param name="conexion">Instancia de <see cref="clsConexion"/> usada para ejecutar la consulta.</param>
        /// <param name="lbl">Etiqueta (<see cref="System.Windows.Forms.Label"/>) que recibirá el valor total.</param>
        public static void totalSolicitudesRegistradas(clsConexion conexion, System.Windows.Forms.Label lbl)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd = new SqlCommand("PA_TOTAL_SOLICITUDES", conexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;
                object resultado = cmd.ExecuteScalar();
                lbl.Text = resultado != DBNull.Value && resultado != null
                    ? resultado.ToString()
                    : "0";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en indicador solicitudes: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }
        /// <summary>
        /// Calcula y muestra en la etiqueta el valor pendiente total de las compras,
        /// incluyendo impuesto (se multiplica por 1.15). El valor se formatea con dos decimales
        /// y se antepone la letra "L".
        /// </summary>
        /// <param name="conexion">Instancia de <see cref="clsConexion"/> usada para la consulta.</param>
        /// <param name="lbl">Etiqueta (<see cref="System.Windows.Forms.Label"/>) donde se mostrará el monto.</param>
        public static void totalValorPendiente(clsConexion conexion, System.Windows.Forms.Label lbl)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd = new SqlCommand("PA_VALOR_PENDIENTE", conexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;
                object resultado = cmd.ExecuteScalar();
                lbl.Text = resultado != DBNull.Value && resultado != null
                    ? "L " + resultado.ToString()
                    : "L 0.00";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en indicador valor: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Calcula el tiempo promedio de espera (en días) entre la fecha de pedido y la fecha de entrega
        /// y actualiza la etiqueta con el resultado seguido de "dias".
        /// </summary>
        /// <param name="conexion">Instancia de <see cref="clsConexion"/> usada para ejecutar la consulta.</param>
        /// <param name="lbl">Etiqueta (<see cref="System.Windows.Forms.Label"/>) donde se mostrará el promedio en días.</param>
        public static void tiempoEsperaPromedio(clsConexion conexion, System.Windows.Forms.Label lbl)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd = new SqlCommand("PA_TIEMPO_ESPERA_PROMEDIO", conexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;
                object resultado = cmd.ExecuteScalar();
                lbl.Text = resultado != DBNull.Value && resultado != null
                    ? resultado.ToString() + " dias"
                    : "0 dias";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en indicador tiempo: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }
    }
}

*/
