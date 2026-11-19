using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TRAMADE.Formularios_Login__Menú;
using TRAMADE.Services;
using TRAMADE.Repositories;
using TRAMADE.Models;

namespace TRAMADE.FormulariosCliente
{
    public partial class ucMenu : UserControl
    {
        private readonly clsClienteService ObjServicio = new clsClienteService();

        private readonly ObservableCollection<clsUltimoClienteRow> FilasUltimos
            = new ObservableCollection<clsUltimoClienteRow>();

        public ucMenu()
        {
            InitializeComponent();
            dgUltimos.ItemsSource = FilasUltimos;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            recargarClientes();
            recargarGraficaEstadoCli();
            recargarGraficaDepa();

            if (clsPermisos.esEmpleado)
            {
                btnActualizar.Visibility = Visibility.Collapsed;
                btnAutorizar.Visibility = Visibility.Collapsed;
            }
        }

        public void recargarClientes()
        {
            try
            {
                DataTable dt = ObjServicio.ObtenerUltimosClientes();
                FilasUltimos.Clear();

                // Se leen las columnas por posición (0,1,2,3) en vez de por nombre,
                // ya que la vista Ultimos_Clientes puede tener nombres de columna
                // distintos al texto mostrado en el header.
                foreach (DataRow r in dt.Rows)
                {
                    FilasUltimos.Add(new clsUltimoClienteRow
                    {
                        IdCliente = r[0]?.ToString() ?? "",
                        NombreCliente = r[1]?.ToString() ?? "",
                        FechaRegistro = r[2]?.ToString() ?? "",
                        Estado = r[3]?.ToString() ?? ""
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los clientes: " + ex.Message);
            }
        }

        public void recargarGraficaDepa()
        {
            DataTable dt = ObjServicio.ObtenerTopDepartamentos();
            var grafico = new clsGraficoTopDepartamentos(plotTopDepartamentos);
            grafico.Cargar(dt);
        }

        public void recargarGraficaEstadoCli()
        {
            DataTable dt = ObjServicio.ObtenerPorcentajeEstados();
            var grafico = new clsGraficoEstadoClientes(plotEstadoClientes);
            grafico.Cargar(dt);
            ConstruirLeyendaEstados(grafico.ItemsLeyenda);
        }

        private void ConstruirLeyendaEstados(System.Collections.Generic.List<clsItemLeyenda> items)
        {
            spLeyendaEstados.Children.Clear();

            foreach (var item in items)
            {
                var contenedor = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(8, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var punto = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = new SolidColorBrush(item.Color),
                    Margin = new Thickness(0, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var texto = new TextBlock
                {
                    Text = item.Nombre,
                    FontFamily = new System.Windows.Media.FontFamily("Poppins"),
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center
                };

                contenedor.Children.Add(punto);
                contenedor.Children.Add(texto);
                spLeyendaEstados.Children.Add(contenedor);
            }
        }

        private void btnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            frmRegistro objRegis = new frmRegistro();
            objRegis.ShowDialog();
            recargarClientes();
            recargarGraficaEstadoCli();
            recargarGraficaDepa();
        }

        private void btnActualizar_Click(object sender, RoutedEventArgs e)
        {
            frmRegistroNuevo objRegisNue = new frmRegistroNuevo();
            objRegisNue.ShowDialog();
            recargarClientes();
            recargarGraficaEstadoCli();
            recargarGraficaDepa();
        }

        private void btnAutorizar_Click(object sender, RoutedEventArgs e)
        {
            frmAprobacion objApro = new frmAprobacion();
            objApro.ShowDialog();
            recargarClientes();
            recargarGraficaEstadoCli();
            recargarGraficaDepa();
        }

        private void btnSeguimiento_Click(object sender, RoutedEventArgs e)
        {
            frmSeguimientoCliente objSegui = new frmSeguimientoCliente();
            objSegui.ShowDialog();
            recargarClientes();
            recargarGraficaEstadoCli();
            recargarGraficaDepa();
        }
    }
}