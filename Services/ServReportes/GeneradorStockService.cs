using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using TRAMADE.Models.ModelsReportes;
using TRAMADE.Repositories.RepoReportes;

namespace TRAMADE.Services.ServReportes
{
    public class GeneradorStockService : IGeneradorReporte
    {
        private readonly IReporteRepository _reporteRepository;

        public GeneradorStockService(
            IReporteRepository reporteRepository)
        {
            _reporteRepository = reporteRepository;
        }
        public LocalReport Generar(clsReporteItemModel reporte)
        {
            DataTable datos = _reporteRepository.ObtenerStock(
                reporte.IdSucursal);

            if (datos == null || datos.Rows.Count == 0)
            {
                return null;
            }

            string nombreRecurso = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceNames()
                .FirstOrDefault(
                    recurso => recurso.EndsWith("rptStock.rdlc"));

            if (string.IsNullOrEmpty(nombreRecurso))
            {
                throw new InvalidOperationException(
                    "No se encontró el archivo rptStock.rdlc.");
            }

            LocalReport reporteLocal = new LocalReport();

            reporteLocal.ReportEmbeddedResource = nombreRecurso;

            reporteLocal.DataSources.Add(
                new ReportDataSource(
                    "DataSet_Stock",
                    datos));

            reporteLocal.SetParameters(new[]
            {
                new ReportParameter(
                    "Sucursal",
                    reporte.NombreSucursal)
            });

            return reporteLocal;
        }
    }
}