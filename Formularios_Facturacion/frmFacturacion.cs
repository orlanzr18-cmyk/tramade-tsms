using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE
{
    public partial class frmFacturacion : Form
    {
        public frmFacturacion()
        {
            InitializeComponent();
            this.Activated += frmFacturacion_Activated;
        }

        private void frmFacturacion_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;

            var host = new ElementHost
            {
                Child = new ucFacturacionControl(),
                Dock = DockStyle.Fill
            };
            this.Controls.Add(host);
            host.BringToFront();
        }

        private void frmFacturacion_Activated(object sender, EventArgs e)
        {
            ucFacturacionControl.InstanciaActiva?.RefrescarAutoCompleteClientes();
        }
    }
}