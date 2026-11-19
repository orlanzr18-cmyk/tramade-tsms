using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TRAMADE.Formulario_Proveedores.Clases;

namespace TRAMADE.FormulariosProveedor
{
    public partial class ProveedoresEditarControl : UserControl
    {
        private int _idProveedor;
        private clsConexion _conexion = new clsConexion();
        private int _idEstado = 1;

        public ProveedoresEditarControl(int idProveedor)
        {
            InitializeComponent();
            _idProveedor = idProveedor;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LlenarClasificacion();
            LlenarTerminosPago();
            CargarDatosActuales();
        }

        private void LlenarClasificacion()
        {
            try
            {
                _conexion.Abrir();
                var adapter = new SqlDataAdapter(
                    "SELECT id_clasificacion_proveedor, descripcion_clasificacion_proveedor FROM CLASIFICACION_PROVEEDOR",
                    _conexion.SqlC);
                var dt = new DataTable();
                adapter.Fill(dt);
                cmbClasificacion.ItemsSource = dt.DefaultView;
                cmbClasificacion.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar Clasificación: " + ex.Message);
            }
            finally { _conexion.Cerrar(); }
        }

        private void LlenarTerminosPago()
        {
            try
            {
                _conexion.Abrir();
                var adapter = new SqlDataAdapter(
                    "SELECT id_terminos_de_pago_proveedor, descripcion_terminos_de_pago FROM TERMINOS_DE_PAGO_PROVEEDOR",
                    _conexion.SqlC);
                var dt = new DataTable();
                adapter.Fill(dt);
                cmbTerminosdePago.ItemsSource = dt.DefaultView;
                cmbTerminosdePago.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar Términos de Pago: " + ex.Message);
            }
            finally { _conexion.Cerrar(); }
        }

        private void CargarDatosActuales()
        {
            try
            {
                _conexion.Abrir();
                var cmd = new SqlCommand("PA_OBTENER_PERFIL_PROVEEDOR", _conexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_proveedor", _idProveedor);

                var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    // Precargar campos de texto
                    txtNombreComercial.Text = reader["nombre_comercial_proveedor"].ToString();
                    txtDireccionFiscal.Text = reader["direccion_proveedor"].ToString();
                    txtRtn.Text = reader["rtn_proveedor"].ToString();
                    txtTelefonoGeneral.Text = reader["contacto_proveedor"].ToString();
                    txtCorreoCentral.Text = reader["correo_electronico_proveedor"].ToString();
                    _idEstado = Convert.ToInt32(reader["id_estado"]);

                    // Preseleccionar Razón Social
                    string razon = reader["razon_social_proveedor"].ToString();
                    foreach (ComboBoxItem item in cmbRazonSocial.Items)
                    {
                        if (item.Content.ToString() == razon)
                        {
                            cmbRazonSocial.SelectedItem = item;
                            break;
                        }
                    }
                    if (cmbRazonSocial.SelectedItem == null)
                    {
                        cmbRazonSocial.Items.Add(new ComboBoxItem { Content = razon });
                        cmbRazonSocial.SelectedIndex = cmbRazonSocial.Items.Count - 1;
                    }

                    // Preseleccionar Clasificación
                    int idClasif = Convert.ToInt32(reader["id_clasificacion_proveedor"]);
                    reader.Close();
                    SeleccionarEnCombo(cmbClasificacion, "id_clasificacion_proveedor", idClasif);

                    // Preseleccionar Términos de Pago (re-consultar porque el reader ya está cerrado)
                    _conexion.Cerrar();
                    _conexion.Abrir();
                    var cmd2 = new SqlCommand("PA_OBTENER_PERFIL_PROVEEDOR", _conexion.SqlC);
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Parameters.AddWithValue("@id_proveedor", _idProveedor);
                    var r2 = cmd2.ExecuteReader();
                    if (r2.Read())
                    {
                        int idTerminos = Convert.ToInt32(r2["id_terminos_de_pago_proveedor"]);
                        r2.Close();
                        SeleccionarEnCombo(cmbTerminosdePago, "id_terminos_de_pago_proveedor", idTerminos);
                    }
                }
                else
                {
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message);
            }
            finally { _conexion.Cerrar(); }
        }

        private void SeleccionarEnCombo(ComboBox combo, string idColumn, int idValue)
        {
            if (combo.ItemsSource == null) return;
            foreach (DataRowView drv in (DataView)combo.ItemsSource)
            {
                if (Convert.ToInt32(drv[idColumn]) == idValue)
                {
                    combo.SelectedItem = drv;
                    break;
                }
            }
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // ── Validaciones de presencia mínimas (selección de combos) ──
            if (cmbRazonSocial.SelectedItem == null)
            {
                MessageBox.Show("Seleccione la Razón Social.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cmbClasificacion.SelectedItem == null)
            {
                MessageBox.Show("Seleccione la Clasificación.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cmbTerminosdePago.SelectedItem == null)
            {
                MessageBox.Show("Seleccione los Términos de Pago.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rowClasif = (DataRowView)cmbClasificacion.SelectedItem;
            var rowTerminos = (DataRowView)cmbTerminosdePago.SelectedItem;
            int idClasif = Convert.ToInt32(rowClasif["id_clasificacion_proveedor"]);
            int idTerminos = Convert.ToInt32(rowTerminos["id_terminos_de_pago_proveedor"]);
            string razonSocial = (cmbRazonSocial.SelectedItem as ComboBoxItem)?.Content?.ToString()
                                  ?? cmbRazonSocial.Text;

            // ── Validación completa: formato, longitud, duplicados de RTN/Correo, etc. ──
            // idProveedorActual = _idProveedor para que no se marque como "duplicado" contra sí mismo.
            // Nota: clsValidar espera System.Windows.Forms.Control, pero este UserControl es WPF
            // (System.Windows.Controls.TextBox/ComboBox), tipos incompatibles entre sí.
            // Se pasa null en esos parámetros: solo se usan para .Focus() automático (cosmético),
            // la validación de formato/longitud/duplicados funciona igual.
            if (!clsValidar.ValidarProveedor(
                    txtNombreComercial.Text.Trim(), null,
                    razonSocial, null,
                    txtDireccionFiscal.Text.Trim(), null,
                    txtRtn.Text.Trim(), null,
                    txtTelefonoGeneral.Text.Trim(), null,
                    txtCorreoCentral.Text.Trim(), null,
                    idClasif, idTerminos, _idProveedor))
            {
                return;
            }

            try
            {
                clsProveedores objProv = new clsProveedores(
                    txtNombreComercial.Text.Trim(),
                    razonSocial,
                    txtDireccionFiscal.Text.Trim(),
                    txtRtn.Text.Trim(),
                    idClasif,
                    idTerminos,
                    txtTelefonoGeneral.Text.Trim(),
                    txtCorreoCentral.Text.Trim());

                objProv.id_proveedor = _idProveedor;
                objProv.id_estado = _idEstado;

                clsProveedores_Editar objEditar = new clsProveedores_Editar();
                objEditar.ActualizarProveedor(objProv);
                // Nota: se quitó el MessageBox de éxito de aquí porque ActualizarProveedor
                // ya muestra su propio mensaje internamente (causaba mensaje duplicado).
                // Si NO es así, y ahora ya no aparece ningún mensaje de éxito, avísame y lo agrego de nuevo.

                // Recargar perfil si está abierto
                var frmPerfil = System.Windows.Forms.Application.OpenForms["frmProveedores_Perfil"]
                    as Formulario_Proveedores.frmProveedores_Perfil;
                frmPerfil?.RecargarPerfil();

                // Cerrar este form
                var parent = System.Windows.Forms.Application.OpenForms["frmProveedores_Editar"];
                parent?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            var parent = System.Windows.Forms.Application.OpenForms["frmProveedores_Editar"];
            parent?.Close();
        }

        // ── Solo dígitos mientras se escribe, y bloquea al llegar a 8 dígitos ──
        // (clsValidar.Telefono exige exactamente 8 dígitos, sin guion, por eso ya no se permite "-")
        private void txtTelefono_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^\d+$"))
            {
                e.Handled = true;
                return;
            }
            if (txtTelefonoGeneral.Text.Length >= 8 && txtTelefonoGeneral.SelectionLength == 0)
            {
                e.Handled = true;
            }
        }

        // Respaldo por si el usuario pega texto (Ctrl+V) en vez de escribirlo:
        // PreviewTextInput no se dispara al pegar, así que se recorta aquí también.
        private void txtTelefonoGeneral_TextChanged(object sender, TextChangedEventArgs e)
        {
            string soloDigitos = Regex.Replace(txtTelefonoGeneral.Text, @"\D", "");
            if (soloDigitos.Length > 8) soloDigitos = soloDigitos.Substring(0, 8);
            if (soloDigitos != txtTelefonoGeneral.Text)
            {
                int caret = txtTelefonoGeneral.CaretIndex;
                txtTelefonoGeneral.Text = soloDigitos;
                txtTelefonoGeneral.CaretIndex = Math.Min(caret, soloDigitos.Length);
            }
        }

        // ── Solo dígitos mientras se escribe, y bloquea al llegar a 14 dígitos ──
        private void txtRtn_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (txtRtn.Text.Length >= 14 && txtRtn.SelectionLength == 0)
            {
                e.Handled = true;
                return;
            }
            e.Handled = !Regex.IsMatch(e.Text, @"[\d]");
        }

        // Respaldo por si el usuario pega texto (Ctrl+V) en el RTN:
        private void txtRtn_TextChanged(object sender, TextChangedEventArgs e)
        {
            string soloDigitos = Regex.Replace(txtRtn.Text, @"\D", "");
            if (soloDigitos.Length > 14) soloDigitos = soloDigitos.Substring(0, 14);
            if (soloDigitos != txtRtn.Text)
            {
                int caret = txtRtn.CaretIndex;
                txtRtn.Text = soloDigitos;
                txtRtn.CaretIndex = Math.Min(caret, soloDigitos.Length);
            }
        }
    }
}