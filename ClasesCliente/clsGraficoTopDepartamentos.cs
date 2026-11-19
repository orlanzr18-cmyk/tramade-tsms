using System;
using System.Data;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace TRAMADE.Formularios_Login__Menú
{
    internal class clsGraficoTopDepartamentos
    {
        private readonly PlotView ObjPlotView;
        public clsGraficoTopDepartamentos(PlotView plotView)
        {
            ObjPlotView = plotView;
        }

        public void Cargar(DataTable dt)
        {
            try
            {
                var modelo = new PlotModel
                {
                    DefaultFont = "Poppins",
                    Background = OxyColors.White,
                    PlotAreaBorderColor = OxyColor.FromArgb(255, 60, 60, 60),
                    PlotAreaBorderThickness = new OxyThickness(1)
                };

                var ejeCategorias = new CategoryAxis
                {
                    Position = AxisPosition.Left,
                    GapWidth = 0.35,
                    StartPosition = 1,
                    EndPosition = 0,
                    AxislineColor = OxyColors.Transparent,
                    TicklineColor = OxyColors.Transparent,
                    MajorGridlineStyle = LineStyle.None,
                    TextColor = OxyColor.FromArgb(255, 60, 60, 60),
                    FontSize = 11
                };

                var ejeValores = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Minimum = 0,
                    MajorGridlineStyle = LineStyle.Solid,
                    MajorGridlineColor = OxyColor.FromArgb(255, 225, 215, 200),
                    AxislineColor = OxyColors.Transparent,
                    TicklineColor = OxyColors.Transparent,
                    TextColor = OxyColor.FromArgb(255, 130, 110, 90),
                    FontSize = 10
                };

                var serie = new BarSeries
                {
                    TrackerFormatString = "{1}: {2} clientes",
                    StrokeThickness = 0
                };

                OxyColor[] colores =
                {
                    OxyColor.FromArgb(255, 120, 85, 45),
                    OxyColor.FromArgb(255, 160, 120, 75),
                    OxyColor.FromArgb(255, 190, 155, 105),
                    OxyColor.FromArgb(255, 210, 178, 135),
                    OxyColor.FromArgb(255, 225, 200, 165)
                };

                int i = 0;
                foreach (DataRow r in dt.Rows)
                {
                    string depa = r["nombre_departamento"].ToString();
                    int total = Convert.ToInt32(r["Total"]);

                    ejeCategorias.Labels.Add(depa);
                    serie.Items.Add(new BarItem
                    {
                        Value = total,
                        Color = colores[i < colores.Length ? i : colores.Length - 1]
                    });
                    i++;
                }

                modelo.Axes.Add(ejeCategorias);
                modelo.Axes.Add(ejeValores);
                modelo.Series.Add(serie);

                ObjPlotView.Model = modelo;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar gráfica de departamentos: " + ex.Message);
            }
        }
    }
}