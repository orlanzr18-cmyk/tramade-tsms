using Microsoft.Reporting.WinForms;
using System;
using System.Collections.Generic;
using TRAMADE.Models;
using TRAMADE.Models.ModelsReportes;
using TRAMADE.Repositories;
using TRAMADE.Repositories.RepoReportes;

namespace TRAMADE.Services.ServReportes
{
    public class ReporteService
    {
        private readonly Dictionary<string, IGeneradorReporte> _generadores;
        private readonly ISucursalRepository _sucursalRepository;
        public ReporteService()
        {
            IReporteRepository reporteRepository =
                new ReporteRepository();

            _sucursalRepository =
                new SucursalRepository();

            _generadores =
                new Dictionary<string, IGeneradorReporte>(
                    StringComparer.OrdinalIgnoreCase)
                {
                    {
                        "Ventas por período",
                        new GeneradorVentasService(reporteRepository)
                    },
                    {
                        "Compras por período",
                        new GeneradorComprasService(reporteRepository)
                    },
                    {
                        "Stock por sucursal",
                        new GeneradorStockService(reporteRepository)
                    },
                    {
                        "Productos más vendidos",
                        new GeneradorProductosService(reporteRepository)
                    }
                };
        }

        public List<clsSucursalModel> ObtenerSucursales()
        {
            List<clsSucursalModel> sucursales =
                _sucursalRepository.ObtenerSucursales();

            sucursales.Insert(0, new clsSucursalModel
            {
                Id = 0,
                Nombre = "Todas"
            });

            return sucursales;
        }

        public List<string> ObtenerTiposReporte()
        {
            return new List<string>
            {
                "Ventas por período",
                "Compras por período",
                "Stock por sucursal",
                "Productos más vendidos"
            };
        }

        public LocalReport GenerarReporte(
            clsReporteItemModel reporte)
        {
            if (reporte == null)
            {
                throw new ArgumentNullException(
                    nameof(reporte));
            }

            if (string.IsNullOrWhiteSpace(
                reporte.TipoReporte))
            {
                throw new ArgumentException(
                    "El tipo de reporte es obligatorio.");
            }

            if (!_generadores.TryGetValue(
                reporte.TipoReporte,
                out IGeneradorReporte generador))
            {
                throw new InvalidOperationException(
                    "El tipo de reporte seleccionado no existe.");
            }

            return generador.Generar(reporte);
        }
        public clsReporteItemModel CrearReporteItem(string tipoReporte, DateTime fechaInicio, DateTime fechaFin, int idSucursal, string nombreSucursal)
        {
            string descripcion;

            switch (tipoReporte)
            {
                case "Ventas por período":
                    descripcion = $"Ventas | {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy} | {nombreSucursal}";
                    break;

                case "Compras por período":
                    descripcion = $"Compras | {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy} | {nombreSucursal}";
                    break;

                case "Stock por sucursal":
                    descripcion = $"Stock | {nombreSucursal}";
                    break;

                case "Productos más vendidos":
                    descripcion = $"Productos más vendidos | {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy} | {nombreSucursal}";
                    break;

                default:
                    throw new InvalidOperationException("El tipo de reporte seleccionado no es válido.");
            }

            return new clsReporteItemModel
            {
                Reporte = descripcion,
                TipoReporte = tipoReporte,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                IdSucursal = idSucursal,
                NombreSucursal = nombreSucursal
            };
        }

        public bool RequiereFechas(string tipoReporte)
        {
            return tipoReporte != "Stock por sucursal";
        }
    }
}