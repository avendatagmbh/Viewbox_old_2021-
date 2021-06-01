using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ViewboxArchiveConverter
{
    internal class DocToThumbnailConverter : GeneralConverter, IFileConverter
    {
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private TextToThumbnailConverter _textToThumbnailConverter = null;
        public TextToThumbnailConverter TextToThumbnailConverter {
            get {
                if (_textToThumbnailConverter == null) {
                    semaphore.Wait();
                    try {
                        if (_textToThumbnailConverter == null) {
                            _textToThumbnailConverter = new TextToThumbnailConverter();
                        }
                    } finally {
                        semaphore.Release();
                    }
                }
                return _textToThumbnailConverter;
            }
        }

        public bool Convert(FileConvertOptions fileConvertOptions)
        {
            try
            {
                String newFileName = GetThumbnailFullName(fileConvertOptions);
                if (fileConvertOptions.ContinueProcessing)
                {
                    if (File.Exists(newFileName)) return true;
                }

                Microsoft.Office.Interop.Word.Application word = new Microsoft.Office.Interop.Word.Application();
                try
                {
                    Microsoft.Office.Interop.Word.Document doc = word.Documents.Open(Path.Combine(fileConvertOptions.ArchiveDirectory.FullName, fileConvertOptions.ArchiveFileName));
                    //doc.Content.CopyAsPicture();

                    //if (Clipboard.ContainsData(System.Windows.DataFormats.Rtf))
                    //{
                    //    string content = Clipboard.GetData(System.Windows.DataFormats.Rtf).ToString();
                    //    using (Image img = TextToThumbnailConverter.ConvertTextToImage(fileConvertOptions, content)) {
                    //        img.Save(newFileName, ImageFormat.Jpeg);
                    //    }
                    //}
                    string content = doc.Content.FormattedText.Text;
                    using (Image img = TextToThumbnailConverter.ConvertTextToImage(fileConvertOptions, content)) {
                        img.Save(newFileName, ImageFormat.Jpeg);
                    }
                    object objFalse = false;
                    doc.Close(objFalse, Type.Missing, Type.Missing);
                }
                finally
                {
                    word.Quit();
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
