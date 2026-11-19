using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TRAMADE.FormulariosAdministrador;

namespace TRAMADE
{
    public partial class frmNuevo : Form
    {
        public frmNuevo()
        {
            InitializeComponent();

            var wpfControl = new wndNuevoUsuario();

            wpfControl.UsuarioRegistrado += (s, e) =>
            {
                this.Close();
            };

            var host = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = wpfControl
            };

            Controls.Clear();
            Controls.Add(host);
        }
    }
}