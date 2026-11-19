/*using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Krypton.Toolkit;
using System.Linq.Expressions;
using System.Drawing;


namespace TRAMADE.ClasesCliente
{
    /// <summary>
    /// Representa la lógica y operaciones relacionadas con clientes.
    /// Proporciona setters para los campos del cliente, métodos para llenar combos
    /// (departamentos, ciudades, tipos), operaciones de inserción/actualización y
    /// funciones auxiliares para autorización y consultas/estadísticas.
    /// </summary>
    internal class clsCliente
    {
        /// <summary>Identificador de cliente (cadena, puede ser convertido a int cuando aplica).</summary>
        private string id, nombre, razonSocial, tipoCliente, contacto, dni;
        /// <summary>Datos de contacto y ubicación del cliente.</summary>
        private string correo, direccion, ciudad, telefono, departamento;
        /// <summary>Estado del cliente y usuario que realiza la operación.</summary>
        private int estado = 7, idUsuario;
        /// <summary>RTN del cliente (persona jurídica).</summary>
        private string RTN;
        /// <summary>Indicador de autorización (uso interno).</summary>
        private bool autorizar = false;
        /// <summary>Fecha de registro del cliente.</summary>
        private DateTime fechaRegistro;

        /// <summary>Tablas auxiliares para listas de departamento, ciudad y tipo de cliente.</summary>
        private DataTable ListaDepar = new DataTable();
        private DataTable ListaCiudad = new DataTable();
        private DataTable ListaTipoCliente = new DataTable();

        /// <summary>
        /// Constructor por defecto de la clase clsCliente.
        /// </summary>
        public clsCliente()
        {

        }
        /// <summary>
        /// Asigna el identificador del cliente.
        /// </summary>
        /// <param name="valor">Id del cliente como cadena.</param>
        public void setID(String valor)
        {
            id = valor;
        }

        /// <summary>
        /// Asigna el nombre del cliente.
        /// </summary>
        /// <param name="valor">Nombre completo o razón comercial.</param>
        public void setNombre(String valor)
        {
            nombre = valor;
        }

        /// <summary>
        /// Asigna la razón social (para persona jurídica).
        /// </summary>
        /// <param name="valor">Texto con la razón social.</param>
        public void setRazonSocial(String valor)
        {
            razonSocial = valor;
        }

        /// <summary>
        /// Establece el tipo de cliente (por ejemplo "PERSONA NATURAL" o "PERSONA JURÍDICA").
        /// </summary>
        /// <param name="valor">Tipo de cliente.</param>
        public void setTipoCliente(String valor)
        {
            tipoCliente = valor;
        }

        /// <summary>
        /// Define el contacto del cliente.
        /// </summary>
        /// <param name="valor">Nombre del contacto o persona de referencia.</param>
        public void setContacto(String valor)
        {
            contacto = valor;
        }

        /// <summary>
        /// Establece el teléfono del cliente.
        /// </summary>
        /// <param name="valor">Número de teléfono (cadena).</param>
        public void setTelefono(String valor)
        {
            telefono = valor;
        }

        /// <summary>
        /// Define el departamento del cliente (id o valor según uso interno).
        /// </summary>
        /// <param name="valor">Departamento asociado.</param>
        public void setDepartamento(String valor)
        {
            departamento = valor;
        }

        /// <summary>
        /// Asigna el correo electrónico del cliente.
        /// </summary>
        /// <param name="valor">Dirección de correo.</param>
        public void setCorreo(String valor)
        {
            correo = valor;
        }

        /// <summary>
        /// Asigna la dirección física del cliente.
        /// </summary>
        /// <param name="valor">Dirección completa.</param>
        public void setDireccion(String valor)
        {
            direccion = valor;
        }

        /// <summary>
        /// Establece la ciudad asociada al cliente.
        /// </summary>
        /// <param name="valor">Id o nombre de la ciudad.</param>
        public void setCiudad(String valor)
        {
            ciudad = valor;
        }

        /// <summary>
        /// Establece la fecha de registro del cliente.
        /// </summary>
        /// <param name="valor">Fecha de registro.</param>
        public void setFechaRegistro(DateTime valor)
        {
            fechaRegistro = valor;
        }

        /// <summary>
        /// Asigna el RTN del cliente (para persona jurídica).
        /// </summary>
        /// <param name="valor">Texto con el RTN.</param>
        public void setRTN(string valor)
        {
            RTN = valor;
        }

        /// <summary>
        /// Establece el identificador del usuario que realiza la operación.
        /// </summary>
        /// <param name="valor">Id del usuario (entero).</param>
        public void setIdUsuario(int valor)
        {
            idUsuario = valor;
        }

        /// <summary>
        /// Asigna el DNI del cliente (para persona natural).
        /// </summary>
        /// <param name="valor">Cadena con el DNI (13 dígitos).</param>
        public void setDNI(string valor)
        {
            dni = valor;
        }


        /// <summary>
        /// Carga la lista de departamentos en el combo indicado desde la tabla DEPARTAMENTO.
        /// </summary>
        /// <param name="cmbDepartamento">Combo a llenar.</param>
        /// <param name="conexion">Conexión a la base de datos.</param>
        public static void llenarcomboDepartamento(Krypton.Toolkit.KryptonComboBox cmbDepartamento, clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT id_departamento, nombre_departamento FROM DEPARTAMENTO";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmbDepartamento.DataSource = dt;
                cmbDepartamento.DisplayMember = "nombre_departamento";
                cmbDepartamento.ValueMember = "id_departamento";

                cmbDepartamento.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar departamentos: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }


        /// <summary>
        /// Llena el combo de ciudades según el departamento seleccionado.
        /// </summary>
        /// <param name="cmbCiudad">Combo de ciudades a llenar.</param>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idDepartamento">Id del departamento para filtrar ciudades.</param>
        public static void llenarcombociudad(KryptonComboBox cmbCiudad, clsConexion conexion, int idDepartamento)
        {
            try
            {
                conexion.Abrir();

                string consulta = "SELECT id_ciudad, nombre_ciudad FROM CIUDAD WHERE id_departamento = @idDep";
                SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC);
                cmd.Parameters.AddWithValue("@idDep", idDepartamento);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                cmbCiudad.DataSource = dt;
                cmbCiudad.DisplayMember = "nombre_ciudad";
                cmbCiudad.ValueMember = "id_ciudad";

                cmbCiudad.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar ciudades: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Carga en el combo los diferentes tipos o clasificaciones de cliente disponibles.
        /// </summary>
        /// <param name="cmbTipoCliente">Combo que será llenado.</param>
        /// <param name="conexion">Conexión a la base de datos.</param>
        public static void llenarcomboTipoCliente(KryptonComboBox cmbTipoCliente, clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT id_clasificacion_cliente, descripcion_clasificacion_cliente FROM CLASIFICACION_CLIENTE";
                SqlDataAdapter adapter = new SqlDataAdapter(consulta, conexion.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                cmbTipoCliente.DataSource = dt;
                cmbTipoCliente.DisplayMember = "descripcion_clasificacion_cliente";
                cmbTipoCliente.ValueMember = "id_clasificacion_cliente";
                cmbTipoCliente.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar tipos de cliente: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }


        /// <summary>
        /// Inserta el cliente en la base de datos usando el procedimiento almacenado <c>PA_INSERTAR_CLIENTE</c>.
        /// Utiliza los valores establecidos mediante los setters de la instancia.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>True si la inserción fue exitosa; false en caso de error.</returns>
        public bool InsertarCliente(clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmdCliente = new SqlCommand("PA_INSERTAR_CLIENTE", conexion.SqlC);
                cmdCliente.CommandType = CommandType.StoredProcedure;

                cmdCliente.Parameters.AddWithValue("@nombre_cliente", nombre);

                cmdCliente.Parameters.AddWithValue("@rtn_cliente", string.IsNullOrWhiteSpace(RTN) ? (object)DBNull.Value : RTN);
                cmdCliente.Parameters.AddWithValue("@razon_social", string.IsNullOrWhiteSpace(razonSocial) ? (object)DBNull.Value : razonSocial);
                cmdCliente.Parameters.AddWithValue("@dni_cliente", string.IsNullOrWhiteSpace(dni) ? (object)DBNull.Value : dni);


                cmdCliente.Parameters.AddWithValue("@telefono_cliente", telefono);
                cmdCliente.Parameters.AddWithValue("@contacto_cliente", contacto);
                cmdCliente.Parameters.AddWithValue("@correo_electronico_cliente", correo);
                cmdCliente.Parameters.AddWithValue("@direccion_cliente", direccion);
                cmdCliente.Parameters.AddWithValue("@id_estado", Convert.ToInt32(estado));
                cmdCliente.Parameters.AddWithValue("@id_clasificacion_cliente", Convert.ToInt32(tipoCliente));
                cmdCliente.Parameters.AddWithValue("@id_ciudad", Convert.ToInt32(ciudad));

                cmdCliente.Parameters.AddWithValue("@id_usuario", idUsuario);
                cmdCliente.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la base de datos: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }
        /// <summary>
        /// Actualiza la información del cliente en la base de datos usando el procedimiento <c>PA_ACTUALIZAR_CLIENTE</c>.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>True si la actualización fue exitosa; false en caso de error.</returns>
        public bool ActulizarCliente(clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmdCliente = new SqlCommand("PA_ACTUALIZAR_CLIENTE", conexion.SqlC);
                cmdCliente.CommandType = CommandType.StoredProcedure;

                cmdCliente.Parameters.AddWithValue("@id_cliente", Convert.ToInt32(id));
                cmdCliente.Parameters.AddWithValue("@nombre_cliente", nombre);

                cmdCliente.Parameters.AddWithValue("@rtn_cliente", string.IsNullOrWhiteSpace(RTN) ? (object)DBNull.Value : RTN);
                cmdCliente.Parameters.AddWithValue("@razon_social", string.IsNullOrWhiteSpace(razonSocial) ? (object)DBNull.Value : razonSocial);
                cmdCliente.Parameters.AddWithValue("@dni_cliente", string.IsNullOrWhiteSpace(dni) ? (object)DBNull.Value : dni);


                cmdCliente.Parameters.AddWithValue("@telefono_cliente", telefono);
                cmdCliente.Parameters.AddWithValue("@contacto_cliente", contacto);
                cmdCliente.Parameters.AddWithValue("@correo_electronico_cliente", correo);
                cmdCliente.Parameters.AddWithValue("@direccion_cliente", direccion);
                cmdCliente.Parameters.AddWithValue("@id_clasificacion_cliente", Convert.ToInt32(tipoCliente));
                cmdCliente.Parameters.AddWithValue("@id_ciudad", Convert.ToInt32(ciudad));


                cmdCliente.Parameters.AddWithValue("@id_usuario", idUsuario);

                cmdCliente.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar en la base de datos: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Autoriza (aprueba) un cliente mediante el procedimiento almacenado correspondiente.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idCliente">Id del cliente a autorizar.</param>
        /// <returns>True si la operación fue exitosa; false en caso de error.</returns>
        public bool AutorizarCliente(clsConexion conexion, int idCliente)
        {
            try
            {
                conexion.Abrir();
                using (SqlCommand cmd = new SqlCommand("PA_AUTORIZAR_CLIENTE", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al autorizar cliente: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Deniega (rechaza) un cliente mediante el procedimiento almacenado correspondiente.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idCliente">Id del cliente a denegar.</param>
        /// <returns>True si la operación fue exitosa; false en caso de error.</returns>
        public bool DenegarCliente(clsConexion conexion, int idCliente)
        {
            try
            {
                conexion.Abrir();
                using (SqlCommand cmd = new SqlCommand("PA_DENEGAR_CLIENTE", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_cliente", idCliente);
                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al denegar cliente: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Busca clientes pendientes de aprobación cuyo nombre contenga el texto indicado.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="textoBuscar">Texto a buscar en el nombre del cliente.</param>
        /// <returns>DataTable con los resultados de la búsqueda o <c>null</c> en caso de error.</returns>
        public DataTable BuscarCliente(clsConexion conexion, string textoBuscar)
        {
            try
            {
                conexion.Abrir();

                string consulta = "select * from vista_aprobacion_clientes where [Nombre Cliente] like @nombre";

                using (SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC))
                {
                    cmd.Parameters.AddWithValue("@nombre", "%" + textoBuscar.Trim() + "%");
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar cliente: " + ex.Message);
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Recupera estadísticas generales de clientes ejecutando el procedimiento <c>PA_OBTENER_CONTADORES</c>.
        /// Retorna un objeto dinámico con propiedades Total, Activos e Inactivos (como cadenas).
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>Objeto dinámico con contadores o <c>null</c> si no se obtienen valores.</returns>
        public dynamic ObtenerEstadisticas(clsConexion conexion)
        {
            try
            {
                conexion.Abrir();

                string consulta = "PA_OBTENER_CONTADORES ";

                SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new
                    {
                        Total = reader["Total"].ToString(),
                        Activos = reader["Activos"].ToString(),
                        Inactivos = reader["Inactivos"].ToString()
                    };
                }
                return null;
            }
            finally 
            { 
                conexion.Cerrar(); 
            }
        }

        /// <summary>
        /// Obtiene un DataTable con el porcentaje de clientes por estado ejecutando el SP correspondiente.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>DataTable con los porcentajes por estado.</returns>
        public DataTable ObtenerPorcentajeEstados(clsConexion conexion)
        {
            DataTable dt = new DataTable();
            string sql = " PA_PorcentajeEstados ";
            try
            {
                conexion.Abrir();
                SqlDataAdapter da = new SqlDataAdapter(sql, conexion.SqlC);
                da.Fill(dt);
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        /// <summary>
        /// Recupera un DataTable con los departamentos con mayores índices (Top Departamentos).
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>DataTable con resultados del procedimiento <c>PA_TopDepartamentos</c>.</returns>
        public DataTable ObtenerTopDepartamentos(clsConexion conexion)
        {
            DataTable dt = new DataTable();
            string sql = "PA_TopDepartamentos";
            try
            {
                conexion.Abrir();
                SqlDataAdapter da = new SqlDataAdapter(sql, conexion.SqlC);
                da.Fill(dt);
            }
            finally { conexion.Cerrar(); }
            return dt;
        }

        /// <summary>
        /// Configura visualmente los controles del formulario según el tipo de cliente:
        /// - Si es PERSONA JURÍDICA habilita RTN y razón social, deshabilita DNI.
        /// - Si es PERSONA NATURAL habilita DNI y deshabilita RTN/razón social.
        /// </summary>
        /// <param name="tipoCliente">Tipo de cliente esperado (ej: "PERSONA JURÍDICA").</param>
        /// <param name="txtRTN">Control que contiene el RTN.</param>
        /// <param name="cmbRazonSocial">Control del combo de razón social.</param>
        /// <param name="txtDNI">Control que contiene el DNI.</param>
        public  void ConfigurarTipo(string tipoCliente,Control txtRTN,Control cmbRazonSocial,Control txtDNI)
        {
            if (tipoCliente == "PERSONA JURÍDICA")
            {

                txtRTN.Enabled = true;
                txtRTN.BackColor = Color.White;

                cmbRazonSocial.Enabled = true;
                cmbRazonSocial.BackColor = Color.White;

                txtDNI.Enabled = false;
                txtDNI.Text = "";
                txtDNI.BackColor = Color.LightGray;
            }
            if (tipoCliente == "PERSONA NATURAL")
            {

                txtRTN.Enabled = false;
                txtRTN.Text = "";
                txtRTN.BackColor = Color.LightGray;

                cmbRazonSocial.Enabled = false;
                if (cmbRazonSocial is ComboBox cb) 
                {
                    cb.SelectedIndex = -1;
                }
                cmbRazonSocial.BackColor = Color.LightGray;

                txtDNI.Enabled = true;
                txtDNI.BackColor = Color.White;
            }

        }
        /// <summary>
        /// Busca un cliente por RTN o DNI y devuelve sus datos junto con el id de departamento.
        /// </summary>
        /// <param name="busqueda">RTN o DNI a buscar.</param>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>DataTable con la información del cliente o lanza excepción en caso de error.</returns>
        public  DataTable BuscarCliente(string busqueda, clsConexion conexion)
        {
            try
            {
                conexion.Abrir();

                string consulta = @"SELECT C.*, CI.id_departamento 
                            FROM CLIENTE C 
                            INNER JOIN CIUDAD CI ON C.id_ciudad = CI.id_ciudad 
                            WHERE C.rtn_cliente = @busqueda OR C.dni_cliente = @busqueda";

                SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC);
                cmd.Parameters.AddWithValue("@busqueda", busqueda);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("Error en la consulta de búsqueda: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }

    }
}*/
