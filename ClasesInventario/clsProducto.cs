/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRAMADE.ClasesInventario
{
    /// <summary>
    /// Representa un producto del inventario del sistema TRAMADE.
    /// Encapsula todos los atributos de un producto, incluyendo identificación,
    /// precios, stock, imagen y su relación con proveedores, categorías y sucursales.
    /// </summary>
    internal class clsProducto
    {
        /// <summary>
        /// Obtiene o establece el identificador único del producto en la base de datos.
        /// </summary>
        /// <value>
        /// Entero positivo que corresponde a la clave primaria <c>id_producto</c> en la tabla <c>PRODUCTO</c>.
        /// </value>
        public int ID { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre descriptivo del producto.
        /// </summary>
        /// <value>
        /// Cadena de texto con el nombre tal como aparece en el catálogo e inventario.
        /// </value>
        public string Nombre { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre del proveedor asociado al producto.
        /// </summary>
        /// <value>
        /// Nombre del proveedor principal que suministra este producto.
        /// </value>
        public string Proveedor { get; set; }

        /// <summary>
        /// Obtiene o establece la categoría a la que pertenece el producto.
        /// </summary>
        /// <value>
        /// Nombre de la categoría del producto (ej. "Electrónica", "Ferretería", etc.).
        /// </value>
        public string Categoria { get; set; }

        /// <summary>
        /// Obtiene o establece la sucursal donde se encuentra disponible el producto.
        /// </summary>
        /// <value>
        /// Nombre de la sucursal asociada al registro de inventario actual.
        /// </value>
        public string Sucursal { get; set; }

        /// <summary>
        /// Obtiene o establece el precio de venta unitario del producto al cliente.
        /// </summary>
        /// <value>
        /// Valor decimal que representa el precio en lempiras (L.) con dos decimales.
        /// Corresponde a la columna <c>precio_unitario</c> en la tabla <c>PRODUCTO</c>.
        /// </value>
        public decimal PrecioUnitario { get; set; }

        /// <summary>
        /// Obtiene o establece el precio de costo de adquisición del producto.
        /// </summary>
        /// <value>
        /// Valor decimal que representa el costo de compra en lempiras (L.).
        /// Corresponde a la columna <c>precio_costo</c> en la tabla <c>PRODUCTO</c>.
        /// </value>
        public decimal PrecioCosto { get; set; }

        /// <summary>
        /// Obtiene o establece la cantidad de unidades disponibles en inventario.
        /// </summary>
        /// <value>
        /// Número entero que representa las unidades en stock en la sucursal asociada.
        /// </value>
        public int Stock { get; set; }

        /// <summary>
        /// Obtiene o establece la imagen del producto almacenada en formato binario.
        /// </summary>
        /// <value>
        /// Arreglo de bytes que representa la imagen del producto.
        /// Puede ser <c>null</c> si el producto no tiene imagen registrada.
        /// Se almacena como <c>VarBinary</c> en la base de datos.
        /// </value>
        public byte[] Imagen { get; set; }
    }
}
*/