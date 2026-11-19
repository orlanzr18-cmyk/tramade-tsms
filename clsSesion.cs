using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRAMADE
{
    internal class clsSesion
    {
        public static int id_usuario;
        public static string nombre_usuario;
        public static string nombre_sucursal;
        public static int id_rol;
        public static int id_sucursal;

        public clsSesion(){ }
        public void setId_usuario(int valor){id_usuario = valor;}
        public void setNombre_usuario(string valor) {nombre_usuario = valor; }
        public void setId_rol(int valor) { id_rol = valor; }
        public void setNombre_sucursal(string valor) { nombre_sucursal = valor; }
        public void setId_sucursal(int valor) { id_sucursal = valor; }
    }
}
