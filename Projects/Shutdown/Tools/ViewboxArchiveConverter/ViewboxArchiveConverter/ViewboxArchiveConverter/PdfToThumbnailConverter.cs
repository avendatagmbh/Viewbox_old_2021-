using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ViewboxArchiveConverter
{
    internal class PdfToThumbnailConverter : GeneralConverter, IFileConverter
    {
        public bool Convert(FileConvertOptions fileConvertOptions)
        {
            try {
                String newFileName = GetThumbnailFullName(fileConvertOptions);
                if (fileConvertOptions.ContinueProcessing)
                {
                    if (File.Exists(newFileName)) return true;
                }

                using (ShellThumbnail thumb = new ShellThumbnail()) {
                    thumb.DesiredSize = new Size(fileConvertOptions.TumbnailWidth, fileConvertOptions.TumbnailHeight);
                    Bitmap bmp = thumb.GetThumbnail(Path.Combine(fileConvertOptions.ArchiveDirectory.FullName, fileConvertOptions.ArchiveFileName));

                    EncoderParameters encoderParameters = new EncoderParameters(1);
                    long quality = 100;
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                    bmp.Save(newFileName, GetEncoder(ImageFormat.Jpeg), encoderParameters);
                
                    return true;
                }
            }
            catch (Exception ex) {
                log.Error(ex.Message, ex);
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            return codecs.Single(codec => codec.FormatID == format.Guid);
        }
    }
}
