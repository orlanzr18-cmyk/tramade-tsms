using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TRAMADE.ClasesAdministrador;

namespace TRAMADE.FormulariosAdministrador
{
    public partial class wndEditarUsuario : UserControl
    {
        private readonly clsConexion _conexion = new clsConexion();
        private readonly clsUsuario _objUsuario = new clsUsuario();

        private int _idUsuario;
        private int _idRolInicial;
        private int _idSucursalInicial;

        public event EventHandler UsuarioEditado;

        // ═════════════════════════════════════════════════════════════════════
        //  Constructor — recibe datos del usuario a editar
        // ═════════════════════════════════════════════════════════════════════
        public wndEditarUsuario(int id, string nombre, string correo, int idRol, int idSucursal)
        {
            InitializeComponent();

            _idUsuario = id;
            _idRolInicial = idRol;
            _idSucursalInicial = idSucursal;

            txtNombre.Text = nombre;
            txtCorreo.Text = correo;

            // Ocultar placeholders porque ya hay texto
            phNombre.Visibility = Visibility.Collapsed;
            phCorreo.Visibility = Visibility.Collapsed;

            Loaded += wndEditarUsuario_Loaded;

            // Placeholder contraseña
            txtContrasena.PasswordChanged += (s, e) =>
                phContrasena.Visibility = txtContrasena.Password.Length > 0
                    ? Visibility.Collapsed : Visibility.Visible;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Loaded — llenar combos y preseleccionar
        // ═════════════════════════════════════════════════════════════════════
        private void wndEditarUsuario_Loaded(object sender, RoutedEventArgs e)
        {
            CargarRoles();
            CargarSucursales();
        }

        private void CargarRoles()
        {
            try
            {
                _conexion.Abrir();
                var adapter = new SqlDataAdapter("SELECT id_rol, descripcion_rol FROM VistaRol", _conexion.SqlC);
                var dt = new DataTable();
                adapter.Fill(dt);
                cmbRol.ItemsSource = dt.DefaultView;

                // Preseleccionar el rol actual
                foreach (DataRowView row in dt.DefaultView)
                {
                    if (Convert.ToInt32(row["id_rol"]) == _idRolInicial)
                    {
                        cmbRol.SelectedItem = row;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar roles: " + ex.Message);
            }
            finally { _conexion.Cerrar(); }
        }

        private void CargarSucursales()
        {
            try
            {
                _conexion.Abrir();
                var adapter = new SqlDataAdapter("SELECT id_sucursal, nombre_sucursal FROM VistaSucursal", _conexion.SqlC);
                var dt = new DataTable();
                adapter.Fill(dt);
                cmbSucursal.ItemsSource = dt.DefaultView;

                // Preseleccionar la sucursal actual
                foreach (DataRowView row in dt.DefaultView)
                {
                    if (Convert.ToInt32(row["id_sucursal"]) == _idSucursalInicial)
                    {
                        cmbSucursal.SelectedItem = row;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar sucursales: " + ex.Message);
            }
            finally { _conexion.Cerrar(); }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Placeholders nombre y correo
        // ═════════════════════════════════════════════════════════════════════
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

        // ═════════════════════════════════════════════════════════════════════
        //  Combos
        // ═════════════════════════════════════════════════════════════════════
        private void cmbRol_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void cmbSucursal_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        // ═════════════════════════════════════════════════════════════════════
        //  Cancelar
        // ═════════════════════════════════════════════════════════════════════
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            // Cierra el WinForms padre
            var ventana = System.Windows.Window.GetWindow(this);
            if (ventana != null) ventana.Close();
            else
            {
                var parent = System.Windows.Forms.Application.OpenForms["frmEditar"];
                parent?.Close();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Confirmar / Guardar
        // ═════════════════════════════════════════════════════════════════════
        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtNombre.Text.Trim();
            string correo = txtCorreo.Text.Trim();
            string contrasena = txtContrasena.Password.Trim();

            // Validaciones
            if (!clsValidar.NullOVacio(nombre, "Nombre", null)) return;
            if (!clsValidar.SinEspaciosExtremos(nombre, "Nombre", null)) return;
            if (!clsValidar.SinDobleEspacio(nombre, "Nombre", null)) return;
            if (!clsValidar.LongitudMinima(nombre, "Nombre", 3, null)) return;
            if (!clsValidar.LongitudMaxima(nombre, "Nombre", 50, null)) return;
            if (!clsValidar.SinTresLetrasIguales(nombre, "Nombre")) return;

            if (!clsValidar.ValidarCorreoUsuario(correo, null, _idUsuario)) return;
            if (clsValidar.ContieneEspacios(correo, "Correo", null)) return;

            if (!string.IsNullOrWhiteSpace(contrasena))
            {
                if (!clsValidar.LongitudMinima(contrasena, "Contraseña", 6, null)) return;
                if (!clsValidar.LongitudMaxima(contrasena, "Contraseña", 20, null)) return;
                if (clsValidar.ContieneEspacios(contrasena, "Contraseña", null)) return;
            }

            if (cmbRol.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un rol.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (cmbSucursal.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una sucursal.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var filaRol = (DataRowView)cmbRol.SelectedItem;
            var filaSucursal = (DataRowView)cmbSucursal.SelectedItem;
            int idRol = Convert.ToInt32(filaRol["id_rol"]);
            int idSucursal = Convert.ToInt32(filaSucursal["id_sucursal"]);

            _objUsuario.setRol(idRol);
            _objUsuario.setSucursal(idSucursal);

            bool actualizado = _objUsuario.actualizarUsuario(
                _conexion, _idUsuario, nombre, correo, contrasena);

            if (actualizado)
            {
                MessageBox.Show("Usuario actualizado correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                UsuarioEditado?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MessageBox.Show("Error al actualizar el usuario.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}