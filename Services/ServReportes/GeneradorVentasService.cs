using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using TRAMADE.Models.ModelsReportes;
using TRAMADE.Repositories.RepoReportes;

namespace TRAMADE.Services.ServReportes
{
    public class GeneradorVentasService : IGeneradorReporte
    {
        private readonly IReporteRepository _reporteRepository;

        public GeneradorVentasService(
            IReporteRepository reporteRepository)
        {
            _reporteRepository = reporteRepository;
        }
        public LocalReport Generar(clsReporteItemModel reporte)
        {
            DataTable datos = _reporteRepository.ObtenerVentas(
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
                    recurso => recurso.EndsWith("rptVentas.rdlc"));

            if (string.IsNullOrEmpty(nombreRecurso))
            {
                throw new InvalidOperationException(
                    "No se encontró el archivo rptVentas.rdlc.");
            }

            LocalReport reporteLocal = new LocalReport();

            reporteLocal.ReportEmbeddedResource = nombreRecurso;

            reporteLocal.DataSources.Add(
                new ReportDataSource(
                    "DataSet_Ventas",
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