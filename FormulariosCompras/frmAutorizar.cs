using System;
using System.Windows.Forms;
using System.Windows.Interop;
using TRAMADE.FormulariosCompras;

namespace TRAMADE
{
    public partial class frmAutorizar : Form
    {
        public frmAutorizar()
        {
            InitializeComponent();
            Load += frmAutorizar_Load;
        }

        private void frmAutorizar_Load(object sender, EventArgs e)
        {
            MostrarWpf(new AutorizarCompraControl());
        }

        private void MostrarWpf(System.Windows.Window ventana)
        {
            Hide();
            ventana.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            new WindowInteropHelper(ventana).Owner = Handle;
            ventana.ShowDialog();
            Close();
        }

        private void frmAutorizar_Load_1(object sender, EventArgs e)
        {

        }
    }
}
