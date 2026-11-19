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
using TRAMADE.DatasSet_Reportes.dsStockTableAdapters;

namespace TRAMADE
{
    internal class clsRptStock
    {
        /// <summary>
        /// Genera un <see cref="LocalReport"/> que muestra el stock de productos para
        /// la sucursal indicada. Carga los datos desde el DataSet correspondiente y
        /// configura el recurso RDLC embebido y sus parámetros.
        /// </summary>
        /// <param name="idSucursal">Identificador de la sucursal cuyo stock se desea generar (0 = todas).</param>
        /// <param name="nombreSucursal">Nombre de la sucursal que se mostrará en el reporte.</param>
        /// <returns>
        /// Un objeto <see cref="LocalReport"/> configurado con el DataSource y parámetros,
        /// o <c>null</c> si no hay datos o si ocurre un error durante la generación.
        /// </returns>
        public LocalReport Generar(int idSucursal, string nombreSucursal)
        {
            try
            {
                dsStock ds = new dsStock();
                new DatasSet_Reportes.dsStockTableAdapters.DataTable1TableAdapter()
                    .Fill(ds.DataTable1, idSucursal);

                // ✅ Validación
                if (ds.DataTable1.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró stock para la sucursal seleccionada.",
                        "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }

                var report = new LocalReport();

                // ✅ Búsqueda dinámica
                string nombreRecurso = Assembly.GetExecutingAssembly()
                    .GetManifestResourceNames()
                    .FirstOrDefault(rn => rn.EndsWith("rptStock.rdlc"));

                if (string.IsNullOrEmpty(nombreRecurso))
                {
                    MessageBox.Show("Error: No se encontró el archivo rptStock.rdlc.",
                        "ERROR DE RECURSO");
                    return null;
                }

                report.ReportEmbeddedResource = nombreRecurso;
                // ✅ Nombre correcto del DataSource
                report.DataSources.Add(new ReportDataSource("DataSet_Stock", (DataTable)ds.DataTable1));

                report.SetParameters(new[]
                {
                    new ReportParameter("Sucursal", nombreSucursal)
                });
                return report;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reporte stock: " + ex.Message,
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}*/