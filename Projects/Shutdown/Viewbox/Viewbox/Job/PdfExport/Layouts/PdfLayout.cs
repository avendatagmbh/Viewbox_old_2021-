using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Viewbox.Job.PdfExport.Layouts
{
	public abstract class PdfLayout
	{
		public abstract void GeneratePdf(Stream stream);

		protected PdfPCell CreateCell(string text, FontWeight fontWeight = FontWeight.Normal, Alignment alignment = Alignment.Left, Decoration decoration = Decoration.None, Color backgroundColor = Color.White, int colspan = 1)
		{
			Font font = GetFont(fontWeight);
			int border = GetBorder(decoration);
			int align = GetAlignment(alignment);
			return new PdfPCell(new Phrase(text, font))
			{
				Border = border,
				HorizontalAlignment = align,
				BackgroundColor = ((backgroundColor == Color.White) ? BaseColor.WHITE : BaseColor.LIGHT_GRAY),
				Colspan = colspan,
				Padding = 3f
			};
		}

		private static int GetBorder(Decoration decoration)
		{
			int border = 0;
			return decoration switch
			{
				Decoration.None => 0, 
				Decoration.Underlined => 2, 
				Decoration.Overlined => 1, 
				_ => throw new ArgumentOutOfRangeException("decoration"), 
			};
		}

		private static Font GetFont(FontWeight fontWeight)
		{
			return (fontWeight == FontWeight.Normal) ? FontFactory.GetFont("Helvetica", 8f) : FontFactory.GetFont("Helvetica-Bold", 8f);
		}

		private static int GetAlignment(Alignment alignment)
		{
			return alignment switch
			{
				Alignment.Left => 0, 
				Alignment.Center => 1, 
				Alignment.Right => 2, 
				_ => 0, 
			};
		}
	}
}
