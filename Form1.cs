using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TRAMADE
{
    public partial class Form1 : Form
    {
        private DashboardControl _dashboard;
        private ElementHost _host;
        private DateTime _ultimaCarga = DateTime.MinValue;
        private static readonly TimeSpan IntervaloMinimoRecarga = TimeSpan.FromSeconds(3);

        public Form1()
        {
            InitializeComponent();
            this.VisibleChanged += async (s, e) =>
            {
                if (!this.Visible || _dashboard == null) return;

                // Evita recargas repetidas si el usuario cambia de pestaña rápido
                if (DateTime.Now - _ultimaCarga < IntervaloMinimoRecarga) return;

                _ultimaCarga = DateTime.Now;
                await _dashboard.CargarAsync();
            };
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            this.ControlBox = false;

            _dashboard = new DashboardControl();

            _dashboard.CardProductosClick += () => NavegacionSolicitada?.Invoke("Inventario");
            _dashboard.CardAgotadosClick += () => NavegacionSolicitada?.Invoke("Inventario");
            _dashboard.CardIngresosClick += () => NavegacionSolicitada?.Invoke("Ventas");
            _dashboard.CardDeudoresClick += () => NavegacionSolicitada?.Invoke("Deudores");

            _host = new ElementHost { Child = _dashboard, Dock = DockStyle.Fill };
            this.Controls.Add(_host);

            this.BeginInvoke((Action)(() =>
            {
                this.BeginInvoke((Action)(() =>
                {
                    _dashboard.InvalidateMeasure();
                    _dashboard.InvalidateArrange();
                    _dashboard.UpdateLayout();
                }));
            }));

            _ultimaCarga = DateTime.Now;
            await _dashboard.CargarAsync();
        }

        public event Action<string> NavegacionSolicitada;

        private void Form1_SizeChanged(object sender, EventArgs e) { }
        private void btnProductos_Click(object sender, EventArgs e) { }
        private void btnVentasRealizadas_Click(object sender, EventArgs e) { }
        private void btnComprasRealizadas_Click(object sender, EventArgs e) { }
        private void btnProductosAgotados_Click(object sender, EventArgs e) { }
        private void dgvCompras_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void btnSalir_Click(object sender, EventArgs e) { Application.Exit(); }
    }
}