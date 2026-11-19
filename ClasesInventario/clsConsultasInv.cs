/*using System;
using System.Data;
using System.Data.SqlClient;

namespace TRAMADE.ClasesInventario
{
   
    internal class clsConsultasInv
    {
       
        public static DataTable ObtenerProductos(string filtro, int pagina, int registrosPorPagina)
        {
            clsConexion obj = new clsConexion();
            obj.Abrir();
            int offset = (pagina - 1) * registrosPorPagina;
            SqlCommand cmd = new SqlCommand("PA_OBTENER_PRODUCTOS", obj.SqlC);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@filtro", filtro);
            cmd.Parameters.AddWithValue("@offset", offset);
            cmd.Parameters.AddWithValue("@cantidad", registrosPorPagina);
            cmd.Parameters.AddWithValue("@idSucursal", clsSesion.id_sucursal);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            obj.Cerrar();
            return dt;
        }
        
        public static void EliminarProducto(SqlConnection con, int idProducto)
        {
            SqlCommand cmd = new SqlCommand("PA_ELIMINAR_PRODUCTO", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@idProducto", idProducto);
            cmd.ExecuteNonQuery();
        }

        
        public static int VerificarProductoExiste(string nombreProducto)
        {
            clsConexion obj = new clsConexion();
            obj.Abrir();
            SqlCommand cmd = new SqlCommand("PA_VERIFICAR_PRODUCTO_EXISTE", obj.SqlC);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@nombre", nombreProducto);
            int count = (int)cmd.ExecuteScalar();
            obj.Cerrar();
            return count;
        }

       
    }
}
*/