using System;
using System.Threading.Tasks;
using TRAMADE.Models;
using TRAMADE.Repositories;

namespace TRAMADE.Services
{
    /// <summary>
    /// Reemplaza frmLogin.login() + RegistrarIntentoFallido() + ResetearBloqueo().
    /// Misma política exacta de seguridad:
    ///   - Máximo 3 intentos fallidos antes de bloquear.
    ///   - 1er bloqueo: 10 min. 2do bloqueo: 20 min. 3er bloqueo: cuenta desactivada (id_estado = 2).
    ///   - 3 reintentos de conexión con 2s de espera entre cada uno si hay error de BD.
    /// La vista (frmLogin) ya NO decide nada de esto — solo recibe un
    /// LoginResultado y muestra el MessageBox correspondiente.
    /// </summary>
    public class AuthService : IAuthService
    {
        private const int MAX_INTENTOS = 3;
        private const int INTENTOS_CONEXION = 3;
        private const int ESPERA_MS = 2000;

        private readonly IUsuarioRepository _usuarioRepo;

        public AuthService(IUsuarioRepository usuarioRepo = null)
        {
            _usuarioRepo = usuarioRepo ?? new UsuarioRepository();
        }

        public async Task<LoginResultado> IniciarSesionAsync(string usuario, string contraseña)
        {
            // Misma validación de entrada que ya tenías (clsValidar ya muestra su propio mensaje).
            if (clsValidar.ContieneEspacios(contraseña, "Contraseña"))
                return LoginResultado.Simple(LoginEstado.ValidacionFallida);

            for (int intento = 0; intento < INTENTOS_CONEXION; intento++)
            {
                try
                {
                    clsUsuarioLoginModel u = await _usuarioRepo.ObtenerPorCorreoAsync(usuario);

                    if (u == null)
                        return LoginResultado.Simple(LoginEstado.UsuarioOContraseñaIncorrectos);

                    // ── Cuenta inactiva (bloqueo definitivo) ──────────────────
                    if (u.IdEstado == 2)
                        return LoginResultado.Simple(LoginEstado.CuentaInactiva);

                    // ── Bloqueo temporal activo ────────────────────────────────
                    if (u.BloqueadoHasta.HasValue && u.BloqueadoHasta.Value > DateTime.Now)
                    {
                        return new LoginResultado
                        {
                            Estado = LoginEstado.CuentaBloqueadaTemporal,
                            TiempoRestanteBloqueo = u.BloqueadoHasta.Value - DateTime.Now
                        };
                    }

                    // ── Verificar contraseña con hash ──────────────────────────
                    bool credencialesOk = clsSeguridad.VerificarPassword(contraseña, u.PasswordHash);

                    if (!credencialesOk)
                        return await RegistrarIntentoFallidoAsync(u);

                    // ── Login correcto: resetear contadores ────────────────────
                    await _usuarioRepo.ResetearBloqueoAsync(u.IdUsuario);
                    return LoginResultado.Exitoso(u);
                }
                catch (Exception ex)
                {
                    if (intento < INTENTOS_CONEXION - 1)
                        await Task.Delay(ESPERA_MS);
                    else
                        return new LoginResultado { Estado = LoginEstado.ErrorConexion, MensajeError = ex.Message };
                }
            }

            // No debería llegar aquí, pero por si acaso (mismo comportamiento defensivo).
            return new LoginResultado { Estado = LoginEstado.ErrorConexion, MensajeError = "No se pudo completar el inicio de sesión." };
        }

        /// <summary>Réplica exacta de RegistrarIntentoFallido(idUsuario, intentosActuales, nivelBloqueoActual).</summary>
        private async Task<LoginResultado> RegistrarIntentoFallidoAsync(clsUsuarioLoginModel u)
        {
            int nuevosIntentos = u.IntentosFallidos + 1;

            if (nuevosIntentos >= MAX_INTENTOS)
            {
                int nuevoNivel = u.NivelBloqueo + 1;

                if (nuevoNivel >= 3)
                {
                    await _usuarioRepo.DesactivarCuentaAsync(u.IdUsuario, nuevoNivel);
                    return LoginResultado.Simple(LoginEstado.CuentaDesactivada);
                }

                int minutosBloqueo = nuevoNivel == 1 ? 10 : 20;
                DateTime hasta = DateTime.Now.AddMinutes(minutosBloqueo);

                await _usuarioRepo.ActualizarBloqueoAsync(u.IdUsuario, nuevoNivel, hasta);

                return new LoginResultado
                {
                    Estado = LoginEstado.CuentaBloqueadaAhora,
                    MinutosBloqueo = minutosBloqueo
                };
            }

            await _usuarioRepo.ActualizarIntentosFallidosAsync(u.IdUsuario, nuevosIntentos);

            return new LoginResultado
            {
                Estado = LoginEstado.UsuarioOContraseñaIncorrectos,
                IntentosRestantes = MAX_INTENTOS - nuevosIntentos
            };
        }
    }
}
