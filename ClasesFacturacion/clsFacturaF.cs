/*using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TRAMADE.ClasesFacturacion;

namespace TRAMADE
{
    /// <summary>
    /// Representa una línea de detalle de factura: el identificador del producto y la cantidad.
    /// </summary>
    public class clsDetalleFactura
    {
        /// <summary>Identificador del producto.</summary>
        public int id_producto { get; set; }
        /// <summary>Cantidad vendida del producto (unidades o medida correspondiente).</summary>
        public decimal cantidad { get; set; }
    }

    public class clsFacturaF
    {
        /// <summary>Número de factura que será asignado al documento.</summary>
        public int numero_factura { get; set; }
        /// <summary>Identificador del usuario que emite la factura.</summary>
        public int id_usuario { get; set; }
        /// <summary>Identificador de la forma de pago seleccionada.</summary>
        public int id_forma_pago { get; set; }
        /// <summary>Identificador del cliente asociado a la factura.</summary>
        public int id_cliente { get; set; }
        /// <summary>Fecha de emisión de la factura.</summary>
        public DateTime fecha_emision { get; set; }
        /// <summary>Fecha de vencimiento de la factura.</summary>
        public DateTime fecha_vencimiento { get; set; }
        /// <summary>Subtotal sin impuestos.</summary>
        public decimal subtotal { get; set; }
        /// <summary>Importe de impuestos aplicados.</summary>
        public decimal impuesto { get; set; }
        /// <summary>Total de la factura (subtotal + impuesto).</summary>
        public decimal total { get; set; }

        /// <summary>
        /// Lista de detalles que incluye cada producto agregado a la factura y su cantidad.
        /// </summary>
        public List<clsDetalleFactura> Detalles { get; set; } = new List<clsDetalleFactura>();

        /// <summary>
        /// Obtiene el siguiente número de factura disponible consultando la tabla FACTURA.
        /// </summary>
        /// <returns>Entero con el siguiente número de factura (MAX(numero_factura) + 1).</returns>
        public int ObtenerSiguienteNumeroFactura()
        {
            int siguiente = 1;
            clsConexion conexion = new clsConexion();
            conexion.Abrir();
            try
            {
                // Busca el número máximo. Si no hay nada (ISNULL), toma 0. Y le suma 1.
                string query = "SELECT ISNULL(MAX(numero_factura), 0) + 1 FROM FACTURA";
                SqlCommand cmd = new SqlCommand(query, conexion.SqlC);
                siguiente = Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el número de factura: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
            return siguiente;
        }

        public bool es_cliente_ocasional { get; set; }
        public string nombre_cliente_ocasional { get; set; }
        public string direccion_cliente_ocasional { get; set; }
        public string dni_cliente_ocasional { get; set; }

        /// <summary>
        /// Inserta la factura y sus detalles en una transacción, y actualiza el stock restando
        /// las cantidades vendidas en la tabla PRODUCTO_SUCURSAL. Si ocurre un error realiza
        /// rollback de la transacción.
        /// </summary>
        /// <returns>Id de la factura generada (SCOPE_IDENTITY) o 0 en caso de error.</returns>
        public int GuardarFacturaTransaccion()
        {
            int idFacturaGenerado = 0;
            clsConexion objConexion = new clsConexion();
            objConexion.Abrir();
            SqlTransaction transaccion = null;

            try
            {
                transaccion = objConexion.SqlC.BeginTransaction();

                // id_cliente va NULL si es ocasional
                string queryFactura = @"INSERT INTO FACTURA 
                              (numero_factura, id_usuario, id_forma_pago, id_cliente, 
                               fecha_emision, fecha_vencimiento, id_estado, id_tipo_factura) 
                              VALUES 
                              (@num, @user, @pago, @cliente, 
                               @emision, @vence, 1, 1);
                              SELECT SCOPE_IDENTITY();";

                SqlCommand cmdFactura = new SqlCommand(queryFactura, objConexion.SqlC, transaccion);
                cmdFactura.Parameters.AddWithValue("@num", numero_factura);
                cmdFactura.Parameters.AddWithValue("@user", id_usuario);
                cmdFactura.Parameters.AddWithValue("@pago", id_forma_pago);
                cmdFactura.Parameters.AddWithValue("@cliente",
                    es_cliente_ocasional ? (object)DBNull.Value : id_cliente);
                cmdFactura.Parameters.AddWithValue("@emision", fecha_emision);
                cmdFactura.Parameters.AddWithValue("@vence", fecha_vencimiento);

                idFacturaGenerado = Convert.ToInt32(cmdFactura.ExecuteScalar());

                // Si es ocasional, insertar en CLIENTE_OCASIONAL
                if (es_cliente_ocasional)
                {
                    clsClienteOcasionalF objOcasional = new clsClienteOcasionalF
                    {
                        nombre_cliente = nombre_cliente_ocasional,
                        direccion_cliente = direccion_cliente_ocasional,
                        dni_cliente = dni_cliente_ocasional
                    };
                    objOcasional.Insertar(idFacturaGenerado, objConexion.SqlC, transaccion);
                }

                // Insertar productos y descontar stock
                foreach (var detalle in Detalles)
                {
                    string queryDetalle = @"INSERT INTO FACTURA_PRODUCTO 
                                  (id_factura, id_producto, cantidad) 
                                  VALUES (@idFac, @idProd, @cant)";

                    SqlCommand cmdDetalle = new SqlCommand(queryDetalle, objConexion.SqlC, transaccion);
                    cmdDetalle.Parameters.AddWithValue("@idFac", idFacturaGenerado);
                    cmdDetalle.Parameters.AddWithValue("@idProd", detalle.id_producto);
                    cmdDetalle.Parameters.AddWithValue("@cant", detalle.cantidad);
                    cmdDetalle.ExecuteNonQuery();

                    string queryDescuento = @"UPDATE PRODUCTO_SUCURSAL 
                                      SET cantidad_stock = cantidad_stock - @cantVendida 
                                      WHERE id_producto = @idProdDesc";

                    SqlCommand cmdDescuento = new SqlCommand(queryDescuento, objConexion.SqlC, transaccion);
                    cmdDescuento.Parameters.AddWithValue("@cantVendida", detalle.cantidad);
                    cmdDescuento.Parameters.AddWithValue("@idProdDesc", detalle.id_producto);
                    cmdDescuento.ExecuteNonQuery();
                }

                transaccion.Commit();
            }
            catch (Exception ex)
            {
                transaccion?.Rollback();
                throw new Exception("Error al guardar factura y descontar stock: " + ex.Message);
            }
            finally
            {
                objConexion.Cerrar();
            }

            return idFacturaGenerado;
        }
    }
}
*/