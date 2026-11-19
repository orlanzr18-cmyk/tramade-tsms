using System;
using System.Windows.Forms;
using System.Windows.Interop;
using TRAMADE.FormulariosCompras;

namespace TRAMADE
{
    public partial class frmActualizar : Form
    {
        public frmActualizar()
        {
            InitializeComponent();
            Load += frmActualizar_Load;
        }

        private void frmActualizar_Load(object sender, EventArgs e)
        {
            MostrarWpf(new ActualizarCompraControl());
        }

        private void MostrarWpf(System.Windows.Window ventana)
        {
            Hide();
            ventana.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            new WindowInteropHelper(ventana).Owner = Handle;
            ventana.ShowDialog();
            Close();
        }

        private void frmActualizar_Load_1(object sender, EventArgs e)
        {

        }
    }
}
