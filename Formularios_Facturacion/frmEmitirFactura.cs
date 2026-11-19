using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE
{
    public partial class frmEmitirFactura : Form
    {
        public int IdFacturaRecibido;

        public frmEmitirFactura()
        {
            InitializeComponent();
        }

        private void frmEmitirFactura_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = false;

            var wpfControl = new wndEmitirFactura
            {
                IdFacturaRecibido = this.IdFacturaRecibido
            };

            var host = new ElementHost
            {
                Child = wpfControl,
                Dock = DockStyle.Fill
            };

            this.Controls.Add(host);
            host.BringToFront();
        }
    }
}
