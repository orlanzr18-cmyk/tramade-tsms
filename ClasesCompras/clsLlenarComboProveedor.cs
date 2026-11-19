/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace TRAMADE.ClasesCompras
{
    /// <summary>
    /// Clase auxiliar para cargar proveedores en un control <c>KryptonComboBox</c>.
    /// Proporciona un método estático que consulta la vista <c>VistaProveedor</c>
    /// y asigna el resultado al combo, soportando filtro por texto y una acción
    /// opcional a ejecutar al finalizar la carga en la UI.
    /// </summary>
    internal class clsLlenarComboProveedor
    {
        /// <summary>
        /// Constructor privado para evitar instanciación de la clase auxiliar.
        /// </summary>
        clsLlenarComboProveedor() { }

        /// <summary>
        /// Llena el combo con proveedores. Si <paramref name="texto"/> no está vacío,
        /// aplica un filtro por nombre comercial usando LIKE '%texto%'. La asignación
        /// al control se realiza mediante <c>BeginInvoke</c> para evitar bloquear la UI
        /// y, al finalizar, se invoca <paramref name="onFinish"/> si se proporcionó.
        /// </summary>
        /// <param name="cmb">Combo (<see cref="Krypton.Toolkit.KryptonComboBox"/>) que recibirá los datos.</param>
        /// <param name="conexion">Conexión a la base de datos (<see cref="clsConexion"/>).</param>
        /// <param name="texto">Texto opcional para filtrar proveedores por nombre comercial.</param>
        /// <param name="onFinish">Acción opcional a ejecutar en el hilo de la UI cuando termine la carga.</param>
        public static void llenarComboProveedor(Krypton.Toolkit.KryptonComboBox cmb, clsConexion conexion, string texto = "", Action onFinish = null)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd;

                if (string.IsNullOrWhiteSpace(texto))
                {
                    cmd = new SqlCommand("SELECT * FROM VistaProveedor", conexion.SqlC);
                }
                else
                {
                    cmd = new SqlCommand("SELECT * FROM VistaProveedor WHERE nombre_comercial_proveedor LIKE @texto", conexion.SqlC);
                    cmd.Parameters.AddWithValue("@texto", "%" + texto.Trim() + "%");
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                string textoGuardado = texto;

                cmb.BeginInvoke(new Action(() =>
                {
                    cmb.DataSource = null;
                    cmb.DisplayMember = "";
                    cmb.ValueMember = "";
                    cmb.Items.Clear();

                    if (dt.Rows.Count > 0)
                    {

                        cmb.DisplayMember = "nombre_comercial_proveedor";
                        cmb.ValueMember = "id_proveedor";
                        cmb.DataSource = dt;
                        cmb.SelectedIndex = -1;
                    }

                    cmb.Text = textoGuardado;
                    cmb.SelectionStart = textoGuardado.Length;
                    cmb.SelectionLength = 0;

                    Timer t = new Timer() { Interval = 50 };
                    t.Tick += (s, ev) =>
                    {
                        t.Stop();
                        if (cmb.IsDisposed || !cmb.IsHandleCreated) return;
                        cmb.SelectedIndex = -1;
                        cmb.Text = textoGuardado;
                        cmb.SelectionStart = textoGuardado.Length;
                        cmb.SelectionLength = 0;
                        onFinish?.Invoke(); // ← avisa que ya terminó
                    };
                    t.Start();
                }));
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
