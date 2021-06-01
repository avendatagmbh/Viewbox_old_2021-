using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Fireball.Drawing;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ViewboxMdConverter
{
	internal class PcxToPdfConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			Document document = new Document();
			using (FileStream stream = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				Bitmap bitmap = new FreeImage(input).GetBitmap();
				if (bitmap.Width > bitmap.Height)
				{
					PageSize.A4.Rotate();
					document.SetPageSize(PageSize.A4.Rotate());
				}
				document.Open();
				PdfWriter.GetInstance(document, stream);
				iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(bitmap, ImageFormat.MemoryBmp);
				image.SetAbsolutePosition(0f, 0f);
				image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
				document.Add(image);
				document.Close();
			}
			return true;
		}
	}
}
