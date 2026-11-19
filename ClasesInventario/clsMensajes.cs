using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.ClasesInventario
{
    /// <summary>
    /// Proporciona métodos estáticos de utilidad para mostrar cuadros de diálogo
    /// estandarizados en el módulo de Inventario.
    /// Centraliza todos los mensajes al usuario para mantener consistencia visual
    /// y facilitar el mantenimiento de los textos mostrados en la interfaz.
    /// </summary>
    internal class clsMensajes
    {
        /// <summary>
        /// Muestra un cuadro de diálogo de información indicando que una operación
        /// se completó exitosamente.
        /// </summary>
        /// <param name="mensaje">
        /// Texto descriptivo de la operación que fue completada con éxito.
        /// </param>
        /// <remarks>
        /// El cuadro de diálogo se presenta con título "Éxito", ícono informativo
        /// (<see cref="MessageBoxIcon.Information"/>) y un único botón de confirmación (OK).
        /// </remarks>
        /// <example>
        /// <code>
        /// clsMensajes.Exito("Producto registrado correctamente.");
        /// </code>
        /// </example>
        public static void Exito(string mensaje)
        {
            MessageBox.Show(mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Muestra un cuadro de diálogo de error para informar al usuario
        /// que ocurrió un fallo en una operación.
        /// </summary>
        /// <param name="mensaje">
        /// Texto descriptivo del error ocurrido.
        /// </param>
        /// <remarks>
        /// El cuadro de diálogo se presenta con título "ERROR", ícono de error
        /// (<see cref="MessageBoxIcon.Error"/>) y un único botón de confirmación (OK).
        /// Debe usarse para errores de base de datos, validaciones fallidas u operaciones
        /// que no pudieron completarse.
        /// </remarks>
        /// <example>
        /// <code>
        /// clsMensajes.Error("No se pudo eliminar el producto. Verifique la conexión.");
        /// </code>
        /// </example>
        public static void Error(string mensaje)
        {
            MessageBox.Show(mensaje, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Muestra un cuadro de diálogo de advertencia para alertar al usuario
        /// sobre una condición que requiere atención pero no constituye un error crítico.
        /// </summary>
        /// <param name="mensaje">
        /// Texto descriptivo de la advertencia o condición a informar.
        /// </param>
        /// <remarks>
        /// El cuadro de diálogo se presenta con título "Aviso", ícono de advertencia
        /// (<see cref="MessageBoxIcon.Warning"/>) y un único botón de confirmación (OK).
        /// Ideal para validaciones de campos vacíos, datos fuera de rango o situaciones
        /// límite que no impiden el funcionamiento del sistema.
        /// </remarks>
        /// <example>
        /// <code>
        /// clsMensajes.Aviso("No hay productos con código de barras registrado.");
        /// </code>
        /// </example>
        public static void Aviso(string mensaje)
        {
            MessageBox.Show(mensaje, "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Muestra un cuadro de diálogo de confirmación y retorna la respuesta del usuario.
        /// </summary>
        /// <param name="mensaje">
        /// Pregunta o descripción de la acción que se solicita confirmar al usuario.
        /// </param>
        /// <returns>
        /// <c>true</c> si el usuario seleccionó "Sí" (<see cref="DialogResult.Yes"/>);
        /// <c>false</c> si seleccionó "No".
        /// </returns>
        /// <remarks>
        /// El cuadro de diálogo se presenta con título "Confirmar", ícono de pregunta
        /// (<see cref="MessageBoxIcon.Question"/>) y botones Sí/No (<see cref="MessageBoxButtons.YesNo"/>).
        /// Se recomienda usar antes de operaciones destructivas como eliminaciones.
        /// </remarks>
        /// <example>
        /// <code>
        /// if (clsMensajes.Confirmar("¿Está seguro que desea eliminar este producto?"))
        /// {
        ///     clsProductoDAL.EliminarProducto(idProducto);
        /// }
        /// </code>
        /// </example>
        public static bool Confirmar(string mensaje)
        {
            DialogResult result = MessageBox.Show(mensaje, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }
    }
}
