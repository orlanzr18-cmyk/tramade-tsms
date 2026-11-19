using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TRAMADE.ClasesInventario;
using TRAMADE.Formularios_Login__Menú;
using TRAMADE.Services;

namespace TRAMADE
{
    public partial class RegistrarInvControl : UserControl
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly IProveedorService _proveedorService;

        private byte[] _imagenBytes = null;

        public event Action Cerrar;

        public RegistrarInvControl() : this(new ProductoService(), new CategoriaService(), new ProveedorService()) { }

        public RegistrarInvControl(IProductoService productoService, ICategoriaService categoriaService, IProveedorService proveedorService)
        {
            InitializeComponent();
            _productoService = productoService;
            _categoriaService = categoriaService;
            _proveedorService = proveedorService;
            this.Loaded += RegistrarInvControl_Loaded;
        }

        private void RegistrarInvControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarProveedores();
            CargarCategorias();
            txtSucursal.Text = $"Sucursal: {clsSesion.nombre_sucursal}";
        }

        // ── Carga de combos ─────────────────────────────────────────────────
        private void CargarProveedores()
        {
            foreach (var p in _proveedorService.ListarProveedores())
                cmbProveedor.Items.Add(p.Nombre);
        }

        private void CargarCategorias()
        {
            foreach (var c in _categoriaService.ListarCategorias())
                cmbCategoria.Items.Add(c.Nombre);
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

        // ── Registrar ───────────────────────────────────────────────────────
        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (!Validar()) return;

            try
            {
                decimal precio = decimal.Parse(txtPrecio.Text.Trim(), CultureInfo.InvariantCulture);
                decimal costo = decimal.Parse(txtPrecioCosto.Text.Trim(), CultureInfo.InvariantCulture);
                int stock = int.Parse(txtStockInicial.Text.Trim());
                int idSucursal = clsSesion.id_sucursal;

                // Toda la orquestación (insertar producto, generar código,
                // vincular proveedor y sucursal) ahora vive en el Service,
                // en una sola transacción con una sola conexión.
                var (nuevoId, codigoBarras) = _productoService.RegistrarProducto(
                    txtNombreProducto.Text.Trim(),
                    cmbCategoria.SelectedItem.ToString(),
                    cmbProveedor.SelectedItem.ToString(),
                    precio, costo, stock, _imagenBytes, idSucursal);

                MessageBox.Show(
                    $"Producto registrado correctamente.\nCódigo generado: {codigoBarras}",
                    "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                var confirm = MessageBox.Show(
                    "¿Deseas imprimir la etiqueta ahora?",
                    "Imprimir etiqueta", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                    clsCodigoBarras.ImprimirEtiqueta(txtNombreProducto.Text.Trim(), codigoBarras, precio);

                Cerrar?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Cancelar ────────────────────────────────────────────────────────
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Cerrar?.Invoke();
        }

        // ── Validación (delegada al Service, mismos mensajes y mismo orden) ──
        private bool Validar()
        {
            bool ok = _productoService.ValidarRegistro(
                txtNombreProducto.Text,
                txtPrecio.Text,
                txtPrecioCosto.Text,
                txtStockInicial.Text,
                cmbProveedor.SelectedItem,
                cmbCategoria.SelectedItem,
                out string mensajeError);

            if (!ok) Aviso(mensajeError);
            return ok;
        }

        private void Aviso(string msg) =>
            MessageBox.Show(msg, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
