using System.Collections.Generic;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Viewbox.Job.PdfExport.Layouts
{
	public abstract class FacturaKreditnotePdfLayout : StatementPdfLayout
	{
		public class ProductItem
		{
			public string Produkt { get; set; }

			public string Produktbeskrivelse { get; set; }

			public string Antall { get; set; }

			public string MvaPercentage2 { get; set; }

			public string Pris { get; set; }

			public string Belop { get; set; }
		}

		private List<ProductItem> _productitemlist;

		public string TotalNetto { get; set; }

		public string MvaPercentage { get; set; }

		public string Mva { get; set; }

		public string TotalBelop { get; set; }

		public string SerialNumber { get; set; }

		public string Handteringsgebyr { get; set; }

		public string Fraktkostnad { get; set; }

		public string Leveringsadresse1 { get; set; }

		public string Leveringsadresse2 { get; set; }

		public string Leveringsadresse3 { get; set; }

		public string Leveringsadresse4 { get; set; }

		public string Ordredato { get; set; }

		public string Ordrenummer { get; set; }

		public string Kundenummer { get; set; }

		public string KundesRef { get; set; }

		public string BestiltAv { get; set; }

		public string Salgsperson { get; set; }

		public string Sendingsnummer { get; set; }

		public string Leveringsdato { get; set; }

		public string Leveringsbetingelser { get; set; }

		public string Leveringsmate { get; set; }

		public string Betalingsbetingelser { get; set; }

		public string Morarenter { get; set; }

		public string FakturaId { get; set; }

		public string Fakturadato { get; set; }

		public List<ProductItem> ProductItems => _productitemlist ?? (_productitemlist = new List<ProductItem>());

		public override void GeneratePdf(Stream stream)
		{
			Document document = new Document(PageSize.A4);
			PdfWriter pdfWriter = PdfWriter.GetInstance(document, stream);
			document.Open();
			PdfPTable table = new PdfPTable(2);
			table.SetWidthPercentage(new float[2] { 300f, 300f }, PageSize.A4);
			table.SetExtendLastRow(extendLastRows: true, extendFinalRow: true);
			table.AddCell(new PdfPCell(CreateFakturaId())
			{
				Border = 0
			});
			table.AddCell(CreateLogo());
			table.AddCell(new PdfPCell(CreateCell("1/1"))
			{
				Colspan = 2,
				Border = 0,
				HorizontalAlignment = 2,
				PaddingTop = 10f,
				PaddingBottom = 10f
			});
			table.AddCell(CreateFakturaAddress());
			AddCustomBoxes(table);
			table.AddCell(new PdfPCell(CreateProductLine())
			{
				Border = 0,
				PaddingBottom = 20f,
				PaddingTop = 20f
			});
			table.AddCell(new PdfPCell(CreateTotalLine())
			{
				Border = 0
			});
			if (base.FooterEnabled)
			{
				AddTextBox(table);
				table.AddCell(new PdfPCell
				{
					FixedHeight = 10f,
					Colspan = 2,
					Border = 0
				});
				table.AddCell(new PdfPCell(CreateFooter())
				{
					Colspan = 2
				});
			}
			document.Add(table);
			document.Close();
			pdfWriter.Close();
		}

		protected abstract void AddCustomBoxes(PdfPTable table);

		protected abstract void AddTextBox(PdfPTable table);

		protected abstract string GetFraktkostnadString();

		protected abstract string GetFakturaString();

		protected abstract string GetFakturaDatoString();

		protected PdfPCell CreateFakturaId()
		{
			PdfPTable fakturaTable = new PdfPTable(2);
			fakturaTable.AddCell(CreateCell(GetFakturaString(), FontWeight.Bold));
			fakturaTable.AddCell(CreateCell(FakturaId, FontWeight.Bold));
			fakturaTable.AddCell(CreateCell(GetFakturaDatoString()));
			fakturaTable.AddCell(CreateCell(Fakturadato));
			return new PdfPCell(fakturaTable)
			{
				VerticalAlignment = 6
			};
		}

		protected PdfPCell CreateTotalLine()
		{
			PdfPTable totalLineTable = new PdfPTable(4);
			totalLineTable.AddCell(CreateCell("Total Netto", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			totalLineTable.AddCell(CreateCell("MVA %", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			totalLineTable.AddCell(CreateCell("MVA", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			totalLineTable.AddCell(CreateCell("Total Beløp", FontWeight.Bold, Alignment.Right, Decoration.Underlined));
			totalLineTable.AddCell(CreateCell(TotalNetto));
			totalLineTable.AddCell(CreateCell(MvaPercentage));
			totalLineTable.AddCell(CreateCell(Mva));
			totalLineTable.AddCell(CreateCell(TotalBelop, FontWeight.Normal, Alignment.Right));
			return new PdfPCell(totalLineTable)
			{
				Colspan = 2,
				PaddingBottom = 20f
			};
		}

		protected PdfPCell CreateProductLine()
		{
			PdfPTable productLineTable = new PdfPTable(6);
			productLineTable.AddCell(CreateCell("Produkt", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productLineTable.AddCell(CreateCell("Produktbeskrivelse", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productLineTable.AddCell(CreateCell("Antall", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productLineTable.AddCell(CreateCell("MVA %", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productLineTable.AddCell(CreateCell("Pris", FontWeight.Bold, Alignment.Left, Decoration.Underlined));
			productLineTable.AddCell(CreateCell("Beløp", FontWeight.Bold, Alignment.Right, Decoration.Underlined));
			foreach (ProductItem productItem in ProductItems)
			{
				productLineTable.AddCell(CreateCell(productItem.Produkt));
				productLineTable.AddCell(CreateCell(productItem.Produktbeskrivelse));
				productLineTable.AddCell(CreateCell(productItem.Antall));
				productLineTable.AddCell(CreateCell(productItem.MvaPercentage2));
				productLineTable.AddCell(CreateCell(productItem.Pris));
				productLineTable.AddCell(CreateCell(productItem.Belop, FontWeight.Normal, Alignment.Right));
			}
			double.TryParse(Fraktkostnad, out var fraktosnad);
			if (fraktosnad > 0.0)
			{
				productLineTable.AddCell(new PdfPCell(CreateCell(GetFraktkostnadString()))
				{
					Colspan = 3,
					PaddingTop = 10f
				});
				productLineTable.AddCell(new PdfPCell(CreateCell(Fraktkostnad))
				{
					Colspan = 3,
					HorizontalAlignment = 2,
					PaddingTop = 10f
				});
			}
			return new PdfPCell(productLineTable)
			{
				Colspan = 2
			};
		}
	}
}
