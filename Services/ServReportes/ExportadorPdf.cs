using Microsoft.Reporting.WinForms;
using System;

namespace TRAMADE.Services.ServReportes
{
    public class ExportadorPdf : IExportadorReporte
    {
        public string Formato => "PDF";
        public string Extension => ".pdf";
        public string FiltroDialogo => "Archivo PDF|*.pdf";

        public byte[] Exportar(LocalReport reporte)
        {
            if (reporte == null)
                throw new ArgumentNullException(nameof(reporte));

            string mimeType;
            string encoding;
            string extension;
            string[] streamIds;
            Warning[] warnings;

            string deviceInfo =
                "<DeviceInfo>" +
                "<OutputFormat>PDF</OutputFormat>" +
                "</DeviceInfo>";

            return reporte.Render(
                "PDF",
                deviceInfo,
                out mimeType,
                out encoding,
                out extension,
                out streamIds,
                out warnings);
        }
    }
}