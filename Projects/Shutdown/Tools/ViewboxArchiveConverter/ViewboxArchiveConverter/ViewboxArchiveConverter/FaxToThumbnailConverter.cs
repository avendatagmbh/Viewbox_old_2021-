using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using AV.Log;
using log4net;

namespace ViewboxArchiveConverter
{
    internal class FaxToThumbnailConverter : GeneralConverter, IFileConverter
    {
        public bool Convert(FileConvertOptions fileConvertOptions)
        {
            String newFileName = GetThumbnailFullName(fileConvertOptions);
            if (fileConvertOptions.ContinueProcessing)
            {
                if (File.Exists(newFileName)) return true;
            }

            string archiveFileName = fileConvertOptions.ArchiveFileName;
            if (fileConvertOptions.ArchiveFileName.Contains(".md"))
                archiveFileName = fileConvertOptions.ArchiveFileName.Replace(".md", "");

            using (Image jpeg = ImageConverter.ConvertToThumbnail(Path.Combine(fileConvertOptions.CurrentDirectory,
                                       archiveFileName), fileConvertOptions.TumbnailWidth, fileConvertOptions.TumbnailHeight))
            {
                jpeg.Save(newFileName, ImageFormat.Jpeg);
            }
            return true;
        }
    }
}
