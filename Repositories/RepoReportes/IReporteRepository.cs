using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRAMADE.Repositories.RepoReportes
{
    public interface IReporteRepository
    {
        DataTable ObtenerVentas(DateTime fechaInicio, DateTime fechaFin, int idSucursal);

        DataTable ObtenerCompras( DateTime fechaInicio,DateTime fechaFin,int idSucursal);

        DataTable ObtenerStock( int idSucursal);

        DataTable ObtenerProductosMasVendidos(DateTime fechaInicio,DateTime fechaFin,int idSucursal);
    }
}
