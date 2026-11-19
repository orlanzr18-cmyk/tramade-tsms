/*using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TRAMADE.DatasSet_Reportes;
using TRAMADE.DatasSet_Reportes.dsVentasTableAdapters;

namespace TRAMADE
{
    internal class clsRptVentas
    {
        /// <summary>
        /// Nombre de la sucursal que se mostrará en el reporte de ventas.
        /// </summary>
        private string nombreSucursal;

        /// <summary>
        /// Genera un <see cref="LocalReport"/> con las ventas del período y sucursal indicados.
        /// Consulta el DataSet correspondiente, verifica si hay resultados y configura el recurso RDLC embebido.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del período a reportar.</param>
        /// <param name="fechaFin">Fecha de fin del período a reportar.</param>
        /// <param name="idSucursal">Identificador de la sucursal (0 = todas las sucursales).</param>
        /// <param name="nombreSucursal">Nombre de la sucursal que se mostrará en el encabezado del reporte.</param>
        /// <returns>
        /// Un objeto <see cref="LocalReport"/> configurado con el DataSource y parámetros,
        /// o <c>null</c> si no existen datos para el período/sucursal o si ocurre un error.
        /// </returns>
        public LocalReport Generar(DateTime fechaInicio, DateTime fechaFin, int idSucursal, string nombreSucursal)
        {
            try
            {
                dsVentas ds = new dsVentas();
                new DatasSet_Reportes.dsVentasTableAdapters.DataTable1TableAdapter()
                    .Fill(ds.DataTable1, fechaInicio, fechaFin, idSucursal);

                // ✅ Validación
                if (ds.DataTable1.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron ventas para el período y sucursal seleccionados.",
                        "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }

                var report = new LocalReport();

                string nombreRecurso = Assembly.GetExecutingAssembly()
                    .GetManifestResourceNames()
                    .FirstOrDefault(rn => rn.EndsWith("rptVentas.rdlc"));

                if (string.IsNullOrEmpty(nombreRecurso))
                {
                    MessageBox.Show("Error: No se encontró el archivo rptVentas.rdlc.",
                        "ERROR DE RECURSO");
                    return null;
                }

                report.ReportEmbeddedResource = nombreRecurso;
                report.DataSources.Add(new ReportDataSource("DataSet_Ventas", (DataTable)ds.DataTable1));
                report.SetParameters(new[]
                {
                    new ReportParameter("FechaInicio", fechaInicio.ToString("dd/MM/yyyy")),
                    new ReportParameter("FechaFin",    fechaFin.ToString("dd/MM/yyyy")),
                    new ReportParameter("Sucursal",    nombreSucursal)
                });
                return report;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reporte ventas: " + ex.Message,
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }

}*/
