using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using TRAMADE.Models;

namespace TRAMADE.Services
{
    public class clsPdfService
    {
        // Fuentes y Colores (Hallazgo TS-51-H13: Ahora es más fácil de extender)
        private readonly Font _fTitulo = FontFactory.GetFont("Arial", 18, Font.BOLD);
        private readonly Font _fNormal = FontFactory.GetFont("Arial", 10, Font.NORMAL);
        private readonly Font _fNegrita = FontFactory.GetFont("Arial", 10, Font.BOLD);
        private readonly Font _fBlanco = FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE);
        private readonly BaseColor _colorPrincipal = new BaseColor(139, 90, 43); // Café
        private readonly BaseColor _colorFilaAlterna = new BaseColor(245, 235, 220);

        public void GenerarDocumentoFactura(clsFacturaConsultaModel datos, string rutaDestino)
        {
            Document document = new Document(PageSize.LETTER, 40, 40, 40, 40);

            try
            {
                PdfWriter.GetInstance(document, new FileStream(rutaDestino, FileMode.Create));
                document.Open();

                // Hallazgo TS-51-H13: Dividimos el método en partes para mejor legibilidad
                EscribirEncabezado(document);
                EscribirInformacionGeneral(document, datos);
                EscribirTablaProductos(document, datos);
                EscribirTotales(document, datos);

                document.Close();
            }
            catch (Exception ex)
            {
                if (document.IsOpen()) document.Close();
                throw new Exception("Error técnico al construir el PDF: " + ex.Message);
            }
        }

        private void EscribirEncabezado(Document doc)
        {
            doc.Add(new Paragraph("TRAMADE", _fTitulo));
            doc.Add(new Paragraph("Módulo de Facturación", _fNormal));
            doc.Add(new Paragraph(" "));
        }

        private void EscribirInformacionGeneral(Document doc, clsFacturaConsultaModel datos)
        {
            PdfPTable tbl = new PdfPTable(2) { WidthPercentage = 100 };
            tbl.SetWidths(new float[] { 1f, 1f });

            // Columna Cliente
            PdfPCell c1 = new PdfPCell { Border = PdfPCell.NO_BORDER };
            c1.AddElement(new Paragraph("DATOS DEL CLIENTE", _fNegrita));
            c1.AddElement(new Paragraph($"DNI/RTN: {datos.DniRtnCliente}", _fNormal));
            c1.AddElement(new Paragraph($"Nombre: {datos.NombreCliente}", _fNormal));
            c1.AddElement(new Paragraph($"Dirección: {datos.DireccionCliente}", _fNormal));
            tbl.AddCell(c1);

            // Columna Vendedor / Factura
            PdfPCell c2 = new PdfPCell { Border = PdfPCell.NO_BORDER };
            c2.AddElement(new Paragraph("DETALLE DE FACTURA", _fNegrita));
            c2.AddElement(new Paragraph($"Factura N°: {datos.NumeroFormateado}", _fNormal));
            c2.AddElement(new Paragraph($"Fecha Emisión: {datos.FechaEmision:dd/MM/yyyy}", _fNormal));
            c2.AddElement(new Paragraph($"Vendedor: {datos.NombreUsuario}", _fNormal));
            tbl.AddCell(c2);

            doc.Add(tbl);
            doc.Add(new Paragraph(" "));
        }

        private void EscribirTablaProductos(Document doc, clsFacturaConsultaModel datos)
        {
            PdfPTable tabla = new PdfPTable(5) { WidthPercentage = 100 };
            tabla.SetWidths(new float[] { 2f, 4f, 1.5f, 2f, 2f });

            // Encabezados
            string[] headers = { "Producto", "Descripción", "Cant.", "Precio Unit.", "Subtotal" };
            foreach (var h in headers)
            {
                PdfPCell c = new PdfPCell(new Phrase(h, _fBlanco))
                {
                    BackgroundColor = _colorPrincipal,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };
                tabla.AddCell(c);
            }

            // Filas (Hallazgo TS-51-H13: Lógica de filas más limpia)
            bool par = false;
            foreach (var prod in datos.Detalles)
            {
                BaseColor bg = par ? _colorFilaAlterna : BaseColor.WHITE;
                tabla.AddCell(CrearCelda(prod.Producto, bg));
                tabla.AddCell(CrearCelda(prod.Descripcion, bg));
                tabla.AddCell(CrearCelda(prod.Cantidad.ToString(), bg));
                tabla.AddCell(CrearCelda(prod.PrecioUnitario.ToString("N2"), bg));
                tabla.AddCell(CrearCelda(prod.Subtotal.ToString("N2"), bg));
                par = !par;
            }

            doc.Add(tabla);
        }

        private PdfPCell CrearCelda(string texto, BaseColor bg)
        {
            return new PdfPCell(new Phrase(texto, _fNormal))
            {
                BackgroundColor = bg,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
        }

        private void EscribirTotales(Document doc, clsFacturaConsultaModel datos)
        {
            doc.Add(new Paragraph(" "));
            doc.Add(new Paragraph($"Subtotal: L. {datos.Subtotal:N2}", _fNormal));
            doc.Add(new Paragraph($"Impuesto (15%): L. {datos.Impuesto:N2}", _fNormal));
            doc.Add(new Paragraph($"TOTAL A PAGAR: L. {datos.Total:N2}", _fNegrita));
        }
    }
}