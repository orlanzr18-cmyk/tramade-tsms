using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRAMADE.Formularios_Login__Menú
{
    /// <summary>
    /// Clase base para utilidades de diseño adaptativo.
    /// </summary>
    internal class clsResponsive
    {
    }

    /// <summary>
    /// Gestiona la redimensión automática y proporcional de los controles de un formulario.
    /// Permite que los elementos mantengan su posición relativa, tamaño y escala de fuente
    /// cuando el usuario cambia el tamaño de la ventana.
    /// </summary>
    public class ResponsiveFormManager
    {
        /// <summary>
        /// Estructura interna para almacenar las dimensiones y posición original de un control.
        /// </summary>
        private struct ControlInfo
        {
            public float X, Y, Width, Height;
            public float FontSize;
        }

        private readonly Form _form;
        private readonly Dictionary<Control, ControlInfo> _originalLayout = new Dictionary<Control, ControlInfo>();
        private SizeF _originalFormSize;
        private bool _initialized = false;

        /// <summary>
        /// Crea una nueva instancia del gestor de diseño responsivo para el formulario especificado.
        /// </summary>
        /// <param name="form">El formulario (Form) que se desea hacer responsivo.</param>
        public ResponsiveFormManager(Form form)
        {
            _form = form;
        }

        /// <summary>
        /// Escanea el formulario y guarda las posiciones, tamaños y fuentes originales de todos los controles.
        /// Debe llamarse en el evento 'Load' del formulario, después de 'InitializeComponent()'.
        /// </summary>
        public void Initialize()
        {
            _originalFormSize = new SizeF(_form.ClientSize.Width, _form.ClientSize.Height);
            SaveLayout(_form.Controls);
            _initialized = true;
        }

        /// <summary>
        /// Recalcula y aplica las nuevas dimensiones a todos los controles basadas en la escala actual del formulario.
        /// Debe llamarse en el evento 'Resize' o 'SizeChanged' del formulario.
        /// </summary>
        public void AdjustLayout()
        {
            if (!_initialized) return;

            float scaleX = _form.ClientSize.Width / _originalFormSize.Width;
            float scaleY = _form.ClientSize.Height / _originalFormSize.Height;

            ApplyLayout(_form.Controls, scaleX, scaleY);
        }

        // ── Privados ────────────────────────────────────────────────
        /// <summary>
        /// Recorre recursivamente la colección de controles para guardar sus propiedades originales.
        /// </summary>
        /// <param name="controls">Colección de controles a registrar.</param>

        private void SaveLayout(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                // ✅ Ignorar el MdiClient
                if (ctrl is MdiClient) continue;

                _originalLayout[ctrl] = new ControlInfo
                {
                    X = ctrl.Location.X,
                    Y = ctrl.Location.Y,
                    Width = ctrl.Size.Width,
                    Height = ctrl.Size.Height,
                    FontSize = ctrl.Font.Size
                };
                if (ctrl.HasChildren)
                    SaveLayout(ctrl.Controls);
            }
        }

        /// <summary>
        /// Aplica los cambios de escala a la posición, tamaño y fuente de cada control registrado.
        /// </summary>
        /// <param name="controls">Colección de controles a ajustar.</param>
        /// <param name="scaleX">Factor de escala en el eje X.</param>
        /// <param name="scaleY">Factor de escala en el eje Y.</param>
        private void ApplyLayout(Control.ControlCollection controls, float scaleX, float scaleY)
        {
            foreach (Control ctrl in controls)
            {
                // ✅ Ignorar el MdiClient
                if (ctrl is MdiClient) continue;

                if (!_originalLayout.TryGetValue(ctrl, out ControlInfo info)) continue;
                ctrl.SuspendLayout();
                ctrl.Location = new Point(
                    (int)(info.X * scaleX),
                    (int)(info.Y * scaleY));
                ctrl.Size = new Size(
                    (int)(info.Width * scaleX),
                    (int)(info.Height * scaleY));

                float fontScale = Math.Min(scaleX, scaleY);
                float newSize = Math.Max(5f, info.FontSize * fontScale);
                ctrl.Font = new Font(ctrl.Font.FontFamily, newSize, ctrl.Font.Style);
                ctrl.ResumeLayout();

                if (ctrl.HasChildren)
                    ApplyLayout(ctrl.Controls, scaleX, scaleY);
            }
        }
    }
}
