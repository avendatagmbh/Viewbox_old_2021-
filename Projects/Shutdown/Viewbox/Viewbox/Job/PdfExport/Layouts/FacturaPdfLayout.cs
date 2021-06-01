using iTextSharp.text.pdf;

namespace Viewbox.Job.PdfExport.Layouts
{
	public class FacturaPdfLayout : FacturaKreditnotePdfLayout
	{
		public string Kidnr { get; set; }

		public string Forfallsdato { get; set; }

		protected override void AddCustomBoxes(PdfPTable table)
		{
			table.AddCell(CreateInformation());
			table.AddCell(CreateLeveringsAddress());
		}

		protected override void AddTextBox(PdfPTable table)
		{
			table.AddCell(new PdfPCell(CreateTextbox()));
			table.AddCell(new PdfPCell
			{
				FixedHeight = 10f,
				Colspan = 2,
				Border = 0
			});
		}

		private PdfPCell CreateLeveringsAddress()
		{
			PdfPTable leveringsAddressTable = new PdfPTable(1);
			leveringsAddressTable.AddCell(CreateCell("Leveringsadresse", FontWeight.Bold, Alignment.Left, Decoration.None, Color.Gray));
			leveringsAddressTable.AddCell(CreateCell(base.Leveringsadresse1));
			leveringsAddressTable.AddCell(CreateCell(base.Leveringsadresse2));
			leveringsAddressTable.AddCell(CreateCell(base.Leveringsadresse3));
			leveringsAddressTable.AddCell(CreateCell(base.Leveringsadresse4));
			return new PdfPCell(leveringsAddressTable)
			{
				PaddingBottom = 20f
			};
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
			informationCell.AddCell(CreateCell("Sendingsnummer"));
			informationCell.AddCell(CreateCell(base.Sendingsnummer));
			informationCell.AddCell(CreateCell("Leveringsdato"));
			informationCell.AddCell(CreateCell(base.Leveringsdato));
			informationCell.AddCell(CreateCell("Leveringsbetingelser"));
			informationCell.AddCell(CreateCell(base.Leveringsbetingelser));
			informationCell.AddCell(CreateCell("Leveringsmate"));
			informationCell.AddCell(CreateCell(base.Leveringsmate));
			informationCell.AddCell(CreateCell("Betalingsbetingelser"));
			informationCell.AddCell(CreateCell(base.Betalingsbetingelser));
			return new PdfPCell(informationCell)
			{
				Rowspan = 2,
				PaddingBottom = 20f
			};
		}

		private PdfPCell CreateTextbox()
		{
			PdfPTable textTable = new PdfPTable(2);
			textTable.AddCell(new PdfPCell(CreateCell("ALSO Norway AS fordring i henhold til denne faktura er overdratt til ALSO Nordic Finance. Frigjørende betaling kan kun skje ved innbetaling til ALSO Nordic Finance, kontonr. 6021 07 34069 hos Nordea Bank Norge ASA. SWIFT: NDEANOKK. IBAN: NO5560210734069. Angi alltid KIDnr. eller fakturanr. ved betaling. Evt. reklamasjon sendes nor-credit@also.com (eller tlf. 33 44 96 60).\n"))
			{
				Colspan = 2
			});
			textTable.AddCell(CreateCell("MOTTAGERE"));
			textTable.AddCell(CreateCell("ALSO Norway AS"));
			textTable.AddCell(CreateCell("BETALES TIL"));
			textTable.AddCell(CreateCell("Nordea Bank Norge Asa 60210734069"));
			textTable.AddCell(CreateCell("KIDNR"));
			textTable.AddCell(CreateCell(Kidnr));
			textTable.AddCell(CreateCell("FORFALLSDATO"));
			textTable.AddCell(CreateCell(Forfallsdato));
			return new PdfPCell(textTable)
			{
				Colspan = 2,
				Border = 15,
				BorderWidth = 0.5f,
				Padding = 10f
			};
		}

		protected override string GetFraktkostnadString()
		{
			return "Fraktkostnad";
		}

		protected override string GetFakturaString()
		{
			return "Faktura";
		}

		protected override string GetFakturaDatoString()
		{
			return "Fakturadato";
		}
	}
}
