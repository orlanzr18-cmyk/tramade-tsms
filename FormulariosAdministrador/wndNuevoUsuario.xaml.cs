using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using TRAMADE.ClasesAdministrador;

namespace TRAMADE.FormulariosAdministrador
{
    public partial class wndNuevoUsuario : UserControl
    {
        private readonly clsConexion _conexion = new clsConexion();
        private readonly clsUsuario _objUsuario = new clsUsuario();

        public event EventHandler UsuarioRegistrado;


        public wndNuevoUsuario()
        {
            InitializeComponent();
            Loaded += wndNuevoUsuario_Loaded;
        }

        // ═══════════════════════════════════════════
        //  Placeholders
        // ═══════════════════════════════════════════
        private void txtNombre_GotFocus(object sender, RoutedEventArgs e)
            => phNombre.Visibility = Visibility.Collapsed;

        private void txtNombre_LostFocus(object sender, RoutedEventArgs e)
            => phNombre.Visibility = string.IsNullOrEmpty(txtNombre.Text)
                ? Visibility.Visible : Visibility.Collapsed;

        private void txtCorreo_GotFocus(object sender, RoutedEventArgs e)
            => phCorreo.Visibility = Visibility.Collapsed;

        private void txtCorreo_LostFocus(object sender, RoutedEventArgs e)
            => phCorreo.Visibility = string.IsNullOrEmpty(txtCorreo.Text)
                ? Visibility.Visible : Visibility.Collapsed;

        private void txtContrasena_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // placeholder de contraseña lo maneja el PasswordBox nativo
        }
        // ═════════════════════════════════════════════════════════════════════
        //  Loaded — llenar combos
        // ═════════════════════════════════════════════════════════════════════
        private void wndNuevoUsuario_Loaded(object sender, RoutedEventArgs e)
        {
            CargarRoles();
            CargarSucursales();
        }

        private void CargarRoles()
        {
            try
            {
                _conexion.Abrir();
                string consulta = "SELECT id_rol, descripcion_rol FROM VistaRol";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, _conexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                cmbRol.ItemsSource = dt.DefaultView;
                cmbRol.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar roles: " + ex.Message);
            }
            finally
            {
                _conexion.Cerrar();
            }
        }

        private void CargarSucursales()
        {
            try
            {
                _conexion.Abrir();
                string consulta = "SELECT id_sucursal, nombre_sucursal FROM VistaSucursal";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, _conexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                cmbSucursal.ItemsSource = dt.DefaultView;
                cmbSucursal.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar sucursales: " + ex.Message);
            }
            finally
            {
                _conexion.Cerrar();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Agregar
        // ═════════════════════════════════════════════════════════════════════
        private void btnAgregar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string correo = txtCorreo.Text.Trim();
            string contrasena = txtContrasena.Password.Trim();

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena) ||
                cmbRol.SelectedIndex == -1 ||
                cmbSucursal.SelectedIndex == -1)
            {
                MessageBox.Show("Llene todos los campos.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!clsValidar.NullOVacio(nombre, "Nombre", null)) return;
            if (!clsValidar.SinEspaciosExtremos(nombre, "Nombre", null)) return;
            if (!clsValidar.SinDobleEspacio(nombre, "Nombre", null)) return;
            if (!clsValidar.LongitudMinima(nombre, "Nombre", 3, null)) return;
            if (!clsValidar.LongitudMaxima(nombre, "Nombre", 50, null)) return;
            if (!clsValidar.SinTresLetrasIguales(nombre, "Nombre")) return;

            if (!clsValidar.ValidarCorreoUsuario(correo, null)) return;
            if (clsValidar.ContieneEspacios(correo, "Correo", null)) return;

            if (!clsValidar.LongitudMinima(contrasena, "Contraseña", 6, null)) return;
            if (!clsValidar.LongitudMaxima(contrasena, "Contraseña", 20, null)) return;
            if (clsValidar.ContieneEspacios(contrasena, "Contraseña", null)) return;

            // Obtener IDs seleccionados
            var filaRol = ((DataRowView)cmbRol.SelectedItem);
            var filaSucursal = ((DataRowView)cmbSucursal.SelectedItem);
            int idRol = Convert.ToInt32(filaRol["id_rol"]);
            int idSucursal = Convert.ToInt32(filaSucursal["id_sucursal"]);

            _objUsuario.setNombre(nombre);
            _objUsuario.setCorreo(correo);
            _objUsuario.setContrasena(contrasena);
            _objUsuario.setRol(idRol);
            _objUsuario.setSucursal(idSucursal);

            if (_objUsuario.insertarUsuarios(_conexion))
            {
                MessageBox.Show("¡Usuario registrado con éxito!", "Sistema",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                UsuarioRegistrado?.Invoke(this, EventArgs.Empty);
                LimpiarCampos();
            }
            else
            {
                MessageBox.Show("No se pudo completar el registro.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Limpiar
        // ═════════════════════════════════════════════════════════════════════
        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            txtNombre.Text = string.Empty;
            txtCorreo.Text = string.Empty;
            txtContrasena.Clear();
            cmbRol.SelectedIndex = -1;
            cmbSucursal.SelectedIndex = -1;
        }
    }
}