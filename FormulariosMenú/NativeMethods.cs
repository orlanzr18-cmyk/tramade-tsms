using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

internal static class NativeMethods
{
    public const int WM_SETREDRAW = 0x000B;

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int msg, bool wParam, int lParam);
}
