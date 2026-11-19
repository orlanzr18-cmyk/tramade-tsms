/*using Microsoft.Reporting.WinForms;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using TRAMADE.Clases_Reporte;

namespace TRAMADE.Formulario_Reporte
{
    internal class clsReportes
    {
        private readonly DataGrid _dgvReportes;
        private readonly ComboBox _cmbSucursal;
        private readonly ComboBox _cmbTipoReporte;
        private readonly ObservableCollection<clsReportItem> _listaReportes;

        public clsReportes(ComboBox cmbSucursal, ComboBox cmbTipoReporte, DataGrid dgvReportes)
        {
            _cmbSucursal = cmbSucursal;
            _cmbTipoReporte = cmbTipoReporte;
            _dgvReportes = dgvReportes;

            _listaReportes = new ObservableCollection<clsReportItem>();
            _dgvReportes.ItemsSource = _listaReportes;
        }

        public void CargarSucursales()
        {
            clsConexion cn = new clsConexion();

            try
            {
                cn.Abrir();

                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT id_sucursal, nombre_sucursal FROM SUCURSAL",
                    cn.SqlC);

                DataTable dt = new DataTable();
                da.Fill(dt);

                DataRow row = dt.NewRow();
                row["id_sucursal"] = 0;
                row["nombre_sucursal"] = "Todas";
                dt.Rows.InsertAt(row, 0);

                _cmbSucursal.ItemsSource = null;
                _cmbSucursal.Items.Clear();

                foreach (DataRow rowSucursal in dt.Rows)
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = rowSucursal["nombre_sucursal"].ToString();
                    item.Tag = Convert.ToInt32(rowSucursal["id_sucursal"]);

                    _cmbSucursal.Items.Add(item);
                }

            
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar sucursales: " + ex.Message,
                    "ERROR",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        public void CargarTiposReporte()
        {
            _cmbTipoReporte.Items.Clear();

            _cmbTipoReporte.Items.Add("Ventas por período");
            _cmbTipoReporte.Items.Add("Compras por período");
            _cmbTipoReporte.Items.Add("Stock por sucursal");
            _cmbTipoReporte.Items.Add("Productos más vendidos");


        }

        public void Generar(DateTime fechaInicio, DateTime fechaFin, int idSucursal)
        {
            if (_cmbTipoReporte.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecciona un tipo de reporte.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string tipoReporte = _cmbTipoReporte.SelectedItem.ToString();

            switch (tipoReporte)
            {
                case "Ventas por período":
                    AgregarALista(
                        $"Ventas | {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy} | {_cmbSucursal.Text}",
                        "Ventas por período",
                        fechaInicio,
                        fechaFin,
                        idSucursal);
                    break;

                case "Compras por período":
                    AgregarALista(
                        $"Compras | {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy} | {_cmbSucursal.Text}",
                        "Compras por período",
                        fechaInicio,
                        fechaFin,
                        idSucursal);
                    break;

                case "Stock por sucursal":
                    AgregarALista(
                        $"Stock | {_cmbSucursal.Text}",
                        "Stock por sucursal",
                        fechaInicio,
                        fechaFin,
                        idSucursal);
                    break;

                case "Productos más vendidos":
                    AgregarALista(
                        $"Productos más vendidos | {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy} | {_cmbSucursal.Text}",
                        "Productos más vendidos",
                        fechaInicio,
                        fechaFin,
                        idSucursal);
                    break;
            }
        }

        public void AbrirReporte(string formato)
        {
            if (_dgvReportes.SelectedItem == null)
            {
                MessageBox.Show(
                    "Selecciona un reporte de la lista.",
                    "Aviso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            clsReportItem item = (clsReportItem)_dgvReportes.SelectedItem;
            LocalReport report = null;

            switch (item.TipoReporte)
            {
                case "Ventas por período":
                    report = new clsRptVentas().Generar(
                        item.FechaInicio,
                        item.FechaFin,
                        item.IdSucursal,
                        item.NombreSucursal);
                    break;

                case "Compras por período":
                    report = new clsRptCompras().Generar(
                        item.FechaInicio,
                        item.FechaFin,
                        item.IdSucursal,
                        item.NombreSucursal);
                    break;

                case "Stock por sucursal":
                    report = new clsRptStock().Generar(
                        item.IdSucursal,
                        item.NombreSucursal);
                    break;

                case "Productos más vendidos":
                    report = new clsRptProductos().Generar(
                        item.FechaInicio,
                        item.FechaFin,
                        item.IdSucursal,
                        item.NombreSucursal);
                    break;
            }

            if (report != null)
                clsDescarga.Descargar(report, formato);
        }

  
        private void AgregarALista(
            string descripcion,
            string tipoReporte,
            DateTime fechaInicio,
            DateTime fechaFin,
            int idSucursal)
        {
            clsReportItem item = new clsReportItem
            {
                Reporte = descripcion,
                TipoReporte = tipoReporte,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                IdSucursal = idSucursal,
                NombreSucursal = _cmbSucursal.Text
            };

            _listaReportes.Add(item);
            _dgvReportes.Items.Refresh();
            _dgvReportes.SelectedItem = item;
        }
    }
}*/