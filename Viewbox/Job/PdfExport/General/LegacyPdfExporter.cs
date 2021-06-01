using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using SystemDb;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfGenerator;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Job.PdfExport.General
{
	public class LegacyPdfExporter : IPdfExporter
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

		public LegacyPdfExporter(CancellationToken token)
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
					optimizationTexts.AddRange(temp.GetParameters(TempTableObjects.First().OriginalTable.Id));
				}
			}
			int defaultFontSize = 12;
			try
			{
				IProperty property2 = ViewboxApplication.FindProperty(User, "defaultPDFfontSize");
				if (property2 != null && !int.TryParse(property2.Value, out defaultFontSize))
				{
					defaultFontSize = 12;
				}
			}
			catch
			{
			}
			if (defaultFontSize < 5)
			{
				defaultFontSize = 5;
			}
			else if (defaultFontSize > 20)
			{
				defaultFontSize = 20;
			}
			float multiplier = 0f;
			switch (defaultFontSize)
			{
			case 5:
				multiplier = 2f;
				break;
			case 6:
				multiplier = 1.8f;
				break;
			case 7:
				multiplier = 1.7f;
				break;
			case 8:
				multiplier = 1.6f;
				break;
			case 9:
				multiplier = 1.6f;
				break;
			case 10:
				multiplier = 1.5f;
				break;
			case 11:
				multiplier = 1.5f;
				break;
			case 12:
				multiplier = 1.4f;
				break;
			case 13:
				multiplier = 1.4f;
				break;
			case 14:
				multiplier = 1.3f;
				break;
			case 15:
				multiplier = 1.3f;
				break;
			case 16:
				multiplier = 1.3f;
				break;
			case 17:
				multiplier = 1.3f;
				break;
			case 18:
				multiplier = 1.2f;
				break;
			case 19:
				multiplier = 1.2f;
				break;
			case 20:
				multiplier = 1.2f;
				break;
			}
			List<string> headers = Columns.Select((IColumn c) => c.GetDescription(Language)).ToList();
			string fontPath = HttpRuntime.AppDomainAppPath + "Content\\fonts\\ARIALUNI.TTF";
			BaseFont bf = BaseFont.CreateFont(fontPath, "Identity-H", embedded: true);
			Font normalBlack = new Font(bf, defaultFontSize, 0, BaseColor.BLACK);
			Font normalRed = new Font(bf, defaultFontSize, 0, BaseColor.RED);
			Font headLineNormalGrey = new Font(bf, defaultFontSize, 0, BaseColor.GRAY);
			DataTable dbTable = new DataTable();
			using (DataSet ds = new DataSet
			{
				EnforceConstraints = false
			})
			{
				ds.Tables.Add(dbTable);
				dbTable.Load(DataReader, LoadOption.OverwriteChanges);
				ds.Tables.Remove(dbTable);
			}
			List<float> maxWidth = GetMaxWidthPoints(dbTable, normalBlack, headers, headLineNormalGrey, Columns, User);
			float maxHeight = normalBlack.CalculatedSize * multiplier;
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.PageSizeAndOrientation = PageSize.A4.Rotate();
			traits.MarginLeft = 60f;
			traits.MarginRight = 60f;
			traits.MarginTop = 100f;
			traits.MarginBottom = 60f;
			if (optimizationTexts.Count > 5)
			{
				traits.MarginTop += (optimizationTexts.Count - 5) * 15;
			}
			float maxTableWidth = traits.PageSizeAndOrientation.Width - traits.MarginLeft - traits.MarginRight;
			float maxTableHeight = traits.PageSizeAndOrientation.Height - traits.MarginTop - traits.MarginBottom;
			int columnCount = dbTable.Columns.Count;
			if (HasGroupSubtotal)
			{
				columnCount = Columns.Count;
			}
			List<PdfPTable> tables = GetTableList(headers, maxTableWidth, maxWidth, columnCount);
			global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
			int row = 2;
			int horizontalPage = 0;
			for (int j = 0; j < dbTable.Rows.Count; j++)
			{
				CancellationToken.ThrowIfCancellationRequested();
				if ((float)row * (maxHeight + 10f) > maxTableHeight)
				{
					foreach (PdfPTable t in tables)
					{
						generator.Document.Add(t);
						generator.Document.NewPage();
					}
					tables = GetTableList(headers, maxTableWidth, maxWidth, columnCount);
					row = 2;
				}
				int lastIndex = GetLastIndex(maxTableWidth, maxWidth, 0);
				horizontalPage = 0;
				PdfPTable table = tables[horizontalPage];
				for (int k = 0; k < columnCount; k++)
				{
					CancellationToken.ThrowIfCancellationRequested();
					if (k > lastIndex - 1)
					{
						lastIndex = GetLastIndex(maxTableWidth, maxWidth, k);
						table = tables[++horizontalPage];
					}
					string value = dbTable.Rows[j][k].ToString();
					bool redColor = false;
					if (dbTable.Rows[j][k] == DBNull.Value)
					{
						value = "";
					}
					else if (dbTable.Rows[j][k].GetType() == typeof(byte[]))
					{
						byte[] array = dbTable.Rows[j][k] as byte[];
						value = string.Empty;
						for (int l = 0; l < array.Length; l++)
						{
							if (l > 0)
							{
								value += ((l % 16 > 0) ? " " : Environment.NewLine);
							}
							value += $"{array[l]:X2}";
						}
					}
					else if (SqlType.String < (Columns[k].DataType & SqlType.Numeric))
					{
						if (value.StartsWith("-"))
						{
							redColor = true;
						}
						if (Columns[k].DataType == SqlType.Decimal)
						{
							IProperty property = ViewboxApplication.FindProperty(User, "thousand");
							string decimals = new string('0', Columns[k].MaxLength);
							string format = "{0:#,0." + decimals + "}";
							if (property != null && property.Value == "false")
							{
								format = "{0:#0." + decimals + "}";
							}
							value = string.Format(format, dbTable.Rows[j][k]);
						}
					}
					PdfPCell cell = new PdfPCell(new Phrase(value, redColor ? normalRed : normalBlack));
					cell.ExtraParagraphSpace = 5f;
					cell.VerticalAlignment = 4;
					if (k == lastIndex - 1)
					{
						cell.Border = 2;
					}
					else
					{
						cell.Border = 10;
					}
					cell.BorderColor = BaseColor.LIGHT_GRAY;
					cell.PaddingBottom = 5f;
					cell.PaddingTop = 5f;
					cell.MinimumHeight = maxHeight;
					table.AddCell(cell);
				}
				row++;
			}
			if (row == 2)
			{
				foreach (PdfPTable table3 in tables)
				{
					for (int i = 0; i < table3.NumberOfColumns; i++)
					{
						table3.AddCell(" ");
					}
				}
			}
			foreach (PdfPTable table2 in tables)
			{
				generator.Document.Add(table2);
				generator.Document.NewPage();
			}
			bool bukrs = ((TempTableObjects != null) ? (TempTableObjects.First().OriginalTable.OptimizationHidden == 0) : (TableObject.OptimizationHidden == 0));
			bool gjahr = ((TempTableObjects != null) ? (TempTableObjects.First().OriginalTable.OptimizationHidden == 0) : (TableObject.OptimizationHidden == 0));
			foreach (Tuple<string, string> opt in optimizationTexts)
			{
				if (TableObject != null && TableObject.OrderAreas.FirstOrDefault((IOrderArea x) => x.SplitValue == opt.Item2) != null)
				{
					bukrs = true;
				}
				if (TableObject != null && TableObject.OrderAreas.FirstOrDefault((IOrderArea x) => x.SortValue == opt.Item2) != null)
				{
					gjahr = true;
				}
				if (TempTableObjects != null && TempTableObjects.First().Table.OrderAreas.FirstOrDefault((IOrderArea x) => x.SplitValue == opt.Item2) != null)
				{
					bukrs = true;
				}
				if (TempTableObjects != null && TempTableObjects.First().Table.OrderAreas.FirstOrDefault((IOrderArea x) => x.SortValue == opt.Item2) != null)
				{
					gjahr = true;
				}
			}
			generator.WriteFile(TargetStream, new Phrase(Resources.Table + ": " + TableObject.GetDescription(Language)), addPageHeader: false, addViewboxStyle: true, optimizationTexts, new Tuple<string, string>(Resources.Processor, User.GetName()), Resources.PdfPageFrom, horizontalPage + 1, Resources.Page, Resources.Part, Closing, TableObject.OptimizationHidden, bukrs, gjahr);
		}

		private List<float> GetMaxWidthPoints(DataTable table, Font font, List<string> headers, Font headLineFont, List<IColumn> columns, IUser user)
		{
			List<float> maxWidth = new List<float>();
			int columnCount = table.Columns.Count;
			if (HasGroupSubtotal)
			{
				columnCount = columns.Count;
			}
			for (int i = 0; i < columnCount; i++)
			{
				float width = 0f;
				for (int k = 0; k < table.Rows.Count; k++)
				{
					string value = table.Rows[k][i].ToString();
					if (table.Rows[k][i] == DBNull.Value)
					{
						value = "";
					}
					else if (table.Rows[k][i].GetType() == typeof(byte[]))
					{
						byte[] array = table.Rows[k][i] as byte[];
						value = string.Empty;
						for (int l = 0; l < array.Length; l++)
						{
							if (l > 0)
							{
								value += ((l % 16 > 0) ? " " : Environment.NewLine);
							}
							value += $"{array[l]:X2}";
						}
					}
					else if (SqlType.String < (columns[i].DataType & SqlType.Numeric) && columns[i].DataType == SqlType.Decimal)
					{
						IProperty property = ViewboxApplication.FindProperty(user, "thousand");
						string decimals = new string('0', columns[i].MaxLength);
						string format = "{0:#,0." + decimals + "}";
						if (property != null && property.Value == "false")
						{
							format = "{0:#0." + decimals + "}";
						}
						value = string.Format(format, table.Rows[k][i]);
					}
					string[] parts = value.Split('\n');
					string[] array2 = parts;
					foreach (string p in array2)
					{
						width = Math.Max(width, font.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(p, font.CalculatedSize) + 5f);
					}
				}
				maxWidth.Add(width);
			}
			for (int j = 0; j < headers.Count; j++)
			{
				maxWidth[j] = Math.Max(maxWidth[j], headLineFont.GetCalculatedBaseFont(specialEncoding: true).GetWidthPoint(headers[j], headLineFont.CalculatedSize) + 5f);
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
			List<PdfPTable> tables = new List<PdfPTable> { Viewbox.Job.Export.CreateTableWithHeader(headers, isOverviewPdf: false, cellWidth, lastIndex) };
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
	}
}
