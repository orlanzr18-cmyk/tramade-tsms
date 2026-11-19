using System;
using TRAMADE.Models;

namespace TRAMADE.Services
{
    /// <summary>Todos los desenlaces posibles de un intento de login, 1 a 1 con los mensajes originales.</summary>
    public enum LoginEstado
    {
        Exitoso,
        ValidacionFallida,          
        UsuarioOContraseñaIncorrectos,
        CuentaInactiva,
        CuentaBloqueadaTemporal,
        CuentaBloqueadaAhora,       
        CuentaDesactivada,          
        ErrorConexion
    }

    /// <summary>
    /// DTO de salida de AuthService.IniciarSesionAsync. Trae todo lo que la
    /// vista necesita para armar el MessageBox exacto que ya tenías, sin que
    /// la vista tenga que conocer ninguna regla de negocio.
    /// </summary>
    public class LoginResultado
    {
        public LoginEstado Estado { get; set; }
        public clsUsuarioLoginModel Usuario { get; set; }      
        public TimeSpan? TiempoRestanteBloqueo { get; set; }   
        public int? MinutosBloqueo { get; set; }               
        public int? IntentosRestantes { get; set; }            
        public string MensajeError { get; set; }               

        public static LoginResultado Exitoso(clsUsuarioLoginModel u) => new LoginResultado { Estado = LoginEstado.Exitoso, Usuario = u };
        public static LoginResultado Simple(LoginEstado estado) => new LoginResultado { Estado = estado };
    }

    public interface IAuthService
    {
        System.Threading.Tasks.Task<LoginResultado> IniciarSesionAsync(string usuario, string contraseña);
    }
}
