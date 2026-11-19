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
using System.Reflection;
using TRAMADE.DatasSet_Reportes.dsProductosTableAdapters;

namespace TRAMADE
{
    internal class clsRptProductos
    {
        /// <summary>
        /// Genera un <see cref="LocalReport"/> para el reporte de productos más vendidos
        /// en el rango de fechas y la sucursal indicados.
        /// Carga los datos desde el DataSet correspondiente y configura el recurso RDLC embebido.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio del período a reportar.</param>
        /// <param name="fechaFin">Fecha de fin del período a reportar.</param>
        /// <param name="idSucursal">Identificador de la sucursal (0 = todas las sucursales).</param>
        /// <param name="nombreSucursal">Nombre de la sucursal que se mostrará en el encabezado del reporte.</param>
        /// <returns>
        /// Instancia de <see cref="LocalReport"/> configurada con datos y parámetros,
        /// o <c>null</c> si no hay datos o ocurre un error durante la generación.
        /// </returns>
        public LocalReport Generar(DateTime fechaInicio, DateTime fechaFin, int idSucursal, string nombreSucursal)
        {
            try
            {
                dsProductos ds = new dsProductos();
                new DatasSet_Reportes.dsProductosTableAdapters.DataTable1TableAdapter()
                    .Fill(ds.DataTable1, fechaInicio, fechaFin, idSucursal);

                // ✅ Validación
                if (ds.DataTable1.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron productos para el período y sucursal seleccionados.",
                        "Sin resultados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }

                var report = new LocalReport();

                // ✅ Búsqueda dinámica igual que clsRptCompras
                string nombreRecurso = Assembly.GetExecutingAssembly()
                    .GetManifestResourceNames()
                    .FirstOrDefault(rn => rn.EndsWith("rptProductos.rdlc"));

                if (string.IsNullOrEmpty(nombreRecurso))
                {
                    MessageBox.Show("Error: No se encontró el archivo rptProductos.rdlc.",
                        "ERROR DE RECURSO");
                    return null;
                }

                report.ReportEmbeddedResource = nombreRecurso;
                report.DataSources.Add(new ReportDataSource("DataSet_Productos", (DataTable)ds.DataTable1));
                report.SetParameters(new[]
                {
                    new ReportParameter("FechaInicio",  fechaInicio.ToString("dd/MM/yyyy")),
                    new ReportParameter("FechaFin",     fechaFin.ToString("dd/MM/yyyy")),
                    new ReportParameter("Sucursal",     nombreSucursal)
                });
                return report;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reporte productos: " + ex.Message,
                    "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

    }
}*/