using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace TRAMADE
{
    /// <summary>
    /// Sidebar WPF con animación de expansión/contracción.
    /// Se comunica con frmMenuPrincipal a través de eventos.
    /// </summary>
    public partial class SidebarControl : UserControl
    {
        // ── Constantes de ancho ──────────────────────────────────────────────
        private const double ANCHO_EXPANDIDO = 220;
        private const double ANCHO_CONTRAIDO = 56;  
        private const double DURACION_MS = 180;

        private bool _expandido = true;

        // ── Eventos que escucha frmMenuPrincipal ─────────────────────────────

        /// <summary>Se dispara cuando el usuario hace clic en un ítem del menú.</summary>
        public event Action<string> NavegacionSolicitada;

        /// <summary>Se dispara cuando el usuario confirma cerrar sesión.</summary>
        public event Action CerrarSesionSolicitada;

        // ── Constructor ───────────────────────────────────────────────────────
        public SidebarControl()
        {
            InitializeComponent();
        }

        // ── Permisos (llamar desde frmMenuPrincipal después de crear el control) ──
        public void OcultarReportes() => btnReportes.Visibility = Visibility.Collapsed;
        public void OcultarUsuarios() => btnUsuarios.Visibility = Visibility.Collapsed;
        public void OcultarBitacora() => btnBitacora.Visibility = Visibility.Collapsed;

        public void SetUsuario(string nombre)
        {
            txtBienvenido.Text = "Bienvenido, " + nombre;
        }

        // ── Nombre de usuario en el topbar ────────────────────────────────────
        // (el topbar sigue en WinForms; este control solo es el sidebar)

        // ── Toggle hamburguesa ────────────────────────────────────────────────
        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            AnimarSidebar(!_expandido);
        }

        public event Action<double> AnchoSidebarCambiado;

        private void AnimarSidebar(bool expandir)
        {
            double destino = expandir ? ANCHO_EXPANDIDO : ANCHO_CONTRAIDO;

            AnchoSidebarCambiado?.Invoke(destino);

            double opacidadDestino = expandir ? 1.0 : 0.0;
            var fadeAnim = new DoubleAnimation
            {
                To = opacidadDestino,
                Duration = TimeSpan.FromMilliseconds(expandir ? DURACION_MS : 80)
            };
            FadeTextos(fadeAnim);

            _expandido = expandir;
        }

        /// <summary>Aplica la animación de opacidad a todos los TextBlock de etiquetas.</summary>
        private void FadeTextos(DoubleAnimation anim)
        {
            TextBlock[] etiquetas =
            {
                txtBienvenido,
                txtInicio, txtClientes, txtVentas, txtCompras,
                txtProveedores, txtInventario, txtDeudores,
                txtReportes, txtUsuarios, txtBitacora, txtCerrarSesion
            };

            foreach (var tb in etiquetas)
                tb?.BeginAnimation(UIElement.OpacityProperty, anim);
        }

        // ── Navegación ────────────────────────────────────────────────────────
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string modulo)
            {
                SetActivo(btn);
                NavegacionSolicitada?.Invoke(modulo);
            }
        }

        /// <summary>Aplica el estilo "activo" al botón pulsado y quita el de los demás.</summary>
        private void SetActivo(Button activo)
        {
            Button[] todos = { btnInicio, btnClientes, btnVentas, btnCompras,
                       btnProveedores, btnInventario, btnDeudores,
                       btnReportes, btnUsuarios, btnBitacora };

            foreach (var b in todos)
            {
                if (b == null) continue;

                bool esActivo = (b == activo);

                if (esActivo)
                {
                    b.Style = (Style)Resources["NavButtonActiveStyle"];
                    var tb = ObtenerTextBlock(b);
                    if (tb != null) tb.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                            .ConvertFromString("#daa564"));
                }
                else
                {
                    b.Style = (Style)Resources["NavButtonStyle"];
                    var tb = ObtenerTextBlock(b);
                    if (tb != null) tb.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter
                            .ConvertFromString("#c4a882"));
                }
            }
        }

        public void SetModuloActivo(string modulo)
        {
            var mapa = new System.Collections.Generic.Dictionary<string, Button>
            {
                {"Inicio", btnInicio}, {"Clientes", btnClientes}, {"Ventas", btnVentas},
                {"Compras", btnCompras}, {"Proveedores", btnProveedores},
                {"Inventario", btnInventario}, {"Deudores", btnDeudores},
                {"Reportes", btnReportes}, {"Usuarios", btnUsuarios}, {"Bitacora", btnBitacora}
            };

            if (mapa.TryGetValue(modulo, out var btn))
                SetActivo(btn);
        }


        /// <summary>Obtiene el segundo TextBlock (etiqueta) dentro del Grid de un botón nav.</summary>
        private TextBlock ObtenerTextBlock(Button b)
        {
            if (b.Content is Grid g && g.Children.Count >= 2)
                return g.Children[1] as TextBlock;
            return null;
        }

        // ── Cerrar sesión ─────────────────────────────────────────────────────
        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            CerrarSesionSolicitada?.Invoke();
        }
    }
}
