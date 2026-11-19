using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TRAMADE.ClasesInventario;
using TRAMADE.Formularios_Login__Menú;
using TRAMADE.Services;

namespace TRAMADE
{
    public partial class InventarioControl : UserControl
    {
        // ── Servicio de negocio (única puerta de entrada a las reglas de Inventario) ──
        // Se puede inyectar por constructor para pruebas; si no se provee, usa la
        // implementación real (misma idea que ya usan los Services de Facturación).
        private readonly IProductoService _productoService;

        // ── Estado de paginación ─────────────────────────────────────────────
        private int _paginaActual = 1;
        private int _registrosPorPagina = 20;
        private int _totalPaginas = 1;
        private string _filtroActual = "ORDER BY ID ASC";

        // ── Ítem seleccionado actualmente ────────────────────────────────────
        private clsProductoVM _seleccionado = null;

        // ── Evento hacia frmInventario (para abrir sub-forms modales) ────────
        public event Action RegistrarSolicitado;
        public event Action<int> EditarSolicitado;

        // ── Constructor ──────────────────────────────────────────────────────
        public InventarioControl() : this(new ProductoService()) { }

        public InventarioControl(IProductoService productoService)
        {
            InitializeComponent();
            _productoService = productoService;

            // Fecha de encabezado
            var dias = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            var meses = new[] { "enero","febrero","marzo","abril","mayo","junio",
                                  "julio","agosto","septiembre","octubre","noviembre","diciembre" };
            var hoy = DateTime.Today;
            txtFecha.Text = $"{dias[(int)hoy.DayOfWeek]} {hoy.Day} de {meses[hoy.Month - 1]}, {hoy.Year}";
        }

        // ── Carga inicial (llamar desde frmInventario.Load) ──────────────────
        public async System.Threading.Tasks.Task CargarAsync()
        {
            await System.Threading.Tasks.Task.Run(() => { /* pre-calentamiento si se necesita */ });
            AplicarPermisos();
            CargarProductos();
        }

        // ── Permisos ─────────────────────────────────────────────────────────
        private void AplicarPermisos()
        {
            if (clsPermisos.esEmpleado)
            {
                btnRegistrar.Visibility = Visibility.Collapsed;
                btnEditar.Visibility = Visibility.Collapsed;
                btnEliminar.Visibility = Visibility.Collapsed;
                btnExportarInventario.Visibility = Visibility.Collapsed;
            }
        }

        // ── Carga / recarga del grid ─────────────────────────────────────────
        private void CargarProductos()
        {
            var productos = _productoService.ObtenerProductosPaginados(
                _filtroActual, _paginaActual, _registrosPorPagina, out _totalPaginas);

            lblPagina.Text = $"Página {_paginaActual} de {_totalPaginas}";

            var lista = productos.Select(clsProductoVM.DesdeModel).ToList();
            dgInventario.ItemsSource = lista;

            LimpiarDetalle();

            if (lista.Count == 0)
                MessageBox.Show("No hay productos registrados.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // ── Selección en el DataGrid ─────────────────────────────────────────
        private void dgInventario_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _seleccionado = dgInventario.SelectedItem as clsProductoVM;
            bool hay = _seleccionado != null;

            btnEditar.IsEnabled = hay;
            btnEliminar.IsEnabled = hay;
            btnExportarTodasEtiquetas.IsEnabled = hay;

            if (hay) MostrarDetalle(_seleccionado);
            else LimpiarDetalle();
        }

        // ── Panel de detalle (imagen + nombre + stock) ───────────────────────
        private void MostrarDetalle(clsProductoVM vm)
        {
            txtNombreProducto.Text = vm.Nombre;
            txtStockProducto.Text = $"Stock: {vm.Stock} unidades";
            txtStockProducto.Foreground = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(vm.AlertaColor));

            if (vm.Imagen != null && vm.Imagen.Length > 0)
            {
                try
                {
                    using (var ms = new MemoryStream(vm.Imagen))
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = ms;
                        bmp.EndInit();
                        imgProducto.Source = bmp;
                    }
                }
                catch { imgProducto.Source = ImagenPorDefecto(); }
            }
            else
            {
                imgProducto.Source = ImagenPorDefecto();
            }
        }

        private void LimpiarDetalle()
        {
            txtNombreProducto.Text = "—";
            txtStockProducto.Text = string.Empty;
            imgProducto.Source = ImagenPorDefecto();
            btnEditar.IsEnabled = false;
            btnEliminar.IsEnabled = false;
            btnExportarTodasEtiquetas.IsEnabled = false;
        }

        private BitmapImage ImagenPorDefecto()
        {
            try
            {
                return new BitmapImage(
                    new Uri("pack://application:,,,/TRAMADE;component/Resources/photo_8924441.png"));
            }
            catch { return null; }
        }

        // ── Búsqueda ─────────────────────────────────────────────────────────
        private void btnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string texto = txtBuscar.Text.Trim();

            if (string.IsNullOrEmpty(texto))
            {
                CargarProductos();
                return;
            }

            var lista = _productoService.BuscarProductos(texto)
                .Select(clsProductoVM.DesdeModel)
                .ToList();

            dgInventario.ItemsSource = lista;
            LimpiarDetalle();

            if (lista.Count == 0)
                MessageBox.Show($"No se encontraron resultados para: {texto}",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // ── Refrescar ────────────────────────────────────────────────────────
        private void btnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cmbFiltrar.SelectedIndex = -1;
            _filtroActual = "ORDER BY ID ASC";
            _paginaActual = 1;
            CargarProductos();
        }

        // ── Filtro ComboBox ──────────────────────────────────────────────────
        private void cmbFiltrar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbFiltrar.SelectedItem == null) return;

            string seleccion = (cmbFiltrar.SelectedItem as ComboBoxItem)?.Content?.ToString();
            switch (seleccion)
            {
                case "Orden Alfabético": _filtroActual = "ORDER BY Nombre ASC"; break;
                case "Categoría": _filtroActual = "ORDER BY Categoría ASC"; break;
                case "Sucursal": _filtroActual = "ORDER BY Sucursal ASC"; break;
                case "Proveedor": _filtroActual = "ORDER BY Proveedor ASC"; break;
                case "Mayor Stock": _filtroActual = "ORDER BY Stock DESC"; break;
                case "Menor Stock": _filtroActual = "ORDER BY Stock ASC"; break;
            }
            _paginaActual = 1;
            CargarProductos();
        }

        // ── Paginación ───────────────────────────────────────────────────────
        private void btnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaActual < _totalPaginas)
            {
                _paginaActual++;
                CargarProductos();
            }
        }

        private void btnAtras_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaActual > 1)
            {
                _paginaActual--;
                CargarProductos();
            }
        }

        // ── CRUD ─────────────────────────────────────────────────────────────
        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            RegistrarSolicitado?.Invoke();
        }

        private void btnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccionado == null)
            {
                MessageBox.Show("Selecciona un producto para editar.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            EditarSolicitado?.Invoke(_seleccionado.ID);
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (_seleccionado == null)
            {
                MessageBox.Show("Selecciona un producto para eliminar.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                "¿Estás seguro que deseas eliminar este producto?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                // Toda la regla de "si tiene más de un proveedor, solo se
                // quita la relación; si no, se elimina el producto completo"
                // ahora vive en ProductoService.EliminarProducto — misma
                // lógica, solo que ya no está mezclada con SQL en la UI.
                var resultado = _productoService.EliminarProducto(_seleccionado.ID, _seleccionado.Proveedor);

                if (resultado == ResultadoEliminacionProducto.RelacionProveedorEliminada)
                    MessageBox.Show("Relación con proveedor eliminada correctamente.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Producto eliminado correctamente.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                CargarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Exportar ─────────────────────────────────────────────────────────
        private void btnExportarInventario_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "Inventario_" + DateTime.Now.ToString("dd-MM-yyyy")
            })
            {
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                try
                {
                    OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("TRAMADE");

                    DataTable dt = _productoService.ObtenerTodosParaExportar(_filtroActual);

                    if (dt.Columns.Contains("imagen_producto"))
                        dt.Columns.Remove("imagen_producto");

                    using (var excel = new OfficeOpenXml.ExcelPackage())
                    {
                        var hoja = excel.Workbook.Worksheets.Add("Inventario");

                        hoja.Cells[1, 1].Value = "Fecha de exportación: " +
                            DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                        hoja.Cells[1, 1].Style.Font.Bold = true;
                        hoja.Cells[1, 1, 1, dt.Columns.Count].Merge = true;

                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            hoja.Cells[2, i + 1].Value = dt.Columns[i].ColumnName;
                            hoja.Cells[2, i + 1].Style.Font.Bold = true;
                            hoja.Cells[2, i + 1].Style.Fill.PatternType =
                                OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            hoja.Cells[2, i + 1].Style.Fill.BackgroundColor
                                .SetColor(System.Drawing.Color.Brown);
                            hoja.Cells[2, i + 1].Style.Font.Color
                                .SetColor(System.Drawing.Color.White);
                        }

                        for (int i = 0; i < dt.Rows.Count; i++)
                            for (int j = 0; j < dt.Columns.Count; j++)
                                hoja.Cells[i + 3, j + 1].Value = dt.Rows[i][j];

                        hoja.Cells.AutoFitColumns();
                        excel.SaveAs(new System.IO.FileInfo(dlg.FileName));
                    }

                    MessageBox.Show("Inventario exportado correctamente.",
                        "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnExportarTodasEtiquetas_Click(object sender, RoutedEventArgs e)
        {
            // clsCodigoBarras se mantiene como utilidad de impresión (genera el PDF);
            // internamente ya no ejecuta SQL directo, delega en CodigoBarrasRepository.
            clsCodigoBarras.ExportarTodasEtiquetas();
        }
    }
}
