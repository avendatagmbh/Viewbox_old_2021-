using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ViewboxArchiveConverter
{
    internal class TextToPdfConverter : GeneralConverter, IFileConverter
    {
        public bool Convert(FileConvertOptions fileConvertOptions)
        {
            try
            {
                String newFileName = GetPdfFullName(fileConvertOptions);

                if (fileConvertOptions.ContinueProcessing)
                {
                    if (File.Exists(newFileName)) return true;
                }

                string[] text = null;

                string archiveFileName = fileConvertOptions.ArchiveFileName;
                if (fileConvertOptions.ArchiveFileName.Contains(".md"))
                    archiveFileName = fileConvertOptions.ArchiveFileName.Replace(".md", "");

                string oldFileName = Path.Combine(fileConvertOptions.CurrentDirectory, archiveFileName);

                var document = new Document();
                using (var stream = File.OpenWrite(newFileName))
                {
                    var writer = PdfWriter.GetInstance(document, stream);
                    document.Open();
                    File.ReadAllLines(oldFileName)
                        .ToList()
                        .ForEach(line => document.Add(new Paragraph(line)));
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
