using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE
{
    /// <summary>
    /// Contiene métodos estáticos de validación y utilidades para controles y datos
    /// usados en formularios (clientes, proveedores, inventario, facturación, etc.).
    /// Proporciona validaciones de campos, formatos (RTN, DNI, teléfono, correo),
    /// comprobaciones en base de datos y helpers para eventos KeyPress.
    /// </summary>
    internal class clsValidar
    {
        // ─── LIMPIAR CONTROL AL FALLAR ────────────────────────────────
        /// <summary>
        /// Limpia y pone el foco en el control indicado. Maneja <see cref="TextBox"/> y <see cref="KryptonTextBox"/>.
        /// </summary>
        /// <param name="campo">Control a limpiar y enfocar.</param>
        private static void LimpiarControl(Control campo)
        {
            if (campo == null) return;
            if (campo is TextBox tb) { tb.Clear(); tb.Focus(); }
            else if (campo is Krypton.Toolkit.KryptonTextBox ktb) { ktb.Text = ""; ktb.Focus(); }
        }

        // ─── VALIDAR CAMPO VACÍO ──────────────────────────────────────
        /// <summary>
        /// Valida que una cadena no sea nula, vacía o compuesta solo por espacios.
        /// </summary>
        /// <param name="valor">Valor a validar.</param>
        /// <param name="nombreCampo">Nombre del campo para mensajes al usuario.</param>
        /// <param name="campo">Control opcional asociado (no se modifica aquí).</param>
        /// <returns>True si el valor no está vacío; false en caso contrario.</returns>
        public static bool NullOVacio(string valor, string nombreCampo, Control campo = null)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                MessageBox.Show($"El campo '{nombreCampo}' no puede estar vacío.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── VALIDAR LONGITUD MÍNIMA ──────────────────────────────────
        /// <summary>
        /// Verifica que la longitud de la cadena sea al menos el mínimo indicado.
        /// </summary>
        /// <param name="valor">Valor a validar.</param>
        /// <param name="nombreCampo">Nombre del campo para mensajes.</param>
        /// <param name="minimo">Longitud mínima permitida.</param>
        /// <param name="campo">Control opcional asociado.</param>
        /// <returns>True si cumple la longitud mínima; false en caso contrario.</returns>
        public static bool LongitudMinima(string valor, string nombreCampo, int minimo, Control campo = null)
        {
            if (valor.Trim().Length < minimo)
            {
                MessageBox.Show($"El campo '{nombreCampo}' debe tener al menos {minimo} caracteres.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── VALIDAR LONGITUD MÁXIMA ──────────────────────────────────
        /// <summary>
        /// Verifica que la longitud de la cadena no exceda el máximo indicado.
        /// </summary>
        /// <param name="valor">Valor a validar.</param>
        /// <param name="nombreCampo">Nombre del campo para mensajes.</param>
        /// <param name="maximo">Longitud máxima permitida.</param>
        /// <param name="campo">Control opcional asociado.</param>
        /// <returns>True si cumple la longitud máxima; false en caso contrario.</returns>
        public static bool LongitudMaxima(string valor, string nombreCampo, int maximo, Control campo = null)
        {
            if (valor.Trim().Length > maximo)
            {
                MessageBox.Show($"El campo '{nombreCampo}' no puede tener más de {maximo} caracteres.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private static readonly HashSet<int> municipiosValidos = new HashSet<int>
        {
            // ATLÁNTIDA
            0101, 0102, 0103, 0104, 0105, 0106, 0107, 0108,
            // CHOLUTECA
            0201, 0202, 0203, 0204, 0205, 0206, 0207, 0208, 0209, 0210, 0211, 0212, 0213, 0214, 0215, 0216,
            // COLÓN
            0301, 0302, 0303, 0304, 0305, 0306, 0307, 0308, 0309, 0310,
            // COMAYAGUA
            0401, 0402, 0403, 0404, 0405, 0406, 0407, 0408, 0409, 0410, 0411, 0412, 0413, 0414, 0415, 0416, 0417,
            // COPÁN
            0501, 0502, 0503, 0504, 0505, 0506, 0507, 0508, 0509, 0510, 0511, 0512, 0513, 0514, 0515, 0516, 0517, 0518, 0519, 0520, 0521, 0522, 0523,
            // CORTÉS
            0601, 0602, 0603, 0604, 0605, 0606, 0607, 0608, 0609, 0610, 0611, 0612,
            // EL PARAÍSO
            0701, 0702, 0703, 0704, 0705, 0706, 0707, 0708, 0709, 0710, 0711, 0712, 0713, 0714, 0715, 0716, 0717, 0718, 0719,
            // FRANCISCO MORAZÁN
            0801, 0802, 0803, 0804, 0805, 0806, 0807, 0808, 0809, 0810, 0811, 0812, 0813, 0814, 0815, 0816, 0817, 0818, 0819, 0820, 0821, 0822, 0823, 0824, 0825, 0826, 0827, 0828,
            // GRACIAS A DIOS
            0901, 0902, 0903, 0904, 0905, 0906,
            // INTIBUCÁ
            1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013, 1014, 1015, 1016, 1017,
            // ISLAS DE LA BAHÍA
            1101, 1102, 1103, 1104,
            // LA PAZ
            1201, 1202, 1203, 1204, 1205, 1206, 1207, 1208, 1209, 1210, 1211, 1212, 1213, 1214, 1215, 1216, 1217, 1218, 1219,
            // LEMPIRA
            1301, 1302, 1303, 1304, 1305, 1306, 1307, 1308, 1309, 1310, 1311, 1312, 1313, 1314, 1315, 1316, 1317, 1318, 1319,
            // OCOTEPEQUE
            1401, 1402, 1403, 1404, 1405, 1406, 1407, 1408, 1409, 1410, 1411, 1412, 1413, 1414, 1415, 1416,
            // OLANCHO
            1501, 1502, 1503, 1504, 1505, 1506, 1507, 1508, 1509, 1510, 1511, 1512, 1513, 1514, 1515, 1516, 1517, 1518, 1519, 1520, 1521, 1522, 1523,
            // SANTA BÁRBARA
            1601, 1602, 1603, 1604, 1605, 1606, 1607, 1608, 1609, 1610, 1611, 1612, 1613, 1614, 1615, 1616, 1617, 1618, 1619, 1620, 1621, 1622, 1623, 1624, 1625, 1626, 1627, 1628,
            // VALLE
            1701, 1702, 1703, 1704, 1705, 1706, 1707, 1708, 1709,
            // YORO
            1801, 1802, 1803, 1804, 1805, 1806, 1807, 1808, 1809, 1810, 1811
        };

        // ─── VALIDAR RTN (14 dígitos, formato específico) ───────────────────────────────
        /// <summary>
        /// RTNs the specified valor.
        /// </summary>
        /// <param name="valor">The valor.</param>
        /// <param name="campo">The campo.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool RTN(string valor, Control campo = null)
        {
            HashSet<int> municipiosValidos = new HashSet<int>
            {
                // ATLÁNTIDA
                0101, 0102, 0103, 0104, 0105, 0106, 0107, 0108,
                // CHOLUTECA
                0201, 0202, 0203, 0204, 0205, 0206, 0207, 0208, 0209, 0210, 0211, 0212, 0213, 0214, 0215, 0216,
                // COLÓN
                0301, 0302, 0303, 0304, 0305, 0306, 0307, 0308, 0309, 0310,
                // COMAYAGUA
                0401, 0402, 0403, 0404, 0405, 0406, 0407, 0408, 0409, 0410, 0411, 0412, 0413, 0414, 0415, 0416, 0417,
                // COPÁN
                0501, 0502, 0503, 0504, 0505, 0506, 0507, 0508, 0509, 0510, 0511, 0512, 0513, 0514, 0515, 0516, 0517, 0518, 0519, 0520, 0521, 0522, 0523,
                // CORTÉS
                0601, 0602, 0603, 0604, 0605, 0606, 0607, 0608, 0609, 0610, 0611, 0612,
                // EL PARAÍSO
                0701, 0702, 0703, 0704, 0705, 0706, 0707, 0708, 0709, 0710, 0711, 0712, 0713, 0714, 0715, 0716, 0717, 0718, 0719,
                // FRANCISCO MORAZÁN
                0801, 0802, 0803, 0804, 0805, 0806, 0807, 0808, 0809, 0810, 0811, 0812, 0813, 0814, 0815, 0816, 0817, 0818, 0819, 0820, 0821, 0822, 0823, 0824, 0825, 0826, 0827, 0828,
                // GRACIAS A DIOS
                0901, 0902, 0903, 0904, 0905, 0906,
                // INTIBUCÁ
                1001, 1002, 1003, 1004, 1005, 1006, 1007, 1008, 1009, 1010, 1011, 1012, 1013, 1014, 1015, 1016, 1017,
                // ISLAS DE LA BAHÍA
                1101, 1102, 1103, 1104,
                // LA PAZ
                1201, 1202, 1203, 1204, 1205, 1206, 1207, 1208, 1209, 1210, 1211, 1212, 1213, 1214, 1215, 1216, 1217, 1218, 1219,
                // LEMPIRA
                1301, 1302, 1303, 1304, 1305, 1306, 1307, 1308, 1309, 1310, 1311, 1312, 1313, 1314, 1315, 1316, 1317, 1318, 1319,
                // OCOTEPEQUE
                1401, 1402, 1403, 1404, 1405, 1406, 1407, 1408, 1409, 1410, 1411, 1412, 1413, 1414, 1415, 1416,
                // OLANCHO
                1501, 1502, 1503, 1504, 1505, 1506, 1507, 1508, 1509, 1510, 1511, 1512, 1513, 1514, 1515, 1516, 1517, 1518, 1519, 1520, 1521, 1522, 1523,
                // SANTA BÁRBARA
                1601, 1602, 1603, 1604, 1605, 1606, 1607, 1608, 1609, 1610, 1611, 1612, 1613, 1614, 1615, 1616, 1617, 1618, 1619, 1620, 1621, 1622, 1623, 1624, 1625, 1626, 1627, 1628,
                // VALLE
                1701, 1702, 1703, 1704, 1705, 1706, 1707, 1708, 1709,
                // YORO
                1801, 1802, 1803, 1804, 1805, 1806, 1807, 1808, 1809, 1810, 1811
            };
            string rtn = valor.Trim();

            // ─── Solo números y exactamente 14 dígitos ────────────────
            if (!Regex.IsMatch(rtn, @"^\d{14}$"))
            {
                MessageBox.Show("El RTN debe contener solo números y tener exactamente 14 dígitos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // ─── Código de municipio válido de Honduras ───────────────
            int codigoMunicipio = Convert.ToInt32(rtn.Substring(0, 4));

            if (!municipiosValidos.Contains(codigoMunicipio))
            {
                MessageBox.Show("Los primeros 4 dígitos del RTN no corresponden a un municipio válido de Honduras.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // ─── Año de registro: entre 1900 y hace al menos 18 años ──
            int anioRegistro = Convert.ToInt32(rtn.Substring(4, 4));
            int anioActual = DateTime.Now.Year;
            if (anioRegistro < 1900 || anioRegistro > anioActual)
            {
                MessageBox.Show($"Los dígitos 5 al 8 del RTN deben ser un año válido entre 1900 y {anioActual}.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (anioActual - anioRegistro < 18)
            {
                MessageBox.Show("El RTN debe corresponder a una empresa registrada hace al menos 18 años.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // ─── Dígitos 9-13: correlativo no puede ser 00000 ─────────
            string correlativo = rtn.Substring(8, 5);
            if (correlativo == "00000")
            {
                MessageBox.Show("Los dígitos 9 al 13 del RTN (correlativo) no pueden ser todos ceros.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }


        // ─── BLOQUEAR TECLAS EN RTN AL LLEGAR A 14 DÍGITOS ───────────        
        public static void RtnMaximo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is Krypton.Toolkit.KryptonTextBox ktb)
            {
                if (!char.IsControl(e.KeyChar) && ktb.Text.Length >= 14)
                    e.Handled = true;
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            }
        }

        // ─── VALIDAR TELÉFONO (inicia con 2, 3, 8 o 9 — exactamente 8 dígitos) ──
        /// <summary>
        /// Valida que el teléfono tenga exactamente 8 dígitos y comience con 2, 3, 8 o 9.
        /// </summary>
        /// <param name="valor">Cadena con el teléfono.</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si el teléfono es válido; false en caso contrario.</returns>
        public static bool Telefono(string valor, Control campo = null)
        {
            string tel = valor.Trim();

            if (!Regex.IsMatch(tel, @"^\d{8}$"))
            {
                MessageBox.Show("El teléfono debe contener solo números y tener exactamente 8 dígitos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(tel, @"^[2389]"))
            {
                MessageBox.Show("El teléfono debe iniciar con 2, 3, 8 o 9.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        // ─── VALIDAR CORREO ELECTRÓNICO ───────────────────────────────
        /// <summary>
        /// Valida formato y dominio permitido del correo electrónico.
        /// Acepta dominios gmail, outlook, yahoo y hotmail con extensin .com y longitud del usuario entre 6 y 30.
        /// </summary>
        /// <param name="valor">Correo a validar.</param>
        /// <param name="campo">Control asociado (se enfoca si hay error).</param>
        /// <returns>True si el correo cumple el patrón; false en caso contrario.</returns>
        public static bool Correo(string valor, Control campo = null)
        {
            string correo = valor.Trim().ToLower();

            // ─── Formato general ──────────────────────────────────────
            string patron = @"^[a-zA-Z0-9._%+-]{6,30}@(gmail|outlook|yahoo|hotmail)\.com$";
            if (!Regex.IsMatch(correo, patron))
            {
                MessageBox.Show("El correo debe tener entre 6 y 30 caracteres antes del @ y ser de un dominio permitido (Gmail, Outlook, Yahoo o Hotmail).",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (campo != null) campo.Focus();
                return false;
            }

            // ─── Parte antes del @ ────────────────────────────────────
            string antesDelArroba = correo.Split('@')[0];

            // Sin tres caracteres iguales seguidos
            if (Regex.IsMatch(antesDelArroba, @"(.)\1\1"))
            {
                MessageBox.Show("El correo no puede contener tres caracteres iguales seguidos antes del @.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (campo != null) campo.Focus();
                return false;
            }

            // Sin solo números antes del @
            if (Regex.IsMatch(antesDelArroba, @"^\d+$"))
            {
                MessageBox.Show("El correo no puede contener solo números antes del @.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (campo != null) campo.Focus();
                return false;
            }

            // Sin iniciar o terminar con punto, guion o guion bajo
            if (Regex.IsMatch(antesDelArroba, @"^[.\-_]|[.\-_]$"))
            {
                MessageBox.Show("El correo no puede iniciar ni terminar con punto, guion o guion bajo antes del @.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (campo != null) campo.Focus();
                return false;
            }

            return true;
        }

        // ─── VALIDAR SOLO CARACTERES DE CORREO EN TIEMPO REAL (WPF) ───
        /// <summary>
        /// Manejador para PreviewTextInput (WPF) que permite solo letras, números
        /// y los símbolos válidos en un correo electrónico (@, ., _, -) mientras
        /// el usuario escribe. No reemplaza la validación de formato completo,
        /// ver <see cref="Correo"/>, que se ejecuta al guardar.
        /// </summary>
        /// <param name="e">Argumentos del evento PreviewTextInput.</param>
        /// <returns>True si el carácter es permitido; false si se bloquea.</returns>
        public static bool SoloCaracteresCorreo_PreviewTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!(char.IsLetterOrDigit(c) || c == '@' || c == '.' || c == '_' || c == '-'))
                {
                    e.Handled = true;
                    return false;
                }
            }
            return true;
        }

        // ─── VALIDAR COMBO SELECCIONADO ───────────────────────────────
        /// <summary>
        /// Valida que el valor seleccionado en un combo sea mayor que cero.
        /// </summary>
        /// <param name="selectedValue">Valor seleccionado (normalmente SelectedIndex o SelectedValue).</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje.</param>
        /// <returns>True si hay selección válida; false en caso contrario.</returns>
        public static bool ComboSeleccionado(int selectedValue, string nombreCampo)
        {
            if (selectedValue <= 0)
            {
                MessageBox.Show($"Debe seleccionar una opción en '{nombreCampo}'.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── VALIDAR RTN DUPLICADO ────────────────────────────────────
        /// <summary>
        /// Comprueba en la base de datos si el RTN ya está registrado en otro proveedor.
        /// </summary>
        /// <param name="rtn">RTN a comprobar.</param>
        /// <param name="idProveedorActual">Id del proveedor actual para excluirlo de la comprobación (opcional).</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si no existe duplicado; false si ya existe o ocurre un error.</returns>
        public static bool RTNDuplicado(string rtn, int idProveedorActual = 0, Control campo = null)
        {
            clsConexion ObjConexion = new clsConexion();
            try
            {
                ObjConexion.Abrir();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM PROVEEDOR 
                    WHERE rtn_proveedor = @rtn 
                    AND id_proveedor != @idProveedorActual", ObjConexion.SqlC);
                cmd.Parameters.AddWithValue("@rtn", rtn.Trim());
                cmd.Parameters.AddWithValue("@idProveedorActual", idProveedorActual);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("El RTN ingresado ya está registrado en otro proveedor.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al validar RTN: " + ex.Message);
                return false;
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }

        // ─── VALIDAR CORREO DUPLICADO ─────────────────────────────────
        /// <summary>
        /// Comprueba si un correo ya está registrado en otro proveedor en la base de datos.
        /// </summary>
        /// <param name="correo">Correo a comprobar.</param>
        /// <param name="idProveedorActual">Id del proveedor actual para excluirlo (opcional).</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si no existe duplicado; false si ya existe o hay error.</returns>
        public static bool CorreoDuplicado(string correo, int idProveedorActual = 0, Control campo = null)
        {
            clsConexion ObjConexion = new clsConexion();
            try
            {
                ObjConexion.Abrir();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM PROVEEDOR 
                    WHERE correo_electronico_proveedor = @correo 
                    AND id_proveedor != @idProveedorActual", ObjConexion.SqlC);
                cmd.Parameters.AddWithValue("@correo", correo.Trim());
                cmd.Parameters.AddWithValue("@idProveedorActual", idProveedorActual);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("El correo ingresado ya está registrado en otro proveedor.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al validar correo: " + ex.Message);
                return false;
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }

        // ─── VALIDAR COMPATIBILIDAD CLASIFICACIÓN + TÉRMINOS DE PAGO ──
        /// <summary>
        /// Verifica la compatibilidad entre la clasificación del proveedor y los términos de pago seleccionados.
        /// </summary>
        /// <param name="idClasificacion">Id de clasificación.</param>
        /// <param name="idTerminos">Id de términos de pago.</param>
        /// <returns>True si la combinación es válida; false en caso contrario.</returns>
        public static bool ClasificacionTerminos(int idClasificacion, int idTerminos)
        {
            if (idClasificacion == 1 && idTerminos != 1 && idTerminos != 4)
            {
                MessageBox.Show(
                    "La clasificación MADERA solo permite los términos:\n• CREDITO MADERA\n• AL CONTADO",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (idClasificacion == 2 && idTerminos != 2 && idTerminos != 3 && idTerminos != 4)
            {
                MessageBox.Show(
                    "La clasificación VARIOS solo permite los términos:\n• CREDITO VARIOS 15 DIAS\n• CREDITO VARIOS 1 MES\n• AL CONTADO",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── VALIDAR DOBLE ESPACIO ────────────────────────────────────
        /// <summary>
        /// Valida que el texto no contenga espacios dobles consecutivos.
        /// </summary>
        /// <param name="valor">Texto a validar.</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje.</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si no hay doble espacio; false en caso contrario.</returns>
        public static bool SinDobleEspacio(string valor, string nombreCampo, Control campo = null)
        {
            if (valor.Contains("  "))
            {
                MessageBox.Show($"El campo '{nombreCampo}' no puede contener espacios dobles.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── VALIDAR SIN ESPACIOS AL INICIO O FINAL ───────────────────
        /// <summary>
        /// Valida que el texto no tenga espacios al inicio ni al final.
        /// </summary>
        /// <param name="valor">Texto a validar.</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje.</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si no tiene espacios extremos; false en caso contrario.</returns>
        public static bool SinEspaciosExtremos(string valor, string nombreCampo, Control campo = null)
        {
            if (valor != valor.Trim())
            {
                MessageBox.Show($"El campo '{nombreCampo}' no puede iniciar ni terminar con espacios.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── VALIDAR SIN SOLO CARACTERES ESPECIALES ───────────────────
        /// <summary>
        /// Valida que el texto no esté compuesto únicamente por caracteres especiales.
        /// </summary>
        /// <param name="valor">Texto a validar.</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje.</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si contiene caracteres válidos además de especiales; false en caso contrario.</returns>
        public static bool SinSoloEspeciales(string valor, string nombreCampo, Control campo = null)
        {
            if (!Regex.IsMatch(valor.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$"))
            {
                MessageBox.Show($"El campo '{nombreCampo}' solo puede contener letras y espacios.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (campo != null) campo.Focus();
                return false;
            }
            return true;
        }

        // ─── LIMPIAR ESPACIOS ─────────────────────────────────────────
        /// <summary>
        /// Retorna la cadena sin espacios extremos o cadena vacía si es nula.
        /// </summary>
        /// <param name="valor">Texto a limpiar.</param>
        /// <returns>Texto recortado o cadena vacía.</returns>
        public static string Limpiar(string valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        // ─── VALIDAR SOLO NÚMEROS EN TXT ─────────────────────────
        /// <summary>
        /// Manejador para KeyPress que permite solo dígitos y teclas de control.
        /// </summary>
        /// <param name="e">Argumentos del evento KeyPress.</param>
        /// <returns>True si el carácter es permitido; false si se bloquea.</returns>
        public static bool SoloNumeros_KeyPress(KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return false;
            }
            return true;
        }
        // ─── VALIDAR FORZAR MINUSCULAS ─────────────────────────
        /// <summary>
        /// Convierte la tecla presionada a minúscula en eventos KeyPress.
        /// </summary>
        /// <param name="e">Argumentos del evento KeyPress.</param>
        public static void ForzarMinusculas_KeyPress(KeyPressEventArgs e)
        {

            e.KeyChar = char.ToLower(e.KeyChar);
        }

        // ─── VALIDAR DNI (13 DÍGITOS) ────────────────────────────────
        /// <summary>
        /// Valida que el DNI tenga 13 dígitos numéricos y cumpla reglas básicas de estructura.
        /// </summary>
        /// <param name="valor">DNI a validar.</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si el DNI es válido; false en caso contrario.</returns>
        public static bool DNI(string valor, Control campo = null)
        {
            string dni = valor.Trim();

            // Solo números y exactamente 13 dígitos
            if (!Regex.IsMatch(dni, @"^\d{13}$"))
            {
                MessageBox.Show("El DNI debe contener solo números y tener exactamente 13 dígitos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            // Primeros 4 dígitos: código municipio Honduras
            int codigoMunicipio = Convert.ToInt32(dni.Substring(0, 4));
            if (!municipiosValidos.Contains(codigoMunicipio))
            {
                MessageBox.Show("Los primeros 4 dígitos del DNI no corresponden a un municipio válido de Honduras.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            // Dígitos 5-8: año de registro (1900 al año actual)
            int anioRegistro = Convert.ToInt32(dni.Substring(4, 4));
            int anioActual = DateTime.Now.Year;
            if (anioRegistro < 1900 || anioRegistro > anioActual)
            {
                MessageBox.Show($"Los dígitos 5 al 8 del DNI deben ser un año válido entre 1900 y {anioActual}.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            if (anioActual - anioRegistro < 18)
            {
                MessageBox.Show("El DNI debe corresponder a una persona mayor de 18 años.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            // Últimos 6 dígitos: secuencial, no puede ser 000000
            string secuencial = dni.Substring(8, 5);
            if (secuencial == "000000")
            {
                MessageBox.Show("Los últimos 5 dígitos del DNI no pueden ser todos ceros.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return false;
            }

            return true;
        }

        // ─── VALIDAR SIN TRES LETRAS IGUALES SEGUIDAS ─────────────────
        /// <summary>
        /// Verifica que no existan tres letras iguales consecutivas en la cadena.
        /// </summary>
        /// <param name="valor">Texto a validar.</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje.</param>
        /// <returns>True si no existe la repetición; false en caso contrario.</returns>
        public static bool SinTresLetrasIguales(string valor, string nombreCampo)
        {
            if (Regex.IsMatch(valor.Trim(), @"(.)\1\1"))
            {
                MessageBox.Show($"El campo '{nombreCampo}' contiene caracteres repetidos inválidos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── BLOQUEAR TECLAS EN TELÉFONO AL LLEGAR A 8 DÍGITOS ───────
        /// <summary>
        /// Manejador KeyPress para controlar la longitud máxima y permitir solo dígitos en KryptonTextBox de teléfono.
        /// </summary>
        /// <param name="sender">Control emisor (se espera KryptonTextBox).</param>
        /// <param name="e">Argumentos del evento KeyPress.</param>
        public static void TelefonoMaximo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (sender is Krypton.Toolkit.KryptonTextBox ktb)
            {
                if (!char.IsControl(e.KeyChar) && ktb.Text.Length >= 8)
                    e.Handled = true;
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            }
        }

        // ─── VALIDAR SIN CUATRO NÚMEROS IGUALES SEGUIDOS ──────────────
        /// <summary>
        /// Verifica que no existan cuatro dígitos iguales consecutivos en el texto.
        /// </summary>
        /// <param name="valor">Texto a validar.</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje.</param>
        /// <returns>True si no contiene la secuencia; false en caso contrario.</returns>
        public static bool SinCuatroNumerosIguales(string valor, string nombreCampo)
        {
            if (Regex.IsMatch(valor.Trim(), @"(\d)\1{3}"))
            {
                MessageBox.Show($"El campo '{nombreCampo}' no puede contener cuatro números iguales seguidos.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
        // ─── VALIDAR EXISTENCIA DE DATO DUPLICADO EN LA BASE DE DATOS (Clientes) ─────
        /// <summary>
        /// Comprueba si existe un valor duplicado en la tabla CLIENTE para columnas permitidas.
        /// </summary>
        /// <param name="valor">Valor a comprobar.</param>
        /// <param name="columna">Nombre de la columna (solo columnas permitidas).</param>
        /// <param name="mensaje">Texto que describe el dato para el mensaje al usuario.</param>
        /// <param name="idActual">Id actual para excluir de la búsqueda (opcional).</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si no existe duplicado; false si existe.</returns>
        public static bool ExisteDatoDuplicado(string valor, string columna, string mensaje, int idActual = 0, Control campo = null)
        {
            var columnasPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "dni_cliente", "rtn_cliente", "correo_electronico_cliente"
                };

            if (!columnasPermitidas.Contains(columna))
                throw new ArgumentException("Nombre de columna no permitido.", nameof(columna));

            clsConexion ObjConexion = new clsConexion();
            try
            {
                ObjConexion.Abrir();
                string sql = $"SELECT COUNT(*) FROM CLIENTE WHERE {columna} = @valor";

                if (idActual > 0)
                {
                    sql += " AND id_cliente <> @idActual";
                }

                SqlCommand cmd = new SqlCommand(sql, ObjConexion.SqlC);
                cmd.Parameters.AddWithValue("@valor", valor.Trim());
                if (idActual > 0)
                {
                    cmd.Parameters.AddWithValue("@idActual", idActual);
                }

                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                {
                    MessageBox.Show($"El {mensaje} ya está registrado.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return false;
                }
                return true;


            }
            finally { ObjConexion.Cerrar(); }
        }

        /// <summary>
        /// Valida que el texto no contenga dígitos numéricos.
        /// </summary>
        /// <param name="valor">Texto a validar.</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje.</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si no contiene números o el texto está vacío; false si contiene números.</returns>
        public static bool SinNumeros(string valor, string nombreCampo, Control campo = null)
        {
            if (string.IsNullOrWhiteSpace(valor)) return true;
            if (System.Text.RegularExpressions.Regex.IsMatch(valor.Trim(), @"\d"))
            {
                MessageBox.Show($"El campo '{nombreCampo}' no puede contener números.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (campo != null) campo.Focus();
                return false;
            }
            return true;
        }


        // ─── VALIDAR DATOS DE CONTACTO COMUNES PARA CLIENTES Y PROVEEDORES ───────────────────────────────
        /// <summary>
        /// Valida un conjunto básico de datos de contacto: nombre, dirección, teléfono y correo.
        /// Utilizado como base para validaciones de clientes y proveedores.
        /// </summary>
        /// <param name="nombre">Nombre/comercial a validar.</param>
        /// <param name="cNombre">Control asociado al nombre.</param>
        /// <param name="etiquetaNombre">Etiqueta para mensajes del nombre.</param>
        /// <param name="direccion">Dirección a validar.</param>
        /// <param name="cDir">Control asociado a la dirección.</param>
        /// <param name="tel">Teléfono a validar.</param>
        /// <param name="cTel">Control asociado al teléfono.</param>
        /// <param name="correo">Correo a validar.</param>
        /// <param name="cCorreo">Control asociado al correo.</param>
        /// <returns>True si todos los checks pasan; false en caso contrario.</returns>
        private static bool ValidarDatosContactoBase(
        string nombre, Control cNombre, string etiquetaNombre,
        string direccion, Control cDir,
        string tel, Control cTel,
        string correo, Control cCorreo)
        {

            if (!NullOVacio(nombre, etiquetaNombre, cNombre)) return false;
            if (!SinEspaciosExtremos(nombre, etiquetaNombre, cNombre)) return false;
            if (!SinDobleEspacio(nombre, etiquetaNombre, cNombre)) return false;
            if (!SinSoloEspeciales(nombre, etiquetaNombre, cNombre)) return false;
            if (!SinTresLetrasIguales(nombre, etiquetaNombre)) return false; 
            if (!LongitudMinima(nombre, etiquetaNombre, 3, cNombre)) return false;
            if (!LongitudMaxima(nombre, etiquetaNombre, 50, cNombre)) return false;
            if (!SinNumeros(nombre, etiquetaNombre, cNombre)) return false;

            // --- Validación de Dirección ---
            if (!NullOVacio(direccion, "Dirección", cDir)) return false;
            if (!SinEspaciosExtremos(direccion, "Dirección", cDir)) return false;
            if (!SinDobleEspacio(direccion, "Dirección", cDir)) return false;
            if (!SinTresLetrasIguales(direccion, "Dirección")) return false; 
            if (!LongitudMinima(direccion, "Dirección", 5, cDir)) return false;

            // --- Validación de Teléfono ---
            if (!NullOVacio(tel, "Teléfono", cTel)) return false;
            if (!Telefono(tel, cTel)) return false;
            if (!SinCuatroNumerosIguales(tel, "Teléfono")) return false;

            // --- Validación de Correo ---
            if (!NullOVacio(correo, "Correo Electrónico", cCorreo)) return false;
            if (!SinEspaciosExtremos(correo, "Correo Electrónico", cCorreo)) return false;
            if (!Correo(correo, cCorreo)) return false;

            return true;
        }

        // ─── VALIDAR PROVEEDOR COMPLETO ───────────────────────────────
        /// <summary>
        /// Valida todos los campos necesarios para crear o editar un proveedor.
        /// Realiza validaciones de contacto, RTN, duplicados, y combinaciones de clasificación/términos.
        /// </summary>
        /// <param name="nombre">Nombre comercial.</param>
        /// <param name="ctrlNombre">Control asociado al nombre.</param>
        /// <param name="razon">Razón social seleccionada.</param>
        /// <param name="ctrlRazon">Control asociado a la razón social.</param>
        /// <param name="direccion">Dirección fiscal.</param>
        /// <param name="ctrlDireccion">Control de dirección.</param>
        /// <param name="rtn">RTN del proveedor.</param>
        /// <param name="ctrlRtn">Control asociado al RTN.</param>
        /// <param name="telefono">Teléfono principal.</param>
        /// <param name="ctrlTelefono">Control del teléfono.</param>
        /// <param name="correo">Correo electrónico.</param>
        /// <param name="ctrlCorreo">Control del correo.</param>
        /// <param name="idClasificacion">Id de la clasificación seleccionada.</param>
        /// <param name="idTerminos">Id de los términos de pago seleccionados.</param>
        /// <param name="idProveedorActual">Id del proveedor actual para excepciones de duplicado (opcional).</param>
        /// <returns>True si todas las validaciones pasan; false en caso contrario.</returns>
        public static bool ValidarProveedor(
        string nombre, Control ctrlNombre, string razon, Control ctrlRazon,
        string direccion, Control ctrlDireccion, string rtn, Control ctrlRtn,
        string telefono, Control ctrlTelefono, string correo, Control ctrlCorreo,
        int idClasificacion, int idTerminos, int idProveedorActual = 0)
        {

            if (!ValidarDatosContactoBase(nombre, ctrlNombre, "Nombre Comercial", direccion, ctrlDireccion, telefono, ctrlTelefono, correo, ctrlCorreo))
                return false;


            if (string.IsNullOrWhiteSpace(razon))
            {
                MessageBox.Show("Debe seleccionar una Razón Social.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }


            if (!NullOVacio(rtn, "RTN", ctrlRtn)) return false;
            if (!RTN(rtn, ctrlRtn)) return false;
            if (!RTNDuplicado(rtn, idProveedorActual, ctrlRtn)) return false;
            if (!CorreoDuplicado(correo, idProveedorActual, ctrlCorreo)) return false;


            if (!ComboSeleccionado(idClasificacion, "Clasificación")) return false;
            if (!ComboSeleccionado(idTerminos, "Términos de Pago")) return false;
            if (!ClasificacionTerminos(idClasificacion, idTerminos)) return false;

            return true;
        }
        // ─── VALIDAR FORMULARIO DE CLIENTE COMPLETO (PERSONA NATURAL O JURÍDICA) ───────────────────────────────
        /// <summary>
        /// Valida el formulario completo de cliente, soportando persona natural y jurídica.
        /// Realiza validaciones de contacto, identificación, duplicados y ubicación.
        /// </summary>
        /// <param name="nombre">Nombre del cliente.</param>
        /// <param name="cNombre">Control del nombre.</param>
        /// <param name="tipo">Tipo de cliente (PERSONA NATURAL o PERSONA JURÍDICA).</param>
        /// <param name="dni">DNI para persona natural.</param>
        /// <param name="cDni">Control del DNI.</param>
        /// <param name="rtn">RTN para persona jurídica.</param>
        /// <param name="cRtn">Control del RTN.</param>
        /// <param name="razon">Razón social para persona jurídica.</param>
        /// <param name="cRazon">Control de la razón social.</param>
        /// <param name="tel">Teléfono.</param>
        /// <param name="cTel">Control del teléfono.</param>
        /// <param name="correo">Correo.</param>
        /// <param name="cCorreo">Control del correo.</param>
        /// <param name="direccion">Dirección.</param>
        /// <param name="cDir">Control de dirección.</param>
        /// <param name="depto">Departamento seleccionado.</param>
        /// <param name="ciudad">Ciudad seleccionada.</param>
        /// <param name="tipocli">Tipo de cliente seleccionado.</param>
        /// <param name="idActual">Id actual para exclusiones (opcional).</param>
        /// <param name="contacto">Contacto adicional (opcional).</param>
        /// <param name="cContacto">Control del contacto (opcional).</param>
        /// <returns>True si todas las validaciones pasan; false en caso contrario.</returns>
        public static bool ValidarTodoElFormulario
            (
                string nombre, Control cNombre, string tipo, string dni, Control cDni,
                string rtn, Control cRtn, string razon, Control cRazon,
                string tel, Control cTel, string correo, Control cCorreo,
                string direccion, Control cDir, object depto, object ciudad, object tipocli, int idActual = 0,
                string contacto = "", Control cContacto = null
            )
        {

            if (!ValidarDatosContactoBase(nombre, cNombre, "Nombre", direccion, cDir, tel, cTel, correo, cCorreo))
                return false;
            if (!ExisteDatoDuplicado(correo, "correo_electronico_cliente", "Correo Electrónico", idActual))
                return false;
            if (!NullOVacio(contacto, "Contacto", cContacto)) return false;
            if (!SinEspaciosExtremos(contacto, "Contacto", cContacto)) return false;
            if (!SinDobleEspacio(contacto, "Contacto", cContacto)) return false;
            if (!SinSoloEspeciales(contacto, "Contacto", cContacto)) return false;
            if (!SinTresLetrasIguales(contacto, "Contacto")) return false;
            if (!LongitudMinima(contacto,"Contacto", 3, cContacto)) return false;
            if (!LongitudMaxima(contacto, "Contacto", 50, cContacto)) return false;
            if (!SinNumeros(contacto, "Contacto", cContacto)) return false;

            if (tipocli == null || tipocli.ToString() == "0")
            {
                MessageBox.Show("Debe seleccionar un Tipo de Cliente.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (tipo == "PERSONA NATURAL")
            {
                if (!DNI(dni, cDni)) return false;
                if (!ExisteDatoDuplicado(dni, "dni_cliente", "DNI", idActual)) return false;
            }
            else if (tipo == "PERSONA JURÍDICA")
            {
                if (string.IsNullOrWhiteSpace(razon))
                {
                    MessageBox.Show("Debe seleccionar una Razón Social.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!RTN(rtn, cRtn)) return false;
                if (!ExisteDatoDuplicado(rtn, "rtn_cliente", "RTN", idActual)) return false;
            }


            if (depto == null || ciudad == null || depto.ToString() == "0" || ciudad.ToString() == "0")
            {
                MessageBox.Show("Seleccione una ubicación válida (Departamento y Ciudad).", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }


            return true;
        }
        // ─── VALIDAR QUE NO CONTENGA ESPACIOS EN BLANCO (EN CUALQUIER PARTE) ───────────────────────────────
        /// <summary>
        /// Comprueba si la cadena contiene cualquier espacio en blanco (espacio, tabulación, etc.).
        /// </summary>
        /// <param name="valor">Texto a comprobar.</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje.</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <returns>True si encontró espacios; false si no contiene espacios.</returns>
        public static bool ContieneEspacios(string valor, string nombreCampo, Control campo = null)
        {
            // \s detecta cualquier espacio en blanco (espacio, tabulación, etc.)
            if (valor != null && System.Text.RegularExpressions.Regex.IsMatch(valor, @"\s"))
            {
                MessageBox.Show($"El campo '{nombreCampo}' no puede contener espacios en blanco.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                if (campo != null) campo.Focus();
                return true; // Retorna true si ENCONTRÓ espacios
            }
            return false;
        }

        // ─── VALIDAR FECHA DE ENTREGA ─────────────────────────────────
        /// <summary>
        /// Verifica que la fecha de entrega no sea anterior a la fecha actual.
        /// </summary>
        /// <param name="entrega">Fecha de entrega a validar.</param>
        /// <returns>True si la fecha es válida; false en caso contrario.</returns>
        public static bool FechaEntrega(DateTime entrega)
        {
            if (entrega.Date < DateTime.Today)
            {
                MessageBox.Show("La fecha de entrega no puede ser menor a la fecha actual", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── VALIDAR RANGO DE FECHAS (FILTRO) ────────────────────────
        /// <summary>
        /// Valida un rango de fechas usado para filtros: desde no puede ser mayor a hoy y hasta no puede ser anterior a desde.
        /// </summary>
        /// <param name="desde">Fecha de inicio.</param>
        /// <param name="hasta">Fecha final.</param>
        /// <returns>True si el rango es válido; false en caso contrario.</returns>
        public static bool FechaFiltro(DateTime desde, DateTime hasta)
        {
            if (desde.Date > DateTime.Today)
            {
                MessageBox.Show("La fecha de inicio del filtro no puede ser mayor a la fecha actual", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (hasta.Date > DateTime.Today)
            {
                MessageBox.Show("La fecha 'Hasta' no puede ser una fecha futura.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (hasta.Date < desde.Date)
            {
                MessageBox.Show("La fecha final del filtro no puede ser menor a la fecha inicial", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── VALIDAR FILA SELECCIONADA EN DATAGRIDVIEW ────────────────
        /// <summary>
        /// Comprueba si existe al menos una fila marcada en la columna "Seleccionar" del DataGridView.
        /// </summary>
        /// <param name="dgv">DataGridView a evaluar.</param>
        /// <returns>True si hay al menos una fila seleccionada; false en caso contrario.</returns>
        public static bool FilaSeleccionada(DataGridView dgv)
        {
            foreach (DataGridViewRow fila in dgv.Rows)
                if (Convert.ToBoolean(fila.Cells["Seleccionar"].Value))
                    return true;

            MessageBox.Show("Debe seleccionar al menos una fila", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        //Validar que exista una compra seleccionada a la hora de actualizar o insertar
        /// <summary>
        /// Verifica que exista un id seleccionado (no nulo ni cero) para operaciones sobre compras.
        /// </summary>
        /// <param name="id">Id a validar.</param>
        /// <returns>True si el id es válido; false en caso contrario.</returns>
        public static bool IdSeleccionado(int id)
        {
            if (id == 0)
            {
                MessageBox.Show("Ingrese una solicitud de compra primero", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }


        // ─── VALIDAR COMBO SELECCIONADO ───────────────────────────────
        /// <summary>
        /// Valida que el valor seleccionado sea mayor o igual a cero (útil cuando el índice puede ser -1).
        /// </summary>
        /// <param name="selectedValue">Valor seleccionado.</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje.</param>
        /// <returns>True si la selección es válida; false en caso contrario.</returns>
        public static bool ComboSeleccionadoConIndiceMenorCero(int selectedValue, string nombreCampo)
        {
            if (selectedValue < 0)
            {
                MessageBox.Show($"Debe seleccionar una opción en '{nombreCampo}'.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }



        //Validacion de existencia en lista de productos
        /// <summary>
        /// Comprueba que un KryptonListBox contenga al menos un elemento.
        /// </summary>
        /// <param name="lst">Lista a evaluar.</param>
        /// <param name="nombreCampo">Nombre descriptivo para el mensaje.</param>
        /// <returns>True si contiene elementos; false en caso contrario.</returns>
        public static bool ListBoxConElementos(KryptonListBox lst, string nombreCampo)
        {

            if (lst.Items.Count == 0)
            {
                MessageBox.Show($"Debe de agregar al menos un {nombreCampo}", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }


        //Validacion de buscar
        /// <summary>
        /// Valida que el texto de búsqueda sea un entero positivo válido.
        /// </summary>
        /// <param name="buscar">Texto a validar como id.</param>
        /// <returns>True si es un id válido; false en caso contrario.</returns>
        public static bool BuscarId(string buscar)
        {
            if (string.IsNullOrWhiteSpace(buscar))
            {
                MessageBox.Show("Ingrese el id primero antes de buscar", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(buscar, out int numero) || numero <= 0)
            {
                MessageBox.Show("Solo se permiten números enteros positivos", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;

        }

        // Validación para modificar cantidad
        /// <summary>
        /// Verifica condiciones necesarias para modificar la cantidad de un producto en una compra.
        /// </summary>
        /// <param name="idSeleccionado">Id de la compra seleccionada.</param>
        /// <param name="lst">Lista de productos asociada.</param>
        /// <param name="cantidad">Cantidad a asignar.</param>
        /// <returns>True si las condiciones se cumplen; false en caso contrario.</returns>
        public static bool ModificarCantidad(int idSeleccionado, KryptonListBox lst, int cantidad)
        {
            if (idSeleccionado == 0)
            {
                MessageBox.Show("Primero busca una compra.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (lst.SelectedIndex == -1)
            {
                MessageBox.Show("Selecciona un producto de la lista.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cantidad <= 0)
            {
                MessageBox.Show("La cantidad debe ser mayor a 0.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }
        // Validación general para compras
        /// <summary>
        /// Valida que los combos proveedor y forma de pago tengan una selección.
        /// </summary>
        /// <param name="proveedor">Combo del proveedor.</param>
        /// <param name="formaPago">Combo de forma de pago.</param>
        /// <returns>True si ambos están seleccionados; false en caso contrario.</returns>
        public static bool ValidarCompras(KryptonComboBox proveedor, KryptonComboBox formaPago)
        {
            if (proveedor.SelectedIndex == -1 || formaPago.SelectedIndex == -1)
            {
                MessageBox.Show("Por favor, llene todos los campos vacíos", "Campos incompletos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // Validación para combo box sin resultado
        /// <summary>
        /// Indica si un combo no tiene selección o no contiene resultado válido.
        /// </summary>
        /// <param name="cmb">Combo a validar.</param>
        /// <param name="nombreCampo">Nombre descriptivo para el mensaje.</param>
        /// <returns>True si tiene resultado válido; false en caso contrario.</returns>
        public static bool validarComboSinResultado(KryptonComboBox cmb, string nombreCampo)
        {
            if (cmb.SelectedIndex == -1 || cmb.SelectedValue == null)
            {
                MessageBox.Show($"No se encontró ningún {nombreCampo} con ese criterio de búsqueda.",
                    "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        //-----------------------------------------------------------------------------------------------------------
        //Validaciones de inventario

        // ─── VALIDAR DECIMAL (INVENTARIO) ────────────────────────────
        /// <summary>
        /// Valida que el contenido del TextBox sea un decimal válido según la cultura invariante.
        /// </summary>
        /// <param name="campo">TextBox a validar.</param>
        /// <returns>True si es decimal válido; false en caso contrario.</returns>
        public static bool ValidarDecimal(TextBox campo, string nombreCampo = null)
        {
            string etiqueta = nombreCampo ?? campo.Name;
            if (!decimal.TryParse(campo.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            {
                MessageBox.Show($"El campo '{etiqueta}' debe ser un número válido.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                campo.Focus();
                return false;
            }
            return true;
        }


        // ─── VALIDAR ENTERO (INVENTARIO) ─────────────────────────────
        /// <summary>
        /// Valida que el contenido del TextBox sea un entero.
        /// </summary>
        /// <param name="campo">TextBox a validar.</param>
        /// <returns>True si es entero válido; false en caso contrario.</returns>
        public static bool ValidarEntero(TextBox campo, string nombreCampo = null)
        {
            string etiqueta = nombreCampo ?? campo.Name;
            if (!int.TryParse(campo.Text, out _))
            {
                MessageBox.Show($"El campo '{etiqueta}' debe ser un número entero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                campo.Focus();
                return false;
            }
            return true;
        }

        // ─── VALIDAR POSITIVO (INVENTARIO) ───────────────────────────
        /// <summary>
        /// Valida que el valor numérico del TextBox sea mayor a cero.
        /// </summary>
        /// <param name="campo">TextBox a validar.</param>
        /// <returns>True si el valor es positivo; false en caso contrario.</returns>
        public static bool ValidarPositivo(TextBox campo, string nombreCampo = null)
        {
            string etiqueta = nombreCampo ?? campo.Name;
            decimal.TryParse(campo.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal resultado);
            if (resultado <= 0)
            {
                MessageBox.Show($"El campo '{etiqueta}' debe ser mayor a cero.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                campo.Focus();
                return false;
            }
            return true;
        }

        // ─── VALIDAR COSTO MENOR QUE PRECIO (INVENTARIO) ─────────────
        /// <summary>
        /// Verifica que el costo sea menor que el precio de venta.
        /// </summary>
        /// <param name="txtPrecio">TextBox con el precio de venta.</param>
        /// <param name="txtCosto">TextBox con el precio de costo.</param>
        /// <returns>True si el costo es menor que el precio; false en caso contrario.</returns>
        public static bool ValidarCostoMenorPrecio(TextBox txtPrecio, TextBox txtCosto)
        {
            decimal.TryParse(txtPrecio.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal precio);
            decimal.TryParse(txtCosto.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal costo);

            if (costo >= precio)
            {
                MessageBox.Show("El precio de costo no puede ser mayor o igual al precio de venta.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCosto.Focus();
                return false;
            }
            return true;
        }

        // ─── VALIDAR PRODUCTO EXISTENTE (INVENTARIO) ─────────────────
        /// <summary>
        /// Comprueba en la base de datos si ya existe un producto con el mismo nombre.
        /// </summary>
        /// <param name="nombreProducto">Nombre del producto a verificar.</param>
        /// <returns>True si no existe; false si ya existe.</returns>
        public static bool ValidarProductoExiste(string nombreProducto)
        {
            int count = new TRAMADE.Repositories.ProductoRepository().VerificarProductoExiste(nombreProducto);
            if (count > 0)
            {
                MessageBox.Show("El producto '" + nombreProducto + "' ya existe", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Valida los campos necesarios para el formulario de inventario (nombre, precios, stock, combos).
        /// </summary>
        public static bool ValidarInventario(
        TextBox txtNombreProducto,
        TextBox txtPrecio,
        TextBox txtPrecioCosto,
        TextBox txtStockInicial,
        ComboBox cmbCategoria,
        ComboBox cmbSucursal,
        ComboBox cmbProveedor = null,
        bool esNuevo = false)
        {
            // --- Nombre del Producto ---
            if (!NullOVacio(txtNombreProducto.Text, "Nombre Producto", txtNombreProducto)) return false;
            if (!SinEspaciosExtremos(txtNombreProducto.Text, "Nombre Producto", txtNombreProducto)) return false;
            if (!SinDobleEspacio(txtNombreProducto.Text, "Nombre Producto", txtNombreProducto)) return false;
            if (!SinSoloEspeciales(txtNombreProducto.Text, "Nombre Producto", txtNombreProducto)) return false;
            if (!SinTresLetrasIguales(txtNombreProducto.Text, "Nombre Producto")) return false;
            if (!LongitudMinima(txtNombreProducto.Text, "Nombre Producto", 3, txtNombreProducto)) return false;
            if (!LongitudMaxima(txtNombreProducto.Text, "Nombre Producto", 100, txtNombreProducto)) return false;

            // --- Precio de Venta ---
            if (!NullOVacio(txtPrecio.Text, "Precio", txtPrecio)) return false;
            if (!SinCeroInicial(txtPrecio, "Precio")) return false;
            if (!ValidarDecimal(txtPrecio, "Precio")) return false;
            if (!ValidarPositivo(txtPrecio, "Precio")) return false;

            // --- Precio de Costo ---
            if (!NullOVacio(txtPrecioCosto.Text, "Precio Costo", txtPrecioCosto)) return false;
            if (!SinCeroInicial(txtPrecioCosto, "Precio Costo")) return false;
            if (!ValidarDecimal(txtPrecioCosto, "Precio Costo")) return false;
            if (!ValidarPositivo(txtPrecioCosto, "Precio Costo")) return false;

            // --- Stock Inicial ---
            if (!NullOVacio(txtStockInicial.Text, "Stock Inicial", txtStockInicial)) return false;
            if (!SinCeroInicial(txtStockInicial, "Stock Inicial")) return false;
            if (!ValidarEntero(txtStockInicial, "Stock Inicial")) return false;
            if (!ValidarPositivo(txtStockInicial, "Stock Inicial")) return false;

            // --- Combos (índice porque se cargaron con .Items.Add) ---
            if (!ComboSeleccionadoPorIndice(cmbCategoria.SelectedIndex, "Categoría")) return false;
          //  if (!ComboSeleccionadoPorIndice(cmbSucursal.SelectedIndex, "Sucursal")) return false;
            if (cmbProveedor != null)
                if (!ComboSeleccionadoPorIndice(cmbProveedor.SelectedIndex, "Proveedor")) return false;

            // --- Precio debe ser mayor que Costo ---
            if (!ValidarCostoMenorPrecio(txtPrecio, txtPrecioCosto)) return false;

            // --- Nombre duplicado solo al registrar nuevo ---
            if (esNuevo)
                if (!ValidarProductoExiste(txtNombreProducto.Text.Trim())) return false;

            return true;
        }

        // ─── VALIDAR USUARIO LOGUEADO ───────────────────────────────────────
        /// <summary>
        /// Verifica que exista un usuario logueado (id distinto de cero).
        /// </summary>
        /// <param name="idUsuario">Id del usuario.</param>
        /// <returns>True si hay usuario logueado; false en caso contrario.</returns>
        public static bool ValidarUsuarioLogueado(int idUsuario)
        {
            if (idUsuario == 0)
            {
                MessageBox.Show("No se detectó ningún usuario logueado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── VALIDAR GRID VACÍO ───────────────────────────────────────
        /// <summary>
        /// Indica si un grid está vacío y muestra un mensaje si corresponde.
        /// </summary>
        /// <param name="cantidadFilas">Número de filas presentes.</param>
        /// <param name="mensajeError">Mensaje a mostrar si está vacío.</param>
        /// <returns>True si está vacío; false en caso contrario.</returns>
        public static bool ValidarGridVacio(int cantidadFilas, string mensajeError)
        {
            if (cantidadFilas == 0)
            {
                MessageBox.Show(mensajeError, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }

        // ─── VALIDAR FILA SELECCIONADA (FACTURA) ─────────────────────
        /// <summary>
        /// Valida que exista una fila actual seleccionada (para factura u operaciones similares).
        /// </summary>
        /// <param name="filaActual">Fila actual seleccionada.</param>
        /// <param name="mensajeError">Mensaje a mostrar si no hay fila.</param>
        /// <returns>True si hay fila; false en caso contrario.</returns>
        public static bool ValidarFilaSeleccionadaFactura(DataGridViewRow filaActual, string mensajeError)
        {
            if (filaActual == null)
            {
                MessageBox.Show(mensajeError, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // ─── PEDIR CONFIRMACIÓN ───────────────────────────────────────
        /// <summary>
        /// Muestra un cuadro de diálogo de confirmación con opciones Sí/No.
        /// </summary>
        /// <param name="mensajeProceso">Mensaje a mostrar al usuario.</param>
        /// <returns>True si el usuario confirma (Sí); false en caso contrario.</returns>
        public static bool PedirConfirmacion(string mensajeProceso)
        {
            DialogResult respuesta = MessageBox.Show(mensajeProceso, "Confirmación de TRAMADE", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return (respuesta == DialogResult.Yes);
        }

        // ─── VALIDAR DNI/RTN PARA BÚSQUEDA ───────────────────────────
        /// <summary>
        /// Determina si un texto es un DNI o RTN válido en cuanto a longitud (13 o 14) y sin espacios.
        /// </summary>
        /// <param name="documento">Texto a evaluar.</param>
        /// <returns>True si es válido para búsqueda; false en caso contrario.</returns>
        public static bool EsDniRtnValido(string documento)
        {
            if (documento.Contains(" ")) return false;
            return (documento.Length == 13 || documento.Length == 14);
        }

        // ─── VALIDAR EMISIÓN DE FACTURA ───────────────────────────────
        /// <summary>
        /// Valida las condiciones mínimas para emitir una factura y retorna un mensaje de error si falla.
        /// </summary>
        /// <param name="nombreCliente">Nombre del cliente.</param>
        /// <param name="direccionCliente">Dirección del cliente.</param>
        /// <param name="idVendedor">Identificador del vendedor.</param>
        /// <param name="fechaEmision">Fecha de emisión.</param>
        /// <param name="fechaVencimiento">Fecha de vencimiento.</param>
        /// <param name="cantidadProductos">Cantidad de productos en la factura.</param>
        /// <param name="contado">Indica si la factura es contado.</param>
        /// <param name="credito">Indica si la factura es crédito.</param>
        /// <param name="dniRtn">DNI o RTN del cliente (opcional).</param>
        /// <returns>Cadena vacía si no hay error; mensaje de error en caso contrario.</returns>
        public static string ValidarEmisionFactura(string nombreCliente, string direccionCliente, string idVendedor, DateTime fechaEmision, DateTime fechaVencimiento, int cantidadProductos, bool contado, bool credito, string dniRtn)
        {
            // ── NOMBRE DEL CLIENTE ────────────────────────────────────────   
            if (string.IsNullOrWhiteSpace(nombreCliente))
                return "El Nombre del cliente es obligatorio.";
            if (nombreCliente != nombreCliente.Trim())
                return "El nombre del cliente no puede iniciar ni terminar con espacios.";
            if (nombreCliente.Contains("  "))
                return "El nombre del cliente no puede contener espacios dobles.";
            if (System.Text.RegularExpressions.Regex.IsMatch(nombreCliente.Trim(), @"(.)\1\1"))
                return "El nombre del cliente no puede contener tres caracteres iguales seguidos.";
            if (nombreCliente.Trim().Length < 3)
                return "El nombre del cliente debe tener al menos 3 caracteres.";
            if (nombreCliente.Trim().Length > 50)
                return "El nombre del cliente no puede tener más de 50 caracteres.";
            if (!System.Text.RegularExpressions.Regex.IsMatch(nombreCliente.Trim(), @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$"))
                return "El nombre del cliente solo puede contener letras y espacios.";

            // ── DIRECCIÓN DEL CLIENTE ─────────────────────────────────────
            if (string.IsNullOrWhiteSpace(direccionCliente))
                return "La Dirección del cliente es obligatoria.";
            if (direccionCliente != direccionCliente.Trim())
                return "La dirección no puede iniciar ni terminar con espacios.";
            if (direccionCliente.Contains("  "))
                return "La dirección no puede contener espacios dobles.";
            if (System.Text.RegularExpressions.Regex.IsMatch(direccionCliente.Trim(), @"(.)\1\1"))
                return "La dirección no puede contener tres caracteres iguales seguidos.";
            if (direccionCliente.Trim().Length < 5)
                return "La dirección debe tener al menos 5 caracteres.";

            // ── RESTO DE VALIDACIONES (sin cambios) ───────────────────────
            if (string.IsNullOrWhiteSpace(idVendedor))
                return "Los datos del vendedor son obligatorios. Haga clic en el botón 'ID de vendedor'.";
            if (fechaVencimiento.Date < fechaEmision.Date)
                return "La fecha de vencimiento no puede ser menor a la fecha de emisión.";
            if (cantidadProductos == 0)
                return "La factura no puede ir vacía. Debe agregar al menos 1 producto.";
            if (!contado && !credito)
                return "Debe seleccionar una forma de pago (Contado o Crédito).";
            if (!string.IsNullOrWhiteSpace(dniRtn))
            {
                if (dniRtn.Contains(" ")) return "El DNI/RTN no debe contener espacios en blanco.";
                if (dniRtn.Length != 13 && dniRtn.Length != 14) return "El DNI debe tener exactamente 13 números y el RTN 14 números.";
            }
            return "";
        }

        // ─── VALIDAR CORREO DUPLICADO EN USUARIOS ────────────────────────────────
        /// <summary>
        /// Verifica si un correo ya existe registrado en la tabla USUARIO, excluyendo un id opcional.
        /// </summary>
        /// <param name="correo">Correo a validar.</param>
        /// <param name="idUsuarioActual">Id del usuario actual para excluirlo (opcional).</param>
        /// <param name="campo">Control asociado (se limpia si hay duplicado).</param>
        /// <returns>True si no existe duplicado; false en caso contrario.</returns>
        public static bool CorreoDuplicadoUsuario(string correo, int idUsuarioActual = 0, Control campo = null)
        {
            clsConexion ObjConexion = new clsConexion();
            try
            {
                ObjConexion.Abrir();
                SqlCommand cmd = new SqlCommand(@"
                SELECT COUNT(*) FROM USUARIO 
                WHERE correo_usuario = @correo 
                AND id_usuario != @idUsuarioActual", ObjConexion.SqlC);
                cmd.Parameters.AddWithValue("@correo", correo.Trim());
                cmd.Parameters.AddWithValue("@idUsuarioActual", idUsuarioActual);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                if (count > 0)
                {
                    MessageBox.Show("El correo ingresado ya está registrado en otro usuario.",
                        "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LimpiarControl(campo);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al validar correo: " + ex.Message);
                return false;
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }
        // ─── VALIDAR CORREO DE USUARIO COMPLETO ──────────────────────────────────
        /// <summary>
        /// Valida un correo de usuario completo: no vacío, formato, longitud y duplicados.
        /// </summary>
        /// <param name="correo">Correo a validar.</param>
        /// <param name="campo">Control asociado (opcional).</param>
        /// <param name="idUsuarioActual">Id actual para excluir de duplicados (opcional).</param>
        /// <returns>True si el correo es válido; false en caso contrario.</returns>
        public static bool ValidarCorreoUsuario(string correo, Control campo = null, int idUsuarioActual = 0)
        {
            if (!NullOVacio(correo, "Correo Electrónico", campo)) return false;
            if (!SinEspaciosExtremos(correo, "Correo Electrónico", campo)) return false;
            if (!Correo(correo, campo)) return false;
            if (!LongitudMaxima(correo, "Correo Electrónico", 100, campo)) return false;
            if (!CorreoDuplicadoUsuario(correo, idUsuarioActual, campo)) return false;
            return true;
        }

        // ─── VALIDAR CREDENCIALES CASE-SENSITIVE (LOGIN) ──────────────────
        /// <summary>
        /// Compara credenciales exactamente (case-sensitive) usando StringComparison.Ordinal.
        /// </summary>
        /// <param name="usuarioIngresado">Usuario ingresado por el usuario.</param>
        /// <param name="passIngresada">Contraseña ingresada por el usuario.</param>
        /// <param name="usuarioEnBD">Usuario obtenido de la base de datos.</param>
        /// <param name="passEnBD">Contraseña almacenada en la base de datos.</param>
        /// <returns>True si ambos coinciden exactamente; false en caso contrario.</returns>
        public static bool CredencialesExactas(string usuarioIngresado, string passIngresada,
                                                string usuarioEnBD, string passEnBD)
        {
            bool correoExacto = string.Equals(usuarioIngresado, usuarioEnBD, StringComparison.Ordinal);
            bool passExacta = string.Equals(passIngresada, passEnBD, StringComparison.Ordinal);
            return correoExacto && passExacta;
        }

        // ─── VALIDAR QUE UN DECIMAL NO INICIE CON CERO SEGUIDO DE DÍGITO ─────────────
        /// <summary>
        /// Rechaza valores como "09", "01.5" — números con cero inicial innecesario.
        /// </summary>
        public static bool SinCeroInicial(TextBox campo, string nombreCampo)
        {
            string val = campo.Text.Trim();
            // Solo rechaza si empieza con 0 seguido de otro DÍGITO (ej: 09, 01)
            // Permite 0.50, 0.99 porque el segundo char es un punto
            if (val.Length > 1 && val[0] == '0' && char.IsDigit(val[1]))
            {
                MessageBox.Show(
                    $"El campo '{nombreCampo}' no puede iniciar con cero (ej: escriba 9 en lugar de 09).",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                campo.Focus();
                return false;
            }
            return true;
        }

        // ─── VALIDAR COMBO POR ÍNDICE (acepta índice 0 como válido) ──────────────────
        /// <summary>
        /// Valida que el SelectedIndex del combo no sea -1 (ninguna selección).
        /// Usar este método cuando los items se cargan con .Items.Add().
        /// </summary>
        public static bool ComboSeleccionadoPorIndice(int selectedIndex, string nombreCampo)
        {
            if (selectedIndex < 0)
            {
                MessageBox.Show($"Debe seleccionar una opción en '{nombreCampo}'.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}
