using System.Drawing;
using System.Drawing.Imaging;
using Fireball.Drawing;

namespace ViewboxMdConverter
{
	internal class PcxToThumbnailConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			new Bitmap(new FreeImage(input).GetBitmap(), new Size(700, 990)).Save(output, ImageFormat.Jpeg);
			return true;
		}
	}
}
