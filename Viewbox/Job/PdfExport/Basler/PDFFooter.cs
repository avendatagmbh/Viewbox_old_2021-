using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Viewbox.Job.PdfExport.Basler
{
	public class PDFFooter : PdfPageEventHelper
	{
		public override void OnOpenDocument(PdfWriter writer, Document document)
		{
			base.OnOpenDocument(writer, document);
			PdfPTable tabFot = new PdfPTable(new float[1] { 1f });
			tabFot.SpacingAfter = 10f;
			tabFot.TotalWidth = 300f;
			PdfPCell cell = new PdfPCell(new Phrase("Header"));
			tabFot.AddCell(cell);
			tabFot.WriteSelectedRows(0, -1, 150f, document.Top, writer.DirectContent);
		}

		public override void OnStartPage(PdfWriter writer, Document document)
		{
			base.OnStartPage(writer, document);
		}

		public override void OnEndPage(PdfWriter writer, Document document)
		{
			base.OnEndPage(writer, document);
			PdfPTable tabFot = new PdfPTable(new float[1] { 1f });
			tabFot.TotalWidth = 300f;
			PdfPCell cell = new PdfPCell(new Phrase("Footer"));
			tabFot.AddCell(cell);
			tabFot.WriteSelectedRows(0, -1, 150f, document.Bottom, writer.DirectContent);
		}

		public override void OnCloseDocument(PdfWriter writer, Document document)
		{
			base.OnCloseDocument(writer, document);
		}
	}
}
