using System.Drawing;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Viewbox.Job.PdfExport.Layouts
{
	public abstract class StatementPdfLayout : PdfLayout
	{
		public string Fakturaadresse1 { get; set; }

		public string Fakturaadresse2 { get; set; }

		public string Fakturaadresse3 { get; set; }

		public string Fakturaadresse4 { get; set; }

		public string Fakturaadresse5 { get; set; }

		public string Fakturaadresse6 { get; set; }

		public Bitmap Logo { get; set; }

		public bool FooterEnabled { get; set; }

		public string[] FooterFields { get; set; }

		protected StatementPdfLayout()
		{
			FooterEnabled = true;
		}

		protected PdfPCell CreateLogo()
		{
			if (Logo == null)
			{
				return new PdfPCell
				{
					Border = 0
				};
			}
			byte[] logo = GetBitmapBytes(Logo);
			return new PdfPCell(iTextSharp.text.Image.GetInstance(logo))
			{
				HorizontalAlignment = 2,
				Border = 0
			};
		}

		protected static byte[] GetBitmapBytes(Bitmap bitmap)
		{
			MemoryStream memStream = new MemoryStream();
			try
			{
				bitmap.Save(memStream, bitmap.RawFormat);
				byte[] bytes = new byte[memStream.Length];
				memStream.Seek(0L, SeekOrigin.Begin);
				memStream.Read(bytes, 0, bytes.Length);
				return bytes;
			}
			finally
			{
				memStream.Close();
			}
		}

		protected virtual PdfPCell CreateFakturaAddress()
		{
			PdfPTable fakturaAddressTable = new PdfPTable(1);
			fakturaAddressTable.AddCell(CreateCell("Fakturaadresse", FontWeight.Bold, Alignment.Left, Decoration.None, Color.Gray));
			fakturaAddressTable.AddCell(CreateCell(Fakturaadresse1));
			fakturaAddressTable.AddCell(CreateCell(Fakturaadresse2));
			fakturaAddressTable.AddCell(CreateCell(Fakturaadresse3));
			fakturaAddressTable.AddCell(CreateCell(Fakturaadresse4));
			fakturaAddressTable.AddCell(CreateCell(Fakturaadresse5));
			fakturaAddressTable.AddCell(CreateCell(Fakturaadresse6));
			return new PdfPCell(fakturaAddressTable)
			{
				PaddingBottom = 20f
			};
		}

		protected virtual PdfPCell CreateFooter()
		{
			PdfPTable footerTable = new PdfPTable(3);
			footerTable.SetTotalWidth(new float[3] { 400f, 100f, 100f });
			footerTable.AddCell(CreateCell("ALSO Norway AS", FontWeight.Bold, Alignment.Left, Decoration.Overlined));
			footerTable.AddCell(CreateCell("E-mail:", FontWeight.Normal, Alignment.Right, Decoration.Overlined));
			footerTable.AddCell(CreateCell("nor-credit@also.com", FontWeight.Normal, Alignment.Left, Decoration.Overlined));
			footerTable.AddCell(CreateCell("Postboks 2020, 3202 Sandefjord"));
			footerTable.AddCell(CreateCell("Phone:", FontWeight.Normal, Alignment.Right));
			footerTable.AddCell(CreateCell("+4733449660"));
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
