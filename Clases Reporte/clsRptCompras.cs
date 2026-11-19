/*using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection; // Necesario para buscar el reporte en el repositorio de GitHub
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TRAMADE.DatasSet_Reportes;
using TRAMADE.DatasSet_Reportes.dsComprasTableAdapters;

namespace TRAMADE
{
    internal class clsRptCompras
    {
        /// <summary>
        /// Nombre de la sucursal asociado al reporte (valor usado como parámetro en el informe).
        /// </summary>
        private string nombreSucursal;

        /// <summary>
        /// Genera un <see cref="LocalReport"/> para el reporte de compras en el rango de fechas y sucursal indicados.
        /// Carga los datos desde el DataTable correspondiente y asigna el recurso .rdlc embebido.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del período a reportar.</param>
        /// <param name="fechaFin">Fecha de fin del período a reportar.</param>
        /// <param name="idSucursal">Identificador de la sucursal (0 = todas las sucursales).</param>
        /// <param name="nombreSucursal">Texto con el nombre de la sucursal para mostrar en el reporte.</param>
        /// <returns>
        /// Un objeto <see cref="LocalReport"/> configurado listo para exportar o mostrar,
        /// o null si no hubo datos o si ocurrió un error durante la generación.
        /// </returns>
        public LocalReport Generar(DateTime fechaInicio, DateTime fechaFin, int idSucursal, string nombreSucursal)
        {
            try
            {
                dsCompras ds = new dsCompras();
                new DatasSet_Reportes.dsComprasTableAdapters.DataTable1TableAdapter()
                    .Fill(ds.DataTable1, fechaInicio, fechaFin, idSucursal);

                
                if (ds.DataTable1.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron compras para el período y sucursal seleccionados.",
                        "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }

                var report = new LocalReport();
                string nombreRecurso = Assembly.GetExecutingAssembly()
                    .GetManifestResourceNames()
                    .FirstOrDefault(rn => rn.EndsWith("rptCompras.rdlc"));

                if (string.IsNullOrEmpty(nombreRecurso))
                {
                    MessageBox.Show("Error: No se encontró el archivo rptCompras.rdlc.",
                        "ERROR DE RECURSO");
                    return null;
                }

                report.ReportEmbeddedResource = nombreRecurso;
                report.DataSources.Add(new ReportDataSource("DataSet_Compras", (DataTable)ds.DataTable1));

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
                MessageBox.Show("Error en la generación del reporte: " + ex.Message,
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}*/