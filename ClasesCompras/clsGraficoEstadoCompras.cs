/*using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;

namespace TRAMADE.ClasesCompras
{
    internal class clsGraficoEstadoCompras
    {
        private readonly Chart _chart;
        private readonly clsConexion _conexion;

        public clsGraficoEstadoCompras(Chart chart, clsConexion conexion)
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
            var titulo = _chart.Titles.Add("Estado de Compras");
            titulo.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            titulo.ForeColor = Color.FromArgb(45, 45, 45);
            titulo.Docking = Docking.Top;
            titulo.Alignment = ContentAlignment.TopLeft;

            // Serie tipo dona
            var serie = new Series("Compras");
            serie.ChartType = SeriesChartType.Doughnut;
            serie["DoughnutRadius"] = "55";
            serie["PieStartAngle"] = "90";
            serie["PieLabelStyle"] = "Disabled";
            serie.ToolTip = "#VALX: #PERCENT{P0}";
            serie.Legend = "LeyendaCompras";
            serie.IsVisibleInLegend = true;

            try
            {
                _conexion.Abrir();
                string query = "SELECT ComprasPendientes, ComprasAutorizadas FROM VistaGraficoComprasPendientes";
                SqlCommand cmd = new SqlCommand(query, _conexion.SqlC);
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    int pendientes = Convert.ToInt32(dr["ComprasPendientes"]);
                    int autorizadas = Convert.ToInt32(dr["ComprasAutorizadas"]);

                    serie.Points.AddXY("Pendientes", pendientes);
                    serie.Points[0].Color = Color.FromArgb(210, 168, 110);
                    serie.Points[0].LegendText = "Pendientes";

                    serie.Points.AddXY("Autorizadas", autorizadas);
                    serie.Points[1].Color = Color.FromArgb(72, 199, 174);
                    serie.Points[1].LegendText = "Autorizadas";
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                serie.Points.AddXY("Pendientes", 0);
                serie.Points[0].Color = Color.FromArgb(210, 168, 110);
                serie.Points[0].LegendText = "Pendientes";

                serie.Points.AddXY("Autorizadas", 0);
                serie.Points[1].Color = Color.FromArgb(72, 199, 174);
                serie.Points[1].LegendText = "Autorizadas";

                System.Windows.Forms.MessageBox.Show("Error al cargar gráfico: " + ex.Message);
            }
            finally { _conexion.Cerrar(); }

            _chart.Series.Add(serie);

            // Leyenda
            var leyenda = _chart.Legends.Add("LeyendaCompras");
            leyenda.Docking = Docking.Bottom;
            leyenda.Alignment = StringAlignment.Center;
            leyenda.Font = new Font("Segoe UI", 9f);
            leyenda.BorderColor = Color.Transparent;
            leyenda.BackColor = Color.Transparent;
            leyenda.LegendStyle = LegendStyle.Row;

            // Área
            var area = _chart.ChartAreas[0];
            area.BackColor = Color.Transparent;
            area.BorderColor = Color.Transparent;
            area.Position.Auto = false;
            area.Position = new ElementPosition(0, 12, 100, 68);

            _chart.BackColor = Color.White;
            _chart.BorderlineColor = Color.Transparent;
        }
    }
}
*/
