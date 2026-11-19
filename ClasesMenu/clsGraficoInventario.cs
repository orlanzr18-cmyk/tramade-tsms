using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Data.SqlClient;

namespace TRAMADE.Formularios_Login__Menú
{
    public class clsGraficoInventario
    {
        public PlotModel Modelo { get; private set; }

        public void Cargar()
        {
            int normal = 0, bajo = 0, critico = 0, agotado = 0;

            var cn = new clsConexion();
            try
            {
                cn.Abrir();
                // Mismos umbrales que clsProductoVM: 0 = Agotado, <=10 = Crítico,
                // <=50 = Bajo, > 50 = Normal. MAX(cantidad_stock) por producto
                // para clasificar por el mejor proveedor disponible.
                string query = @"
                    SELECT
                        SUM(CASE WHEN MaxStock > 50 THEN 1 ELSE 0 END) AS Normal,
                        SUM(CASE WHEN MaxStock BETWEEN 11 AND 50 THEN 1 ELSE 0 END) AS Bajo,
                        SUM(CASE WHEN MaxStock BETWEEN 1 AND 10 THEN 1 ELSE 0 END) AS Critico,
                        SUM(CASE WHEN MaxStock = 0 OR MaxStock IS NULL THEN 1 ELSE 0 END) AS Agotado
                    FROM (
                        SELECT P.id_producto, MAX(PP.cantidad_stock) AS MaxStock
                        FROM PRODUCTO P
                        LEFT JOIN PRODUCTO_PROVEEDOR PP ON PP.id_producto = P.id_producto
                        GROUP BY P.id_producto
                    ) AS StockPorProducto";

                var cmd = new SqlCommand(query, cn.SqlC);
                var dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    normal = dr["Normal"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Normal"]);
                    bajo = dr["Bajo"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Bajo"]);
                    critico = dr["Critico"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Critico"]);
                    agotado = dr["Agotado"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Agotado"]);
                }
                dr.Close();
            }
            catch { }
            finally { cn.Cerrar(); }

            var modelo = new PlotModel { Background = OxyColors.White };
            var leyenda = new Legend
            {
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendFontSize = 10,
                LegendTextColor = OxyColor.Parse("#5a3e2b"),
                LegendMargin = 6
            };
            modelo.Legends.Add(leyenda);

            var serie = new PieSeries
            {
                StrokeThickness = 1.0,
                InnerDiameter = 0.55,
                AngleSpan = 360,
                StartAngle = 90,
                InsideLabelFormat = "",
                OutsideLabelFormat = "",
                TickHorizontalLength = 0,
                TickRadialLength = 0,
            };

            // Mismos colores exactos que clsProductoVM.AlertaColor
            serie.Slices.Add(new PieSlice("Normal", normal) { Fill = OxyColor.Parse("#27ae60") });
            serie.Slices.Add(new PieSlice("Bajo", bajo) { Fill = OxyColor.Parse("#e67e22") });
            serie.Slices.Add(new PieSlice("Crítico", critico) { Fill = OxyColor.Parse("#e74c3c") });
            serie.Slices.Add(new PieSlice("Agotado", agotado) { Fill = OxyColor.Parse("#c0392b") });

            modelo.Series.Add(serie);
            Modelo = modelo;
        }
    }
}