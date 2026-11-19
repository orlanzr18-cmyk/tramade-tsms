using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE
{
    public partial class frmEditarInv : Form
    {
        private readonly int _idProducto;
        private EditarInvControl _control;

        public frmEditarInv(int idProducto)
        {
            InitializeComponent();
            _idProducto = idProducto;
        }

        private void frmEditarInv_Load(object sender, EventArgs e)
        {
            _control = new EditarInvControl(_idProducto);
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