using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TRAMADE.ClasesInventario;
using TRAMADE.Models;
using TRAMADE.Services;

namespace TRAMADE
{
    public partial class EditarInvControl : UserControl
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly IProveedorService _proveedorService;

        private readonly int _idProducto;
        private byte[] _imagenBytes = null;

        public event Action Cerrar;

        public EditarInvControl(int idProducto)
            : this(idProducto, new ProductoService(), new CategoriaService(), new ProveedorService()) { }

        public EditarInvControl(int idProducto, IProductoService productoService, ICategoriaService categoriaService, IProveedorService proveedorService)
        {
            InitializeComponent();
            _idProducto = idProducto;
            _productoService = productoService;
            _categoriaService = categoriaService;
            _proveedorService = proveedorService;
            this.Loaded += EditarInvControl_Loaded;
        }

        private void EditarInvControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarCategorias();
            CargarProveedores();
            CargarDatosProducto();
        }

        // ── Carga de combos ─────────────────────────────────────────────────
        private void CargarCategorias()
        {
            foreach (var c in _categoriaService.ListarCategorias())
                cmbCategoria.Items.Add(c.Nombre);
        }

        private void CargarProveedores()
        {
            foreach (var p in _proveedorService.ListarProveedores())
                cmbProveedorNuevo.Items.Add(p.Nombre);
        }

        // ── Cargar datos del producto ────────────────────────────────────────
        private void CargarDatosProducto()
        {
            clsProductoModel producto = _productoService.ObtenerProductoPorId(_idProducto);
            if (producto == null) return;

            AplicarDatosAFormulario(producto);
            CargarProveedoresDelProducto();
        }

        private void AplicarDatosAFormulario(clsProductoModel producto)
        {
            txtNombreProducto.Text = producto.Nombre;
            txtPrecio.Text = producto.PrecioUnitario.ToString();
            txtPrecioCosto.Text = producto.PrecioCosto.ToString();
            txtStock.Text = producto.Stock.ToString();

            foreach (var item in cmbCategoria.Items)
            {
                if (item.ToString() == producto.Categoria)
                {
                    cmbCategoria.SelectedItem = item;
                    break;
                }
            }

            if (producto.Imagen != null && producto.Imagen.Length > 0)
            {
                _imagenBytes = producto.Imagen;
                MostrarImagen(_imagenBytes);
            }
        }

        private void CargarProveedoresDelProducto()
        {
            lstProveedores.Items.Clear();
            foreach (var nombreProveedor in _productoService.ObtenerProveedoresDelProducto(_idProducto))
                lstProveedores.Items.Add(nombreProveedor);

            if (lstProveedores.Items.Count > 0)
                lstProveedores.SelectedIndex = 0;
        }

        // ── Selección en lista de proveedores ────────────────────────────────
        private void lstProveedores_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstProveedores.SelectedIndex == -1) return;

            string nombreProveedor = lstProveedores.SelectedItem.ToString();
            var producto = _productoService.ObtenerProductoPorIdYProveedor(_idProducto, nombreProveedor);
            if (producto == null) return;

            AplicarDatosAFormulario(producto);
        }

        // ── Agregar proveedor ────────────────────────────────────────────────
        private void btnAgregarProveedor_Click(object sender, RoutedEventArgs e)
        {
            if (cmbProveedorNuevo.SelectedItem == null)
            {
                Aviso("Selecciona un proveedor para agregar."); return;
            }

            string nombre = cmbProveedorNuevo.SelectedItem.ToString();

            if (lstProveedores.Items.Contains(nombre))
            {
                Aviso("Este proveedor ya está asignado al producto."); return;
            }

            // Antes este botón mandaba precio/costo/stock directo a la base de
            // datos sin validar nada — así fue como se colaron los negativos
            // que viste en el grid. Ahora pasa por la misma validación que el
            // resto del formulario.
            if (!_productoService.ValidarPrecioCostoStock(txtPrecio.Text, txtPrecioCosto.Text, txtStock.Text,
                    permitirStockCero: true, out string mensajeError))
            {
                Aviso(mensajeError);
                return;
            }

            try
            {
                _productoService.AgregarProveedorAProducto(
                    _idProducto, nombre,
                    ParseDecimal(txtPrecio.Text),
                    ParseDecimal(txtPrecioCosto.Text),
                    ParseInt(txtStock.Text),
                    clsSesion.id_sucursal);

                MessageBox.Show("Proveedor agregado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarProveedoresDelProducto();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Quitar proveedor ─────────────────────────────────────────────────
        private void btnQuitarProveedor_Click(object sender, RoutedEventArgs e)
        {
            if (lstProveedores.SelectedIndex == -1)
            {
                Aviso("Selecciona un proveedor para quitar."); return;
            }
            if (lstProveedores.Items.Count == 1)
            {
                Aviso("El producto debe tener al menos un proveedor."); return;
            }

            var confirm = MessageBox.Show(
                "¿Estás seguro que deseas quitar este proveedor?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            string nombre = lstProveedores.SelectedItem.ToString();
            try
            {
                _productoService.QuitarProveedorDeProducto(_idProducto, nombre);
                MessageBox.Show("Proveedor quitado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CargarProveedoresDelProducto();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Subir imagen ────────────────────────────────────────────────────
        private void btnSubir_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Seleccionar imagen del producto"
            })
            {
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                _imagenBytes = File.ReadAllBytes(dlg.FileName);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(dlg.FileName);
                bmp.EndInit();
                imgProducto.Source = bmp;
                pnlPlaceholderImg.Visibility = Visibility.Collapsed;
            }
        }

        // ── Confirmar edición ────────────────────────────────────────────────
        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            if (!Validar()) return;

            try
            {
                decimal precio = ParseDecimal(txtPrecio.Text);
                decimal costo = ParseDecimal(txtPrecioCosto.Text);
                int stock = ParseInt(txtStock.Text);
                int idSucursal = clsSesion.id_sucursal;

                string proveedorSeleccionado = lstProveedores.SelectedIndex != -1
                    ? lstProveedores.SelectedItem.ToString()
                    : null;

                // Misma secuencia que el original: actualizar producto,
                // actualizar/insertar en PRODUCTO_SUCURSAL según exista,
                // y actualizar PRODUCTO_PROVEEDOR del proveedor seleccionado.
                _productoService.ActualizarProducto(
                    _idProducto,
                    txtNombreProducto.Text.Trim(),
                    cmbCategoria.SelectedItem.ToString(),
                    precio, costo, stock, _imagenBytes, idSucursal,
                    proveedorSeleccionado);

                MessageBox.Show("Producto actualizado correctamente.",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                Cerrar?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Cancelar ────────────────────────────────────────────────────────
        private void btnCancelar_Click(object sender, RoutedEventArgs e) =>
            Cerrar?.Invoke();

        // ── Helpers ─────────────────────────────────────────────────────────
        private void MostrarImagen(byte[] bytes)
        {
            try
            {
                using (var ms = new MemoryStream(bytes))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    imgProducto.Source = bmp;
                    pnlPlaceholderImg.Visibility = Visibility.Collapsed;
                }
            }
            catch { /* imagen corrupta, dejar placeholder */ }
        }

        // ── Validación (delegada al Service, mismos mensajes y mismo orden) ──
        private bool Validar()
        {
            bool ok = _productoService.ValidarEdicion(
                txtNombreProducto.Text,
                txtPrecio.Text,
                txtPrecioCosto.Text,
                txtStock.Text,
                cmbCategoria.SelectedItem,
                lstProveedores.Items.Count,
                out string mensajeError);

            if (!ok) Aviso(mensajeError);
            return ok;
        }

        private void Aviso(string msg) =>
            MessageBox.Show(msg, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

        private decimal ParseDecimal(string s) =>
            decimal.Parse(s.Trim(), CultureInfo.InvariantCulture);

        private int ParseInt(string s) =>
            int.Parse(s.Trim());
    }
}
