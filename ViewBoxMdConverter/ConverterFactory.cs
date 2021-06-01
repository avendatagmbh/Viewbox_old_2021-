using System.Threading;

namespace ViewboxMdConverter
{
	internal class ConverterFactory
	{
		private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		private static ConverterFactory _instance;

		private DocToPdfConverter _docToPdfConverter;

		private DocToThumbnailConverter _docToThumbnailConverter;

		private HtmlToPdfConverter _htmlToPdfConverter;

		private HtmlToThumbnailConverter _htmlToThumbnailConverter;

		private ImageToPdfConverter _imageToPdfConverterInstance;

		private PcxToPdfConverter _pcxToPdfConverterInstance;

		private PcxToThumbnailConverter _pcxToThumbnailConverterInstance;

		private PdfToPdfConverter _pdfToPdfConverterInstance;

		private PdfToThumbnailConverter _pdfToThumbnailConverterInstance;

		private PptToPdfConverter _pptToPdfConverter;

		private PptToThumbnailConverter _pptToThumbnailConverter;

		private PsToPdfConverter _psToPdfConverterInstance;

		private TextToPdfConverter _textToPdfConverter;

		private TextToThumbnailConverter _textToThumbnailConverter;

		private TiffToPdfConverter _tiffToPdfConverterInstance;

		private TiffToThumbnailConverter _tiffToThumbnailConverterInstance;

		private XlsToPdfConverter _xlsToPdfConverter;

		private XlsToThumbnailConverter _xlsToThumbnailConverter;

		public static ConverterFactory Instance
		{
			get
			{
				if (_instance == null)
				{
					semaphore.Wait();
					try
					{
						if (_instance == null)
						{
							_instance = new ConverterFactory();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _instance;
			}
		}

		public TextToThumbnailConverter TextToThumbnailConverter
		{
			get
			{
				if (_textToThumbnailConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_textToThumbnailConverter == null)
						{
							_textToThumbnailConverter = new TextToThumbnailConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _textToThumbnailConverter;
			}
		}

		public TextToPdfConverter TextToPdfConverter
		{
			get
			{
				if (_textToPdfConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_textToPdfConverter == null)
						{
							_textToPdfConverter = new TextToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _textToPdfConverter;
			}
		}

		public TiffToThumbnailConverter TiffToThumbnailConverterInstance
		{
			get
			{
				if (_tiffToThumbnailConverterInstance == null)
				{
					semaphore.Wait();
					try
					{
						if (_tiffToThumbnailConverterInstance == null)
						{
							_tiffToThumbnailConverterInstance = new TiffToThumbnailConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _tiffToThumbnailConverterInstance;
			}
		}

		public PdfToThumbnailConverter PdfToThumbnailConverterInstance
		{
			get
			{
				if (_pdfToThumbnailConverterInstance == null)
				{
					semaphore.Wait();
					try
					{
						if (_pdfToThumbnailConverterInstance == null)
						{
							_pdfToThumbnailConverterInstance = new PdfToThumbnailConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _pdfToThumbnailConverterInstance;
			}
		}

		public PcxToThumbnailConverter PcxToThumbnailConverterInstance
		{
			get
			{
				if (_pcxToThumbnailConverterInstance == null)
				{
					semaphore.Wait();
					try
					{
						if (_pcxToThumbnailConverterInstance == null)
						{
							_pcxToThumbnailConverterInstance = new PcxToThumbnailConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _pcxToThumbnailConverterInstance;
			}
		}

		public HtmlToThumbnailConverter HtmlToThumbnailConverterInstance
		{
			get
			{
				if (_htmlToThumbnailConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_htmlToThumbnailConverter == null)
						{
							_htmlToThumbnailConverter = new HtmlToThumbnailConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _htmlToThumbnailConverter;
			}
		}

		public DocToThumbnailConverter DocToThumbnailConverterInstance
		{
			get
			{
				if (_docToThumbnailConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_docToThumbnailConverter == null)
						{
							_docToThumbnailConverter = new DocToThumbnailConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _docToThumbnailConverter;
			}
		}

		public XlsToThumbnailConverter XlsToThumbnailConverterInstance
		{
			get
			{
				if (_xlsToThumbnailConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_xlsToThumbnailConverter == null)
						{
							_xlsToThumbnailConverter = new XlsToThumbnailConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _xlsToThumbnailConverter;
			}
		}

		public PptToThumbnailConverter PptToThumbnailConverterInstance
		{
			get
			{
				if (_pptToThumbnailConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_pptToThumbnailConverter == null)
						{
							_pptToThumbnailConverter = new PptToThumbnailConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _pptToThumbnailConverter;
			}
		}

		public HtmlToPdfConverter HtmlToPdfConverterInstance
		{
			get
			{
				if (_htmlToPdfConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_htmlToPdfConverter == null)
						{
							_htmlToPdfConverter = new HtmlToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _htmlToPdfConverter;
			}
		}

		public TiffToPdfConverter TiffToPdfConverterInstance
		{
			get
			{
				if (_tiffToPdfConverterInstance == null)
				{
					semaphore.Wait();
					try
					{
						if (_tiffToPdfConverterInstance == null)
						{
							_tiffToPdfConverterInstance = new TiffToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _tiffToPdfConverterInstance;
			}
		}

		public ImageToPdfConverter ImageToPdfConverterInstance
		{
			get
			{
				if (_imageToPdfConverterInstance == null)
				{
					semaphore.Wait();
					try
					{
						if (_imageToPdfConverterInstance == null)
						{
							_imageToPdfConverterInstance = new ImageToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _imageToPdfConverterInstance;
			}
		}

		public PsToPdfConverter PsToPdfConverterInstance
		{
			get
			{
				if (_psToPdfConverterInstance == null)
				{
					semaphore.Wait();
					try
					{
						if (_psToPdfConverterInstance == null)
						{
							_psToPdfConverterInstance = new PsToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _psToPdfConverterInstance;
			}
		}

		public PdfToPdfConverter PdfToPdfConverterInstance
		{
			get
			{
				if (_pdfToPdfConverterInstance == null)
				{
					semaphore.Wait();
					try
					{
						if (_pdfToPdfConverterInstance == null)
						{
							_pdfToPdfConverterInstance = new PdfToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _pdfToPdfConverterInstance;
			}
		}

		public PcxToPdfConverter PcxToPdfConverterInstance
		{
			get
			{
				if (_pcxToPdfConverterInstance == null)
				{
					semaphore.Wait();
					try
					{
						if (_pcxToPdfConverterInstance == null)
						{
							_pcxToPdfConverterInstance = new PcxToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _pcxToPdfConverterInstance;
			}
		}

		public DocToPdfConverter DocToPdfConverterInstance
		{
			get
			{
				if (_docToPdfConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_docToPdfConverter == null)
						{
							_docToPdfConverter = new DocToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _docToPdfConverter;
			}
		}

		public XlsToPdfConverter XlsToPdfConverterInstance
		{
			get
			{
				if (_xlsToPdfConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_xlsToPdfConverter == null)
						{
							_xlsToPdfConverter = new XlsToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _xlsToPdfConverter;
			}
		}

		public PptToPdfConverter PptToPdfConverterInstance
		{
			get
			{
				if (_pptToPdfConverter == null)
				{
					semaphore.Wait();
					try
					{
						if (_pptToPdfConverter == null)
						{
							_pptToPdfConverter = new PptToPdfConverter();
						}
					}
					finally
					{
						semaphore.Release();
					}
				}
				return _pptToPdfConverter;
			}
		}

		private ConverterFactory()
		{
		}

		public IFileConverter GetMdToImageExtension(string exStr)
		{
			switch (exStr)
			{
			case ".html":
				return HtmlToThumbnailConverterInstance;
			case ".rtf":
			case ".doc":
			case ".xls":
			case ".ppt":
			case ".zip":
				return null;
			case ".ps":
			case ".pdf":
				return PdfToThumbnailConverterInstance;
			case ".pcx":
				return PcxToThumbnailConverterInstance;
			case ".bmp":
			case ".gif":
			case ".tiff":
			case ".tif":
			case ".fax":
			case ".jpg":
				return TiffToThumbnailConverterInstance;
			default:
				return TextToThumbnailConverter;
			}
		}

		public IFileConverter GetMdToImageExtension2(string exStr)
		{
			switch (exStr.ToLower())
			{
			case ".html":
			case ".rtf":
			case ".doc":
			case ".xls":
			case ".ppt":
			case "application/msword":
				return null;
			case ".ps":
			case ".pdf":
			case "application/pdf":
				return PdfToThumbnailConverterInstance;
			case ".pcx":
				return PcxToThumbnailConverterInstance;
			case ".bmp":
			case ".gif":
			case ".tiff":
			case "image/tiff":
			case ".tif":
			case ".fax":
			case ".jpg":
				return TiffToThumbnailConverterInstance;
			case "text/plain":
				return TextToThumbnailConverter;
			default:
				return TextToThumbnailConverter;
			}
		}

		public IFileConverter GetMdToPdfExtension(string exStr)
		{
			return exStr.ToLower() switch
			{
				"text/plain" => TextToPdfConverter, 
				"image/tiff" => TiffToPdfConverterInstance, 
				"application/pdf" => PdfToPdfConverterInstance, 
				"application/msword" => DocToPdfConverterInstance, 
				"application/vnd.ser.ci" => TextToPdfConverter, 
				_ => TextToPdfConverter, 
			};
		}
	}
}
