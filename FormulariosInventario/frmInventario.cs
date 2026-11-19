using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE
{
    /// <summary>
    /// Form MDI hijo vacío que hospeda el UserControl WPF InventarioControl.
    /// Toda la lógica visual está en InventarioControl.xaml / .xaml.cs
    /// </summary>
    public partial class frmInventario : Form
    {
        private InventarioControl _inventario;
        private bool _cargaInicialCompletada;

        public frmInventario()
        {
            InitializeComponent();
            ControlBox = false;

            _inventario = new InventarioControl();

            _inventario.RegistrarSolicitado += () =>
            {
                var frm = new frmRegistrarInv();
                frm.ShowDialog();
                _ = _inventario.CargarAsync();
            };

            _inventario.EditarSolicitado += (idProducto) =>
            {
                var frm = new frmEditarInv(idProducto);
                frm.ShowDialog();
                _ = _inventario.CargarAsync();
            };

            Controls.Add(new ElementHost { Child = _inventario, Dock = DockStyle.Fill });
        }

        private async void frmInventario_Load(object sender, EventArgs e)
        {
            await _inventario.CargarAsync();
            _cargaInicialCompletada = true;
        }

        protected override async void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            // El menú conserva este formulario en caché y lo oculta al cambiar
            // de módulo. Al volver a Inventario se deben consultar nuevamente
            // las existencias, incluidas las compras que acaban de recibirse.
            if (Visible && _cargaInicialCompletada && _inventario != null)
                await _inventario.CargarAsync();
        }

        // Liberar el timer del UserControl WPF al cerrar el form
        private void frmInventario_FormClosed(object sender, FormClosedEventArgs e) { }
    }
}
