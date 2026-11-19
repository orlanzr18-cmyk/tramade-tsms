using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;          // ElementHost
using TRAMADE.Formularios_Login__Menú;
using TRAMADE.Formularios_Login__Menú.Clases;
using TRAMADE.FormulariosDeudor;

namespace TRAMADE
{
    /// <summary>
    /// Ventana principal MDI. El sidebar WinForms original se reemplaza
    /// por un UserControl WPF hospedado dentro de un ElementHost.
    /// </summary>
    public partial class frmMenuPrincipal : Form
    {
        // ── Referencia al sidebar WPF ────────────────────────────────────────
        private SidebarControl _sidebar;

        // ── Constructor ──────────────────────────────────────────────────────
        public frmMenuPrincipal()
        {
            InitializeComponent();
            mdiProp();
            InicializarSidebarWpf();
        }

        // ── MDI helpers ──────────────────────────────────────────────────────
        private void mdiProp()
        {
            this.SetBevel(false);
            Controls.OfType<MdiClient>().FirstOrDefault().BackColor = Color.FromArgb(232, 234, 237);
        }

        private ElementHost _sidebarHost;

        private void CerrarFormulariosHijos()
        {
            foreach (Form hijo in this.MdiChildren)
                hijo.Close();
        }

        private void AbrirFormulario(Form formulario)
        {
            NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETREDRAW, false, 0);

            formulario.ShowInTaskbar = false;
            formulario.FormBorderStyle = FormBorderStyle.None;
            formulario.MinimumSize = Size.Empty;
            formulario.MaximumSize = Size.Empty;
            formulario.AutoSize = false;
            formulario.Text = "";
            formulario.MdiParent = this;

            var mdiClient = Controls.OfType<MdiClient>().FirstOrDefault();
            if (mdiClient != null)
            {
                formulario.Location = new Point(0, 0);
                formulario.Size = mdiClient.ClientSize;
            }

            var formulariosAnteriores = this.MdiChildren.Where(f => f != formulario).ToArray();
            formulario.ShowInTaskbar = false;
            formulario.Show();

            foreach (var anterior in formulariosAnteriores)
                anterior.Hide();

            this.BeginInvoke((Action)(() =>
            {
                RedimensionarHijos();
                NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETREDRAW, true, 0);
                this.Refresh();
            }));
        }

        private void RedimensionarHijos()
        {
            var mdiClient = Controls.OfType<MdiClient>().FirstOrDefault();
            if (mdiClient == null) return;

            foreach (Form hijo in MdiChildren)
            {
                hijo.Location = new Point(0, 0);
                hijo.Size = mdiClient.ClientSize;
            }

        }

        private System.Collections.Generic.Dictionary<string, Form> _formsCache
        = new System.Collections.Generic.Dictionary<string, Form>();

        private Form ObtenerOCrear(string clave, Func<Form> factory)
        {
            if (_formsCache.TryGetValue(clave, out var existente) && !existente.IsDisposed)
                return existente;

            var nuevo = factory();
            _formsCache[clave] = nuevo;
            return nuevo;
        }


        // ── Inicialización del Sidebar WPF ────────────────────────────────────
        /// <summary>
        /// Crea el ElementHost, instancia el SidebarControl WPF y lo posiciona
        /// en el lado izquierdo del formulario, debajo del topbar existente.
        /// El topbar de WinForms (pnlTopBar / lblUsuario / btnMenu) se mantiene.
        /// </summary>
        private void InicializarSidebarWpf()
        {
            _sidebar = new SidebarControl();

            _sidebar.NavegacionSolicitada += OnNavegacion;
            _sidebar.CerrarSesionSolicitada += OnCerrarSesion;
            _sidebar.AnchoSidebarCambiado += OnAnchoSidebarCambiado;

            _sidebarHost = new ElementHost
            {
                Child = _sidebar,
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = Color.FromArgb(74, 51, 37)
            };

            this.Controls.Add(_sidebarHost);
            _sidebarHost.BringToFront();
        }

        private void OnAnchoSidebarCambiado(double nuevoAncho)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)(() => OnAnchoSidebarCambiado(nuevoAncho)));
                return;
            }

            NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETREDRAW, false, 0);

            _sidebarHost.Width = (int)nuevoAncho;
            this.PerformLayout();      // fuerza que el MdiClient recalcule su ClientSize YA
            RedimensionarHijos();      // ← esto es lo que faltaba

            NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETREDRAW, true, 0);
            this.Refresh();
        }

        // ── Eventos del Sidebar ───────────────────────────────────────────────
        private string _moduloActivo = "";

        private void OnNavegacion(string modulo)
        {
            if (modulo == _moduloActivo) return;
            _moduloActivo = modulo;
            _sidebar?.SetModuloActivo(modulo);

            switch (modulo)
            {

                case "Inicio":
                    var dash = (Form1)ObtenerOCrear("Inicio", () =>
                    {
                        var f = new Form1();
                        f.NavegacionSolicitada += (m) => OnNavegacion(m);
                        return f;
                    });
                    AbrirFormulario(dash);
                    break;
                case "Clientes": AbrirFormulario(ObtenerOCrear("Clientes", () => new frmMenu())); break;
                case "Ventas": AbrirFormulario(ObtenerOCrear("Ventas", () => new frmFacturacion())); break;
                case "Compras": AbrirFormulario(ObtenerOCrear("Compras", () => new frmCompras())); break;
                case "Proveedores": AbrirFormulario(ObtenerOCrear("Proveedores", () => new frmProveedores())); break;
                case "Inventario": AbrirFormulario(ObtenerOCrear("Inventario", () => new frmInventario())); break;
                case "Deudores": AbrirFormulario(ObtenerOCrear("Deudores", () => new frmDeudor())); break;

                case "Reportes": AbrirFormulario(ObtenerOCrear("Reportes", () => new frmReportes())); break;

                case "Usuarios": AbrirFormulario(ObtenerOCrear("Usuarios", () => new frmUsuarios())); break;
                case "Bitacora": AbrirFormulario(ObtenerOCrear("Bitacora", () => new frmBitacora())); break;
            }
        }

        private void OnCerrarSesion()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((Action)OnCerrarSesion);
                return;
            }

            var confirm = MessageBox.Show(
                "¿Estás seguro de que deseas cerrar sesión?",
                "Cerrar sesión",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                clsSesion.id_usuario = 0;
                clsSesion.nombre_usuario = string.Empty;

                CerrarFormulariosHijos();

                // CerrarFormulariosHijos() ya cierra (y dispone) cada MdiChild.
                // Limpiamos la caché completa para no retener referencias a
                // formularios ya disposed y para que la próxima sesión abra
                // instancias frescas.
                foreach (var f in _formsCache.Values)
                {
                    if (!f.IsDisposed)
                        f.Dispose();
                }
                _formsCache.Clear();
                _moduloActivo = "";

                _cerrandoPorSesion = true;

                frmLogin login = new frmLogin();
                login.Show();
                this.Close();
            }
        }

        // ── Eventos del Form ──────────────────────────────────────────────────
        private void frmMenuPrincipal_Load(object sender, EventArgs e)
        {
            aplicarPermisos();
            _sidebar?.SetUsuario(clsSesion.nombre_usuario);
            OnNavegacion("Inicio");
        }

        private void aplicarPermisos()
        {
            if (clsPermisos.esEmpleado)
            {
                _sidebar?.OcultarReportes();
                _sidebar?.OcultarUsuarios();
                _sidebar?.OcultarBitacora();
            }
            if (clsPermisos.esGerente)
            {
                _sidebar?.OcultarUsuarios();
                _sidebar?.OcultarBitacora();
            }
        }

        private void frmMenuPrincipal_SizeChanged(object sender, EventArgs e)
        {
            NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETREDRAW, false, 0);

            this.BeginInvoke((Action)(() =>
            {
                RedimensionarHijos();
                NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETREDRAW, true, 0);
                this.Refresh();
            }));
        }

        private bool _confirmadoSalir = false;
        private bool _cerrandoPorSesion = false;

        private void frmMenuPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_confirmadoSalir || _cerrandoPorSesion)
                return; // no preguntar si ya se confirmó o si es cierre de sesión

            var confirm = MessageBox.Show(
                "¿Estás seguro de que deseas salir del programa?",
                "Salir",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            _confirmadoSalir = true;
            Application.Exit();
        }

        private void frmMenuPrincipal_Shown(object sender, EventArgs e) { }

        // ── Handlers de botones WinForms originales (por si el diseñador los requiere) ──
        // Si en tu .designer.cs estos botones ya no existen puedes borrar los métodos.
        private void btnInicio_Click(object sender, EventArgs e) => AbrirFormulario(new Form1());
        private void btnClientes_Click(object sender, EventArgs e) => AbrirFormulario(new frmMenu());
        private void btnVentas_Click(object sender, EventArgs e) => AbrirFormulario(new frmFacturacion());
        private void btnInventario_Click(object sender, EventArgs e) => AbrirFormulario(new frmInventario());
        private void btnCompras_Click(object sender, EventArgs e) => AbrirFormulario(new frmCompras());
        private void btnProveedores_Click(object sender, EventArgs e) => AbrirFormulario(new frmProveedores());
        private void btnReportes_Click(object sender, EventArgs e) => AbrirFormulario(new frmReportes());
        private void btnUsuarios_Click(object sender, EventArgs e) => AbrirFormulario(new frmUsuarios());
        private void btnBitacora_Click(object sender, EventArgs e) => AbrirFormulario(new frmBitacora());
        private void button1_Click(object sender, EventArgs e) => AbrirFormulario(new frmDeudor());
        private void btnCerrarSesion_Click(object sender, EventArgs e) => OnCerrarSesion();
    }
}