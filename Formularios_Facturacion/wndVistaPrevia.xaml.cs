using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TRAMADE.Models; // Importante para usar los modelos refactorizados

namespace TRAMADE
{
    public partial class wndVistaPrevia : UserControl
    {
        // Hallazgo TS-51-H50: Ahora usamos el modelo de consulta para los detalles, desacoplando la UI del modelo de datos
        private readonly ObservableCollection<clsFacturaDetalleConsultaModel> _detalles
            = new ObservableCollection<clsFacturaDetalleConsultaModel>();

        public event EventHandler SolicitudCerrar;

        public wndVistaPrevia()
        {
            InitializeComponent();
            dgvFacturaVP.ItemsSource = _detalles;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  CARGA DE DATOS (Hallazgo TS-51-H49 y H51)
        // ═════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Recibe un único objeto de factura ya ensamblado y validado.
        /// Cumple con la Responsabilidad Única (SRP) al solo mostrar información.
        /// </summary>
        public void CargarDatos(clsFacturaConsultaModel factura)
        {
            if (factura == null) return;

            // Datos Maestros
            lblNumeroFacturaVP.Text = factura.NumeroFormateado;
            lblFechaEmisionVP.Text = factura.FechaEmision.ToString("dd/MM/yyyy");
            lblfechaVencimientoVP.Text = factura.FechaVencimiento.ToString("dd/MM/yyyy");

            // Datos del Cliente
            txtDNIClienteVP.Text = factura.DniRtnCliente;
            txtNombreClienteVP.Text = factura.NombreCliente;
            txtDireccionClienteVP.Text = factura.DireccionCliente;

            // Datos del Vendedor
            txtIDVendedorVP.Text = factura.IdUsuario.ToString();
            txtNombreVendedorVP.Text = factura.NombreUsuario;

            // Totales
            lblSubtotalVP.Text = "L. " + factura.Subtotal.ToString("N2");
            lblImpuestoVP.Text = "L. " + factura.Impuesto.ToString("N2");
            lblTotalVP.Text = "L. " + factura.Total.ToString("N2");

            // Forma de Pago
            rbCreditoVP.IsChecked = (factura.IdFormaPago == 1);
            rbContadoVP.IsChecked = (factura.IdFormaPago != 1);

            // Cargar Detalles (Hallazgo TS-51-H50)
            _detalles.Clear();
            foreach (var item in factura.Detalles)
            {
                _detalles.Add(item);
            }
        }

        private void btnCerrarVP_Click(object sender, RoutedEventArgs e)
        {
            SolicitudCerrar?.Invoke(this, EventArgs.Empty);
        }
    }
}