using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TRAMADE.ClasesAdministrador;

namespace TRAMADE.FormulariosAdministrador
{
    public partial class ucUsuarios : UserControl
    {
        private readonly clsConexion _conexion = new clsConexion();
        private readonly clsUsuario _objUsuario = new clsUsuario();
        private bool _placeholderActivo = true;
        private DataTable _tablaActual;

        public ucUsuarios()
        {
            InitializeComponent();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Loaded
        // ═════════════════════════════════════════════════════════════════════
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RecargarUsuarios();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Cargar tabla
        // ═════════════════════════════════════════════════════════════════════
        private void RecargarUsuarios(string filtro = "")
        {
            DataTable dt = string.IsNullOrWhiteSpace(filtro)
                ? _objUsuario.obtenerUsuariosTabla(_conexion)
                : _objUsuario.buscarUsuariosPorNombre(_conexion, filtro);

            if (dt == null) return;

            if (!dt.Columns.Contains("Seleccionado"))
                dt.Columns.Add("Seleccionado", typeof(bool)).DefaultValue = false;

            foreach (DataRow row in dt.Rows)
                if (row["Seleccionado"] == DBNull.Value)
                    row["Seleccionado"] = false;

            _tablaActual = dt;
            dgbUsuarios.ItemsSource = null;         // forzar refresco limpio
            dgbUsuarios.ItemsSource = dt.DefaultView;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Placeholder buscador
        // ═════════════════════════════════════════════════════════════════════
        private void txtBuscar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_placeholderActivo)
            {
                txtBuscar.Text = string.Empty;
                txtBuscar.Foreground = Brushes.Black;
                _placeholderActivo = false;
            }
        }

        private void txtBuscar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                _placeholderActivo = true;

                // Desuscribir antes de cambiar el texto para no disparar TextChanged
                txtBuscar.TextChanged -= txtBuscar_TextChanged;
                txtBuscar.Text = (string)txtBuscar.Tag;
                txtBuscar.Foreground = Brushes.Gray;
                txtBuscar.TextChanged += txtBuscar_TextChanged;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Buscador en tiempo real
        // ═════════════════════════════════════════════════════════════════════
        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_placeholderActivo) return;
            if (string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                RecargarUsuarios();
                return;
            }
            RecargarUsuarios(txtBuscar.Text.Trim());
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (_placeholderActivo)
            {
                RecargarUsuarios();
                return;
            }
            RecargarUsuarios(txtBuscar.Text.Trim());
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Nuevo
        // ═════════════════════════════════════════════════════════════════════
        private void btnNuevo_Click(object sender, RoutedEventArgs e)
        {
            frmNuevo ventana = new frmNuevo();
            ventana.ShowDialog();

            // Recargar sin perder el filtro activo
            string filtroActual = _placeholderActivo ? "" : txtBuscar.Text.Trim();
            RecargarUsuarios(filtroActual);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Editar
        // ═════════════════════════════════════════════════════════════════════
        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgbUsuarios.SelectedItem is DataRowView fila))
            {
                MessageBox.Show("Seleccione un usuario de la lista.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int id = Convert.ToInt32(fila["ID"]);
            string nombre = fila["Nombre"].ToString();
            string correo = fila["Correo electrónico"].ToString();
            int idRol = Convert.ToInt32(fila["ID rol"]);
            int idSucursal = Convert.ToInt32(fila["ID sucursal"]);

            frmEditar ventana = new frmEditar(id, nombre, correo, idRol, idSucursal);
            ventana.ShowDialog();

            string filtroActual = _placeholderActivo ? "" : txtBuscar.Text.Trim();
            RecargarUsuarios(filtroActual);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Biométrico
        // ═════════════════════════════════════════════════════════════════════
        private void btnBiometrico_Click(object sender, RoutedEventArgs e)
        {
            frmAgregarUsusarioBiometrico ventana = new frmAgregarUsusarioBiometrico();
            ventana.ShowDialog();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Activar / Inactivar
        // ═════════════════════════════════════════════════════════════════════
        private void btnActivar_Click(object sender, RoutedEventArgs e)
            => CambiarEstadoSeleccionados(1, "activado");

        private void btnInactivar_Click(object sender, RoutedEventArgs e)
            => CambiarEstadoSeleccionados(2, "inactivado");

        private void CambiarEstadoSeleccionados(int estado, string accion)
        {
            if (_tablaActual == null) return;

            bool haySeleccionados = false;
            int contador = 0;

            foreach (DataRow row in _tablaActual.Rows)
            {
                if (row["Seleccionado"] != DBNull.Value &&
                    Convert.ToBoolean(row["Seleccionado"]))
                {
                    haySeleccionados = true;
                    try
                    {
                        int id = Convert.ToInt32(row["ID"]);
                        if (_objUsuario.cambiarEstado(_conexion, id, estado))
                            contador++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            if (haySeleccionados)
            {
                MessageBox.Show($"{contador} usuario(s) {accion}(s) correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                string filtroActual = _placeholderActivo ? "" : txtBuscar.Text.Trim();
                RecargarUsuarios(filtroActual);
            }
            else
            {
                MessageBox.Show("Marque la casilla en los usuarios que desea modificar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}