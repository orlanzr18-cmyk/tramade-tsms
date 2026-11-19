using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.FormulariosProveedor;

namespace TRAMADE.Formulario_Proveedores
{
    public partial class frmProveedores_Perfil : Form
    {
        private int _idProveedor;
        private ProveedoresPerfilControl _control;

        public frmProveedores_Perfil(int idProveedor)
        {
            InitializeComponent();
            _idProveedor = idProveedor;
        }

        private void frmProveedores_Perfil_Load(object sender, EventArgs e)
        {
            _control = new ProveedoresPerfilControl(_idProveedor);
            var host = new ElementHost
            {
                Child = _control,
                Dock = DockStyle.Fill
            };
            this.Controls.Add(host);
            host.BringToFront();
        }

        public void RecargarPerfil()
        {
            _control?.CargarPerfil();
        }
    }
}
