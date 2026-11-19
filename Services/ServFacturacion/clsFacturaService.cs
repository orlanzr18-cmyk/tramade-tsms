using System;
using System.Linq;
using TRAMADE.Models;
using TRAMADE.Repositories;

namespace TRAMADE.Services
{
    public class clsFacturaService
    {

        private readonly clsFacturaRepository repoFactura = new clsFacturaRepository();
        private readonly ClienteOcasionalRepository repoOcasional = new ClienteOcasionalRepository();

        private readonly clsFacturaRepository repo = new clsFacturaRepository();
        private readonly ClienteRepository repoCliente = new ClienteRepository();
        public int ObtenerNuevoNumero() => repo.ObtenerCorrelativo();

        public int ProcesarVenta(clsFacturaModel factura)
        {
            // Lógica de negocio (Hallazgo TS-51-H16)
            if (factura.Detalles.Count == 0) throw new Exception("La factura no tiene productos.");
            if (factura.Total <= 0) throw new Exception("El total debe ser mayor a cero.");

            if (factura.IdFormaPago == 1 && factura.IdCliente.HasValue)
            {
                if (repoCliente.TieneMora(factura.IdCliente.Value))
                    throw new Exception("El cliente tiene facturas vencidas. No puede facturar a crédito hasta regularizar su saldo.");
            }

            return repo.GuardarFacturaCompleta(factura);
        }

        public clsFacturaConsultaModel CargarFacturaCompleta(int idFactura)
        {
            // 1. Cargamos el maestro desde el Repo
            var factura = repoFactura.ObtenerMaestro(idFactura);
            if (factura == null) return null;

            // 2. Cargamos los detalles
            factura.Detalles = repoFactura.ObtenerDetalles(idFactura);

            // 3. APLICAR REGLA DE NEGOCIO (Recargo del 10% para Crédito)
            // Suponiendo que 1 es Crédito (ajusta el ID según tu tabla)
            if (factura.IdFormaPago == 1)
            {
                // Recalculamos sobre los detalles para asegurar exactitud
                decimal subtotalBase = factura.Detalles.Sum(d => d.Subtotal);

                factura.Subtotal = subtotalBase * 1.10m; // Aplicamos el 10%
                factura.Impuesto = factura.Subtotal * 0.15m; // 15% de ISV
                factura.Total = factura.Subtotal + factura.Impuesto;
            }

            // 4. Resolución de datos de cliente ocasional
            if (string.IsNullOrEmpty(factura.NombreCliente))
            {
                var ocasional = repoOcasional.ObtenerPorFactura(idFactura);
                if (ocasional != null)
                {
                    factura.NombreCliente = ocasional.Nombre;
                    factura.DireccionCliente = ocasional.Direccion;
                    factura.DniRtnCliente = ocasional.Dni;
                }
            }
            return factura;
        }
    }
}