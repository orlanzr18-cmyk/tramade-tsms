using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.Formularios_Login__Menú.Clases
{
    /// <summary>
    /// Proporciona métodos de extensión para manipular propiedades visuales avanzadas 
    /// de los contenedores MDI (Multiple Document Interface) mediante llamadas a la API de Windows (User32.dll).
    /// </summary>
    public static class mdiProperties
    {

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_CLIENTEDGE = 0X200;
        private const uint SWP_NOSIZE = 0X0001;
        private const uint SWP_NOMOVE = 0X0002;
        private const uint SWP_NOZORDER = 0X0004;
        private const uint SWO_NOACTVATE = 0X0010;
        private const uint SWP_FRAMECHANGED = 0X0020;
        private const uint SWO_NOOWNERZORDER = 0X0200;

        /// <summary>
        /// Método de extensión para <see cref="frmMenuPrincipal"/> que permite mostrar u ocultar 
        /// el borde interno (Bevel) del contenedor MDI.
        /// </summary>
        /// <remarks>
        /// Por defecto, los formularios MDI en WinForms muestran un borde 3D hacia adentro. 
        /// Este método permite eliminarlo para lograr un diseño "Flat" (plano) más moderno.
        /// </remarks>
        /// <param name="form">El formulario principal que actúa como contenedor MDI.</param>
        /// <param name="show">True para mostrar el borde 3D, False para ocultarlo y dejar un fondo plano.</param>
        /// <returns>True si se encontró el control MdiClient y se aplicó el cambio; de lo contrario, False.</returns>
        public static bool SetBevel(this frmMenuPrincipal form, bool show)
        {
            foreach (Control c in form.Controls)
            {
                MdiClient client = c as MdiClient;
                if (client != null)
                {
                    int windowLong = GetWindowLong(c.Handle, GWL_EXSTYLE);
                    if (show)
                    {
                        windowLong |= WS_EX_CLIENTEDGE;
                    }
                    else
                    {
                        windowLong &= WS_EX_CLIENTEDGE;
                    }
                    SetWindowLong(c.Handle, GWL_EXSTYLE, windowLong);
                    SetWindowPos(client.Handle, IntPtr.Zero, 0, 0, 0, 0,
                        SWO_NOACTVATE | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER |
                        SWO_NOOWNERZORDER | SWP_FRAMECHANGED);

                    return true;
                }
            }
            return false;
        }
    }
}


