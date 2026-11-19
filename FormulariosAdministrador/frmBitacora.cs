using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.FormulariosAdministrador;

namespace TRAMADE
{
    /// <summary>
    /// Form WinForms que hostea el UserControl WPF ucBitacora.
    /// Toda la lógica de filtrado, búsqueda y carga de datos vive en ucBitacora.
    /// </summary>
    public partial class frmBitacora : Form
    {
        private ucBitacora _wpfControl;

        public frmBitacora()
        {
            InitializeComponent();
        }

        private void frmBitacora_Load(object sender, EventArgs e)
        {
            // Instanciar el UserControl WPF — toda la lógica vive ahí
            _wpfControl = new ucBitacora();

            var host = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = _wpfControl
            };

            this.Controls.Clear();
            this.Controls.Add(host);
        }
    }
}
