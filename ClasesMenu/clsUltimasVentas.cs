using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace TRAMADE.Formularios_Login__Menú
{
    /// <summary>
    /// Clase encargada de la lógica de negocio y visualización de las ventas más recientes.
    /// Recupera los datos de facturación desde la base de datos y los presenta en un control DataGridView
    /// con un estilo visual personalizado.
    /// </summary>
    internal class clsUltimasVentas
    {
        /// <summary>
        /// Referencia al control DataGridView donde se inyectarán los datos de las ventas.
        /// </summary>
        private readonly DataGridView _dgv;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="clsUltimasVentas"/>.
        /// </summary>
        /// <param name="dgv">Control DataGridView de la interfaz de usuario que se desea gestionar.</param>
        public clsUltimasVentas(DataGridView dgv)
        {
            _dgv = dgv;
        }

        /// <summary>
        /// Ejecuta una consulta a la base de datos para obtener las últimas 5 facturas emitidas,
        /// realiza el cálculo de montos totales por factura y formatea los resultados para su exhibición.
        /// </summary>
        /// <remarks>
        /// El proceso incluye:
        /// <list type="bullet">
        /// <item><description>Cálculo de monto total sumando (cantidad * precio_unitario) de los productos asociados.</description></item>
        /// <item><description>Formateo de la columna 'Monto' a moneda local (Lempiras).</description></item>
        /// <item><description>Aplicación automática del esquema de colores institucional al grid.</description></item>
        /// </list>
        /// </remarks>
        public void Cargar()
        {
            clsConexion cn = new clsConexion();
            try
            {
                cn.Abrir();
                string query = @"
            SELECT TOP 8
                f.id_factura                         AS 'ID Venta',
                c.nombre_cliente                     AS 'Cliente',
                CONVERT(varchar, f.fecha_emision, 103) AS 'Fecha',
                SUM(fp.cantidad * p.precio_unitario) AS 'Monto'
            FROM FACTURA f
            INNER JOIN CLIENTE c           ON f.id_cliente   = c.id_cliente
            INNER JOIN FACTURA_PRODUCTO fp ON f.id_factura   = fp.id_factura
            INNER JOIN PRODUCTO p          ON fp.id_producto = p.id_producto
            GROUP BY f.id_factura, c.nombre_cliente, f.fecha_emision
            ORDER BY f.fecha_emision DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, cn.SqlC);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Formatear Monto
                DataColumn montoFormateado = new DataColumn("MontoFormateado", typeof(string));
                dt.Columns.Add(montoFormateado);
                foreach (DataRow row in dt.Rows)
                    row["MontoFormateado"] = $"L. {Convert.ToDecimal(row["Monto"]):N2}";
                dt.Columns.Remove("Monto");
                montoFormateado.ColumnName = "Monto";

                _dgv.DataSource = dt;
                EstiloGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error ventas: " + ex.Message, "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { cn.Cerrar(); }
        }

        /// <summary>
        /// Define la apariencia visual del DataGridView (colores de cabecera, selección, fuentes y bordes).
        /// Utiliza la paleta de colores café (RGB 139, 90, 43) y dorado (RGB 212, 160, 48).
        /// </summary>
        private void EstiloGrid()
        {
            _dgv.EnableHeadersVisualStyles = false;
            _dgv.BorderStyle = BorderStyle.None;
            _dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            _dgv.GridColor = Color.FromArgb(240, 232, 222);
            _dgv.RowHeadersVisible = false;
            _dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _dgv.Columns[0].FillWeight = 15;
            _dgv.Columns[1].FillWeight = 35;
            _dgv.Columns[2].FillWeight = 25;
            _dgv.Columns[3].FillWeight = 20;
            _dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            _dgv.ReadOnly = true;
            _dgv.AllowUserToAddRows = false;
            _dgv.AllowUserToResizeRows = false;
            _dgv.AllowUserToResizeColumns = false;
            _dgv.ScrollBars = ScrollBars.None;
            _dgv.BackgroundColor = Color.FromArgb(245, 240, 235);  // mismo fondo que el dashboard

            // Headers
            _dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(139, 90, 43);
            _dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            _dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(0, 6, 0, 6);
            _dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            _dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            _dgv.ColumnHeadersHeight = 34;

            // Celdas
            _dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            _dgv.DefaultCellStyle.ForeColor = Color.FromArgb(61, 43, 31);
            _dgv.DefaultCellStyle.BackColor = Color.White;
            _dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(218, 165, 100);
            _dgv.DefaultCellStyle.SelectionForeColor = Color.White;
            _dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            _dgv.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);

            // Filas alternas
            _dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(253, 248, 242);

            
        }
    }
}
