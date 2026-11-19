using System;

namespace TRAMADE.Models.ModelsReportes
{
    public class clsReporteItemModel
    {
        public string TipoReporte { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public int IdSucursal { get; set; }

        public string NombreSucursal { get; set; }

        public string Reporte { get; set; }
    }
}