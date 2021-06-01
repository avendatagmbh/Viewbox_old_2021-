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
	public class KontoauszuegePdfExport : IPdfExporter
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

		public KontoauszuegePdfExport(CancellationToken token)
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
			List<string> headers = new List<string> { "Bu.Datum Bsl", "Betrag", "Text" };
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, defaultFontSize, 0, BaseColor.BLACK);
			Font normalRed = new Font(bf, defaultFontSize, 0, BaseColor.RED);
			Font headLineNormalGrey = new Font(bf, defaultFontSize, 0, BaseColor.GRAY);
			DataTable dbTable = new DataTable();
			dbTable.Load(DataReader);
			DataRow firstRow = dbTable.Rows[0];
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
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				firstRow[0].ToString(),
				"BD/Werber: " + firstRow[5].ToString() + "/" + firstRow[6].ToString(),
				firstRow[1].ToString(),
				"Pers.-Nr: " + firstRow[4].ToString(),
				firstRow[2].ToString() + " " + firstRow[3].ToString(),
				"",
				"",
				"Vertrag: " + firstRow[13].ToString(),
				"",
				"",
				"",
				"Hamburg, " + DateTime.Now.ToString("dd.MM.yyyy"),
				"",
				""
			};
			List<float> maxWidth = GetMaxWidthPoints(dbTable, normalBlack, headers, headLineNormalGrey, Columns, User);
			float maxHeight = normalBlack.CalculatedSize;
			PdfTraits traits = new PdfTraits("APKF Kontoauszüge", "APKF Kontoauszüge");
			traits.CreateTableOfContents = false;
			traits.PageSizeAndOrientation = PageSize.A4;
			traits.MarginLeft = 50f;
			traits.MarginRight = 50f;
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
			int row = contactTable.Rows.Count;
			int horizontalPage = 0;
			PdfPTable yearInitTable = new PdfPTable(1);
			yearInitTable.WidthPercentage = 100f;
			yearInitTable.HorizontalAlignment = 0;
			string[] years = optimizationTexts[3].Item2.Split(new string[1] { "und" }, StringSplitOptions.None);
			string[] months = optimizationTexts[4].Item2.Split(new string[1] { "und" }, StringSplitOptions.None);
			string timeInterval = $"{months[0].Trim()}/{years[0].Trim()} - {months[1].Trim()}/{years[1].Trim()}";
			PdfPCell initCell = new PdfPCell(new Phrase(string.Format("Provisionskonto - Abrechnung für {0} Zeitraum : {1}", firstRow[5].ToString() + "/" + firstRow[6].ToString(), timeInterval), normalBlack));
			initCell.Border = 2;
			yearInitTable.AddCell(initCell);
			generator.Document.Add(yearInitTable);
			int initYear = int.Parse(dbTable.Rows[0][7].ToString());
			int pages = 1;
			for (int i = 0; i < dbTable.Rows.Count; i++)
			{
				CancellationToken.ThrowIfCancellationRequested();
				if (row > 70)
				{
					row = 0;
					foreach (PdfPTable t in tables)
					{
						generator.Document.Add(t);
					}
					tables = GetTableList(headers, maxTableWidth, maxWidth, columnCount);
					generator.Document.NewPage();
					initYear = int.Parse(dbTable.Rows[i][7].ToString());
					yearInitTable = new PdfPTable(1);
					yearInitTable.WidthPercentage = 100f;
					yearInitTable.HorizontalAlignment = 0;
					initCell = new PdfPCell(new Phrase(string.Format("Provisionskonto - Abrechnung für {0} Zeitraum : {1}", firstRow[5].ToString() + "/" + firstRow[6].ToString(), timeInterval), normalBlack));
					initCell.Border = 2;
					yearInitTable.AddCell(initCell);
					generator.Document.Add(yearInitTable);
					row++;
					pages++;
				}
				int lastIndex = GetLastIndex(maxTableWidth, maxWidth, 0);
				horizontalPage = 0;
				PdfPTable table = tables[horizontalPage];
				if (dbTable.Rows[i][9].ToString() == "Provision")
				{
					table.AddCell(emptyDummyCell);
					table.AddCell(emptyDummyCell);
					table.AddCell(emptyDummyCell);
					PdfPCell placeHolderCell = new PdfPCell(new Phrase(string.Format("Saldo per {0}", new DateTime(int.Parse(dbTable.Rows[i][7].ToString()), int.Parse(dbTable.Rows[i][8].ToString()), 1).ToString("dd.MM.yyyy")), normalBlack));
					placeHolderCell.ExtraParagraphSpace = 0f;
					placeHolderCell.VerticalAlignment = 4;
					placeHolderCell.Border = 2;
					placeHolderCell.MinimumHeight = maxHeight;
					table.AddCell(placeHolderCell);
					table.AddCell(emptyDummyCell);
					table.AddCell(emptyDummyCell);
					row += 2;
				}
				string value = dbTable.Rows[i][9].ToString();
				PdfPCell cell = new PdfPCell(new Phrase(value, normalBlack));
				cell.ExtraParagraphSpace = 0f;
				cell.VerticalAlignment = 4;
				cell.Border = 0;
				cell.PaddingBottom = 0f;
				cell.PaddingTop = 0f;
				cell.MinimumHeight = maxHeight;
				table.AddCell(cell);
				string SHSpace = string.Empty;
				if (dbTable.Rows[i][9].ToString() != "Provision" && dbTable.Rows[i][9].ToString() != "Spesen" && dbTable.Rows[i][9].ToString() != "Sonstiges" && dbTable.Rows[i][9].ToString() != "Gesamtsaldo" && dbTable.Rows[i][9].ToString() != "davon Stornoreserve" && dbTable.Rows[i][9].ToString() != "Saldo Stornoreserve angerechnet")
				{
					SHSpace = ((dbTable.Rows[i][11].ToString() == "S") ? "        " : " ");
				}
				value = $"{dbTable.Rows[i][10]:#,0.00} " + SHSpace + $"{dbTable.Rows[i][11].ToString()}";
				cell = new PdfPCell(new Phrase(value, normalBlack));
				cell.ExtraParagraphSpace = 0f;
				cell.VerticalAlignment = 4;
				cell.HorizontalAlignment = 2;
				cell.Border = 0;
				cell.PaddingBottom = 0f;
				cell.PaddingTop = 0f;
				cell.MinimumHeight = maxHeight;
				table.AddCell(cell);
				value = "   " + dbTable.Rows[i][12].ToString();
				cell = new PdfPCell(new Phrase(value, normalBlack));
				cell.ExtraParagraphSpace = 0f;
				cell.VerticalAlignment = 4;
				cell.Border = 0;
				cell.PaddingBottom = 0f;
				cell.PaddingTop = 0f;
				cell.MinimumHeight = maxHeight;
				table.AddCell(cell);
				if (dbTable.Rows[i][9].ToString() == "Gesamtsaldo")
				{
					table.AddCell(emptyDummyCell);
					table.AddCell(emptyDummyCell);
					table.AddCell(emptyDummyCell);
					row++;
				}
				row++;
			}
			foreach (PdfPTable table2 in tables)
			{
				generator.Document.Add(table2);
				pages++;
			}
			generator.WriteFile(TargetStream, new Phrase(""), addPageHeader: false, addViewboxStyle: false, optimizationTexts, new Tuple<string, string>(Resources.Processor, User.GetName()), Resources.PdfPageFrom, horizontalPage + 1, Resources.Page, Resources.Part, Closing, TableObject.OptimizationHidden);
		}

		private List<float> GetMaxWidthPoints(DataTable table, Font font, List<string> headers, Font headLineFont, List<IColumn> columns, IUser user)
		{
			List<float> maxWidth = new List<float> { 0f, 0f, 0f };
			for (int j = 0; j < table.Rows.Count; j++)
			{
				string value = table.Rows[j][9].ToString();
				maxWidth[0] = Math.Max(maxWidth[0], font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(value, font.CalculatedSize) + 5f);
				value = $"{table.Rows[j][10]:#,0.00} {table.Rows[j][11].ToString()}";
				maxWidth[1] = Math.Max(maxWidth[0], font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(value, font.CalculatedSize) + 5f);
				value = table.Rows[j][12].ToString();
				maxWidth[2] = Math.Max(maxWidth[0], font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(value, font.CalculatedSize) + 5f);
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
			cellWidths[0] *= 1.35f;
			cellWidths[2] *= 1.75f;
			PdfPTable table = new PdfPTable(cellWidths);
			table.DefaultCell.Border = 0;
			table.SpacingBefore = 5f;
			table.SpacingAfter = 5f;
			table.WidthPercentage = widthPercentage;
			table.HeaderRows = 1;
			table.HorizontalAlignment = 0;
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 10f, 0, BaseColor.BLACK);
			for (int i = start; i < lastIndex; i++)
			{
				string name = ((headers.Count > i) ? headers[i] : "");
				PdfPCell cell = new PdfPCell(new Phrase(name, normalBlack));
				cell.Border = 2;
				cell.VerticalAlignment = 5;
				cell.BorderColor = BaseColor.LIGHT_GRAY;
				cell.HorizontalAlignment = 0;
				table.AddCell(cell);
			}
			return table;
		}
	}
}
