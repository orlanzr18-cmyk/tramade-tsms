using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.Formulario_Reporte;

namespace TRAMADE
{
    public partial class frmReportes : Form
    {
        public frmReportes()
        {
            InitializeComponent();
        }

        private void frmReportes_Load(object sender, EventArgs e)
        {
            ElementHost host = new ElementHost();
            host.Dock = DockStyle.Fill;
            host.Child = new ucReportes();

            this.Controls.Clear();
            this.Controls.Add(host);
        }
    }
}