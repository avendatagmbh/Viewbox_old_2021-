using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;

namespace SerialNumberPrinter {

    public static class Print {
        
        private static float mm { get { return PageSize.A4.Rotate().Width / 210; } }
        private static Font font = new Font(Font.FontFamily.HELVETICA, 15);
        private static Font font1 = new Font(Font.FontFamily.COURIER, 15);

        public static void PrintSerialNumber(string serialNumber) {

            // create new document
            Document PdfFile = new Document(PageSize.A4.Rotate(), 0, 0, 0, 0);

            string path = "Serials";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            try {
                FileStream fstream = new FileStream(path + "\\" + serialNumber + ".pdf", FileMode.Create, FileAccess.ReadWrite);
                PdfWriter writer = PdfWriter.GetInstance(PdfFile, fstream);

                PdfFile.Open();

                Paragraph p;
                Phrase ph;

                var cb = writer.DirectContent;
                var col = new ColumnText(cb);
                col.SetSimpleColumn(25.5f * mm, 0 * mm, 300 * mm, 18.0f * mm);

                p = new Paragraph();
                p.Add(new Chunk("Seriennummer: ", font));
                p.Add(new Chunk(serialNumber, font1));
                col.AddText(p);
                col.Go();

                PdfFile.Close();

            } catch (Exception ex) {
                PdfFile.Close();
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

    }
}
