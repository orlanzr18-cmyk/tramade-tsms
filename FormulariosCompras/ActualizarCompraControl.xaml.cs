using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TRAMADE.Models.ModelsCompras;
using TRAMADE.Services.ServCompras;

namespace TRAMADE.FormulariosCompras
{
    public partial class ActualizarCompraControl : Window
    {
        private const decimal Iva = 0.15m;
        private readonly ICompraService _compraService;
        private readonly DataTable _detalle = new DataTable();
        private readonly List<string> _sugerenciasCompras = new List<string>();
        private int _idCompra;
        private bool _seleccionandoSugerencia;

        public ActualizarCompraControl() : this(new CompraService())
        {
        }

        public ActualizarCompraControl(ICompraService compraService)
        {
            if (compraService == null) throw new ArgumentNullException("compraService");
            _compraService = compraService;
            InitializeComponent();
            CrearDetalle();
            ConfigurarGrid();
            DgDetalle.ItemsSource = _detalle.DefaultView;
            EstablecerEdicionHabilitada(false);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarCombo(CmbProveedor, _compraService.ObtenerProveedores(),
                "nombre_comercial_proveedor", "id_proveedor");
            CargarCombo(CmbFormaPago, _compraService.ObtenerFormasPago(),
                "descripcion_forma_pago", "id_forma_pago");
            CargarCombo(CmbProducto, _compraService.ObtenerProductosPorSucursal(clsSesion.id_sucursal),
                "nombre_producto", "id_producto");
            CargarSugerenciasCompras();
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
            DgDetalle.Columns.Add(new DataGridTextColumn
            {
                Header = "Producto", Width = new DataGridLength(1, DataGridLengthUnitType.Star),
                IsReadOnly = true, Binding = new Binding("[nombreProducto]")
            });
            DgDetalle.Columns.Add(new DataGridTextColumn
            {
                Header = "Cantidad", Width = 110, IsReadOnly = true,
                Binding = new Binding("[cantidad]")
            });
            DgDetalle.Columns.Add(new DataGridTextColumn
            {
                Header = "Precio costo", Width = 130, IsReadOnly = true,
                Binding = new Binding("[precioCosto]") { StringFormat = "N2" }
            });
            DgDetalle.Columns.Add(new DataGridTextColumn
            {
                Header = "Subtotal", Width = 130, IsReadOnly = true,
                Binding = new Binding("[subtotal]") { StringFormat = "N2" }
            });
        }

        private static void CargarCombo(
            ComboBox combo, DataTable tabla, string mostrar, string valor)
        {
            combo.ItemsSource = tabla.DefaultView;
            combo.DisplayMemberPath = mostrar;
            combo.SelectedValuePath = valor;
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

        private void CargarSugerenciasCompras()
        {
            try
            {
                DataTable compras = _compraService.ObtenerCompras();
                _sugerenciasCompras.Clear();
                HashSet<int> ids = new HashSet<int>();

                foreach (DataRow fila in compras.Rows)
                {
                    if (!compras.Columns.Contains("ID compra") || fila["ID compra"] == DBNull.Value)
                        continue;

                    int id = Convert.ToInt32(fila["ID compra"]);
                    if (!ids.Add(id)) continue;

                    string proveedor = compras.Columns.Contains("Proveedor")
                        ? Convert.ToString(fila["Proveedor"])
                        : string.Empty;
                    string producto = compras.Columns.Contains("Nombre producto")
                        ? Convert.ToString(fila["Nombre producto"])
                        : string.Empty;
                    _sugerenciasCompras.Add(id + " - " + proveedor + " - " + producto);
                }
            }
            catch
            {
                _sugerenciasCompras.Clear();
            }
        }

        private void TxtIdCompra_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_seleccionandoSugerencia) return;

            if (_idCompra > 0 &&
                !string.Equals(TxtIdCompra.Text.Trim(), _idCompra.ToString(),
                    StringComparison.Ordinal))
            {
                _idCompra = 0;
                EstablecerEdicionHabilitada(false);
            }

            MostrarSugerencias(TxtIdCompra.Text);
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
            TxtIdCompra.Text = separador >= 0 ? seleccion.Substring(0, separador) : seleccion;
            TxtIdCompra.CaretIndex = TxtIdCompra.Text.Length;
            PopupCompras.IsOpen = false;
            LstCompras.SelectedItem = null;
            _seleccionandoSugerencia = false;

            BtnBuscar_Click(sender, e);
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            EstablecerEdicionHabilitada(false);
            int id;
            if (!int.TryParse(TxtIdCompra.Text, out id) || id <= 0)
            {
                MessageBox.Show(string.IsNullOrWhiteSpace(TxtIdCompra.Text)
                        ? "Ingrese el id primero antes de buscar"
                        : "Solo se permiten números enteros positivos",
                    "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DataTable tabla = _compraService.ObtenerCompraPorId(id);
                if (tabla.Rows.Count == 0)
                {
                    _idCompra = 0;
                    MessageBox.Show("Solicitud de compra no encontrada", "Aviso",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DataRow cabecera = tabla.Rows[0];
                _idCompra = id;
                SeleccionarSiExiste(CmbProveedor, cabecera, "id_proveedor");
                SeleccionarSiExiste(CmbFormaPago, cabecera, "id_forma_pago");
                if (tabla.Columns.Contains("fecha_entrega") && cabecera["fecha_entrega"] != DBNull.Value)
                    DpEntrega.SelectedDate = Convert.ToDateTime(cabecera["fecha_entrega"]);

                _detalle.Rows.Clear();
                foreach (DataRow fila in tabla.Rows)
                {
                    _detalle.Rows.Add(fila["id_producto"], fila["nombre_producto"],
                        fila["cantidad"], fila["precio_costo"]);
                }
                ActualizarTotales();
                EstablecerEdicionHabilitada(true);
            }
            catch (Exception ex)
            {
                _idCompra = 0;
                EstablecerEdicionHabilitada(false);
                MessageBox.Show("No se pudo cargar la compra.\n" + ex.Message,
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void SeleccionarSiExiste(ComboBox combo, DataRow fila, string columna)
        {
            if (fila.Table.Columns.Contains(columna) && fila[columna] != DBNull.Value)
                combo.SelectedValue = Convert.ToInt32(fila[columna]);
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
            DataRow[] existentes = _detalle.Select("idProducto = " + idProducto);
            if (existentes.Length > 0)
            {
                MessageBox.Show("Este producto ya fue agregado.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _detalle.Rows.Add(idProducto, producto["nombre_producto"], cantidad, precio);
            ActualizarTotales();
            SetCantidad(1);
            CmbProducto.SelectedIndex = -1;
            CmbProducto.Text = string.Empty;
            TxtPrecio.Text = "0.00";
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
        }

        private void DgDetalle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView fila = DgDetalle.SelectedItem as DataRowView;
            if (fila == null) return;

            TxtPrecio.Text = Convert.ToDecimal(fila["precioCosto"]).ToString("N2");
            SetCantidad(Convert.ToInt32(fila["cantidad"]));
        }

        private void BtnModificarCantidad_Click(object sender, RoutedEventArgs e)
        {
            DataRowView fila = DgDetalle.SelectedItem as DataRowView;
            int cantidad = ObtenerCantidad();
            if (fila == null || cantidad <= 0)
            {
                MessageBox.Show("Seleccione un producto e ingrese una cantidad válida.",
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            fila["cantidad"] = cantidad;
            ActualizarTotales();
            MessageBox.Show("Cantidad actualizada correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DgDetalle_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(ActualizarTotales));
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

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarActualizacion()) return;

            if (_idCompra == 0 || CmbProveedor.SelectedValue == null ||
                CmbFormaPago.SelectedValue == null || !DpEntrega.SelectedDate.HasValue ||
                _detalle.Rows.Count == 0)
            {
                MessageBox.Show("Primero cargue una compra y complete todos los datos.",
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _compraService.ActualizarCompra(CrearSolicitud());
                MessageBox.Show("¡COMPRA ACTUALIZADA EXITOSAMENTE!", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Limpiar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo actualizar la compra.\n" + ex.Message,
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarActualizacion()
        {
            if (_idCompra == 0)
            {
                MessageBox.Show("Ingrese una solicitud de compra primero", "ERROR",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (CmbProveedor.SelectedIndex < 0)
            {
                MessageBox.Show("Debe seleccionar una opción en 'Proveedor'.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CmbFormaPago.SelectedIndex < 0 || CmbFormaPago.SelectedValue == null)
            {
                MessageBox.Show("Por favor, llene todos los campos vacíos", "Campos incompletos",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (CmbProveedor.SelectedValue == null)
            {
                MessageBox.Show("No se encontró ningún proveedor con ese criterio de búsqueda.",
                    "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                IdCompra = _idCompra,
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
            _idCompra = 0;
            TxtIdCompra.Clear();
            CmbProveedor.SelectedIndex = -1;
            CmbFormaPago.SelectedIndex = -1;
            CmbProducto.SelectedIndex = -1;
            DpEntrega.SelectedDate = null;
            SetCantidad(1);
            TxtPrecio.Text = "0.00";
            _detalle.Rows.Clear();
            ActualizarTotales();
            EstablecerEdicionHabilitada(false);
        }

        private void EstablecerEdicionHabilitada(bool habilitada)
        {
            PanelDatosCompra.IsEnabled = habilitada;
            PanelAgregarProducto.IsEnabled = habilitada;
            PanelCantidad.IsEnabled = habilitada;
            PanelDetalle.IsEnabled = habilitada;
            PanelTotales.IsEnabled = habilitada;
            PanelAcciones.IsEnabled = habilitada;
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
