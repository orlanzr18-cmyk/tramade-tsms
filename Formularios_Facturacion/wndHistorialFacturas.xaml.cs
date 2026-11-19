using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TRAMADE.Models;
using TRAMADE.Services;

namespace TRAMADE
{
    // Modelo de fila para la UI (Mantiene el "Numeral" para la vista)
    public class HistorialFacturaRow
    {
        public int Numeral { get; set; }
        public string NumFactura { get; set; }
        public string IDVendedor { get; set; }
        public string DNIRTNCliente { get; set; }
        public string FechaEmision { get; set; }
        public int IdFactura { get; set; }
    }

    public partial class wndHistorialFacturas : UserControl
    {
        public event EventHandler SolicitudCerrar;

        // --- SERVICIOS (Auditoría: TS-51-H45) ---
        private readonly clsHistorialFacturaService _historialService = new clsHistorialFacturaService();

        // --- ESTADO ---
        private bool _ordenAscendente = false;
        private DateTime? _fechaFiltro = null;
        private readonly ObservableCollection<HistorialFacturaRow> _filas = new ObservableCollection<HistorialFacturaRow>();

        public wndHistorialFacturas()
        {
            InitializeComponent();
            dgvHistorialFacturas.ItemsSource = _filas;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarHistorial();

            // Bloqueo de escritura manual en el DatePicker (solo selección desde el calendario)
            DataObject.AddPastingHandler(dtpCalendario, (s, args) => args.CancelCommand());
            dtpCalendario.PreviewTextInput += dtpCalendario_PreviewTextInput;
            dtpCalendario.PreviewKeyDown += dtpCalendario_PreviewKeyDown;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  CARGA DE HISTORIAL (Auditoría TS-51-H44 y H46)
        // ═════════════════════════════════════════════════════════════════════
        private void CargarHistorial(string filtro = "", DateTime? fecha = null)
        {
            try
            {
                _filas.Clear();

                // Hallazgo TS-51-H46: El servicio ahora maneja la lógica de negocio y filtros
                var listaModelos = _historialService.Consultar(filtro, fecha, _ordenAscendente);
                int total = _historialService.ObtenerTotal();

                // Lógica de numeración visual (Numeral)
                int contador = _ordenAscendente ? 1 : listaModelos.Count;

                foreach (var fac in listaModelos)
                {
                    _filas.Add(new HistorialFacturaRow
                    {
                        Numeral = contador,
                        NumFactura = fac.NumeroFormateado, // Propiedad inteligente del Modelo
                        IDVendedor = fac.IdUsuario.ToString(),
                        DNIRTNCliente = fac.DocumentoCliente, // Resuelve RTN/DNI/SN automáticamente
                        FechaEmision = fac.FechaEmision.ToString("dd/MM/yyyy HH:mm"),
                        IdFactura = fac.Id
                    });

                    if (_ordenAscendente) contador++; else contador--;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar historial: " + ex.Message, "TRAMADE", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  ACCIONES Y NAVEGACIÓN (Auditoría TS-51-H48)
        // ═════════════════════════════════════════════════════════════════════
        private void AbrirFacturaSeleccionada()
        {
            if (dgvHistorialFacturas.SelectedItem is HistorialFacturaRow fila)
            {
                // Navegación hacia la visualización/impresión
                var wnd = new frmEmitirFactura { IdFacturaRecibido = fila.IdFactura };
                wnd.ShowDialog();
            }
            else
            {
                MessageBox.Show("Seleccione una factura de la lista.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnOrdenarFacturas_Click(object sender, RoutedEventArgs e)
        {
            _ordenAscendente = !_ordenAscendente;
            btnOrdenarFacturas.Content = _ordenAscendente ? "⇅  Asc" : "⇅  Desc";
            CargarHistorial(ObtenerTextoBusqueda(), _fechaFiltro);
        }

        private void btnCalendarioFacturas_Click(object sender, RoutedEventArgs e)
        {
            if (dtpCalendario.SelectedDate.HasValue)
            {
                _fechaFiltro = dtpCalendario.SelectedDate.Value;
                // Al filtrar por fecha, limpiamos visualmente el buscador de texto
                LimpiarBuscadorTexto();
                CargarHistorial("", _fechaFiltro);
            }
        }

        private void btnLimpiarFecha_Click(object sender, RoutedEventArgs e)
        {
            _fechaFiltro = null;
            dtpCalendario.SelectedDate = null;
            CargarHistorial(ObtenerTextoBusqueda());
        }

        // ═════════════════════════════════════════════════════════════════════
        //  MANEJO DE UI Y PLACEHOLDERS
        // ═════════════════════════════════════════════════════════════════════
        private void txtBuscarFactura_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgvHistorialFacturas == null) return;
            string texto = ObtenerTextoBusqueda();

            // Si el usuario escribe, priorizamos búsqueda por texto sobre fecha
            if (!string.IsNullOrEmpty(texto)) _fechaFiltro = null;

            CargarHistorial(texto, _fechaFiltro);
        }

        private string ObtenerTextoBusqueda()
        {
            string texto = txtBuscarFactura.Text.Trim();
            return (texto == (txtBuscarFactura.Tag as string)) ? "" : texto;
        }

        private void LimpiarBuscadorTexto()
        {
            txtBuscarFactura.Text = txtBuscarFactura.Tag as string;
            txtBuscarFactura.Foreground = System.Windows.Media.Brushes.Gray;
        }

        private void btnVerFacturaHF_Click(object sender, RoutedEventArgs e) => AbrirFacturaSeleccionada();
        private void dgvHistorialFacturas_MouseDoubleClick(object sender, MouseButtonEventArgs e) => AbrirFacturaSeleccionada();
        private void btnRegresarHF_Click(object sender, RoutedEventArgs e) => SolicitudCerrar?.Invoke(this, EventArgs.Empty);

        private void dtpCalendario_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = true; // Bloquea cualquier tecla de texto
        }

        private void dtpCalendario_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Permite Tab para navegación, bloquea todo lo demás (Backspace, Delete, números, etc.)
            if (e.Key != Key.Tab)
                e.Handled = true;
        }
        private void txtBuscar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.Text == tb.Tag as string)
            {
                tb.Text = "";
                tb.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void txtBuscar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = tb.Tag as string;
                tb.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}