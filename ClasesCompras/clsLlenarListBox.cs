/*using Krypton.Toolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.ClasesCompras
{
    /// <summary>
    /// Helper que gestiona una lista en memoria de productos para una operación de compra.
    /// Hereda de <see cref="clsCompras"/> para reutilizar getters y cálculos relacionados.
    /// Proporciona métodos para vincular la lista a un ListBox, agregar/eliminar ítems y calcular totales.
    /// </summary>
    internal class clsLlenarListBox:clsCompras
        {

        /// <summary>
        /// Constructor que inicializa la estructura interna de la tabla de productos.
        /// </summary>
        public clsLlenarListBox() {
            listaProductos.Columns.Add("idProducto", typeof(int));
            listaProductos.Columns.Add("nombreProducto", typeof(string));
            listaProductos.Columns.Add("cantidad", typeof(int));
            listaProductos.Columns.Add("precioCosto", typeof(decimal));
            listaProductos.Columns.Add("esNuevo", typeof(bool));
        }

        /// <summary>
        /// Tabla en memoria que contiene los productos agregados (id, nombre, cantidad y precio de costo).
        /// </summary>
        public DataTable listaProductos = new DataTable();


        /// <summary>
        /// Vincula la tabla interna al <see cref="KryptonListBox"/>, usando la columna <c>nombreProducto</c>
        /// como el valor que se mostrará en la lista.
        /// </summary>
        /// <param name="lst">ListBox donde se mostrará la lista de productos.</param>
        public void vincularListBox(KryptonListBox lst)
        {
            lst.DataSource = listaProductos;
            lst.DisplayMember = "nombreProducto";
        }


        /// <summary>
        /// Agrega el producto actual (según los getters heredados) a la lista con el precio proporcionado.
        /// Si el producto ya existe en la lista no se añade y se muestra un mensaje de aviso.
        /// </summary>
        /// <param name="precioDirecto">Precio de costo a asociar al producto en la lista.</param>
        public void agregarProducto(decimal precioDirecto)
        {
            // Validar que no se agregue el mismo producto dos veces
            bool existe = false;
            foreach (DataRow fila in listaProductos.Rows)
            {
                if (Convert.ToInt32(fila["idProducto"]) == getProducto())
                {
                    existe = true;
                    break;
                }
            }

            if (existe)
            {
                MessageBox.Show("Este producto ya fue agregado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataRow nuevaFila = listaProductos.NewRow();
            nuevaFila["idProducto"] = getProducto();
            nuevaFila["nombreProducto"] = getNombreProducto();
            nuevaFila["cantidad"] = getCantidad();
            nuevaFila["precioCosto"] = precioDirecto;
            nuevaFila["esNuevo"] = true;
            listaProductos.Rows.Add(nuevaFila); // El ListBox se actualiza automaticamente
        }


        /// <summary>
        /// Elimina el producto actualmente seleccionado en el ListBox.
        /// </summary>
        /// <param name="lst">ListBox vinculado que contiene la selección.</param>
        public void eliminarProducto(KryptonListBox lst)
        {
            if (lst.SelectedIndex == -1)
            {
                MessageBox.Show("Seleccione un producto para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            listaProductos.Rows[lst.SelectedIndex].Delete();
            listaProductos.AcceptChanges();
        }

        /// <summary>
        /// Calcula el total de la lista incluyendo IVA.
        /// </summary>
        /// <returns>Total calculado como SUM(cantidad * precioCosto) * (1 + IVA).</returns>
        public decimal SubtotalLista()
        {
            decimal subtotal = 0;
            foreach (DataRow fila in listaProductos.Rows)
                subtotal += Convert.ToInt32(fila["cantidad"]) * Convert.ToDecimal(fila["precioCosto"]);
            return subtotal;
        }

        public decimal ImpuestoLista()
        {
            return SubtotalLista() * getIVA();
        }

        // Reemplaza el TotalLista() existente con este:
        public decimal TotalLista()
        {
            return SubtotalLista() + ImpuestoLista();
        }

        /// <summary>
        /// Limpia todos los elementos de la lista interna de productos.
        /// </summary>
        public void limpiarLista()
        {
            listaProductos.Rows.Clear();
        }

        /// <summary>
        /// Devuelve la fila de la lista en el índice indicado.
        /// </summary>
        /// <param name="indice">Índice de la fila a obtener.</param>
        /// <returns>Objeto <see cref="DataRow"/> correspondiente.</returns>
        public DataRow obtenerFila(int indice)
        {
            return listaProductos.Rows[indice];
        }

    }
}

*/
