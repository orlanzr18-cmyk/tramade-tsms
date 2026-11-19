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
    /// Representa la entidad y operaciones relacionadas con los proveedores.
    /// Contiene propiedades públicas para los datos del proveedor y métodos
    /// para obtener, buscar y cambiar el estado de proveedores en la base de datos.
    /// </summary>
    public class clsProveedores
    {

        /// <summary>
        /// Identificador del proveedor.
        /// </summary>
        public int id_proveedor;

        /// <summary>
        /// Nombre comercial del proveedor.
        /// </summary>
        public string nombre_comercial_proveedor;

        /// <summary>
        /// Razón social del proveedor.
        /// </summary>
        public string razon_social_proveedor;

        /// <summary>
        /// Dirección fiscal o principal del proveedor.
        /// </summary>
        public string direccion_proveedor;

        /// <summary>
        /// RTN (Registro Tributario) del proveedor.
        /// </summary>
        public string rtn_proveedor;

        /// <summary>
        /// Identificador de la clasificación del proveedor.
        /// </summary>
        public int id_clasificacion_proveedor;

        /// <summary>
        /// Identificador del estado del proveedor (ej. activo/inactivo).
        /// </summary>
        public int id_estado;

        /// <summary>
        /// Identificador de los términos de pago aplicables al proveedor.
        /// </summary>
        public int id_terminos_de_pago_proveedor;

        /// <summary>
        /// Contacto principal del proveedor (teléfono o persona de contacto).
        /// </summary>
        public string contacto_proveedor;

        /// <summary>
        /// Correo electrónico central del proveedor.
        /// </summary>
        public string correo_electronico_proveedor;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="clsProveedores"/> con estado por defecto activo (1).
        /// </summary>
        public clsProveedores()
        {
            id_estado = 5;
        }

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="clsProveedores"/> con los valores proporcionados.
        /// </summary>
        /// <param name="nombre">Nombre comercial del proveedor.</param>
        /// <param name="razon">Razón social del proveedor.</param>
        /// <param name="direccion">Dirección del proveedor.</param>
        /// <param name="rtn">RTN del proveedor.</param>
        /// <param name="clasificacion">Id de clasificación del proveedor.</param>
        /// <param name="terminos">Id de términos de pago del proveedor.</param>
        /// <param name="contacto">Contacto del proveedor.</param>
        /// <param name="correo">Correo electrónico del proveedor.</param>
        public clsProveedores(string nombre, string razon, string direccion, string rtn, int clasificacion, int terminos, string contacto, string correo)
        {
            nombre_comercial_proveedor = nombre;
            razon_social_proveedor = razon;
            direccion_proveedor = direccion;
            rtn_proveedor = rtn;
            id_clasificacion_proveedor = clasificacion;
            id_estado = 5;
            id_terminos_de_pago_proveedor = terminos;
            contacto_proveedor = contacto;
            correo_electronico_proveedor = correo;
        }

        // ─── OBTENER TABLA DE PROVEEDORES ────────────────────────────
        /// <summary>
        /// Obtiene la tabla completa de proveedores desde la vista 'VistaProveedorTabla'.
        /// </summary>
        /// <returns>DataTable con los registros de proveedores.</returns>
        public DataTable ObtenerProveedores()
        {
            clsConexion ObjConexion = new clsConexion();
            DataTable dt = new DataTable();
            try
            {
                ObjConexion.Abrir();
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM VistaProveedorTabla", ObjConexion.SqlC);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener proveedores: " + ex.Message);
            }
            finally
            {
                ObjConexion.Cerrar();
            }
            return dt;
        }

        // ─── BUSCAR PROVEEDORES ───────────────────────────────────────
        /// <summary>
        /// Busca proveedores cuyo nombre comercial, razón social o RTN contengan el criterio indicado.
        /// </summary>
        /// <param name="criterio">Texto a buscar en los campos del proveedor.</param>
        /// <returns>DataTable con los resultados que coinciden con el criterio.</returns>
        public DataTable BuscarProveedores(string criterio)
        {
            clsConexion ObjConexion = new clsConexion();
            DataTable dt = new DataTable();
            try
            {
                ObjConexion.Abrir();
                SqlCommand cmd = new SqlCommand(@"
                    SELECT * FROM VistaProveedorTabla 
                    WHERE [Nombre Comercial] COLLATE SQL_Latin1_General_CP1_CI_AI LIKE @criterio
                       OR [RTN]              COLLATE SQL_Latin1_General_CP1_CI_AI LIKE @criterio", ObjConexion.SqlC);
                cmd.Parameters.AddWithValue("@criterio", "%" + criterio.Trim() + "%");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar: " + ex.Message);
            }
            finally
            {
                ObjConexion.Cerrar();
            }
            return dt;
        }

        // ─── CAMBIAR ESTADO INDIVIDUAL ────────────────────────────────
        /// <summary>
        /// Cambia el estado de un proveedor individual a activo o inactivo.
        /// </summary>
        /// <param name="idProveedor">Identificador del proveedor a modificar.</param>
        /// <param name="activar">True para activar, false para desactivar.</param>
        /// <returns>True si la operación fue exitosa, false en caso contrario.</returns>
        public bool CambiarEstado(int idProveedor, bool activar)
        {
            clsConexion ObjConexion = new clsConexion();
            try
            {
                ObjConexion.Abrir();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE PROVEEDOR SET id_estado = @estado WHERE id_proveedor = @id",
                    ObjConexion.SqlC);
                cmd.Parameters.AddWithValue("@estado", activar ? 5 : 6);
                cmd.Parameters.AddWithValue("@id", idProveedor);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar estado: " + ex.Message);
                return false;
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }

        // ─── OBTENER ESTADO ACTUAL ────────────────────────────────────
        /// <summary>
        /// Consulta si el proveedor especificado se encuentra en estado activo.
        /// </summary>
        /// <param name="idProveedor">Identificador del proveedor.</param>
        /// <returns>True si está activo (id_estado == 5), false en caso contrario.</returns>
        public bool EstaActivo(int idProveedor)
        {
            clsConexion ObjConexion = new clsConexion();
            try
            {
                ObjConexion.Abrir();
                SqlCommand cmd = new SqlCommand(
                    "SELECT id_estado FROM PROVEEDOR WHERE id_proveedor = @id",
                    ObjConexion.SqlC);
                cmd.Parameters.AddWithValue("@id", idProveedor);
                int estado = Convert.ToInt32(cmd.ExecuteScalar());
                return estado == 5; // 5 = ACTIVO proveedor
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener estado: " + ex.Message);
                return false;
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }

        // ─── OBTENER IDs SELECCIONADOS DEL DGV ───────────────────────
        /// <summary>
        /// Obtiene una lista de ids de proveedores seleccionados en el DataGridView
        /// (columna "Seleccionar" marcada).
        /// </summary>
        /// <param name="dgv">Control DataGridView que contiene la columna "Seleccionar" y "id_proveedor".</param>
        /// <returns>Lista de identificadores de proveedores seleccionados.</returns>
        private List<int> ObtenerIdsSeleccionados(DataGridView dgv)
        {
            List<int> ids = new List<int>();
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells["Seleccionar"].Value != null &&
                    Convert.ToBoolean(row.Cells["Seleccionar"].Value))
                {
                    ids.Add(Convert.ToInt32(row.Cells["id_proveedor"].Value));
                }
            }
            return ids;
        }

        // ─── CAMBIAR ESTADO MASIVO ────────────────────────────────────
        /// <summary>
        /// Cambia el estado de forma masiva para una lista de identificadores.
        /// Solo actualiza aquellos cuyo estado actual difiere del solicitado.
        /// </summary>
        /// <param name="ids">Lista de ids de proveedores a actualizar.</param>
        /// <param name="activar">True para activar, false para desactivar.</param>
        /// <returns>True si la operación fue exitosa, false en caso de error.</returns>
        /*private bool CambiarEstadoMasivo(List<int> ids, bool activar)
        {
            clsConexion ObjConexion = new clsConexion();
            try
            {
                ObjConexion.Abrir();
                int idEstado = activar ? 5 : 6;
                foreach (int id in ids)
                {
                    if (EstaActivo(id) != activar)
                    {
                        SqlCommand cmd = new SqlCommand(
                            "UPDATE PROVEEDOR SET id_estado = @idEstado WHERE id_proveedor = @id",
                            ObjConexion.SqlC);
                        cmd.Parameters.AddWithValue("@idEstado", idEstado);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar estados: " + ex.Message);
                return false;
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }*/
        private bool CambiarEstadoMasivo(List<int> ids, bool activar)
        {
            clsConexion ObjConexion = new clsConexion();
            try
            {
                ObjConexion.Abrir();
                int idEstado = activar ? 5 : 6;
                foreach (int id in ids)
                {
                    // ─── Verificar estado sin usar EstaActivo (misma conexión) ──
                    SqlCommand cmdVerificar = new SqlCommand(
                        "SELECT id_estado FROM PROVEEDOR WHERE id_proveedor = @id",
                        ObjConexion.SqlC);
                    cmdVerificar.Parameters.AddWithValue("@id", id);
                    int estadoActual = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                    bool estaActivoActual = (estadoActual == 5);
                    if (estaActivoActual != activar)
                    {
                        SqlCommand cmd = new SqlCommand(
                            "UPDATE PROVEEDOR SET id_estado = @idEstado WHERE id_proveedor = @id",
                            ObjConexion.SqlC);
                        cmd.Parameters.AddWithValue("@idEstado", idEstado);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cambiar estados: " + ex.Message);
                return false;
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }

        // ─── CAMBIAR ESTADO MASIVO ACTIVO ─────────────────────────────
        /// <summary>
        /// Cambia a activo los proveedores seleccionados en el DataGridView.
        /// Solicita confirmación al usuario antes de aplicar el cambio.
        /// </summary>
        /// <param name="dgv">DataGridView con los proveedores y la columna "Seleccionar".</param>
        public void CambiarEstadoMasivo_Activo(DataGridView dgv)
        {
            List<int> ids = ObtenerIdsSeleccionados(dgv);
            if (ids.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un proveedor.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"¿Desea activar los {ids.Count} proveedor(es) seleccionado(s)?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
                CambiarEstadoMasivo(ids, true);
        }

        // ─── CAMBIAR ESTADO MASIVO INACTIVO ───────────────────────────
        /// <summary>
        /// Cambia a inactivo los proveedores seleccionados en el DataGridView.
        /// Solicita confirmación al usuario antes de aplicar el cambio.
        /// </summary>
        /// <param name="dgv">DataGridView con los proveedores y la columna "Seleccionar".</param>
        public void CambiarEstadoMasivo_Inactivo(DataGridView dgv)
        {
            List<int> ids = ObtenerIdsSeleccionados(dgv);
            if (ids.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un proveedor.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show(
                $"¿Desea desactivar los {ids.Count} proveedor(es) seleccionado(s)?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
                CambiarEstadoMasivo(ids, false);
        }

        // ─── INSTRUCCIONES DGV ────────────────────────────────────────
        /// <summary>
        /// Texto de instrucciones para mostrar en la interfaz donde se presenta el DataGridView.
        /// </summary>
        /// <returns>Cadena con instrucciones para el usuario.</returns>
        public static string InstruccionesDGV()
        {
            return "💡 Doble clic en una fila para ver el perfil del proveedor" + Environment.NewLine +
                   "💡 Seleccione la casilla del proveedor(s) que desea cambiar estado";
        }
    }

}
