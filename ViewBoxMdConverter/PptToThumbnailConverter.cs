using System.IO;
using System.Text;

namespace ViewboxMdConverter
{
	internal class PptToThumbnailConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			StringBuilder tempFile = new StringBuilder();
			tempFile.Append(input);
			do
			{
				tempFile.Append("_TEMP");
			}
			while (File.Exists(tempFile.ToString()));
			tempFile.Append(".pdf");
			try
			{
				ConverterFactory.Instance.GetMdToPdfExtension("application/ppt").Convert(input, tempFile.ToString());
				return ConverterFactory.Instance.GetMdToImageExtension("application/pdf").Convert(tempFile.ToString(), output);
			}
			finally
			{
				File.Delete(tempFile.ToString());
			}
		}
	}
}
