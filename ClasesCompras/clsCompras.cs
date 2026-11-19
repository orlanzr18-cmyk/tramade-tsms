/*using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.SessionState;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;


namespace TRAMADE.ClasesCompras
{
    internal class clsCompras
    {
        /// <summary>
        /// Campos internos que representan el estado de una compra en proceso.
        /// </summary>
        private int cantidad, proveedor, producto, formaPago, estado = 7, idUsuario, idCompra;
        private DateTime entrega;
        private string nombreProducto;
        private decimal precioCosto;
        private bool autorizar = false;
        private const decimal IVA = 0.15m;
        clsConexion ObjConexion = new clsConexion();

        /// <summary>
        /// Constructor por defecto de la clase <see cref="clsCompras"/>.
        /// </summary>
        public clsCompras()
        {
        }

        // ─── SETTERS ───────────────────────────────────────────────
        /// <summary>Asigna el id del proveedor.</summary>
        public void setProveedor(int valor) { proveedor = valor; }
        /// <summary>Asigna el id del producto.</summary>
        public void setProducto(int valor) { producto = valor; }
        /// <summary>Asigna la forma de pago seleccionada.</summary>
        public void setFormaPago(int valor) { formaPago = valor; }
        /// <summary>Asigna la cantidad de unidades a comprar.</summary>
        public void setCantidad(int valor) { cantidad = valor; }
        /// <summary>Asigna el nombre descriptivo del producto.</summary>
        public void setNombreProducto(string valor) { nombreProducto = valor; }
        /// <summary>Asigna el precio de costo unitario.</summary>
        public void setPrecio(decimal valor) { precioCosto = valor; }
        /// <summary>Asigna la fecha de entrega prevista.</summary>
        public void setEntrega(DateTime valor) { entrega = valor; }
        /// <summary>Asigna el identificador de la compra (cuando aplica).</summary>
        public void setIdCompra(int valor) { idCompra = valor; }


        // ─── GETTERS ───────────────────────────────────────────────
        /// <summary>Devuelve el id del producto.</summary>
        public int getProducto() { return producto; }
        /// <summary>Devuelve la cantidad solicitada.</summary>
        public int getCantidad() { return cantidad; }
        /// <summary>Devuelve el nombre del producto.</summary>
        public string getNombreProducto() { return nombreProducto; }
        /// <summary>Devuelve el precio de costo unitario.</summary>
        public decimal getPrecio() { return precioCosto; }
        /// <summary>Devuelve la constante de IVA aplicada.</summary>
        public decimal getIVA() { return IVA; }
        /// <summary>Devuelve el id del proveedor.</summary>
        public int getProveedor() { return proveedor; }
        /// <summary>Devuelve la forma de pago seleccionada.</summary>
        public int getFormaPago() { return formaPago; }
        /// <summary>Devuelve el estado actual de la compra.</summary>
        public int getEstado() { return estado; }
        /// <summary>Devuelve el id del usuario que realiza la operación.</summary>
        public int getIdUsuario() { return idUsuario; }
        /// <summary>Devuelve el id de la compra (si existe).</summary>
        public int getIdCompra() { return idCompra; }
        /// <summary>Devuelve la fecha de entrega prevista.</summary>
        public DateTime getEntrega() { return entrega; }
        /// <summary>Indica si la compra requiere autorización.</summary>
        public bool getAutorizar() { return autorizar; }

        // ─── CÁLCULOS ──────────────────────────────────────────────
        /// <summary>Asigna el id del usuario que realiza la operación.</summary>
        public void setIdUsuario(int valor) { idUsuario = valor; }
        /// <summary>Calcula el subtotal (cantidad * precio unitario).</summary>
        public decimal Subtotal() { return cantidad * precioCosto; }
        /// <summary>Calcula el impuesto (IVA) basado en el subtotal.</summary>
        public decimal Impuesto() { return Subtotal() * IVA; }
        /// <summary>Calcula el total incluyendo IVA.</summary>
        public decimal Total() { return Subtotal() * (1 + IVA); }


        // ─── CARGA DE DATOS ───────────────────────────────────────
        /// <summary>
        /// Recupera la tabla de compras (vista <c>VistaComprasTabla</c>) desde la base de datos.
        /// </summary>
        /// <returns>DataTable con las compras o <c>null</c> en caso de error.</returns>
        public DataTable obtenerTablaCompras()
        {
            try
            {
                ObjConexion.Abrir();
                SqlCommand cmd = new SqlCommand("PA_OBTENER_COMPRAS", ObjConexion.SqlC);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al recargar: " + ex.Message);
                return null;
            }
            finally
            {
                ObjConexion.Cerrar();
            }
        }

    }
}

*/
