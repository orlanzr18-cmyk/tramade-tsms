using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TRAMADE.Repositories;
using ZXing;
using ZXing.Common;

namespace TRAMADE.ClasesInventario
{
    /// <summary>
    /// Se mantiene exactamente igual en su rol: generación/impresión de
    /// etiquetas de código de barras (Bitmap + PDF). Es una utilidad de
    /// IMPRESIÓN/REPORTES, no DAL ni regla de negocio, por eso NO se movió
    /// a Repositories/Services.
    ///
    /// ÚNICO CAMBIO: las dos partes que antes ejecutaban SQL directo
    /// (guardar el código en BD y consultar los productos con código para
    /// el export masivo) ahora delegan en <see cref="ICodigoBarrasRepository"/>,
    /// para que toda la capa de datos quede centralizada en Repositories,
    /// tal como pediste. Las firmas públicas no cambiaron, así que cualquier
    /// código existente que ya llame a esta clase sigue funcionando igual.
    /// </summary>
    internal class clsCodigoBarras
    {
        private static readonly ICodigoBarrasRepository _repo = new CodigoBarrasRepository();

        // ══════════════════════════════════════════════════════════════
        // 1. GENERAR CÓDIGO DE BARRAS COMO BITMAP (sin cambios)
        // ══════════════════════════════════════════════════════════════
        public static Bitmap GenerarImagen(string codigo, int ancho = 350, int alto = 100)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Width = ancho,
                    Height = alto,
                    Margin = 8,
                    PureBarcode = false
                }
            };

            var pixelData = writer.Write(codigo);
            var bmp = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb);

            var bmpData = bmp.LockBits(
                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppRgb);

            System.Runtime.InteropServices.Marshal.Copy(
                pixelData.Pixels, 0, bmpData.Scan0, pixelData.Pixels.Length);

            bmp.UnlockBits(bmpData);
            return bmp;
        }

        // ══════════════════════════════════════════════════════════════
        // 2. GENERAR CÓDIGO ÚNICO  PRD-00012
        //    (La MISMA regla ahora también vive en Services.CodigoBarrasService
        //    para que ProductoService la use al registrar un producto nuevo.
        //    Se deja aquí también para no romper otras llamadas existentes.)
        // ══════════════════════════════════════════════════════════════
        public static string GenerarCodigo(int idProducto) => $"PRD-{idProducto:D5}";

        // ══════════════════════════════════════════════════════════════
        // 3. GUARDAR CÓDIGO EN BD — ahora delega en el Repository
        // ══════════════════════════════════════════════════════════════
        public static void GuardarEnBD(int idProducto, string codigo, clsConexion conexion)
        {
            _repo.GuardarCodigo(conexion.SqlC, idProducto, codigo);
        }

        // ══════════════════════════════════════════════════════════════
        // 4. IMPRIMIR ETIQUETA INDIVIDUAL (sin cambios: no toca BD)
        // ══════════════════════════════════════════════════════════════
        public static void ImprimirEtiqueta(string nombre, string codigo, decimal precio)
        {
            string rutaPdf = Path.Combine(Path.GetTempPath(),
                $"Etiqueta_{codigo}_{DateTime.Now:yyyyMMddHHmmss}.pdf");

            float pgW = 300f, pgH = 160f;
            float lblW = pgW - 20f, lblH = pgH - 20f;
            float ex = 10f, ey = 10f;

            float headerH = 22f;
            float footerH = 22f;
            float codeTextH = 12f;
            float barcodeH = lblH - headerH - footerH - codeTextH - 10f;
            float barcodePad = 10f;

            var colorHeader = new BaseColor(93, 58, 26);
            var colorPrecio = new BaseColor(46, 125, 50);
            var colorBorde = new BaseColor(187, 187, 187);
            var colorCodigo = new BaseColor(68, 68, 68);

            var fntNombre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8.5f, BaseColor.WHITE);
            var fntCodigo = FontFactory.GetFont(FontFactory.COURIER_BOLD, 7.5f, colorCodigo);
            var fntPrecio = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f, colorPrecio);

            using (var fs = new FileStream(rutaPdf, FileMode.Create))
            using (var doc = new Document(new iTextSharp.text.Rectangle(pgW, pgH), 0, 0, 0, 0))
            {
                var pw2 = PdfWriter.GetInstance(doc, fs);
                doc.Open();
                PdfContentByte cb2 = pw2.DirectContent;

                cb2.SetColorStroke(colorBorde);
                cb2.SetLineWidth(0.8f);
                cb2.RoundRectangle(ex, ey, lblW, lblH, 5f);
                cb2.Stroke();

                cb2.SetColorFill(BaseColor.WHITE);
                cb2.RoundRectangle(ex, ey, lblW, lblH, 5f);
                cb2.Fill();

                float hdrY2 = ey + lblH - headerH;
                cb2.SetColorFill(colorHeader);
                cb2.RoundRectangle(ex, hdrY2, lblW, headerH, 5f);
                cb2.Fill();
                cb2.Rectangle(ex, hdrY2, lblW, headerH / 2f);
                cb2.Fill();

                string nombreCorto = nombre.Length > 32 ? nombre.Substring(0, 30) + "…" : nombre;
                DrawCenteredText(cb2, fntNombre, nombreCorto, ex, hdrY2 + 7f, lblW);

                using (Bitmap bmp2 = GenerarImagen(codigo,
                           ancho: (int)(lblW - barcodePad * 2) * 4,
                           alto: (int)barcodeH * 4))
                using (var ms2 = new MemoryStream())
                {
                    bmp2.Save(ms2, ImageFormat.Png);
                    ms2.Position = 0;
                    var itxImg2 = iTextSharp.text.Image.GetInstance(ms2.ToArray());
                    itxImg2.ScaleAbsolute(lblW - barcodePad * 2, barcodeH);
                    itxImg2.SetAbsolutePosition(ex + barcodePad, ey + footerH + codeTextH + 2f);
                    cb2.AddImage(itxImg2);
                }

                DrawCenteredText(cb2, fntCodigo, codigo, ex, ey + footerH + 2f, lblW);
                DrawCenteredText(cb2, fntPrecio, $"L. {precio:N2}", ex, ey + 6f, lblW);

                doc.Close();
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = rutaPdf,
                UseShellExecute = true
            });
        }

        // ══════════════════════════════════════════════════════════════
        // 5. EXPORTAR TODAS LAS ETIQUETAS A PDF — la consulta ahora
        //    viene del Repository; el layout/dibujo del PDF no cambia.
        // ══════════════════════════════════════════════════════════════
        public static void ExportarTodasEtiquetas() => ExportarTodasEtiquetas(new clsConexion());

        public static void ExportarTodasEtiquetas(clsConexion conexion)
        {
            using (var dlg = new SaveFileDialog
            {
                Title = "Guardar etiquetas PDF",
                Filter = "PDF (*.pdf)|*.pdf",
                FileName = $"Etiquetas_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
                DefaultExt = "pdf"
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;

                string rutaPdf = dlg.FileName;

                // Antes: SELECT embebido aquí mismo abriendo "conexion".
                // Ahora: la consulta vive en CodigoBarrasRepository.
                var productos = _repo.ObtenerProductosConCodigoBarras();

                if (productos.Count == 0)
                {
                    clsMensajes.Aviso("No hay productos con código de barras registrado.");
                    return;
                }

                float pgW = 612f, pgH = 792f;
                int cols = 2, rows = 4;
                float marginX = 18f, marginY = 18f;
                float gapX = 10f, gapY = 8f;

                float lblW = (pgW - 2 * marginX - gapX) / cols;
                float lblH = (pgH - 2 * marginY - gapY * (rows - 1)) / rows;

                float headerH = 22f;
                float footerH = 22f;
                float codeTextH = 12f;
                float barcodeH = lblH - headerH - footerH - codeTextH - 10f;
                float barcodePad = 10f;

                var colorHeader = new BaseColor(93, 58, 26);
                var colorPrecio = new BaseColor(46, 125, 50);
                var colorBorde = new BaseColor(187, 187, 187);
                var colorCodigo = new BaseColor(68, 68, 68);

                var fntNombre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8.5f, BaseColor.WHITE);
                var fntCodigo = FontFactory.GetFont(FontFactory.COURIER_BOLD, 7.5f, colorCodigo);
                var fntPrecio = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12f, colorPrecio);

                using (var fs = new FileStream(rutaPdf, FileMode.Create))
                using (var doc = new Document(new iTextSharp.text.Rectangle(pgW, pgH), 0, 0, 0, 0))
                {
                    var writer2 = PdfWriter.GetInstance(doc, fs);
                    doc.Open();
                    PdfContentByte cb = writer2.DirectContent;

                    int totalPaginas = (int)Math.Ceiling(productos.Count / (double)(cols * rows));
                    int idx = 0;

                    for (int pag = 0; pag < totalPaginas; pag++)
                    {
                        if (pag > 0) doc.NewPage();

                        for (int r = 0; r < rows && idx < productos.Count; r++)
                            for (int col = 0; col < cols && idx < productos.Count; col++, idx++)
                            {
                                var (codigo, nombre, precio) = productos[idx];

                                float x = marginX + col * (lblW + gapX);
                                float y = pgH - marginY - (r + 1) * lblH - r * gapY;

                                cb.SetColorStroke(colorBorde);
                                cb.SetLineWidth(0.8f);
                                RoundRect(cb, x, y, lblW, lblH, 5f, stroke: true, fill: false, fillColor: null);

                                cb.SetColorFill(BaseColor.WHITE);
                                RoundRect(cb, x, y, lblW, lblH, 5f, stroke: false, fill: true, fillColor: BaseColor.WHITE);

                                float hdrY = y + lblH - headerH;
                                cb.SetColorFill(colorHeader);
                                RoundRect(cb, x, hdrY, lblW, headerH, 5f, stroke: false, fill: true, fillColor: colorHeader);
                                cb.Rectangle(x, hdrY, lblW, headerH / 2f);
                                cb.Fill();

                                string nombreCorto = nombre.Length > 32 ? nombre.Substring(0, 30) + "…" : nombre;
                                DrawCenteredText(cb, fntNombre, nombreCorto, x, hdrY + 7f, lblW);

                                using (Bitmap bmp = GenerarImagen(codigo,
                                           ancho: (int)(lblW - barcodePad * 2) * 4,
                                           alto: (int)barcodeH * 4))
                                {
                                    float bcX = x + barcodePad;
                                    float bcY = y + footerH + codeTextH + 2f;
                                    float bcW = lblW - barcodePad * 2;

                                    using (var ms = new MemoryStream())
                                    {
                                        bmp.Save(ms, ImageFormat.Png);
                                        ms.Position = 0;
                                        var itxImg = iTextSharp.text.Image.GetInstance(ms.ToArray());
                                        itxImg.ScaleAbsolute(bcW, barcodeH);
                                        itxImg.SetAbsolutePosition(bcX, bcY);
                                        cb.AddImage(itxImg);
                                    }
                                }

                                DrawCenteredText(cb, fntCodigo, codigo, x, y + footerH + 2f, lblW);
                                DrawCenteredText(cb, fntPrecio, $"L. {precio:N2}", x, y + 6f, lblW);
                            }
                    }

                    doc.Close();
                }

                clsMensajes.Exito(
                    $"PDF generado correctamente.\n{productos.Count} etiquetas en {(int)Math.Ceiling(productos.Count / (double)(cols * rows))} página(s).");

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = rutaPdf,
                    UseShellExecute = true
                });
            }
        }

        // ═══════════════════════════════
        // HELPERS INTERNOS (sin cambios)
        // ═══════════════════════════════
        private static void RoundRect(PdfContentByte cb,
            float x, float y, float w, float h, float r,
            bool stroke, bool fill, BaseColor fillColor)
        {
            if (fill && fillColor != null) cb.SetColorFill(fillColor);
            cb.RoundRectangle(x, y, w, h, r);
            if (stroke && fill) cb.FillStroke();
            else if (fill) cb.Fill();
            else if (stroke) cb.Stroke();
        }

        private static void DrawCenteredText(PdfContentByte cb,
            iTextSharp.text.Font font, string text, float lblX, float y, float lblW)
        {
            cb.BeginText();
            cb.SetFontAndSize(font.BaseFont, font.Size);
            cb.SetColorFill(font.Color ?? BaseColor.BLACK);
            float textW = font.BaseFont.GetWidthPoint(text, font.Size);
            float x = lblX + (lblW - textW) / 2f;
            cb.SetTextMatrix(x, y);
            cb.ShowText(text);
            cb.EndText();
        }
    }
}
