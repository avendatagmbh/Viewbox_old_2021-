using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ViewboxArchiveConverter
{
    internal class FileConvertOptions {
        
        public DirectoryInfo ArchiveDirectory { get; set; }

        public DirectoryInfo ThumbnailDirectory { get; set; }
        public DirectoryInfo PdfDirectory { get; set; }
        public string CurrentDirectory { get; set; }
        
        public string ArchiveFileName { get; set; }
        public string CurrentThumbnailDirectory { get; set; }
        public string CurrentPdfDirectory { get; set; }

        public string ConvertFrom { get; set; }
        public string ConvertTo { get; set; }
        public bool IgnoreFormats { get; set; }

        private bool continueProcessing = true;
        public bool ContinueProcessing { get { return continueProcessing; } set { continueProcessing = value; } }

        private int tumbnailHeight = 491;
        public int TumbnailHeight { get { return tumbnailHeight; } set { tumbnailHeight = value; } }

        private int tumbnailWidth = 347;
        public int TumbnailWidth { get { return tumbnailWidth; } set { tumbnailWidth = value; } }
    }

    internal interface IFileConverter {
        bool Convert(FileConvertOptions fileConvertOptions);
    }
}
