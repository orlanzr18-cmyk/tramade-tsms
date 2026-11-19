using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TRAMADE.Models.ModelsCompras;
using TRAMADE.Services.ServCompras;

namespace TRAMADE.FormulariosCompras
{
    public partial class RegistrarCompraControl : Window
    {
        private const decimal Iva = 0.15m;
        private readonly ICompraService _compraService;
        private readonly DataTable _detalle = new DataTable();

        public RegistrarCompraControl() : this(new CompraService())
        {
        }

        public RegistrarCompraControl(ICompraService compraService)
        {
            if (compraService == null) throw new ArgumentNullException("compraService");
            _compraService = compraService;
            InitializeComponent();
            CrearDetalle();
            ConfigurarGrid();
            DgDetalle.ItemsSource = _detalle.DefaultView;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DpEntrega.SelectedDate = DateTime.Today;
            CargarCombos();
        }

        private void CrearDetalle()
        {
            _detalle.Columns.Add("idProducto", typeof(int));
            _detalle.Columns.Add("nombreProducto", typeof(string));
            _detalle.Columns.Add("cantidad", typeof(int));
            _detalle.Columns.Add("precioCosto", typeof(decimal));
            _detalle.Columns.Add("subtotal", typeof(decimal), "cantidad * precioCosto");
        }

        private void ConfigurarGrid()
        {
            DgDetalle.Columns.Clear();
            AgregarColumna("Producto", "nombreProducto", new DataGridLength(1, DataGridLengthUnitType.Star));
            AgregarColumna("Cantidad", "cantidad", 100);
            AgregarColumna("Precio costo", "precioCosto", 130, "N2");
            AgregarColumna("Subtotal", "subtotal", 130, "N2");
        }

        private void AgregarColumna(string titulo, string campo, DataGridLength ancho, string formato = null)
        {
            DgDetalle.Columns.Add(new DataGridTextColumn
            {
                Header = titulo,
                Width = ancho,
                Binding = new Binding("[" + campo + "]") { StringFormat = formato }
            });
        }

        private void CargarCombos()
        {
            CargarCombo(CmbProveedor, _compraService.ObtenerProveedores(),
                "nombre_comercial_proveedor", "id_proveedor");
            CargarCombo(CmbFormaPago, _compraService.ObtenerFormasPago(),
                "descripcion_forma_pago", "id_forma_pago");
            CargarCombo(CmbProducto, _compraService.ObtenerProductosPorSucursal(clsSesion.id_sucursal),
                "nombre_producto", "id_producto");
        }

        private static void CargarCombo(
            ComboBox combo, DataTable tabla, string mostrar, string valor)
        {
            combo.ItemsSource = tabla.DefaultView;
            combo.SelectedValuePath = valor;

            if (combo.IsEditable)
            {
                combo.DisplayMemberPath = mostrar;
            }
            else
            {
                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(TextBlock));
                factory.SetBinding(TextBlock.TextProperty, new Binding("[" + mostrar + "]"));
                combo.ItemTemplate = new DataTemplate { VisualTree = factory };
            }

            combo.SelectedIndex = -1;

            if (combo.IsEditable)
                ConfigurarBusquedaCombo(combo, tabla.DefaultView, mostrar);
        }

        private static void ConfigurarBusquedaCombo(
            ComboBox combo, DataView vista, string campoMostrar)
        {
            combo.IsTextSearchEnabled = false;
            combo.StaysOpenOnEdit = true;
            combo.ApplyTemplate();

            TextBox editor = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
            if (editor == null) return;

            bool actualizandoFiltro = false;
            editor.TextChanged += delegate
            {
                if (actualizandoFiltro) return;

                string texto = editor.Text;
                string criterio = texto.Trim();
                int posicionCursor = editor.CaretIndex;
                DataRowView seleccionada = combo.SelectedItem as DataRowView;
                if (seleccionada != null &&
                    string.Equals(criterio, Convert.ToString(seleccionada[campoMostrar]),
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    vista.RowFilter = string.Empty;
                    return;
                }

                actualizandoFiltro = true;
                try
                {
                    vista.RowFilter = string.IsNullOrWhiteSpace(criterio)
                        ? string.Empty
                        : "[" + campoMostrar + "] LIKE '%" + EscaparFiltro(criterio) + "%'";
                    combo.IsDropDownOpen = !string.IsNullOrWhiteSpace(criterio) && vista.Count > 0;

                    editor.Text = texto;
                    editor.CaretIndex = Math.Min(posicionCursor, editor.Text.Length);
                }
                finally
                {
                    actualizandoFiltro = false;
                }
            };
        }

        private static string EscaparFiltro(string texto)
        {
            return texto.Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("*", "[*]");
        }

        private void CmbProducto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView fila = CmbProducto.SelectedItem as DataRowView;
            TxtPrecio.Text = fila == null || !fila.Row.Table.Columns.Contains("precio_costo")
                ? "0.00"
                : Convert.ToDecimal(fila["precio_costo"]).ToString("N2");
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            DataRowView producto = CmbProducto.SelectedItem as DataRowView;
            int cantidad = ObtenerCantidad();
            decimal precio;
            if (producto == null || cantidad <= 0 ||
                !decimal.TryParse(TxtPrecio.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out precio))
            {
                MessageBox.Show("Seleccione un producto e ingrese una cantidad válida.",
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int idProducto = Convert.ToInt32(CmbProducto.SelectedValue);
            if (_detalle.Select("idProducto = " + idProducto).Length > 0)
            {
                MessageBox.Show("Este producto ya fue agregado.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _detalle.Rows.Add(idProducto, producto["nombre_producto"], cantidad, precio);
            ActualizarTotales();
            LimpiarProductoActual();
        }

        private void BtnQuitar_Click(object sender, RoutedEventArgs e)
        {
            DataRowView fila = DgDetalle.SelectedItem as DataRowView;
            if (fila == null)
            {
                MessageBox.Show("Seleccione un producto para eliminar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            fila.Row.Delete();
            _detalle.AcceptChanges();
            ActualizarTotales();
            LimpiarProductoActual();
        }

        private void LimpiarProductoActual()
        {
            DgDetalle.SelectedIndex = -1;
            CmbProducto.SelectedIndex = -1;
            CmbProducto.Text = string.Empty;
            TxtPrecio.Text = "0.00";
            SetCantidad(1);
        }

        private void DgDetalle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView fila = DgDetalle.SelectedItem as DataRowView;
            if (fila == null) return;

            TxtPrecio.Text = Convert.ToDecimal(fila["precioCosto"]).ToString("N2");
            SetCantidad(Convert.ToInt32(fila["cantidad"]));
        }

        private void ActualizarTotales()
        {
            decimal subtotal = 0m;
            foreach (DataRow fila in _detalle.Rows)
                subtotal += Convert.ToDecimal(fila["subtotal"]);
            TxtSubtotal.Text = "L " + subtotal.ToString("N2");
            TxtImpuesto.Text = "L " + (subtotal * Iva).ToString("N2");
            TxtTotal.Text = "L " + (subtotal * (1 + Iva)).ToString("N2");
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarRegistro()) return;

            try
            {
                _compraService.RegistrarCompra(CrearSolicitud());
                MessageBox.Show("¡COMPRA REGISTRADA EXITOSAMENTE!", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Limpiar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo registrar la compra.\n" + ex.Message,
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarRegistro()
        {
            if (CmbProveedor.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar una opción en 'Proveedor'.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CmbFormaPago.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar una opción en 'Forma de pago'.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CmbProveedor.SelectedValue == null)
            {
                MessageBox.Show("No se encontró ningún proveedor con ese criterio de búsqueda.",
                    "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CmbFormaPago.SelectedValue == null)
            {
                MessageBox.Show("Por favor, llene todos los campos vacíos", "Campos incompletos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!DpEntrega.SelectedDate.HasValue || DpEntrega.SelectedDate.Value.Date < DateTime.Today)
            {
                MessageBox.Show("La fecha de entrega no puede ser menor a la fecha actual",
                    "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (_detalle.Rows.Count == 0)
            {
                MessageBox.Show("Debe de agregar al menos un producto", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private DateTime ObtenerFechaEntregaConHoraActual()
        {
            return DpEntrega.SelectedDate.Value.Date.Add(DateTime.Now.TimeOfDay);
        }

        private CompraSolicitudModel CrearSolicitud()
        {
            CompraSolicitudModel compra = new CompraSolicitudModel
            {
                IdProveedor = Convert.ToInt32(CmbProveedor.SelectedValue),
                IdFormaPago = Convert.ToInt32(CmbFormaPago.SelectedValue),
                IdUsuario = clsSesion.id_usuario,
                FechaEntrega = ObtenerFechaEntregaConHoraActual()
            };

            foreach (DataRow fila in _detalle.Rows)
            {
                compra.Detalles.Add(new CompraDetalleModel
                {
                    IdProducto = Convert.ToInt32(fila["idProducto"]),
                    NombreProducto = Convert.ToString(fila["nombreProducto"]),
                    Cantidad = Convert.ToInt32(fila["cantidad"]),
                    PrecioCosto = Convert.ToDecimal(fila["precioCosto"])
                });
            }

            return compra;
        }

        private void Limpiar()
        {
            CmbProveedor.SelectedIndex = -1;
            CmbFormaPago.SelectedIndex = -1;
            LimpiarProductoActual();
            DpEntrega.SelectedDate = DateTime.Today;
            _detalle.Rows.Clear();
            ActualizarTotales();
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e) { Limpiar(); }

        private int ObtenerCantidad()
        {
            return Convert.ToInt32(NudCantidad.Value);
        }

        private void SetCantidad(int cantidad)
        {
            NudCantidad.Value = Math.Max((decimal)NudCantidad.Minimum,
                Math.Min((decimal)NudCantidad.Maximum, cantidad));
        }
    }
}
