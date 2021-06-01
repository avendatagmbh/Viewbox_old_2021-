using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Viewbox.Job.PdfExport.Layouts
{
	public class OrderPdfLayout : StatementPdfLayout
	{
		public class ProductItem
		{
			public string Nr { get; set; }

			public string Produkt { get; set; }

			public string Produktbeskrivelse { get; set; }

			public string Antall { get; set; }

			public string Enhet { get; set; }

			public string Pris { get; set; }

			public string Total { get; set; }
		}

		private IList<ProductItem> _productItems;

		public string Vbeln { get; set; }

		public string Leveringsadresse1 { get; set; }

		public string Leveringsadresse2 { get; set; }

		public string Leveringsadresse3 { get; set; }

		public string Leveringsadresse4 { get; set; }

		public string Leveringsadresse5 { get; set; }

		public string Leveringsadresse6 { get; set; }

		public string Ordredato { get; set; }

		public string Kundenummer { get; set; }

		public string Kundenavn { get; set; }

		public string KundensRef { get; set; }

		public string Salgsperson { get; set; }

		public string Betalingsbetingelser { get; set; }

		public string SalgspersonTlfNr { get; set; }

		public string SalgspersonEmail { get; set; }

		public string Leveringsbetingelser { get; set; }

		public string Leveringsmetode { get; set; }

		public string Leveringsinfo { get; set; }

		public string BestiltAv { get; set; }

		public string OnsketLeveringsdato { get; set; }

		public string EstLeveringsdato { get; set; }

		public string SumEksMva { get; set; }

		public string EstimertFraktkostnad { get; set; }

		public string MinimumOrdregebyr { get; set; }

		public string PapirFakturagebyr { get; set; }

		public string FolgeseddelPaPapier { get; set; }

		public string Mva { get; set; }

		public string SumInklMva { get; set; }

		public IList<ProductItem> ProductItems => _productItems ?? (_productItems = new List<ProductItem>());

		public override void GeneratePdf(Stream stream)
		{
			Document document = new Document(PageSize.A4);
			PdfWriter pdfWriter = PdfWriter.GetInstance(document, stream);
			document.Open();
			PdfPTable table = new PdfPTable(1);
			table.SetWidthPercentage(new float[1] { 600f }, PageSize.A4);
			table.SetExtendLastRow(extendLastRows: true, extendFinalRow: true);
			FillUpData(table);
			document.Add(table);
			document.Close();
			pdfWriter.Close();
		}

		private void FillUpData(PdfPTable table)
		{
			table.AddCell(CreateLogo());
			table.AddCell(CreateTitle());
			table.AddCell(CreateSideCounter());
			table.AddCell(CreateInformationArea());
			table.AddCell(CreateValuta());
			table.AddCell(CreateProducts());
			table.AddCell(CreateSummary());
			table.AddCell(CreateCell("Leveringsomkostninger, forsikringsomkostninger og håndteringskostnader er ikke inkludert."));
			if (base.FooterEnabled)
			{
				table.AddCell(CreateFooter());
			}
		}

		private PdfPCell CreateTitle()
		{
			return new PdfPCell(new Phrase($"Ordrebekreftelse - N. {Vbeln}", FontFactory.GetFont("Helvetica", 14f)))
			{
				Padding = 20f,
				HorizontalAlignment = 1,
				Border = 0
			};
		}

		private PdfPCell CreateSideCounter()
		{
			return CreateCell("Side 1/1", FontWeight.Normal, Alignment.Right);
		}

		private PdfPCell CreateInformationArea()
		{
			PdfPTable table = new PdfPTable(2);
			table.AddCell(CreateFakturaAddress());
			table.AddCell(CreateInformation());
			table.AddCell(CreateDeliveryAddress());
			return new PdfPCell(table);
		}

		private PdfPCell CreateInformation()
		{
			PdfPTable informationCell = new PdfPTable(2);
			informationCell.SetTotalWidth(new float[2] { 100f, 200f });
			informationCell.AddCell(CreateCell("Informasjon", FontWeight.Bold, Alignment.Left, Decoration.None, Color.Gray, 2));
			informationCell.AddCell(CreateCell("Ordre dato:"));
			informationCell.AddCell(CreateCell(Ordredato));
			informationCell.AddCell(CreateCell("Kundenummer:"));
			informationCell.AddCell(CreateCell(Kundenummer));
			informationCell.AddCell(CreateCell("Kundenavn:"));
			informationCell.AddCell(CreateCell(Kundenavn));
			informationCell.AddCell(CreateCell("Kundens ref.nr:"));
			informationCell.AddCell(CreateCell(KundensRef));
			informationCell.AddCell(CreateCell("Bestilt av"));
			informationCell.AddCell(CreateCell(BestiltAv));
			informationCell.AddCell(CreateCell("Salgsperson"));
			informationCell.AddCell(CreateCell(Salgsperson));
			informationCell.AddCell(CreateCell("Salgsperson tlf. nr:"));
			informationCell.AddCell(CreateCell(SalgspersonTlfNr));
			informationCell.AddCell(CreateCell("Salgsperson e-mail:"));
			informationCell.AddCell(CreateCell(SalgspersonEmail));
			informationCell.AddCell(CreateCell("Leveringsbetingelser:"));
			informationCell.AddCell(CreateCell(Leveringsbetingelser));
			informationCell.AddCell(CreateCell("Leveringsmetode:"));
			informationCell.AddCell(CreateCell(Leveringsmetode));
			informationCell.AddCell(CreateCell("Betalingsbetingelser:"));
			informationCell.AddCell(CreateCell(Betalingsbetingelser));
			informationCell.AddCell(CreateCell("Leveringsinfo:"));
			informationCell.AddCell(CreateCell(Leveringsinfo));
			return new PdfPCell(informationCell)
			{
				PaddingBottom = 20f,
				Rowspan = 2
			};
		}

		protected override PdfPCell CreateFakturaAddress()
		{
			PdfPTable fakturaAddressTable = new PdfPTable(1);
			fakturaAddressTable.AddCell(CreateCell("Fakturaadresse", FontWeight.Bold, Alignment.Left, Decoration.None, Color.Gray));
			fakturaAddressTable.AddCell(CreateCell(base.Fakturaadresse1));
			fakturaAddressTable.AddCell(CreateCell(base.Fakturaadresse2));
			fakturaAddressTable.AddCell(CreateCell(base.Fakturaadresse3));
			fakturaAddressTable.AddCell(CreateCell($"{base.Fakturaadresse4} {base.Fakturaadresse5}"));
			fakturaAddressTable.AddCell(CreateCell(base.Fakturaadresse6));
			return new PdfPCell(fakturaAddressTable)
			{
				PaddingBottom = 20f
			};
		}

		private PdfPCell CreateDeliveryAddress()
		{
			PdfPTable table = new PdfPTable(1);
			table.AddCell(CreateCell("Leveringsadresse", FontWeight.Bold, Alignment.Left, Decoration.None, Color.Gray));
			table.AddCell(CreateCell(Leveringsadresse1));
			table.AddCell(CreateCell(Leveringsadresse2));
			table.AddCell(CreateCell(Leveringsadresse3));
			table.AddCell(CreateCell($"{Leveringsadresse4} {Leveringsadresse5}"));
			table.AddCell(CreateCell(Leveringsadresse6));
			return new PdfPCell(table)
			{
				PaddingBottom = 20f
			};
		}

		private PdfPCell CreateValuta()
		{
			return CreateCell("Valuta: NOK", FontWeight.Normal, Alignment.Right);
		}

		private PdfPCell CreateProducts()
		{
			PdfPTable table = new PdfPTable(7);
			table.SetTotalWidth(new float[7] { 75f, 75f, 100f, 75f, 75f, 75f, 75f });
			table.AddCell(CreateCell("Nr.", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			table.AddCell(CreateCell("Produkt", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			table.AddCell(CreateCell("Produktbeskrivelse", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			table.AddCell(CreateCell("Antall", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			table.AddCell(CreateCell("Enhet", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			table.AddCell(CreateCell("Pris", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			table.AddCell(CreateCell("Total", FontWeight.Bold, Alignment.Right, Decoration.Underlined));
			foreach (ProductItem productItem in ProductItems)
			{
				table.AddCell(CreateCell(productItem.Nr, FontWeight.Bold));
				table.AddCell(CreateCell(productItem.Produkt, FontWeight.Bold));
				table.AddCell(CreateCell(productItem.Produktbeskrivelse, FontWeight.Bold));
				table.AddCell(CreateCell(productItem.Antall, FontWeight.Bold));
				table.AddCell(CreateCell(productItem.Enhet, FontWeight.Bold));
				table.AddCell(CreateCell(productItem.Pris, FontWeight.Bold));
				table.AddCell(CreateCell(productItem.Total, FontWeight.Bold, Alignment.Right));
			}
			return new PdfPCell(table)
			{
				Border = 0
			};
		}

		private PdfPCell CreateSummary()
		{
			PdfPTable table = new PdfPTable(2);
			table.SetTotalWidth(new float[2] { 150f, 300f });
			table.AddCell(CreateCell("Ønsket leveringsdato:"));
			table.AddCell(CreateCell(OnsketLeveringsdato));
			table.AddCell(new PdfPCell(CreateCell("Est.leveringsdato:"))
			{
				PaddingBottom = 20f
			});
			table.AddCell(new PdfPCell(CreateCell(EstLeveringsdato, FontWeight.Normal, Alignment.Left, Decoration.Underlined))
			{
				PaddingBottom = 20f
			});
			table.AddCell(CreateCell("Sum eks. mva:", FontWeight.Bold));
			table.AddCell(CreateCell(SumEksMva, FontWeight.Normal, Alignment.Right));
			table.AddCell(CreateCell("Estimert Fraktkostnad:", FontWeight.Bold));
			table.AddCell(CreateCell(EstimertFraktkostnad, FontWeight.Normal, Alignment.Right));
			table.AddCell(CreateCell("Minimum ordregebyr:", FontWeight.Bold));
			table.AddCell(CreateCell(MinimumOrdregebyr, FontWeight.Normal, Alignment.Right));
			table.AddCell(CreateCell("Papir fakturagebyr:", FontWeight.Bold));
			table.AddCell(CreateCell(PapirFakturagebyr, FontWeight.Normal, Alignment.Right));
			table.AddCell(CreateCell("Følgeseddel på papier:", FontWeight.Bold));
			table.AddCell(CreateCell(FolgeseddelPaPapier, FontWeight.Normal, Alignment.Right));
			table.AddCell(CreateCell("25% mva:", FontWeight.Bold));
			table.AddCell(CreateCell(Mva, FontWeight.Normal, Alignment.Right));
			table.AddCell(CreateCell("Sum inkl. mva:", FontWeight.Bold));
			table.AddCell(CreateCell(SumInklMva, FontWeight.Normal, Alignment.Right));
			return new PdfPCell(table)
			{
				Border = 0,
				PaddingLeft = 75f,
				PaddingTop = 20f,
				PaddingBottom = 20f,
				PaddingRight = 75f
			};
		}

		protected override PdfPCell CreateFooter()
		{
			PdfPTable footerTable = new PdfPTable(3);
			footerTable.SetTotalWidth(new float[3] { 350f, 100f, 150f });
			footerTable.AddCell(CreateCell("ALSO Norway AS", FontWeight.Bold, Alignment.Left, Decoration.Overlined));
			footerTable.AddCell(CreateCell("E-mail:", FontWeight.Normal, Alignment.Right, Decoration.Overlined));
			footerTable.AddCell(CreateCell("nor-smbsales@also.com", FontWeight.Normal, Alignment.Left, Decoration.Overlined));
			footerTable.AddCell(CreateCell("Østre Kullerød 2, 3241 Sandefjord"));
			footerTable.AddCell(CreateCell("Telefon:", FontWeight.Normal, Alignment.Right));
			footerTable.AddCell(CreateCell("+4733449500"));
			footerTable.AddCell(CreateCell("NO943347069MVA"));
			footerTable.AddCell(CreateCell("Internet:", FontWeight.Normal, Alignment.Right));
			footerTable.AddCell(CreateCell("www.also.no"));
			return new PdfPCell(footerTable)
			{
				VerticalAlignment = 6,
				Border = 0
			};
		}
	}
}
