using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using AV.Log;
using log4net;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ViewboxArchiveConverter
{
    internal class FaxToPdfConverter : GeneralConverter, IFileConverter
    {
        public bool Convert(FileConvertOptions fileConvertOptions)
        {
            String newFileName = GetPdfFullName(fileConvertOptions);
            if (fileConvertOptions.ContinueProcessing)
            {
                if (File.Exists(newFileName)) return true;
            }

            try {
                string archiveFileName = fileConvertOptions.ArchiveFileName;
                if (fileConvertOptions.ArchiveFileName.Contains(".md"))
                    archiveFileName = fileConvertOptions.ArchiveFileName.Replace(".md", "");

                var document = new Document();
                string oldFileName = Path.Combine(fileConvertOptions.CurrentDirectory, archiveFileName);

                using (var stream = File.OpenWrite(newFileName))
                {
                    var writer = PdfWriter.GetInstance(document, stream);
                    document.Open();

                    AddTiffToPdf(oldFileName, ref writer, ref document);
                    document.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private void AddTiffToPdf(string tiffFileName, ref PdfWriter writer, ref Document document)
        {
            Bitmap bitmap = new Bitmap(tiffFileName);
            int numberOfPages = bitmap.GetFrameCount(FrameDimension.Page);
            PdfContentByte cb = writer.DirectContent;
            for (int page = 0; page < numberOfPages; page++)
            {
                bitmap.SelectActiveFrame(FrameDimension.Page, page);
                MemoryStream stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Png);
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(stream.ToArray());
                stream.Close();
                img.ScalePercent(72f / bitmap.HorizontalResolution * 100);
                img.SetAbsolutePosition(0, 0);
                cb.AddImage(img);
                document.NewPage();
            }
        }
    }
}
