/*using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.ClasesCompras
{
    internal class clsFiltrarCompras
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="clsFiltrarCompras"/>.
        /// </summary>
        public clsFiltrarCompras() { }

        /// <summary>
        /// Filtra las compras por rango de fechas y, opcionalmente, por id de compra si el texto de búsqueda
        /// contiene un número entero. Devuelve las filas de la vista <c>VistaComprasTabla</c> que cumplen
        /// el criterio de búsqueda.
        /// </summary>
        /// <param name="conexion">Instancia de <see cref="clsConexion"/> usada para ejecutar la consulta.</param>
        /// <param name="desde">Selector de fecha de inicio del rango (se toma la fecha sin hora).</param>
        /// <param name="hasta">Selector de fecha final del rango (se considera hasta las 23:59:59 del día).</param>
        /// <param name="textoBuscar">Texto opcional para buscar el id de compra; si es un número entero se filtra por id.</param>
        /// <returns>Un <see cref="DataTable"/> con los registros filtrados o <c>null</c> en caso de error.</returns>
        public DataTable FiltrarFechas(clsConexion conexion, KryptonDateTimePicker desde, KryptonDateTimePicker hasta, string textoBuscar)
        {
            try
            {
                conexion.Abrir();

                string consulta = "SELECT * FROM VistaComprasTabla WHERE [Fecha pedido] >= @desde AND [Fecha pedido] <= @hasta";

                int id = 0;
                bool filtrarPorId = int.TryParse(textoBuscar.Trim(), out id);

                if (filtrarPorId)
                {
                    consulta += " AND [ID compra] = @id";
                }

                using (SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC))
                {
                    cmd.Parameters.AddWithValue("@desde", desde.Value.Date);
                    cmd.Parameters.AddWithValue("@hasta", hasta.Value.Date.AddDays(1).AddSeconds(-1));

                    if (filtrarPorId)
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al filtrar compras: " + ex.Message);
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }

        /// <summary>
        /// Busca una compra por id si <paramref name="textoBuscar"/> contiene un entero.
        /// Si no es posible interpretar el texto como id, devuelve todas las filas de <c>VistaComprasTabla</c>.
        /// </summary>
        /// <param name="conexion">Instancia de <see cref="clsConexion"/> usada para la consulta.</param>
        /// <param name="textoBuscar">Texto que puede contener el id de compra a buscar.</param>
        /// <returns>Un <see cref="DataTable"/> con la(s) compra(s) encontrada(s) o <c>null</c> en caso de error.</returns>
        public DataTable BuscarCompra(clsConexion conexion, string textoBuscar)
        {
            try
            {
                conexion.Abrir();

                string consulta = "SELECT * FROM VistaComprasTabla";

                int id = 0;
                bool filtrarPorId = int.TryParse(textoBuscar.Trim(), out id);

                if (filtrarPorId)
                {
                    consulta += " WHERE [ID compra] = @id";
                }

                using (SqlCommand cmd = new SqlCommand(consulta, conexion.SqlC))
                {
                    if (filtrarPorId)
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al buscar compra: " + ex.Message);
                return null;
            }
            finally
            {
                conexion.Cerrar();
            }
        }
    }
}

*/
