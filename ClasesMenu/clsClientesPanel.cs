using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace TRAMADE.Formularios_Login__Menú
{
    /// <summary>
    /// Clase encargada de gestionar la lógica visual del panel de indicadores de clientes.
    /// Calcula estadísticas de clientes activos/inactivos y actualiza una barra de progreso dinámica.
    /// </summary>
    internal class clsClientesPanel
    {
        private readonly Label _lblTotal;
        private readonly Panel _pnlBarraFondo;
        private readonly Panel _pnlBarraActivos;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="clsClientesPanel"/>.
        /// </summary>
        /// <param name="lblTotal">Label donde se mostrará el número total de clientes.</param>
        /// <param name="pnlBarraFondo">Panel que actúa como el fondo de la barra de porcentaje.</param>
        /// <param name="pnlBarraActivos">Panel que actúa como el indicador visual de clientes activos.</param>
        public clsClientesPanel(Label lblTotal, Panel pnlBarraFondo, Panel pnlBarraActivos)
        {
            _lblTotal = lblTotal;
            _pnlBarraFondo = pnlBarraFondo;
            _pnlBarraActivos = pnlBarraActivos;
        }

        /// <summary>
        /// Realiza la consulta a la base de datos para obtener el total de clientes y los activos.
        /// Actualiza el ancho y color de la barra según el porcentaje de clientes activos.
        /// </summary>
        /// <remarks>
        /// El color de la barra cambia según el porcentaje:
        /// <list type="bullet">
        /// <item><description>Verde (70%+): Nivel óptimo.</description></item>
        /// <item><description>Naranja (40%-69%): Nivel preventivo.</description></item>
        /// <item><description>Rojo (Menos de 40%): Nivel crítico.</description></item>
        /// </list>
        /// </remarks>
        public void Cargar()
        {
            clsConexion cn = new clsConexion();
            try
            {
                cn.Abrir();
                int total = ToInt(EjecutarScalar(cn, "SELECT COUNT(*) FROM CLIENTE"));
                int activos = ToInt(EjecutarScalar(cn,
                @"SELECT COUNT(*) FROM CLIENTE c
                  INNER JOIN ESTADO e ON c.id_estado = e.id_estado
                  WHERE UPPER(e.descripcion_estado) = 'ACTIVO'"));
                int inactivos = total - activos;
                int porcentaje = total > 0 ? (int)((activos * 100.0) / total) : 0;


                _lblTotal.Text = total.ToString("N0");

                _pnlBarraFondo.BackColor = Color.FromArgb(220, 220, 220);
                _pnlBarraActivos.Width = (int)(_pnlBarraFondo.Width * porcentaje / 100.0);
                _pnlBarraActivos.Height = _pnlBarraFondo.Height;
                _pnlBarraActivos.Location = new Point(0, 0);
                _pnlBarraActivos.BackColor = porcentaje >= 70 ? Color.FromArgb(72, 199, 174) :
                                             porcentaje >= 40 ? Color.FromArgb(255, 193, 68) :
                                                                Color.FromArgb(220, 53, 69);

                var tooltip = new ToolTip();
                tooltip.SetToolTip(_pnlBarraFondo, $"Activos: {activos} | Inactivos: {inactivos} | {porcentaje}%");
                tooltip.SetToolTip(_pnlBarraActivos, $"Activos: {activos} | Inactivos: {inactivos} | {porcentaje}%");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error clientes: " + ex.Message, "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { cn.Cerrar(); }
        }

        /// <summary>
        /// Ejecuta una consulta SQL que devuelve un valor único (Scalar).
        /// </summary>
        /// <param name="cn">Instancia de la conexión actual.</param>
        /// <param name="query">Consulta SQL a ejecutar.</param>
        /// <returns>Cadena con el resultado; devuelve "0" si el resultado es nulo.</returns>
        private string EjecutarScalar(clsConexion cn, string query)
        {
            SqlCommand cmd = new SqlCommand(query, cn.SqlC);
            object res = cmd.ExecuteScalar();
            return res == null || res == DBNull.Value ? "0" : res.ToString();
        }

        /// <summary>
        /// Intenta convertir de forma segura una cadena a un entero.
        /// </summary>
        /// <param name="v">Cadena a convertir.</param>
        /// <returns>El valor entero o 0 si la conversión falla.</returns>
        private int ToInt(string v) { return int.TryParse(v, out int r) ? r : 0; }
    }
}
