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
    internal class clsLlenarComboFormaPago
    {
        /// <summary>
        /// Constructor privado para evitar instanciación estática de la clase auxiliar.
        /// </summary>
        clsLlenarComboFormaPago() { }

        /// <summary>
        /// Llena un <see cref="Krypton.Toolkit.KryptonComboBox"/> con las formas de pago
        /// disponibles obtenidas desde la vista <c>VistaFormaPago</c> en la base de datos.
        /// Establece <c>DisplayMember</c>, <c>ValueMember</c> y deja la selección en -1.
        /// </summary>
        /// <param name="cmb">Combo a llenar con las formas de pago.</param>
        /// <param name="conexion">Instancia de <see cref="clsConexion"/> para ejecutar la consulta.</param>
        public static void llenarComboFormaPago(Krypton.Toolkit.KryptonComboBox cmb, clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT * FROM VistaFormaPago";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmb.DataSource = dt;
                cmb.DisplayMember = "descripcion_forma_pago";
                cmb.ValueMember = "id_forma_pago";

                cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar ComboBox: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }
    }
}

*/
