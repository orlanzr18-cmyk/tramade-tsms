using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;

namespace TRAMADE.Services.ServReportes
{
    public class ReporteExportacionService
    {
        private readonly Dictionary<string, IExportadorReporte> _exportadores;

        public ReporteExportacionService()
        {
            _exportadores =
                new Dictionary<string, IExportadorReporte>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "PDF",
                        new ExportadorPdf()
                    },
                    {
                        "Excel",
                        new ExportadorExcel()
                    }
                };
        }

        public byte[] Exportar(
            LocalReport reporte,
            string formato,
            out string extension)
        {
            IExportadorReporte exportador =
                ObtenerExportador(formato);

            extension = exportador.Extension;

            return exportador.Exportar(reporte);
        }

        public string ObtenerFiltroDialogo(string formato)
        {
            return ObtenerExportador(formato).FiltroDialogo;
        }

        private IExportadorReporte ObtenerExportador(string formato)
        {
            if (string.IsNullOrWhiteSpace(formato))
            {
                throw new ArgumentException(
                    "Debes indicar un formato de exportación.");
            }

            if (!_exportadores.TryGetValue(
                formato,
                out IExportadorReporte exportador))
            {
                throw new InvalidOperationException(
                    "El formato de exportación seleccionado no está disponible.");
            }

            return exportador;
        }
    }
}