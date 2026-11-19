using Krypton.Toolkit;
using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace TRAMADE
{
    internal class clsPermisos
    {
        public clsPermisos() { }

        public const int administrador = 1;
        public const int gerente = 2;
        public const int empleado = 3;

        public static bool esAdministrador => clsSesion.id_rol == administrador;
        public static bool esGerente => clsSesion.id_rol == gerente;
        public static bool esEmpleado => clsSesion.id_rol == empleado;

        public static void Ocultar(params KryptonButton[] botones)
        {
            foreach (KryptonButton btn in botones)
                btn.Visible = false;
        }


    }
}
