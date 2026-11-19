using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.FormulariosProveedor;

namespace TRAMADE
{
    public partial class frmProveedores_Añadir : Form
    {
        public frmProveedores_Añadir()
        {
            InitializeComponent();
        }

        private void frmProveedores_Añadir_Load(object sender, EventArgs e)
        {
            var host = new ElementHost
            {
                Child = new ProveedoresAñadirControl(),
                Dock = DockStyle.Fill
            };
            this.Controls.Add(host);
            host.BringToFront();
        }
    }
}
