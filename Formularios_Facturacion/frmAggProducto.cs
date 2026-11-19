using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE
{
    public partial class frmAggProducto : Form
    {
        private wndAggProducto _aggProductoWpf;

        public frmAggProducto()
        {
            InitializeComponent();
            this.FormClosing += frmAggProducto_FormClosing;
        }

       
        private void frmAggProducto_FormClosing(object sender, FormClosingEventArgs e)
        {
            _aggProductoWpf?.ApagarCamara();
        }

        private void frmAggProducto_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = false;

            var aggProductoWpf = new wndAggProducto();
            _aggProductoWpf = aggProductoWpf;

            aggProductoWpf.ProductosSeleccionados += (s, productos) =>
            {
                var principal = ucFacturacionControl.InstanciaActiva;
                if (principal == null) return;

                bool huboError = false;

                foreach (var prod in productos)
                {
                    if (prod.CantidadSeleccionada > prod.existencia_producto)
                    {
                        MessageBox.Show(
                            $"No puede agregar {prod.CantidadSeleccionada} unidades de '{prod.nombre_producto}'. " +
                            $"Solo hay {prod.existencia_producto} en stock.",
                            "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        huboError = true;
                        continue;
                    }

                    principal.AgregarProductoAlDetalle(
                        prod.id_producto.ToString(),
                        prod.nombre_producto,
                        "Unidad",
                        prod.CantidadSeleccionada,
                        prod.precio_unitario);
                }

                if (!huboError) this.Close();
            };

            aggProductoWpf.OperacionCancelada += (s, args) => this.Close();

            var host = new ElementHost
            {
                Child = aggProductoWpf,
                Dock = DockStyle.Fill
            };

            this.Controls.Add(host);
            host.BringToFront();
        }
    }
}