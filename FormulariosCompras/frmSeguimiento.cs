using System;
using System.Windows.Forms;
using System.Windows.Interop;
using TRAMADE.FormulariosCompras;

namespace TRAMADE
{
    public partial class frmSeguimiento : Form
    {
        public frmSeguimiento()
        {
            InitializeComponent();
            Load += frmSeguimiento_Load;
        }

        private void frmSeguimiento_Load(object sender, EventArgs e)
        {
            MostrarWpf(new SeguimientoCompraControl());
        }

        private void MostrarWpf(System.Windows.Window ventana)
        {
            Hide();
            ventana.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            new WindowInteropHelper(ventana).Owner = Handle;
            ventana.ShowDialog();
            Close();
        }
    }
}
