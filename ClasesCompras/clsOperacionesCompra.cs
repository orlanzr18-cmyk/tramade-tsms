/*using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.ClasesCompras
{
    internal class clsOperacionesCompra:clsLlenarListBox
    {
        /// <summary>
        /// Clase que agrupa las operaciones de negocio relacionadas con compras:
        /// inserción, actualización, autorización/denegación y manipulación de detalles.
        /// Hereda de <see cref="clsLlenarListBox"/> para gestionar la lista interna de productos.
        /// </summary>
        public clsOperacionesCompra() { }

        /// <summary>
        /// Inserta una nueva compra (cabecera) y sus detalles en la base de datos.
        /// Ejecuta el procedimiento <c>PA_INSERTAR_COMPRA</c> y, si se genera un id,
        /// inserta los detalles asociados y la relación producto-proveedor cuando aplica.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>True si la inserción fue exitosa; false en caso de error.</returns>
        public bool insertarCompras(clsConexion conexion)
        {
            try
            {
                conexion.Abrir();

                SqlCommand cmdCompra = new SqlCommand("PA_INSERTAR_COMPRA", conexion.SqlC);
                cmdCompra.CommandType = CommandType.StoredProcedure;
                cmdCompra.Parameters.AddWithValue("@id_proveedor", getProveedor());
                cmdCompra.Parameters.AddWithValue("@id_forma_pago", getFormaPago());
                cmdCompra.Parameters.AddWithValue("@id_estado", getEstado());
                cmdCompra.Parameters.AddWithValue("@id_usuario", getIdUsuario());
                cmdCompra.Parameters.AddWithValue("@fecha_entrega",
                    getEntrega() == DateTime.MinValue ? (object)DBNull.Value : getEntrega());

                object resultado = cmdCompra.ExecuteScalar();
                if (resultado == null) return false;

                int idGenerado = Convert.ToInt32(resultado);
                InsertarDetallesYProveedor(conexion, idGenerado);

                limpiarLista(); // heredado de clsLlenarListBox
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error crítico al insertar: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        // ACTUALIZAR
        /// <summary>
        /// Actualiza la cabecera de una compra y reemplaza sus detalles.
        /// Ejecuta <c>PA_ACTUALIZAR_COMPRA</c>, elimina los detalles antiguos y vuelve a insertar los actuales.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <returns>True si la actualización fue exitosa; false en caso de error.</returns>
        public bool actualizarCompra(clsConexion conexion)
        {
            try
            {

                    conexion.Abrir();

                    SqlCommand cmdCompra = new SqlCommand("PA_ACTUALIZAR_COMPRA", conexion.SqlC);
                    cmdCompra.CommandType = CommandType.StoredProcedure;
                    cmdCompra.Parameters.AddWithValue("@id_compra", getIdCompra());
                    cmdCompra.Parameters.AddWithValue("@id_proveedor", getProveedor());
                    cmdCompra.Parameters.AddWithValue("@id_forma_pago", getFormaPago());
                    cmdCompra.Parameters.AddWithValue("@id_usuario", getIdUsuario());
                    cmdCompra.Parameters.AddWithValue("@fecha_entrega",
                        getEntrega() == DateTime.MinValue ? (object)DBNull.Value : getEntrega());

                    object resultado = cmdCompra.ExecuteScalar();


                    if (Convert.ToInt32(resultado) <= 0) return false;

                    // Eliminar detalles antiguos y reinsertar
                    SqlCommand cmdEliminar = new SqlCommand("PA_ELIMINAR_DETALLE_COMPRA", conexion.SqlC);
                    cmdEliminar.CommandType = CommandType.StoredProcedure;
                    cmdEliminar.Parameters.AddWithValue("@id_compra", getIdCompra());
                    cmdEliminar.ExecuteNonQuery();

                    InsertarDetallesYProveedor(conexion, getIdCompra());

                    limpiarLista(); // heredado de clsLlenarListBox
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error crítico al actualizar: " + ex.Message);
                    return false;
                }
                finally
                {
                    conexion.Cerrar();
                }

            }

        // MÉTODO PRIVADO COMPARTIDO ENTRE INSERTAR Y ACTUALIZAR
        /// <summary>
        /// Inserta los registros de detalle para la compra indicada y asegura la relación
        /// producto-proveedor insertando el vínculo si no existe.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idCompraParam">Id de la compra a la que pertenecen los detalles.</param>
        private void InsertarDetallesYProveedor(clsConexion conexion, int idCompraParam)
        {
            foreach (DataRow fila in listaProductos.Rows) // listaProductos heredado de clsLlenarListBox
            {
                int idProductoFila = Convert.ToInt32(fila["idProducto"]);
                int cantidadFila = Convert.ToInt32(fila["cantidad"]);

                SqlCommand cmdDetalle = new SqlCommand("PA_INSERTAR_DETALLE_COMPRA", conexion.SqlC);
                cmdDetalle.CommandType = CommandType.StoredProcedure;
                cmdDetalle.Parameters.AddWithValue("@id_compra", idCompraParam);
                cmdDetalle.Parameters.AddWithValue("@id_producto", idProductoFila);
                cmdDetalle.Parameters.AddWithValue("@cantidad", cantidadFila);
                cmdDetalle.ExecuteNonQuery();

                if (getProveedor() != 0 && idProductoFila != 0)
                {
                    string sqlVerificar = @"SELECT COUNT(*) FROM Vista_Producto_Proveedor
                                            WHERE id_producto  = @id_producto
                                              AND id_proveedor = @id_proveedor";

                    SqlCommand cmdVerificar = new SqlCommand(sqlVerificar, conexion.SqlC);
                    cmdVerificar.Parameters.AddWithValue("@id_producto", idProductoFila);
                    cmdVerificar.Parameters.AddWithValue("@id_proveedor", getProveedor());

                    int existe = Convert.ToInt32(cmdVerificar.ExecuteScalar());
                    if (existe == 0)
                    {
                        SqlCommand cmdPP = new SqlCommand("PA_INSERTAR_PRODUCTO_PROVEEDOR", conexion.SqlC);
                        cmdPP.CommandType = CommandType.StoredProcedure;
                        cmdPP.Parameters.AddWithValue("@id_producto", idProductoFila);
                        cmdPP.Parameters.AddWithValue("@id_proveedor", getProveedor());
                        cmdPP.ExecuteNonQuery();
                    }
                }
            }
        }

        // METODO PARA CARGAR PRODUCTOS POR COMPRA
        /// <summary>
        /// Carga los productos asociados a una compra (detalle) en la tabla interna <c>listaProductos</c>.
        /// Usa la vista <c>VistaBuscarCompra</c> para obtener los campos necesarios.
        /// </summary>
        /// <param name="idCompra">Id de la compra a cargar.</param>
        /// <param name="conexion">Conexión a la base de datos.</param>
        public void cargarProductosPorCompra(int idCompra, clsConexion conexion)
        {
            try
            {
                conexion.Abrir();
                string consulta = "SELECT * FROM VistaBuscarCompra WHERE id_compra = @id_compra";
                SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC);
                cmd.Parameters.AddWithValue("@id_compra", idCompra);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                listaProductos.Clear();

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                foreach (DataRow fila in dt.Rows)
                {
                    DataRow nuevaFila = listaProductos.NewRow();
                    nuevaFila["idProducto"] = fila["id_producto"];
                    nuevaFila["nombreProducto"] = fila["nombre_producto"];
                    nuevaFila["cantidad"] = fila["cantidad"];
                    nuevaFila["precioCosto"] = fila["precio_costo"]; // Verifica el nombre exacto en tu vista
                    nuevaFila["esNuevo"] = false;

                    listaProductos.Rows.Add(nuevaFila);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
        }
        //METODO PARA AUTORIZAR COMPRA
        /// <summary>
        /// Autoriza una compra ejecutando el procedimiento <c>PA_AUTORIZAR_COMPRA</c>.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idCompra">Id de la compra a autorizar.</param>
        /// <returns>True si la autorización fue exitosa; false en caso contrario.</returns>
        public bool autorizarCompra(clsConexion conexion, int idCompra)
        {
            try
            {
                conexion.Abrir();

                using (SqlCommand cmd = new SqlCommand("PA_AUTORIZAR_COMPRA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_compra", idCompra);

                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al autorizar la compra: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        //METODDO PARA DENEGAR COMPRA
        /// <summary>
        /// Deniega una compra ejecutando el procedimiento <c>PA_DENEGAR_COMPRA</c>.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idCompra">Id de la compra a denegar.</param>
        /// <returns>True si la operación fue exitosa; false en caso de error.</returns>
        public bool denegarCompra(clsConexion conexion, int idCompra)
        {
            try
            {
                conexion.Abrir();

                using (SqlCommand cmd = new SqlCommand("PA_DENEGAR_COMPRA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_compra", idCompra);

                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al denegar la compra: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }
        //METODO PARA ACTULIZAR CANTIDAD EN DETALLE COMPRA
        /// <summary>
        /// Modifica la cantidad de un detalle de compra mediante el procedimiento <c>PA_MODIFICAR_CANTIDAD_DETALLE</c>.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idCompra">Id de la compra que contiene el detalle.</param>
        /// <param name="idProducto">Id del producto cuyo detalle se modificará.</param>
        /// <param name="cantidad">Nueva cantidad a establecer.</param>
        /// <returns>True si la modificación fue exitosa; false en caso de error.</returns>
        public bool modificarCantidad(clsConexion conexion, int idCompra, int idProducto, int cantidad)
        {
            try
            {
                conexion.Abrir();
                SqlCommand cmd = new SqlCommand("PA_MODIFICAR_CANTIDAD_DETALLE", conexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id_compra", idCompra);
                cmd.Parameters.AddWithValue("@id_producto", idProducto);
                cmd.Parameters.AddWithValue("@cantidad", cantidad);

                int filas = cmd.ExecuteNonQuery();
                return filas > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al modificar cantidad: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Marca una compra como recibida y actualiza el stock de cada producto.
        /// Ejecuta el procedimiento <c>PA_RECIBIR_COMPRA</c>.
        /// </summary>
        /// <param name="conexion">Conexión a la base de datos.</param>
        /// <param name="idCompra">Id de la compra a marcar como recibida.</param>
        /// <param name="idUsuario">Id del usuario que confirma la recepción.</param>
        /// <returns>True si la operación fue exitosa; false en caso de error.</returns>
        public bool recibirCompra(clsConexion conexion, int idCompra, int idUsuario, int idSucursal)
        {
            try
            {
                conexion.Abrir();
                using (SqlCommand cmd = new SqlCommand("PA_RECIBIR_COMPRA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_compra", idCompra);
                    cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                    cmd.Parameters.AddWithValue("@id_sucursal", idSucursal);
                    int filas = cmd.ExecuteNonQuery();
                    return filas > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al recibir la compra: " + ex.Message);
                return false;
            }
            finally
            {
                conexion.Cerrar();
            }
        }
    }

}

*/
