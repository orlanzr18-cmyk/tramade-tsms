using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using TRAMADE.Models.ModelsReportes;
using TRAMADE.Repositories.RepoReportes;

namespace TRAMADE.Services.ServReportes
{
    public class GeneradorComprasService : IGeneradorReporte
    {
        private readonly IReporteRepository _reporteRepository;

        
        public GeneradorComprasService(
            IReporteRepository reporteRepository)
        {
            _reporteRepository = reporteRepository;
        }
        public LocalReport Generar(clsReporteItemModel reporte)
        {
            DataTable datos = _reporteRepository.ObtenerCompras(
                reporte.FechaInicio,
                reporte.FechaFin,
                reporte.IdSucursal);

            if (datos == null || datos.Rows.Count == 0)
            {
                return null;
            }

            string nombreRecurso = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceNames()
                .FirstOrDefault(
                    recurso => recurso.EndsWith("rptCompras.rdlc"));

            if (string.IsNullOrEmpty(nombreRecurso))
            {
                throw new InvalidOperationException(
                    "No se encontró el archivo rptCompras.rdlc.");
            }

            LocalReport reporteLocal = new LocalReport();

            reporteLocal.ReportEmbeddedResource = nombreRecurso;

            reporteLocal.DataSources.Add(
                new ReportDataSource(
                    "DataSet_Compras",
                    datos));

            reporteLocal.SetParameters(new[]
            {
                new ReportParameter(
                    "FechaInicio",
                    reporte.FechaInicio.ToString("dd/MM/yyyy")),

                new ReportParameter(
                    "FechaFin",
                    reporte.FechaFin.ToString("dd/MM/yyyy")),

                new ReportParameter(
                    "Sucursal",
                    reporte.NombreSucursal)
            });

            return reporteLocal;
        }
    }
}