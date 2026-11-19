using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32; // Para SaveFileDialog en WPF
using TRAMADE.Models;
using TRAMADE.Services;

namespace TRAMADE
{
    public partial class wndEmitirFactura : UserControl
    {
        public event EventHandler SolicitudCerrar;

        // --- SERVICIOS (Auditoría: Cumple SRP y DIP) ---
        private readonly clsFacturaService _facturaService = new clsFacturaService();
        private readonly clsPdfService _pdfService = new clsPdfService();

        // --- ESTADO ---
        public int IdFacturaRecibido { get; set; }
        private clsFacturaConsultaModel _facturaActual;

        public wndEmitirFactura()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IdFacturaRecibido > 0)
                CargarDatosFactura();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  CARGA DE DATOS (Solución al problema de campos vacíos)
        // ═════════════════════════════════════════════════════════════════════
        private void CargarDatosFactura()
        {
            try
            {
                // El servicio ahora resuelve automáticamente si el cliente es Ocasional o Registrado
                _facturaActual = _facturaService.CargarFacturaCompleta(IdFacturaRecibido);

                if (_facturaActual != null)
                {
                    // Datos de Cabecera
                    lblNumeroFacturaEF.Text = _facturaActual.NumeroFormateado;
                    txtDNIClienteEF.Text = _facturaActual.DniRtnCliente;
                    txtNombreClienteEF.Text = _facturaActual.NombreCliente;
                    txtDireccionClienteEF.Text = _facturaActual.DireccionCliente;

                    // Datos del Vendedor
                    txtIDVendedorEF.Text = _facturaActual.IdUsuario.ToString();
                    txtNombreVendedorEF.Text = _facturaActual.NombreUsuario;

                    // Fechas y Totales
                    lblFechaEmisionEF.Text = _facturaActual.FechaEmision.ToString("dd/MM/yyyy");
                    lblFechaVencimientoEF.Text = _facturaActual.FechaVencimiento.ToString("dd/MM/yyyy");
                    lblSubtotalEF.Text = "L. " + _facturaActual.Subtotal.ToString("N2");
                    lblImpuestoEF.Text = "L. " + _facturaActual.Impuesto.ToString("N2");
                    lblTotalEF.Text = "L. " + _facturaActual.Total.ToString("N2");

                    // Forma de Pago (Sincronizado con clsFacturaModel: 1=Crédito, 2=Contado)
                    if (_facturaActual.IdFormaPago == 1) rbCreditoEF.IsChecked = true;
                    else rbContadoEF.IsChecked = true;

                    // CARGA DEL DETALLE (Directo del Modelo)
                    dgvEmitirFactura.ItemsSource = _facturaActual.Detalles;
                }
                else
                {
                    MessageBox.Show("No se encontró la factura solicitada.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error crítico al cargar: " + ex.Message, "TRAMADE", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  GENERAR PDF (Usando el nuevo clsPdfService)
        // ═════════════════════════════════════════════════════════════════════
        private void btnDescargarPDF_Click(object sender, RoutedEventArgs e)
        {
            if (_facturaActual == null) return;

            try
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Documento PDF (*.pdf)|*.pdf",
                    FileName = $"Factura_{_facturaActual.NumeroFactura}"
                };

                if (sfd.ShowDialog() == true)
                {
                    // El servicio de PDF ahora usa el modelo real de consulta
                    _pdfService.GenerarDocumentoFactura(_facturaActual, sfd.FileName);
                    MessageBox.Show("PDF generado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar el PDF: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            SolicitudCerrar?.Invoke(this, EventArgs.Empty);
        }
    }
}