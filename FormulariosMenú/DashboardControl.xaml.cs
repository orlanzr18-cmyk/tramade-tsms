using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TRAMADE.Formularios_Login__Menú;
using OxyPlot.Wpf;
namespace TRAMADE
{
    public partial class DashboardControl : UserControl
    {
        public DashboardControl()
        {
            InitializeComponent();
            var dias = new[] { "Domingo", "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado" };
            var meses = new[] { "enero","febrero","marzo","abril","mayo","junio",
                                 "julio","agosto","septiembre","octubre","noviembre","diciembre" };
            var hoy = DateTime.Today;
            txtFecha.Text = $"{dias[(int)hoy.DayOfWeek]} {hoy.Day} de {meses[hoy.Month - 1]}, {hoy.Year}";
            this.SizeChanged += (s, e) => AnimarBarra(_porcentajeBarra);
        }

        public async Task CargarAsync()
        {
            var taskStats = Task.Run(() => { var s = new clsStatCards(); s.Cargar(); return s; });
            var taskGrafInv = Task.Run(() => { var g = new clsGraficoInventario(); g.Cargar(); return g; });
            var taskGrafTend = Task.Run(() => { var g = new clsGraficoTendencia(); g.Cargar(); return g; });

            await Task.WhenAll(taskStats, taskGrafInv, taskGrafTend);

            var stats = taskStats.Result;
            var grafInv = taskGrafInv.Result;
            var grafTend = taskGrafTend.Result;

            var compras = await Task.Run(() => new clsUltimasCompras(8).ObtenerDatos());
            dgComprasRecientes.ItemsSource = compras.DefaultView;

            SetProductos(stats.TotalProductos);
            SetAgotados(stats.Agotados);
            SetIngresos(stats.IngresosMes);
            SetDeudores(stats.DeudoresPendientes);
            SetClientes(stats.TotalClientes, stats.ClientesActivos);

            plotInventario.Model = grafInv.Modelo;
            plotTendencia.Model = grafTend.Modelo;
        }

        public void SetProductos(int total)
        {
            txtTotalProductos.Text = total.ToString("N0");
            txtSubProductos.Text = "en inventario";
        }

        public void SetAgotados(int agotados)
        {
            txtTotalAgotados.Text = agotados.ToString("N0");
        }

        public void SetIngresos(decimal monto)
        {
            txtIngresosMes.Text = $"L. {monto:N2}";
        }

        public void SetDeudores(int deudores)
        {
            txtDeudoresPendientes.Text = deudores.ToString("N0");
        }

        private int _porcentajeBarra = 0;

        public void SetClientes(int total, int activos)
        {
            txtTotalClientes.Text = total.ToString("N0");
            _porcentajeBarra = total > 0 ? (int)(activos * 100.0 / total) : 0;
            txtPorcentajeActivos.Text = $"Activos {_porcentajeBarra}%";
            txtRatioClientes.Text = $"{activos} / {total}";
            this.Loaded += (s, e) => AnimarBarra(_porcentajeBarra);
            AnimarBarra(_porcentajeBarra);
        }

        private void AnimarBarra(int pct)
        {
            if (barraActivos.Parent is Border contenedor && contenedor.ActualWidth > 0)
            {
                barraActivos.Width = contenedor.ActualWidth * pct / 100.0;
                barraActivos.Background = pct >= 70
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#48c7ae"))
                    : pct >= 40
                        ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#daa564"))
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#c0392b"));
            }   
        }

        public event Action CardProductosClick;
        public event Action CardAgotadosClick;
        public event Action CardIngresosClick;
        public event Action CardDeudoresClick;

        private void btnProductos_Click(object sender, RoutedEventArgs e) => CardProductosClick?.Invoke();
        private void btnAgotados_Click(object sender, RoutedEventArgs e) => CardAgotadosClick?.Invoke();
        private void btnIngresos_Click(object sender, RoutedEventArgs e) => CardIngresosClick?.Invoke();
        private void btnDeudores_Click(object sender, RoutedEventArgs e) => CardDeudoresClick?.Invoke();
    }
}