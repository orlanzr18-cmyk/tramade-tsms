using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TRAMADE.ClasesDeudores;

namespace TRAMADE.FormulariosDeudor
{
    // ── Modelo de fila ────────────────────────────────────────────────────────
    public class DeudorRow
    {
        public int ID { get; set; }
        public string Cliente { get; set; }
        public string DNI { get; set; }
        public string RTN { get; set; }
        public string Telefono { get; set; }
        public string DeudaBase { get; set; }
        public string Mora { get; set; }
        public string DeudaTotal { get; set; }
        public string TotalPagado { get; set; }
        public string SaldoPendiente { get; set; }
        public string Estado { get; set; }
    }

    public partial class ucDeudor : UserControl
    {
        public event EventHandler SolicitudCerrar;

        private readonly clsConexion _conexion = new clsConexion();
        private readonly clsDeudor _objDeudor = new clsDeudor();

        private readonly ObservableCollection<DeudorRow> _filas
            = new ObservableCollection<DeudorRow>();

        public ucDeudor()
        {
            InitializeComponent();
            dgvDeudor.ItemsSource = _filas;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RecargarDeudores();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Cargar / recargar lista
        // ═════════════════════════════════════════════════════════════════════
        private void RecargarDeudores(string filtro = "")
        {
            try
            {
                _filas.Clear();

                DataTable dt = string.IsNullOrWhiteSpace(filtro)
                    ? _objDeudor.obtenerDeudores(_conexion)
                    : _objDeudor.buscarDeudores(_conexion, filtro);

                foreach (DataRow fila in dt.Rows)
                {
                    _filas.Add(new DeudorRow
                    {
                        ID = Convert.ToInt32(fila["ID"]),
                        Cliente = fila["Cliente"]?.ToString() ?? "",
                        DNI = fila["DNI"]?.ToString() ?? "",
                        RTN = fila["RTN"]?.ToString() ?? "",
                        Telefono = fila["Teléfono"]?.ToString() ?? "",
                        DeudaBase = fila["Deuda base"]?.ToString() ?? "",
                        Mora = fila["Mora"]?.ToString() ?? "",
                        DeudaTotal = fila["Deuda total"]?.ToString() ?? "",
                        TotalPagado = fila["Total pagado"]?.ToString() ?? "",
                        SaldoPendiente = fila["Saldo pendiente"]?.ToString() ?? "",
                        Estado = fila["Estado"]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar deudores: " + ex.Message,
                    "TRAMADE", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Eventos
        // ═════════════════════════════════════════════════════════════════════
        private void btnRegistrarPago_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgvDeudor.SelectedItem is DeudorRow fila))
            {
                MessageBox.Show("Seleccione un deudor de la lista.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Extraer saldo numérico
            string saldoStr = fila.SaldoPendiente
                .Replace("L ", "").Replace(",", "").Trim();

            decimal saldo = decimal.TryParse(saldoStr, out decimal s) ? s : 0;

            var frmPago = new FrmAgregarPago(fila.ID, fila.Cliente, saldo);
            frmPago.ShowDialog();

            RecargarDeudores(ObtenerTextoBusqueda());
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgvDeudor == null) return;
            RecargarDeudores(ObtenerTextoBusqueda());
        }

        // ── Placeholder ───────────────────────────────────────────────────────
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

        private string ObtenerTextoBusqueda()
        {
            string texto = txtBuscar.Text.Trim();
            return texto == (txtBuscar.Tag as string) ? "" : texto;
        }
    }
}