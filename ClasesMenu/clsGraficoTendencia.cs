using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TRAMADE.Formularios_Login__Menú
{
    public class clsGraficoTendencia
    {
        public PlotModel Modelo { get; private set; }

        public void Cargar()
        {
            var valores = new List<double>();
            var etiquetas = new List<string>();

            var colores = new[]
            {
                "#e6cdaa", "#d2b28a", "#be9669",
                "#a57d50", "#947247", "#78552d"
            };

            var cn = new clsConexion();
            try
            {
                cn.Abrir();
                string query = @"
                    SELECT
                        FORMAT(fecha_emision, 'MMM yyyy', 'es-HN') AS Mes,
                        COUNT(*) AS Total
                    FROM FACTURA
                    WHERE fecha_emision >= DATEADD(MONTH, -5,
                          DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
                      AND fecha_emision <  DATEADD(MONTH,  1,
                          DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1))
                    GROUP BY
                        YEAR(fecha_emision),
                        MONTH(fecha_emision),
                        FORMAT(fecha_emision, 'MMM yyyy', 'es-HN')
                    ORDER BY
                        YEAR(fecha_emision),
                        MONTH(fecha_emision)";

                var cmd = new SqlCommand(query, cn.SqlC);
                var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    etiquetas.Add(dr["Mes"].ToString());
                    valores.Add(Convert.ToDouble(dr["Total"]));
                }
                dr.Close();
            }
            catch { }
            finally { cn.Cerrar(); }

            var modelo = new PlotModel { Background = OxyColors.White };

            // Eje X con etiquetas de mes
            var ejeY = new CategoryAxis
            {
                Position = AxisPosition.Left,
                TextColor = OxyColor.Parse("#9a7a5a"),
                TicklineColor = OxyColors.Transparent,
                MajorGridlineStyle = LineStyle.None,
                FontSize = 10
            };
            foreach (var etiqueta in etiquetas)
                ejeY.Labels.Add(etiqueta);

            var ejeX = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                TextColor = OxyColor.Parse("#9a7a5a"),
                TicklineColor = OxyColors.Transparent,
                MajorGridlineStyle = LineStyle.Solid,
                MajorGridlineColor = OxyColor.Parse("#eeddd0"),
                MinorGridlineStyle = LineStyle.None,
                Minimum = 0,
                FontSize = 10
            };

            modelo.Axes.Add(ejeY);
            modelo.Axes.Add(ejeX);

            for (int i = 0; i < valores.Count; i++)
            {
                int idx = i < colores.Length ? i : colores.Length - 1;
                var serie = new BarSeries
                {
                    StrokeThickness = 0,
                    BarWidth = 0.6,
                    FillColor = OxyColor.Parse(colores[idx]),
                    TrackerFormatString = "{0}: {2:0}",
                    Title = etiquetas.Count > i ? etiquetas[i] : ""
                };
                serie.Items.Add(new BarItem { Value = valores[i] });
                modelo.Series.Add(serie);
            }
            Modelo = modelo;
        }
    }
}