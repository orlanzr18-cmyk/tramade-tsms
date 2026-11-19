using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace TRAMADE.ClasesAdministrador
{
    /// <summary>
    /// Representa las operaciones y propiedades relacionadas con un usuario del sistema.
    /// Proporciona métodos para insertar, actualizar, cambiar estado y obtener listados/consultas
    /// de usuarios desde la base de datos, así como helpers para llenar combos de interfaz.
    /// </summary>
    internal class clsUsuario
    {
        // Propiedades de usuario (campos internos)
        /// <summary>Nombre completo o nombre de usuario.</summary>
        private string nombre, correo, contrasena;
        /// <summary>Identificador del rol asignado.</summary>
        private int rol, sucursal, estado = 1;


        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public clsUsuario()
        {

        }

        // Setters
        /// <summary>
        /// Establece el nombre del usuario.
        /// </summary>
        /// <param name="valor">Nombre o identificador del usuario.</param>
        public void setNombre(string valor)
        {
            nombre = valor;
        }

        /// <summary>
        /// Establece el identificador del rol del usuario.
        /// </summary>
        /// <param name="valor">Id del rol.</param>
        public void setRol(int valor)
        {
            rol = valor;
        }

        /// <summary>
        /// Establece la sucursal asociada al usuario.
        /// </summary>
        /// <param name="valor">Id de la sucursal.</param>
        public void setSucursal(int valor)
        {
            sucursal = valor;
        }

        /// <summary>
        /// Establece el correo electrónico del usuario.
        /// </summary>
        /// <param name="valor">Correo electrónico.</param>
        public void setCorreo(string valor)
        {
            correo = valor;
        }

        /// <summary>
        /// Establece la contraseña del usuario.
        /// </summary>
        /// <param name="valor">Contraseña en texto plano (se asume que será manejada con seguridad en la BD).</param>
        public void setContrasena(string valor)
        {
            contrasena = valor;
        }
        /// <summary>
        /// Llena el <see cref="Krypton.Toolkit.KryptonComboBox"/> con los roles disponibles
        /// obtenidos desde la vista <c>VistaRol</c> en la base de datos.
        /// </summary>
        /// <param name="cmb">Combo a llenar.</param>
        /// <param name="conexion">Conexión a la base de datos.</param>
        public static void llenarComboRol(Krypton.Toolkit.KryptonComboBox cmb, clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT id_rol, descripcion_rol FROM VistaRol";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion.SqlC); //mensajero que lleva la consulta a la base de datos y trae los resultados de vuelta.
                DataTable dt = new DataTable(); //Crea una tabla vacía en la memoria(como una hoja en blanco) y el mensajero(adapter) la llena(con fill) con los datos que trajo de la base de datos.
                adapter.Fill(dt);

                cmb.DataSource = dt; //Conecta el ComboBox con la tabla que acabamos de llenar. Ahora el ComboBox ya tiene los datos.
                cmb.DisplayMember = "descripcion_rol"; //Muestra al usuario el texto de la columna
                cmb.ValueMember = "id_rol"; //Guarda el ID como el valor real
                cmb.SelectedIndex = -1; // Para que no seleccione nada por defecto
            }
            catch (Exception ex) //Por si sale un error , lo atrapa y muetra un mensaje en lugar de que el programa se cierre de una
            {
                MessageBox.Show("Error al cargar ComboBox: " + ex.Message);
            }
            finally // Cierra la puerta de la base de datos siempre, pase lo que pase(haya error o no), para no desperdiciar memoria.
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Llena el <see cref="Krypton.Toolkit.KryptonComboBox"/> con las sucursales disponibles
        /// obtenidas desde la vista <c>VistaSucursal</c> en la base de datos.
        /// </summary>
        /// <param name="cmb">Combo a llenar.</param>
        /// <param name="conexion">Conexión a la base de datos.</param>
        public static void llenarComboSucrusal(Krypton.Toolkit.KryptonComboBox cmb, clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT id_sucursal, nombre_sucursal FROM VistaSucursal";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmb.DataSource = dt;
                cmb.DisplayMember = "nombre_sucursal";
                cmb.ValueMember = "id_sucursal";
                cmb.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar ComboBox: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Inserta un nuevo usuario en la base de datos usando el procedimiento almacenado <c>PA_INSERTAR_USUARIO</c>.
        /// Usa las propiedades internas previamente establecidas mediante los setters.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>True si la operación afectó al menos una fila; false en caso de error.</returns>
        public bool insertarUsuarios(clsConexion conexion)
        {

            try
            {
                conexion.Abrir();
                SqlCommand comando = new SqlCommand("PA_INSERTAR_USUARIO", conexion.SqlC);
                comando.CommandType = CommandType.StoredProcedure; //Avisamos que es un Procedimiento Almacenado y no texto común
                comando.Parameters.AddWithValue("@id_rol", rol);
                comando.Parameters.AddWithValue("@id_estado", estado);
                comando.Parameters.AddWithValue("@id_sucursal", sucursal);
                comando.Parameters.AddWithValue("@nombre_usuario", nombre);
                comando.Parameters.AddWithValue("@correo_usuario", correo);
                comando.Parameters.AddWithValue("@password_usuario", clsSeguridad.HashPassword(contrasena));

                int filasAfectadas = comando.ExecuteNonQuery(); //Ejecutamos la orden

                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al ejecutar procedimiento: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Actualiza los datos de un usuario existente. Si <paramref name="contrasena"/> viene vacía,
        /// se preserva la contraseña actual recuperándola de la base de datos.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idUsuario">Id del usuario a actualizar.</param>
        /// <param name="nombre">Nuevo nombre para el usuario.</param>
        /// <param name="correo">Nuevo correo para el usuario.</param>
        /// <param name="contrasena">Contraseña nueva (si está vacía se mantiene la actual).</param>
        /// <returns>True si la actualización fue exitosa; false en caso de error.</returns>
        public bool actualizarUsuario(clsConexion conexion, int idUsuario, string nombre, string correo, string contrasena)
        {
            try
            {
                conexion.Abrir();

                string contrasenaFinal = contrasena;

                if (string.IsNullOrWhiteSpace(contrasena))
                {
                    SqlCommand cmdObtener = new SqlCommand(
                        "SELECT password_usuario FROM USUARIO WHERE id_usuario = @id",
                        conexion.SqlC);
                    cmdObtener.Parameters.AddWithValue("@id", idUsuario);

                    object resultado = cmdObtener.ExecuteScalar();

                    if (resultado == null || resultado == DBNull.Value)
                    {
                        MessageBox.Show("No se pudo recuperar la contraseña actual.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    contrasenaFinal = resultado.ToString(); 
                }
                else
                {
                    contrasenaFinal = clsSeguridad.HashPassword(contrasena); 
                }

                // Ejecutar el SP con la contraseña correcta (nueva o la que ya tenía)
                SqlCommand comando = new SqlCommand("PA_ACTUALIZAR_USUARIO", conexion.SqlC);
                comando.CommandType = CommandType.StoredProcedure;

                comando.Parameters.AddWithValue("@id_usuario", idUsuario);
                comando.Parameters.AddWithValue("@id_rol", rol);
                comando.Parameters.AddWithValue("@id_estado", estado);
                comando.Parameters.AddWithValue("@id_sucursal", sucursal);
                comando.Parameters.AddWithValue("@nombre_usuario", nombre);
                comando.Parameters.AddWithValue("@correo_usuario", correo);
                comando.Parameters.AddWithValue("@password_usuario", contrasenaFinal);

                int filasAfectadas = comando.ExecuteNonQuery();
                return filasAfectadas > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar usuario: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Cambia el estado (activo/inactivo) de un usuario usando el procedimiento almacenado correspondiente.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idUsuario">Id del usuario a actualizar.</param>
        /// <param name="estado">Nuevo estado (por ejemplo 1 = activo, 2 = inactivo).</param>
        /// <returns>True si la operación fue exitosa; false en caso contrario.</returns>
        public bool cambiarEstado(clsConexion conexion, int idUsuario, int estado)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd = new SqlCommand("PA_CAMBIAR_ESTADO_USUARIO", conexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                cmd.Parameters.AddWithValue("@id_estado", estado);

                int filas = cmd.ExecuteNonQuery();

                return filas > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar el estado del usuario: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }
        // ─── RECARGAR TODOS LOS USUARIOS ─────────────────────────────
        // ─── RECARGAR TODOS LOS USUARIOS ─────────────────────────────
        /// <summary>
        /// Recupera todos los usuarios usando la vista <c>VistaUsuarioTabla</c>.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>DataTable con los usuarios o <c>null</c> en caso de error.</returns>
        public DataTable obtenerUsuariosTabla(clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT * FROM VistaUsuarioTabla";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar usuarios: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        // ─── BUSCAR USUARIOS POR NOMBRE ──────────────────────────────
        // ─── BUSCAR USUARIOS POR NOMBRE ──────────────────────────────
        /// <summary>
        /// Busca usuarios cuyo nombre contenga la cadena indicada y devuelve el resultado en un DataTable.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="nombre">Texto a buscar dentro del nombre (se usa LIKE %nombre%).</param>
        /// <returns>DataTable con los usuarios encontrados o <c>null</c> en caso de error.</returns>
        public DataTable buscarUsuariosPorNombre(clsConexion conexion, string nombre)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT * FROM VistaUsuarioTabla WHERE Nombre LIKE @nombre";
                SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC);
                cmd.Parameters.AddWithValue("@nombre", "%" + nombre.Trim() + "%");

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar usuarios: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }
    }
    //Fin de clase cls usuio
} //Fin de namespace

