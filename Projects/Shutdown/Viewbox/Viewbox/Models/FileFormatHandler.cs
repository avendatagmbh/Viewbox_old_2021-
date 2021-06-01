using System.Collections.Generic;

namespace Viewbox.Models
{
	public class FileFormatHandler
	{
		private static FileFormatHandler instance;

		private List<string> _allowedFileFormats;

		public static FileFormatHandler Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new FileFormatHandler();
				}
				return instance;
			}
		}

		public List<string> AllowedFileFormats => _allowedFileFormats;

		private FileFormatHandler()
		{
			_allowedFileFormats = new List<string>
			{
				".html", ".rtf", ".doc", ".xls", ".ppt", ".ps", ".pdf", ".pcx", ".bmp", ".gif",
				".tiff", ".tif", ".fax", ".jpg", ".xml", ".csv", ".zip"
			};
		}
	}
}
