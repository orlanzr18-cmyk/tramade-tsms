using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace TRAMADE.ClasesProveedor
{
    internal class clsToolTip
    {
        /// <summary>
        /// Clase auxiliar para aplicar ToolTips a controles del formulario de proveedores.
        /// </summary>
        public static void AplicarTooltipsProveedor(
            /// <param name="cmbRazonSocial">Combo que contiene las opciones de razón social.</param>
            KryptonComboBox cmbRazonSocial,
            /// <param name="txtNombreComercial">Textbox para el nombre comercial del proveedor.</param>
            KryptonTextBox txtNombreComercial,
            /// <param name="txtDireccionFiscal">Textbox para la dirección fiscal del proveedor.</param>
            KryptonTextBox txtDireccionFiscal,
            /// <param name="txtRtn">Textbox para el RTN del proveedor.</param>
            KryptonTextBox txtRtn,
            /// <param name="txtTelefonoGeneral">Textbox para el teléfono general del proveedor.</param>
            KryptonTextBox txtTelefonoGeneral,
            /// <param name="txtCorreoCentral">Textbox para el correo central del proveedor.</param>
            KryptonTextBox txtCorreoCentral,
            /// <param name="cmbClasificacion">Combo con la clasificación del proveedor (MADERA/VARIOS).</param>
            KryptonComboBox cmbClasificacion,
            /// <param name="cmbTerminosdePago">Combo con los términos de pago disponibles para el proveedor.</param>
            KryptonComboBox cmbTerminosdePago)
        {
            /// <summary>
            /// Aplica textos de ayuda (ToolTips) a los controles de entrada de datos del formulario de proveedores.
            /// Los ToolTips se muestran al posicionar el cursor sobre el control y ofrecen ejemplos/ayuda breve.
            /// </summary>
            ToolTip tip = new ToolTip();
            tip.AutoPopDelay = 5000;
            tip.InitialDelay = 500;
            tip.ReshowDelay = 200;
            tip.BackColor = Color.FromArgb(210, 180, 140);
            tip.ForeColor = Color.FromArgb(50, 30, 10);

            
            tip.SetToolTip(cmbRazonSocial.Controls[0], "Seleccione el tipo de razón social del proveedor");
            tip.SetToolTip(txtNombreComercial.Controls[0], "Nombre Comercial\nNombre con el que opera el proveedor.\nEj: Maderas del Norte");
            tip.SetToolTip(txtDireccionFiscal.Controls[0], "Dirección Fiscal\nEj: Col. Las Palmas, Tegucigalpa");
            tip.SetToolTip(txtRtn.Controls[0], "RTN\nEj: 05011994000123\n(14 dígitos: municipio + año + secuencial)");
            tip.SetToolTip(txtTelefonoGeneral.Controls[0], "Teléfono General\nEj: 22345678\n(8 dígitos, inicia con 2, 3, 8 o 9)");
            tip.SetToolTip(txtCorreoCentral.Controls[0], "Correo Central\nEj: contacto@maderasnorte.com");
            tip.SetToolTip(cmbClasificacion.Controls[0], "Clasificación\nSeleccione si el proveedor es de MADERA o VARIOS");
            tip.SetToolTip(cmbTerminosdePago.Controls[0], "Términos de Pago\nSeleccione el plazo de pago según la clasificación");
        }
    }
}
