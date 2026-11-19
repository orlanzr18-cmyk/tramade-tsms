using System;
using System.Collections.Generic;
using System.Data;
using TRAMADE.Models;
using TRAMADE.Repositories;

namespace TRAMADE.Services
{
    public class clsClienteService
    {
        private readonly ClienteRepository repo = new ClienteRepository();

        public clsClienteModel BuscarCliente(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento)) return null;
            return repo.BuscarPorDocumento(documento.Trim());
        }

        public bool RegistrarCliente(clsClienteModel cliente)
        {
            return repo.Insertar(cliente);
        }

        public bool ActualizarCliente(clsClienteModel cliente)
        {
            return repo.Actualizar(cliente);
        }

        public bool AutorizarCliente(int idCliente)
        {
            return repo.Autorizar(idCliente);
        }

        public bool DenegarCliente(int idCliente)
        {
            return repo.Denegar(idCliente);
        }

        public DataTable ObtenerDepartamentos()
        {
            return repo.ObtenerDepartamentos();
        }

        public DataTable ObtenerCiudades(int idDepartamento)
        {
            return repo.ObtenerCiudadesPorDepartamento(idDepartamento);
        }

        public DataTable ObtenerTiposCliente()
        {
            return repo.ObtenerTiposCliente();
        }

        public DataTable ObtenerUltimosClientes()
        {
            return repo.ObtenerUltimosClientes();
        }

        public DataTable ObtenerVistaAprobacion()
        {
            return repo.ObtenerVistaAprobacion();
        }

        public DataTable BuscarEnAprobacionPorNombre(string texto)
        {
            return repo.BuscarEnVistaAprobacionPorNombre(texto);
        }

        public DataTable ObtenerVistaSeguimiento()
        {
            return repo.ObtenerVistaSeguimiento();
        }

        public DataTable ObtenerPorcentajeEstados()
        {
            return repo.ObtenerPorcentajeEstados();
        }

        public DataTable ObtenerTopDepartamentos()
        {
            return repo.ObtenerTopDepartamentos();
        }

        public dynamic ObtenerEstadisticas()
        {
            return repo.ObtenerEstadisticas();
        }

        public DataTable ObtenerClienteCompletoPorDocumento(string documento)
        {
            return repo.ObtenerClienteCompletoPorDocumento(documento);
        }

        public List<string> ObtenerSugerenciasAutoComplete()
        {
            return repo.ObtenerSugerenciasAutoComplete();
        }

        public DataTable BuscarEnVistaSeguimientoPorNombre(string texto)
        {
            return repo.BuscarEnVistaSeguimientoPorNombre(texto);
        }
    }


}