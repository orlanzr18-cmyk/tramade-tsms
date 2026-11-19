/*using System;
using System.Data;
using System.Data.SqlClient;
using TRAMADE.ClasesFacturacion;

namespace TRAMADE
{
    public class clsEmitirFacturaF
    {
        /// <summary>
        /// Número de la factura (tal como aparece en la cabecera y documentos).
        /// </summary>
        public int numero_factura { get; set; }

        /// <summary>
        /// RTN o DNI del cliente (valor elegido según disponibilidad para mostrar en la factura).
        /// </summary>
        public string dni_rtn_cliente { get; set; }

        /// <summary>
        /// Nombre o razón social del cliente.
        /// </summary>
        public string nombre_cliente { get; set; }

        /// <summary>
        /// Dirección fiscal o de envío del cliente.
        /// </summary>
        public string direccion_cliente { get; set; }

        /// <summary>
        /// Id del usuario que emitió la factura.
        /// </summary>
        public int id_usuario { get; set; }

        /// <summary>
        /// Nombre del usuario que emitió la factura.
        /// </summary>
        public string nombre_usuario { get; set; }

        /// <summary>
        /// Fecha de emisión de la factura.
        /// </summary>
        public DateTime fecha_emision { get; set; }

        /// <summary>
        /// Fecha de vencimiento de la factura.
        /// </summary>
        public DateTime fecha_vencimiento { get; set; }

        /// <summary>
        /// Subtotal antes de impuestos.
        /// </summary>
        public decimal subtotal { get; set; }

        /// <summary>
        /// Valor del impuesto aplicado.
        /// </summary>
        public decimal impuesto { get; set; }

        /// <summary>
        /// Total final de la factura (subtotal + impuesto).
        /// </summary>
        public decimal total { get; set; }

        /// <summary>
        /// Identificador de la forma de pago usada en la factura.
        /// </summary>
        public int id_forma_pago { get; set; }

        /// <summary>
        /// Carga los datos maestros de la factura (cabecera) en las propiedades de la instancia
        /// a partir del <paramref name="idFactura"/> proporcionado.
        /// </summary>
        /// <param name="idFactura">Identificador de la factura a cargar.</param>
        /// <returns>True si se encontró y cargó la factura; false en caso contrario.</returns>
        public bool CargarMaestro(int idFactura)
        {
            bool encontrado = false;
            clsConexion conexion = new clsConexion();
            conexion.Abrir();


            try
            {
                string queryMaestro = @"SELECT F.numero_factura, F.fecha_emision, F.fecha_vencimiento, 
                               F.id_forma_pago, F.id_cliente,
                               V.subtotal, V.impuesto, V.total,
                               C.nombre_cliente, C.rtn_cliente, C.dni_cliente, C.direccion_cliente,
                               U.id_usuario, U.nombre_usuario
                        FROM VW_FACTURA_CON_TOTALES V
                        INNER JOIN FACTURA F ON V.id_factura = F.id_factura
                        LEFT JOIN CLIENTE C ON F.id_cliente = C.id_cliente
                        INNER JOIN USUARIO U ON F.id_usuario = U.id_usuario
                        WHERE V.id_factura = @id";

                SqlCommand cmd = new SqlCommand(queryMaestro, conexion.SqlC);
                cmd.Parameters.AddWithValue("@id", idFactura);

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    numero_factura = Convert.ToInt32(dr["numero_factura"]);

                    // Leemos todo lo que necesitamos ANTES de cerrar el reader
                    bool esOcasional = dr["id_cliente"] == DBNull.Value;
                    id_usuario = Convert.ToInt32(dr["id_usuario"]);
                    nombre_usuario = dr["nombre_usuario"].ToString();
                    fecha_emision = Convert.ToDateTime(dr["fecha_emision"]);
                    fecha_vencimiento = Convert.ToDateTime(dr["fecha_vencimiento"]);
                    subtotal = Convert.ToDecimal(dr["subtotal"]);
                    impuesto = Convert.ToDecimal(dr["impuesto"]);
                    total = Convert.ToDecimal(dr["total"]);
                    id_forma_pago = Convert.ToInt32(dr["id_forma_pago"]);

                    // Guardamos rtn y dni antes de cerrar por si no es ocasional
                    string rtn_bd = esOcasional ? "" : dr["rtn_cliente"].ToString();
                    string dni_bd = esOcasional ? "" : dr["dni_cliente"].ToString();
                    string nombre_bd = esOcasional ? "" : dr["nombre_cliente"].ToString();
                    string dir_bd = esOcasional ? "" : dr["direccion_cliente"].ToString();

                    encontrado = true;
                    dr.Close(); // cerramos aquí para poder abrir otro reader si hace falta

                    if (esOcasional)
                    {
                        clsClienteOcasionalF objOcasional = new clsClienteOcasionalF();
                        if (objOcasional.CargarPorFactura(idFactura, conexion.SqlC))
                        {
                            nombre_cliente = objOcasional.nombre_cliente;
                            direccion_cliente = objOcasional.direccion_cliente;
                            dni_rtn_cliente = objOcasional.dni_cliente;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(rtn_bd) && !rtn_bd.StartsWith("SN-"))
                            dni_rtn_cliente = rtn_bd;
                        else if (!string.IsNullOrEmpty(dni_bd) && !dni_bd.StartsWith("SN-"))
                            dni_rtn_cliente = dni_bd;
                        else
                            dni_rtn_cliente = "S/N";

                        nombre_cliente = nombre_bd;
                        direccion_cliente = dir_bd;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar maestro de factura: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
            return encontrado;
        }

        /// <summary>
        /// Carga los detalles (líneas) de la factura indicada y devuelve un <see cref="DataTable"/>
        /// con las columnas: Producto, Descripción, Cantidad, Precio Unitario y Subtotal.
        /// </summary>
        /// <param name="idFactura">Id de la factura cuyos detalles se desean cargar.</param>
        /// <returns>DataTable con los detalles de la factura.</returns>
        public DataTable CargarDetalles(int idFactura)
        {
            DataTable dt = new DataTable();
            clsConexion conexion = new clsConexion();
            conexion.Abrir();

            try
            {
                string queryDetalle = @"SELECT P.nombre_producto as Producto, 
                                               'Unidad' as Descripción, 
                                               FP.cantidad as Cantidad, 
                                               P.precio_unitario as [Precio Unitario], 
                                               (FP.cantidad * P.precio_unitario) as Subtotal
                                        FROM FACTURA_PRODUCTO FP
                                        INNER JOIN PRODUCTO P ON FP.id_producto = P.id_producto
                                        WHERE FP.id_factura = @id";

                SqlDataAdapter da = new SqlDataAdapter(queryDetalle, conexion.SqlC);
                da.SelectCommand.Parameters.AddWithValue("@id", idFactura);
                da.Fill(dt);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cargar detalle de factura: " + ex.Message);
            }
            finally
            {
                conexion.Cerrar();
            }
            return dt;
        }
    }
}
*/