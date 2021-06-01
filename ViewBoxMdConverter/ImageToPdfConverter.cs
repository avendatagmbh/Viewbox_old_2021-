using System.Drawing;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ViewboxMdConverter
{
	internal class ImageToPdfConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			Document document = new Document();
			using (FileStream stream = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				PdfWriter.GetInstance(document, stream);
				using (FileStream imageStream = new FileStream(input, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					Bitmap bitmap = new Bitmap(input);
					if (bitmap.Width > bitmap.Height)
					{
						PageSize.A4.Rotate();
						document.SetPageSize(PageSize.A4.Rotate());
					}
					document.Open();
					iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(iTextSharp.text.Image.GetInstance((Stream)imageStream));
					image.SetAbsolutePosition(0f, 0f);
					image.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
					document.Add(image);
				}
				document.Close();
			}
			return true;
		}
	}
}
