using System;

namespace TRAMADE.Models
{
    /// <summary>
    /// Representa la fila que hoy trae "VistaUsuariosLoginCompleta" en
    /// frmLogin.login(). Solo atributos — ninguna regla de negocio aquí.
    /// </summary>
    public class clsUsuarioLoginModel
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public int IdRol { get; set; }
        public int IdSucursal { get; set; }
        public string NombreSucursal { get; set; }
        public string CorreoUsuario { get; set; }
        public string PasswordHash { get; set; }
        public int IntentosFallidos { get; set; }
        public int NivelBloqueo { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
        public int IdEstado { get; set; }
    }
}
