using System;
using System.Data;
using TRAMADE.DatasSet_Reportes;
using TRAMADE.Repositories.RepoReportes;

namespace TRAMADE.Repositories.RepoReportes
{
    public class ReporteRepository : IReporteRepository
    {
        public DataTable ObtenerVentas(
            DateTime fechaInicio,
            DateTime fechaFin,
            int idSucursal)
        {
            dsVentas ds = new dsVentas();

            var adapter =
                new DatasSet_Reportes
                .dsVentasTableAdapters
                .DataTable1TableAdapter();

            adapter.Fill(
                ds.DataTable1,
                fechaInicio,
                fechaFin,
                idSucursal);

            return ds.DataTable1;
        }

        public DataTable ObtenerCompras(
            DateTime fechaInicio,
            DateTime fechaFin,
            int idSucursal)
        {
            dsCompras ds = new dsCompras();

            var adapter =
                new DatasSet_Reportes
                .dsComprasTableAdapters
                .DataTable1TableAdapter();

            adapter.Fill(
                ds.DataTable1,
                fechaInicio,
                fechaFin,
                idSucursal);

            return ds.DataTable1;
        }

        public DataTable ObtenerStock(int idSucursal)
        {
            dsStock ds = new dsStock();

            var adapter =
                new DatasSet_Reportes
                .dsStockTableAdapters
                .DataTable1TableAdapter();

            adapter.Fill(
                ds.DataTable1,
                idSucursal);

            return ds.DataTable1;
        }

        public DataTable ObtenerProductosMasVendidos(
            DateTime fechaInicio,
            DateTime fechaFin,
            int idSucursal)
        {
            dsProductos ds = new dsProductos();

            var adapter =
                new DatasSet_Reportes
                .dsProductosTableAdapters
                .DataTable1TableAdapter();

            adapter.Fill(
                ds.DataTable1,
                fechaInicio,
                fechaFin,
                idSucursal);

            return ds.DataTable1;
        }
    }
}