using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using TRAMADE.Models;

namespace TRAMADE.Formularios_Login__Menú
{
    internal class clsGraficoEstadoClientes
    {
        private readonly PlotView ObjPlotView;

        public List<clsItemLeyenda> ItemsLeyenda { get; private set; } = new List<clsItemLeyenda>();

        public clsGraficoEstadoClientes(PlotView plotView)
        {
            ObjPlotView = plotView;
        }

        public void Cargar(DataTable dtEstados)
        {
            try
            {
                ItemsLeyenda.Clear();

                var modelo = new PlotModel
                {
                    DefaultFont = "Poppins",
                    Background = OxyColors.White,
                    PlotAreaBorderColor = OxyColors.Transparent,
                    IsLegendVisible = false
                };

                var serie = new PieSeries
                {
                    StrokeThickness = 0,
                    AngleSpan = 360,
                    StartAngle = 90,
                    InnerDiameter = 0.55,
                    TickHorizontalLength = 0,
                    TickLabelDistance = 0,
                    TickRadialLength = 0,
                    OutsideLabelFormat = string.Empty,
                    InsideLabelFormat = string.Empty,
                    FontSize = 0,
                    TrackerFormatString = "{1}: {2:0.##}"
                };

                OxyColor colorActivo = OxyColor.FromArgb(255, 72, 199, 174);
                OxyColor colorPendiente = OxyColor.FromArgb(255, 210, 168, 110);
                OxyColor colorInactivo = OxyColor.FromArgb(255, 200, 80, 80);
                OxyColor[] paletaOtros =
                {
                    OxyColor.FromArgb(255, 100, 160, 210),
                    OxyColor.FromArgb(255, 150, 120, 180)
                };

                int colorOtros = 0;

                foreach (DataRow r in dtEstados.Rows)
                {
                    string estadoOriginal = r["Estado"].ToString();
                    string estado = estadoOriginal.ToUpper().Trim();
                    double valor = Convert.ToDouble(r["Cantidad"]);

                    OxyColor color;
                    if (estado.Contains("INACTIVO") || estado.Contains("NEGADO"))
                        color = colorInactivo;
                    else if (estado.Contains("ACTIVO") || estado.Contains("AUTORIZADO"))
                        color = colorActivo;
                    else if (estado.Contains("PENDIENTE"))
                        color = colorPendiente;
                    else
                    {
                        color = paletaOtros[colorOtros % paletaOtros.Length];
                        colorOtros++;
                    }

                    serie.Slices.Add(new PieSlice(estadoOriginal, valor) { Fill = color });

                    ItemsLeyenda.Add(new clsItemLeyenda
                    {
                        Nombre = estadoOriginal,
                        Color = Color.FromArgb(color.A, color.R, color.G, color.B)
                    });
                }

                modelo.Series.Add(serie);
                ObjPlotView.Model = modelo;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar gráfica de estados: " + ex.Message);
            }
        }
    }
}