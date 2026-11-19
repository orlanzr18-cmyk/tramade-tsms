using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE.FormulariosDeudor
{
    public partial class FrmAgregarPago : Form
    {
        public FrmAgregarPago(int idCliente, string nombreCliente, decimal saldoPendiente)
        {
            InitializeComponent();

            var wpfControl = new wndAgregarPago(
                idCliente,
                nombreCliente,
                saldoPendiente);

            wpfControl.SolicitudCerrar += (s, e) => Close();

            var host = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = wpfControl
            };

            Controls.Clear();
            Controls.Add(host);
        }

        private void FrmAgregarPago_Load(object sender, System.EventArgs e)
        {

        }
    }
}