using System.IO;

namespace ViewboxMdConverter
{
	internal class PdfToPdfConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			File.Copy(input, output);
			return true;
		}
	}
}
