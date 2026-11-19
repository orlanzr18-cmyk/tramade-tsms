using System;
using System.Windows.Forms;

namespace TRAMADE
{
    public partial class frmAprobacion : Form
    {
        public frmAprobacion()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
        }

        private void frmAprobacion_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = false;

            this.Padding = new Padding(0);
            this.BackColor = System.Drawing.Color.White;

            var wpfControl = new TRAMADE.FormulariosCliente.wndAprobacion();

            var host = new System.Windows.Forms.Integration.ElementHost
            {
                Child = wpfControl,
                Dock = System.Windows.Forms.DockStyle.Fill,
                Margin = new Padding(0)
            };

            this.Controls.Add(host);
            host.BringToFront();
        }

        private void kryptonGroup1_Panel_Paint(object sender, PaintEventArgs e) { }
        private void btnNegar_Click(object sender, EventArgs e) { }
        private void btnAutorizar_Click(object sender, EventArgs e) { }
        private void txtBuscar_TextChanged(object sender, EventArgs e) { }
    }
}