using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using SystemDb;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfGenerator;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Job.PdfExport.Basler
{
	public class ProvisionskontoPdfExport : IPdfExporter
	{
		private readonly CancellationToken token;

		public List<IColumn> Columns { get; set; }

		public ITableObject TableObject { get; set; }

		public CancellationToken CancellationToken { get; set; }

		public ILanguage Language { get; set; }

		public IOptimization Optimization { get; set; }

		public IUser User { get; set; }

		public IDataReader DataReader { get; set; }

		public Stream TargetStream { get; set; }

		public bool Closing { get; set; }

		public PdfExporterConfigElement ExporterConfig { get; set; }

		public bool HasGroupSubtotal { get; set; }

		public TableObjectCollection TempTableObjects { get; set; }

		public ProvisionskontoPdfExport(CancellationToken token)
		{
			this.token = token;
		}

		public void Init()
		{
		}

		public void Export()
		{
			List<Tuple<string, string>> optimizationTexts = Optimization.GetDescriptions(Language);
			optimizationTexts.Reverse();
			if (TempTableObjects != null)
			{
				SimpleTableModel temp = TempTableObjects.First().Additional as SimpleTableModel;
				if (temp != null)
				{
					optimizationTexts.AddRange(temp.GetParameters(TableObject.Id));
				}
			}
			int defaultFontSize = 9;
			List<string> headers = new List<string> { "Text", "Bu.Datum  Bsl", "Gutschrift", "Belastung", "Saldo" };
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, defaultFontSize, 0, BaseColor.BLACK);
			Font normalRed = new Font(bf, defaultFontSize, 0, BaseColor.RED);
			DataTable dbTable = new DataTable();
			dbTable.Load(DataReader);
			DataRow firstRow = dbTable.Rows[2];
			string personalNr = firstRow[0].ToString();
			string customerName = firstRow[1].ToString();
			string street = firstRow[2].ToString();
			string postCode = firstRow[3].ToString();
			string city = firstRow[4].ToString();
			string bdWerber = $"{firstRow[5].ToString()}/{firstRow[6].ToString()}";
			string fromDate = $"{optimizationTexts[2].Item2}/{optimizationTexts[3].Item2.PadLeft(2, '0')}";
			string toDate = $"{optimizationTexts[4].Item2}/{optimizationTexts[5].Item2}";
			List<string> contactInfo = new List<string>
			{
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"Basler Lebensversicherungs-AG, 22797 Hamburg",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				customerName,
				"",
				street,
				"Pers.-Nr: " + personalNr,
				postCode + " " + city,
				"",
				"",
				"",
				"",
				"Hamburg, " + DateTime.Now.ToString("dd.MM.yyyy"),
				"",
				""
			};
			List<float> maxWidth = GetMaxWidthPoints(dbTable, normalBlack, headers, normalBlack, Columns, User);
			float maxHeight = normalBlack.CalculatedSize;
			PdfTraits traits = new PdfTraits("APKF Provisionskonto", "APKF Provisionskonto");
			traits.CreateTableOfContents = false;
			traits.PageSizeAndOrientation = PageSize.A4;
			traits.MarginLeft = 40f;
			traits.MarginRight = 40f;
			traits.MarginTop = 50f;
			traits.MarginBottom = 50f;
			float maxTableWidth = traits.PageSizeAndOrientation.Width - traits.MarginLeft - traits.MarginRight;
			float maxTableHeight = traits.PageSizeAndOrientation.Height - traits.MarginTop - traits.MarginBottom;
			int columnCount = 3;
			List<PdfPTable> tables = GetTableList(headers, maxTableWidth, maxWidth, columnCount);
			global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
			PdfPCell emptyDummyCell = new PdfPCell(new Phrase(string.Format("", normalBlack)));
			emptyDummyCell.Border = 0;
			emptyDummyCell.PaddingBottom = 5f;
			PdfPTable contactTable = new PdfPTable(2);
			contactTable.HorizontalAlignment = 0;
			contactTable.WidthPercentage = 100f;
			foreach (string info in contactInfo)
			{
				PdfPCell contactCell = new PdfPCell(new Phrase(info, normalBlack));
				contactCell.Border = 0;
				contactTable.AddCell(contactCell);
			}
			generator.Document.Add(contactTable);
			generator.Document.NewPage();
			int horizontalPage = 0;
			PdfPTable yearInitTable = new PdfPTable(1);
			yearInitTable.WidthPercentage = 100f;
			yearInitTable.HorizontalAlignment = 0;
			PdfPCell initCell = new PdfPCell(new Phrase(string.Format("Provisionskonto - Zusammenfassung von: {0} bis : {1} BD/Werber: {2}" + Environment.NewLine + "A N S P R U E C H E", fromDate, toDate, bdWerber), normalBlack));
			initCell.Border = 0;
			initCell.VerticalAlignment = 5;
			initCell.PaddingBottom = 0f;
			yearInitTable.AddCell(initCell);
			generator.Document.Add(yearInitTable);
			int pages = 2;
			int row = 0;
			string lastHeader = "";
			for (int j = 0; j < dbTable.Rows.Count; j++)
			{
				if (!(dbTable.Rows[j][5].ToString() != "999") || !(dbTable.Rows[j][6].ToString() != "9999"))
				{
					continue;
				}
				CancellationToken.ThrowIfCancellationRequested();
				if (row > 55)
				{
					row = 0;
					foreach (PdfPTable t2 in tables)
					{
						generator.Document.Add(t2);
					}
					tables = GetTableList(headers, maxTableWidth, maxWidth, columnCount);
					generator.Document.NewPage();
					yearInitTable = new PdfPTable(1);
					yearInitTable.WidthPercentage = 100f;
					yearInitTable.HorizontalAlignment = 0;
					initCell = ((!lastHeader.Contains("A N S P R U E C H E") && !(lastHeader == string.Empty)) ? new PdfPCell(new Phrase(string.Format("Provisionskonto - Zusammenfassung von: {0} bis : {1} BD/Werber: {2}" + Environment.NewLine + "A U S Z A H L U N G E N", fromDate, toDate, bdWerber), normalBlack)) : new PdfPCell(new Phrase(string.Format("Provisionskonto - Zusammenfassung von: {0} bis : {1} BD/Werber: {2}" + Environment.NewLine + "A N S P R U E C H E", fromDate, toDate, bdWerber), normalBlack)));
					initCell.Border = 0;
					yearInitTable.AddCell(initCell);
					generator.Document.Add(yearInitTable);
					row++;
					pages++;
				}
				bool isSum = dbTable.Rows[j][0].ToString() == string.Empty;
				string value = dbTable.Rows[j][9].ToString();
				int lastIndex = GetLastIndex(maxTableWidth, maxWidth, 0);
				horizontalPage = 0;
				PdfPTable table = tables[horizontalPage];
				if (value.Contains("ANSPRÜCHE VP GEGENÜBER DR") || value.Contains("AUSZAHLUNG VON DR AN VP"))
				{
					PdfPCell dummyCell = new PdfPCell(new Phrase(string.Empty, normalBlack));
					dummyCell.PaddingBottom = 2f;
					dummyCell.MinimumHeight = maxHeight;
					dummyCell.Border = 2;
					for (int k = 0; k < 5; k++)
					{
						table.AddCell(dummyCell);
					}
				}
				bdWerber = $"{dbTable.Rows[j][5].ToString()}/{dbTable.Rows[j][6].ToString()}";
				PdfPCell cell = new PdfPCell(new Phrase(value, normalBlack));
				cell.ExtraParagraphSpace = 0f;
				cell.VerticalAlignment = 4;
				cell.Border = 0;
				cell.PaddingBottom = (isSum ? 5 : 2);
				cell.PaddingTop = (isSum ? 5 : 0);
				cell.MinimumHeight = maxHeight;
				table.AddCell(cell);
				value = dbTable.Rows[j][10].ToString() + " " + dbTable.Rows[j][11].ToString();
				cell = new PdfPCell(new Phrase(value, normalBlack));
				cell.ExtraParagraphSpace = 0f;
				cell.VerticalAlignment = 4;
				cell.Border = 0;
				cell.PaddingBottom = (isSum ? 5 : 2);
				cell.PaddingTop = (isSum ? 5 : 0);
				cell.MinimumHeight = maxHeight;
				table.AddCell(cell);
				value = $"{dbTable.Rows[j][12]:#,0.00} " + $"{dbTable.Rows[j][13].ToString()}";
				cell = new PdfPCell(new Phrase(value, normalBlack));
				cell.ExtraParagraphSpace = 0f;
				cell.VerticalAlignment = 4;
				cell.HorizontalAlignment = 2;
				cell.Border = (isSum ? 1 : 0);
				cell.PaddingBottom = (isSum ? 5 : 2);
				cell.PaddingTop = (isSum ? 5 : 0);
				cell.MinimumHeight = maxHeight;
				table.AddCell(cell);
				value = $"{dbTable.Rows[j][14]:#,0.00} " + $"{dbTable.Rows[j][15].ToString()}";
				cell = new PdfPCell(new Phrase(value, normalBlack));
				cell.ExtraParagraphSpace = 0f;
				cell.VerticalAlignment = 4;
				cell.HorizontalAlignment = 2;
				cell.Border = (isSum ? 1 : 0);
				cell.PaddingBottom = (isSum ? 5 : 2);
				cell.PaddingTop = (isSum ? 5 : 0);
				cell.MinimumHeight = maxHeight;
				table.AddCell(cell);
				value = $"{dbTable.Rows[j][16]:#,0.00} " + $"{dbTable.Rows[j][17].ToString()}";
				cell = new PdfPCell(new Phrase(value, normalBlack));
				cell.ExtraParagraphSpace = 0f;
				cell.VerticalAlignment = 4;
				cell.HorizontalAlignment = 2;
				cell.Border = (isSum ? 1 : 0);
				cell.PaddingBottom = (isSum ? 5 : 2);
				cell.PaddingTop = (isSum ? 5 : 0);
				cell.MinimumHeight = maxHeight;
				table.AddCell(cell);
				value = dbTable.Rows[j][9].ToString();
				string nextDesc = dbTable.Rows[j + 1][9].ToString();
				if (value.Contains("ANSPRÜCHE VP GEGENÜBER DR") && !nextDesc.Contains("SALDO ANSPRÜCHE ABZÜGL. AUSZAHLUNG PRO BD/WB"))
				{
					bdWerber = $"{dbTable.Rows[j + 1][5].ToString()}/{dbTable.Rows[j + 1][6].ToString()}";
					yearInitTable = new PdfPTable(1);
					yearInitTable.WidthPercentage = 100f;
					yearInitTable.HorizontalAlignment = 0;
					initCell = new PdfPCell(new Phrase(string.Format("Provisionskonto - Zusammenfassung von: {0} bis : {1} BD/Werber: {2}" + Environment.NewLine + "A U S Z A H L U N G E N", fromDate, toDate, bdWerber), normalBlack));
					initCell.Border = 0;
					initCell.VerticalAlignment = 5;
					initCell.PaddingBottom = 0f;
					yearInitTable.AddCell(initCell);
					lastHeader = "A U S Z A H L U N G E N";
				}
				else if (value.Contains("AUSZAHLUNG VON DR AN VP") || nextDesc.Contains("SALDO ANSPRÜCHE ABZÜGL. AUSZAHLUNG PRO BD/WB"))
				{
					bdWerber = $"{dbTable.Rows[j + 1][5].ToString()}/{dbTable.Rows[j + 1][6].ToString()}";
					yearInitTable = new PdfPTable(1);
					yearInitTable.WidthPercentage = 100f;
					yearInitTable.HorizontalAlignment = 0;
					initCell = new PdfPCell(new Phrase(string.Format("Provisionskonto - Zusammenfassung von: {0} bis : {1} BD/Werber: {2}" + Environment.NewLine + "F O R D E R U N G", fromDate, toDate, bdWerber), normalBlack));
					initCell.Border = 0;
					initCell.VerticalAlignment = 5;
					initCell.PaddingBottom = 0f;
					yearInitTable.AddCell(initCell);
				}
				else if (value.Contains("SALDO ANSPRÜCHE ABZÜGL. AUSZAHLUNG PRO BD/WB"))
				{
					if (dbTable.Rows.Count > j + 1)
					{
						bdWerber = $"{dbTable.Rows[j + 1][5].ToString()}/{dbTable.Rows[j + 1][6].ToString()}";
					}
					yearInitTable = new PdfPTable(1);
					yearInitTable.WidthPercentage = 100f;
					yearInitTable.HorizontalAlignment = 0;
					initCell = new PdfPCell(new Phrase(string.Format("Provisionskonto - Zusammenfassung von: {0} bis : {1} BD/Werber: {2}" + Environment.NewLine + "A N S P R U E C H E", fromDate, toDate, bdWerber), normalBlack));
					initCell.Border = 0;
					initCell.VerticalAlignment = 5;
					initCell.PaddingBottom = 0f;
					yearInitTable.AddCell(initCell);
					lastHeader = "A N S P R U E C H E";
				}
				if ((value.Contains("ANSPRÜCHE VP GEGENÜBER DR") || value.Contains("AUSZAHLUNG VON DR AN VP") || value.Contains("SALDO ANSPRÜCHE ABZÜGL. AUSZAHLUNG PRO BD/WB")) && dbTable.Rows.Count - 1 > j)
				{
					row = 0;
					bdWerber = $"{dbTable.Rows[j + 1][5].ToString()}/{dbTable.Rows[j + 1][6].ToString()}";
					foreach (PdfPTable t in tables)
					{
						generator.Document.Add(t);
					}
					tables = GetTableList(headers, maxTableWidth, maxWidth, columnCount);
					generator.Document.NewPage();
					if (bdWerber != "999/9999")
					{
						generator.Document.Add(yearInitTable);
					}
					pages++;
				}
				row++;
			}
			foreach (PdfPTable t4 in tables)
			{
				generator.Document.Add(t4);
			}
			tables = GetTableList(headers, maxTableWidth, maxWidth, columnCount);
			generator.Document.NewPage();
			PdfPTable to = tables[horizontalPage];
			yearInitTable = new PdfPTable(1);
			yearInitTable.WidthPercentage = 100f;
			yearInitTable.HorizontalAlignment = 0;
			initCell = new PdfPCell(new Phrase(string.Format("Provisionskonto - Zusammenfassung von: {0} bis : {1} BD/Werber: 999 9999" + Environment.NewLine + "F O R D E R U N G", fromDate, toDate), normalBlack));
			initCell.Border = 0;
			initCell.VerticalAlignment = 5;
			initCell.PaddingBottom = 0f;
			yearInitTable.AddCell(initCell);
			generator.Document.Add(yearInitTable);
			for (int i = 0; i < dbTable.Rows.Count; i++)
			{
				if (dbTable.Rows[i][5].ToString() == "999" && dbTable.Rows[i][6].ToString() == "9999")
				{
					bool isSum2 = i + 1 == dbTable.Rows.Count;
					string value2 = dbTable.Rows[i][9].ToString();
					int lastIndex2 = GetLastIndex(maxTableWidth, maxWidth, 0);
					horizontalPage = 0;
					PdfPTable table2 = tables[horizontalPage];
					PdfPCell cell2 = new PdfPCell(new Phrase(value2, normalBlack));
					cell2.ExtraParagraphSpace = 0f;
					cell2.VerticalAlignment = 4;
					cell2.Border = 0;
					cell2.PaddingBottom = (isSum2 ? 5 : 2);
					cell2.PaddingTop = (isSum2 ? 5 : 0);
					cell2.MinimumHeight = maxHeight;
					table2.AddCell(cell2);
					value2 = dbTable.Rows[i][10].ToString() + " " + dbTable.Rows[i][11].ToString();
					cell2 = new PdfPCell(new Phrase(value2, normalBlack));
					cell2.ExtraParagraphSpace = 0f;
					cell2.VerticalAlignment = 4;
					cell2.Border = 0;
					cell2.PaddingBottom = (isSum2 ? 5 : 2);
					cell2.PaddingTop = (isSum2 ? 5 : 0);
					cell2.MinimumHeight = maxHeight;
					table2.AddCell(cell2);
					value2 = $"{dbTable.Rows[i][12]:#,0.00} " + $"{dbTable.Rows[i][13].ToString()}";
					cell2 = new PdfPCell(new Phrase(value2, normalBlack));
					cell2.ExtraParagraphSpace = 0f;
					cell2.VerticalAlignment = 4;
					cell2.HorizontalAlignment = 2;
					cell2.Border = (isSum2 ? 1 : 0);
					cell2.PaddingBottom = (isSum2 ? 5 : 2);
					cell2.PaddingTop = (isSum2 ? 5 : 0);
					cell2.MinimumHeight = maxHeight;
					table2.AddCell(cell2);
					value2 = $"{dbTable.Rows[i][14]:#,0.00} " + $"{dbTable.Rows[i][15].ToString()}";
					cell2 = new PdfPCell(new Phrase(value2, normalBlack));
					cell2.ExtraParagraphSpace = 0f;
					cell2.VerticalAlignment = 4;
					cell2.HorizontalAlignment = 2;
					cell2.Border = (isSum2 ? 1 : 0);
					cell2.PaddingBottom = (isSum2 ? 5 : 2);
					cell2.PaddingTop = (isSum2 ? 5 : 0);
					cell2.MinimumHeight = maxHeight;
					table2.AddCell(cell2);
					value2 = $"{dbTable.Rows[i][16]:#,0.00} " + $"{dbTable.Rows[i][17].ToString()}";
					cell2 = new PdfPCell(new Phrase(value2, normalBlack));
					cell2.ExtraParagraphSpace = 0f;
					cell2.VerticalAlignment = 4;
					cell2.HorizontalAlignment = 2;
					cell2.Border = (isSum2 ? 1 : 0);
					cell2.PaddingBottom = (isSum2 ? 5 : 2);
					cell2.PaddingTop = (isSum2 ? 5 : 0);
					cell2.MinimumHeight = maxHeight;
					table2.AddCell(cell2);
				}
			}
			pages++;
			foreach (PdfPTable t3 in tables)
			{
				generator.Document.Add(t3);
				pages++;
			}
			generator.WriteFile(TargetStream, new Phrase(""), addPageHeader: false, addViewboxStyle: false, optimizationTexts, new Tuple<string, string>(Resources.Processor, User.GetName()), Resources.PdfPageFrom, horizontalPage + 1, Resources.Page, Resources.Part, Closing, TableObject.OptimizationHidden);
		}

		private List<float> GetMaxWidthPoints(DataTable table, Font font, List<string> headers, Font headLineFont, List<IColumn> columns, IUser user)
		{
			List<float> maxWidth = new List<float> { 0f, 0f, 0f, 0f, 0f };
			for (int j = 0; j < table.Rows.Count; j++)
			{
				string value = table.Rows[j][9].ToString();
				maxWidth[0] = Math.Min(Math.Max(maxWidth[0], font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(value, font.CalculatedSize) + 5f), 230f);
				value = $"{table.Rows[j][10]} {table.Rows[j][11].ToString()}";
				maxWidth[1] = Math.Min(Math.Max(maxWidth[1], font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(value, font.CalculatedSize) + 5f), 230f);
				value = $"{table.Rows[j][12]:#,0.00} {table.Rows[j][13].ToString()}";
				maxWidth[2] = Math.Min(Math.Max(maxWidth[2], font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(value, font.CalculatedSize) + 5f), 230f);
				value = $"{table.Rows[j][14]:#,0.00} {table.Rows[j][15].ToString()}";
				maxWidth[3] = Math.Min(Math.Max(maxWidth[3], font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(value, font.CalculatedSize) + 5f), 230f);
				value = $"{table.Rows[j][16]:#,0.00} {table.Rows[j][17].ToString()}";
				maxWidth[4] = Math.Min(Math.Max(maxWidth[4], font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(value, font.CalculatedSize) + 5f), 230f);
			}
			for (int i = 0; i < headers.Count; i++)
			{
				maxWidth[i] = Math.Max(maxWidth[i], headLineFont.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(headers[i], headLineFont.CalculatedSize) + 5f);
			}
			return maxWidth;
		}

		private int GetLastIndex(float maxTableWidth, List<float> maxWidth, int start)
		{
			float currentWidth = 0f;
			for (int i = start; i < maxWidth.Count; i++)
			{
				if (currentWidth + maxWidth[i] > maxTableWidth)
				{
					return i;
				}
				currentWidth += maxWidth[i];
			}
			return maxWidth.Count;
		}

		private List<PdfPTable> GetTableList(List<string> headers, float maxTableWidth, List<float> maxWidth, int columnCount)
		{
			int lastIndex = GetLastIndex(maxTableWidth, maxWidth, 0);
			float[] cellWidth = maxWidth.GetRange(0, (lastIndex <= 0) ? 1 : lastIndex).ToArray();
			List<PdfPTable> tables = new List<PdfPTable> { CreateTableWithHeader(headers, isOverviewPdf: false, cellWidth, lastIndex) };
			for (int i = 0; i < columnCount; i++)
			{
				if (i > lastIndex - 1)
				{
					lastIndex = GetLastIndex(maxTableWidth, maxWidth, i);
					cellWidth = maxWidth.GetRange(i, (lastIndex - i <= 0) ? 1 : (lastIndex - i)).ToArray();
					tables.Add(Viewbox.Job.Export.CreateTableWithHeader(headers, isOverviewPdf: false, cellWidth, lastIndex, i));
				}
			}
			return tables;
		}

		private PdfPTable CreateTableWithHeader(List<string> headers, bool isOverviewPdf = false, float[] cellWidths = null, int lastIndex = 0, int start = 0, float widthPercentage = 100f)
		{
			PdfPTable table = new PdfPTable(cellWidths);
			table.DefaultCell.Border = 0;
			table.SpacingBefore = 5f;
			table.SpacingAfter = 5f;
			table.WidthPercentage = widthPercentage;
			table.HeaderRows = 1;
			table.HorizontalAlignment = 0;
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 9f, 0, BaseColor.BLACK);
			for (int i = start; i < lastIndex; i++)
			{
				string name = ((headers.Count > i) ? headers[i] : "");
				PdfPCell cell = new PdfPCell(new Phrase(name, normalBlack));
				cell.Border = 3;
				cell.VerticalAlignment = 5;
				cell.BorderColor = BaseColor.BLACK;
				cell.HorizontalAlignment = 0;
				table.AddCell(cell);
			}
			return table;
		}
	}
}
