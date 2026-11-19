using System;
using System.Windows.Forms;
using System.Windows.Interop;
using TRAMADE.FormulariosCompras;

namespace TRAMADE
{
    public partial class frmRegistrar : Form
    {
        public frmRegistrar()
        {
            InitializeComponent();
            Load += frmRegistrar_Load;
        }

        private void frmRegistrar_Load(object sender, EventArgs e)
        {
            MostrarWpf(new RegistrarCompraControl());
        }

        private void MostrarWpf(System.Windows.Window ventana)
        {
            Hide();
            ventana.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            new WindowInteropHelper(ventana).Owner = Handle;
            ventana.ShowDialog();
            Close();
        }

        private void frmRegistrar_Load_1(object sender, EventArgs e)
        {

        }
    }
}
