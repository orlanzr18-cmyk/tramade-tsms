using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TRAMADE.Services.ServBitacora; // Referencia a tu nueva capa de servicio

namespace TRAMADE.FormulariosAdministrador
{
    /// <summary>
    /// Lógica de interacción para ucBitacora.xaml
    /// Refactorizado siguiendo arquitectura en capas y principios SOLID.
    /// </summary>
    public partial class ucBitacora : UserControl
    {
        // ─── DEPENDENCIAS ──────────────────────────────────────────────────
        // Resuelve TS-61-H10: Desacoplamiento de la base de datos
        private readonly clsBitacoraService _bitacoraService = new clsBitacoraService();

        // ─── CONSTANTES Y ESTADO ───────────────────────────────────────────
        private const string PlaceholderBuscar = "🔍  Buscar por usuario...";
        private bool _listo = false;

        public ucBitacora()
        {
            InitializeComponent();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  INICIALIZACIÓN
        // ═════════════════════════════════════════════════════════════════════
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Resuelve TS-61-H08: Carga de catálogos mediante servicio, no SQL directo
                cmbRol.ItemsSource = _bitacoraService.ListarRoles().DefaultView;

                EjecutarConsulta();
            }
            catch (Exception ex)
            {
                NotificarError("Error al inicializar el módulo", ex);
            }
            finally
            {
                _listo = true; // Permite que los eventos de UI disparen búsquedas
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  LÓGICA DE CARGA Y FILTRADO
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Captura los estados de la UI y solicita los datos al servicio.
        /// Resuelve TS-61-H07, H09 y H11.
        /// </summary>
        private void EjecutarConsulta()
        {
            try
            {
                // Extraer valores de la interfaz
                string nombre = ObtenerTextoBuscador();
                string rol = (cmbRol.SelectedItem is DataRowView drv) ? drv["Rol"].ToString() : null;
                DateTime? desde = dtDesde.SelectedDate;
                DateTime? hasta = dtHasta.SelectedDate;

                // Delegar lógica de negocio al servicio (Resuelve TS-61-H09)
                DataTable dt = _bitacoraService.FiltrarBitacora(desde, hasta, rol, nombre);

                // Actualizar la grilla
                ActualizarResultados(dt);
            }
            catch (ArgumentException ex)
            {
                // Errores de validación de fechas (desde > hasta)
                MessageBox.Show(ex.Message, "Validación de Filtros", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                NotificarError("Error al consultar la bitácora", ex);
            }
        }

        private void ActualizarResultados(DataTable dt)
        {
            if (dgvBitacora == null) return;

            dgvBitacora.ItemsSource = dt?.DefaultView;

            // Actualizar conteo de registros
            int total = dt?.Rows.Count ?? 0;
            lblConteo.Text = (total == 1) ? "(1 registro)" : $"({total} registros)";
        }

        private string ObtenerTextoBuscador()
        {
            string texto = txtBuscarUsuario.Text;
            return (texto == PlaceholderBuscar || string.IsNullOrWhiteSpace(texto)) ? null : texto.Trim();
        }

        private void NotificarError(string mensaje, Exception ex)
        {
            MessageBox.Show($"{mensaje}:\n{ex.Message}", "Error de Sistema", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  EVENTOS DE LA INTERFAZ (UI)
        // ═════════════════════════════════════════════════════════════════════

        private void txtBuscarUsuario_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Resuelve TS-61-H13: Cohesión en el manejo de eventos
            if (!_listo) return;

            string texto = ObtenerTextoBuscador();
            // Disparar búsqueda solo si hay suficiente texto o si se limpia el campo
            if (texto == null || texto.Length >= 2)
            {
                EjecutarConsulta();
            }
        }

        private void cmbRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_listo) EjecutarConsulta();
        }

        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            EjecutarConsulta();
        }

        private void btnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            // Bloquear eventos para evitar múltiples llamadas
            _listo = false;

            // Resetear controles
            txtBuscarUsuario.Text = PlaceholderBuscar;
            txtBuscarUsuario.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b0a090"));
            cmbRol.SelectedIndex = -1;
            dtDesde.SelectedDate = null;
            dtHasta.SelectedDate = null;

            _listo = true;
            EjecutarConsulta();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  MANEJO DE PLACEHOLDER (ESTÉTICO)
        // ═════════════════════════════════════════════════════════════════════
        private void txtBuscarUsuario_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtBuscarUsuario.Text == PlaceholderBuscar)
            {
                txtBuscarUsuario.Text = "";
                txtBuscarUsuario.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3d2b1f"));
            }
        }

        private void txtBuscarUsuario_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBuscarUsuario.Text))
            {
                txtBuscarUsuario.Text = PlaceholderBuscar;
                txtBuscarUsuario.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b0a090"));
            }
        }
    }
}