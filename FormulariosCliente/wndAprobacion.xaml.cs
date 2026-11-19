using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TRAMADE.Services;
using TRAMADE.Models;

namespace TRAMADE.FormulariosCliente
{
    public partial class wndAprobacion : UserControl
    {
        private readonly clsClienteService ObjServicio = new clsClienteService();

        private readonly ObservableCollection<clsAprobacionClienteRow> Filas
            = new ObservableCollection<clsAprobacionClienteRow>();

        public wndAprobacion()
        {
            InitializeComponent();
            dgvAprobacion.ItemsSource = Filas;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            recargarClientes();
        }

        private void recargarClientes()
        {
            try
            {
                DataTable dt = ObjServicio.ObtenerVistaAprobacion();
                CargarFilas(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar los clientes: " + ex.Message);
            }
        }

        private void CargarFilas(DataTable dt)
        {
            Filas.Clear();
            foreach (DataRow r in dt.Rows)
            {
                Filas.Add(new clsAprobacionClienteRow
                {
                    Seleccionado = false,
                    IdCliente = Convert.ToInt32(r["ID Cliente"]),
                    NombreCliente = r["Nombre Cliente"]?.ToString() ?? "",
                    FechaRegistro = r["Fecha de Registro"]?.ToString() ?? "",
                    Estado = r["Estado"]?.ToString() ?? ""
                });
            }
        }

        private void btnAutorizar_Click(object sender, RoutedEventArgs e)
        {
            var seleccionados = Filas.Where(f => f.Seleccionado).ToList();

            if (seleccionados.Count == 0)
            {
                MessageBox.Show("Por favor, marque la casilla 'Seleccionar' en los clientes que desea autorizar.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int contadorExitos = 0;
            foreach (var fila in seleccionados)
            {
                try
                {
                    if (ObjServicio.AutorizarCliente(fila.IdCliente))
                        contadorExitos++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al procesar un cliente: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            MessageBox.Show($"{contadorExitos} cliente(s) autorizado(s) correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
            recargarClientes();
        }

        private void btnNegar_Click(object sender, RoutedEventArgs e)
        {
            var seleccionados = Filas.Where(f => f.Seleccionado).ToList();

            if (seleccionados.Count == 0)
            {
                MessageBox.Show("No has marcado la columna 'Seleccionar' en ningún cliente para negar.",
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int contadorNegados = 0;
            foreach (var fila in seleccionados)
            {
                try
                {
                    if (ObjServicio.DenegarCliente(fila.IdCliente))
                        contadorNegados++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al negar el cliente {fila.NombreCliente}: {ex.Message}", "Error");
                }
            }

            MessageBox.Show($"Se han negado {contadorNegados} solicitudes de clientes.", "TRAMADE - Rechazo",
                MessageBoxButton.OK, MessageBoxImage.Information);
            recargarClientes();
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            DataTable dt = ObjServicio.BuscarEnAprobacionPorNombre(txtBuscar.Text);
            if (dt != null)
            {
                CargarFilas(dt);
            }
        }
    }
}