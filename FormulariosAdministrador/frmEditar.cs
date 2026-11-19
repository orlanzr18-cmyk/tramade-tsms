using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE.FormulariosAdministrador
{
    public partial class frmEditar : Form
    {
        public frmEditar(int id, string nombre, string correo, int idRol, int idSucursal)
        {
            InitializeComponent();

            var wpfControl = new wndEditarUsuario(
                id,
                nombre,
                correo,
                idRol,
                idSucursal);

            wpfControl.UsuarioEditado += (s, e) => Close();

            var host = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = wpfControl
            };

            Controls.Clear();
            Controls.Add(host);
        }

        private void frmEditar_Load(object sender, System.EventArgs e)
        {

        }
    }
}