using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.FormulariosProveedor;

namespace TRAMADE
{
    public partial class frmProveedores_Editar : Form
    {
        private int _idProveedor;

        public frmProveedores_Editar(int idProveedor)
        {
            InitializeComponent();
            _idProveedor = idProveedor;
        }

        private void frmProveedores_Editar_Load(object sender, EventArgs e)
        {
            var host = new ElementHost
            {
                Child = new ProveedoresEditarControl(_idProveedor),
                Dock = DockStyle.Fill
            };
            this.Controls.Add(host);
            host.BringToFront();
        }
    }
}
