using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TRAMADE.Services;
using TRAMADE.Models;

namespace TRAMADE.FormulariosCliente
{
    public partial class wndRegistro : UserControl
    {
        private readonly clsClienteService ObjServicio = new clsClienteService();

        public wndRegistro()
        {
            InitializeComponent();
            Loaded += wndRegistro_Loaded;
        }

        private void wndRegistro_Loaded(object sender, RoutedEventArgs e)
        {
            CargarComboDepartamento();
            CargarComboTipoCliente();
            CargarComboRazonSocial();

            txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtID.Text = "AUTOGENERADO";

            txtDNI.IsEnabled = false;
            txtDNI.Background = System.Windows.Media.Brushes.LightGray;

            cmbCiudad.IsEnabled = false;

            txtNombre.Focus();
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
                cmbCiudad.SelectedIndex = -1;
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
                    cmbRazonSocial.SelectedIndex = -1;
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
            txtID.Text = "AUTOGENERADO";
            txtNombre.Text = "";
            txtContacto.Text = "";
            txtTelefono.Text = "";
            txtCorreo.Text = "";
            txtDireccion.Text = "";
            txtRTN.Text = "";
            txtDNI.Text = "";

            cmbTipoCliente.SelectedIndex = -1;
            cmbDepartamento.SelectedIndex = -1;
            cmbCiudad.ItemsSource = null;
            cmbCiudad.IsEnabled = false;
            cmbRazonSocial.SelectedIndex = -1;

            txtFecha.Text = DateTime.Now.ToString("dd/MM/yyyy");

            txtNombre.Focus();
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
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
                0,
                txtContacto.Text, null
            );

            if (!esValido) return;

            try
            {
                var ObjCliente = new clsClienteModel
                {
                    IdUsuario = clsSesion.id_usuario,
                    Nombre = txtNombre.Text,
                    RazonSocial = cmbRazonSocial.Text,
                    Contacto = txtContacto.Text,
                    Telefono = txtTelefono.Text,
                    Correo = txtCorreo.Text,
                    Direccion = txtDireccion.Text,
                    Rtn = txtRTN.Text,
                    Dni = txtDNI.Text,
                    FechaRegistro = DateTime.Now,
                    TipoCliente = idTipoCliente.ToString(),
                    IdDepartamento = Convert.ToInt32(idDepartamento),
                    IdCiudad = Convert.ToInt32(idCiudad),
                    Estado = 7
                };

                bool resultado = ObjServicio.RegistrarCliente(ObjCliente);

                if (resultado)
                {
                    MessageBox.Show("¡Cliente registrado exitosamente!", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    btnLimpiar_Click(sender, e);
                }
                else
                {
                    MessageBox.Show("No se pudo registrar la información del cliente.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error crítico al registrar: " + ex.Message, "Error de Sistema",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}