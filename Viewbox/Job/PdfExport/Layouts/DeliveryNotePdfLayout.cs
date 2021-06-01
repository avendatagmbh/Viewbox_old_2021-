using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Viewbox.Properties;

namespace Viewbox.Job.PdfExport.Layouts
{
	public class DeliveryNotePdfLayout : StatementPdfLayout
	{
		public class ProductItem
		{
			public string Nr { get; set; }

			public string Varenummer { get; set; }

			public string Bestilt { get; set; }

			public string Levert { get; set; }

			public string Tidligere { get; set; }

			public string Rest { get; set; }

			public string PakkeNummer { get; set; }
		}

		private IList<ProductItem> _productItems;

		public string Vbeln { get; set; }

		public string Leveringsadresse1 { get; set; }

		public string Leveringsadresse2 { get; set; }

		public string Leveringsadresse3 { get; set; }

		public string Leveringsadresse4 { get; set; }

		public string Leveringsadresse5 { get; set; }

		public string DelNr { get; set; }

		public string Dato { get; set; }

		public string Kundenummer { get; set; }

		public string Kundename { get; set; }

		public string Leveringsmate { get; set; }

		public string Speditor { get; set; }

		public string Leveringsbetingelser { get; set; }

		public string PakketAv { get; set; }

		public string Antall { get; set; }

		public string PakkeIds { get; set; }

		public string TotalVekt { get; set; }

		public string Ordrenummer { get; set; }

		public string Salgsperson { get; set; }

		public string DeresRef { get; set; }

		public string BestiltAv { get; set; }

		public IList<ProductItem> ProductItems => _productItems ?? (_productItems = new List<ProductItem>());

		private bool IsAteaVersion => base.Fakturaadresse1.Trim().ToUpper().Equals("ATEA AS");

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
			table.AddCell(CreateCurrentLogo());
			table.AddCell(CreateTitle());
			table.AddCell(CreateSideCounter());
			table.AddCell(CreateInformationArea());
			CreateProducts(table);
			table.AddCell(CreateSummary());
			if (base.FooterEnabled)
			{
				table.AddCell(CreateFooter());
			}
		}

		private PdfPCell CreateCurrentLogo()
		{
			//base.Logo = (IsAteaVersion ? Resources.AteaLogo : Resources.AlsoLogo);
			return CreateLogo();
		}

		private PdfPCell CreateTitle()
		{
			return new PdfPCell(new Phrase($"Følgeseddel {Vbeln}", FontFactory.GetFont("Helvetica-Bold", 14f)))
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
			informationCell.AddCell(CreateCell("Del.nr./Dato:"));
			informationCell.AddCell(CreateCell($"{DelNr} / {Dato}"));
			informationCell.AddCell(CreateCell("Kundenummer:"));
			informationCell.AddCell(CreateCell($"{Kundenummer} / {Kundename}"));
			informationCell.AddCell(CreateCell("Leveringsmåte:"));
			informationCell.AddCell(CreateCell(Leveringsmate));
			informationCell.AddCell(CreateCell("Speditør:"));
			informationCell.AddCell(CreateCell(Speditor));
			informationCell.AddCell(CreateCell("Lev.bet:"));
			informationCell.AddCell(CreateCell(Leveringsbetingelser));
			informationCell.AddCell(CreateCell("Pakket av:"));
			informationCell.AddCell(CreateCell(PakketAv));
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
			return new PdfPCell(table)
			{
				PaddingBottom = 20f
			};
		}

		private void CreateProducts(PdfPTable table)
		{
			table.AddCell(CreateProductHeader());
			foreach (ProductItem productItem in ProductItems)
			{
				table.AddCell(CreateProductItemLine(productItem));
			}
		}

		private PdfPCell CreateProductItemLine(ProductItem productItem)
		{
			PdfPTable itemTable = new PdfPTable(6);
			itemTable.SetTotalWidth(new float[6] { 50f, 250f, 50f, 50f, 50f, 50f });
			itemTable.AddCell(CreateCell(productItem.Nr, FontWeight.Bold));
			itemTable.AddCell(CreateCell(productItem.Varenummer, FontWeight.Bold));
			itemTable.AddCell(CreateCell(productItem.Bestilt, FontWeight.Bold));
			itemTable.AddCell(CreateCell(productItem.Levert, FontWeight.Bold));
			itemTable.AddCell(CreateCell(productItem.Tidligere, FontWeight.Bold));
			itemTable.AddCell(CreateCell(productItem.Rest, FontWeight.Bold));
			itemTable.AddCell(new PdfPCell(CreateCell($"Pakke Nummer: {productItem.PakkeNummer}"))
			{
				Colspan = 6,
				PaddingTop = 15f,
				PaddingBottom = 2f,
				PaddingLeft = 52f
			});
			return new PdfPCell(itemTable)
			{
				Border = 0
			};
		}

		private PdfPCell CreateProductHeader()
		{
			PdfPTable productHeader = new PdfPTable(6);
			productHeader.SetTotalWidth(new float[6] { 50f, 250f, 50f, 50f, 50f, 50f });
			productHeader.AddCell(CreateCell("Nr.", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productHeader.AddCell(CreateCell("Varenummer og varebeskrivelse", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productHeader.AddCell(CreateCell("Bestilt", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productHeader.AddCell(CreateCell("Levert", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productHeader.AddCell(CreateCell("Tidligere", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productHeader.AddCell(CreateCell("Rest", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			PdfPTable detailsTable = new PdfPTable(2);
			detailsTable.SetTotalWidth(new float[2] { 60f, 250f });
			detailsTable.AddCell(CreateCell("Ordrenummer:", FontWeight.Bold));
			detailsTable.AddCell(CreateCell(Ordrenummer));
			detailsTable.AddCell(CreateCell("Salgsperson:", FontWeight.Bold));
			detailsTable.AddCell(CreateCell(Salgsperson));
			detailsTable.AddCell(CreateCell("Deres referanse:", FontWeight.Bold));
			detailsTable.AddCell(CreateCell(DeresRef));
			detailsTable.AddCell(CreateCell("bestilt av:", FontWeight.Bold));
			detailsTable.AddCell(CreateCell(BestiltAv));
			productHeader.AddCell(new PdfPCell(detailsTable)
			{
				Colspan = 6,
				PaddingTop = 5f,
				PaddingBottom = 10f,
				PaddingLeft = 52f,
				Border = 2
			});
			return new PdfPCell(productHeader)
			{
				Border = 0,
				PaddingTop = 10f
			};
		}

		private PdfPCell CreateSummary()
		{
			PdfPTable table = new PdfPTable(2);
			table.SetTotalWidth(new float[2] { 150f, 300f });
			table.AddCell(CreateCell("Antall kolli:"));
			table.AddCell(CreateCell(Antall));
			table.AddCell(CreateCell("Pakke IDs:"));
			table.AddCell(CreateCell(PakkeIds));
			table.AddCell(CreateCell("Total vekt:"));
			table.AddCell(CreateCell(TotalVekt));
			return new PdfPCell(table)
			{
				Border = 0,
				PaddingTop = 50f,
				PaddingBottom = 20f,
				PaddingRight = 50f
			};
		}

		protected override PdfPCell CreateFooter()
		{
			return IsAteaVersion ? CreateAteaFooter() : CreateAlsoFooter();
		}

		private PdfPCell CreateAlsoFooter()
		{
			PdfPTable footerTable = new PdfPTable(3);
			footerTable.SetTotalWidth(new float[3] { 350f, 100f, 150f });
			footerTable.AddCell(CreateCell("ALSO Norway AS", FontWeight.Bold, Alignment.Left, Decoration.Overlined));
			footerTable.AddCell(CreateCell("Email:", FontWeight.Normal, Alignment.Right, Decoration.Overlined));
			footerTable.AddCell(CreateCell("nor-kundesenter@also.com", FontWeight.Normal, Alignment.Left, Decoration.Overlined));
			footerTable.AddCell(CreateCell("Østre Kullerød 2 NO - 3241 Sandefjord"));
			footerTable.AddCell(CreateCell("Telefon:", FontWeight.Normal, Alignment.Right));
			footerTable.AddCell(CreateCell("+4733449550"));
			footerTable.AddCell(CreateCell(string.Empty));
			footerTable.AddCell(CreateCell("Internet:", FontWeight.Normal, Alignment.Right));
			footerTable.AddCell(CreateCell("www.also.no"));
			return new PdfPCell(footerTable)
			{
				VerticalAlignment = 6,
				Border = 0
			};
		}

		private PdfPCell CreateAteaFooter()
		{
			PdfPTable footerTable = new PdfPTable(3);
			footerTable.SetTotalWidth(new float[3] { 350f, 100f, 150f });
			footerTable.AddCell(CreateCell("ATEA AS", FontWeight.Bold, Alignment.Left, Decoration.Overlined));
			footerTable.AddCell(CreateCell("Email:", FontWeight.Normal, Alignment.Right, Decoration.Overlined));
			footerTable.AddCell(CreateCell("merethe.stuen@alsoactebis.com", FontWeight.Normal, Alignment.Left, Decoration.Overlined));
			footerTable.AddCell(CreateCell("BRYNSALLÉEN 2 NO - 0605 OSLO"));
			footerTable.AddCell(CreateCell("Telefon:", FontWeight.Normal, Alignment.Right));
			footerTable.AddCell(CreateCell("+4722095000"));
			return new PdfPCell(footerTable)
			{
				VerticalAlignment = 6,
				Border = 0
			};
		}
	}
}
