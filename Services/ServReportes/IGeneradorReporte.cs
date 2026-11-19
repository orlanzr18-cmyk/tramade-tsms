using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reporting.WinForms;
using TRAMADE.Models.ModelsReportes;

namespace TRAMADE.Services.ServReportes
{
    public interface IGeneradorReporte
    {
        LocalReport Generar(clsReporteItemModel reporte);
    }
}
