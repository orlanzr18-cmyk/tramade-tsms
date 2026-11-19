using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace TRAMADE
{
    public partial class RecuperarContrasenaWindow : Window
    {
        clsRecuperacion recuperacion = new clsRecuperacion();

        int idUsuario = 0;
        int idRecuperacionActual = 0;
        string correoUsuario = "";

        public RecuperarContrasenaWindow()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(d1, Caja_Pasting);
            DataObject.AddPastingHandler(d2, Caja_Pasting);
            DataObject.AddPastingHandler(d3, Caja_Pasting);
            DataObject.AddPastingHandler(d4, Caja_Pasting);
            DataObject.AddPastingHandler(d5, Caja_Pasting);
            DataObject.AddPastingHandler(d6, Caja_Pasting);
        }

        private void Caja_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string textoPegado = (string)e.DataObject.GetData(typeof(string));
                DistribuirCodigoPegado(textoPegado);
            }
            e.CancelCommand(); 
        }

        private void MostrarPanel(string panel)
        {
            PanelCorreo.Visibility = Visibility.Collapsed;
            PanelCodigo.Visibility = Visibility.Collapsed;
            PanelPassword.Visibility = Visibility.Collapsed;
            PanelExito.Visibility = Visibility.Collapsed;

            switch (panel)
            {
                case "correo": PanelCorreo.Visibility = Visibility.Visible; break;
                case "codigo": PanelCodigo.Visibility = Visibility.Visible; break;
                case "password": PanelPassword.Visibility = Visibility.Visible; break;
                case "exito": PanelExito.Visibility = Visibility.Visible; break;
            }
        }

        private bool CorreoValido(string correo)
        {
            return Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void BtnEnviarCodigo_Click(object sender, RoutedEventArgs e)
        {
            lblErrorCorreo.Visibility = Visibility.Collapsed;
            string correo = txtCorreo.Text.Trim();

            if (string.IsNullOrWhiteSpace(correo) || !CorreoValido(correo))
            {
                lblErrorCorreo.Text = "Ingresa un correo válido.";
                lblErrorCorreo.Visibility = Visibility.Visible;
                return;
            }

            int? id = recuperacion.ObtenerIdUsuarioPorCorreo(correo);
            if (id == null)
            {
                lblErrorCorreo.Text = "No existe ningún usuario con ese correo.";
                lblErrorCorreo.Visibility = Visibility.Visible;
                return;
            }

            idUsuario = id.Value;
            correoUsuario = correo;

            EnviarNuevoCodigo();
        }

        private void EnviarNuevoCodigo()
        {
            string codigo = recuperacion.GenerarCodigo();
            recuperacion.CrearSolicitud(idUsuario, codigo, minutosExpiracion: 1);

            try
            {
                clsCorreo.EnviarCodigo(correoUsuario, codigo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo enviar el correo: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            lblCorreoEnviado.Text = $"Enviamos un código a {correoUsuario}. Expira en 1 minuto.";
            lblErrorCodigo.Visibility = Visibility.Collapsed;
            MostrarPanel("codigo");
        }

        private void LblReenviar_Click(object sender, MouseButtonEventArgs e)
        {
            EnviarNuevoCodigo();
        }

        private void BtnVerificarCodigo_Click(object sender, RoutedEventArgs e)
        {
            lblErrorCodigo.Visibility = Visibility.Collapsed;
            string codigo = ObtenerCodigoCompleto();

            if (codigo.Length != 6)
            {
                lblErrorCodigo.Text = "El código debe tener 6 dígitos.";
                lblErrorCodigo.Visibility = Visibility.Visible;
                return;
            }

            int idRec;
            int resultado = recuperacion.ValidarCodigo(idUsuario, codigo, out idRec);

            switch (resultado)
            {
                case 0:
                    idRecuperacionActual = idRec;
                    MostrarPanel("password");
                    break;
                case 1:
                    lblErrorCodigo.Text = "Código incorrecto.";
                    lblErrorCodigo.Visibility = Visibility.Visible;
                    break;
                case 2:
                    lblErrorCodigo.Text = "El código expiró. Solicita uno nuevo.";
                    lblErrorCodigo.Visibility = Visibility.Visible;
                    break;
                case 3:
                    lblErrorCodigo.Text = "Demasiados intentos fallidos. Solicita un nuevo código.";
                    lblErrorCodigo.Visibility = Visibility.Visible;
                    break;
                case 4:
                    lblErrorCodigo.Text = "No hay una solicitud activa. Vuelve a empezar.";
                    lblErrorCodigo.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void BtnConfirmarPassword_Click(object sender, RoutedEventArgs e)
        {
            lblErrorPassword.Visibility = Visibility.Collapsed;
            string p1 = txtPassword1.Visibility == Visibility.Visible ? txtPassword1.Password : txtPassword1Visible.Text;
            string p2 = txtPassword2.Visibility == Visibility.Visible ? txtPassword2.Password : txtPassword2Visible.Text;

            if (string.IsNullOrWhiteSpace(p1) || p1.Length < 6)
            {
                lblErrorPassword.Text = "La contraseña debe tener al menos 6 caracteres.";
                lblErrorPassword.Visibility = Visibility.Visible;
                return;
            }

            if (p1 != p2)
            {
                lblErrorPassword.Text = "Las contraseñas no coinciden.";
                lblErrorPassword.Visibility = Visibility.Visible;
                return;
            }

            recuperacion.CambiarPassword(idUsuario, idRecuperacionActual, p1);
            MostrarPanel("exito");
        }

        private void Digito_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox actual = sender as TextBox;
            if (actual.Text.Length > 0)
            {
                // Si pegó varios caracteres de una vez (ej. pegar el código completo)
                if (actual.Text.Length > 1)
                {
                    DistribuirCodigoPegado(actual.Text);
                    return;
                }
                MoveFocus(actual, siguiente: true);
            }
        }

        private void Digito_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox actual = sender as TextBox;
            if (e.Key == Key.Back && string.IsNullOrEmpty(actual.Text))
            {
                MoveFocus(actual, siguiente: false);
            }
        }

        private void MoveFocus(TextBox actual, bool siguiente)
        {
            TextBox[] cajas = { d1, d2, d3, d4, d5, d6 };
            int idx = Array.IndexOf(cajas, actual);
            int destino = siguiente ? idx + 1 : idx - 1;

            if (destino >= 0 && destino < cajas.Length)
            {
                cajas[destino].Focus();
                cajas[destino].SelectAll();
            }
        }

        private void DistribuirCodigoPegado(string texto)
        {
            TextBox[] cajas = { d1, d2, d3, d4, d5, d6 };
            string limpio = new string(texto.Where(char.IsDigit).ToArray());

            for (int i = 0; i < cajas.Length; i++)
                cajas[i].Text = i < limpio.Length ? limpio[i].ToString() : "";

            if (limpio.Length >= 6) d6.Focus();
        }

        private string ObtenerCodigoCompleto()
        {
            return d1.Text + d2.Text + d3.Text + d4.Text + d5.Text + d6.Text;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void ToggleOjo1_Click(object sender, RoutedEventArgs e)
        {
            bool mostrar = btnOjo1.IsChecked == true;
            if (mostrar) txtPassword1Visible.Text = txtPassword1.Password;
            else txtPassword1.Password = txtPassword1Visible.Text;

            txtPassword1.Visibility = mostrar ? Visibility.Collapsed : Visibility.Visible;
            txtPassword1Visible.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ToggleOjo2_Click(object sender, RoutedEventArgs e)
        {
            bool mostrar = btnOjo2.IsChecked == true;
            if (mostrar) txtPassword2Visible.Text = txtPassword2.Password;
            else txtPassword2.Password = txtPassword2Visible.Text;

            txtPassword2.Visibility = mostrar ? Visibility.Collapsed : Visibility.Visible;
            txtPassword2Visible.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}