using System;
using System.Windows.Forms;

namespace TRAMADE
{
    public partial class frmSeguimientoCliente : Form
    {
        public frmSeguimientoCliente()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
        }

        private void frmSeguimientoCliente_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = false;

            var wpfControl = new TRAMADE.FormulariosCliente.wndSeguimientoCliente();

            var host = new System.Windows.Forms.Integration.ElementHost
            {
                Child = wpfControl,
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            this.Controls.Add(host);
            host.BringToFront();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e) { }
    }
}