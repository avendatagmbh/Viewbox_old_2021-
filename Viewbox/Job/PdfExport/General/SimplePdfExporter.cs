using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ViewboxDb;

namespace Viewbox.Job.PdfExport.General
{
	public class SimplePdfExporter : IPdfExporter
	{
		private const string Zeile = "zeile_int";

		public Stream TargetStream { get; set; }

		public bool Closing { get; set; }

		public IDataReader DataReader { get; set; }

		public PdfExporterConfigElement ExporterConfig { get; set; }

		public bool HasGroupSubtotal { get; set; }

		public TableObjectCollection TempTableObjects { get; set; }

		public void Export()
		{
			if (string.IsNullOrEmpty(ExporterConfig.ExtInfo))
			{
				ExporterConfig.ExtInfo = "zeile_int";
			}
			IEnumerable<string> lines = ReadLines();
			IEnumerable<IEnumerable<string>> pages = CreatePages(lines);
			CreatePdfDocument(pages);
		}

		public void Init()
		{
			DataTable dataTable = new DataTable();
			dataTable.Load(DataReader);
			DataColumn fileColumn = dataTable.Columns[ExporterConfig.ExtInfo5];
			if (fileColumn == null)
			{
				return;
			}
			using IEnumerator<string> enumerator = (from row in dataTable.AsEnumerable()
				select row[fileColumn].ToString()).GetEnumerator();
			if (enumerator.MoveNext())
			{
				string row2 = enumerator.Current;
				ExporterConfig.OutInfo1 = row2;
			}
		}

		private IEnumerable<string> ReadLines()
		{
			DataTable dataTable = new DataTable();
			dataTable.Load(DataReader);
			DataColumn column = dataTable.Columns[ExporterConfig.ExtInfo];
			if (column != null)
			{
				return from row in dataTable.AsEnumerable()
					select row[column].ToString();
			}
			throw new ApplicationException($"The {ExporterConfig.ExtInfo} column is not available!");
		}

		private IEnumerable<IEnumerable<string>> CreatePages(IEnumerable<string> lines)
		{
			List<List<string>> pages = new List<List<string>>();
			List<string> pageBuilder = new List<string>();
			bool firstSiteFound = false;
			foreach (string line in lines)
			{
				if (line.Contains("Seite") && !firstSiteFound)
				{
					firstSiteFound = true;
				}
				else if (line.Contains("Seite") && firstSiteFound)
				{
					pages.Add(pageBuilder);
					pageBuilder = new List<string>();
				}
				string tempLine = line;
				if (!"false".Equals(ExporterConfig.ExtInfo2))
				{
					tempLine = tempLine.Replace("?&a0L", "     ");
					tempLine = tempLine.Replace("?&k11.7H", "        ");
					tempLine = tempLine.Replace("°999", "");
					string result = "";
					Regex regex = new Regex("[\\?][\\&][a](\\d{4})[C]");
					while (regex.IsMatch(tempLine))
					{
						Match match = regex.Match(tempLine);
						result += tempLine.Substring(0, match.Index).Trim();
						if (int.TryParse(match.Groups[1].Value, out var padding))
						{
							result = result.PadRight(padding, ' ');
						}
						tempLine = tempLine.Substring(match.Index + 8);
					}
					result += tempLine;
					tempLine = result.TrimEnd();
					if (tempLine.Length > 81)
					{
						tempLine = tempLine.Remove(81);
					}
				}
				pageBuilder.Add(tempLine);
			}
			pages.Add(pageBuilder);
			return pages;
		}

		private void CreatePdfDocument(IEnumerable<IEnumerable<string>> pages)
		{
			if (!float.TryParse(ExporterConfig.ExtInfo3, NumberStyles.Float, CultureInfo.InvariantCulture, out var fontSize) && !float.TryParse(ExporterConfig.ExtInfo3, out fontSize))
			{
				fontSize = 9.7f;
			}
			Font Font = FontFactory.GetFont("Courier", fontSize);
			if (!float.TryParse(ExporterConfig.ExtInfo4, NumberStyles.Float, CultureInfo.InvariantCulture, out var marginLeft) && !float.TryParse(ExporterConfig.ExtInfo4, out marginLeft))
			{
				marginLeft = 30f;
			}
			Document pdfDocument = new Document(PageSize.A4, marginLeft, 0f, 60f, 0f);
			PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDocument, TargetStream);
			pdfDocument.Open();
			pdfDocument.AddAuthor("Viewbox");
			pdfDocument.AddCreator("Viewbox");
			pdfDocument.AddCreationDate();
			foreach (IEnumerable<string> page in pages)
			{
				foreach (string line in page)
				{
					Paragraph par = new Paragraph(line, Font);
					pdfDocument.Add(par);
				}
				pdfDocument.NewPage();
			}
			pdfDocument.Close();
			pdfWriter.Close();
		}
	}
}
