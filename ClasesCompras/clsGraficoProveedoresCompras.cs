/*using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace TRAMADE.ClasesCompras
{
    internal class clsGraficoProveedoresCompras
    {
        private readonly Chart _chart;
        private readonly clsConexion _conexion;

        public clsGraficoProveedoresCompras(Chart chart, clsConexion conexion)
        {
            _chart = chart;
            _conexion = conexion;
        }

        public void Cargar()
        {
            _chart.Series.Clear();
            _chart.Titles.Clear();
            _chart.Legends.Clear();

            // Título
            var titulo = _chart.Titles.Add("Top 5 Proveedores Más Usados");
            titulo.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(45, 45, 45);
            titulo.Docking = Docking.Top;
            titulo.Alignment = ContentAlignment.TopLeft;

            // Serie
            var serie = new Series("Proveedores");
            serie.ChartType = SeriesChartType.Bar;
            serie.IsVisibleInLegend = false;
            serie.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            serie.IsValueShownAsLabel = true;
            serie.LabelForeColor = Color.FromArgb(60, 60, 60);
            serie["PointWidth"] = "0.6";
            serie.ToolTip = "#VALX: #VALY órdenes";

            // Misma paleta café que productos
            Color[] colores = {
                Color.FromArgb(120,  85,  45),
                Color.FromArgb(148, 114,  71),
                Color.FromArgb(178, 144, 101),
                Color.FromArgb(205, 172, 130),
                Color.FromArgb(230, 205, 170),
            };

            try
            {
                _conexion.Abrir();
                string query = "SELECT * FROM VistaTop5ProveedoresMasUsados";
                SqlCommand cmd = new SqlCommand(query, _conexion.SqlC);
                SqlDataReader dr = cmd.ExecuteReader();

                int i = 0;
                while (dr.Read())
                {
                    string proveedor = dr["Proveedor"].ToString();
                    int total = Convert.ToInt32(dr["TotalOrdenes"]);
                    int idx = serie.Points.AddXY(proveedor, total);
                    serie.Points[idx].Color = colores[i < colores.Length ? i : colores.Length - 1];
                    i++;
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Error al cargar gráfico: " + ex.Message);
            }
            finally { _conexion.Cerrar(); }

            _chart.Series.Add(serie);

            // Área — idéntica a la de Productos para consistencia visual
            var area = _chart.ChartAreas[0];
            area.BackColor = Color.Transparent;
            area.BorderColor = Color.Transparent;
            area.Position.Auto = false;
            area.Position = new ElementPosition(0, 10, 100, 88);

            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.LineColor = Color.FromArgb(200, 200, 200);
            area.AxisX.LabelStyle.Font = new Font("Segoe UI", 8.5f);
            area.AxisX.LabelStyle.ForeColor = Color.FromArgb(100, 100, 100);
            area.AxisX.MajorTickMark.Enabled = false;
            area.AxisX.IsReversed = true;

            area.AxisY.MajorGrid.LineColor = Color.FromArgb(230, 230, 230);
            area.AxisY.LineColor = Color.Transparent;
            area.AxisY.LabelStyle.ForeColor = Color.Silver;
            area.AxisY.LabelStyle.Font = new Font("Segoe UI", 8f);
            area.AxisY.Minimum = 0;

            _chart.BackColor = Color.White;
            _chart.BorderlineColor = Color.Transparent;
        }
    }
}
*/
