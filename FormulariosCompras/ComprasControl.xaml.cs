using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using TRAMADE.Services.ServCompras;

namespace TRAMADE.FormulariosCompras
{
    public partial class ComprasControl : UserControl
    {
        private readonly ICompraService _compraService;

        public ComprasControl() : this(new CompraService())
        {
        }

        public ComprasControl(ICompraService compraService)
        {
            if (compraService == null) throw new ArgumentNullException("compraService");
            _compraService = compraService;
            InitializeComponent();
            ConfigurarGridRecientes();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarDashboard();
            if (clsPermisos.esEmpleado)
            {
                btnActualizar.Visibility = Visibility.Collapsed;
                btnAutorizar.Visibility = Visibility.Collapsed;
            }
        }

        private void ConfigurarGridRecientes()
        {
            DgRecientes.Columns.Clear();
            AgregarColumna(DgRecientes, "ID compra", "ID compra", AnchoProporcional(0.7), 80);
            AgregarColumna(DgRecientes, "Proveedor", "Proveedor", AnchoProporcional(1.2), 145);
            AgregarColumna(DgRecientes, "Producto", "Producto", AnchoProporcional(2.2), 200);
            AgregarColumna(DgRecientes, "Estado", "Estado", AnchoProporcional(1), 110);
            AgregarColumna(DgRecientes, "Fecha pedido", "Fecha pedido", AnchoProporcional(1.35), 165, "dd/MM/yyyy HH:mm:ss");
            AgregarColumna(DgRecientes, "Fecha entrega", "Fecha entrega", AnchoProporcional(1.35), 165, "dd/MM/yyyy HH:mm:ss");
        }

        private static DataGridLength AnchoProporcional(double valor)
        {
            return new DataGridLength(valor, DataGridLengthUnitType.Star);
        }

        private static void AgregarColumna(
            DataGrid grid, string titulo, string campo, DataGridLength ancho,
            double anchoMinimo, string formato = null)
        {
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = titulo,
                Width = ancho,
                MinWidth = anchoMinimo,
                Binding = new Binding("[" + campo + "]") { StringFormat = formato }
            });
        }

        private void CargarDashboard()
        {
            try
            {
                DataTable recientes = _compraService.ObtenerComprasRecientes(15);
                NormalizarColumnaProducto(recientes);
                DgRecientes.ItemsSource = recientes.DefaultView;
                CargarGraficoProductos();
                CargarGraficoProveedores();
                CargarGraficoEstados();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No fue posible cargar el módulo de compras.\n" + ex.Message,
                    "Compras", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarGraficoProductos()
        {
            DataTable tabla = _compraService.ObtenerTopProductos();
            PlotProductos.Model = CrearBarras("Top 5 Productos Más Comprados", tabla, "Producto", "TotalComprado");
        }

        private static void NormalizarColumnaProducto(DataTable tabla)
        {
            if (tabla.Columns.Contains("Producto"))
            {
                return;
            }

            if (tabla.Columns.Contains("Nombre producto"))
            {
                tabla.Columns["Nombre producto"].ColumnName = "Producto";
                return;
            }

            if (tabla.Columns.Contains("nombre_producto"))
            {
                tabla.Columns["nombre_producto"].ColumnName = "Producto";
            }
        }

        private void CargarGraficoProveedores()
        {
            DataTable tabla = _compraService.ObtenerTopProveedores();
            PlotProveedores.Model = CrearBarras("Top 5 Proveedores Más Usados", tabla, "Proveedor", "TotalOrdenes");
        }

        private void CargarGraficoEstados()
        {
            DataTable tabla = _compraService.ObtenerEstadoCompras();
            PlotModel modelo = CrearModelo("Estado de Compras");
            PieSeries serie = new PieSeries
            {
                InnerDiameter = 0.48,
                StrokeThickness = 0,
                AngleSpan = 360,
                StartAngle = 90,
                InsideLabelFormat = string.Empty,
                OutsideLabelFormat = string.Empty,
                TickHorizontalLength = 0,
                TickRadialLength = 0,
                AreInsideLabelsAngled = false
            };
            if (tabla.Rows.Count > 0)
            {
                serie.Slices.Add(new PieSlice("Pendientes",
                    Convert.ToDouble(tabla.Rows[0]["ComprasPendientes"]))
                    { Fill = OxyColor.Parse("#D2A86E") });
                serie.Slices.Add(new PieSlice("Autorizadas",
                    Convert.ToDouble(tabla.Rows[0]["ComprasAutorizadas"]))
                    { Fill = OxyColor.Parse("#48C7AE") });
            }
            modelo.Series.Add(serie);
            PlotEstados.Model = modelo;
        }

        private static PlotModel CrearModelo(string titulo)
        {
            return new PlotModel
            {
                Title = titulo,
                DefaultFont = "Poppins",
                TitleFont = "Poppins",
                TitleHorizontalAlignment = OxyPlot.TitleHorizontalAlignment.CenteredWithinView,
                TitleFontWeight = 600,
                TitleFontSize = 12,
                PlotAreaBorderThickness = new OxyThickness(0),
                Background = OxyColors.White,
                TextColor = OxyColor.Parse("#3D2B1F"),
                Padding = new OxyThickness(0, 0, 0, 0),
                IsLegendVisible = false
            };
        }

        private static PlotModel CrearBarras(string titulo, DataTable tabla, string etiqueta, string valor)
        {
            PlotModel modelo = CrearModelo(titulo);
            modelo.PlotMargins = new OxyThickness(118, 10, 22, 26);

            CategoryAxis categorias = new CategoryAxis
            {
                Position = AxisPosition.Left,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                Font = "Poppins",
                FontSize = 9,
                TextColor = OxyColor.Parse("#4D4640"),
                GapWidth = 0.55
            };
            LinearAxis numeros = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                TickStyle = TickStyle.None,
                AxislineStyle = LineStyle.None,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.Parse("#EFE6DD"),
                TextColor = OxyColor.Parse("#B9B0A8"),
                FontSize = 9,
                MinorGridlineStyle = LineStyle.None
            };
            BarSeries barras = new BarSeries
            {
                FillColor = OxyColor.Parse("#9A7449"),
                StrokeThickness = 0,
                LabelPlacement = LabelPlacement.Outside,
                LabelFormatString = "{0:N0}",
                LabelColor = OxyColor.Parse("#3D2B1F"),
                BarWidth = 0.58,
                FontSize = 9
            };

            OxyColor[] colores =
            {
                OxyColor.Parse("#E6CDAA"),
                OxyColor.Parse("#D2B28A"),
                OxyColor.Parse("#BE9669"),
                OxyColor.Parse("#A57D50"),
                OxyColor.Parse("#947247")
            };

            int indice = 0;
            foreach (DataRow fila in tabla.Rows)
            {
                categorias.Labels.Add(Convert.ToString(fila[etiqueta]));
                barras.Items.Add(new BarItem(Convert.ToDouble(fila[valor]))
                {
                    Color = colores[indice < colores.Length ? indice : colores.Length - 1]
                });
                indice++;
            }
            modelo.Axes.Add(categorias);
            modelo.Axes.Add(numeros);
            modelo.Series.Add(barras);
            return modelo;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint flags);

        private void MostrarVentana(Window ventana)
        {
            ventana.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ventana.ShowActivated = true;

            HwndSource origen = PresentationSource.FromVisual(this) as HwndSource;
            if (origen != null)
            {
                // El UserControl vive dentro de un ElementHost. GA_ROOT obtiene
                // el HWND del formulario WinForms principal para usarlo como dueño.
                IntPtr propietario = GetAncestor(origen.Handle, 2);
                if (propietario != IntPtr.Zero)
                    new WindowInteropHelper(ventana).Owner = propietario;
            }

            ventana.ShowDialog();
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            MostrarVentana(new RegistrarCompraControl(_compraService));
            CargarDashboard();
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            MostrarVentana(new ActualizarCompraControl(_compraService));
            CargarDashboard();
        }

        private void BtnAutorizar_Click(object sender, RoutedEventArgs e)
        {
            MostrarVentana(new AutorizarCompraControl(_compraService));
            CargarDashboard();
        }

        private void BtnSeguimiento_Click(object sender, RoutedEventArgs e)
        {
            MostrarVentana(new SeguimientoCompraControl(_compraService));
            CargarDashboard();
        }
    }
}
