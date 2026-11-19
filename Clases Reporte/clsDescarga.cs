/*
using Microsoft.Reporting.WinForms;
using System;
using System.IO;
using System.Windows.Forms;

namespace TRAMADE.Clases_Reporte
{
    internal class clsDescarga
    {
        /// <summary>
        /// Genera y descarga un informe a disco en el formato especificado.
        /// Renderiza el <see cref="LocalReport"/> en memoria y muestra un diálogo para guardar el archivo.
        /// </summary>
        /// <param name="reportOriginal">Instancia de <see cref="LocalReport"/> que contiene el informe a exportar.</param>
        /// <param name="formato">Formato de salida: "PDF" para PDF, cualquier otro valor para Excel (.xlsx).</param>
        public static void Descargar(LocalReport reportOriginal, string formato)
        {
            if (reportOriginal == null) return;

            try
            {
                string mimeType, encoding, extension;
                string[] streamids;
                Warning[] warnings;

                string deviceInfo = formato == "PDF"
                    ? "<DeviceInfo><OutputFormat>PDF</OutputFormat><EmbedFonts>None</EmbedFonts></DeviceInfo>"
                    : "<DeviceInfo><OutputFormat>EXCELOPENXML</OutputFormat></DeviceInfo>";

                byte[] bytes = reportOriginal.Render(
                    formato == "PDF" ? "PDF" : "EXCELOPENXML",
                    deviceInfo, out mimeType, out encoding, out extension,
                    out streamids, out warnings);

                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = formato == "PDF" ? "PDF|*.pdf" : "Excel|*.xlsx";
                    sfd.FileName = $"Reporte_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        File.WriteAllBytes(sfd.FileName, bytes);
                        MessageBox.Show("Archivo guardado con éxito.", "Éxito",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message +
                    "\n" + ex.InnerException?.Message, "ERROR",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}*/