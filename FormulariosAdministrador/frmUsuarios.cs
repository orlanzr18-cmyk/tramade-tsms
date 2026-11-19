using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.FormulariosAdministrador;

namespace TRAMADE
{
    public partial class frmUsuarios : Form
    {
        public frmUsuarios()
        {
            InitializeComponent();

            var wpfControl = new ucUsuarios();

            var host = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = wpfControl
            };

            Controls.Clear();
            Controls.Add(host);
        }

        private void frmUsuarios_Load(object sender, System.EventArgs e)
        {

        }
    }
}