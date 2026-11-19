using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using TRAMADE.Models;
using TRAMADE.Services;

namespace TRAMADE
{
    public partial class wndAggProducto : UserControl
    {
        // --- SERVICIOS (Auditoría TS-51-H38) ---
        private readonly clsProductoService _productoService = new clsProductoService();

        // --- CÁMARA Y ESCÁNER (Lógica técnica preservada) ---
        private FilterInfoCollection _camarasDisponibles;
        private VideoCaptureDevice _fuenteVideo;
        private readonly BarcodeReader _lectorCodigo = new BarcodeReader
        {
            AutoRotate = true,
            Options = new ZXing.Common.DecodingOptions
            {
                TryHarder = false,
                PossibleFormats = new[] { BarcodeFormat.CODE_128, BarcodeFormat.EAN_13, BarcodeFormat.CODE_39 }
            }
        };

        private DateTime _ultimoIntento = DateTime.MinValue;
        private const int INTERVALO_DECODE_MS = 200;
        private string _ultimoCodigo = string.Empty;

        // --- COLECCIONES Y EVENTOS ---
        public ObservableCollection<clsProductoUI> Productos { get; set; } = new ObservableCollection<clsProductoUI>();
        public event EventHandler<List<clsProductoUI>> ProductosSeleccionados;
        public event EventHandler OperacionCancelada;

        public wndAggProducto()
        {
            InitializeComponent();
            dgvProductos.ItemsSource = Productos;
            BuscarDispositivosVideo();
            CargarCatalogoProductos();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  CATÁLOGO DE PRODUCTOS (Refactorizado con Service)
        // ═════════════════════════════════════════════════════════════════════

        private void CargarCatalogoProductos(string busqueda = "")
        {
            Productos.Clear();

            // Hallazgo TS-51-H38: Ahora usamos el Servicio, no SQL directo
            var listaBackend = _productoService.ListarProductos(busqueda);

            foreach (var prod in listaBackend)
            {
                // Mapeamos del Modelo de Negocio al Modelo de UI
                Productos.Add(new clsProductoUI
                {
                    id_producto = prod.Id,
                    nombre_producto = prod.Nombre,
                    existencia_producto = prod.Stock,
                    precio_unitario = prod.PrecioUnitario,
                    CantidadSeleccionada = 0
                });
            }
        }

        private void txtBuscarProducto_TextChanged(object sender, TextChangedEventArgs e)
        {
            CargarCatalogoProductos(txtBuscarProducto.Text.Trim());
        }

        // ═════════════════════════════════════════════════════════════════════
        //  LÓGICA DEL ESCÁNER (Cámara y Procesamiento)
        // ═════════════════════════════════════════════════════════════════════

        private void BuscarDispositivosVideo()
        {
            _camarasDisponibles = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            cmbCamaras.Items.Clear();

            if (_camarasDisponibles.Count > 0)
            {
                foreach (FilterInfo camara in _camarasDisponibles) cmbCamaras.Items.Add(camara.Name);
                cmbCamaras.SelectedIndex = 0;
            }
            else
            {
                cmbCamaras.Items.Add("No se detectó cámara");
                btnActivarScanner.IsEnabled = false;
            }
        }

        private void btnActivarScanner_Click(object sender, RoutedEventArgs e)
        {
            if (_fuenteVideo != null && _fuenteVideo.IsRunning) ApagarCamara();
            else EncenderCamara();
        }

        private bool _deteniendoCamara = false;

        private void EncenderCamara()
        {
            if (_deteniendoCamara) return; // evita crear un dispositivo nuevo mientras el anterior sigue cerrando

            wfhCamara.Visibility = Visibility.Visible;
            _ultimoCodigo = string.Empty;
            _fuenteVideo = new VideoCaptureDevice(_camarasDisponibles[cmbCamaras.SelectedIndex].MonikerString);
            _fuenteVideo.NewFrame += CapturarNuevoFrame;
            _fuenteVideo.Start();
            lblEstadoScanner.Text = "🟢 Escaneando...";
            btnActivarScanner.Content = "📷 Detener Cámara";
        }

        private void CapturarNuevoFrame(object sender, NewFrameEventArgs eventArgs)
        {
            System.Drawing.Bitmap bmpPreview;
            System.Drawing.Bitmap bmpDecode = null;

            try { bmpPreview = (System.Drawing.Bitmap)eventArgs.Frame.Clone(); }
            catch { return; }

            // Actualizar preview en PictureBox
            if (picCamara.IsHandleCreated && !picCamara.IsDisposed)
            {
                picCamara.BeginInvoke(new Action(() => {
                    var anterior = picCamara.Image;
                    picCamara.Image = bmpPreview;
                    anterior?.Dispose();
                }));
            }
            else { bmpPreview.Dispose(); return; }

            // Throttle para el decodificador
            if ((DateTime.Now - _ultimoIntento).TotalMilliseconds < INTERVALO_DECODE_MS) return;
            _ultimoIntento = DateTime.Now;

            try { bmpDecode = (System.Drawing.Bitmap)eventArgs.Frame.Clone(); }
            catch { return; }

            Result resultado = null;
            try { resultado = _lectorCodigo.Decode(bmpDecode); }
            catch { }
            finally { bmpDecode.Dispose(); }

            if (resultado != null && resultado.Text != _ultimoCodigo)
            {
                _ultimoCodigo = resultado.Text;
                Dispatcher.BeginInvoke(new Action(() => {
                    txtBuscarProducto.Text = resultado.Text;
                    ApagarCamara();
                }));
            }
        }

        public void ApagarCamara()
        {
            if (_fuenteVideo != null && !_deteniendoCamara)
            {
                var dispositivoASerCerrado = _fuenteVideo; 
                dispositivoASerCerrado.NewFrame -= CapturarNuevoFrame;
                _fuenteVideo = null;
                _deteniendoCamara = true;
                btnActivarScanner.IsEnabled = false;

                System.Threading.Tasks.Task.Run(() =>
                {
                    try { dispositivoASerCerrado.SignalToStop(); dispositivoASerCerrado.WaitForStop(); }
                    catch { }
                    finally
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _deteniendoCamara = false;
                            btnActivarScanner.IsEnabled = true;
                        }));
                    }
                });
            }
            wfhCamara.Visibility = Visibility.Collapsed;
            lblEstadoScanner.Text = "⭕ Escáner apagado";
            btnActivarScanner.Content = "📷 Activar Escáner";
        }

        // ═════════════════════════════════════════════════════════════════════
        //  BOTONES Y ACCIONES
        // ═════════════════════════════════════════════════════════════════════

        private void btnSeleccionar_Click(object sender, RoutedEventArgs e)
        {
            var seleccionados = Productos.Where(p => p.CantidadSeleccionada > 0).ToList();

            if (!seleccionados.Any())
            {
                MessageBox.Show("Ingrese una cantidad mayor a 0.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ApagarCamara();
            ProductosSeleccionados?.Invoke(this, seleccionados);
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            ApagarCamara();
            OperacionCancelada?.Invoke(this, EventArgs.Empty);
        }

        private void btnLimpiarBusqueda_Click(object sender, RoutedEventArgs e) => txtBuscarProducto.Text = string.Empty;
    }

    // Modelo ligero para la UI (Mantiene la funcionalidad de selección)
    public class clsProductoUI
    {
        public int id_producto { get; set; }
        public string nombre_producto { get; set; }
        public int existencia_producto { get; set; }
        public decimal precio_unitario { get; set; }
        public int CantidadSeleccionada { get; set; }
    }
}