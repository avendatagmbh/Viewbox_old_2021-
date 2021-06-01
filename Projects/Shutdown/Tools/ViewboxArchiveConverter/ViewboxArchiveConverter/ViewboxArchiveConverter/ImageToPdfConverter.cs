using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ViewboxArchiveConverter
{
    internal class ImageToPdfConverter : GeneralConverter, IFileConverter
    {
        public bool Convert(FileConvertOptions fileConvertOptions)
        {
            String newFileName = GetPdfFullName(fileConvertOptions);
            if (fileConvertOptions.ContinueProcessing)
            {
                if (File.Exists(newFileName)) return true;
            }

            try {
                Document document = new Document();
                using (FileStream stream = new FileStream(newFileName, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    PdfWriter.GetInstance(document, stream);
                    document.Open();
                    using (FileStream imageStream = new FileStream(Path.Combine(fileConvertOptions.ArchiveDirectory.FullName, fileConvertOptions.ArchiveFileName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        //Image image = Image.GetInstance(imageStream);
                        //document.Add(image);
                        Image image = Image.GetInstance(Image.GetInstance(imageStream));
                        image.SetAbsolutePosition(0, 0);
                        image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
                        document.Add(image);
                    }
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
    }
}
