/*using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace TRAMADE
{
    /// <summary>
    /// Clase encargada de la lógica de generación y exportación de documentos PDF
    /// utilizando la librería iTextSharp.
    /// </summary>
    public class clsExportarPDFF
    {
        /// <summary>
        /// Crea un documento PDF profesional con el formato de factura de la empresa TRAMADE,
        /// incluyendo encabezados, datos del cliente/vendedor, tabla de productos y totales.
        /// </summary>
        /// <param name="datos">Objeto con toda la información necesaria para llenar la factura.</param>
        public void GenerarFactura(clsFacturaDatosPDF datos)
        {
            string rutaPDF = ObtenerRutaGuardado(datos.NumeroFactura);
            if (rutaPDF == null) return;

            Document document = new Document(PageSize.LETTER, 40, 40, 40, 40);
            PdfWriter.GetInstance(document, new FileStream(rutaPDF, FileMode.Create));
            document.Open();

            // ─── FUENTES ──────────────────────────────────────────
            var fTitulo = FontFactory.GetFont("Arial", 18, Font.BOLD);
            var fNormal = FontFactory.GetFont("Arial", 10, Font.NORMAL);
            var fNegrita = FontFactory.GetFont("Arial", 10, Font.BOLD);
            var fBlanco = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
            var fPequena = FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.GRAY);
            BaseColor colorCafe = new BaseColor(139, 90, 43);
            BaseColor colorCafeClaro = new BaseColor(245, 235, 220);

            // ─── ENCABEZADO ───────────────────────────────────────
            document.Add(new Paragraph("TRAMADE", fTitulo));
            document.Add(new Paragraph("Módulo de Facturación", fNormal));
            document.Add(new Paragraph(" "));

            // ─── NÚMERO DE FACTURA ────────────────────────────────
            var pNumero = new Paragraph();
            pNumero.Add(new Chunk("Factura N°: ", fNormal));
            pNumero.Add(new Chunk(datos.NumeroFactura, fNegrita));
            document.Add(pNumero);
            document.Add(new Paragraph(" "));

            // ─── DATOS CLIENTE / VENDEDOR (tabla de 2 columnas) ──
            PdfPTable tblDatos = new PdfPTable(2);
            tblDatos.WidthPercentage = 100;
            tblDatos.SetWidths(new float[] { 1f, 1f });
            tblDatos.DefaultCell.Border = PdfPCell.NO_BORDER;

            // Columna izquierda — Cliente
            var celdaCliente = new PdfPCell();
            celdaCliente.Border = PdfPCell.NO_BORDER;
            celdaCliente.AddElement(new Paragraph("Datos del Cliente", fNegrita));
            celdaCliente.AddElement(new Paragraph("DNI/RTN: " + datos.DniCliente, fNormal));
            celdaCliente.AddElement(new Paragraph("Cliente: " + datos.NombreCliente, fNormal));
            celdaCliente.AddElement(new Paragraph("Dirección: " + datos.DireccionCliente, fNormal));
            tblDatos.AddCell(celdaCliente);

            // Columna derecha — Vendedor
            var celdaVendedor = new PdfPCell();
            celdaVendedor.Border = PdfPCell.NO_BORDER;
            celdaVendedor.AddElement(new Paragraph("Datos del Vendedor", fNegrita));
            celdaVendedor.AddElement(new Paragraph("ID: " + datos.IdVendedor, fNormal));
            celdaVendedor.AddElement(new Paragraph("Nombre: " + datos.NombreVendedor, fNormal));
            tblDatos.AddCell(celdaVendedor);

            document.Add(tblDatos);
            document.Add(new Paragraph(" "));

            // ─── FECHAS ───────────────────────────────────────────
            var tblFechas = new PdfPTable(2);
            tblFechas.WidthPercentage = 100;
            tblFechas.DefaultCell.Border = PdfPCell.NO_BORDER;

            var cFechaIzq = new PdfPCell(new Phrase("Emisión: " + datos.FechaEmision, fNormal));
            cFechaIzq.Border = PdfPCell.NO_BORDER;
            var cFechaDer = new PdfPCell(new Phrase("Vencimiento: " + datos.FechaVencimiento, fNormal));
            cFechaDer.Border = PdfPCell.NO_BORDER;
            tblFechas.AddCell(cFechaIzq);
            tblFechas.AddCell(cFechaDer);

            document.Add(tblFechas);
            document.Add(new Paragraph(" "));

            // ─── TABLA DE PRODUCTOS ───────────────────────────────
            PdfPTable tabla = new PdfPTable(5);
            tabla.WidthPercentage = 100;
            tabla.SetWidths(new float[] { 2f, 4f, 1.5f, 2f, 2f });

            // Encabezados
            string[] encabezados = { "Producto", "Descripción", "Cantidad", "Precio Unit.", "Subtotal" };
            foreach (var enc in encabezados)
            {
                PdfPCell celda = new PdfPCell(new Phrase(enc, fBlanco));
                celda.BackgroundColor = colorCafe;
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                celda.Padding = 6;
                tabla.AddCell(celda);
            }

            // ─── FILAS DE PRODUCTOS (aquí estaba el problema) ────
            bool filaPar = false;
            foreach (var prod in datos.Productos)
            {
                BaseColor fondoFila = filaPar ? colorCafeClaro : BaseColor.WHITE;

                PdfPCell[] celdas = {
                    new PdfPCell(new Phrase(prod.Nombre,                           fNormal)),
                    new PdfPCell(new Phrase(prod.Descripcion,                      fNormal)),
                    new PdfPCell(new Phrase(prod.Cantidad.ToString(),              fNormal)),
                    new PdfPCell(new Phrase("L. " + prod.PrecioUnitario.ToString("N2"), fNormal)),
                    new PdfPCell(new Phrase("L. " + prod.Subtotal.ToString("N2"), fNormal))
                };

                foreach (var c in celdas)
                {
                    c.BackgroundColor = fondoFila;
                    c.HorizontalAlignment = Element.ALIGN_CENTER;
                    c.Padding = 5;
                    tabla.AddCell(c);
                }

                filaPar = !filaPar;
            }

            document.Add(tabla);
            document.Add(new Paragraph(" "));

            // ─── TOTALES ──────────────────────────────────────────
            PdfPTable tblTotales = new PdfPTable(2);
            tblTotales.WidthPercentage = 50;
            tblTotales.HorizontalAlignment = Element.ALIGN_LEFT;
            tblTotales.DefaultCell.Border = PdfPCell.NO_BORDER;

            void AgregarFila(string label, string valor, iTextSharp.text.Font fuente)
            {
                var cL = new PdfPCell(new Phrase(label, fuente)); cL.Border = PdfPCell.NO_BORDER;
                var cV = new PdfPCell(new Phrase(valor, fuente)); cV.Border = PdfPCell.NO_BORDER;
                tblTotales.AddCell(cL);
                tblTotales.AddCell(cV);
            }

            AgregarFila("Subtotal:", datos.Subtotal, fNormal);
            AgregarFila("Impuesto 15%:", datos.Impuesto, fNormal);
            AgregarFila("Total a pagar:", datos.Total, fNegrita);

            document.Add(tblTotales);
            document.Add(new Paragraph(" "));

            // ─── FORMA DE PAGO ────────────────────────────────────
            document.Add(new Paragraph("Forma de pago: " + datos.FormaPago, fNormal));
            document.Add(new Paragraph(" "));

            // ─── PIE DE PÁGINA ────────────────────────────────────
            document.Add(new Paragraph("¡MUCHAS GRACIAS POR SU COMPRA!", fPequena));

            document.Close();
            MessageBox.Show("PDF generado exitosamente en:\n" + rutaPDF, "TRAMADE",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Muestra un cuadro de diálogo al usuario para seleccionar la ubicación de guardado
        /// y define el nombre por defecto del archivo basado en el número de factura.
        /// </summary>
        /// <param name="numeroFactura">Número identificador de la factura para el nombre del archivo.</param>
        /// <returns>La ruta completa del archivo si se aceptó el diálogo; de lo contrario, null.</returns>
        private string ObtenerRutaGuardado(string numeroFactura)
        {
            string nombreArchivo = "Factura_" + numeroFactura.Replace("/", "_");

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF|*.pdf";
            sfd.FileName = nombreArchivo; 
            return sfd.ShowDialog() == DialogResult.OK ? sfd.FileName : null;
        }
    }

    /// <summary>
    /// Modelo de datos que representa un producto individual dentro de una factura PDF.
    /// </summary>
    // ─── CLASES DE DATOS ──────────────────────────────────────
    public class clsProductoPDF
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    /// <summary>
    /// Clase contenedora de toda la información de cabecera, detalle y totales
    /// requerida para la generación del reporte PDF.
    /// </summary>
    public class clsFacturaDatosPDF
    {
        public string NumeroFactura { get; set; }
        public string DniCliente { get; set; }
        public string NombreCliente { get; set; }
        public string DireccionCliente { get; set; }
        public string IdVendedor { get; set; }
        public string NombreVendedor { get; set; }
        public string FechaEmision { get; set; }
        public string FechaVencimiento { get; set; }
        public string Subtotal { get; set; }
        public string Impuesto { get; set; }
        public string Total { get; set; }
        public string FormaPago { get; set; }
        public List<clsProductoPDF> Productos { get; set; }
    }
}
*/