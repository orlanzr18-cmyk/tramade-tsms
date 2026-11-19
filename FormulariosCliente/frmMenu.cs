using System;
using System.Windows.Forms;

namespace TRAMADE
{
    public partial class frmMenu : Form
    {
        public frmMenu()
        {
            InitializeComponent();
            ControlBox = false;
        }

        private void frmMenu_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = false;

            var wpfControl = new TRAMADE.FormulariosCliente.ucMenu();

            var host = new System.Windows.Forms.Integration.ElementHost
            {
                Child = wpfControl,
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            this.Controls.Add(host);
            host.BringToFront();
        }

        // Métodos vacíos requeridos por frmMenu_Designer.cs (controles ocultos, ya no se usan)
        private void label2_Click(object sender, EventArgs e) { }
        private void label18_Click(object sender, EventArgs e) { }
        private void btnRegistrar_Click(object sender, EventArgs e) { }
        private void btnActualizar_Click(object sender, EventArgs e) { }
        private void btnAutorizar_Click(object sender, EventArgs e) { }
        private void btnSeguimiento_Click(object sender, EventArgs e) { }
        private void btnRefrescar_Click(object sender, EventArgs e) { }
    }
}