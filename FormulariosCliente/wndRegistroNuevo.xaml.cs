using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TRAMADE.Models;
using TRAMADE.Services;

namespace TRAMADE.FormulariosCliente
{
    public partial class wndRegistroNuevo : UserControl
    {
        private readonly clsClienteService ObjServicio = new clsClienteService();
        private System.Collections.Generic.List<string> ObjSugerencias = new System.Collections.Generic.List<string>();
        private bool ObjSeleccionandoSugerencia = false;

        public wndRegistroNuevo()
        {
            InitializeComponent();
            Loaded += wndRegistroNuevo_Loaded;
        }

        private void wndRegistroNuevo_Loaded(object sender, RoutedEventArgs e)
        {
            CargarComboDepartamento();
            CargarComboTipoCliente();
            CargarComboRazonSocial();

            txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");

            txtDNI.IsEnabled = false;
            txtDNI.Background = System.Windows.Media.Brushes.LightGray;

            cmbCiudad.IsEnabled = false;

            txtBuscar.Focus();

            CargarSugerenciasAutoComplete();
        }

        private void CargarComboDepartamento()
        {
            try
            {
                DataTable dt = ObjServicio.ObtenerDepartamentos();
                cmbDepartamento.ItemsSource = dt.DefaultView;
                cmbDepartamento.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar departamentos: " + ex.Message);
            }
        }

        private void CargarComboCiudad(int idDepartamento)
        {
            try
            {
                DataTable dt = ObjServicio.ObtenerCiudades(idDepartamento);
                cmbCiudad.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar ciudades: " + ex.Message);
            }
        }

        private void CargarComboTipoCliente()
        {
            try
            {
                DataTable dt = ObjServicio.ObtenerTiposCliente();
                cmbTipoCliente.ItemsSource = dt.DefaultView;
                cmbTipoCliente.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar tipos de cliente: " + ex.Message);
            }
        }

        private void CargarComboRazonSocial()
        {
            cmbRazonSocial.Items.Clear();
            cmbRazonSocial.Items.Add("S.A. de C.V.");
            cmbRazonSocial.Items.Add("S. de R.L.");
            cmbRazonSocial.Items.Add("S. de R.L. de C.V.");
            cmbRazonSocial.Items.Add("Comerciante Individual");
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string busqueda = txtBuscar.Text.Trim();
            if (string.IsNullOrEmpty(busqueda))
            {
                MessageBox.Show("Por favor, ingrese el DNI/RTN del cliente.", "Búsqueda",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DataTable dt = ObjServicio.ObtenerClienteCompletoPorDocumento(busqueda);

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    txtID.Text = row["id_cliente"]?.ToString() ?? "";
                    txtNombre.Text = row["nombre_cliente"]?.ToString() ?? "";
                    txtContacto.Text = row["contacto_cliente"]?.ToString() ?? "";
                    txtTelefono.Text = row["telefono_cliente"]?.ToString() ?? "";
                    txtCorreo.Text = row["correo_electronico_cliente"]?.ToString() ?? "";
                    txtDireccion.Text = row["direccion_cliente"]?.ToString() ?? "";
                    txtDNI.Text = row["dni_cliente"]?.ToString() ?? "";
                    txtRTN.Text = row["rtn_cliente"]?.ToString() ?? "";
                    cmbRazonSocial.Text = row["razon_social"]?.ToString() ?? "";

                    if (row["id_clasificacion_cliente"] != DBNull.Value)
                        cmbTipoCliente.SelectedValue = row["id_clasificacion_cliente"];

                    txtDNI.IsEnabled = true;
                    txtDNI.Background = System.Windows.Media.Brushes.White;

                    if (row["id_departamento"] != DBNull.Value)
                    {
                        int idDepa = Convert.ToInt32(row["id_departamento"]);
                        cmbDepartamento.SelectedValue = idDepa;
                        CargarComboCiudad(idDepa);
                        cmbCiudad.IsEnabled = true;

                        if (row["id_ciudad"] != DBNull.Value)
                            cmbCiudad.SelectedValue = row["id_ciudad"];
                    }
                }
                else
                {
                    MessageBox.Show("No se encontró ningún cliente con el ID proporcionado.", "Búsqueda",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarSugerenciasAutoComplete()
        {
            try
            {
                ObjSugerencias = ObjServicio.ObtenerSugerenciasAutoComplete();
            }
            catch { }
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ObjSeleccionandoSugerencia) return;

            string texto = txtBuscar.Text.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                popupSugerencias.IsOpen = false;
                return;
            }

            var filtradas = ObjSugerencias
                .Where(s => s.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(8).ToList();

            if (filtradas.Any())
            {
                lstSugerencias.ItemsSource = filtradas;
                popupSugerencias.IsOpen = true;
            }
            else
            {
                popupSugerencias.IsOpen = false;
            }
        }

        private void lstSugerencias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstSugerencias.SelectedItem == null) return;

            ObjSeleccionandoSugerencia = true;
            string sel = lstSugerencias.SelectedItem.ToString();
            string doc = sel.Contains(" — ") ? sel.Split(new[] { " — " }, StringSplitOptions.None)[0].Trim() : sel;
            txtBuscar.Text = doc;
            popupSugerencias.IsOpen = false;
            ObjSeleccionandoSugerencia = false;

            btnBuscar_Click(sender, e);
        }

        private void cmbDepartamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbDepartamento.SelectedItem is DataRowView drv)
            {
                int idSeleccionado = Convert.ToInt32(drv["id_departamento"]);
                CargarComboCiudad(idSeleccionado);
                cmbCiudad.IsEnabled = true;
            }
            else
            {
                cmbCiudad.ItemsSource = null;
                cmbCiudad.IsEnabled = false;
            }
        }

        private void cmbTipoCliente_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTipoCliente.SelectedItem is DataRowView drv)
            {
                string tipo = drv["descripcion_clasificacion_cliente"].ToString();

                if (tipo == "PERSONA JURÍDICA")
                {
                    txtRTN.IsEnabled = true;
                    txtRTN.Background = System.Windows.Media.Brushes.White;
                    cmbRazonSocial.IsEnabled = true;

                    txtDNI.IsEnabled = false;
                    txtDNI.Text = "";
                    txtDNI.Background = System.Windows.Media.Brushes.LightGray;
                }
                else if (tipo == "PERSONA NATURAL")
                {
                    txtDNI.IsEnabled = true;
                    txtDNI.Background = System.Windows.Media.Brushes.White;

                    txtRTN.IsEnabled = false;
                    txtRTN.Text = "";
                    txtRTN.Background = System.Windows.Media.Brushes.LightGray;
                    cmbRazonSocial.IsEnabled = false;
                }
            }
        }

        private void txtSoloNumeros_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void txtCorreo_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!clsValidar.SoloCaracteresCorreo_PreviewTextInput(e)) return;

            var textBox = sender as TextBox;
            if (textBox != null && e.Text.Length > 0 && char.IsUpper(e.Text[0]))
            {
                e.Handled = true;
                int caret = textBox.CaretIndex;
                textBox.Text = textBox.Text.Insert(caret, e.Text.ToLower());
                textBox.CaretIndex = caret + 1;
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtID.Clear();
            txtNombre.Text = "";
            txtContacto.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";
            txtDireccion.Text = "";
            txtRTN.Text = "";
            txtDNI.Text = "";
            txtBuscar.Text = "";

            cmbTipoCliente.SelectedIndex = -1;
            cmbDepartamento.SelectedIndex = -1;
            cmbCiudad.ItemsSource = null;
            cmbCiudad.IsEnabled = false;
            cmbRazonSocial.SelectedIndex = -1;

            txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");

            txtDNI.IsEnabled = false;
            txtDNI.Background = System.Windows.Media.Brushes.LightGray;
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(txtID.Text, out int idActual))
            {
                MessageBox.Show("Debe buscar un cliente válido antes de guardar.", "TRAMADE",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string tipoClienteTexto = (cmbTipoCliente.SelectedItem as DataRowView)?
                ["descripcion_clasificacion_cliente"]?.ToString() ?? "";

            object idTipoCliente = cmbTipoCliente.SelectedValue;
            object idDepartamento = cmbDepartamento.SelectedValue;
            object idCiudad = cmbCiudad.SelectedValue;

            bool esValido = clsValidar.ValidarTodoElFormulario(
                txtNombre.Text, null,
                tipoClienteTexto,
                txtDNI.Text, null,
                txtRTN.Text, null,
                cmbRazonSocial.Text, null,
                txtTelefono.Text, null,
                txtCorreo.Text, null,
                txtDireccion.Text, null,
                idDepartamento, idCiudad, idTipoCliente,
                idActual,
                txtContacto.Text, null
            );

            if (!esValido) return;

            try
            {
                var ObjClienteEditado = new clsClienteModel
                {
                    Id = idActual,
                    IdUsuario = clsSesion.id_usuario,
                    Nombre = txtNombre.Text,
                    RazonSocial = cmbRazonSocial.Text,
                    Contacto = txtContacto.Text,
                    Telefono = txtTelefono.Text,
                    Correo = txtCorreo.Text,
                    Direccion = txtDireccion.Text,
                    Rtn = txtRTN.Text,
                    Dni = txtDNI.Text,
                    TipoCliente = idTipoCliente.ToString(),
                    IdCiudad = Convert.ToInt32(idCiudad)
                };

                bool resultado = ObjServicio.ActualizarCliente(ObjClienteEditado);

                if (resultado)
                {
                    MessageBox.Show("¡Datos del cliente actualizados exitosamente!", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLimpiar_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar la información del cliente.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error crítico al actualizar: " + ex.Message, "Error de Sistema",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}