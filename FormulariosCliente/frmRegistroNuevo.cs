using System;
using System.Windows.Forms;

namespace TRAMADE
{
    public partial class frmRegistroNuevo : Form
    {
        public frmRegistroNuevo()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
        }

        private void frmRegistroNuevo_Load(object sender, EventArgs e)
        {
            foreach (Control ctrl in this.Controls)
                ctrl.Visible = false;

            var wpfControl = new TRAMADE.FormulariosCliente.wndRegistroNuevo();

            var host = new System.Windows.Forms.Integration.ElementHost
            {
                Child = wpfControl,
                Dock = System.Windows.Forms.DockStyle.Fill
            };

            this.Controls.Add(host);
            host.BringToFront();
        }

        private void kryptonGroup1_Panel_Paint(object sender, PaintEventArgs e) { }
        private void txtSoloNumeros_KeyPress(object sender, KeyPressEventArgs e) { }
        private void cmbTipoCliente_SelectedIndexChanged_1(object sender, EventArgs e) { }
        private void cmbDepartamento_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtBuscar_TextChanged(object sender, EventArgs e) { }
        private void btnBuscar_Click(object sender, EventArgs e) { }
        private void txtDNI_TextChanged(object sender, EventArgs e) { }
        private void btnGuardar_Click(object sender, EventArgs e) { }
        private void btnLimpiar_Click(object sender, EventArgs e) { }
        private void txtCorreo_KeyPress(object sender, KeyPressEventArgs e) { }
        private void txtDireccion_TextChanged(object sender, EventArgs e) { }
        private void label14_Click(object sender, EventArgs e) { }
    }
}