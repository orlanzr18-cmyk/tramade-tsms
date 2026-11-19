using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TRAMADE.Services;

namespace TRAMADE
{
    /// <summary>
    /// Formulario de inicio de sesión de la aplicación.
    /// Ya NO contiene SQL ni reglas de bloqueo/seguridad — esas viven en
    /// <see cref="IAuthService"/> (Services). Esta clase solo recoge la
    /// entrada del usuario, pide el resultado al Service, y traduce ese
    /// resultado a los mismos MessageBox que ya tenías.
    /// </summary>
    public partial class frmLogin : Form
    {
        private readonly IAuthService _authService;
        private readonly IHistorialUsuariosService _historialService;

        private bool _passwordVisible = false;
        private AutoCompleteStringCollection _sugerenciasUsuarios;

        public frmLogin() : this(new AuthService(), new HistorialUsuariosService()) { }

        public frmLogin(IAuthService authService, IHistorialUsuariosService historialService)
        {
            InitializeComponent();
            _authService = authService;
            _historialService = historialService;

            txtUsuario.Text = "Ingrese su usuario";
            txtUsuario.ForeColor = Color.Gray;
            txtPassword.Text = "Ingrese su contraseña";
            txtPassword.ForeColor = Color.Gray;
        }

        private void CargarAutoCompleteUsuarios()
        {
            _sugerenciasUsuarios = new AutoCompleteStringCollection();
            _sugerenciasUsuarios.AddRange(_historialService.ObtenerSugerencias().ToArray());

            txtUsuario.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            txtUsuario.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtUsuario.AutoCompleteCustomSource = _sugerenciasUsuarios;
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            CargarAutoCompleteUsuarios();
        }

        /// <summary>
        /// Intenta autenticar al usuario. Toda la política de seguridad
        /// (bloqueo por intentos, niveles, reintentos de conexión) vive en
        /// AuthService — aquí solo se traduce el resultado a UI.
        /// </summary>
        public async Task login(string usuario, string contraseña)
        {
            LoginResultado resultado = await _authService.IniciarSesionAsync(usuario, contraseña);

            switch (resultado.Estado)
            {
                case LoginEstado.ValidacionFallida:
                    // clsValidar.ContieneEspacios ya mostró su propio mensaje.
                    return;

                case LoginEstado.CuentaInactiva:
                    MessageBox.Show("Tu cuenta está inactiva.\nContacta a un administrador para reactivarla.",
                        "Cuenta bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                case LoginEstado.CuentaBloqueadaTemporal:
                    var restante = resultado.TiempoRestanteBloqueo.Value;
                    MessageBox.Show($"Cuenta bloqueada temporalmente.\nIntenta de nuevo en {restante.Minutes} min {restante.Seconds} seg.",
                        "Acceso bloqueado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;

                case LoginEstado.CuentaBloqueadaAhora:
                    MessageBox.Show($"Has superado el máximo de intentos.\nTu cuenta se bloqueará por {resultado.MinutosBloqueo} minutos.",
                        "Acceso bloqueado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                case LoginEstado.CuentaDesactivada:
                    MessageBox.Show("Has alcanzado el máximo de intentos en repetidas ocasiones.\nTu cuenta ha sido desactivada. Contacta a un administrador.",
                        "Cuenta desactivada", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                case LoginEstado.UsuarioOContraseñaIncorrectos:
                    if (resultado.IntentosRestantes.HasValue)
                        MessageBox.Show($"Usuario o contraseña incorrectos.\nIntentos restantes: {resultado.IntentosRestantes}",
                            "Error de acceso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    else
                        MessageBox.Show("Usuario o contraseña incorrectos.", "Error de acceso",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;

                case LoginEstado.ErrorConexion:
                    MessageBox.Show("Error al conectar: " + resultado.MensajeError);
                    return;

                case LoginEstado.Exitoso:
                    var u = resultado.Usuario;
                    clsSesion sesion = new clsSesion();
                    sesion.setId_usuario(u.IdUsuario);
                    sesion.setNombre_usuario(u.NombreUsuario);
                    sesion.setId_rol(u.IdRol);
                    sesion.setId_sucursal(u.IdSucursal);
                    sesion.setNombre_sucursal(u.NombreSucursal);

                    _historialService.Guardar(usuario);
                    _sugerenciasUsuarios?.Add(usuario);

                    this.Hide();
                    //frmVerificacionFacial frm = new frmVerificacionFacial(usuario);
                    frmMenuPrincipal frm = new frmMenuPrincipal();
                    frm.Show();
                    return;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) { }

        private async void btnIngresar_Click(object sender, EventArgs e)
        {
            if (txtUsuario.Text == "Ingrese su usuario" || txtUsuario.Text == "" ||
                txtPassword.Text == "Ingrese su contraseña" || txtPassword.Text == "")
            {
                MessageBox.Show("Necesita llenar ambos campos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                btnIngresar.Enabled = false; // Evita doble clic mientras carga
                string usuario = txtUsuario.Text.Trim();
                string contra = txtPassword.Text.Trim();
                await login(usuario, contra);
                btnIngresar.Enabled = true;
            }
        }

        private void txtUsuario_Enter_1(object sender, EventArgs e)
        {
            if (txtUsuario.Text == "Ingrese su usuario")
            {
                txtUsuario.Text = "";
                txtUsuario.ForeColor = Color.Black;
            }
        }

        private void txtUsuario_Leave_1(object sender, EventArgs e)
        {
            if (txtUsuario.Text == "")
            {
                txtUsuario.Text = "Ingrese su usuario";
                txtUsuario.ForeColor = Color.Gray;
            }
        }

        private void txtPassword_Enter(object sender, EventArgs e)
        {
            if (txtPassword.Text == "Ingrese su contraseña")
            {
                txtPassword.Text = "";
                txtPassword.ForeColor = Color.Black;
                txtPassword.UseSystemPasswordChar = true;
                _passwordVisible = false;
                lblVerPassword.Text = "👁";
                lblVerPassword.ForeColor = Color.Gray;
            }
        }

        private void txtPassword_Leave(object sender, EventArgs e)
        {
            if (txtPassword.Text == "")
            {
                txtPassword.Text = "Ingrese su contraseña";
                txtPassword.ForeColor = Color.Gray;
                txtPassword.UseSystemPasswordChar = false;
                _passwordVisible = false;
                lblVerPassword.Text = "👁";
                lblVerPassword.ForeColor = Color.Gray;
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("¿Estás seguro de que deseas salir?", "Confirmar salida",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
                Application.Exit();
        }

        private void pictureBox1_Click_1(object sender, EventArgs e) { }

        private void txtUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtPassword.Focus();

            if (e.KeyCode == Keys.Space)
            {
                clsValidar.ContieneEspacios(" ", "Usuario");
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnIngresar.PerformClick();

            if (e.KeyCode == Keys.Space)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }

        private void lblVerPassword_Click(object sender, EventArgs e)
        {
            _passwordVisible = !_passwordVisible;

            if (_passwordVisible)
            {
                txtPassword.UseSystemPasswordChar = false;
                lblVerPassword.Text = "🙈";
                lblVerPassword.ForeColor = Color.Gray;
            }
            else
            {
                txtPassword.UseSystemPasswordChar = true;
                lblVerPassword.Text = "👁";
                lblVerPassword.ForeColor = Color.Gray;
            }
        }

        private void lblVerPassword_MouseEnter(object sender, EventArgs e)
        {
            lblVerPassword.ForeColor = Color.DimGray;
        }

        private void lblVerPassword_MouseLeave(object sender, EventArgs e)
        {
            if (!_passwordVisible)
                lblVerPassword.ForeColor = Color.Gray;
        }

        private void labelBienvenida_Click(object sender, EventArgs e) { }

        private void lkRestablecer_LinkClicked(object sender, EventArgs e)
        {
            RecuperarContrasenaWindow win = new RecuperarContrasenaWindow();
            win.ShowDialog();
        }
    }
}
