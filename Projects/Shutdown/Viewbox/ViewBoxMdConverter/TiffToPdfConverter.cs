using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ViewboxMdConverter
{
	internal class TiffToPdfConverter : GeneralConverter, IFileConverter
	{
		public bool Convert(string input, string output)
		{
			Document document = new Document();
			using (FileStream stream = File.OpenWrite(output))
			{
				PdfWriter writer = PdfWriter.GetInstance(document, stream);
				AddTiffToPdf(input, ref writer, ref document);
				document.Close();
			}
			return true;
		}

		private void AddTiffToPdf(string tiffFileName, ref PdfWriter writer, ref Document document)
		{
			iTextSharp.text.Rectangle size = PageSize.A4;
			Bitmap bitmap = new Bitmap(tiffFileName);
			int numberOfPages = bitmap.GetFrameCount(FrameDimension.Page);
			iTextSharp.text.Rectangle rect = new iTextSharp.text.Rectangle(0f, 0f, bitmap.Width, bitmap.Height);
			document.Open();
			PdfContentByte cb = writer.DirectContent;
			for (int page = 0; page < numberOfPages; page++)
			{
				if (bitmap.Width > bitmap.Height)
				{
					cb.PdfDocument.SetPageSize(rect);
				}
				else
				{
					cb.PdfDocument.SetPageSize(size);
				}
				bitmap.SelectActiveFrame(FrameDimension.Page, page);
				MemoryStream stream = new MemoryStream();
				bitmap.Save(stream, ImageFormat.Png);
				iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(stream.ToArray());
				stream.Close();
				img.ScalePercent(72f / bitmap.HorizontalResolution * 100f);
				img.SetAbsolutePosition(0f, 0f);
				img.ScaleToFit(document.PageSize.Width, document.PageSize.Height);
				cb.AddImage(img);
				document.NewPage();
			}
		}
	}
}
