using System.IO;
using System.Text;

namespace ViewboxMdConverter
{
	public class FileInputData
	{
		private string _locator;

		private string _relativeDirectoryLocation;

		private string _relativeFileLocation;

		public string Mandant { get; set; }

		public string ContentObjectUuidS { get; set; }

		public string DokumentenTyp { get; set; }

		public string CreationDate { get; set; }

		public string MimeType { get; set; }

		public string BelegNummer { get; set; }

		public string Buchungsdatum { get; set; }

		public string Locator
		{
			get
			{
				return _locator;
			}
			set
			{
				_locator = value;
				new StringBuilder();
				try
				{
					FileInfo fi = new FileInfo(_locator);
					_relativeDirectoryLocation = fi.Directory.FullName;
					_relativeFileLocation = fi.FullName;
				}
				catch
				{
					throw new FileException("Locator string isn't suitable.");
				}
			}
		}

		public string RelativeFileLocation => _relativeFileLocation;

		public string RelativeDirectoryLocation => _relativeDirectoryLocation;

		public string Target { get; set; }

		public string TargetTiffFile => Path.Combine(Target, "JPGs", $"{RelativeFileLocation}.jpg");

		public string TargetPdfFile => Path.Combine(Target, "PDFs", $"{RelativeFileLocation}.pdf");

		public FileInputData(string input)
		{
			string[] inputs = input.Split(';');
			try
			{
				Mandant = inputs[0];
				ContentObjectUuidS = inputs[1];
				DokumentenTyp = inputs[2];
				CreationDate = inputs[3];
				MimeType = inputs[4];
				BelegNummer = inputs[5];
				Buchungsdatum = inputs[6];
				Locator = inputs[7];
			}
			catch
			{
				throw new FileException("Input string isn't suitable.");
			}
		}
	}
}
