using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text;

namespace ViewboxMdConverter
{
	internal class TextToThumbnailConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			string text;
			using (StreamReader sr = new StreamReader(input, Encoding.Default))
			{
				text = sr.ReadToEnd();
				sr.Close();
			}
			Image img = ConvertTextToImage(text);
			if (img != null)
			{
				img = img.GetThumbnailImage(347, 491, ThumnailDelegate, IntPtr.Zero);
				img.Save(output, ImageFormat.Jpeg);
			}
			return true;
		}

		internal static bool ThumnailDelegate()
		{
			return false;
		}

		public Image ConvertTextToImage(string text)
		{
			Bitmap original = new Bitmap(700, 990);
			Font objFont = new Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Pixel);
			Bitmap bitmap = new Bitmap(original);
			Graphics graphics = Graphics.FromImage(bitmap);
			graphics.Clear(Color.White);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			graphics.DrawString(text, objFont, new SolidBrush(Color.FromArgb(102, 102, 102)), 0f, 0f);
			graphics.Flush();
			return bitmap;
		}
	}
}
