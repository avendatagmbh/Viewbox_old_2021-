using System.IO;
using NReco.ImageGenerator;

namespace ViewboxMdConverter
{
	internal class HtmlToThumbnailConverter : GeneralConverter, IFileConverter
	{
		public HtmlToThumbnailConverter()
		{
			new HtmlToImageConverter().GenerateImage("<html></html>", "JPG");
		}

		public bool Convert(string input, string output)
		{
			File.ReadAllText(input);
			byte[] result = new HtmlToImageConverter().GenerateImageFromFile(input, "JPG");
			using (FileStream f = File.Create(output))
			{
				byte[] array = result;
				foreach (byte b in array)
				{
					f.WriteByte(b);
				}
			}
			return true;
		}
	}
}
