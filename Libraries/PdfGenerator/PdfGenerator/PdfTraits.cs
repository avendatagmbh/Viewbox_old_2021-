using iTextSharp.text;

namespace PdfGenerator
{
	public class PdfTraits
	{
		public readonly string Author;

		public bool CreateTableOfContents;

		public readonly string Creator;

		public float MarginBottom;

		public float MarginLeft;

		public float MarginRight;

		public float MarginTop;

		public Rectangle PageSizeAndOrientation;

		public readonly Font fontH1;

		public readonly Font fontH2;

		public readonly Font fontH3;

		public readonly Font fontH4;

		public readonly Font xfont;

		public PdfTraits(string author, string creator)
		{
			Author = author;
			Creator = creator;
			MarginLeft = 25f * PageSize.A4.Rotate().Width / 210f;
			MarginRight = 25f * PageSize.A4.Rotate().Width / 210f;
			MarginTop = 18f * PageSize.A4.Rotate().Width / 210f;
			MarginBottom = 8f * PageSize.A4.Rotate().Width / 210f;
			PageSizeAndOrientation = PageSize.A4.Rotate();
			CreateTableOfContents = true;
			fontH1 = new Font(Font.FontFamily.HELVETICA, 12f, 1);
			fontH2 = new Font(Font.FontFamily.HELVETICA, 10f, 1);
			fontH3 = new Font(Font.FontFamily.HELVETICA, 9f, 1);
			fontH4 = new Font(Font.FontFamily.HELVETICA, 9f, 2);
			xfont = new Font(Font.FontFamily.HELVETICA, 11f);
		}

		public float mm()
		{
			return PageSize.A4.Rotate().Width / 210f;
		}
	}
}
