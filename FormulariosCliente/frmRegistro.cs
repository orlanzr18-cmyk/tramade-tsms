using System;
using System.Windows.Forms;

namespace TRAMADE
{
    public partial class frmRegistro : Form
    {
        public frmRegistro()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
        }

        private void frmRegistro_Load_1(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = false;

            var wpfControl = new TRAMADE.FormulariosCliente.wndRegistro();

            var host = new System.Windows.Forms.Integration.ElementHost
            {
                Child = wpfControl,
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            this.Controls.Add(host);
            host.BringToFront();
        }

        private void kryptonTextBox1_TextChanged(object sender, EventArgs e) { }
        private void cmbTipoCliente_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtCorreo_KeyPress(object sender, KeyPressEventArgs e) { }
        private void txtCorreo_KeyUp(object sender, KeyEventArgs e) { }
        private void cmbDepartamento_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtSoloNumeros_KeyPress(object sender, KeyPressEventArgs e) { }
        private void label17_Click(object sender, EventArgs e) { }
        private void btnRegistrar_Click(object sender, EventArgs e) { }
        private void btnLimpiar_Click(object sender, EventArgs e) { }
        private void label15_Click(object sender, EventArgs e) { }
        private void label14_Click(object sender, EventArgs e) { }
    }
}