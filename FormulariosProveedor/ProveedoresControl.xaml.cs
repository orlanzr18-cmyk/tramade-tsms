using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TRAMADE.Formulario_Proveedores.Clases;
using TRAMADE.Formulario_Proveedores;

namespace TRAMADE.FormulariosProveedor
{
    public partial class ProveedoresControl : UserControl
    {
        private ObservableCollection<ProveedorItem> _proveedores = new ObservableCollection<ProveedorItem>();

        public ProveedoresControl()
        {
            InitializeComponent();
            dgvProveedor.ItemsSource = _proveedores;
            if (clsPermisos.esEmpleado)
            {
                dgvProveedor.Columns[0].Visibility = Visibility.Collapsed;
                btnAñadirProveedor.Visibility = Visibility.Collapsed;
                btnActivarSeleccion.Visibility = Visibility.Collapsed;
                btnInactivarSeleccion.Visibility = Visibility.Collapsed;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            RecargarProveedores();
        }

        public void RecargarProveedores()
        {
            clsProveedores obj = new clsProveedores();
            CargarDesdeDataTable(obj.ObtenerProveedores());
        }

        private void CargarDesdeDataTable(DataTable dt)
        {
            _proveedores.Clear();
            foreach (DataRow row in dt.Rows)
            {
                _proveedores.Add(new ProveedorItem
                {
                    id_proveedor = Convert.ToInt32(row["id_proveedor"]),
                    RazonSocial = row["Razón Social"]?.ToString(),
                    NombreComercial = row["Nombre Comercial"]?.ToString(),
                    RTN = row["RTN"]?.ToString(),
                    TelefonoPrincipal = row["Teléfono Principal"]?.ToString(),
                    Estado = row["Estado"]?.ToString(),
                    Clasificacion = row["Clasificacion"]?.ToString(),
                    TerminosPago = row["TerminosPago"]?.ToString(),
                    DiasCredito = row["dias_credito"] != DBNull.Value
                                  ? Convert.ToInt32(row["dias_credito"])
                                  : 0,
                    Seleccionado = false
                });
            }
        }

        private void btnAñadirProveedor_Click(object sender, RoutedEventArgs e)
        {
            var frm = new frmProveedores_Añadir();
            frm.ShowDialog();
            RecargarProveedores();
        }

        private void btnActivarSeleccion_Click(object sender, RoutedEventArgs e)
        {
            var ids = ObtenerIdsSeleccionados();
            if (ids.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un proveedor.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Está seguro que desea ACTIVAR {ids.Count} proveedor(es) seleccionado(s)?",
                "Confirmar activación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            clsProveedores obj = new clsProveedores();
            foreach (int id in ids) obj.CambiarEstado(id, true);
            RecargarProveedores();

            MessageBox.Show("Proveedor(es) activado(s) correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnInactivarSeleccion_Click(object sender, RoutedEventArgs e)
        {
            var ids = ObtenerIdsSeleccionados();
            if (ids.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un proveedor.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Está seguro que desea INACTIVAR {ids.Count} proveedor(es) seleccionado(s)?",
                "Confirmar inactivación", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado != MessageBoxResult.Yes) return;

            clsProveedores obj = new clsProveedores();
            foreach (int id in ids) obj.CambiarEstado(id, false);
            RecargarProveedores();

            MessageBox.Show("Proveedor(es) inactivado(s) correctamente.", "Éxito",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void txtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            clsProveedores obj = new clsProveedores();
            if (string.IsNullOrWhiteSpace(txtBuscar.Text))
                RecargarProveedores();
            else
                CargarDesdeDataTable(obj.BuscarProveedores(txtBuscar.Text));
        }

        private void dgvProveedor_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvProveedor.SelectedItem is ProveedorItem item)
            {
                var frmPerfil = new frmProveedores_Perfil(item.id_proveedor);
                frmPerfil.FormClosed += (s, ev) => RecargarProveedores();
                frmPerfil.Show();
            }
        }

        private List<int> ObtenerIdsSeleccionados()
        {
            var ids = new List<int>();
            foreach (var p in _proveedores)
                if (p.Seleccionado) ids.Add(p.id_proveedor);
            return ids;
        }
    }

    public class ProveedorItem : INotifyPropertyChanged
    {
        public int id_proveedor { get; set; }
        public string RazonSocial { get; set; }
        public string NombreComercial { get; set; }
        public string RTN { get; set; }
        public string TelefonoPrincipal { get; set; }
        public string Estado { get; set; }
        public string Clasificacion { get; set; }
        public string TerminosPago { get; set; }
        public int DiasCredito { get; set; }

        private bool _seleccionado;
        public bool Seleccionado
        {
            get => _seleccionado;
            set { _seleccionado = value; OnPropertyChanged(nameof(Seleccionado)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}