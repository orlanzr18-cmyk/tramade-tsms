using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.FormulariosCompras;

namespace TRAMADE
{
    public partial class frmCompras : Form
    {
        public frmCompras()
        {
            InitializeComponent();
            ControlBox = false;

            Controls.Clear();
            Controls.Add(new ElementHost
            {
                Child = new ComprasControl(),
                Dock = DockStyle.Fill,
                Name = "hostComprasWpf"
            });
        }

        private void frmCompras_Load(object sender, System.EventArgs e)
        {

        }
    }
}
