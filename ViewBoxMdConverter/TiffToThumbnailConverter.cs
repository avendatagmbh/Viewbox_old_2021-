using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ViewboxMdConverter
{
	internal class TiffToThumbnailConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			using (Image jpeg = ConvertToThumbnail(input, 700, 990))
			{
				jpeg.Save(output, ImageFormat.Jpeg);
			}
			return true;
		}

		internal static bool ThumnailDelegate()
		{
			return false;
		}

		public static Image ConvertToThumbnail(string fileName, int width, int height)
		{
			using FileStream fs = File.OpenRead(fileName);
			Image retVal = ConvertToThumbnail(fs, width, height);
			fs.Close();
			return retVal;
		}

		public static Image ConvertToThumbnail(Stream imgStream, int width, int height)
		{
			Stream retStream = new MemoryStream();
			Image.GetThumbnailImageAbort myThumnailDelegate = ThumnailDelegate;
			using (Image img = Image.FromStream(imgStream, useEmbeddedColorManagement: true, validateImageData: true))
			{
				img.Save(retStream, ImageFormat.Jpeg);
				retStream.Flush();
			}
			return Image.FromStream(retStream, useEmbeddedColorManagement: true, validateImageData: true).GetThumbnailImage(width, height, myThumnailDelegate, IntPtr.Zero);
		}
	}
}
