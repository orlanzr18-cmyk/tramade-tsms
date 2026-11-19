using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TRAMADE.Models;

namespace TRAMADE.Repositories
{
    public class clsFacturaRepository
    {
        private readonly clsConexion conexion = new clsConexion();

        public int ObtenerCorrelativo()
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_CORRELATIVO_FACTURA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            finally { conexion.Cerrar(); }
        }

        public int GuardarFacturaCompleta(clsFacturaModel factura)
        {
            int idFactura = 0;
            conexion.Abrir();
            SqlTransaction trans = conexion.SqlC.BeginTransaction();

            try
            {
                // 1. Insertar Cabecera
                using (SqlCommand cmdFac = new SqlCommand("PA_INSERTAR_FACTURA_CABECERA", conexion.SqlC, trans))
                {
                    cmdFac.CommandType = CommandType.StoredProcedure;
                    cmdFac.Parameters.AddWithValue("@numero_factura", factura.NumeroFactura);
                    cmdFac.Parameters.AddWithValue("@id_usuario", factura.IdUsuario);
                    cmdFac.Parameters.AddWithValue("@id_forma_pago", factura.IdFormaPago);
                    cmdFac.Parameters.AddWithValue("@id_cliente", factura.EsOcasional ? (object)DBNull.Value : factura.IdCliente);
                    cmdFac.Parameters.AddWithValue("@fecha_emision", factura.FechaEmision);
                    cmdFac.Parameters.AddWithValue("@fecha_vencimiento", factura.FechaVencimiento);
                    idFactura = Convert.ToInt32(cmdFac.ExecuteScalar());
                }

                // 2. Si es ocasional, insertar datos (Hallazgo TS-51-H10)
                if (factura.EsOcasional)
                {
                    using (SqlCommand cmdOca = new SqlCommand("PA_INSERTAR_CLIENTE_OCASIONAL", conexion.SqlC, trans))
                    {
                        cmdOca.CommandType = CommandType.StoredProcedure;
                        cmdOca.Parameters.AddWithValue("@id_factura", idFactura);
                        cmdOca.Parameters.AddWithValue("@nombre_cliente", factura.NombreOcasional);
                        cmdOca.Parameters.AddWithValue("@dni_cliente", factura.DniOcasional);
                        cmdOca.Parameters.AddWithValue("@direccion_cliente", factura.DireccionOcasional);
                        cmdOca.ExecuteNonQuery();
                    }
                }

                // 3. Detalles y Stock (Hallazgo TS-51-H18)
                foreach (var det in factura.Detalles)
                {
                    using (SqlCommand cmdDet = new SqlCommand("PA_INSERTAR_FACTURA_PRODUCTO", conexion.SqlC, trans))
                    {
                        cmdDet.CommandType = CommandType.StoredProcedure;
                        cmdDet.Parameters.AddWithValue("@id_factura", idFactura);
                        cmdDet.Parameters.AddWithValue("@id_producto", det.IdProducto);
                        cmdDet.Parameters.AddWithValue("@cantidad", det.Cantidad);
                        cmdDet.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdStock = new SqlCommand("PA_DESCONTAR_STOCK_PRODUCTO", conexion.SqlC, trans))
                    {
                        cmdStock.CommandType = CommandType.StoredProcedure;
                        cmdStock.Parameters.AddWithValue("@id_producto", det.IdProducto);
                        cmdStock.Parameters.AddWithValue("@cantidad", det.Cantidad);
                        cmdStock.Parameters.AddWithValue("@id_sucursal", clsSesion.id_sucursal);
                        cmdStock.ExecuteNonQuery();
                    }
                }

                trans.Commit();
                return idFactura;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
            finally { conexion.Cerrar(); }
        }

        public clsFacturaConsultaModel ObtenerMaestro(int idFactura)
        {
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_MAESTRO_FACTURA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_factura", idFactura);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            return new clsFacturaConsultaModel
                            {
                                NumeroFactura = Convert.ToInt32(dr["numero_factura"]),
                                IdUsuario = Convert.ToInt32(dr["id_usuario"]),
                                NombreUsuario = dr["nombre_usuario"].ToString(),
                                FechaEmision = Convert.ToDateTime(dr["fecha_emision"]),
                                FechaVencimiento = Convert.ToDateTime(dr["fecha_vencimiento"]),
                                Subtotal = Convert.ToDecimal(dr["subtotal"]),
                                Impuesto = Convert.ToDecimal(dr["impuesto"]),
                                Total = Convert.ToDecimal(dr["total"]),
                                IdFormaPago = Convert.ToInt32(dr["id_forma_pago"]),

                                NombreCliente = dr["nombre_cliente"]?.ToString(),
                                DireccionCliente = dr["direccion_cliente"]?.ToString(),
                                DniRtnCliente = !string.IsNullOrEmpty(dr["rtn_cliente"]?.ToString())
                                                ? dr["rtn_cliente"].ToString()
                                                : dr["dni_cliente"]?.ToString()
                            };
                        }
                    }
                }
            }
            finally { conexion.Cerrar(); }
            return null;
        }

        public List<clsFacturaDetalleConsultaModel> ObtenerDetalles(int idFactura)
        {
            List<clsFacturaDetalleConsultaModel> lista = new List<clsFacturaDetalleConsultaModel>();
            conexion.Abrir();
            try
            {
                using (SqlCommand cmd = new SqlCommand("PA_OBTENER_DETALLES_FACTURA", conexion.SqlC))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_factura", idFactura);

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            decimal cant = Convert.ToDecimal(dr["cantidad"]);
                            decimal precio = Convert.ToDecimal(dr["precio_unitario"]);
                            lista.Add(new clsFacturaDetalleConsultaModel
                            {
                                Producto = dr["nombre_producto"].ToString(),
                                Descripcion = "Unidad",
                                Cantidad = cant,
                                PrecioUnitario = precio,
                                Subtotal = cant * precio
                            });
                        }
                    }
                }
            }
            finally { conexion.Cerrar(); }
            return lista;
        }
    }
}