using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TRAMADE.ClasesMenu
{
    /// <summary>
    /// Clase de acceso a datos especializada en la gestión de biometría facial,
    /// entrenamiento de modelos y control de sesiones de usuario.
    /// </summary>
    public class clsDBCon
    {
        clsConexion cn = new clsConexion();

        /// <summary>Arreglo que almacena los nombres de los usuarios cargados para entrenamiento.</summary>
        public string[] nombre;

        /// <summary>Arreglo que almacena los correos electrónicos de los usuarios cargados.</summary>
        public string[] correo;

        /// <summary>Almacenamiento temporal del rostro procesado en formato de bytes.</summary>
        public byte[] rostro;

        /// <summary>Lista de colecciones de bytes que representan los rostros de los usuarios activos.</summary>
        public List<byte[]> rostros = new List<byte[]>();

        /// <summary>Contador total de registros de imágenes faciales obtenidos.</summary>
        public int totalUsuarios;

        // ── VERIFICAR SI USUARIO YA TIENE ROSTRO REGISTRADO ───────
        /// <summary>
        /// Verifica si un usuario distinto al indicado ya tiene imágenes faciales registradas.
        /// Sirve para evitar que un mismo rostro se registre en múltiples usuarios.
        /// </summary>
        /// <param name="idUsuario">ID del usuario que se está registrando actualmente.</param>
        /// <returns>Nombre del usuario que ya tiene imágenes, o null si no hay conflicto.</returns>
        public string ObtenerUsuarioConRostroRegistrado(int idUsuario)
        {
            try
            {
                cn.Abrir();
                string sql = @"SELECT TOP 1 u.nombre_usuario 
                       FROM IMAGEN_FACIAL i
                       INNER JOIN USUARIO u ON i.id_usuario = u.id_usuario
                       WHERE i.id_usuario <> @id_usuario
                         AND u.id_estado = 1";

                SqlCommand cmd = new SqlCommand(sql, cn.SqlC);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);

                object resultado = cmd.ExecuteScalar();
                return resultado != null ? resultado.ToString() : null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar rostro: " + ex.Message);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // ── VERIFICAR SI UN USUARIO ESPECÍFICO YA TIENE ROSTRO ────
        /// <summary>
        /// Verifica si el usuario indicado ya tiene al menos una imagen facial registrada.
        /// </summary>
        /// <param name="idUsuario">ID del usuario a verificar.</param>
        /// <returns><c>true</c> si ya tiene imágenes registradas.</returns>
        public bool UsuarioTieneRostroRegistrado(int idUsuario)
        {
            try
            {
                cn.Abrir();
                string sql = @"SELECT COUNT(*) FROM IMAGEN_FACIAL 
                       WHERE id_usuario = @id_usuario";
                SqlCommand cmd = new SqlCommand(sql, cn.SqlC);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);

                object resultado = cmd.ExecuteScalar();

                // ✅ Convert.ToInt32 en vez de cast directo (evita InvalidCastException)
                if (resultado == null || resultado == DBNull.Value) return false;
                return Convert.ToInt32(resultado) > 0;
            }
            catch
            {
                // ✅ Si falla la consulta, NO bloquear el registro
                return false;
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // ── OBTENER IMÁGENES DE OTROS USUARIOS PARA COMPARAR ──────
        /// <summary>
        /// Retorna las imágenes faciales de todos los usuarios EXCEPTO el indicado,
        /// para comparar biométricamente si el rostro capturado ya pertenece a otro.
        /// </summary>
        /// <param name="idUsuario">ID del usuario que se está registrando (se excluye).</param>
        /// <returns>Lista de tuplas (nombreUsuario, imagenBytes).</returns>
        public List<(string nombre, byte[] imagen)> ObtenerImagenesOtrosUsuarios(int idUsuario)
        {
            var lista = new List<(string, byte[])>();
            try
            {
                cn.Abrir();
                string sql = @"SELECT u.nombre_usuario, i.imagen_facial
                       FROM IMAGEN_FACIAL i
                       INNER JOIN USUARIO u ON i.id_usuario = u.id_usuario
                       WHERE i.id_usuario <> @id_usuario
                         AND u.id_estado = 1";

                SqlCommand cmd = new SqlCommand(sql, cn.SqlC);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string nombre = reader["nombre_usuario"].ToString();
                    byte[] imagen = (byte[])reader["imagen_facial"];
                    lista.Add((nombre, imagen));
                }
                reader.Close();
                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener imágenes: " + ex.Message);
            }
            finally
            {
                cn.Cerrar();
            }
        }


        // ── GUARDAR IMAGEN (reemplaza) ─────────────────────────────
        /// <summary>
        /// Guarda o reemplaza la imagen facial de un usuario utilizando el procedimiento almacenado PA_GUARDAR_IMAGEN_FACIAL.
        /// </summary>
        /// <param name="idUsuario">ID del usuario al que pertenece la imagen.</param>
        /// <param name="imagen">Arreglo de bytes de la imagen facial.</param>
        /// <returns><c>true</c> si la operación fue exitosa; de lo contrario, <c>false</c>.</returns>
        public bool guardarImagen(int idUsuario, byte[] imagen)
        {
            try
            {
                cn.Abrir();
                SqlCommand comando = new SqlCommand("PA_GUARDAR_IMAGEN_FACIAL", cn.SqlC);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@id_usuario", idUsuario);
                comando.Parameters.Add("@imagen_facial", SqlDbType.VarBinary, -1).Value = imagen;
                int resultado = comando.ExecuteNonQuery();
                return resultado > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar imagen: " + ex.Message);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // ── OBTENER IMÁGENES PARA ENTRENAMIENTO ────────────────────
        /// <summary>
        /// Recupera todas las imágenes faciales de usuarios con estado activo para alimentar el algoritmo de entrenamiento.
        /// Actualiza las propiedades <see cref="nombre"/>, <see cref="correo"/> y la lista <see cref="rostros"/>.
        /// </summary>
        /// <returns>Un <see cref="DataTable"/> con la información detallada de los usuarios y sus imágenes.</returns>
        public DataTable ObtenerBytesImagen()
        {
            try
            {
                cn.Abrir();
                // ✅ ORDER BY para que imágenes del mismo usuario queden juntas
                string consulta = @"SELECT u.id_usuario, u.nombre_usuario,u.correo_usuario, i.imagen_facial 
                                    FROM IMAGEN_FACIAL i
                                    INNER JOIN USUARIO u ON i.id_usuario = u.id_usuario
                                    WHERE u.id_estado = 1
                                    ORDER BY u.id_usuario";

                SqlDataAdapter adapter = new SqlDataAdapter(consulta, cn.SqlC);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                int count = dt.Rows.Count;
                nombre = new string[count];
                correo = new string[count];
                rostros.Clear();

                for (int i = 0; i < count; i++)
                {
                    nombre[i] = dt.Rows[i]["nombre_usuario"].ToString();
                    correo[i] = dt.Rows[i]["correo_usuario"].ToString();
                    rostro = (byte[])dt.Rows[i]["imagen_facial"];
                    rostros.Add(rostro);
                }

                totalUsuarios = count;
                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener imágenes: " + ex.Message);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // ── CONVERTIR IMAGEN A BINARY ──────────────────────────────
        /// <summary>
        /// Convierte un objeto <see cref="Image"/> a un arreglo de bytes (BMP) y lo guarda en la base de datos.
        /// </summary>
        /// <param name="idUsuario">ID del usuario asociado.</param>
        /// <param name="img">Objeto de imagen a convertir.</param>
        public void convertirImgABinary(int idUsuario, Image img)
        {
            using (Bitmap bmp = new Bitmap(img))
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                guardarImagen(idUsuario, ms.ToArray());
            }
        }

        // ── CONVERTIR BYTES A IMAGE ────────────────────────────────
        /// <summary>
        /// Convierte un arreglo de bytes almacenado en la lista <see cref="rostros"/> de vuelta a un objeto <see cref="Image"/>.
        /// </summary>
        /// <param name="con">Índice del rostro en la lista.</param>
        /// <returns>El objeto <see cref="Image"/> reconstruido.</returns>
        public Image convertirByteAImg(int con)
        {
            MemoryStream ms = new MemoryStream(rostros[con]);
            return Image.FromStream(ms);
        }

        // ── REGISTRAR INICIO DE SESIÓN ─────────────────────────────
        /// <summary>
        /// Registra una nueva entrada en la tabla INICIO_DE_SESION con la fecha y hora actual del servidor.
        /// </summary>
        /// <param name="idUsuario">ID del usuario que inició sesión.</param>
        public void registrarInicioSesion(int idUsuario)
        {
            try
            {
                cn.Abrir();
                string sql = @"INSERT INTO INICIO_DE_SESION (id_usuario, fecha_inicio_de_sesion)
                               VALUES (@id, GETDATE())";
                SqlCommand comm = new SqlCommand(sql, cn.SqlC);
                comm.Parameters.AddWithValue("@id", idUsuario);
                comm.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar sesión: " + ex.Message);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // ── LLENAR COMBO USUARIOS ──────────────────────────────────
        /// <summary>
        /// Llena de forma asíncrona un <see cref="Krypton.Toolkit.KryptonComboBox"/> con los usuarios activos,
        /// permitiendo filtrado dinámico por nombre.
        /// </summary>
        /// <param name="cmb">Control ComboBox a llenar.</param>
        /// <param name="conexion">Instancia de la conexión a utilizar.</param>
        /// <param name="texto">Texto de búsqueda para filtrar usuarios (opcional).</param>
        internal static void llenarComboUsuarios(Krypton.Toolkit.KryptonComboBox cmb,
            clsConexion conexion, string texto = "", Action onFinish = null)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd;
                if (string.IsNullOrWhiteSpace(texto))
                    cmd = new SqlCommand("SELECT id_usuario, nombre_usuario FROM USUARIO WHERE id_estado = 1", conexion.SqlC);
                else
                {
                    cmd = new SqlCommand("SELECT id_usuario, nombre_usuario FROM USUARIO WHERE id_estado = 1 AND nombre_usuario LIKE @texto", conexion.SqlC);
                    cmd.Parameters.AddWithValue("@texto", "%" + texto.Trim() + "%");
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                string textoGuardado = texto;

                cmb.BeginInvoke(new Action(() =>
                {
                    cmb.DataSource = null;
                    cmb.Items.Clear();
                    if (dt.Rows.Count > 0)
                    {
                        cmb.DataSource = dt;
                        cmb.DisplayMember = "nombre_usuario";
                        cmb.ValueMember = "id_usuario";
                        cmb.SelectedIndex = -1;
                    }
                    cmb.Text = textoGuardado;
                    cmb.SelectionStart = textoGuardado.Length;
                    cmb.SelectionLength = 0;

                    onFinish?.Invoke(); // ← al final del BeginInvoke
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar usuarios: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }

        }

        // ── INSERTAR IMAGEN FACIAL (agrega, no reemplaza) ──────────
        /// <summary>
        /// Inserta una nueva imagen facial para un usuario sin eliminar las existentes,
        /// permitiendo tener múltiples muestras para mejorar la precisión del reconocimiento.
        /// </summary>
        /// <param name="idUsuario">ID del usuario.</param>
        /// <param name="imagen">Arreglo de bytes de la nueva imagen facial.</param>
        /// <returns><c>true</c> si la inserción fue correcta.</returns>
        public bool insertarImagenFacial(int idUsuario, byte[] imagen)
        {
            try
            {
                cn.Abrir();
                SqlCommand comando = new SqlCommand("PA_INSERTAR_IMAGEN_FACIAL", cn.SqlC);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@id_usuario", idUsuario);
                comando.Parameters.Add("@imagen_facial", SqlDbType.VarBinary, -1).Value = imagen;
                int resultado = comando.ExecuteNonQuery();
                return resultado > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar imagen: " + ex.Message);
            }
            finally
            {
                cn.Cerrar();
            }
        }

        // ── ELIMINAR IMÁGENES DE UN USUARIO ───────────────────────
        /// <summary>
        /// Elimina todos los registros de imágenes faciales asociados a un usuario específico.
        /// </summary>
        /// <param name="idUsuario">ID del usuario.</param>
        /// <returns><c>true</c> si el proceso se completó sin errores.</returns>
        public bool eliminarImagenesUsuario(int idUsuario)
        {
            try
            {
                cn.Abrir();
                SqlCommand comando = new SqlCommand("PA_ELIMINAR_IMAGENES_USUARIO", cn.SqlC);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Parameters.AddWithValue("@id_usuario", idUsuario);
                comando.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar imágenes: " + ex.Message);
            }
            finally
            {
                cn.Cerrar();
            }
        }
    }
}