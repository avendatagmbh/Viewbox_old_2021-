using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AV.Log;
using log4net;

namespace ViewboxArchiveConverter
{
    internal abstract class GeneralConverter
    {
        protected static ILog log = LogHelper.GetLogger();

        protected string GetThumbnailFullName(FileConvertOptions fileConvertOptions)
        {
            return GetFullName(fileConvertOptions, fileConvertOptions.CurrentThumbnailDirectory, fileConvertOptions.ThumbnailDirectory.FullName, 
                fileConvertOptions.IgnoreFormats ? ".jpg" : fileConvertOptions.ConvertTo);
        }

        protected string GetPdfFullName(FileConvertOptions fileConvertOptions)
        {
            return GetFullName(fileConvertOptions, fileConvertOptions.CurrentPdfDirectory, fileConvertOptions.PdfDirectory.FullName,
                fileConvertOptions.IgnoreFormats ? ".pdf" : fileConvertOptions.ConvertTo);
        }

        private string GetFullName(FileConvertOptions fileConvertOptions, string currentDirectory, string directoryFullName, string convertTo)
        {
            if (!String.IsNullOrEmpty(currentDirectory))
            {
                DirectoryInfo di = new DirectoryInfo(currentDirectory);
                if (!di.Exists)
                {
                    di.Create();
                }

                string path = Path.Combine(currentDirectory,
                                    fileConvertOptions.ArchiveFileName.ToLower().Replace(
                                        fileConvertOptions.ConvertFrom.ToLower(), convertTo));
                return path;
            }
            else
            {
                return Path.Combine(directoryFullName,
                                    fileConvertOptions.ArchiveFileName.ToLower().Replace(
                                        fileConvertOptions.ConvertFrom.ToLower(), convertTo));
            }
        }
    }
}
