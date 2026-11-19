using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace TRAMADE
{
    internal static class clsCorreo
    {
        private static string CorreoRemitente => ConfigurationManager.AppSettings["CorreoRemitente"];
        private static string ClaveApp => ConfigurationManager.AppSettings["ClaveAppCorreo"];

        public static void EnviarCodigo(string correoDestino, string codigo)
        {
            using (MailMessage mensaje = new MailMessage())
            {
                mensaje.From = new MailAddress(CorreoRemitente, "TRAMADE S.A");
                mensaje.To.Add(correoDestino);
                mensaje.Subject = "Código de recuperación de contraseña - TRAMADE";
                mensaje.Body = ConstruirHtml(codigo);
                mensaje.IsBodyHtml = true; // ← clave

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(CorreoRemitente, ClaveApp);
                    smtp.EnableSsl = true;
                    smtp.Send(mensaje);
                }
            }
        }

        private static string ConstruirHtml(string codigo)
        {
            return $@"
<!DOCTYPE html>
<html>
<body style=""margin:0; padding:0; background-color:#F0EDE9; font-family:Segoe UI, Arial, sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#F0EDE9; padding:30px 0;"">
    <tr>
      <td align=""center"">
        <table width=""420"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#FFFFFF; border-radius:8px; overflow:hidden; box-shadow:0 2px 8px rgba(0,0,0,0.08);"">
          <tr>
            <td style=""background-color:#8B5E3C; padding:22px; text-align:center;"">
              <span style=""color:#FFFFFF; font-size:18px; font-weight:bold; letter-spacing:1px;"">TRAMADE S.A</span>
            </td>
          </tr>
          <tr>
            <td style=""padding:32px 30px;"">
              <p style=""font-size:15px; color:#3A2E22; margin:0 0 6px 0; font-weight:bold;"">
                Recuperación de contraseña
              </p>
              <p style=""font-size:13px; color:#7A7268; margin:0 0 24px 0; line-height:1.5;"">
                Usa el siguiente código para continuar con el restablecimiento de tu contraseña. Este código expira en <b>1 minuto</b>.
              </p>
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td align=""center"" style=""padding:10px 0 26px 0;"">
                    <span style=""display:inline-block; background-color:#F0EDE9; border:1.5px solid #D8B98C; border-radius:8px; padding:14px 28px; font-size:28px; font-weight:bold; letter-spacing:8px; color:#8B5E3C;"">
                      {codigo}
                    </span>
                  </td>
                </tr>
              </table>
              <p style=""font-size:12px; color:#A39C90; margin:0; line-height:1.5;"">
                Si tú no solicitaste este cambio, puedes ignorar este correo de forma segura. Tu contraseña actual seguirá funcionando.
              </p>
            </td>
          </tr>
          <tr>
            <td style=""background-color:#F0EDE9; padding:14px; text-align:center;"">
              <span style=""font-size:11px; color:#A39C90;"">TRAMADE S.A &middot; Sistema de gestión interna</span>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}