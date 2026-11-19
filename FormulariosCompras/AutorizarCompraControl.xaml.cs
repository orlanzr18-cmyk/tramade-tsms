using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TRAMADE.Services.ServCompras;

namespace TRAMADE.FormulariosCompras
{
    public partial class AutorizarCompraControl : Window
    {
        private readonly ICompraService _compraService;
        private readonly List<string> _sugerenciasCompras = new List<string>();
        private DataTable _compras;
        private bool _seleccionandoSugerencia;

        public AutorizarCompraControl() : this(new CompraService())
        {
        }

        public AutorizarCompraControl(ICompraService compraService)
        {
            if (compraService == null) throw new ArgumentNullException("compraService");
            _compraService = compraService;
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarCompras();
        }

        private void CargarCompras()
        {
            try
            {
                _compras = _compraService.ObtenerCompras();

                if (!_compras.Columns.Contains("Seleccionar"))
                    _compras.Columns.Add("Seleccionar", typeof(bool)).SetOrdinal(0);

                foreach (DataRow fila in _compras.Rows)
                    fila["Seleccionar"] = false;

                ConfigurarColumnas();
                DgCompras.ItemsSource = _compras.DefaultView;
                ActualizarSugerenciasCompras();
                AplicarFiltro();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudieron cargar las solicitudes.\n" + ex.Message,
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConfigurarColumnas()
        {
            DgCompras.Columns.Clear();
            FrameworkElementFactory check = new FrameworkElementFactory(typeof(CheckBox));
            check.SetValue(CheckBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            check.SetValue(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center);
            check.SetValue(CheckBox.IsThreeStateProperty, false);
            check.SetBinding(CheckBox.IsCheckedProperty, new Binding("[Seleccionar]")
            {
                Mode = BindingMode.TwoWay,
                TargetNullValue = false,
                FallbackValue = false,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            DgCompras.Columns.Add(new DataGridTemplateColumn
            {
                Header = "Seleccionar",
                Width = AnchoProporcional(0.85),
                MinWidth = 100,
                CellTemplate = new DataTemplate { VisualTree = check }
            });

            AgregarTexto("ID\ncompra", "ID compra", AnchoProporcional(0.65), 70);
            AgregarTexto("Proveedor", "Proveedor", AnchoProporcional(1.45), 115);
            AgregarTexto("Nombre\nproducto", "Nombre producto", AnchoProporcional(1.55), 125);
            AgregarTexto("Precio\ncosto", "Precio costo", AnchoProporcional(0.8), 78, "N2");
            AgregarTexto("Cantidad", "Cantidad", AnchoProporcional(0.65), 72);
            AgregarTexto("Forma\npago", "Forma pago", AnchoProporcional(1.1), 90);
            AgregarCheck("Autorizada", "Autorizada", AnchoProporcional(0.8), 90);
            AgregarTexto("Estado", "Estado", AnchoProporcional(0.95), 88);
            AgregarTexto("Fecha\npedido", "Fecha pedido", AnchoProporcional(1.35), 112,
                "dd/MM/yyyy HH:mm:ss");
            AgregarTexto("Fecha\nentrega", "Fecha entrega", AnchoProporcional(1.35), 112,
                "dd/MM/yyyy HH:mm:ss");
        }

        private static DataGridLength AnchoProporcional(double valor)
        {
            return new DataGridLength(valor, DataGridLengthUnitType.Star);
        }

        private void AgregarTexto(string titulo, string campo, DataGridLength ancho,
            double anchoMinimo, string formato = null)
        {
            DgCompras.Columns.Add(new DataGridTextColumn
            {
                Header = titulo,
                Width = ancho,
                MinWidth = anchoMinimo,
                IsReadOnly = true,
                Binding = new Binding("[" + campo + "]") { StringFormat = formato }
            });
        }

        private void AgregarCheck(string titulo, string campo, DataGridLength ancho,
            double anchoMinimo)
        {
            DgCompras.Columns.Add(new DataGridCheckBoxColumn
            {
                Header = titulo,
                Width = ancho,
                MinWidth = anchoMinimo,
                IsReadOnly = true,
                Binding = new Binding("[" + campo + "]")
            });
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_seleccionandoSugerencia)
                MostrarSugerencias(TxtBuscar.Text);
            AplicarFiltro();
        }

        private void ActualizarSugerenciasCompras()
        {
            _sugerenciasCompras.Clear();
            if (_compras == null || !_compras.Columns.Contains("ID compra")) return;

            HashSet<int> ids = new HashSet<int>();
            foreach (DataRow fila in _compras.Rows)
            {
                if (fila["ID compra"] == DBNull.Value) continue;
                int id = Convert.ToInt32(fila["ID compra"]);
                if (!ids.Add(id)) continue;

                string proveedor = _compras.Columns.Contains("Proveedor")
                    ? Convert.ToString(fila["Proveedor"])
                    : string.Empty;
                string producto = _compras.Columns.Contains("Nombre producto")
                    ? Convert.ToString(fila["Nombre producto"])
                    : string.Empty;
                _sugerenciasCompras.Add(id + " - " + proveedor + " - " + producto);
            }
        }

        private void MostrarSugerencias(string criterio)
        {
            string texto = (criterio ?? string.Empty).Trim();
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
            TxtBuscar.Text = separador >= 0 ? seleccion.Substring(0, separador) : seleccion;
            TxtBuscar.CaretIndex = TxtBuscar.Text.Length;
            PopupCompras.IsOpen = false;
            LstCompras.SelectedItem = null;
            _seleccionandoSugerencia = false;
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            if (_compras == null || !_compras.Columns.Contains("ID compra")) return;
            int id;
            _compras.DefaultView.RowFilter = int.TryParse(TxtBuscar.Text, out id)
                ? "[ID compra] = " + id
                : string.Empty;
        }

        private void EjecutarSeleccion(bool autorizar)
        {
            DgCompras.CommitEdit(DataGridEditingUnit.Cell, true);
            DgCompras.CommitEdit(DataGridEditingUnit.Row, true);

            if (_compras == null || !_compras.Columns.Contains("ID compra")) return;
            HashSet<int> ids = new HashSet<int>();
            foreach (DataRowView vista in _compras.DefaultView)
            {
                DataRow fila = vista.Row;
                if (fila["Seleccionar"] != DBNull.Value && Convert.ToBoolean(fila["Seleccionar"]))
                    ids.Add(Convert.ToInt32(fila["ID compra"]));
            }

            if (ids.Count == 0)
            {
                MessageBox.Show("Debe seleccionar al menos una fila", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int procesadas = 0;
            foreach (int id in ids)
            {
                try
                {
                    bool correcto = autorizar
                        ? _compraService.AutorizarCompra(id)
                        : _compraService.DenegarCompra(id);
                    if (correcto) procesadas++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        (autorizar ? "Error al autorizar la compra: " : "Error al denegar la compra: ") +
                        ex.Message,
                        "Compras", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            MessageBox.Show(autorizar
                    ? "Compras autorizadas correctamente."
                    : "Compras denegadas correctamente.",
                "Compras", MessageBoxButton.OK, MessageBoxImage.Information);
            CargarCompras();
        }

        private void BtnAutorizar_Click(object sender, RoutedEventArgs e)
        {
            EjecutarSeleccion(true);
        }

        private void BtnDenegar_Click(object sender, RoutedEventArgs e)
        {
            EjecutarSeleccion(false);
        }
    }
}
