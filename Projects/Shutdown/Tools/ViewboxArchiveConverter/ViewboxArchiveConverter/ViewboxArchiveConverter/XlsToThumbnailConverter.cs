using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using Excel = Microsoft.Office.Interop.Excel;

namespace ViewboxArchiveConverter
{
    internal class XlsToThumbnailConverter : GeneralConverter, IFileConverter
    {
        public bool Convert(FileConvertOptions fileConvertOptions)
        {
            try
            {
                String newFileName = GetThumbnailFullName(fileConvertOptions);
                if (fileConvertOptions.ContinueProcessing)
                {
                    if (File.Exists(newFileName)) return true;
                }
                
                Excel.Application excel = new Excel.Application();
                try
                {
                    Excel.Workbook wkb = excel.Workbooks.Open(Path.Combine(fileConvertOptions.ArchiveDirectory.FullName, fileConvertOptions.ArchiveFileName));
                    Excel.Worksheet sheet = wkb.Worksheets[1] as Excel.Worksheet;
                    //Excel.Range range = sheet.Cells[1, 10] as Excel.Range;
                    // copy as seen when printed
                    //range.CopyPicture(Excel.XlPictureAppearance.xlPrinter, Excel.XlCopyPictureFormat.xlPicture);

                    //// uncomment to copy as seen on screen
                    //range.CopyPicture(Excel.XlPictureAppearance.xlScreen, Excel.XlCopyPictureFormat.xlBitmap);

                    //sheet.Range["A1","J10"].CopyPicture(Excel.XlPictureAppearance.xlScreen, Excel.XlCopyPictureFormat.xlBitmap);
                    sheet.Range["A1", "C30"].CopyPicture(Excel.XlPictureAppearance.xlScreen, Excel.XlCopyPictureFormat.xlBitmap);

                    /*if (Clipboard.ContainsData(System.Windows.DataFormats.EnhancedMetafile))
                    {
                        using (FileStream fileStream = new FileStream(newFileName, FileMode.Create)) {
                            Metafile metafile = Clipboard.GetData(System.Windows.DataFormats.EnhancedMetafile) as Metafile;
                            metafile.Save(newFileName);
                        }
                    }
                    else */if (Clipboard.ContainsData(System.Windows.DataFormats.Bitmap))
                    {
                        BitmapSource bitmapSource = Clipboard.GetData(System.Windows.DataFormats.Bitmap) as BitmapSource;
                        if (bitmapSource != null) {
                            //the files we have have to be resized to work properly
                            TransformedBitmap tbBitmap = new TransformedBitmap(bitmapSource, new ScaleTransform(fileConvertOptions.TumbnailWidth / bitmapSource.PixelWidth, fileConvertOptions.TumbnailHeight / bitmapSource.PixelHeight, 0, 0));
                            using (FileStream fileStream = new FileStream(newFileName, FileMode.Create)) {
                                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(tbBitmap));
                                encoder.QualityLevel = 100;
                                encoder.Save(fileStream);
                            }
                        }
                        else {
                            Image img = Clipboard.GetImage();
                            if (img != null) {
                                //the files we have have to be resized to work properly
                                img = img.GetThumbnailImage(fileConvertOptions.TumbnailWidth, fileConvertOptions.TumbnailHeight, ImageConverter.ThumnailDelegate, IntPtr.Zero);
                                img.Save(newFileName, ImageFormat.Jpeg);
                            }
                        }
                    }
                    object objFalse = false;
                    wkb.Close(objFalse, Type.Missing, Type.Missing);
                }
                finally {
                    excel.Quit();
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
