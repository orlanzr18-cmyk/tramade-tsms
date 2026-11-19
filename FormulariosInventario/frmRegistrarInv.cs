using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE
{
    public partial class frmRegistrarInv : Form
    {
        private RegistrarInvControl _control;

        public frmRegistrarInv()
        {
            InitializeComponent();
        }

        private void frmRegistrarInv_Load(object sender, EventArgs e)
        {
            _control = new RegistrarInvControl();
            _control.Cerrar += () =>
            {
                if (this.InvokeRequired)
                    this.Invoke((Action)this.Close);
                else
                    this.Close();
            };

            var host = new ElementHost { Child = _control, Dock = DockStyle.Fill };
            this.Controls.Add(host);
        }
    }
}