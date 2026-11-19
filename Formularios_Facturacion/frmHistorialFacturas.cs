using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE
{
    public partial class frmHistorialFacturas : Form
    {
        public frmHistorialFacturas()
        {
            InitializeComponent();
        }

        private void frmHistorialFacturas_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = false;

            var historialWpf = new wndHistorialFacturas();
            historialWpf.SolicitudCerrar += (s, args) => this.Close();

            var host = new ElementHost
            {
                Child = historialWpf,
                Dock = DockStyle.Fill
            };

            this.Controls.Add(host);
            host.BringToFront();
        }
    }
}
