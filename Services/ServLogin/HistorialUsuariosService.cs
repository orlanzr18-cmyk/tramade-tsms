using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TRAMADE.Services
{
    public interface IHistorialUsuariosService
    {
        List<string> ObtenerSugerencias();
        void Guardar(string usuario);
    }

    /// <summary>
    /// Reemplaza CargarAutoCompleteUsuarios()/GuardarUsuarioEnHistorial() de frmLogin.
    /// No toca base de datos (por eso no es un Repository) — persiste en un
    /// archivo de texto en AppData, igual que el original. Mismo path, mismo
    /// comportamiento silencioso ante errores (no se quería romper el login
    /// por un problema de disco).
    /// </summary>
    public class HistorialUsuariosService : IHistorialUsuariosService
    {
        private readonly string _rutaUsuarios = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TRAMADE", "usuarios.txt");

        public List<string> ObtenerSugerencias()
        {
            try
            {
                return File.Exists(_rutaUsuarios)
                    ? File.ReadAllLines(_rutaUsuarios).ToList()
                    : new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        public void Guardar(string usuario)
        {
            try
            {
                string carpeta = Path.GetDirectoryName(_rutaUsuarios);
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var usuarios = File.Exists(_rutaUsuarios)
                    ? File.ReadAllLines(_rutaUsuarios).ToList()
                    : new List<string>();

                if (!usuarios.Contains(usuario, StringComparer.OrdinalIgnoreCase))
                {
                    usuarios.Add(usuario);
                    File.WriteAllLines(_rutaUsuarios, usuarios);
                }
            }
            catch { /* igual que el original: no se rompe el login por esto */ }
        }
    }
}
