using Microsoft.Reporting.WinForms;

namespace TRAMADE.Services.ServReportes
{
    public interface IExportadorReporte
    {
        string Formato { get; }

        string Extension { get; }

        string FiltroDialogo { get; }

        byte[] Exportar(LocalReport reporte);
    }
}