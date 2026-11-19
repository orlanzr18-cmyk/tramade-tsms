using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using TRAMADE.ClasesDeudores;

namespace TRAMADE.FormulariosDeudor
{
    public partial class wndAgregarPago : UserControl
    {
        public event EventHandler SolicitudCerrar;
        public event EventHandler PagoRegistrado;

        private readonly clsConexion _conexion = new clsConexion();
        private readonly clsDeudor _objDeudor = new clsDeudor();
        private int _idCliente;
        private decimal _saldoPendiente;
        private bool _actualizandoTexto = false;

        // ═════════════════════════════════════════════════════════════════════
        //  Constructor
        // ═════════════════════════════════════════════════════════════════════
        public wndAgregarPago(int idCliente, string nombreCliente, decimal saldoPendiente)
        {
            InitializeComponent();
            _idCliente = idCliente;
            _saldoPendiente = saldoPendiente;
            lblDeudor.Text = nombreCliente;
            txtSaldoPendiente.Text = "L " + saldoPendiente.ToString("N2");
        }

        // ═════════════════════════════════════════════════════════════════════
        //  TextChanged — formato automático
        // ═════════════════════════════════════════════════════════════════════
        private void txtMontoPagar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_actualizandoTexto) return;
            _actualizandoTexto = true;

            string texto = txtMontoPagar.Text;
            int posicionCaret = txtMontoPagar.CaretIndex;

            // Construir string limpio: solo dígitos y un punto
            string limpio = string.Empty;
            bool tienePunto = false;
            int digitosEnteros = 0;
            int digitosDecimales = 0;

            foreach (char c in texto)
            {
                if (char.IsDigit(c))
                {
                    if (tienePunto)
                    {
                        if (digitosDecimales < 2) { limpio += c; digitosDecimales++; }
                    }
                    else
                    {
                        if (digitosEnteros < 13) { limpio += c; digitosEnteros++; }
                    }
                }
                else if (c == '.' && !tienePunto)
                {
                    limpio += c;
                    tienePunto = true;
                }
                // letras, comas u otros caracteres → ignorados
            }

            // Aplicar formato con comas de miles
            string formateado;
            if (tienePunto)
            {
                string[] partes = limpio.Split('.');
                string entero = FormatearConComas(partes[0]);
                string decimal_ = partes.Length > 1 ? partes[1] : string.Empty;
                formateado = entero + "." + decimal_;
            }
            else
            {
                formateado = FormatearConComas(limpio);
            }

            // Reposicionar caret correctamente tras insertar/quitar comas
            int comasAntes = ContarComas(texto, posicionCaret);
            int comasDespues = ContarComas(formateado, Math.Min(posicionCaret, formateado.Length));
            int nuevaCaret = Math.Max(0, Math.Min(
                posicionCaret + (comasDespues - comasAntes),
                formateado.Length));

            txtMontoPagar.Text = formateado;
            txtMontoPagar.CaretIndex = nuevaCaret;

            _actualizandoTexto = false;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Confirmar pago
        // ═════════════════════════════════════════════════════════════════════
        private void btnConfirmar_Click(object sender, RoutedEventArgs e)
        {
            decimal monto = ObtenerMonto();

            if (monto <= 0)
            {
                MessageBox.Show("Ingrese un monto válido mayor a 0.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (monto > _saldoPendiente)
            {
                MessageBox.Show(
                    "El monto no puede superar el saldo pendiente de L " +
                    _saldoPendiente.ToString("N2"),
                    "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _objDeudor.setIdCliente(_idCliente);
            _objDeudor.setIdUsuario(clsSesion.id_usuario);
            _objDeudor.setMontoPago(monto);

            bool resultado = _objDeudor.registrarPago(_conexion);

            if (resultado)
            {
                MessageBox.Show("Pago registrado correctamente.", "Éxito",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                PagoRegistrado?.Invoke(this, EventArgs.Empty);
                SolicitudCerrar?.Invoke(this, EventArgs.Empty);
            }
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Cancelar
        // ═════════════════════════════════════════════════════════════════════
        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            SolicitudCerrar?.Invoke(this, EventArgs.Empty);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Helpers
        // ═════════════════════════════════════════════════════════════════════
        private decimal ObtenerMonto()
        {
            string limpio = txtMontoPagar.Text.Replace(",", string.Empty);
            return decimal.TryParse(limpio,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal resultado) ? resultado : 0m;
        }

        private string FormatearConComas(string numeroLimpio)
        {
            if (string.IsNullOrEmpty(numeroLimpio)) return string.Empty;
            return long.TryParse(numeroLimpio, out long n)
                ? n.ToString("N0", CultureInfo.InvariantCulture)
                : numeroLimpio;
        }

        private int ContarComas(string texto, int hastaIndice)
        {
            int cuenta = 0;
            int limite = Math.Min(hastaIndice, texto.Length);
            for (int i = 0; i < limite; i++)
                if (texto[i] == ',') cuenta++;
            return cuenta;
        }
    }
}