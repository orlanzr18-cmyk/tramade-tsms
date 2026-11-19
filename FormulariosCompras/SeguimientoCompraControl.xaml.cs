using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TRAMADE.Models.ModelsCompras;
using TRAMADE.Services.ServCompras;

namespace TRAMADE.FormulariosCompras
{
    public partial class SeguimientoCompraControl : Window
    {
        private readonly ICompraService _compraService;
        private readonly List<string> _sugerenciasCompras = new List<string>();
        private DataTable _tabla;
        private bool _seleccionandoSugerencia;

        public SeguimientoCompraControl() : this(new CompraService())
        {
        }

        public SeguimientoCompraControl(ICompraService compraService)
        {
            if (compraService == null) throw new ArgumentNullException("compraService");
            _compraService = compraService;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DpDesde.SelectedDate = DateTime.Today;
            DpHasta.SelectedDate = DateTime.Today;
            CargarIndicadores();
            CargarCompras(false);
        }

        private void CargarIndicadores()
        {
            IndicadoresCompraModel indicadores = _compraService.ObtenerIndicadores();
            TxtSolicitudes.Text = indicadores.SolicitudesRegistradas;
            TxtValor.Text = "L " + indicadores.ValorTotalPendiente;
            TxtTiempo.Text = indicadores.TiempoRespuestaPromedio + " días";
        }

        private void CargarCompras(bool filtrar)
        {
            try
            {
                if (filtrar)
                {
                    int id;
                    int? idCompra = int.TryParse(TxtId.Text, out id) ? (int?)id : null;
                    _tabla = _compraService.ObtenerComprasSeguimiento(
                        idCompra,
                        (DpDesde.SelectedDate ?? DateTime.Today).Date,
                        (DpHasta.SelectedDate ?? DateTime.Today).Date);
                }
                else
                {
                    _tabla = _compraService.ObtenerCompras();
                }

                PrepararRecepcion();
                ConfigurarColumnas();
                DgSeguimiento.ItemsSource = _tabla.DefaultView;
                if (!filtrar)
                    ActualizarSugerenciasCompras();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo cargar el seguimiento.\n" + ex.Message,
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrepararRecepcion()
        {
            if (!_tabla.Columns.Contains("RecepcionTexto"))
                _tabla.Columns.Add("RecepcionTexto", typeof(string));
            if (!_tabla.Columns.Contains("PuedeRecibir"))
                _tabla.Columns.Add("PuedeRecibir", typeof(bool));

            foreach (DataRow fila in _tabla.Rows)
            {
                string estado = _tabla.Columns.Contains("Estado")
                    ? Convert.ToString(fila["Estado"])
                    : string.Empty;
                bool autorizada = estado.Equals("AUTORIZADA", StringComparison.OrdinalIgnoreCase);
                fila["RecepcionTexto"] = autorizada ? "Marcar recibido" : "-";
                fila["PuedeRecibir"] = autorizada;
            }
        }

        private void ConfigurarColumnas()
        {
            DgSeguimiento.Columns.Clear();
            AgregarTexto("ID\ncompra", "ID compra", AnchoProporcional(0.65), 70);
            AgregarTexto("Proveedor", "Proveedor", AnchoProporcional(1.4), 115);
            AgregarTexto("Nombre\nproducto", "Nombre producto", AnchoProporcional(1.55), 125);
            AgregarTexto("Precio\ncosto", "Precio costo", AnchoProporcional(0.8), 78, "N2");
            AgregarTexto("Cantidad", "Cantidad", AnchoProporcional(0.65), 72);
            AgregarTexto("Forma\npago", "Forma pago", AnchoProporcional(1.05), 90);
            AgregarCheck("Autorizada", "Autorizada", AnchoProporcional(0.8), 90);
            AgregarTexto("Estado", "Estado", AnchoProporcional(0.9), 88);
            AgregarTexto("Fecha\npedido", "Fecha pedido", AnchoProporcional(1.3), 112,
                "dd/MM/yyyy HH:mm:ss");
            AgregarTexto("Fecha\nentrega", "Fecha entrega", AnchoProporcional(1.3), 112,
                "dd/MM/yyyy HH:mm:ss");
            AgregarBotonRecepcion();
        }

        private static DataGridLength AnchoProporcional(double valor)
        {
            return new DataGridLength(valor, DataGridLengthUnitType.Star);
        }

        private void AgregarTexto(string titulo, string campo, DataGridLength ancho,
            double anchoMinimo, string formato = null)
        {
            DgSeguimiento.Columns.Add(new DataGridTextColumn
            {
                Header = titulo,
                Width = ancho,
                MinWidth = anchoMinimo,
                Binding = new Binding("[" + campo + "]") { StringFormat = formato }
            });
        }

        private void AgregarCheck(string titulo, string campo, DataGridLength ancho,
            double anchoMinimo)
        {
            DgSeguimiento.Columns.Add(new DataGridCheckBoxColumn
            {
                Header = titulo,
                Width = ancho,
                MinWidth = anchoMinimo,
                IsThreeState = false,
                Binding = new Binding("[" + campo + "]")
            });
        }

        private void AgregarBotonRecepcion()
        {
            FrameworkElementFactory boton = new FrameworkElementFactory(typeof(Button));
            boton.SetBinding(Button.ContentProperty, new Binding("[RecepcionTexto]"));
            boton.SetBinding(Button.IsEnabledProperty, new Binding("[PuedeRecibir]"));
            boton.SetValue(FrameworkElement.StyleProperty, FindResource("GridActionButton"));
            boton.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            boton.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            boton.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnRecibirFila_Click));

            DgSeguimiento.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Recepción",
                Width = AnchoProporcional(1.25),
                MinWidth = 120,
                CellTemplate = new DataTemplate { VisualTree = boton }
            });
        }

        private void BtnRecibirFila_Click(object sender, RoutedEventArgs e)
        {
            Button boton = sender as Button;
            DataRowView fila = boton == null ? null : boton.DataContext as DataRowView;
            if (fila == null || !_tabla.Columns.Contains("ID compra")) return;

            string estado = _tabla.Columns.Contains("Estado") ? Convert.ToString(fila["Estado"]) : string.Empty;
            if (!estado.Equals("AUTORIZADA", StringComparison.OrdinalIgnoreCase)) return;

            int idCompra = Convert.ToInt32(fila["ID compra"]);
            string producto = _tabla.Columns.Contains("Nombre producto")
                ? Convert.ToString(fila["Nombre producto"])
                : string.Empty;

            MessageBoxResult confirmar = MessageBox.Show(
                "¿Confirmas que la compra #" + idCompra + " fue recibida?\n\nProducto: " +
                producto + "\n\nEl stock se actualizará automáticamente.",
                "Confirmar recepción", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmar != MessageBoxResult.Yes) return;

            try
            {
                if (_compraService.RecibirCompra(
                    idCompra, clsSesion.id_usuario, clsSesion.id_sucursal))
                {
                MessageBox.Show("Compra recibida y stock actualizado correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CargarIndicadores();
                    CargarCompras(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al recibir la compra: " + ex.Message,
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtId.Text))
            {
                int id;
                if (!int.TryParse(TxtId.Text, out id) || id <= 0)
                {
                    MessageBox.Show("Solo se permiten números enteros positivos", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else if (!ValidarFechasFiltro())
            {
                return;
            }

            CargarCompras(true);
        }

        private void ActualizarSugerenciasCompras()
        {
            _sugerenciasCompras.Clear();
            if (_tabla == null || !_tabla.Columns.Contains("ID compra")) return;

            HashSet<int> ids = new HashSet<int>();
            foreach (DataRow fila in _tabla.Rows)
            {
                if (fila["ID compra"] == DBNull.Value) continue;
                int id = Convert.ToInt32(fila["ID compra"]);
                if (!ids.Add(id)) continue;

                string proveedor = _tabla.Columns.Contains("Proveedor")
                    ? Convert.ToString(fila["Proveedor"])
                    : string.Empty;
                string producto = _tabla.Columns.Contains("Nombre producto")
                    ? Convert.ToString(fila["Nombre producto"])
                    : string.Empty;
                _sugerenciasCompras.Add(id + " - " + proveedor + " - " + producto);
            }
        }

        private void TxtId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_seleccionandoSugerencia) return;

            string texto = TxtId.Text.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                PopupCompras.IsOpen = false;
                return;
            }

            List<string> filtradas = new List<string>();
            foreach (string sugerencia in _sugerenciasCompras)
            {
                if (sugerencia.IndexOf(texto, StringComparison.CurrentCultureIgnoreCase) < 0)
                    continue;
                filtradas.Add(sugerencia);
                if (filtradas.Count == 8) break;
            }

            LstCompras.ItemsSource = filtradas;
            PopupCompras.IsOpen = filtradas.Count > 0;
        }

        private void LstCompras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstCompras.SelectedItem == null) return;

            _seleccionandoSugerencia = true;
            string seleccion = Convert.ToString(LstCompras.SelectedItem);
            int separador = seleccion.IndexOf(" - ", StringComparison.Ordinal);
            TxtId.Text = separador >= 0 ? seleccion.Substring(0, separador) : seleccion;
            TxtId.CaretIndex = TxtId.Text.Length;
            PopupCompras.IsOpen = false;
            LstCompras.SelectedItem = null;
            _seleccionandoSugerencia = false;

            BtnBuscar_Click(sender, e);
        }

        private void BtnRecargar_Click(object sender, RoutedEventArgs e)
        {
            TxtId.Clear();
            DpDesde.SelectedDate = DateTime.Today;
            DpHasta.SelectedDate = DateTime.Today;
            CargarIndicadores();
            CargarCompras(false);
        }

        private bool ValidarFechasFiltro()
        {
            DateTime desde = (DpDesde.SelectedDate ?? DateTime.Today).Date;
            DateTime hasta = (DpHasta.SelectedDate ?? DateTime.Today).Date;

            if (desde > DateTime.Today)
            {
                MessageBox.Show("La fecha de inicio del filtro no puede ser mayor a la fecha actual",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (hasta > DateTime.Today)
            {
                MessageBox.Show("La fecha 'Hasta' no puede ser una fecha futura.",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (hasta < desde)
            {
                MessageBox.Show("La fecha final del filtro no puede ser menor a la fecha inicial",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
