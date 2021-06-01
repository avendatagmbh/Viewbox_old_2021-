using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ViewboxArchiveConverter
{
    internal class ConverterFactory {

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private SemaphoreSlim semaphoreConverter = new SemaphoreSlim(1);
        private static ConverterFactory _instance;

        public static ConverterFactory Instance {
            get {
                if (_instance == null) {
                    semaphore.Wait();
                    try {
                        if (_instance == null) {
                            _instance = new ConverterFactory();
                        }
                    } finally {
                        semaphore.Release();
                    }
                }
                return _instance;
            } 
        }

        private ConverterFactory() {
            
        }

        private ImageToPdfConverter _imageToPdfConverterInstance;
        private TextToPdfConverter _textToPdfConverter;
        private FaxToPdfConverter _faxToPdfConverterInstance;
        private PdfToThumbnailConverter _pdfConverterInstance;
        private FaxToThumbnailConverter _faxToThumbnailConverterInstance;
        private XlsToThumbnailConverter _xlsConverterInstance;
        private DocToThumbnailConverter _docConverterInstance;
        private TextToThumbnailConverter _textToThumbnailConverter;
        private ConverterList _mdConverter;

        private ImageToPdfConverter ImageToPdfConverterInstance
        {
            get
            {
                if (_imageToPdfConverterInstance == null)
                {
                    semaphoreConverter.Wait();
                    try
                    {
                        if (_imageToPdfConverterInstance == null)
                        {
                            _imageToPdfConverterInstance = new ImageToPdfConverter();
                        }
                    }
                    finally
                    {
                        semaphoreConverter.Release();
                    }
                }
                return _imageToPdfConverterInstance;
            }
        }

        public TextToPdfConverter TextToPdfConverter
        {
            get
            {
                if (_textToPdfConverter == null)
                {
                    semaphoreConverter.Wait();
                    try
                    {
                        if (_textToPdfConverter == null)
                        {
                            _textToPdfConverter = new TextToPdfConverter();
                        }
                    }
                    finally
                    {
                        semaphoreConverter.Release();
                    }
                }
                return _textToPdfConverter;
            }
        }

        private FaxToPdfConverter FaxToPdfConverterInstance
        {
            get
            {
                if (_faxToPdfConverterInstance == null)
                {
                    semaphoreConverter.Wait();
                    try
                    {
                        if (_faxToPdfConverterInstance == null)
                        {
                            _faxToPdfConverterInstance = new FaxToPdfConverter();
                        }
                    }
                    finally
                    {
                        semaphoreConverter.Release();
                    }
                }
                return _faxToPdfConverterInstance;
            }
        }

        private PdfToThumbnailConverter PdfConverterInstance {
            get {
                if (_pdfConverterInstance == null)
                {
                    semaphoreConverter.Wait();
                    try
                    {
                        if (_pdfConverterInstance == null)
                        {
                            _pdfConverterInstance = new PdfToThumbnailConverter();
                        }
                    }
                    finally
                    {
                        semaphoreConverter.Release();
                    }
                }
                return _pdfConverterInstance;
            }
        }

        private FaxToThumbnailConverter FaxToThumbnailConverterInstance
        {
            get
            {
                if (_faxToThumbnailConverterInstance == null)
                {
                    semaphoreConverter.Wait();
                    try
                    {
                        if (_faxToThumbnailConverterInstance == null)
                        {
                            _faxToThumbnailConverterInstance = new FaxToThumbnailConverter();
                        }
                    }
                    finally
                    {
                        semaphoreConverter.Release();
                    }
                }
                return _faxToThumbnailConverterInstance;
            }
        }

        private XlsToThumbnailConverter XlsConverterInstance
        {
            get
            {
                if (_xlsConverterInstance == null)
                {
                    semaphoreConverter.Wait();
                    try
                    {
                        if (_xlsConverterInstance == null)
                        {
                            _xlsConverterInstance = new XlsToThumbnailConverter();
                        }
                    }
                    finally
                    {
                        semaphoreConverter.Release();
                    }
                }
                return _xlsConverterInstance;
            }
        }

        private DocToThumbnailConverter DocConverterInstance
        {
            get
            {
                if (_docConverterInstance == null)
                {
                    semaphoreConverter.Wait();
                    try
                    {
                        if (_docConverterInstance == null)
                        {
                            _docConverterInstance = new DocToThumbnailConverter();
                        }
                    }
                    finally
                    {
                        semaphoreConverter.Release();
                    }
                }
                return _docConverterInstance;
            }
        }

        public TextToThumbnailConverter TextToThumbnailConverter
        {
            get
            {
                if (_textToThumbnailConverter == null)
                {
                    semaphoreConverter.Wait();
                    try
                    {
                        if (_textToThumbnailConverter == null)
                        {
                            _textToThumbnailConverter = new TextToThumbnailConverter();
                        }
                    }
                    finally
                    {
                        semaphoreConverter.Release();
                    }
                }
                return _textToThumbnailConverter;
            }
        }

        public ConverterList MdConverter
        {
            get
            {
                if (_mdConverter == null)
                {
                    semaphoreConverter.Wait();
                    try
                    {
                        if (_mdConverter == null)
                        {
                            _mdConverter = new ConverterList();
                        }
                    }
                    finally
                    {
                        semaphoreConverter.Release();
                    }
                }
                return _mdConverter;
            }
        }

        public IFileConverter GetConverterInstance(string archiveFileExtension, FileConvertOptions fileConverterOptions)
        {

            if (fileConverterOptions == null)
                throw new ArgumentNullException("fileConverterOptions");

            if (string.IsNullOrWhiteSpace(fileConverterOptions.ConvertFrom))
                throw new ArgumentException("Argument fileConverterOptions.ConvertFrom is null or empty", "fileConverterOptions");

            switch (archiveFileExtension.ToLower())
            {
                case ".pdf":
                    return PdfConverterInstance;
                case ".fax":
                    if (string.IsNullOrWhiteSpace(fileConverterOptions.ConvertTo))
                        throw new ArgumentException("Argument fileConverterOptions.ConvertTo is null or empty", "fileConverterOptions");

                    if (fileConverterOptions.ConvertTo.ToLower() == ".jpg")
                        return FaxToThumbnailConverterInstance;
                    else if (fileConverterOptions.ConvertTo.ToLower() == ".pdf")
                        return ImageToPdfConverterInstance;
                    else
                        throw new ArgumentException(string.Format("[{0}] cannto be converted to [{1}]", fileConverterOptions.ConvertFrom, fileConverterOptions.ConvertTo), "fileConverterOptions");
                case ".txt":
                case ".xml":
                    return TextToThumbnailConverter;
                case ".md":
                    return GetMdExtension(Path.Combine(fileConverterOptions.CurrentDirectory, fileConverterOptions.ArchiveFileName));
                case ".doc":
                case ".docx":
                    //return DocConverterInstance;
                case ".xls":
                case ".xlsx":
                    //return XlsConverterInstance;
                default:
                    throw new ArgumentException(string.Format("Argument [{0}] value is unknown [{1}]", "fileConverterOptions.ConvertFrom", fileConverterOptions.ConvertFrom), "fileConverterOptions");
            }
        }

        public IFileConverter GetMdExtension(string fileName) {            
            string exStr;
            using (StreamReader sr = new StreamReader(fileName))
            {
                string[] lines = sr.ReadToEnd().Split('\n');
                exStr = lines[1];
                sr.Close();
            }

            IFileConverter[] converters = new IFileConverter[2];

            switch (exStr) {
                case "CONTENT_TYPE=text/plain":
                    converters[0] = TextToThumbnailConverter;
                    converters[1] = TextToPdfConverter;
                    break;
                case "CONTENT_TYPE=image/tiff":
                    converters[0] = FaxToThumbnailConverterInstance;
                    converters[1] = FaxToPdfConverterInstance;
                    break;
                default:
                    return null;
            }

            return new ConverterList()
            {
                FileConverters = converters
            };
        }
    }
}
