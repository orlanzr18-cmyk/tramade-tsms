using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using TRAMADE.Services;

namespace TRAMADE.FormulariosCliente
{
    public partial class wndSeguimientoCliente : UserControl
    {
        private readonly clsClienteService ObjServicio = new clsClienteService();

        private System.Windows.Threading.DispatcherTimer _debounceBusqueda;

        public wndSeguimientoCliente()
        {
            InitializeComponent();

            // Sin esto, cada tecla presionada disparaba una consulta a la base de
            // datos de forma sincrónica en el hilo de la UI (escribir "constructora"
            // = 12 consultas, cada una congelando la ventana). Con el debounce, solo
            // se consulta 300ms después de que el usuario deja de escribir.
            _debounceBusqueda = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _debounceBusqueda.Tick += DebounceBusqueda_Tick;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            recargarClientes();
            ActualizarContadores();
        }

        private void recargarClientes()
        {
            try
            {
                DataTable dt = ObjServicio.ObtenerVistaSeguimiento();
                dvgSeguimiento.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los clientes: " + ex.Message);
            }
        }

        private void ActualizarContadores()
        {
            var stats = ObjServicio.ObtenerEstadisticas();
            if (stats != null)
            {
                txtRegistrados.Text = stats.Total;
                txtActivos.Text = stats.Activos;
                txtInactivos.Text = stats.Inactivos;
            }
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Reinicia el temporizador cada vez que se escribe: solo se dispara
            // la búsqueda real cuando pasan 300ms sin que el usuario teclee.
            _debounceBusqueda.Stop();
            _debounceBusqueda.Start();
        }

        private async void DebounceBusqueda_Tick(object sender, EventArgs e)
        {
            _debounceBusqueda.Stop();
            string textoBusqueda = txtBuscar.Text;

            try
            {
                // ExecuteReader es sincrónico; se corre en un hilo de fondo con
                // Task.Run para que la ventana no se congele mientras espera la
                // respuesta de la base de datos.
                DataTable dt = await System.Threading.Tasks.Task.Run(
                    () => ObjServicio.BuscarEnVistaSeguimientoPorNombre(textoBusqueda));

                // Si el usuario siguió escribiendo mientras la consulta estaba en
                // vuelo, se descarta el resultado viejo para no pisar uno más nuevo.
                if (textoBusqueda != txtBuscar.Text) return;

                if (dt != null)
                {
                    dvgSeguimiento.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar: " + ex.Message);
            }
        }
    }
}