using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.Formulario_Proveedores
{
    /// <summary>
    /// Utilidad para llenar combos (KryptonComboBox) relacionados con proveedores.
    /// Proporciona métodos para cargar clasificación, términos de pago y razón social.
    /// </summary>
    internal class clsCombobox
    {
        // ─── LLENAR COMBO CLASIFICACIÓN ──────────────────────────────
        /// <summary>
        /// Llena el combo de clasificación con los registros de la tabla
        /// <c>CLASIFICACION_PROVEEDOR</c> usando la conexión proporcionada.
        /// </summary>
        /// <param name="cmb">Control <see cref="KryptonComboBox"/> que se llenará.</param>
        /// <param name="ObjConexion">Instancia de <see cref="clsConexion"/> para ejecutar la consulta.</param>
        public static void LlenarClasificacion(KryptonComboBox cmb, clsConexion ObjConexion)
        {
            try
            {
                ObjConexion.Abrir();
                SqlDataAdapter adapter = new SqlDataAdapter(
                    "SELECT id_clasificacion_proveedor, descripcion_clasificacion_proveedor FROM CLASIFICACION_PROVEEDOR",
                    ObjConexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmb.DataSource = new BindingSource { DataSource = dt };
                cmb.DisplayMember = "descripcion_clasificacion_proveedor";
                cmb.ValueMember = "id_clasificacion_proveedor";
                cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar Clasificación: " + ex.Message);
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }

        // ─── LLENAR COMBO TÉRMINOS DE PAGO ───────────────────────────
        /// <summary>
        /// Llena el combo de términos de pago con los registros de la tabla
        /// <c>TERMINOS_DE_PAGO_PROVEEDOR</c> usando la conexión proporcionada.
        /// </summary>
        /// <param name="cmb">Control <see cref="KryptonComboBox"/> que se llenará.</param>
        /// <param name="ObjConexion">Instancia de <see cref="clsConexion"/> para ejecutar la consulta.</param>
        public static void LlenarTerminosPago(KryptonComboBox cmb, clsConexion ObjConexion)
        {
            try
            {
                ObjConexion.Abrir();
                SqlDataAdapter adapter = new SqlDataAdapter(
                    "SELECT id_terminos_de_pago_proveedor, descripcion_terminos_de_pago FROM TERMINOS_DE_PAGO_PROVEEDOR",
                    ObjConexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmb.DataSource = new BindingSource { DataSource = dt };
                cmb.DisplayMember = "descripcion_terminos_de_pago";
                cmb.ValueMember = "id_terminos_de_pago_proveedor";
                cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar Términos de Pago: " + ex.Message);
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }

        // ─── LLENAR AMBOS COMBOS DE UNA VEZ ──────────────────────────
        /// <summary>
        /// Llena simultáneamente los combos de clasificación y términos de pago
        /// reutilizando los métodos especializados anteriores.
        /// </summary>
        /// <param name="cmbClasificacion">Combo de clasificación a llenar.</param>
        /// <param name="cmbTerminosdePago">Combo de términos de pago a llenar.</param>
        /// <param name="ObjConexion">Instancia de conexión a la base de datos.</param>
        public static void LlenarCombosProveedor(KryptonComboBox cmbClasificacion,
                                                  KryptonComboBox cmbTerminosdePago,
                                                  clsConexion ObjConexion)
        {
            LlenarClasificacion(cmbClasificacion, ObjConexion);
            LlenarTerminosPago(cmbTerminosdePago, ObjConexion);
        }
        /// <summary>
        /// Llena el combo de razón social con opciones estáticas comunes y establece
        /// el estilo <see cref="ComboBoxStyle.DropDownList"/>.
        /// </summary>
        /// <param name="cmb">Combo que se llenará con las opciones de razón social.</param>
        public static void LlenarRazonSocial(KryptonComboBox cmb)
        {
            cmb.Items.Clear();
            cmb.Items.Add("S.A. de C.V.");
            cmb.Items.Add("S. de R.L.");
            cmb.Items.Add("S. de R.L. de C.V.");
            cmb.Items.Add("Comerciante Individual");
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
        }

    }
}
