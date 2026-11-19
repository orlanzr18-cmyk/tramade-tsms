/*using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRAMADE.ClasesFacturacion
{
    public class clsClienteOcasionalF
    {
        public string nombre_cliente { get; set; }
        public string direccion_cliente { get; set; }
        public string dni_cliente { get; set; }

        /// <summary>
        /// Inserta un cliente ocasional vinculado a una factura de contado.
        /// </summary>
        public void Insertar(int idFactura, SqlConnection conn, SqlTransaction transaccion)
        {
            string query = @"INSERT INTO CLIENTE_OCASIONAL 
                            (id_factura, nombre_cliente, direccion_cliente, dni_cliente)
                            VALUES (@idFac, @nombre, @dir, @dni)";

            SqlCommand cmd = new SqlCommand(query, conn, transaccion);
            cmd.Parameters.AddWithValue("@idFac", idFactura);
            cmd.Parameters.AddWithValue("@nombre", nombre_cliente);
            cmd.Parameters.AddWithValue("@dir", direccion_cliente);
            cmd.Parameters.AddWithValue("@dni", string.IsNullOrEmpty(dni_cliente) ?
                                                (object)DBNull.Value : dni_cliente);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Carga los datos del cliente ocasional por id de factura.
        /// </summary>
        public bool CargarPorFactura(int idFactura, SqlConnection conn)
        {
            string query = @"SELECT nombre_cliente, direccion_cliente, dni_cliente
                             FROM CLIENTE_OCASIONAL
                             WHERE id_factura = @idFac";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@idFac", idFactura);
            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                nombre_cliente = dr["nombre_cliente"].ToString();
                direccion_cliente = dr["direccion_cliente"].ToString();
                dni_cliente = dr["dni_cliente"] == DBNull.Value ?
                                    "S/N" : dr["dni_cliente"].ToString();
                dr.Close();
                return true;
            }
            dr.Close();
            return false;
        }
    }
}
*/