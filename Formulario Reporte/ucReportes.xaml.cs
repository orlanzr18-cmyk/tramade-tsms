using Microsoft.Reporting.WinForms;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TRAMADE.Models;
using TRAMADE.Models.ModelsReportes;
using TRAMADE.Services.ServReportes;

namespace TRAMADE.Formulario_Reporte
{
    public partial class ucReportes : UserControl
    {
        private readonly ReporteService _reporteService;
        private readonly ReporteExportacionService _exportacionService;

        private readonly ObservableCollection<clsReporteItemModel>
            _listaReportes;

        public ucReportes()
        {
            InitializeComponent();
            _reporteService = new ReporteService();
            _exportacionService = new ReporteExportacionService();
            _listaReportes = new ObservableCollection<clsReporteItemModel>();
          
            CargarFiltros();
            dgvReportes.ItemsSource = _listaReportes;

            cmbFormato.Items.Add("PDF");
            cmbFormato.Items.Add("Excel");

            dpDesde.SelectedDate = DateTime.Today;
            dpHasta.SelectedDate = DateTime.Today;
        }


        private void DescargarReporteSeleccionado()
        {
            try
            {
                if (!(dgvReportes.SelectedItem is clsReporteItemModel reporteSeleccionado))
                {
                    MessageBox.Show("No se pudo obtener el reporte seleccionado.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string formato = cmbFormato.SelectedItem?.ToString() ?? "PDF";
                LocalReport reporteLocal = _reporteService.GenerarReporte(reporteSeleccionado);

                if (reporteLocal == null)
                {
                    MessageBox.Show("No se encontraron datos para generar el reporte.", "Sin resultados", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string extension;
                byte[] archivo = _exportacionService.Exportar(reporteLocal, formato, out extension);

                SaveFileDialog dialogo = new SaveFileDialog
                {
                    Filter = _exportacionService.ObtenerFiltroDialogo(formato),
                    FileName = $"Reporte_{DateTime.Now:yyyyMMdd_HHmmss}{extension}"
                };

                if (dialogo.ShowDialog() == true)
                {
                    File.WriteAllBytes(dialogo.FileName, archivo);
                    MessageBox.Show("Archivo guardado correctamente.", "Éxito", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error al generar el reporte: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void CargarFiltros()
        {
            try
            {
                cmbSucursal.ItemsSource = _reporteService.ObtenerSucursales();
                cmbTipoReporte.ItemsSource = _reporteService.ObtenerTiposReporte();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar los filtros: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void btnGenerar_Click(object sender, RoutedEventArgs e)
        {
            if (cmbSucursal.SelectedItem == null)
            {
                MessageBox.Show("Selecciona una sucursal.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbTipoReporte.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un tipo de reporte.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string tipoReporte = cmbTipoReporte.SelectedItem.ToString();

            bool requiereFechas = _reporteService.RequiereFechas(tipoReporte);

            if (requiereFechas)
            {
                if (dpDesde.SelectedDate == null || dpHasta.SelectedDate == null)
                {
                    MessageBox.Show("Selecciona ambas fechas.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!clsValidar.FechaFiltro(dpDesde.SelectedDate.Value, dpHasta.SelectedDate.Value)) return;
            }

            if (!clsValidar.FechaFiltro(dpDesde.SelectedDate.Value, dpHasta.SelectedDate.Value)) return;

            if (!(cmbSucursal.SelectedItem is clsSucursalModel sucursalSeleccionada))
            {
                MessageBox.Show("No se pudo obtener la sucursal seleccionada.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            clsReporteItemModel nuevoReporte =_reporteService.CrearReporteItem
                (
                    tipoReporte,
                    dpDesde.SelectedDate.Value,
                    dpHasta.SelectedDate.Value,
                    sucursalSeleccionada.Id,
                    sucursalSeleccionada.Nombre);

            _listaReportes.Add(nuevoReporte);

            dgvReportes.SelectedItem = nuevoReporte;
            dgvReportes.ScrollIntoView(nuevoReporte);
        }

        private void dgvReportes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvReportes.SelectedItem == null) 
                return;

            DescargarReporteSeleccionado();
        }

        private void btnDescargar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvReportes.Items.Count == 0)
            {
                MessageBox.Show(
                    "Primero debes generar al menos un reporte.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (dgvReportes.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecciona un reporte de la lista.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (cmbFormato.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecciona un formato de descarga.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DescargarReporteSeleccionado();
        }

        private void btnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (dgvReportes.SelectedItem == null)
            {
                MessageBox.Show("Selecciona un reporte de la lista para eliminar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                "¿Deseas eliminar el reporte seleccionado?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                object item = dgvReportes.SelectedItem;

                if (dgvReportes.ItemsSource is IList lista)
                    lista.Remove(item);
                else
                    dgvReportes.Items.Remove(item);
            }
        }

        private void cmbTipoReporte_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string tipoReporte = cmbTipoReporte.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(tipoReporte)) return;

            bool requiereFechas = _reporteService.RequiereFechas(tipoReporte);

            dpDesde.IsEnabled = requiereFechas;
            dpHasta.IsEnabled = requiereFechas;

            if (!requiereFechas)
            {
                var grisClaro = new SolidColorBrush(Color.FromRgb(0xD3, 0xD3, 0xD3));

                dpDesde.Background = grisClaro;
                dpHasta.Background = grisClaro;
            }
            else
            {
                dpDesde.ClearValue(DatePicker.BackgroundProperty);
                dpHasta.ClearValue(DatePicker.BackgroundProperty);
            }
        }
    }
}