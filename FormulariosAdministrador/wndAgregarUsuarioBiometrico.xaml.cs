using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TRAMADE.ClasesMenu;

// Alias para evitar ambigüedad entre System.Drawing.Size y System.Windows.Size
using Size = System.Drawing.Size;

namespace TRAMADE.FormulariosAdministrador
{
    public partial class wndAgregarUsuarioBiometrico : UserControl
    {
        // ── Dependencias ──────────────────────────────────────────────────────
        private readonly clsConexion _conexion = new clsConexion();
        private readonly clsDBCon _dbc = new clsDBCon();

        // ── Cámara ────────────────────────────────────────────────────────────
        private VideoCapture _grabber;
        private CascadeClassifier _detector;
        private CancellationTokenSource _ctsCamara;

        // ── Estado ────────────────────────────────────────────────────────────
        private bool _rostroDetectado = false;
        private bool _capturandoAuto = false;
        private int _fotosCapturadas = 0;
        private const int TotalFotos = 20;
        private List<byte[]> _fotosEnMemoria = new List<byte[]>();

        // ═════════════════════════════════════════════════════════════════════
        //  Constructor
        // ═════════════════════════════════════════════════════════════════════
        public wndAgregarUsuarioBiometrico()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Loaded / Unloaded
        // ═════════════════════════════════════════════════════════════════════
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CargarUsuarios();
            CargarCamaras();
            IniciarCamara();

            btnCapturar.IsEnabled = false;
            btnGuardar.IsEnabled = false;
            SetEstado("Seleccione un usuario para comenzar", "#8a5c35");
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DetenerCamara();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Cargar usuarios — usa tabla USUARIO igual que clsDBCon
        // ═════════════════════════════════════════════════════════════════════
        private void CargarUsuarios(string filtro = "")
        {
            try
            {
                _conexion.Abrir();

                SqlCommand cmd;
                if (string.IsNullOrWhiteSpace(filtro))
                {
                    cmd = new SqlCommand(
                        "SELECT id_usuario, nombre_usuario FROM USUARIO WHERE id_estado = 1",
                        _conexion.SqlC);
                }
                else
                {
                    cmd = new SqlCommand(
                        "SELECT id_usuario, nombre_usuario FROM USUARIO " +
                        "WHERE id_estado = 1 AND nombre_usuario LIKE @texto",
                        _conexion.SqlC);
                    cmd.Parameters.AddWithValue("@texto", "%" + filtro.Trim() + "%");
                }

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmbUsuarios.ItemsSource = dt.DefaultView;
                cmbUsuarios.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar usuarios: " + ex.Message);
            }
            finally
            {
                _conexion.Cerrar();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Cargar cámaras disponibles
        // ═════════════════════════════════════════════════════════════════════
        private void CargarCamaras()
        {
            try
            {
                cmbCamaras.Items.Clear();
                FilterInfoCollection camaras =
                    new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (camaras.Count == 0)
                {
                    cmbCamaras.Items.Add("No se encontraron cámaras");
                    cmbCamaras.IsEnabled = false;
                    btnCapturar.IsEnabled = false;
                    return;
                }

                foreach (FilterInfo cam in camaras)
                    cmbCamaras.Items.Add(cam.Name);

                cmbCamaras.SelectedIndex = 0;
                cmbCamaras.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al detectar cámaras: " + ex.Message);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Iniciar cámara — loop en hilo de fondo para evitar
        //  AccessViolationException cuando EmguCV corre desde el hilo UI
        // ═════════════════════════════════════════════════════════════════════
        private void IniciarCamara()
        {
            try
            {
                string xmlPath = Path.Combine(
                    System.Windows.Forms.Application.StartupPath,
                    "haarcascade_frontalface_default.xml");

                if (!File.Exists(xmlPath))
                {
                    MessageBox.Show("No se encontró el clasificador: " + xmlPath);
                    return;
                }

                _detector = new CascadeClassifier(xmlPath);

                int indice = cmbCamaras.SelectedIndex >= 0 ? cmbCamaras.SelectedIndex : 0;
                _grabber = new VideoCapture(indice);

                if (!_grabber.IsOpened)
                {
                    MessageBox.Show("No se pudo abrir la cámara seleccionada.");
                    return;
                }

                // Lanzar loop en Task de fondo — nunca en hilo UI
                _ctsCamara = new CancellationTokenSource();
                var token = _ctsCamara.Token;

                Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        ProcesarFrameBackground(token);
                        Thread.Sleep(33); // ~30 fps
                    }
                }, token);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cámara no disponible: " + ex.Message);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Detener cámara
        // ═════════════════════════════════════════════════════════════════════
        private void DetenerCamara()
        {
            try
            {
                _ctsCamara?.Cancel();
                Thread.Sleep(120); // esperar que el loop termine
                _grabber?.Dispose();
                _grabber = null;
            }
            catch { }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Procesar frame en hilo de fondo
        // ═════════════════════════════════════════════════════════════════════
        private void ProcesarFrameBackground(CancellationToken token)
        {
            Mat frame = null;
            Mat gris = null;

            try
            {
                if (token.IsCancellationRequested) return;

                frame = new Mat();

                // Lock: solo un hilo lee la cámara a la vez
                lock (_grabber)
                {
                    _grabber.Read(frame);
                }

                if (frame == null || frame.IsEmpty) return;

                gris = new Mat();
                CvInvoke.CvtColor(frame, gris, ColorConversion.Bgr2Gray);
                CvInvoke.EqualizeHist(gris, gris);

                Rectangle[] rostros = _detector.DetectMultiScale(
                    gris, 1.2, 10, new Size(80, 80));

                bool hayRostro = rostros.Length > 0;

                // Dibujar rectángulos en el frame
                foreach (Rectangle r in rostros)
                    CvInvoke.Rectangle(frame, r, new MCvScalar(0, 255, 0), 2);

                // Captura automática en hilo de fondo; UI se actualiza vía Dispatcher
                if (_capturandoAuto && hayRostro && _fotosCapturadas < TotalFotos)
                {
                    Mat grisClone = gris.Clone();
                    Rectangle rectClone = rostros[0];
                    Dispatcher.Invoke(() => GuardarFoto(grisClone, rectClone));
                }

                // Convertir a BitmapSource ANTES de entrar al Dispatcher
                BitmapSource bmpSource = MatToBitmapSource(frame);

                // Actualizar UI en hilo principal
                Dispatcher.Invoke(() =>
                {
                    _rostroDetectado = hayRostro;
                    imgCamara.Source = bmpSource;

                    if (!_capturandoAuto)
                    {
                        if (hayRostro)
                        {
                            SetEstado("✔ Rostro detectado — presione Capturar", "#23891a");
                            btnCapturar.IsEnabled = cmbUsuarios.SelectedIndex != -1;
                        }
                        else
                        {
                            SetEstado("✘ No se detecta rostro", "#c0392b");
                            btnCapturar.IsEnabled = false;
                        }
                    }
                });
            }
            catch (OperationCanceledException) { }
            catch { /* ignorar errores de frame individuales */ }
            finally
            {
                gris?.Dispose();
                frame?.Dispose();
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Convertir Mat a BitmapSource para WPF
        // ═════════════════════════════════════════════════════════════════════
        private BitmapSource MatToBitmapSource(Mat mat)
        {
            try
            {
                using (var bmp = mat.ToImage<Bgr, byte>().ToBitmap())
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);

                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.StreamSource = ms;
                    bi.EndInit();
                    bi.Freeze(); // requerido para usar en otro hilo
                    return bi;
                }
            }
            catch { return null; }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Guardar foto en memoria (se llama desde Dispatcher.Invoke)
        // ═════════════════════════════════════════════════════════════════════
        private void GuardarFoto(Mat frameGris, Rectangle rectRostro)
        {
            try
            {
                Mat rostroRecortado = new Mat(frameGris, rectRostro);
                Mat rostroFinal = new Mat();

                CvInvoke.Resize(rostroRecortado, rostroFinal, new Size(100, 100));
                CvInvoke.EqualizeHist(rostroFinal, rostroFinal);

                byte[] imagenBytes;
                using (var ms = new MemoryStream())
                {
                    var imgGris = rostroFinal.ToImage<Gray, byte>();
                    imgGris.ToBitmap().Save(ms, ImageFormat.Bmp);
                    imagenBytes = ms.ToArray();
                    imgGris.Dispose();
                }

                rostroRecortado.Dispose();
                rostroFinal.Dispose();
                frameGris.Dispose();

                _fotosEnMemoria.Add(imagenBytes);
                _fotosCapturadas++;

                pbProgreso.Value = _fotosCapturadas;
                lblProgreso.Text = $"Fotos: {_fotosCapturadas} / {TotalFotos}";
                SetEstado($"📷 Capturando... {_fotosCapturadas}/{TotalFotos}", "#0f2c83");

                if (_fotosCapturadas >= TotalFotos)
                {
                    _capturandoAuto = false;
                    btnCapturar.IsEnabled = false;
                    btnGuardar.IsEnabled = true;
                    SetEstado($"✔ {TotalFotos} fotos listas — presione Guardar", "#23891a");
                }
            }
            catch (Exception ex)
            {
                _capturandoAuto = false;
                MessageBox.Show("Error al capturar foto: " + ex.Message);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Botón Capturar
        // ═════════════════════════════════════════════════════════════════════
        private void btnCapturar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbUsuarios.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un usuario primero.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_rostroDetectado)
            {
                MessageBox.Show("No se detecta rostro. Póngase frente a la cámara.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fila = (DataRowView)cmbUsuarios.SelectedItem;
            int idUsuario = Convert.ToInt32(fila["id_usuario"]);
            string nombre = fila["nombre_usuario"].ToString();

            bool yaTiene = _dbc.UsuarioTieneRostroRegistrado(idUsuario);
            if (yaTiene)
            {
                var res = MessageBox.Show(
                    $"⚠️ '{nombre}' ya tiene un rostro registrado.\n\n" +
                    "¿Desea sobreescribir el registro biométrico existente?",
                    "Rostro ya registrado",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (res == MessageBoxResult.No)
                {
                    SetEstado("Operación cancelada", "#8a5c35");
                    return;
                }

                _dbc.eliminarImagenesUsuario(idUsuario);
            }

            _fotosEnMemoria.Clear();
            _fotosCapturadas = 0;
            pbProgreso.Value = 0;
            lblProgreso.Text = $"Fotos: 0 / {TotalFotos}";
            _capturandoAuto = true;
            btnCapturar.IsEnabled = false;
            SetEstado("📷 Iniciando captura automática...", "#0f2c83");
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Botón Guardar
        // ═════════════════════════════════════════════════════════════════════
        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            var fila = (DataRowView)cmbUsuarios.SelectedItem;
            int idUsuario = Convert.ToInt32(fila["id_usuario"]);
            string nombre = fila["nombre_usuario"].ToString();

            foreach (byte[] img in _fotosEnMemoria)
                _dbc.insertarImagenFacial(idUsuario, img);

            _fotosEnMemoria.Clear();

            MessageBox.Show(
                $"Registro completado. Se guardaron {TotalFotos} fotos de '{nombre}'.",
                "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

            // Resetear estado
            _fotosCapturadas = 0;
            _capturandoAuto = false;
            pbProgreso.Value = 0;
            lblProgreso.Text = $"Fotos: 0 / {TotalFotos}";
            btnGuardar.IsEnabled = false;
            btnCapturar.IsEnabled = false;
            cmbUsuarios.SelectedIndex = -1;
            SetEstado("Seleccione un usuario para comenzar", "#8a5c35");
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Combo usuarios — SelectionChanged
        // ═════════════════════════════════════════════════════════════════════
        private void cmbUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbUsuarios.SelectedIndex != -1)
            {
                btnCapturar.IsEnabled = _rostroDetectado;
                btnGuardar.IsEnabled = false;
                SetEstado("Listo — póngase frente a la cámara y presione Capturar", "#8a5c35");
            }
            else
            {
                btnCapturar.IsEnabled = false;
                btnGuardar.IsEnabled = false;
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Combo cámaras — SelectionChanged
        // ═════════════════════════════════════════════════════════════════════
        private void cmbCamaras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCamaras.SelectedIndex < 0) return;
            DetenerCamara();
            IniciarCamara();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Helper: actualizar label de estado con color
        // ═════════════════════════════════════════════════════════════════════
        private void SetEstado(string texto, string colorHex)
        {
            lblEstado.Text = texto;
            lblEstado.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                    .ConvertFromString(colorHex));
        }
    }
}
