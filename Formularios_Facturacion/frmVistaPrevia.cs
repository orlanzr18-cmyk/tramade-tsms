using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE
{
    public partial class frmVistaPrevia : Form
    {
        private readonly wndVistaPrevia _vpControl;

        public frmVistaPrevia(wndVistaPrevia vpControl)
        {
            InitializeComponent();
            _vpControl = vpControl;
        }

        private void frmVistaPrevia_Load(object sender, EventArgs e)
        {
            this.ActiveControl = null;

            _vpControl.SolicitudCerrar += (s, args) => this.Close();

            var host = new ElementHost
            {
                Child = _vpControl,
                Dock = DockStyle.Fill
            };

            this.Controls.Add(host);
            host.BringToFront();
        }
    }
}
