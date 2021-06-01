using iTextSharp.text.pdf;

namespace Viewbox.Job.PdfExport.Layouts
{
	public class KreditnotePdfLayout : FacturaKreditnotePdfLayout
	{
		protected override void AddCustomBoxes(PdfPTable table)
		{
			table.AddCell(CreateInformation());
			table.AddCell(new PdfPCell
			{
				Border = 0
			});
		}

		protected override void AddTextBox(PdfPTable table)
		{
		}

		private PdfPCell CreateInformation()
		{
			PdfPTable informationCell = new PdfPTable(2);
			informationCell.AddCell(new PdfPCell(CreateCell("Informasjon", FontWeight.Bold, Alignment.Left, Decoration.None, Color.Gray))
			{
				Colspan = 2
			});
			informationCell.AddCell(CreateCell("Ordredato"));
			informationCell.AddCell(CreateCell(base.Ordredato));
			informationCell.AddCell(CreateCell("Ordrenummer"));
			informationCell.AddCell(CreateCell(base.Ordrenummer));
			informationCell.AddCell(CreateCell("Kundenummer"));
			informationCell.AddCell(CreateCell(base.Kundenummer));
			informationCell.AddCell(CreateCell("Kundes Ref."));
			informationCell.AddCell(CreateCell(base.KundesRef));
			informationCell.AddCell(CreateCell("Salgsperson"));
			informationCell.AddCell(CreateCell(base.Salgsperson));
			informationCell.AddCell(CreateCell("Betalingsbetingelser"));
			informationCell.AddCell(CreateCell(base.Betalingsbetingelser));
			return new PdfPCell(informationCell)
			{
				Rowspan = 2,
				PaddingBottom = 20f
			};
		}

		protected override string GetFraktkostnadString()
		{
			return "Handteringsgebyr";
		}

		protected override string GetFakturaString()
		{
			return "Kreditnota";
		}

		protected override string GetFakturaDatoString()
		{
			return "Kreditnota Dato";
		}
	}
}
