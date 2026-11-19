using Microsoft.Reporting.WinForms;
using System;

namespace TRAMADE.Services.ServReportes
{
    public class ExportadorExcel : IExportadorReporte
    {
        public string Formato => "Excel";
        public string Extension => ".xlsx";
        public string FiltroDialogo => "Archivo Excel|*.xlsx";

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
                "<OutputFormat>EXCELOPENXML</OutputFormat>" +
                "</DeviceInfo>";

            return reporte.Render(
                "EXCELOPENXML",
                deviceInfo,
                out mimeType,
                out encoding,
                out extension,
                out streamIds,
                out warnings);
        }
    }
}