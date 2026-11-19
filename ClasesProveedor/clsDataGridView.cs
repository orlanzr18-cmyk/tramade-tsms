using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.Formulario_Proveedores.Clases
{
    /// <summary>
    /// Utilidades estáticas para configurar y manipular controles DataGridView
    /// usados en el módulo de proveedores. Proporciona estilos visuales, carga
    /// de datos desde consultas, búsqueda en tiempo real y helpers para obtener ids
    /// y agregar columnas de selección.
    /// </summary>
    internal class clsDataGridView
    {
        // ─── APLICAR ESTILO ESTÁNDAR ──────────────────────────────────
        /// <summary>
        /// Aplica un estilo visual y de comportamiento uniforme al <see cref="DataGridView"/> pasado.
        /// Ajusta fuentes, colores, selección, encabezados y comportamiento de edición.
        /// </summary>
        /// <param name="dgv">Control DataGridView al que se aplicará el estilo.</param>
        public static void AplicarEstilo(DataGridView dgv)
        {
            // ─── COMPORTAMIENTO ───────────────────────────────────────
            dgv.ReadOnly = true;
            dgv.AllowUserToResizeRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // ─── ENCABEZADOS ──────────────────────────────────────────
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(148, 114, 71);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Poppins", 10f, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;  // ← nueva
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize; // ← cambiada
            dgv.RowHeadersVisible = false;

            // ─── FILAS ────────────────────────────────────────────────
            dgv.DefaultCellStyle.Font = new Font("Poppins", 9f, FontStyle.Regular);
            dgv.DefaultCellStyle.BackColor = Color.White;
            dgv.DefaultCellStyle.ForeColor = Color.FromArgb(50, 50, 50);
            dgv.RowTemplate.Height = 35;

            // ─── SELECCIÓN ────────────────────────────────────────────
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(178, 154, 111);
            dgv.DefaultCellStyle.SelectionForeColor = Color.White;

            // ─── BORDES Y GRILLA ──────────────────────────────────────
            dgv.BorderStyle = BorderStyle.None;
            dgv.GridColor = Color.FromArgb(220, 210, 195);
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.Single;

            // ─── COLUMNAS AUTO-AJUSTE ─────────────────────────────────
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        // ─── OCULTAR COLUMNA POR NOMBRE ──────────────────────────────
        /// <summary>
        /// Oculta una columna del DataGridView por su nombre (si existe).
        /// </summary>
        /// <param name="dgv">DataGridView que contiene la columna.</param>
        /// <param name="nombreColumna">Nombre de la columna a ocultar.</param>
        public static void OcultarColumna(DataGridView dgv, string nombreColumna)
        {
            if (dgv.Columns[nombreColumna] != null)
                dgv.Columns[nombreColumna].Visible = false;
        }

        // ─── CARGAR DATOS DESDE VISTA O CONSULTA ─────────────────────
        /// <summary>
        /// Ejecuta la consulta indicada y carga el resultado en el DataGridView.
        /// Permite ocultar columnas especificadas por nombre.
        /// </summary>
        /// <param name="dgv">Control DataGridView destino.</param>
        /// <param name="consulta">Cadena SQL o nombre de vista/consulta a ejecutar.</param>
        /// <param name="conexion">Conexión SQL válida para ejecutar la consulta.</param>
        /// <param name="columnasOcultas">Nombres de columnas que se ocultarán tras cargar los datos (opcional).</param>
        public static void Cargar(DataGridView dgv, string consulta, SqlConnection conexion,
                                  params string[] columnasOcultas)
        {
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dgv.DataSource = dt;

                foreach (string col in columnasOcultas)
                    if (dgv.Columns[col] != null)
                        dgv.Columns[col].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── FILTRAR POR TEXTO (BÚSQUEDA EN TIEMPO REAL) ─────────────
        /// <summary>
        /// Realiza una búsqueda en tiempo real sobre el resultado de la consulta indicada
        /// filtrando las columnas de tipo cadena que contengan <paramref name="textoBuscar"/>.
        /// </summary>
        /// <param name="dgv">DataGridView donde se mostrará el resultado filtrado.</param>
        /// <param name="textoBuscar">Texto a buscar dentro de las columnas de texto.</param>
        /// <param name="consulta">Consulta original que devuelve el conjunto base de datos.</param>
        /// <param name="conexion">Conexión SQL para ejecutar la consulta.</param>
        /// <param name="columnasOcultas">Columnas que deben quedar ocultas en el resultado (opcional).</param>
        public static void Buscar(DataGridView dgv, string textoBuscar, string consulta,
                                  SqlConnection conexion, params string[] columnasOcultas)
        {
            if (string.IsNullOrWhiteSpace(textoBuscar))
            {
                Cargar(dgv, consulta, conexion, columnasOcultas);
                return;
            }

            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                DataView dv = dt.DefaultView;

                string filtro = string.Empty;
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.DataType == typeof(string))
                        filtro += $"[{col.ColumnName}] LIKE '%{textoBuscar.Replace("'", "''")}%' OR ";
                }
                if (!string.IsNullOrEmpty(filtro))
                    dv.RowFilter = filtro.TrimEnd(' ', 'O', 'R');

                dgv.DataSource = dv.ToTable();

                foreach (string col in columnasOcultas)
                    if (dgv.Columns[col] != null)
                        dgv.Columns[col].Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── OBTENER ID DE LA FILA SELECCIONADA ──────────────────────
        /// <summary>
        /// Devuelve el valor de la columna de id de la fila actualmente seleccionada.
        /// </summary>
        /// <param name="dgv">DataGridView con la fila seleccionada.</param>
        /// <param name="columnaId">Índice de columna que contiene el id (por defecto 0).</param>
        /// <returns>Id como entero o -1 si no hay fila seleccionada.</returns>
        public static int ObtenerIdFila(DataGridView dgv, int columnaId = 0)
        {
            if (dgv.CurrentRow == null) return -1;
            return Convert.ToInt32(dgv.Rows[dgv.CurrentRow.Index].Cells[columnaId].Value);
        }

        // ─── OBTENER ID POR DOBLE CLIC ────────────────────────────────
        /// <summary>
        /// Obtiene el id de la fila donde se produjo un evento de celda (por ejemplo doble clic).
        /// </summary>
        /// <param name="dgv">DataGridView que generó el evento.</param>
        /// <param name="e">Argumentos del evento de celda que contienen el índice de fila.</param>
        /// <param name="columnaId">Índice de la columna con el id (por defecto 0).</param>
        /// <returns>Id en la fila indicada o -1 si el índice de fila no es válido.</returns>
        public static int ObtenerIdDobleClick(DataGridView dgv,
                                              DataGridViewCellEventArgs e, int columnaId = 0)
        {
            if (e.RowIndex < 0) return -1;
            return Convert.ToInt32(dgv.Rows[e.RowIndex].Cells[columnaId].Value);
        }

        /// <summary>
        /// Agrega una columna de casillas de verificación llamada "Seleccionar" al inicio del DataGridView
        /// y ajusta la edición para que solo esa columna sea editable.
        /// </summary>
        /// <param name="dgv">DataGridView al que se le añadirá la columna checkbox.</param>
        public static void AgregarColumnaCheckbox(DataGridView dgv)
        {
            if (!dgv.Columns.Contains("Seleccionar"))
            {
                DataGridViewCheckBoxColumn chkCol = new DataGridViewCheckBoxColumn();
                chkCol.Name = "Seleccionar";
                chkCol.HeaderText = "Seleccionar";
                chkCol.Width = 40;
                chkCol.ReadOnly = false;
                chkCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                dgv.Columns.Add(chkCol);
                dgv.Columns["Seleccionar"].DisplayIndex = 0;
            }


            // ─── Permitir edición solo en checkbox ────────────────────
            dgv.ReadOnly = false;
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Name != "Seleccionar")
                    col.ReadOnly = true;
            }
        }
    }
}
