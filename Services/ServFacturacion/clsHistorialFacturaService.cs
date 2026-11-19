using System;
using System.Collections.Generic;
using TRAMADE.Models;
using TRAMADE.Repositories;

namespace TRAMADE.Services
{
    public class clsHistorialFacturaService
    {
        private readonly clsHistorialFacturaRepository repo = new clsHistorialFacturaRepository();

        public int ObtenerTotal() => repo.ContarTotal();

        public List<clsHistorialFacturaModel> Consultar(string textoFiltro, DateTime? fechaFiltro, bool ascendente)
        {
            string filtroLimpio = textoFiltro;

            // Lógica de negocio: Si el usuario escribe el formato INV/2026/0001, 
            // extraemos solo el número para la búsqueda en BD
            if (!string.IsNullOrEmpty(textoFiltro) && textoFiltro.Contains("INV/"))
            {
                filtroLimpio = textoFiltro.Replace("INV/2026/", "").TrimStart('0');
            }

            return repo.ObtenerHistorial(filtroLimpio, fechaFiltro, ascendente);
        }
    }
}