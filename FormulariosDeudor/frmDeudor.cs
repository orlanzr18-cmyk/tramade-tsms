using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE.FormulariosDeudor
{
    public partial class frmDeudor : Form
    {
        public frmDeudor()
        {
            InitializeComponent();

            var host = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = new ucDeudor()
            };

            Controls.Clear();
            Controls.Add(host);
        }
    }
}