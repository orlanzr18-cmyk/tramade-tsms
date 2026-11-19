using Org.BouncyCastle.Crypto.Generators;

namespace TRAMADE
{
    internal static class clsSeguridad
    {
        public static string HashPassword(string passwordPlano)
        {
            return BCrypt.Net.BCrypt.HashPassword(passwordPlano);
        }

        public static bool VerificarPassword(string passwordIngresado, string hashGuardado)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(passwordIngresado, hashGuardado);
            }
            catch
            {
                // Si el valor guardado no es un hash válido (ej. aún no migrado), falla seguro
                return false;
            }
        }

        public static bool EsHashBCrypt(string valor)
        {
            return !string.IsNullOrEmpty(valor) &&
                   (valor.StartsWith("$2a$") || valor.StartsWith("$2b$") || valor.StartsWith("$2y$"));
        }
    }
}