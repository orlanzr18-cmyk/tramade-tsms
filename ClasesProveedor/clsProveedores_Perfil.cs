using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.Formulario_Proveedores.Clases
{
    /// <summary>
    /// Clase que representa el perfil completo de un proveedor y contiene métodos
    /// para obtener y cargar sus datos en controles de interfaz.
    /// Hereda los campos básicos de <see cref="clsProveedores"/> e añade propiedades descriptivas.
    /// </summary>
    internal class clsProveedores_Perfil : clsProveedores
    {
        /// <summary>
        /// Conexión a la base de datos utilizada por los métodos de perfil.
        /// </summary>
        clsConexion ObjConexion = new clsConexion();

        /// <summary>
        /// Descripción textual de la clasificación del proveedor (ej. "MADERA").
        /// </summary>
        public string descripcion_clasificacion { get; set; }

        /// <summary>
        /// Descripción de los términos de pago aplicables al proveedor.
        /// </summary>
        public string descripcion_terminos_de_pago { get; set; }

        /// <summary>
        /// Días de crédito asignados al proveedor (si aplica).
        /// </summary>
        public int dias_credito { get; set; }

        // ─── OBTENER DATOS COMPLETOS DEL PROVEEDOR ───────────────────
        /// <summary>
        /// Obtiene el perfil completo del proveedor identificado por <paramref name="idProveedor"/>
        /// consultando el procedimiento almacenado "PA_OBTENER_PERFIL_PROVEEDOR".
        /// </summary>
        /// <param name="idProveedor">Identificador del proveedor a consultar.</param>
        /// <returns>
        /// Un objeto <see cref="clsProveedores_Perfil"/> con los datos cargados,
        /// o null si no se encontraron registros o ocurrió un error.
        /// </returns>
        public clsProveedores_Perfil ObtenerPerfilProveedor(int idProveedor)
        {
            clsProveedores_Perfil ObjPerfil = null;
            try
            {
                ObjConexion.Abrir();
                SqlCommand cmd = new SqlCommand("PA_OBTENER_PERFIL_PROVEEDOR", ObjConexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_proveedor", idProveedor);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ObjPerfil = new clsProveedores_Perfil();
                    ObjPerfil.id_proveedor = Convert.ToInt32(reader["id_proveedor"]);
                    ObjPerfil.nombre_comercial_proveedor = reader["nombre_comercial_proveedor"].ToString();
                    ObjPerfil.razon_social_proveedor = reader["razon_social_proveedor"].ToString();
                    ObjPerfil.direccion_proveedor = reader["direccion_proveedor"].ToString();
                    ObjPerfil.rtn_proveedor = reader["rtn_proveedor"].ToString();
                    ObjPerfil.id_clasificacion_proveedor = Convert.ToInt32(reader["id_clasificacion_proveedor"]);
                    ObjPerfil.id_estado = Convert.ToInt32(reader["id_estado"]);
                    ObjPerfil.id_terminos_de_pago_proveedor = Convert.ToInt32(reader["id_terminos_de_pago_proveedor"]);
                    ObjPerfil.contacto_proveedor = reader["contacto_proveedor"].ToString();
                    ObjPerfil.correo_electronico_proveedor = reader["correo_electronico_proveedor"].ToString();
                    ObjPerfil.descripcion_clasificacion = reader["descripcion_clasificacion_proveedor"].ToString();
                    ObjPerfil.descripcion_terminos_de_pago = reader["descripcion_terminos_de_pago"].ToString();
                    ObjPerfil.dias_credito = Convert.ToInt32(reader["dias_credito"]);
                }
                reader.Close(); // ← quitado el else con MessageBox de debug
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener perfil: " + ex.Message);
            }
            finally
            {
                ObjConexion.Cerrar();
            }
            return ObjPerfil;
        }

        // ─── LLENAR lstInformacionGeneral ─────────────────────────────
        /// <summary>
        /// Llena el <see cref="KryptonListBox"/> con información general del proveedor.
        /// </summary>
        /// <param name="lst">Lista donde se insertarán las líneas de información.</param>
        /// <param name="ObjPerfil">Objeto que contiene los datos del proveedor.</param>
        public void CargarInformacionGeneral(KryptonListBox lst, clsProveedores_Perfil ObjPerfil)
        {
            lst.Items.Clear();
            lst.Items.Add("ID Proveedor:       " + ObjPerfil.id_proveedor);
            lst.Items.Add("Nombre Comercial:   " + ObjPerfil.nombre_comercial_proveedor);
            lst.Items.Add("Razón Social:       " + ObjPerfil.razon_social_proveedor);
            lst.Items.Add("Dirección:          " + ObjPerfil.direccion_proveedor);
            lst.Items.Add("RTN:                " + ObjPerfil.rtn_proveedor);
        }

        // ─── LLENAR lstDatosdeContacto ────────────────────────────────
        /// <summary>
        /// Llena la lista con datos de contacto del proveedor.
        /// </summary>
        /// <param name="lst">Lista destino.</param>
        /// <param name="ObjPerfil">Perfil del proveedor con los datos de contacto.</param>
        public void CargarDatosContacto(KryptonListBox lst, clsProveedores_Perfil ObjPerfil)
        {
            lst.Items.Clear();
            lst.Items.Add("Contacto:           " + ObjPerfil.contacto_proveedor);
            lst.Items.Add("Correo Electrónico: " + ObjPerfil.correo_electronico_proveedor);
        }

        // ─── LLENAR lstClasificacion ──────────────────────────────────
        /// <summary>
        /// Llena la lista con la clasificación, términos de pago y días de crédito del proveedor.
        /// </summary>
        /// <param name="lst">Lista destino.</param>
        /// <param name="ObjPerfil">Perfil del proveedor con la información de clasificación.</param>
        public void CargarClasificacion(KryptonListBox lst, clsProveedores_Perfil ObjPerfil)
        {
            lst.Items.Clear();
            lst.Items.Add("Clasificación:      " + ObjPerfil.descripcion_clasificacion);
            lst.Items.Add("Términos de Pago:   " + ObjPerfil.descripcion_terminos_de_pago);
            lst.Items.Add("Días de Crédito:    " + ObjPerfil.dias_credito + " días");
        }

        // ─── CARGAR PERFIL COMPLETO ───────────────────────────────────
        /// <summary>
        /// Carga el perfil completo del proveedor y rellena los distintos listbox proporcionados.
        /// Muestra un mensaje si no se encuentran datos.
        /// </summary>
        /// <param name="idProveedor">Id del proveedor a cargar.</param>
        /// <param name="lstInformacionGeneral">ListBox para información general.</param>
        /// <param name="lstDatosdeContacto">ListBox para datos de contacto.</param>
        /// <param name="lstClasificacion">ListBox para clasificación y términos.</param>
        public void CargarPerfil(int idProveedor, KryptonListBox lstInformacionGeneral,
                                  KryptonListBox lstDatosdeContacto, KryptonListBox lstClasificacion)
        {
            clsProveedores_Perfil ObjDatos = ObtenerPerfilProveedor(idProveedor);

            if (ObjDatos != null)
            {
                CargarInformacionGeneral(lstInformacionGeneral, ObjDatos);
                CargarDatosContacto(lstDatosdeContacto, ObjDatos);
                CargarClasificacion(lstClasificacion, ObjDatos);
            }
            else
            {
                MessageBox.Show("No se encontraron datos del proveedor.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
      
    }
}
