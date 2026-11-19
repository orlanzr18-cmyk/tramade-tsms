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
    internal class clsLlenarComboProducto
    {
        /// <summary>
        /// Utilidad para llenar de forma asíncrona un combo de productos (<see cref="Krypton.Toolkit.KryptonComboBox"/>)
        /// a partir de la vista <c>VistaProducto</c>. Soporta filtrado por texto y permite ejecutar
        /// una acción opcional al completar la carga (<paramref name="onFinish"/>).
        /// </summary>
        internal clsLlenarComboProducto() { }

        /// <summary>
        /// Llena el combo con productos. Si <paramref name="texto"/> no está vacío, se aplica un filtro
        /// por nombre de producto usando LIKE '%texto%'. La asignación al combo se realiza mediante
        /// <c>BeginInvoke</c> para evitar bloqueos de la UI. Al finalizar la carga se invoca
        /// <paramref name="onFinish"/> si se proporcionó.
        /// </summary>
        /// <param name="cmb">Combo (<see cref="Krypton.Toolkit.KryptonComboBox"/>) que recibirá los productos.</param>
        /// <param name="conexion">Instancia de <see cref="clsConexion"/> usada para ejecutar la consulta.</param>
        /// <param name="texto">Texto opcional para filtrar productos por nombre (por defecto cadena vacía).</param>
        /// <param name="onFinish">Acción opcional a ejecutar en el hilo de la UI una vez finalizada la carga.</param>
        public static void llenarComboProducto(Krypton.Toolkit.KryptonComboBox cmb, clsConexion conexion, string texto = "", Action onFinish = null)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd;
                if (string.IsNullOrWhiteSpace(texto))
                {
                    cmd = new SqlCommand("SELECT * FROM VistaProducto WHERE id_sucursal = @idSucursal", conexion.SqlC);
                    cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);
                }
                else
                {
                    cmd = new SqlCommand("SELECT * FROM VistaProducto WHERE nombre_producto LIKE @texto AND id_sucursal = @idSucursal", conexion.SqlC);
                    cmd.Parameters.AddWithValue("@texto", "%" + texto.Trim() + "%");
                    cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);
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
                        cmb.DataSource = dt;
                        cmb.DisplayMember = "nombre_producto";
                        cmb.ValueMember = "id_producto";
                        cmb.SelectedIndex = -1;
                    }
                    cmb.Text = textoGuardado;
                    cmb.SelectionStart = textoGuardado.Length;
                    cmb.SelectionLength = 0;

                    onFinish?.Invoke(); // ← al final del BeginInvoke
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
