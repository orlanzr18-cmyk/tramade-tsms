using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.FormulariosProveedor;

namespace TRAMADE
{
    public partial class frmProveedores : Form
    {
        private ProveedoresControl _control;

        public frmProveedores()
        {
            InitializeComponent();
        }

        private void frmProveedores_Load(object sender, EventArgs e) {
            this.ControlBox = false;

            var host = new ElementHost
            {
                Child = new FormulariosProveedor.ProveedoresControl(),
                Dock = DockStyle.Fill
            };
            this.Controls.Add(host);
            host.BringToFront();
        }

        public void RecargarProveedores()
        {
            _control?.RecargarProveedores();
        }
    }
}