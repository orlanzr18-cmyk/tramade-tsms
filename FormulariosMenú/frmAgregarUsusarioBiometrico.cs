using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.FormulariosAdministrador;

namespace TRAMADE
{
    public partial class frmAgregarUsusarioBiometrico : Form
    {
        public frmAgregarUsusarioBiometrico()
        {
            InitializeComponent();

            var wpfControl = new wndAgregarUsuarioBiometrico();

            var host = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = wpfControl
            };

            Controls.Clear();
            Controls.Add(host);
        }

        private void frmAgregarUsusarioBiometrico_Load(object sender, EventArgs e)
        {

        }
    }
}