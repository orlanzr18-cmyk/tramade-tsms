using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TRAMADE.Models;
using TRAMADE.Repositories;
using TRAMADE.Services;

namespace TRAMADE
{
    // Modelo para las filas de la tabla de productos (Lógica de UI)
    public class DetalleFacturaRow : INotifyPropertyChanged
    {
        private string _col0, _col1, _col2, _col3, _col4, _col5;
        public string Col0 { get => _col0; set { _col0 = value; OnProp(); } } // ID Producto
        public string Col1 { get => _col1; set { _col1 = value; OnProp(); } } // Nombre
        public string Col2 { get => _col2; set { _col2 = value; OnProp(); } } // Descripción
        public string Col3 { get => _col3; set { _col3 = value; OnProp(); RecalcSubtotal(); } } // Cantidad
        public string Col4 { get => _col4; set { _col4 = value; OnProp(); RecalcSubtotal(); } } // Precio
        public string Col5 { get => _col5; set { _col5 = value; OnProp(); } } // Subtotal

        private void RecalcSubtotal()
        {
            decimal.TryParse(_col3, out decimal cant);
            decimal.TryParse(_col4, out decimal precio);
            Col5 = (cant * precio).ToString("N2");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnProp([System.Runtime.CompilerServices.CallerMemberName] string n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public partial class ucFacturacionControl : UserControl
    {
        public static ucFacturacionControl InstanciaActiva { get; private set; }

        // --- INSTANCIAS DE SERVICIOS (Arquitectura por Capas) ---
        private readonly clsFacturaService _facturaService = new clsFacturaService();
        private readonly clsClienteService _clienteService = new clsClienteService();
        private readonly ClienteRepository _clienteRepo = new ClienteRepository();

        // --- VARIABLES DE ESTADO ---
        private List<string> _clienteSugerencias = new List<string>();
        private bool _seleccionandoSugerencia = false;
        private clsClienteModel _clienteSeleccionado;
        private int numeroFactura = 1;
        private int id_cliente_seleccionado = 0;

        private readonly ObservableCollection<DetalleFacturaRow> _detalleRows = new ObservableCollection<DetalleFacturaRow>();
        public ObservableCollection<DetalleFacturaRow> DetalleRows => _detalleRows;

        public ucFacturacionControl()
        {
            InitializeComponent();
            InstanciaActiva = this;
            dgvDetalleFactura.ItemsSource = _detalleRows;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GenerarCorrelativo();
            CargarVendedorAuto();
            CargarAutoCompleteClientes();
            dtpEmision.SelectedDate = DateTime.Now;
            dtpVencimiento.SelectedDate = DateTime.Now.AddMonths(1);

            dtpEmision.IsEnabled = false;
            dtpVencimiento.IsEnabled = false;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  CÁLCULOS MATEMÁTICOS (CORRECCIÓN: Sincronización de Totales)
        // ═════════════════════════════════════════════════════════════════════

        public void CalcularTotales()
        {
            // 1. Suma base de todos los productos en la tabla
            decimal subtotalBase = _detalleRows.Sum(r => decimal.TryParse(r.Col5, out decimal v) ? v : 0);

            // 2. Aplicar recargo del 10% si es crédito
            decimal subtotalFinal = (rbCredito.IsChecked == true) ? subtotalBase * 1.10m : subtotalBase;

            // 3. Calcular ISV (15%) sobre el subtotal ya procesado
            decimal impuesto = subtotalFinal * 0.15m;

            // 4. Total final
            decimal total = subtotalFinal + impuesto;

            // Actualizar Labels en UI
            lblSubtotal.Text = "L. " + subtotalFinal.ToString("N2");
            lblImpuesto.Text = "L. " + impuesto.ToString("N2");
            lblTotal.Text = "L. " + total.ToString("N2");
        }

        private void rbFormaPago_Checked(object sender, RoutedEventArgs e)
        {
            CalcularTotales(); // Forzar recalculación al cambiar contado/crédito
        }

        // ═════════════════════════════════════════════════════════════════════
        //  BÚSQUEDA Y FILTRADO (Hallazgo TS-51-H31)
        // ═════════════════════════════════════════════════════════════════════

        private void txtDNICliente_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (popupSugerencias == null || _seleccionandoSugerencia) return;
            string texto = txtDNICliente.Text.Trim();
            string placeholder = txtDNICliente.Tag as string;

            if (string.IsNullOrWhiteSpace(texto) || texto == placeholder)
            {
                popupSugerencias.IsOpen = false;
                return;
            }

            var filtradas = _clienteSugerencias
                .Where(s => s.IndexOf(texto, StringComparison.OrdinalIgnoreCase) >= 0)
                .Take(8).ToList();

            if (filtradas.Any())
            {
                lstSugerencias.ItemsSource = filtradas;
                popupSugerencias.IsOpen = true;
            }
            else { popupSugerencias.IsOpen = false; }
        }

        // ── FIX: Se captura el clic ANTES de que el Popup se cierre solo ──
        // (StaysOpen="False" cierra el Popup al perder foco el TextBox, lo
        // que a veces "come" el SelectionChanged del ListBox y da la
        // sensación de que la sugerencia está "bloqueada"/no responde).
        private void lstSugerencias_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(lstSugerencias, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item == null) return;

            string sel = item.Content?.ToString();
            if (string.IsNullOrEmpty(sel)) return;

            // Evita que el clic se pierda con el cierre automático del Popup
            e.Handled = true;

            SeleccionarSugerencia(sel);
        }

        // Se conserva como respaldo (teclado: flechas + Enter también dispara SelectionChanged)
        private void lstSugerencias_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_seleccionandoSugerencia) return; // ya procesado por el clic
            if (lstSugerencias.SelectedItem == null) return;

            string sel = lstSugerencias.SelectedItem.ToString();
            SeleccionarSugerencia(sel);
        }

        private void SeleccionarSugerencia(string sel)
        {
            _seleccionandoSugerencia = true;
            try
            {
                string doc = sel.Contains(" — ") ? sel.Split(new[] { " — " }, StringSplitOptions.None)[0].Trim() : sel;
                txtDNICliente.Text = doc;
                popupSugerencias.IsOpen = false;
                BuscarCliente(doc);
            }
            finally
            {
                _seleccionandoSugerencia = false;
            }
        }

        private void BuscarCliente(string rtnDni)
        {
            try
            {
                _clienteSeleccionado = _clienteService.BuscarCliente(rtnDni);
                if (_clienteSeleccionado != null)
                {
                    id_cliente_seleccionado = _clienteSeleccionado.Id;
                    txtNombreCliente.Text = _clienteSeleccionado.Nombre;
                    txtDireccionCliente.Text = _clienteSeleccionado.Direccion;
                    txtNombreCliente.IsReadOnly = txtDireccionCliente.IsReadOnly = txtDNICliente.IsReadOnly = true;
                }
                else
                {
                    LimpiarDatosCliente();
                    MessageBox.Show("Cliente no registrado. Se habilitarán los campos para Cliente Ocasional.");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void LimpiarDatosCliente()
        {
            id_cliente_seleccionado = 0; _clienteSeleccionado = null;
            txtNombreCliente.Text = ""; txtDireccionCliente.Text = "";
            txtNombreCliente.IsReadOnly = txtDireccionCliente.IsReadOnly = txtDNICliente.IsReadOnly = false;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  EMISIÓN DE FACTURA (Hallazgo TS-51-H33)
        // ═════════════════════════════════════════════════════════════════════

        private void btnEmitirFactura_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos()) return;
            if (MessageBox.Show("¿Está seguro de emitir y guardar esta factura?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                var factura = new clsFacturaModel
                {
                    NumeroFactura = numeroFactura,
                    IdUsuario = int.Parse(txtIDVendedor.Text),
                    IdFormaPago = rbCredito.IsChecked == true ? 1 : 2,
                    FechaEmision = dtpEmision.SelectedDate ?? DateTime.Now,
                    FechaVencimiento = dtpVencimiento.SelectedDate ?? DateTime.Now,
                    Subtotal = ParseLempiras(lblSubtotal.Text),
                    Impuesto = ParseLempiras(lblImpuesto.Text),
                    Total = ParseLempiras(lblTotal.Text),
                    EsOcasional = (id_cliente_seleccionado == 0)
                };

                if (factura.EsOcasional)
                {
                    factura.NombreOcasional = ObtenerTextoReal(txtNombreCliente, "( ! ) Nombre del cliente");
                    factura.DniOcasional = ObtenerTextoReal(txtDNICliente, "Buscar por DNI o RTN");
                    factura.DireccionOcasional = ObtenerTextoReal(txtDireccionCliente, "( ! ) Dirección del cliente");
                }
                else { factura.IdCliente = id_cliente_seleccionado; }

                foreach (var fila in _detalleRows)
                {
                    factura.Detalles.Add(new clsFacturaDetalleModel
                    {
                        IdProducto = int.Parse(fila.Col0),
                        Cantidad = decimal.Parse(fila.Col3)
                    });
                }

                int idGenerado = _facturaService.ProcesarVenta(factura);

                if (idGenerado > 0)
                {
                    MessageBox.Show("Factura emitida con éxito.", "TRAMADE");
                    var wndEmitir = new frmEmitirFactura { IdFacturaRecibido = idGenerado };
                    wndEmitir.ShowDialog();
                    LimpiarFormulario();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error al emitir: " + ex.Message); }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  INTERFAZ Y HELPERS
        // ═════════════════════════════════════════════════════════════════════

        private bool ValidarCampos()
        {
            string nombre = ObtenerTextoReal(txtNombreCliente, "( ! ) Nombre del cliente").Trim();
            if (string.IsNullOrWhiteSpace(nombre)) { MessageBox.Show("El nombre es obligatorio."); return false; }
            if (rbContado.IsChecked != true && rbCredito.IsChecked != true) { MessageBox.Show("Seleccione forma de pago."); return false; }
            if (_detalleRows.Count == 0) { MessageBox.Show("Agregue productos."); return false; }
            if (rbCredito.IsChecked == true && id_cliente_seleccionado == 0) { MessageBox.Show("Venta al crédito requiere cliente registrado."); return false; }
            if (rbContado.IsChecked == true && id_cliente_seleccionado > 0) { MessageBox.Show("Clientes registrados solo compran al crédito."); return false; }
            return true;
        }

        private void LimpiarFormulario()
        {
            id_cliente_seleccionado = 0;
            _clienteSeleccionado = null;
            _detalleRows.Clear();
            rbContado.IsChecked = false;
            rbCredito.IsChecked = false;

            // Aseguramos que el buscador quede totalmente operativo
            _seleccionandoSugerencia = false;
            if (popupSugerencias != null) popupSugerencias.IsOpen = false;

            RestaurarPlaceholder(txtDNICliente, "Buscar por DNI o RTN");
            RestaurarPlaceholder(txtNombreCliente, "( ! ) Nombre del cliente");
            RestaurarPlaceholder(txtDireccionCliente, "( ! ) Dirección del cliente");

            // Desbloqueo explícito y unificado
            txtNombreCliente.IsReadOnly = false;
            txtDireccionCliente.IsReadOnly = false;
            txtDNICliente.IsReadOnly = false;

            CalcularTotales();
            GenerarCorrelativo();
        }

        public void AgregarProductoAlDetalle(string id, string nombre, string descripcion, int cantidad, decimal precio)
        {
            var existente = _detalleRows.FirstOrDefault(r => r.Col0 == id);
            if (existente != null)
            {
                int.TryParse(existente.Col3, out int cantAct);
                existente.Col3 = (cantAct + cantidad).ToString();
            }
            else
            {
                _detalleRows.Add(new DetalleFacturaRow { Col0 = id, Col1 = nombre, Col2 = descripcion, Col3 = cantidad.ToString(), Col4 = precio.ToString("N2"), Col5 = (precio * cantidad).ToString("N2") });
            }
            CalcularTotales();
        }

        private void btnVistaPrevia_Click(object sender, RoutedEventArgs e)
        {
            if (_detalleRows.Count == 0)
            {
                MessageBox.Show("Agregue productos para ver la vista previa.");
                return;
            }

            // 1. ENSAMBLAJE (Ocurre aquí o en un servicio, no en la ventana de destino)
            var facturaParaVista = new clsFacturaConsultaModel
            {
                NumeroFactura = numeroFactura,
                FechaEmision = dtpEmision.SelectedDate ?? DateTime.Now,
                FechaVencimiento = dtpVencimiento.SelectedDate ?? DateTime.Now,
                DniRtnCliente = ObtenerTextoReal(txtDNICliente, "Buscar por DNI o RTN"),
                NombreCliente = ObtenerTextoReal(txtNombreCliente, "( ! ) Nombre del cliente"),
                DireccionCliente = ObtenerTextoReal(txtDireccionCliente, "( ! ) Dirección del cliente"),
                IdUsuario = int.Parse(txtIDVendedor.Text),
                NombreUsuario = txtNombreVendedor.Text,
                Subtotal = ParseLempiras(lblSubtotal.Text),
                Impuesto = ParseLempiras(lblImpuesto.Text),
                Total = ParseLempiras(lblTotal.Text),
                IdFormaPago = rbCredito.IsChecked == true ? 1 : 2
            };

            foreach (var fila in _detalleRows)
            {
                facturaParaVista.Detalles.Add(new clsFacturaDetalleConsultaModel
                {
                    Producto = fila.Col1,
                    Descripcion = fila.Col2,
                    Cantidad = decimal.Parse(fila.Col3),
                    PrecioUnitario = decimal.Parse(fila.Col4),
                    Subtotal = decimal.Parse(fila.Col5)
                });
            }

            // 2. LLAMADO LIMPIO (Hallazgo TS-51-H49)
            var vpControl = new wndVistaPrevia();
            vpControl.CargarDatos(facturaParaVista);

            var frm = new frmVistaPrevia(vpControl);
            frm.ShowDialog();
        }

        private void GenerarCorrelativo() { try { numeroFactura = _facturaService.ObtenerNuevoNumero(); txtNumeroFactura.Text = $"INV/2026/{numeroFactura:D4}"; } catch { } }
        private void CargarVendedorAuto() { if (clsSesion.id_usuario != 0) { txtIDVendedor.Text = clsSesion.id_usuario.ToString(); txtNombreVendedor.Text = clsSesion.nombre_usuario; txtIDVendedor.IsReadOnly = txtNombreVendedor.IsReadOnly = true; } }
        private void CargarAutoCompleteClientes() { try { _clienteSugerencias = _clienteRepo.ObtenerSugerenciasAutoComplete(); } catch { } }

        // Se llama cada vez que el formulario contenedor (frmFacturacion) vuelve a
        // activarse. Como ahora frmFacturacion se cachea y se reutiliza (ver fix del
        // memory leak), el evento Loaded de este UserControl solo dispara una vez,
        // así que sin esto la lista de sugerencias de DNI/RTN queda desactualizada
        // después de registrar un cliente nuevo.
        public void RefrescarAutoCompleteClientes() => CargarAutoCompleteClientes();
        private decimal ParseLempiras(string t) => decimal.TryParse(t.Replace("L.", "").Replace(",", "").Trim(), out decimal v) ? v : 0;
        private string ObtenerTextoReal(TextBox tb, string p) => tb.Text == p ? "" : tb.Text;
        private void RestaurarPlaceholder(TextBox tb, string p) { if (!string.IsNullOrEmpty(p)) tb.Text = p; }
        private void PlaceholderGotFocus(object sender, RoutedEventArgs e) { if (sender is TextBox tb && tb.Text == tb.Tag as string) tb.Text = ""; }
        private void PlaceholderLostFocus(object sender, RoutedEventArgs e) { if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text)) RestaurarPlaceholder(tb, tb.Tag as string); }
        private void btnNuevo_Click(object sender, RoutedEventArgs e) => LimpiarFormulario();
        private void btnLimpiar_Click(object sender, RoutedEventArgs e) => LimpiarFormulario();
        private void btnIDVendedor_Click(object sender, RoutedEventArgs e) => CargarVendedorAuto();
        private void btnVerFacturas_Click(object sender, RoutedEventArgs e) => new frmHistorialFacturas().ShowDialog();
        private void btnAggProducto_Click(object sender, RoutedEventArgs e) { InstanciaActiva = this; new frmAggProducto().ShowDialog(); }
        private void btnEliminarProducto_Click(object sender, RoutedEventArgs e) { if (dgvDetalleFactura.SelectedItem is DetalleFacturaRow r) { _detalleRows.Remove(r); CalcularTotales(); } }
        private void dgvDetalleFactura_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => Dispatcher.InvokeAsync(() => CalcularTotales());
        private void dtpEmision_SelectedDateChanged(object sender, SelectionChangedEventArgs e) { if (dtpEmision.SelectedDate.HasValue) dtpVencimiento.SelectedDate = dtpEmision.SelectedDate.Value.AddMonths(1); }
    }
}