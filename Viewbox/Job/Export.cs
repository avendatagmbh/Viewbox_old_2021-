using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using SystemDb;
using SystemDb.Internal;
using DbAccess;
using Ionic.Zip;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfGenerator;
using Viewbox.Job.PdfExport;
using Viewbox.Models;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Job
{
	public class Export : Base
	{
		public enum UserDefinedExportProjects
		{
			MKN,
			Actebis,
			HofmeisterRechnung,
			HofmeisterLieferschein,
			HofmeisterSofortRechnung
		}

		private struct StructuredLevelNode
		{
			public readonly int level;

			public readonly string levelString;

			public readonly StructuedTableModel.Node node;

			public StructuredLevelNode(StructuedTableModel.Node node, int level, string levelString)
			{
				this.node = node;
				this.level = level;
				this.levelString = levelString;
			}
		}

		private struct DcwLevelNode
		{
			public readonly int level;

			public readonly string levelString;

			public readonly DcwBalanceModel.DcwNode node;

			public DcwLevelNode(DcwBalanceModel.DcwNode node, int level, string levelString)
			{
				this.node = node;
				this.level = level;
				this.levelString = levelString;
			}
		}

		private struct LevelNode
		{
			public readonly int level;

			public readonly string levelString;

			public readonly SapBalanceModel.Node node;

			public LevelNode(SapBalanceModel.Node node, int level, string levelString)
			{
				this.node = node;
				this.level = level;
				this.levelString = levelString;
			}
		}

		private struct UniversalLevelNode
		{
			public readonly int level;

			public readonly string levelString;

			public readonly UniversalTableModel.Node node;

			public UniversalLevelNode(UniversalTableModel.Node node, int level, string levelString)
			{
				this.node = node;
				this.level = level;
				this.levelString = levelString;
			}
		}

		private const string DATA_SEPERATOR = ";";

		private const string FORMAT_DATETIME = "dd.MM.yyyy HH:mm:ss";

		private const string FORMAT_DATE = "dd.MM.yyyy";

		private const string FORMAT_TIME = "HH:mm:ss";

		private Regex specialChars = new Regex("[^\\w\\d\\s_-]", RegexOptions.Compiled);

		private static readonly Dictionary<string, Export> _jobs = new Dictionary<string, Export>();

		private readonly Dictionary<int, List<Tuple<int, bool>>> ColumnsVisible = new Dictionary<int, List<Tuple<int, bool>>>();

		private DatabaseBase _connection;

		public new static IEnumerable<Export> List => _jobs.Values;

		public ITableObjectCollection ExportObjects { get; private set; }

		public ViewboxDb.TableObjectCollection TransformationObjects { get; private set; }

		public ExportType Type { get; private set; }

		public bool IsOverviewPdf { get; private set; }

		public string FileName { get; set; }

		private void ZipMKN(DatabaseBase connection, ExportType type, ITableObjectCollection objects, List<string> sqlList, List<List<IColumn>> columnsList, CancellationToken token, ILanguage language, IOptimization opt, IUser user, string exportDescription, string fileName = "", ViewboxDb.TableObjectCollection tempTableObjects = null, IEnumerable<IOptimization> optimization = null)
		{
			_connection = connection;
			int i = 0;
			if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
			{
				Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
			}
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			using (ZipOutputStream zipstream = new ZipOutputStream(filename + ".zip"))
			{
				zipstream.EnableZip64 = Zip64Option.AsNecessary;
				using StreamWriter ostream = new StreamWriter(zipstream, Encoding.Default);
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "    ";
				settings.Encoding = Encoding.Default;
				foreach (string sql in sqlList)
				{
					ITableObject tobj = objects.ElementAt(i);
					string regex = "[" + string.Join(",", Path.GetInvalidFileNameChars()) + "]";
					string fname = ReplaceSpecialChars(Regex.Replace(tobj.GetDescription(language), (regex ?? "") ?? "", "_"));
					if (type == ExportType.PDF)
					{
						zipstream.PutNextEntry(fname + ".pdf");
						using IDataReader reader = _connection.ExecuteReader(sql);
						ExportMKNPdf(reader, zipstream, tobj, language, token);
					}
					i++;
					ostream.Flush();
				}
			}
			_connection = null;
		}

		private void ExportMKNPdf(IDataReader reader, Stream ostream, ITableObject tobj, ILanguage language, CancellationToken token)
		{
			DataTable dbTable = new DataTable();
			dbTable.Load(reader);
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.PageSizeAndOrientation = PageSize.A4;
			traits.MarginLeft = 50f;
			traits.MarginRight = 50f;
			traits.MarginTop = 80f;
			traits.MarginBottom = 50f;
			global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
			PdfPTable headTable = CreateHeadTable(dbTable);
			generator.Document.Add(headTable);
			PdfPTable bottomTable = CreateBottomTable(dbTable);
			generator.Document.Add(bottomTable);
			generator.WriteFile(ostream, new Phrase(Resources.Table + ": " + tobj.GetDescription(language)), addPageHeader: false);
		}

		private PdfPTable CreateHeadTable(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font boldBigBlack = new Font(bf, 16f, 1, BaseColor.BLACK);
			Font smallBlack = new Font(bf, 6f, 0, BaseColor.BLACK);
			Font boldNormalBlack = new Font(bf, 12f, 1, BaseColor.BLACK);
			Font boldBlack = new Font(bf, 8f, 1, BaseColor.BLACK);
			Dictionary<string, string> descriptions = new Dictionary<string, string>
			{
				{ "IhreUSt_IdNr", "Ihre USt.-IdNr.:" },
				{ "UnsereUSt_IdNr", "Unsere USt.-IdNr.:" },
				{ "KundenNr", "Ihre Kunden-Nr.:" },
				{ "IhreAuftragNr", "Ihr Auftrag Nr.:" },
				{ "IhreKommission", "Ihre Kommission:" },
				{ "IhreZeichen", "Ihre Zeichen/vom:" },
				{ "Ansprechpartner", "Ansprechpartner:" },
				{ "Telefon", "Telefon:" },
				{ "Rechnungsnummer", "RECHNUNG" },
				{ "Auftragsnummer", "AUFTRAGSBESTÄTIGUNG" },
				{ "Kopie", "- KOPIE -" },
				{ "Gruß", "Mit freundlichem Gruß\nIhre MKN" },
				{ "Versandadresse", "Versandadresse:" },
				{ "Versandart", "Versandart:" },
				{ "Verpackung", "Verpackung:" },
				{ "GesamtgewichtNetto", "GesamtgewichtNetto:" },
				{ "GesamtgewichtBrutto", "GesamtgewichtBrutto:" },
				{ "Herkunftsland", "Ursprungsland:" },
				{ "WarentarifNr", "WarentarifNr:" },
				{ "Signatur", "EDV erstellt, ist auch\nohne Unterschrift gültig" },
				{ "UnsereAuftragsnummer", "Unsere Auftragsnummer" },
				{ "LieferscheinNr", "Lieferschein-Nr.:" },
				{ "LieferscheinVom", " vom " }
			};
			if (dbTable.Rows[0]["Sprache"].ToString().ToLower() != "d")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "IhreUSt_IdNr", "Your VAT-No.:" },
					{ "UnsereUSt_IdNr", "Our VAT-No.:" },
					{ "KundenNr", "Client Code:" },
					{ "IhreAuftragNr", "Your order No.:" },
					{ "IhreKommission", "Your commision:" },
					{ "IhreZeichen", "Your sign/from:" },
					{ "Ansprechpartner", "Contact:" },
					{ "Telefon", "Phone extension:" },
					{ "Rechnungsnummer", "INVOICE" },
					{ "Versandadresse", "Shipment to:" },
					{ "Auftragsnummer", "ORDER CONFIRMATION" },
					{ "Kopie", "- COPY -" },
					{ "Gruß", "" },
					{ "Versandart", "Shipment:" },
					{ "Verpackung", "Packing:" },
					{ "GesamtgewichtNetto", "Weight net:" },
					{ "GesamtgewichtBrutto", "Weight gross:" },
					{ "Herkunftsland", "Country of origin:" },
					{ "WarentarifNr", "Customs tarif No.:" },
					{ "Signatur", "EDP created, it's also\nlegal without signature" },
					{ "UnsereAuftragsnummer", "Our order No.:" },
					{ "LieferscheinNr", "Delivery note No.:" },
					{ "LieferscheinVom", " from " }
				};
			}
			PdfPTable headTable = new PdfPTable(new float[2] { 0.4f, 0.6f });
			headTable.DefaultCell.Border = 0;
			headTable.SpacingBefore = 5f;
			headTable.SpacingAfter = 5f;
			headTable.WidthPercentage = 100f;
			PdfPCell leerZeile = new PdfPCell();
			leerZeile.MinimumHeight = 10f;
			leerZeile.Border = 0;
			PdfPCell leerSpalte = new PdfPCell();
			leerSpalte.PaddingLeft = 20f;
			leerSpalte.Border = 0;
			PdfPTable topTable = new PdfPTable(2);
			topTable.DefaultCell.Border = 0;
			topTable.SpacingBefore = 5f;
			topTable.SpacingAfter = 5f;
			topTable.WidthPercentage = 100f;
			topTable.AddCell(leerZeile);
			topTable.AddCell(leerZeile);
			topTable.AddCell(leerSpalte);
			topTable.AddCell(new Phrase(descriptions["Kopie"], boldBigBlack));
			topTable.AddCell(leerZeile);
			topTable.AddCell(leerZeile);
			topTable.AddCell(new Phrase("MKN GmbH & Co. Postfach 1662 D-38286 Wolfenbüttel", smallBlack));
			topTable.AddCell(leerSpalte);
			PdfPCell topCell = new PdfPCell(topTable);
			topCell.ExtraParagraphSpace = 5f;
			topCell.PaddingBottom = 5f;
			topCell.PaddingTop = 5f;
			topCell.VerticalAlignment = 5;
			topCell.Border = 0;
			topCell.Colspan = 2;
			PdfPTable leftTable = new PdfPTable(1);
			leftTable.DefaultCell.Border = 0;
			leftTable.SpacingBefore = 5f;
			leftTable.SpacingAfter = 5f;
			leftTable.WidthPercentage = 100f;
			leftTable.AddCell(new Phrase(dbTable.Rows[0]["Name"].ToString(), normalBlack));
			leftTable.AddCell(new Phrase(dbTable.Rows[0]["Name2"].ToString(), normalBlack));
			leftTable.AddCell(new Phrase(dbTable.Rows[0]["Name3"].ToString(), normalBlack));
			leftTable.AddCell(new Phrase(dbTable.Rows[0]["Anschrift_Postfach"].ToString(), normalBlack));
			leftTable.AddCell(leerZeile);
			leftTable.AddCell(new Phrase(string.Concat(dbTable.Rows[0]["PLZ"], " ", dbTable.Rows[0]["ORT"]), normalBlack));
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["Land"].ToString()))
			{
				leftTable.AddCell(new Phrase(dbTable.Rows[0]["Land"].ToString(), normalBlack));
			}
			PdfPCell leftCell = new PdfPCell(leftTable);
			leftCell.ExtraParagraphSpace = 5f;
			leftCell.PaddingBottom = 5f;
			leftCell.PaddingTop = 5f;
			leftCell.VerticalAlignment = 5;
			leftCell.Border = 0;
			PdfPTable rightTable = new PdfPTable(new float[4] { 0.5f, 0.25f, 0.05f, 0.2f });
			rightTable.DefaultCell.Border = 0;
			rightTable.SpacingBefore = 5f;
			rightTable.SpacingAfter = 5f;
			rightTable.WidthPercentage = 100f;
			rightTable.AddCell(new Phrase(descriptions["IhreUSt_IdNr"], normalBlack));
			rightTable.AddCell(new Phrase(dbTable.Rows[0]["IhreUSt_IdNr"].ToString(), normalBlack));
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(new Phrase(descriptions["UnsereUSt_IdNr"], normalBlack));
			rightTable.AddCell(new Phrase(dbTable.Rows[0]["UnsereUSt_IdNr"].ToString(), normalBlack));
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(new Phrase(descriptions["KundenNr"], normalBlack));
			rightTable.AddCell(new Phrase(dbTable.Rows[0]["KundenNr"].ToString(), normalBlack));
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(new Phrase(descriptions["IhreAuftragNr"], normalBlack));
			rightTable.AddCell(new Phrase(dbTable.Rows[0]["IhreAuftragNr"].ToString(), normalBlack));
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(new Phrase(descriptions["IhreKommission"], normalBlack));
			rightTable.AddCell(new Phrase(dbTable.Rows[0]["IhreKommission"].ToString(), normalBlack));
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(leerSpalte);
			string ihreZeichen = descriptions["IhreZeichen"];
			rightTable.AddCell(new Phrase(ihreZeichen, normalBlack));
			rightTable.AddCell(new Phrase(dbTable.Rows[0]["IhreZeichen"].ToString(), normalBlack));
			rightTable.AddCell(leerSpalte);
			DateTime datum = DateTime.MinValue;
			DateTime.TryParse(dbTable.Rows[0]["IhreZeichenVom"].ToString(), out datum);
			rightTable.AddCell(new Phrase(datum.ToString("dd") + "/" + datum.ToString("MM") + "-" + datum.ToString("yy"), normalBlack));
			rightTable.AddCell(new Phrase(descriptions["Ansprechpartner"], normalBlack));
			rightTable.AddCell(new Phrase(dbTable.Rows[0]["Ansprechpartner"].ToString(), normalBlack));
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(new Phrase(descriptions["Telefon"], normalBlack));
			rightTable.AddCell(new Phrase(dbTable.Rows[0]["Telefon"].ToString(), normalBlack));
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(new Phrase("Wolfenbüttel,", normalBlack));
			rightTable.AddCell(new Phrase(dbTable.Rows[0]["Datum"].ToString(), normalBlack));
			rightTable.AddCell(leerSpalte);
			rightTable.AddCell(leerSpalte);
			PdfPCell rightCell = new PdfPCell(rightTable);
			rightCell.ExtraParagraphSpace = 5f;
			rightCell.PaddingBottom = 5f;
			rightCell.PaddingTop = 5f;
			rightCell.VerticalAlignment = 5;
			rightCell.Border = 0;
			PdfPTable bottomTable = new PdfPTable(4);
			bottomTable.DefaultCell.Border = 0;
			bottomTable.SpacingBefore = 5f;
			bottomTable.SpacingAfter = 5f;
			bottomTable.WidthPercentage = 100f;
			PdfPCell headLeftCell = new PdfPCell(new Phrase(dbTable.Rows[0]["typ"].ToString(), boldNormalBlack));
			headLeftCell.Border = 0;
			headLeftCell.ExtraParagraphSpace = 5f;
			headLeftCell.PaddingBottom = 5f;
			headLeftCell.PaddingTop = 5f;
			headLeftCell.VerticalAlignment = 5;
			headLeftCell.Colspan = 2;
			string nummern = " / ";
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["Auftragsnummer"].ToString()))
			{
				nummern = string.Concat(dbTable.Rows[0]["Auftragsnummer"], nummern);
			}
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["Rechnungsnummer"].ToString()))
			{
				nummern += dbTable.Rows[0]["Rechnungsnummer"].ToString();
			}
			PdfPCell headRightCell = new PdfPCell(new Phrase(nummern, boldNormalBlack));
			headRightCell.Border = 0;
			headRightCell.ExtraParagraphSpace = 5f;
			headRightCell.PaddingBottom = 5f;
			headRightCell.PaddingTop = 5f;
			headRightCell.VerticalAlignment = 5;
			headRightCell.Colspan = 2;
			bottomTable.AddCell(headLeftCell);
			bottomTable.AddCell(headRightCell);
			bottomTable.AddCell(leerSpalte);
			bottomTable.AddCell(leerSpalte);
			bottomTable.AddCell(leerSpalte);
			bottomTable.AddCell(new Phrase(descriptions["Gruß"], normalBlack));
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["Lieferschein_Nr"].ToString()) && dbTable.Rows[0]["Lieferschein_Nr"].ToString() != "1")
			{
				bottomTable.AddCell(new Phrase(descriptions["LieferscheinNr"], boldBlack));
				bottomTable.AddCell(new Phrase(string.Concat(dbTable.Rows[0]["Lieferschein_Nr"], descriptions["LieferscheinVom"], dbTable.Rows[0]["Lieferschein_Dat"]), normalBlack));
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
			}
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["UnsereAuftragsnummer"].ToString()))
			{
				bottomTable.AddCell(new Phrase(descriptions["UnsereAuftragsnummer"], boldBlack));
				bottomTable.AddCell(new Phrase(dbTable.Rows[0]["UnsereAuftragsnummer"].ToString(), normalBlack));
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
			}
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["Versandadresse1"].ToString()))
			{
				bottomTable.AddCell(new Phrase(descriptions["Versandadresse"], boldBlack));
				bottomTable.AddCell(new Phrase(dbTable.Rows[0]["Versandadresse1"].ToString(), normalBlack));
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
				if (!string.IsNullOrEmpty(dbTable.Rows[0]["Versandadresse2"].ToString()))
				{
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(new Phrase(dbTable.Rows[0]["Versandadresse2"].ToString(), normalBlack));
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(leerSpalte);
				}
				if (!string.IsNullOrEmpty(dbTable.Rows[0]["Versandadresse3"].ToString()))
				{
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(new Phrase(dbTable.Rows[0]["Versandadresse3"].ToString(), normalBlack));
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(leerSpalte);
				}
				if (!string.IsNullOrEmpty(dbTable.Rows[0]["Versandadresse4"].ToString()))
				{
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(new Phrase(dbTable.Rows[0]["Versandadresse4"].ToString(), normalBlack));
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(leerSpalte);
				}
				if (!string.IsNullOrEmpty(dbTable.Rows[0]["Lieferadresse5"].ToString()))
				{
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(new Phrase(dbTable.Rows[0]["Lieferadresse5"].ToString(), normalBlack));
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(leerSpalte);
				}
				if (!string.IsNullOrEmpty(dbTable.Rows[0]["Lieferadresse_Land"].ToString()))
				{
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(new Phrase(dbTable.Rows[0]["Lieferadresse_Land"].ToString(), normalBlack));
					bottomTable.AddCell(leerSpalte);
					bottomTable.AddCell(leerSpalte);
				}
			}
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["Versandart"].ToString()))
			{
				bottomTable.AddCell(new Phrase(descriptions["Versandart"], boldBlack));
				bottomTable.AddCell(new Phrase(dbTable.Rows[0]["Versandart"].ToString(), normalBlack));
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
			}
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["Verpackung"].ToString()))
			{
				bottomTable.AddCell(new Phrase(descriptions["Verpackung"], boldBlack));
				bottomTable.AddCell(new Phrase(dbTable.Rows[0]["Verpackung"].ToString(), normalBlack));
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
			}
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["GesamtgewichtNetto"].ToString()))
			{
				bottomTable.AddCell(new Phrase(descriptions["GesamtgewichtNetto"], boldBlack));
				bottomTable.AddCell(new Phrase(string.Concat(dbTable.Rows[0]["GesamtgewichtNetto"], " kg"), normalBlack));
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
			}
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["GesamtgewichtBrutto"].ToString()))
			{
				bottomTable.AddCell(new Phrase(descriptions["GesamtgewichtBrutto"], boldBlack));
				bottomTable.AddCell(new Phrase(string.Concat(dbTable.Rows[0]["GesamtgewichtBrutto"], " kg"), normalBlack));
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
			}
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["Herkunftsland"].ToString()))
			{
				bottomTable.AddCell(new Phrase(descriptions["Herkunftsland"], boldBlack));
				bottomTable.AddCell(new Phrase(dbTable.Rows[0]["Herkunftsland"].ToString(), normalBlack));
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
			}
			if (!string.IsNullOrEmpty(dbTable.Rows[0]["WarentarifNr"].ToString()))
			{
				bottomTable.AddCell(new Phrase(descriptions["WarentarifNr"], boldBlack));
				bottomTable.AddCell(new Phrase(dbTable.Rows[0]["WarentarifNr"].ToString(), normalBlack));
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
			}
			bottomTable.AddCell(leerSpalte);
			bottomTable.AddCell(leerSpalte);
			bottomTable.AddCell(leerSpalte);
			bottomTable.AddCell(new Phrase(descriptions["Signatur"], normalBlack));
			if (dbTable.Rows[0]["Sprache"].ToString().ToLower() != "d")
			{
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
				bottomTable.AddCell(leerSpalte);
				PdfPCell zusatzTextcell = new PdfPCell(new Phrase("This delivery or service is according to § 4 UStG free of VAT.", normalBlack));
				zusatzTextcell.Border = 0;
				zusatzTextcell.Colspan = 4;
				bottomTable.AddCell(zusatzTextcell);
			}
			PdfPCell bottomCell = new PdfPCell(bottomTable);
			bottomCell.ExtraParagraphSpace = 5f;
			bottomCell.PaddingBottom = 5f;
			bottomCell.PaddingTop = 5f;
			bottomCell.VerticalAlignment = 5;
			bottomCell.Border = 2;
			bottomCell.Colspan = 2;
			headTable.AddCell(topCell);
			headTable.AddCell(leftCell);
			headTable.AddCell(rightCell);
			headTable.AddCell(bottomCell);
			return headTable;
		}

		private PdfPTable CreateBottomTable(DataTable dbTable)
		{
			Dictionary<string, string> descriptions = new Dictionary<string, string>
			{
				{ "Pos", "Pos." },
				{ "Menge", "Menge" },
				{ "Gegenstand", "Gegenstand" },
				{ "Einzelpreis", "Einzelpreis EUR" },
				{ "Gesamtpreis", "Gesamtpreis EUR" },
				{ "Bestellnr", "Bestell-Nr." },
				{ "Zwischensumme", "Zwischensumme" },
				{ "GesamtpreisBold", "Gesamtpreis" },
				{ "Endgruß", "" },
				{ "Preisstellung", "PREISSTELLUNG" },
				{ "Zahlung", "ZAHLUNG" },
				{ "MaschinenNr", "Maschinen-Nr." }
			};
			if (dbTable.Rows[0]["Sprache"].ToString().ToLower() != "d")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Pos", "Item" },
					{ "Menge", "Qty" },
					{ "Gegenstand", "Description" },
					{ "Einzelpreis", "Unit price EUR" },
					{ "Gesamtpreis", "Total price EUR" },
					{ "Bestellnr", "Order-No." },
					{ "Zwischensumme", "Subtotal" },
					{ "GesamtpreisBold", "Total price" },
					{ "Endgruß", "With kind regards\nYour MKN" },
					{ "Preisstellung", "PRICE TERMS" },
					{ "Zahlung", "PAYMENT" },
					{ "MaschinenNr", "Maschinen-Nr." }
				};
			}
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font boldBlack = new Font(bf, 8f, 1, BaseColor.BLACK);
			PdfPTable bottomTable = new PdfPTable(1);
			bottomTable.DefaultCell.Border = 0;
			bottomTable.SpacingBefore = 5f;
			bottomTable.SpacingAfter = 5f;
			bottomTable.WidthPercentage = 100f;
			PdfPCell leerZeile = new PdfPCell();
			leerZeile.MinimumHeight = 10f;
			leerZeile.Border = 0;
			PdfPCell leerSpalte = new PdfPCell();
			leerSpalte.PaddingLeft = 20f;
			leerSpalte.Border = 0;
			PdfPTable headTable = new PdfPTable(new float[5] { 0.1f, 0.1f, 0.4f, 0.2f, 0.2f });
			headTable.DefaultCell.Border = 0;
			headTable.SpacingBefore = 5f;
			headTable.SpacingAfter = 5f;
			headTable.WidthPercentage = 100f;
			headTable.AddCell(new Phrase(descriptions["Pos"], boldBlack));
			headTable.AddCell(new Phrase(descriptions["Menge"], boldBlack));
			headTable.AddCell(new Phrase(descriptions["Gegenstand"], boldBlack));
			PdfPCell einzelpreisHeader = new PdfPCell(new Phrase(descriptions["Einzelpreis"], boldBlack));
			einzelpreisHeader.HorizontalAlignment = 2;
			einzelpreisHeader.PaddingRight = 25f;
			einzelpreisHeader.Border = 0;
			PdfPCell gesamtpreisHeader = new PdfPCell(new Phrase(descriptions["Gesamtpreis"], boldBlack));
			gesamtpreisHeader.HorizontalAlignment = 2;
			gesamtpreisHeader.Border = 0;
			headTable.AddCell(einzelpreisHeader);
			headTable.AddCell(gesamtpreisHeader);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			if (!(dbTable.Rows[0]["kopftext"] is DBNull))
			{
				PdfPCell kopfTextCell = new PdfPCell(new Phrase(dbTable.Rows[0]["kopftext"].ToString(), normalBlack));
				kopfTextCell.Border = 0;
				kopfTextCell.Colspan = 5;
				headTable.AddCell(kopfTextCell);
				headTable.AddCell(leerZeile);
				headTable.AddCell(leerZeile);
				headTable.AddCell(leerZeile);
				headTable.AddCell(leerZeile);
				headTable.AddCell(leerZeile);
			}
			decimal summeGesamtpreis = default(decimal);
			List<decimal> gesamtPreise = new List<decimal>();
			for (int i14 = 0; i14 < dbTable.Rows.Count; i14++)
			{
				if (!(dbTable.Rows[i14]["Pos"].ToString() == "VAT") && !(dbTable.Rows[i14]["Pos"].ToString() == "DISC") && !string.IsNullOrEmpty(dbTable.Rows[i14]["Pos"].ToString()) && !(dbTable.Rows[i14]["Pos"].ToString() == "FEE") && !(dbTable.Rows[i14]["Pos"].ToString() == "FRT") && !(dbTable.Rows[i14]["Pos"].ToString() == "PST"))
				{
					decimal preisNachd = default(decimal);
					decimal.TryParse(dbTable.Rows[i14]["PreisNachDisc"].ToString(), out preisNachd);
					headTable.AddCell(leerZeile);
					headTable.AddCell(leerZeile);
					headTable.AddCell(leerZeile);
					headTable.AddCell(leerZeile);
					headTable.AddCell(leerZeile);
					PdfPCell posCell = new PdfPCell(new Phrase(dbTable.Rows[i14]["Pos"].ToString(), normalBlack));
					posCell.HorizontalAlignment = 2;
					posCell.PaddingRight = 25f;
					posCell.Border = 0;
					headTable.AddCell(posCell);
					decimal menge = default(decimal);
					decimal.TryParse(dbTable.Rows[i14]["Menge"].ToString(), out menge);
					headTable.AddCell(new Phrase(((int)menge).ToString(), normalBlack));
					string text = dbTable.Rows[i14]["Gegenstand_lang"].ToString();
					text = string.Concat(descriptions["Bestellnr"], dbTable.Rows[i14]["Bestellnr"], "\n", text);
					if (!string.IsNullOrEmpty(dbTable.Rows[i14]["Maschinennummer"].ToString()))
					{
						text = text + "\n" + descriptions["MaschinenNr"] + ": " + dbTable.Rows[i14]["Maschinennummer"];
					}
					headTable.AddCell(new Phrase(text, normalBlack));
					decimal einzelPreis = default(decimal);
					decimal.TryParse(dbTable.Rows[i14]["Einzelpreis"].ToString(), out einzelPreis);
					string decimalsEinzelpreis = new string('0', 2);
					string formatEinzelpreis = "{0:#,0." + decimalsEinzelpreis + "}";
					PdfPCell einzelpreisZelle = new PdfPCell(new Phrase(string.Format(formatEinzelpreis, einzelPreis), normalBlack));
					einzelpreisZelle.HorizontalAlignment = 2;
					einzelpreisZelle.PaddingRight = 25f;
					einzelpreisZelle.Border = 0;
					headTable.AddCell(einzelpreisZelle);
					decimal gesamtPreis = default(decimal);
					decimal.TryParse(dbTable.Rows[i14]["Gesamtpreis"].ToString(), out gesamtPreis);
					if (menge < 0m)
					{
						gesamtPreis = -gesamtPreis;
					}
					summeGesamtpreis += gesamtPreis;
					string decimalsGesamtpreis = new string('0', 2);
					string formatGesamtpreis = "{0:#,0." + decimalsGesamtpreis + "}";
					gesamtPreise.Add(gesamtPreis);
					PdfPCell gesamtpreisZelle = new PdfPCell(new Phrase(string.Format(formatGesamtpreis, Math.Abs(gesamtPreis)), normalBlack));
					gesamtpreisZelle.HorizontalAlignment = 2;
					gesamtpreisZelle.Border = 0;
					headTable.AddCell(gesamtpreisZelle);
				}
			}
			string decimalsZwischensumme = new string('0', 2);
			string formatZwischensumme = "{0:#,0." + decimalsZwischensumme + "}";
			PdfPCell rabattZelle = new PdfPCell(new Phrase("EU", normalBlack));
			rabattZelle.HorizontalAlignment = 2;
			rabattZelle.PaddingRight = 10f;
			rabattZelle.Border = 0;
			bool discountExists = false;
			for (int i13 = 0; i13 < dbTable.Rows.Count; i13++)
			{
				if (dbTable.Rows[i13]["Pos"].ToString() == "DISC")
				{
					discountExists = true;
				}
			}
			if (discountExists)
			{
				for (int i12 = 1; i12 <= 7; i12++)
				{
					headTable.AddCell(leerZeile);
				}
				headTable.AddCell(new Phrase(descriptions["Zwischensumme"], normalBlack));
				headTable.AddCell(rabattZelle);
				PdfPCell gesamtpreisCell3 = new PdfPCell(new Phrase(string.Format(formatZwischensumme, Math.Abs(summeGesamtpreis)), normalBlack));
				gesamtpreisCell3.HorizontalAlignment = 2;
				gesamtpreisCell3.Border = 0;
				headTable.AddCell(gesamtpreisCell3);
				for (int i11 = 0; i11 < dbTable.Rows.Count; i11++)
				{
					if (!(dbTable.Rows[i11]["Pos"].ToString() != "DISC"))
					{
						try
						{
							headTable.AddCell(leerZeile);
							headTable.AddCell(leerZeile);
							headTable.AddCell(new Phrase(dbTable.Rows[i11]["gegenstand_kurz"].ToString(), normalBlack));
							headTable.AddCell(rabattZelle);
							decimal summeEinzelPreise3 = Convert.ToDecimal(dbTable.Rows[i11]["Gesamtpreis"].ToString());
							decimal multiplikator3 = Convert.ToDecimal(dbTable.Rows[i11]["Menge"].ToString());
							summeGesamtpreis += summeEinzelPreise3 * multiplikator3;
							PdfPCell disc1Zelle3 = new PdfPCell(new Phrase(string.Format(formatZwischensumme, summeEinzelPreise3), normalBlack));
							disc1Zelle3.HorizontalAlignment = 2;
							disc1Zelle3.Border = 0;
							headTable.AddCell(disc1Zelle3);
						}
						catch (Exception)
						{
							headTable.AddCell(leerZeile);
							headTable.AddCell(leerZeile);
							headTable.AddCell(new Phrase(dbTable.Rows[i11]["Gesamtpreis"].ToString(), normalBlack));
							headTable.AddCell(rabattZelle);
							headTable.AddCell(leerZeile);
						}
					}
				}
			}
			bool freightExists = false;
			for (int i10 = 0; i10 < dbTable.Rows.Count; i10++)
			{
				string freightText = dbTable.Rows[i10]["Pos"].ToString();
				if (freightText == "FEE" || freightText == "FRT" || freightText == "PST")
				{
					freightExists = true;
				}
			}
			if (freightExists)
			{
				for (int i9 = 1; i9 <= 7; i9++)
				{
					headTable.AddCell(leerZeile);
				}
				headTable.AddCell(new Phrase(descriptions["Zwischensumme"], normalBlack));
				headTable.AddCell(rabattZelle);
				PdfPCell gesamtpreisCell2 = new PdfPCell(new Phrase(string.Format(formatZwischensumme, Math.Abs(summeGesamtpreis)), normalBlack));
				gesamtpreisCell2.HorizontalAlignment = 2;
				gesamtpreisCell2.Border = 0;
				headTable.AddCell(gesamtpreisCell2);
				for (int i8 = 0; i8 < dbTable.Rows.Count; i8++)
				{
					if (!(dbTable.Rows[i8]["Pos"].ToString() != "FEE") || !(dbTable.Rows[i8]["Pos"].ToString() != "FRT") || !(dbTable.Rows[i8]["Pos"].ToString() != "PST"))
					{
						try
						{
							headTable.AddCell(leerZeile);
							headTable.AddCell(leerZeile);
							headTable.AddCell(new Phrase(dbTable.Rows[i8]["gegenstand_kurz"].ToString(), normalBlack));
							headTable.AddCell(rabattZelle);
							decimal summeEinzelPreise2 = Convert.ToDecimal(dbTable.Rows[i8]["Gesamtpreis"].ToString());
							decimal multiplikator2 = Convert.ToDecimal(dbTable.Rows[i8]["Menge"].ToString());
							summeGesamtpreis += summeEinzelPreise2 * multiplikator2;
							PdfPCell disc1Zelle2 = new PdfPCell(new Phrase(string.Format(formatZwischensumme, summeEinzelPreise2), normalBlack));
							disc1Zelle2.HorizontalAlignment = 2;
							disc1Zelle2.Border = 0;
							headTable.AddCell(disc1Zelle2);
						}
						catch (Exception)
						{
							headTable.AddCell(leerZeile);
							headTable.AddCell(leerZeile);
							headTable.AddCell(new Phrase(dbTable.Rows[i8]["Gesamtpreis"].ToString(), normalBlack));
							headTable.AddCell(rabattZelle);
							headTable.AddCell(leerZeile);
						}
					}
				}
			}
			bool vatExists = false;
			for (int i7 = 0; i7 < dbTable.Rows.Count; i7++)
			{
				if (dbTable.Rows[i7]["Pos"].ToString() == "VAT")
				{
					vatExists = true;
				}
			}
			if (vatExists)
			{
				for (int i6 = 1; i6 <= 10; i6++)
				{
					headTable.AddCell(leerZeile);
				}
				PdfPCell borderBottomCell = new PdfPCell();
				borderBottomCell.Border = 2;
				for (int i5 = 1; i5 <= 4; i5++)
				{
					headTable.AddCell(leerZeile);
				}
				headTable.AddCell(borderBottomCell);
				headTable.AddCell(leerZeile);
				headTable.AddCell(leerZeile);
				headTable.AddCell(new Phrase(descriptions["Zwischensumme"], normalBlack));
				headTable.AddCell(rabattZelle);
				PdfPCell gesamtpreisCell = new PdfPCell(new Phrase(string.Format(formatZwischensumme, Math.Abs(summeGesamtpreis)), normalBlack));
				gesamtpreisCell.HorizontalAlignment = 2;
				gesamtpreisCell.Border = 0;
				headTable.AddCell(gesamtpreisCell);
				for (int i4 = 0; i4 < dbTable.Rows.Count; i4++)
				{
					if (!(dbTable.Rows[i4]["Pos"].ToString() != "VAT"))
					{
						try
						{
							headTable.AddCell(leerZeile);
							headTable.AddCell(leerZeile);
							headTable.AddCell(new Phrase(dbTable.Rows[i4]["Gegenstand_kurz"].ToString(), normalBlack));
							headTable.AddCell(rabattZelle);
							decimal summeEinzelPreise = Convert.ToDecimal(dbTable.Rows[i4]["Gesamtpreis"].ToString());
							decimal multiplikator = Convert.ToDecimal(dbTable.Rows[i4]["Menge"].ToString());
							summeGesamtpreis += summeEinzelPreise * multiplikator;
							PdfPCell disc1Zelle = new PdfPCell(new Phrase(string.Format(formatZwischensumme, summeEinzelPreise), normalBlack));
							disc1Zelle.HorizontalAlignment = 2;
							disc1Zelle.Border = 0;
							headTable.AddCell(disc1Zelle);
						}
						catch (Exception)
						{
							headTable.AddCell(leerZeile);
							headTable.AddCell(leerZeile);
							headTable.AddCell(new Phrase(dbTable.Rows[i4]["Gesamtpreis"].ToString(), normalBlack));
							headTable.AddCell(rabattZelle);
							headTable.AddCell(leerZeile);
						}
					}
				}
				PdfPCell borderTopCell = new PdfPCell();
				borderTopCell.Border = 1;
				for (int i3 = 1; i3 <= 4; i3++)
				{
					headTable.AddCell(leerZeile);
				}
				headTable.AddCell(borderTopCell);
			}
			PdfPCell borderZelle = new PdfPCell();
			borderZelle.Border = 2;
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(new Phrase(descriptions["GesamtpreisBold"], boldBlack));
			PdfPCell rabattZelleBold = new PdfPCell(new Phrase("EU", boldBlack));
			rabattZelleBold.HorizontalAlignment = 2;
			rabattZelleBold.PaddingRight = 10f;
			rabattZelleBold.Border = 0;
			headTable.AddCell(rabattZelleBold);
			decimal totalPrice = Convert.ToDecimal(dbTable.Rows[0]["Rechnungsbetrag"].ToString());
			PdfPCell totalPriceCell = new PdfPCell(new Phrase(string.Format(formatZwischensumme, totalPrice), boldBlack));
			totalPriceCell.HorizontalAlignment = 2;
			totalPriceCell.Border = 0;
			headTable.AddCell(totalPriceCell);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			headTable.AddCell(borderZelle);
			headTable.AddCell(leerSpalte);
			headTable.AddCell(leerSpalte);
			headTable.AddCell(leerSpalte);
			headTable.AddCell(leerSpalte);
			headTable.AddCell(borderZelle);
			for (int i2 = 1; i2 <= 10; i2++)
			{
				headTable.AddCell(leerZeile);
			}
			List<string> toAppend = new List<string> { "Preisstellung", "Zahlung" };
			foreach (string item in toAppend)
			{
				if (!string.IsNullOrEmpty(dbTable.Rows[0][item].ToString()))
				{
					PdfPCell cell = new PdfPCell(new Phrase(descriptions[item], normalBlack));
					cell.Border = 0;
					cell.Colspan = 2;
					headTable.AddCell(cell);
					cell = new PdfPCell(new Phrase(dbTable.Rows[0][item].ToString(), normalBlack));
					cell.Border = 0;
					headTable.AddCell(cell);
					for (int n = 1; n <= 2; n++)
					{
						headTable.AddCell(leerSpalte);
					}
					for (int m = 1; m <= 5; m++)
					{
						headTable.AddCell(leerZeile);
					}
				}
			}
			string zusatzText = string.Empty;
			if (dbTable.Rows[0]["Sprache"].ToString().ToLower() == "d")
			{
				zusatzText = "Zur eventuellen weiteren Entgeldminderung verweisen wir auf die getroffenen Vereinbarungen.";
			}
			else if (!dbTable.Rows[0]["zusatz"].ToString().Contains("The MKN general conditions of sale, delivery, installation"))
			{
				zusatzText = "The MKN general conditions of sale, delivery, installation and payment apply to all sales contracts entered into with MKN unless individual agreements contrary to these conditions have been made in writing. The MKN general terms and conditions of business can be viewed on our web site www.mkn.eu or we will gladly send you these on request.";
			}
			for (int l = 1; l <= 5; l++)
			{
				headTable.AddCell(leerZeile);
			}
			if (!string.IsNullOrEmpty(zusatzText))
			{
				PdfPCell zusatzTextcell = new PdfPCell(new Phrase(zusatzText, normalBlack));
				zusatzTextcell.Border = 0;
				zusatzTextcell.Colspan = 3;
				headTable.AddCell(zusatzTextcell);
				for (int k = 1; k <= 2; k++)
				{
					headTable.AddCell(leerSpalte);
				}
			}
			for (int j = 1; j <= 5; j++)
			{
				headTable.AddCell(leerZeile);
			}
			PdfPCell textCell = new PdfPCell(new Phrase(dbTable.Rows[0]["zusatz"].ToString(), normalBlack));
			textCell.Border = 0;
			textCell.Colspan = 3;
			headTable.AddCell(textCell);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			for (int i = 1; i <= 10; i++)
			{
				headTable.AddCell(leerZeile);
			}
			textCell = new PdfPCell(new Phrase(descriptions["Endgruß"], normalBlack));
			textCell.Border = 0;
			textCell.Colspan = 3;
			headTable.AddCell(textCell);
			headTable.AddCell(leerZeile);
			headTable.AddCell(leerZeile);
			PdfPCell headCell = new PdfPCell(headTable);
			headCell.ExtraParagraphSpace = 5f;
			headCell.PaddingBottom = 5f;
			headCell.PaddingTop = 5f;
			headCell.VerticalAlignment = 5;
			headCell.Border = 0;
			bottomTable.AddCell(headCell);
			return bottomTable;
		}

		private void ZipHofmeisterRechnung(DatabaseBase connection, ExportType type, ITableObjectCollection objects, List<string> sqlList, List<List<IColumn>> columnsList, CancellationToken token, ILanguage language, IOptimization opt, IUser user, string exportDescription, string fileName = "", ViewboxDb.TableObjectCollection tempTableObjects = null, IEnumerable<IOptimization> optimization = null)
		{
			_connection = connection;
			int i = 0;
			if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
			{
				Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
			}
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			using (ZipOutputStream zipstream = new ZipOutputStream(filename + ".zip"))
			{
				zipstream.EnableZip64 = Zip64Option.AsNecessary;
				using StreamWriter ostream = new StreamWriter(zipstream, Encoding.Default);
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "    ";
				settings.Encoding = Encoding.Default;
				foreach (string sql in sqlList)
				{
					ITableObject tobj = objects.ElementAt(i);
					string regex = "[" + string.Join(",", Path.GetInvalidFileNameChars()) + "]";
					string fname = ReplaceSpecialChars(Regex.Replace(tobj.GetDescription(language), (regex ?? "") ?? "", "_"));
					if (type == ExportType.PDF)
					{
						zipstream.PutNextEntry(fname + ".pdf");
						using IDataReader reader = _connection.ExecuteReader(sql);
						ExportHofmeisterRechnungPdf(reader, zipstream, tobj, language, token);
					}
					i++;
					ostream.Flush();
				}
			}
			_connection = null;
		}

		private void ExportHofmeisterRechnungPdf(IDataReader reader, Stream ostream, ITableObject tobj, ILanguage language, CancellationToken token)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			DataTable dbTable = new DataTable();
			dbTable.Load(reader);
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.PageSizeAndOrientation = PageSize.A4;
			traits.MarginLeft = 25f;
			traits.MarginRight = 25f;
			traits.MarginTop = 280f;
			traits.MarginBottom = 140f;
			global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
			generator.Document.Add(CreateContentTableHofmeisterRechnung(dbTable));
			generator.WriteFile(ostream, new Phrase(Resources.Table + ": " + tobj.GetDescription(language)), CreateHeadTableHofmeisterRechnung(dbTable), CreateFooterTableHofmeisterRechnung(dbTable), new Font(bf, 8f, 0), "Seite", 1, currentAndCompleteSideNr: true, " von ", 85f, printHeaderOnlyOnFirstPage: false, printFooterOnlyOnLastPage: true);
		}

		private PdfPTable CreateHeadTableHofmeisterRechnung(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font boldNormalBlack = new Font(bf, 10f, 1, BaseColor.BLACK);
			Font boldSmallBlack = new Font(bf, 8f, 1, BaseColor.BLACK);
			Font boldBigBlack = new Font(bf, 18f, 1, BaseColor.BLACK);
			Font boldMiddleBlack = new Font(bf, 11f, 1, BaseColor.BLACK);
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 5f;
			PdfPTable headTable = new PdfPTable(1);
			headTable.DefaultCell.Border = 0;
			headTable.WidthPercentage = 100f;
			headTable.DefaultCell.Padding = 0f;
			PdfPTable nestedTable = new PdfPTable(new float[3] { 0.6f, 0.05f, 0.35f });
			nestedTable.DefaultCell.Border = 0;
			nestedTable.WidthPercentage = 100f;
			nestedTable.DefaultCell.Padding = 0f;
			PdfPCell rechnungCell = new PdfPCell(new Phrase("Rechnung", boldBigBlack));
			rechnungCell.Border = 0;
			rechnungCell.Padding = 2f;
			rechnungCell.VerticalAlignment = 6;
			nestedTable.AddCell(emptyCell);
			nestedTable.AddCell(emptyCell);
			nestedTable.AddCell(rechnungCell);
			PdfPTable rightTable = new PdfPTable(1);
			rightTable.DefaultCell.Border = 0;
			rightTable.WidthPercentage = 100f;
			rightTable.DefaultCell.Padding = 0f;
			PdfPTable rightNestedTable = new PdfPTable(new float[3] { 0.5f, 0.03f, 0.47f });
			rightNestedTable.DefaultCell.Border = 0;
			rightNestedTable.WidthPercentage = 100f;
			rightNestedTable.DefaultCell.Padding = 2f;
			rightNestedTable.DefaultCell.VerticalAlignment = 6;
			rightNestedTable.AddCell(new Phrase("Rechnungs-Nr:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Re_Nummer"].ToString(), boldNormalBlack));
			rightNestedTable.AddCell(new Phrase("Rechnungs-Datum:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Datum"].ToString(), boldSmallBlack));
			PdfPCell middleLine = new PdfPCell(new Phrase("Bitte stets angeben", normalBlack));
			middleLine.Colspan = 3;
			middleLine.Border = 0;
			middleLine.Padding = 2f;
			middleLine.VerticalAlignment = 6;
			rightNestedTable.AddCell(middleLine);
			rightNestedTable.AddCell(new Phrase("Kunden-Nr:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Kunde_Nr"].ToString(), boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase("Ihre Steuernummer:", normalBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Ihre_Steuernummer"].ToString(), normalBlack));
			rightNestedTable.AddCell(new Phrase("Ihre USt-ID-Nr.:", normalBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Ihre_USt_ID_Nr"].ToString(), normalBlack));
			rightTable.AddCell(emptyCell);
			rightTable.AddCell(emptyCell);
			rightTable.AddCell(rightNestedTable);
			PdfPTable leftTable = new PdfPTable(1);
			leftTable.DefaultCell.Border = 0;
			leftTable.WidthPercentage = 100f;
			leftTable.DefaultCell.PaddingLeft = 45f;
			leftTable.DefaultCell.PaddingRight = 0f;
			leftTable.DefaultCell.PaddingTop = 0f;
			leftTable.DefaultCell.PaddingBottom = 0f;
			PdfPTable leftNestedTable = new PdfPTable(1);
			leftNestedTable.DefaultCell.Border = 0;
			leftNestedTable.WidthPercentage = 100f;
			string kundenadresse = string.Empty;
			if (!(dbTable.Rows[0]["Adressat1"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Adressat1"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Adressat1"], "\n");
			}
			if (!(dbTable.Rows[0]["Adressat2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Adressat2"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Adressat2"], "\n");
			}
			if (!(dbTable.Rows[0]["Adressat3"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Adressat3"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Adressat3"], "\n");
			}
			if (!(dbTable.Rows[0]["Str_Nr"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Str_Nr"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Str_Nr"], "\n");
			}
			if (!(dbTable.Rows[0]["PLZ_Ort"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["PLZ_Ort"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["PLZ_Ort"], "\n");
			}
			leftNestedTable.AddCell(new Phrase(kundenadresse, boldMiddleBlack));
			leftTable.AddCell(leftNestedTable);
			nestedTable.AddCell(leftTable);
			nestedTable.AddCell(emptyCell);
			nestedTable.AddCell(rightTable);
			string lieferAdresse = string.Empty;
			if (!(dbTable.Rows[0]["Lieferadresse1"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lieferadresse1"].ToString()))
			{
				lieferAdresse += dbTable.Rows[0]["Lieferadresse1"];
			}
			if (!(dbTable.Rows[0]["Lieferadresse2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lieferadresse2"].ToString()))
			{
				if (!string.IsNullOrEmpty(lieferAdresse))
				{
					lieferAdresse += " ";
				}
				lieferAdresse += dbTable.Rows[0]["Lieferadresse2"];
			}
			if (!(dbTable.Rows[0]["Lieferadresse3"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lieferadresse3"].ToString()))
			{
				if (!string.IsNullOrEmpty(lieferAdresse))
				{
					lieferAdresse += " ";
				}
				lieferAdresse += dbTable.Rows[0]["Lieferadresse3"];
			}
			if (!(dbTable.Rows[0]["Lieferadresse4"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lieferadresse4"].ToString()))
			{
				if (!string.IsNullOrEmpty(lieferAdresse))
				{
					lieferAdresse += " ";
				}
				lieferAdresse += dbTable.Rows[0]["Lieferadresse4"];
			}
			if (!string.IsNullOrEmpty(lieferAdresse))
			{
				PdfPCell lieferAdresseCell = new PdfPCell(new Phrase("Lieferadresse: " + lieferAdresse, normalBlack));
				lieferAdresseCell.Colspan = 7;
				lieferAdresseCell.Border = 0;
				lieferAdresseCell.Padding = 15f;
				for (int i = 0; i < 12; i++)
				{
					nestedTable.AddCell(emptyCell);
				}
				nestedTable.AddCell(lieferAdresseCell);
			}
			headTable.AddCell(emptyCell);
			headTable.AddCell(nestedTable);
			return headTable;
		}

		private PdfPTable CreateFooterTableHofmeisterRechnung(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font smallBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font smallBlackBold = new Font(bf, 8f, 1, BaseColor.BLACK);
			Font normalBlackBold = new Font(bf, 10f, 1, BaseColor.BLACK);
			PdfPTable footerTable = new PdfPTable(new float[8] { 0.13f, 0.14f, 0.1f, 0.1f, 0.1f, 0.09f, 0.09f, 0.25f });
			footerTable.WidthPercentage = 100f;
			footerTable.DefaultCell.Border = 0;
			footerTable.DefaultCell.Padding = 1f;
			footerTable.DefaultCell.VerticalAlignment = 6;
			footerTable.DefaultCell.HorizontalAlignment = 2;
			PdfPCell emptyLine = new PdfPCell();
			emptyLine.Colspan = 8;
			emptyLine.Border = 0;
			emptyLine.Padding = 0f;
			emptyLine.FixedHeight = 15f;
			PdfPCell firstLine = new PdfPCell(new Phrase("Bezüglich der Entgeldminderung verweisen wir auf die aktuellen Zahlungs- und Konditionsvereinbarungen", smallBlack));
			firstLine.Colspan = 8;
			firstLine.Border = 0;
			firstLine.Padding = 0f;
			footerTable.AddCell(firstLine);
			footerTable.AddCell(emptyLine);
			for (int i = 0; i < 7; i++)
			{
				footerTable.AddCell("");
			}
			footerTable.AddCell(new Phrase(string.Format("Zwischensumme: {0}", Convert.ToDecimal(dbTable.Rows[0]["zwischensumme"]).ToString("#,##0.00", CultureInfo.InvariantCulture)), smallBlackBold));
			footerTable.AddCell(new Phrase("ant. Verpa.", smallBlack));
			footerTable.AddCell(new Phrase("Rg.-Wert netto", smallBlackBold));
			if (!(dbTable.Rows[0]["AT_WERT"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["AT_WERT"].ToString()))
			{
				footerTable.AddCell(new Phrase("AT-Wert", smallBlack));
			}
			else
			{
				footerTable.AddCell("");
			}
			if (!(dbTable.Rows[0]["Davon_10prz"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["Davon_10prz"].ToString()))
			{
				footerTable.AddCell(new Phrase("Davon 10%", smallBlack));
			}
			else
			{
				footerTable.AddCell("");
			}
			if (!(dbTable.Rows[0]["AT_Steuer"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["AT_Steuer"].ToString()))
			{
				footerTable.AddCell(new Phrase("AT-Steuer", smallBlack));
			}
			else
			{
				footerTable.AddCell("");
			}
			footerTable.AddCell(new Phrase("MWST %", smallBlackBold));
			footerTable.AddCell(new Phrase("MWST", smallBlackBold));
			footerTable.AddCell(new Phrase("Endbetrag", smallBlackBold));
			if (!(dbTable.Rows[0]["ant_Verpa"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["ant_Verpa"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["ant_Verpa"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlack));
			}
			else
			{
				footerTable.AddCell("");
			}
			if (!(dbTable.Rows[0]["RG_Wert_netto"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["RG_Wert_netto"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["RG_Wert_netto"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlackBold));
			}
			else
			{
				footerTable.AddCell("");
			}
			if (!(dbTable.Rows[0]["AT_WERT"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["AT_WERT"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["AT_WERT"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlack));
			}
			else
			{
				footerTable.AddCell("");
			}
			if (!(dbTable.Rows[0]["Davon_10prz"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["Davon_10prz"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["Davon_10prz"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlack));
			}
			else
			{
				footerTable.AddCell("");
			}
			if (!(dbTable.Rows[0]["AT_Steuer"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["AT_Steuer"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["AT_Steuer"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlack));
			}
			else
			{
				footerTable.AddCell("");
			}
			if (!(dbTable.Rows[0]["MWST_PRZ"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["MWST_PRZ"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["MWST_PRZ"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlackBold));
			}
			else
			{
				footerTable.AddCell("");
			}
			if (!(dbTable.Rows[0]["MWST"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["MWST"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["MWST"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlackBold));
			}
			else
			{
				footerTable.AddCell("");
			}
			if (!(dbTable.Rows[0]["Endbetrag"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["Endbetrag"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["Endbetrag"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlackBold));
			}
			else
			{
				footerTable.AddCell("");
			}
			footerTable.AddCell(emptyLine);
			PdfPCell lastLine = new PdfPCell(new Phrase(dbTable.Rows[0]["Skonto_Info"].ToString(), smallBlack));
			lastLine.Colspan = 7;
			lastLine.Border = 0;
			lastLine.Padding = 0f;
			footerTable.AddCell(lastLine);
			PdfPCell waehrungCell = new PdfPCell(new Phrase(string.Format("Währung: {0}", dbTable.Rows[0]["Waehrung"]), smallBlack));
			waehrungCell.Border = 0;
			waehrungCell.Padding = 0f;
			waehrungCell.HorizontalAlignment = 2;
			footerTable.AddCell(waehrungCell);
			PdfPCell lastEmptyLine = new PdfPCell();
			lastEmptyLine.Border = 0;
			lastEmptyLine.FixedHeight = 35f;
			lastEmptyLine.Colspan = 8;
			footerTable.AddCell(lastEmptyLine);
			return footerTable;
		}

		private PdfPTable CreateContentTableHofmeisterRechnung(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font boldBlack = new Font(bf, 8f, 1, BaseColor.BLACK);
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 10f;
			PdfPTable table = new PdfPTable(1);
			table.DefaultCell.Border = 0;
			table.WidthPercentage = 100f;
			table.DefaultCell.PaddingLeft = 10f;
			table.DefaultCell.PaddingRight = 10f;
			table.DefaultCell.PaddingTop = 0f;
			table.DefaultCell.PaddingBottom = 0f;
			table.HeaderRows = 1;
			PdfPTable headerTable = new PdfPTable(new float[10] { 0.045f, 0.17f, 0.03f, 0.23f, 0.07f, 0.045f, 0.11f, 0.09f, 0.105f, 0.105f });
			headerTable.DefaultCell.Border = 15;
			headerTable.WidthPercentage = 100f;
			headerTable.DefaultCell.Padding = 0f;
			headerTable.AddCell(CreateCell(new Phrase("Pos", boldBlack), BaseColor.WHITE, 5, 0, 0, 3f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Artikel", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("", boldBlack), BaseColor.WHITE, 5, 0, 0, 3f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Artikelbezeichnung", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Menge", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("ME", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Brutto", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("RAB.%", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("E-Preis", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Gesamt", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			PdfPTable contentTable = new PdfPTable(new float[10] { 0.045f, 0.17f, 0.03f, 0.23f, 0.07f, 0.045f, 0.11f, 0.09f, 0.105f, 0.105f });
			contentTable.DefaultCell.Border = 0;
			contentTable.WidthPercentage = 100f;
			contentTable.DefaultCell.Padding = 0f;
			DataRow[] content = dbTable.Select("", "Auftrags_Nr,Pos");
			string lastAuftragsNr = string.Empty;
			for (int i = 0; i < content.Length; i++)
			{
				if (lastAuftragsNr != content[i]["Auftrags_Nr"].ToString())
				{
					PdfPTable headTable = new PdfPTable(40);
					headTable.DefaultCell.Border = 0;
					headTable.WidthPercentage = 100f;
					headTable.DefaultCell.Padding = 0f;
					PdfPCell auftragsCell = CreateCell(new Phrase("Auftrag H&M: " + content[i]["Auftrags_Nr"], boldBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					auftragsCell.Colspan = 8;
					headTable.AddCell(auftragsCell);
					PdfPCell sachbearbeiterCell = CreateCell(new Phrase("Sachbearbeiter H&M: " + content[i]["Sachbearbeiter"], normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					sachbearbeiterCell.Colspan = 20;
					headTable.AddCell(sachbearbeiterCell);
					PdfPCell vaCell = CreateCell(new Phrase("VA: " + content[i]["VA"], normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					vaCell.Colspan = 12;
					vaCell.NoWrap = true;
					headTable.AddCell(vaCell);
					PdfPCell auftragsdatumCell = CreateCell(new Phrase("Auftragsdatum: " + content[i]["Auftragsdatum"], normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					auftragsdatumCell.Colspan = 10;
					headTable.AddCell(auftragsdatumCell);
					PdfPCell lieferdatumCell = CreateCell(new Phrase("Lieferdatum: " + content[i]["Lieferdatum"], normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					lieferdatumCell.Colspan = 10;
					headTable.AddCell(lieferdatumCell);
					PdfPCell lieferscheinCell = CreateCell(new Phrase("Lieferscheinnr.: " + content[i]["Lieferscheinnr"], normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					lieferscheinCell.Colspan = 20;
					lieferscheinCell.NoWrap = true;
					headTable.AddCell(lieferscheinCell);
					PdfPCell bestellungCell = CreateCell(new Phrase("Ihre Bestellung: " + content[i]["Ihre_Bestellung"], normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					bestellungCell.Colspan = 28;
					headTable.AddCell(bestellungCell);
					PdfPCell vertreterCell = CreateCell(new Phrase("Vertreter: " + content[i]["Vertreter"], normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					vertreterCell.Colspan = 12;
					vertreterCell.NoWrap = true;
					headTable.AddCell(vertreterCell);
					if (content[i]["Versandanweisung"] != null && !string.IsNullOrEmpty(content[i]["Versandanweisung"].ToString()))
					{
						PdfPCell versandanweisungCell = CreateCell(new Phrase("Versandanweisung: " + content[i]["Versandanweisung"], normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
						versandanweisungCell.Colspan = 40;
						versandanweisungCell.NoWrap = true;
						headTable.AddCell(versandanweisungCell);
					}
					PdfPCell headCell = new PdfPCell(headTable);
					headCell.Colspan = 10;
					headCell.Border = 0;
					headCell.PaddingLeft = 5f;
					headCell.PaddingRight = 5f;
					headCell.PaddingTop = 0f;
					headCell.PaddingBottom = 0f;
					for (int k = 0; k < 20; k++)
					{
						contentTable.AddCell(emptyCell);
					}
					contentTable.AddCell(headCell);
				}
				contentTable.AddCell(CreateCell(new Phrase(content[i]["Pos"].ToString(), normalBlack), BaseColor.WHITE, 4, 2, 0, 1f));
				contentTable.AddCell(CreateCell(new Phrase(content[i]["Artikel"].ToString(), normalBlack), BaseColor.WHITE, 4, 0, 0, 5f));
				if (!(dbTable.Rows[i]["Org"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[i]["Org"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(content[i]["Org"].ToString(), normalBlack), BaseColor.WHITE, 6, 2, 0, 1f));
				}
				else
				{
					contentTable.AddCell("");
				}
				contentTable.AddCell(CreateCell(new Phrase(string.Concat(content[i]["Artikelbezeichnung"], (content[i]["Org"] != null && !string.IsNullOrEmpty(content[i]["Org"].ToString())) ? "\n " : ""), normalBlack), BaseColor.WHITE, 4, 0, 0, 5f));
				if (!(dbTable.Rows[i]["Menge"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[i]["Menge"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["Menge"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				contentTable.AddCell(CreateCell(new Phrase(content[i]["ME"].ToString(), normalBlack), BaseColor.WHITE, 4, 2, 0, 0f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				if (!(dbTable.Rows[i]["Brutto"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[i]["Brutto"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["Brutto"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				if (!(dbTable.Rows[i]["Rab"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[i]["Rab"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(content[i]["Rab"].ToString(), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f));
				}
				else
				{
					contentTable.AddCell("");
				}
				if (!(dbTable.Rows[i]["E_Preis"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[i]["E_Preis"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["E_Preis"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				if (!(dbTable.Rows[i]["Gesamt"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[i]["Gesamt"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["Gesamt"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				for (int j = 0; j < 10; j++)
				{
					contentTable.AddCell(emptyCell);
				}
				lastAuftragsNr = content[i]["Auftrags_Nr"].ToString();
			}
			table.AddCell(headerTable);
			table.AddCell(contentTable);
			return table;
		}

		private Export(ITableObjectCollection objects, IOptimization opt, UserDefinedExportProjects project)
		{
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			ExportObjects = objects;
			Type = ExportType.PDF;
			IsOverviewPdf = false;
			foreach (ITableObject o in ExportObjects)
			{
				foreach (IColumn c in o.Columns)
				{
					if (ColumnsVisible.ContainsKey(o.Id))
					{
						ColumnsVisible[o.Id].Add(new Tuple<int, bool>(c.Id, c.IsVisible));
						continue;
					}
					ColumnsVisible.Add(o.Id, new List<Tuple<int, bool>>
					{
						new Tuple<int, bool>(c.Id, c.IsVisible)
					});
				}
			}
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterDownload(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			StartJob(delegate(object obj, CancellationToken token)
			{
				DoUserDefinedExport((ITableObjectCollection)((object[])obj)[0], (IOptimization)((object[])obj)[1], (ILanguage)((object[])obj)[2], (IUser)((object[])obj)[3], (global::ViewboxDb.ViewboxDb.ReaderExport)((object[])obj)[4], token);
			}, new object[5]
			{
				objects,
				opt,
				ViewboxSession.Language,
				ViewboxSession.User,
				GetDelegateForUserDefinedProject(project)
			});
			ITableObject firstObject = ExportObjects.First();
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				string ttype = "";
				if (ExportObjects.Count > 1)
				{
					ttype += ((firstObject.Type == TableType.Table) ? Resources.ResourceManager.GetString("Tables", culture) : "");
					ttype += ((firstObject.Type == TableType.View) ? Resources.ResourceManager.GetString("Views", culture) : "");
					ttype += ((firstObject.Type == TableType.Issue) ? Resources.ResourceManager.GetString("Issues", culture) : "");
				}
				else
				{
					ttype += ((firstObject.Type == TableType.Table) ? Resources.ResourceManager.GetString("Table", culture) : "");
					ttype += ((firstObject.Type == TableType.View) ? Resources.ResourceManager.GetString("View", culture) : "");
					ttype += ((firstObject.Type == TableType.Issue) ? Resources.ResourceManager.GetString("Issue", culture) : "");
				}
				base.Descriptions[i.CountryCode] = ((objects.Count > 1) ? string.Format(Resources.ResourceManager.GetString("ExportDescription", culture), ExportType.PDF.ToString(), objects.Count, ttype, base.StartTime.ToString(culture)) : string.Format(Resources.ResourceManager.GetString("ExportTableLabel", culture), ExportType.PDF.ToString(), ttype, firstObject.GetDescription(i)));
			}
		}

		private Export(ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection objects, IOptimization opt, UserDefinedExportProjects project)
		{
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			Type = ExportType.PDF;
			ExportObjects = objects;
			TransformationObjects = tempTableObjects;
			IsOverviewPdf = false;
			foreach (ITableObject o in ExportObjects)
			{
				foreach (IColumn c in o.Columns)
				{
					if (ColumnsVisible.ContainsKey(o.Id))
					{
						ColumnsVisible[o.Id].Add(new Tuple<int, bool>(c.Id, c.IsVisible));
						continue;
					}
					ColumnsVisible.Add(o.Id, new List<Tuple<int, bool>>
					{
						new Tuple<int, bool>(c.Id, c.IsVisible)
					});
				}
			}
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterDownload(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			StartJob(delegate(object obj, CancellationToken token)
			{
				DoUserDefinedExport((ViewboxDb.TableObjectCollection)((object[])obj)[0], (IOptimization)((object[])obj)[1], (ILanguage)((object[])obj)[2], (IUser)((object[])obj)[3], (global::ViewboxDb.ViewboxDb.ReaderExport)((object[])obj)[4], token);
			}, new object[5]
			{
				tempTableObjects,
				opt,
				ViewboxSession.Language,
				ViewboxSession.User,
				GetDelegateForUserDefinedProject(project)
			});
			ITableObject firstObject = ExportObjects.First();
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				string ttype = "";
				if (ExportObjects.Count > 1)
				{
					ttype += ((firstObject.Type == TableType.Table) ? Resources.ResourceManager.GetString("Tables", culture) : "");
					ttype += ((firstObject.Type == TableType.View) ? Resources.ResourceManager.GetString("Views", culture) : "");
					ttype += ((firstObject.Type == TableType.Issue) ? Resources.ResourceManager.GetString("Issues", culture) : "");
				}
				else
				{
					ttype += ((firstObject.Type == TableType.Table) ? Resources.ResourceManager.GetString("Table", culture) : "");
					ttype += ((firstObject.Type == TableType.View) ? Resources.ResourceManager.GetString("View", culture) : "");
					ttype += ((firstObject.Type == TableType.Issue) ? Resources.ResourceManager.GetString("Issue", culture) : "");
				}
				base.Descriptions[i.CountryCode] = ((tempTableObjects.Count > 1) ? string.Format(Resources.ResourceManager.GetString("ExportDescription", culture), ExportType.PDF.ToString(), tempTableObjects.Count, ttype, base.StartTime.ToString(culture)) : string.Format(Resources.ResourceManager.GetString("ExportTableLabel", culture), ExportType.PDF.ToString(), ttype, firstObject.GetDescription(i)));
			}
		}

		public static Export CreateUserDefined(ITableObjectCollection objects, IOptimization opt, UserDefinedExportProjects project)
		{
			return new Export(objects, opt, project);
		}

		public static Export CreateUserDefined(ViewboxDb.TableObjectCollection tempTableObjects, IOptimization opt, UserDefinedExportProjects project)
		{
			ITableObjectCollection objects = ViewboxApplication.Database.SystemDb.CreateTableObjectCollection();
			foreach (ViewboxDb.TableObject to in tempTableObjects)
			{
				objects.Add(to.Table);
			}
			return new Export(tempTableObjects, objects, opt, project);
		}

		private global::ViewboxDb.ViewboxDb.ReaderExport GetDelegateForUserDefinedProject(UserDefinedExportProjects project)
		{
			return project switch
			{
				UserDefinedExportProjects.MKN => ZipMKN, 
				UserDefinedExportProjects.Actebis => ZipActebis, 
				UserDefinedExportProjects.HofmeisterRechnung => ZipHofmeisterRechnung, 
				UserDefinedExportProjects.HofmeisterLieferschein => ZipHofmeisterLieferschein, 
				UserDefinedExportProjects.HofmeisterSofortRechnung => ZipHofmeisterSofortRechnung, 
				_ => throw new InvalidOperationException(), 
			};
		}

		private void DoUserDefinedExport(ITableObjectCollection objects, IOptimization opt, ILanguage language, IUser user, global::ViewboxDb.ViewboxDb.ReaderExport func, CancellationToken token)
		{
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			try
			{
				ViewboxSession.LoadExportData(func, ExportType.PDF, objects, token, opt, language, user, null, 0, 0L, "", getAllFields: true);
			}
			catch (Exception)
			{
				if (File.Exists(filename + ".zip"))
				{
					File.Delete(filename + ".zip");
				}
				throw;
			}
		}

		private void DoUserDefinedExport(ViewboxDb.TableObjectCollection tempTableObjects, IOptimization opt, ILanguage language, IUser user, global::ViewboxDb.ViewboxDb.ReaderExport func, CancellationToken token)
		{
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			try
			{
				ViewboxSession.LoadExportData(func, ExportType.PDF, tempTableObjects, token, opt, language, user, null, 0, 0L, "", getAllFields: true, ViewboxSession.Optimizations);
			}
			catch (Exception)
			{
				if (File.Exists(filename + ".zip"))
				{
					File.Delete(filename + ".zip");
				}
				throw;
			}
		}

		private void ZipActebis(DatabaseBase connection, ExportType type, ITableObjectCollection objects, List<string> sqlList, List<List<IColumn>> columnsList, CancellationToken token, ILanguage language, IOptimization opt, IUser user, string exportDescription, string fileName = "", ViewboxDb.TableObjectCollection tempTableObjects = null, IEnumerable<IOptimization> optimization = null)
		{
			_connection = connection;
			int i = 0;
			if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
			{
				Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
			}
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			using (ZipOutputStream zipstream = new ZipOutputStream(filename + ".zip"))
			{
				zipstream.EnableZip64 = Zip64Option.AsNecessary;
				using StreamWriter ostream = new StreamWriter(zipstream, Encoding.Default);
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "    ";
				settings.Encoding = Encoding.Default;
				foreach (string sql in sqlList)
				{
					ITableObject tobj = objects.ElementAt(i);
					string regex = "[" + string.Join(",", Path.GetInvalidFileNameChars()) + "]";
					string fname = ReplaceSpecialChars(Regex.Replace(tobj.GetDescription(language), (regex ?? "") ?? "", "_"));
					if (type == ExportType.PDF)
					{
						zipstream.PutNextEntry(fname + ".pdf");
						using IDataReader reader = _connection.ExecuteReader(sql);
						string bukrsValue = opt.GetOptimizationValue(OptimizationType.SplitTable);
						switch (bukrsValue)
						{
						case "4500":
						case "4650":
						case "4750":
							ExportActebisPdfLayout1(reader, zipstream, tobj, language, bukrsValue, token);
							break;
						case "4600":
						case "4700":
							ExportActebisPdfLayout2(reader, zipstream, tobj, language, bukrsValue, token);
							break;
						default:
							throw new InvalidOperationException();
						}
					}
					i++;
					ostream.Flush();
				}
			}
			_connection = null;
		}

		private void ExportActebisPdfLayout1(IDataReader reader, Stream ostream, ITableObject tobj, ILanguage language, string bukrsValue, CancellationToken token)
		{
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			DataTable dbTable = new DataTable();
			dbTable.Load(reader);
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.PageSizeAndOrientation = PageSize.A4;
			traits.MarginLeft = 25f;
			traits.MarginRight = 25f;
			traits.MarginTop = 315f;
			traits.MarginBottom = 75f;
			global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
			generator.Document.Add(CreateContentTableLayout1Actebis(dbTable, bukrsValue));
			generator.WriteFile(ostream, new Phrase(Resources.Table + ": " + tobj.GetDescription(language)), CreateHeadTableLayout1Actebis(generator, dbTable, bukrsValue), CreateFooterTableLayout1Actebis(dbTable, bukrsValue), new Font(bf, 7f, 0), (bukrsValue == "4650") ? "Sida" : "Side", 1);
		}

		private void ExportActebisPdfLayout2(IDataReader reader, Stream ostream, ITableObject tobj, ILanguage language, string bukrsValue, CancellationToken token)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			DataTable dbTable = new DataTable();
			dbTable.Load(reader);
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.PageSizeAndOrientation = PageSize.A4;
			traits.MarginLeft = 50f;
			traits.MarginRight = 50f;
			traits.MarginTop = 240f;
			traits.MarginBottom = 50f;
			global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
			generator.Document.Add(CreateContentTableLayout2Actebis(dbTable, bukrsValue));
			generator.WriteFile(ostream, new Phrase(Resources.Table + ": " + tobj.GetDescription(language)), CreateHeadTableLayout2Actebis(dbTable, bukrsValue), CreateFooterTableLayout2Actebis(dbTable), new Font(bf, 7f, 0), (bukrsValue == "4600") ? "Sida" : "Side", 1, currentAndCompleteSideNr: false);
		}

		private PdfPTable CreateHeadTableLayout1Actebis(global::PdfGenerator.PdfGenerator generator, DataTable dbTable, string bukrsValue)
		{
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 10f, 0, BaseColor.BLACK);
			Font boldNormalBlack = new Font(bf, 14f, 1, BaseColor.BLACK);
			Font boldBlack = new Font(bf, 10f, 1, BaseColor.BLACK);
			Font smallBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Dictionary<string, string> descriptions = new Dictionary<string, string>();
			if (bukrsValue == "4500")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Kundenadresse", "Kundeadresse" },
					{ "Lieferadresse", "Leveringsadresse" },
					{ "Information", "Information" },
					{ "Rechnungsnummer", "Fakturanummer" },
					{ "Rechnungsdatum", "Fakturadato" },
					{ "Debitor", "Debitor" },
					{ "KID", "KID" },
					{ "Faelligkeitsdatum", "Forfaldsdato" },
					{ "IhreReferenz", "Deres reference" },
					{ "SMGReferenz", "SMG reference" },
					{ "Telefon", "Telefon" },
					{ "Bestellnummer", "Ordrenummer" },
					{ "Bestelldatum", "Ordredato" },
					{ "Zahlungsbedingungen", "Betalingsbetingelser" },
					{ "Lieferungsbedingungen", "Leveringsbetingelser" }
				};
			}
			if (bukrsValue == "4650")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Kundenadresse", "Köpare" },
					{ "Lieferadresse", "Leveransadress" },
					{ "Information", "Information" },
					{ "Rechnungsnummer", "Fakturanummer" },
					{ "Rechnungsdatum", "Fakturadatum" },
					{ "Debitor", "Kundnummer" },
					{ "KID", "KID" },
					{ "Faelligkeitsdatum", "Förfallodatum" },
					{ "IhreReferenz", "Er referens" },
					{ "SMGReferenz", "Vår referens" },
					{ "Telefon", "Telefon" },
					{ "Bestellnummer", "Ordernummer" },
					{ "Bestelldatum", "Orderdatum" },
					{ "Zahlungsbedingungen", "Betalningsvillkor" },
					{ "Lieferungsbedingungen", "Leveransvillkor" }
				};
			}
			if (bukrsValue == "4750")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Kundenadresse", "Kundeadresse" },
					{ "Lieferadresse", "Leveringsadresse" },
					{ "Information", "Informasjon" },
					{ "Rechnungsnummer", "Fakturanummer" },
					{ "Rechnungsdatum", "Fakturadato" },
					{ "Debitor", "Kundenummer" },
					{ "KID", "KID" },
					{ "Faelligkeitsdatum", "Forfallsdato" },
					{ "IhreReferenz", "Deres referanse" },
					{ "SMGReferenz", "Vår referanse" },
					{ "Telefon", "Telefon" },
					{ "Bestellnummer", "Ordrenummer" },
					{ "Bestelldatum", "Ordredato" },
					{ "Zahlungsbedingungen", "Betalingsbetingelser" },
					{ "Lieferungsbedingungen", "Leveringsbetingelser" }
				};
			}
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 5f;
			PdfPTable headTable = new PdfPTable(1);
			headTable.DefaultCell.Border = 0;
			headTable.WidthPercentage = 100f;
			headTable.DefaultCell.Padding = 0f;
			PdfPCell firstLine = new PdfPCell(new Phrase((!(dbTable.Rows[0]["Kreditnota"] is DBNull)) ? "Kreditnota" : "Faktura", boldNormalBlack));
			firstLine.MinimumHeight = 10f;
			firstLine.Border = 0;
			firstLine.HorizontalAlignment = 2;
			firstLine.VerticalAlignment = 5;
			firstLine.Padding = 0f;
			headTable.AddCell(emptyCell);
			headTable.AddCell(firstLine);
			headTable.AddCell(emptyCell);
			PdfPTable nestedTable = new PdfPTable(new float[3] { 0.45f, 0.05f, 0.5f });
			nestedTable.DefaultCell.Border = 0;
			nestedTable.WidthPercentage = 100f;
			nestedTable.DefaultCell.Padding = 0f;
			PdfPTable rightTable = new PdfPTable(1);
			rightTable.DefaultCell.Border = 15;
			rightTable.WidthPercentage = 100f;
			rightTable.DefaultCell.Padding = 0f;
			PdfPTable rightNestedTable = new PdfPTable(1);
			rightNestedTable.DefaultCell.Border = 0;
			rightNestedTable.WidthPercentage = 100f;
			rightNestedTable.DefaultCell.Padding = 0f;
			PdfPCell informationCell = new PdfPCell(new Phrase(descriptions["Information"], boldBlack));
			informationCell.Border = 0;
			informationCell.BackgroundColor = new GrayColor(0.85f);
			informationCell.VerticalAlignment = 5;
			rightNestedTable.AddCell(informationCell);
			PdfPTable rightNestedNestedTable = new PdfPTable(new float[3] { 0.37f, 0.03f, 0.6f });
			rightNestedNestedTable.DefaultCell.Border = 0;
			rightNestedNestedTable.WidthPercentage = 100f;
			rightNestedNestedTable.DefaultCell.SetLeading(1.5f, 1f);
			rightNestedNestedTable.AddCell(new Phrase(descriptions["Rechnungsnummer"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase((!(dbTable.Rows[0]["Kreditnota"] is DBNull)) ? dbTable.Rows[0]["Kreditnota"].ToString() : dbTable.Rows[0]["Fakturanr"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["Rechnungsdatum"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Fakturadatum"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["Debitor"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Kunde_nr"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["KID"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["KID"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["Faelligkeitsdatum"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Forfallodatum"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["IhreReferenz"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Deres_ref"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["SMGReferenz"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Selger"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["Telefon"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Telefon"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["Bestellnummer"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Ordrenr"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["Bestelldatum"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["Ordredato"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["Zahlungsbedingungen"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["bet_bet"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(new Phrase(descriptions["Lieferungsbedingungen"], boldBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(new Phrase(dbTable.Rows[0]["lev_mate"].ToString(), normalBlack));
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(rightNestedNestedTable);
			rightTable.AddCell(rightNestedTable);
			PdfPTable heightTable = new PdfPTable(rightTable);
			heightTable.SetTotalWidth(new float[1] { 300f });
			float height = generator.CalculatePdfPTableHeight(heightTable);
			PdfPTable leftTable = new PdfPTable(1);
			leftTable.DefaultCell.Border = 0;
			leftTable.WidthPercentage = 100f;
			leftTable.DefaultCell.Padding = 0f;
			PdfPTable leftUpperTable = new PdfPTable(1);
			leftUpperTable.DefaultCell.Border = 15;
			leftUpperTable.WidthPercentage = 100f;
			PdfPTable leftUpperNestedTable = new PdfPTable(1);
			leftUpperNestedTable.DefaultCell.Border = 0;
			leftUpperNestedTable.WidthPercentage = 100f;
			leftUpperNestedTable.DefaultCell.SetLeading(1.5f, 1f);
			PdfPCell kundenadresseCell = new PdfPCell(new Phrase(descriptions["Kundenadresse"], boldBlack));
			kundenadresseCell.Border = 0;
			kundenadresseCell.BackgroundColor = new GrayColor(0.85f);
			kundenadresseCell.VerticalAlignment = 5;
			leftUpperNestedTable.AddCell(kundenadresseCell);
			string kundenadresse = string.Empty;
			if (!(dbTable.Rows[0]["Name1"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Name1"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Name1"], "\n");
			}
			if (!(dbTable.Rows[0]["Name2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Name2"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Name2"], "\n");
			}
			if (!(dbTable.Rows[0]["PO_Box"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["PO_Box"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, "PO Box. ", dbTable.Rows[0]["PO_Box"], "\n");
			}
			if (!(dbTable.Rows[0]["Stras"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Stras"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Stras"], "\n");
			}
			if ((!(dbTable.Rows[0]["Pstlz"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Pstlz"].ToString())) || (!(dbTable.Rows[0]["Ort01"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Ort01"].ToString())))
			{
				string pstlz2 = ((!(dbTable.Rows[0]["Pstlz"] is DBNull)) ? dbTable.Rows[0]["Pstlz"].ToString() : string.Empty);
				string ort2 = ((!(dbTable.Rows[0]["Ort01"] is DBNull)) ? dbTable.Rows[0]["Ort01"].ToString() : string.Empty);
				string adresse2 = pstlz2;
				adresse2 += ((!string.IsNullOrEmpty(adresse2) && !string.IsNullOrEmpty(ort2)) ? " " : string.Empty);
				adresse2 += ort2;
				kundenadresse = kundenadresse + adresse2 + "\n";
			}
			if (!(dbTable.Rows[0]["Ort02"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Ort02"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Ort02"], "\n");
			}
			leftUpperNestedTable.AddCell(new Phrase(kundenadresse, normalBlack));
			PdfPCell leftUpperNestedTableCell = new PdfPCell(leftUpperNestedTable);
			leftUpperNestedTableCell.FixedHeight = height / 2f;
			leftUpperTable.AddCell(leftUpperNestedTableCell);
			leftTable.AddCell(leftUpperTable);
			PdfPTable leftBottomTable = new PdfPTable(1);
			leftBottomTable.DefaultCell.Border = 15;
			leftBottomTable.WidthPercentage = 100f;
			PdfPTable leftBottomNestedTable = new PdfPTable(1);
			leftBottomNestedTable.DefaultCell.Border = 0;
			leftBottomNestedTable.WidthPercentage = 100f;
			leftBottomNestedTable.DefaultCell.SetLeading(1.5f, 1f);
			PdfPCell lieferAdresseCell = new PdfPCell(new Phrase(descriptions["Lieferadresse"], boldBlack));
			lieferAdresseCell.Border = 0;
			lieferAdresseCell.BackgroundColor = new GrayColor(0.85f);
			lieferAdresseCell.VerticalAlignment = 5;
			leftBottomNestedTable.AddCell(lieferAdresseCell);
			string lieferadresse = string.Empty;
			if (!(dbTable.Rows[0]["Lever_Name1"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Name1"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Name1"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Name2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Name2"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Name2"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Name3"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Name3"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Name3"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Name_Co"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Name_Co"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Name_Co"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Po_Box"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Po_Box"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Po_Box"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Str"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Str"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Str"], "\n");
			}
			if ((!(dbTable.Rows[0]["Lever_Post"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Post"].ToString())) || (!(dbTable.Rows[0]["Lever_City"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_City"].ToString())))
			{
				string pstlz = ((!(dbTable.Rows[0]["Lever_Post"] is DBNull)) ? dbTable.Rows[0]["Lever_Post"].ToString() : string.Empty);
				string ort = ((!(dbTable.Rows[0]["Lever_City"] is DBNull)) ? dbTable.Rows[0]["Lever_City"].ToString() : string.Empty);
				string adresse = pstlz;
				adresse += ((!string.IsNullOrEmpty(adresse) && !string.IsNullOrEmpty(ort)) ? " " : string.Empty);
				adresse += ort;
				lieferadresse = lieferadresse + adresse + "\n";
			}
			leftBottomNestedTable.AddCell(new Phrase(lieferadresse, normalBlack));
			PdfPCell leftBottomNestedTableCell = new PdfPCell(leftBottomNestedTable);
			leftBottomNestedTableCell.FixedHeight = height / 2f;
			leftBottomTable.AddCell(leftBottomNestedTableCell);
			leftTable.AddCell(leftBottomTable);
			nestedTable.AddCell(leftTable);
			nestedTable.AddCell(emptyCell);
			nestedTable.AddCell(rightTable);
			headTable.AddCell(nestedTable);
			PdfPCell lastLine = new PdfPCell(new Phrase(dbTable.Rows[0]["Info"].ToString(), smallBlack));
			lastLine.MinimumHeight = 10f;
			lastLine.Border = 0;
			lastLine.HorizontalAlignment = 0;
			lastLine.VerticalAlignment = 5;
			headTable.AddCell(emptyCell);
			headTable.AddCell(lastLine);
			headTable.AddCell(emptyCell);
			return headTable;
		}

		private PdfPTable CreateFooterTableLayout1Actebis(DataTable dbTable, string bukrsValue)
		{
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			Font smallBlack = new Font(bf, 6f, 0, BaseColor.BLACK);
			PdfPTable footerTable;
			if (bukrsValue == "4650")
			{
				footerTable = new PdfPTable(6);
				footerTable.DefaultCell.Border = 0;
				footerTable.WidthPercentage = 100f;
				footerTable.DefaultCell.Padding = 0f;
				footerTable.AddCell(new Phrase(string.Format("Postadress\n{0}\n{1}\n{2} {3}", dbTable.Rows[0]["firma1"], dbTable.Rows[0]["firma3"], dbTable.Rows[0]["firma4"], dbTable.Rows[0]["firma5"]), smallBlack));
				footerTable.AddCell(new Phrase(string.Format("Besöksadress\n{0}\n{1}\n{2}", dbTable.Rows[0]["firma6"], dbTable.Rows[0]["firma7"], dbTable.Rows[0]["firma8"]), smallBlack));
				footerTable.AddCell(new Phrase(string.Format("Godsadress\n{0}\n{1}\n{2} {3}", dbTable.Rows[0]["firma9"], dbTable.Rows[0]["firma10"], dbTable.Rows[0]["firma11"], dbTable.Rows[0]["firma12"]), smallBlack));
				footerTable.AddCell(new Phrase(string.Format("Telefon\n{0}\n{1}\n{2}\nTelefax\n{3}", dbTable.Rows[0]["firma13"], dbTable.Rows[0]["firma14"], dbTable.Rows[0]["firma15"], dbTable.Rows[0]["firma16"]), smallBlack));
				footerTable.AddCell(new Phrase(string.Format("Org nr {0}\nMomsnr {1}\nInnehar {2}\nSäte {3}", dbTable.Rows[0]["firma19"], dbTable.Rows[0]["firma20"], dbTable.Rows[0]["firma21"], dbTable.Rows[0]["firma22"]), smallBlack));
				footerTable.AddCell(new Phrase(string.Format("BANKGIRO POSTGIRO\n{0}   {1}", dbTable.Rows[0]["firma23"], dbTable.Rows[0]["firma24"]), smallBlack));
			}
			else
			{
				footerTable = new PdfPTable(3);
				footerTable.DefaultCell.Border = 0;
				footerTable.WidthPercentage = 100f;
				footerTable.DefaultCell.Padding = 0f;
				if (bukrsValue == "4500")
				{
					footerTable.AddCell(new Phrase(string.Format("{0}\n{1}\n{2} {3}", dbTable.Rows[0]["firma1"], dbTable.Rows[0]["firma2"], dbTable.Rows[0]["firma4"], dbTable.Rows[0]["firma5"]), smallBlack));
					footerTable.AddCell(new Phrase(string.Format("Tel.: {0}\nFax: {1}", dbTable.Rows[0]["firma13"], dbTable.Rows[0]["firma16"]), smallBlack));
					footerTable.AddCell(new Phrase(string.Format("Bankkonto:  {0}\nCVR/SE nr.: {1}", dbTable.Rows[0]["firma23"], dbTable.Rows[0]["firma26"]), smallBlack));
				}
				if (bukrsValue == "4750")
				{
					footerTable.AddCell(new Phrase(string.Format("{0}\n{1}\n{2}\n{3} {4}", dbTable.Rows[0]["firma1"], dbTable.Rows[0]["firma2"], dbTable.Rows[0]["firma3"], dbTable.Rows[0]["firma4"], dbTable.Rows[0]["firma5"]), smallBlack));
					footerTable.AddCell(new Phrase(string.Format("Tel.: {0}\nFax: {1}\nOrdre fax: {2}\nOrdre tel.: {3}", dbTable.Rows[0]["firma13"], dbTable.Rows[0]["firma16"], dbTable.Rows[0]["firma17"], dbTable.Rows[0]["firma18"]), smallBlack));
					footerTable.AddCell(new Phrase(string.Format("Bankgiro: {0}\nForetaksregisteret:\n{1}", dbTable.Rows[0]["firma23"], dbTable.Rows[0]["firma25"]), smallBlack));
				}
			}
			return footerTable;
		}

		private PdfPTable CreateContentTableLayout1Actebis(DataTable dbTable, string bukrsValue)
		{
			BaseFont bf = BaseFont.CreateFont("Helvetica", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 10f, 0, BaseColor.BLACK);
			Font boldBlack = new Font(bf, 10f, 1, BaseColor.BLACK);
			Dictionary<string, string> descriptions = new Dictionary<string, string>();
			if (bukrsValue == "4500")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Linie", "Linie" },
					{ "Materialnummer", "Varenr./Betegnelse" },
					{ "Anzahl", "Antal" },
					{ "Einheitspreis", "Pris pr. enhed" },
					{ "Rabatt", "Rabat" },
					{ "Betrag", "Beløb" },
					{ "InternetRabatt", "Internet rabat" },
					{ "Fracht", "Fragt" },
					{ "SummeEinzelposten", "Moms grundlag" },
					{ "MehrwertSteuer", "Moms" },
					{ "Total", "Total Sum incl. moms" }
				};
			}
			if (bukrsValue == "4650")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Linie", "Linje" },
					{ "Materialnummer", "Artikelnr./Benämning" },
					{ "Anzahl", "Antal" },
					{ "Einheitspreis", "Enhets pris" },
					{ "Rabatt", "Rabatt" },
					{ "Betrag", "Belopp" },
					{ "InternetRabatt", "Internet rabatt" },
					{ "Fracht", "Frakt" },
					{ "SummeEinzelposten", "Belopp före moms" },
					{ "MehrwertSteuer", "Moms" },
					{ "Total", "Summa Att Betala i" }
				};
			}
			if (bukrsValue == "4750")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Linie", "Linje" },
					{ "Materialnummer", "Materialnr./Beskrivelse" },
					{ "Anzahl", "Antall" },
					{ "Einheitspreis", "Pris pr. enhet" },
					{ "Rabatt", "Rabatt" },
					{ "Betrag", "Beløp" },
					{ "InternetRabatt", "Internett rabatt" },
					{ "Fracht", "Frakt" },
					{ "SummeEinzelposten", "Sum grunnlag" },
					{ "MehrwertSteuer", "Moms" },
					{ "Total", "Totalt" }
				};
			}
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 10f;
			PdfPTable table = new PdfPTable(1);
			table.DefaultCell.Border = 15;
			table.WidthPercentage = 100f;
			table.DefaultCell.Padding = 0f;
			table.HeaderRows = 1;
			PdfPTable headerTable = new PdfPTable(new float[6] { 0.075f, 0.43f, 0.075f, 0.16f, 0.13f, 0.13f });
			headerTable.DefaultCell.Border = 15;
			headerTable.WidthPercentage = 100f;
			headerTable.DefaultCell.Padding = 0f;
			GrayColor grayColor = new GrayColor(0.85f);
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Linie"], boldBlack), grayColor, 5, 0, 2, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Materialnummer"], boldBlack), grayColor, 5, 0, 2, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Anzahl"], boldBlack), grayColor, 5, 2, 2, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Einheitspreis"], boldBlack), grayColor, 5, 2, 2, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Rabatt"], boldBlack), grayColor, 5, 2, 2, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Betrag"], boldBlack), grayColor, 5, 2, 2, 5f, 2f));
			PdfPTable contentTable = new PdfPTable(new float[6] { 0.075f, 0.43f, 0.075f, 0.16f, 0.13f, 0.13f });
			contentTable.DefaultCell.Border = 15;
			contentTable.WidthPercentage = 100f;
			contentTable.DefaultCell.Padding = 0f;
			string decimals = new string('0', 2);
			string format = "{0:#,0." + decimals + "}";
			List<string> varenrList = new List<string>();
			for (int i2 = 0; i2 < dbTable.Rows.Count; i2++)
			{
				string warennr = dbTable.Rows[i2]["VARENR"].ToString();
				if (!string.IsNullOrEmpty(warennr) && !varenrList.Contains(warennr))
				{
					varenrList.Add(warennr);
				}
			}
			for (int n = 0; n < dbTable.Rows.Count; n++)
			{
				if (dbTable.Rows[n]["zusatz"].ToString().ToLower() == "x" || dbTable.Rows[n]["zusatz"].ToString().ToLower() == "y")
				{
					continue;
				}
				string warennr2 = dbTable.Rows[n]["VARENR"].ToString();
				if (!varenrList.Contains(warennr2))
				{
					continue;
				}
				varenrList.Remove(warennr2);
				contentTable.AddCell(CreateCell(new Phrase(dbTable.Rows[n]["Linie"].ToString(), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 5f));
				string beschreibung = dbTable.Rows[n]["BESKRIVELSE"].ToString();
				string ware = warennr2;
				ware += ((!string.IsNullOrEmpty(warennr2) && !string.IsNullOrEmpty(beschreibung)) ? "\n" : string.Empty);
				ware = ware + beschreibung + "\n";
				List<string> seriennummern = GetSeriennummern(dbTable, warennr2);
				Phrase warePhrase = new Phrase(new Chunk(ware, normalBlack));
				for (int j2 = 0; j2 < seriennummern.Count; j2++)
				{
					if (j2 == 0)
					{
						warePhrase.Add(new Chunk("Serienummer:\n", boldBlack));
					}
					if (j2 > 0)
					{
						warePhrase.Add(new Chunk("\n", normalBlack));
					}
					warePhrase.Add(new Chunk(seriennummern[j2], normalBlack));
				}
				contentTable.AddCell(CreateCell(warePhrase, BaseColor.WHITE, 4, 0, 0, 5f, 5f));
				string anzahl = dbTable.Rows[n]["ANT"].ToString();
				contentTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(anzahl)) ? string.Format(format, Convert.ToDecimal(anzahl).ToString(".#")) : anzahl, normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 5f));
				string enhetspris = dbTable.Rows[n]["ENHETSPRIS"].ToString();
				contentTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(enhetspris)) ? string.Format(format, Convert.ToDecimal(enhetspris)) : enhetspris, normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 5f));
				string rabatt = dbTable.Rows[n]["RABATT"].ToString();
				contentTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(rabatt)) ? string.Format(format, Convert.ToDecimal(rabatt)) : rabatt, normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 5f));
				string belop = dbTable.Rows[n]["BELOP"].ToString();
				contentTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(belop)) ? string.Format(format, Convert.ToDecimal(belop)) : belop, normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 5f));
			}
			PdfPTable summaryTable = new PdfPTable(new float[4] { 0.4f, 0.26f, 0.17f, 0.17f });
			summaryTable.DefaultCell.Border = 0;
			summaryTable.WidthPercentage = 100f;
			summaryTable.DefaultCell.Padding = 0f;
			for (int m = 0; m < dbTable.Rows.Count; m++)
			{
				if (!(dbTable.Rows[m]["zusatz"].ToString().ToLower() != "x"))
				{
					string beschreibung2 = dbTable.Rows[m]["Beskrivelse"].ToString();
					string proz = dbTable.Rows[m]["Proz"].ToString();
					if (!string.IsNullOrEmpty(beschreibung2) || !string.IsNullOrEmpty(proz))
					{
						summaryTable.AddCell(emptyCell);
						summaryTable.AddCell(CreateCell(new Phrase(beschreibung2, boldBlack), BaseColor.WHITE, 5, 0, 0, 0f, 2f));
						summaryTable.AddCell(CreateCell(new Phrase(string.IsNullOrEmpty(proz) ? proz : (proz + "%"), normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
						summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(dbTable.Rows[m]["Total"].ToString())) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[m]["Total"].ToString())) : dbTable.Rows[m]["Total"].ToString(), normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
					}
				}
			}
			summaryTable.AddCell(emptyCell);
			summaryTable.AddCell(CreateCell(new Phrase(descriptions["Fracht"], boldBlack), BaseColor.WHITE, 5, 0, 0, 0f, 2f));
			summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
			string fracht = dbTable.Rows[0]["Frakt"].ToString();
			summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(fracht)) ? string.Format(format, Convert.ToDecimal(fracht)) : fracht, normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
			summaryTable.AddCell(emptyCell);
			summaryTable.AddCell(CreateCell(new Phrase(descriptions["SummeEinzelposten"], boldBlack), BaseColor.WHITE, 5, 0, 0, 0f, 2f));
			summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
			string summeEinzelposten = dbTable.Rows[0]["Sum_gr_lag"].ToString();
			summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(summeEinzelposten)) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[0]["Sum_gr_lag"].ToString())) : summeEinzelposten, normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
			summaryTable.AddCell(emptyCell);
			summaryTable.AddCell(CreateCell(new Phrase(descriptions["MehrwertSteuer"], boldBlack), BaseColor.WHITE, 5, 0, 0, 0f, 2f));
			summaryTable.AddCell(CreateCell(new Phrase(string.Concat(dbTable.Rows[0]["Proz"], "%"), normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
			string mwsProz = dbTable.Rows[0]["MWS_Proz"].ToString();
			summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(mwsProz)) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[0]["MWS_Proz"].ToString())) : mwsProz, normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
			for (int l = 0; l < dbTable.Rows.Count; l++)
			{
				if (!(dbTable.Rows[l]["zusatz"].ToString().ToLower() != "y"))
				{
					summaryTable.AddCell(emptyCell);
					summaryTable.AddCell(CreateCell(new Phrase(dbTable.Rows[l]["Beskrivelse"].ToString(), boldBlack), BaseColor.WHITE, 5, 0, 0, 0f, 2f));
					summaryTable.AddCell(CreateCell(new Phrase(string.Concat(dbTable.Rows[l]["Proz"], "%"), normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
					summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(dbTable.Rows[l]["Total"].ToString())) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[l]["Total"].ToString())) : dbTable.Rows[l]["Total"].ToString(), normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f));
				}
			}
			summaryTable.AddCell(emptyCell);
			summaryTable.AddCell(CreateCell(new Phrase("", boldBlack), BaseColor.WHITE, 5, 0, 2, 0f, 1f, 1f));
			summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 2, 0f, 1f, 1f));
			summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 2, 0f, 1f, 1f));
			summaryTable.AddCell(emptyCell);
			summaryTable.AddCell(CreateCell(new Phrase(descriptions["Total"] + " " + dbTable.Rows[0]["WAERK"], boldBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f));
			summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 1, 0f, 1f));
			string total = dbTable.Rows[0]["TOTAL"].ToString();
			summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(total)) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[0]["TOTAL"].ToString())) : total, normalBlack), BaseColor.WHITE, 5, 2, 1, 0f, 1f));
			PdfPCell summaryTableCell = new PdfPCell(summaryTable);
			summaryTableCell.Border = 0;
			summaryTableCell.Padding = 20f;
			summaryTableCell.Colspan = 6;
			summaryTableCell.HorizontalAlignment = 2;
			for (int k = 0; k < 6; k++)
			{
				contentTable.AddCell(emptyCell);
			}
			contentTable.AddCell(summaryTableCell);
			for (int j = 0; j < 6; j++)
			{
				contentTable.AddCell(emptyCell);
			}
			contentTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 4, 2, 6));
			for (int i = 0; i < 4; i++)
			{
				contentTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 4, 2, 2));
			}
			contentTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 4, 2, 10));
			table.AddCell(headerTable);
			table.AddCell(contentTable);
			return table;
		}

		private List<string> GetSeriennummern(DataTable dbTable, string warennr)
		{
			List<string> seriennummern = new List<string>();
			for (int i = 0; i < dbTable.Rows.Count; i++)
			{
				if (dbTable.Rows[i]["zusatz"].ToString().ToLower() == "x")
				{
					continue;
				}
				string ware = dbTable.Rows[i]["VARENR"].ToString();
				if (!(ware != warennr))
				{
					string seriennummer = dbTable.Rows[i]["SERIENR"].ToString();
					if (!string.IsNullOrEmpty(seriennummer))
					{
						seriennummern.Add(seriennummer);
					}
				}
			}
			return seriennummern;
		}

		private PdfPTable CreateHeadTableLayout2Actebis(DataTable dbTable, string bukrsValue)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font boldNormalBlack = new Font(bf, 9f, 1, BaseColor.BLACK);
			Font boldBigBlack = new Font(bf, 12f, 1, BaseColor.BLACK);
			Font smallBlack = new Font(bf, 7f, 0, BaseColor.BLACK);
			Font boldSmallBlack = new Font(bf, 7f, 1, BaseColor.BLACK);
			Dictionary<string, string> descriptions = new Dictionary<string, string>();
			if (bukrsValue == "4600")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Lieferadresse", "Leveransadress" },
					{ "Telefon", "Telefon" },
					{ "Telefax", "Telefax" },
					{ "FirmaID", "Org.nr" },
					{ "Bankgiro", "Bankgiro" },
					{ "Postgiro", "Postgiro" },
					{ "Firmenregister", "" },
					{ "PO_Box", "P.O. BOX" },
					{ "Rechnungsdatum", "Fakt.datum" },
					{ "Verfallsdatum", "Förf.datum" },
					{ "Deres_ref", "Er ref" },
					{ "Lev_bet", "Vår ref" },
					{ "Best_Dato", "Best.datum" },
					{ "Rek_nr", "Ert rekvnr" },
					{ "Lev_mate", "Lev.sätt" },
					{ "Kunde_nr", "Kundnr." },
					{ "Ordrenr", "Vårt ordernr." },
					{ "Bet_bet", "Bet.villkor." },
					{ "Selger", "Säljare" },
					{ "ref", "ref" },
					{ "gjelder_ordre", "gjelder ordre" },
					{ "Faktura", "Faktura" }
				};
			}
			if (bukrsValue == "4700")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Lieferadresse", "Leveringsadresse" },
					{ "Telefon", "Telefon" },
					{ "Telefax", "Telefax" },
					{ "FirmaID", "" },
					{ "Bankgiro", "Bankgiro" },
					{ "Postgiro", "Postgiro" },
					{ "Firmenregister", "F.reg." },
					{ "PO_Box", "P.O. BOX" },
					{ "Rechnungsdatum", "Fakt.dato" },
					{ "Verfallsdatum", "Forf.dato" },
					{ "Deres_ref", "Deres ref" },
					{ "Lev_bet", "Lev bet" },
					{ "Best_Dato", "Best.Dato" },
					{ "Rek_nr", "Rek.nr" },
					{ "Lev_mate", "Lev.måte" },
					{ "Kunde_nr", "Kunde nr." },
					{ "Ordrenr", "Ordrenr." },
					{ "Bet_bet", "Bet.bet." },
					{ "Selger", "Selger" },
					{ "ref", "ref" },
					{ "gjelder_ordre", "gjelder ordre" },
					{ "Faktura", "Faktura" }
				};
			}
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 5f;
			PdfPTable headTable = new PdfPTable(1);
			headTable.DefaultCell.Border = 0;
			headTable.WidthPercentage = 100f;
			headTable.DefaultCell.Padding = 0f;
			PdfPTable firstTable = new PdfPTable(new float[2] { 0.65f, 0.35f });
			firstTable.DefaultCell.Border = 0;
			firstTable.WidthPercentage = 100f;
			firstTable.DefaultCell.Padding = 0f;
			string lieferadresse = descriptions["Lieferadresse"] + "\n";
			if (!(dbTable.Rows[0]["Lever_Name1"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Name1"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Name1"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Name2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Name2"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Name2"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Name3"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Name3"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Name3"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Name_Co"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Name_Co"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Name_Co"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Po_Box"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Po_Box"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Po_Box"], "\n");
			}
			if (!(dbTable.Rows[0]["Lever_Str"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Str"].ToString()))
			{
				lieferadresse = string.Concat(lieferadresse, dbTable.Rows[0]["Lever_Str"], "\n");
			}
			if ((!(dbTable.Rows[0]["Lever_Post"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_Post"].ToString())) || (!(dbTable.Rows[0]["Lever_City"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Lever_City"].ToString())))
			{
				string pstlz2 = ((!(dbTable.Rows[0]["Lever_Post"] is DBNull)) ? dbTable.Rows[0]["Lever_Post"].ToString() : string.Empty);
				string ort2 = ((!(dbTable.Rows[0]["Lever_City"] is DBNull)) ? dbTable.Rows[0]["Lever_City"].ToString() : string.Empty);
				string adressenString2 = pstlz2;
				adressenString2 += ((!string.IsNullOrEmpty(adressenString2) && !string.IsNullOrEmpty(ort2)) ? " " : string.Empty);
				adressenString2 += ort2;
				lieferadresse = lieferadresse + adressenString2 + "\n";
			}
			firstTable.AddCell(new Phrase(lieferadresse, normalBlack));
			PdfPTable firmaTable = new PdfPTable(new float[2] { 0.4f, 0.6f });
			firmaTable.DefaultCell.Border = 0;
			firmaTable.WidthPercentage = 100f;
			firmaTable.DefaultCell.Padding = 0f;
			if (bukrsValue == "4600")
			{
				PdfPCell firstLine2 = new PdfPCell(new Phrase(dbTable.Rows[0]["firma1"].ToString(), boldNormalBlack));
				firstLine2.Colspan = 2;
				firstLine2.Border = 0;
				firstLine2.Padding = 0f;
				firmaTable.AddCell(firstLine2);
				PdfPCell secondLine2 = new PdfPCell(new Phrase(dbTable.Rows[0]["firma2"].ToString(), boldNormalBlack));
				secondLine2.Colspan = 2;
				secondLine2.Border = 0;
				secondLine2.Padding = 0f;
				firmaTable.AddCell(secondLine2);
				PdfPCell thirdLine = new PdfPCell(new Phrase(string.Format("{0}, {1} {2}", dbTable.Rows[0]["firma3"], dbTable.Rows[0]["firma4"], dbTable.Rows[0]["firma5"]), boldNormalBlack));
				thirdLine.Colspan = 2;
				thirdLine.Border = 0;
				thirdLine.Padding = 0f;
				firmaTable.AddCell(thirdLine);
				firmaTable.AddCell(new Phrase(descriptions["Telefon"], boldNormalBlack));
				firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma13"].ToString(), boldNormalBlack));
				firmaTable.AddCell(new Phrase(descriptions["Telefax"], boldNormalBlack));
				firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma16"].ToString(), boldNormalBlack));
				firmaTable.AddCell(new Phrase(descriptions["FirmaID"], boldNormalBlack));
				firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma19"].ToString(), boldNormalBlack));
				if (!(dbTable.Rows[0]["firma23"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["firma23"].ToString()))
				{
					firmaTable.AddCell(new Phrase(descriptions["Bankgiro"], boldNormalBlack));
					firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma23"].ToString(), boldNormalBlack));
				}
				if (!(dbTable.Rows[0]["firma24"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["firma24"].ToString()))
				{
					firmaTable.AddCell(new Phrase(descriptions["Postgiro"], boldNormalBlack));
					firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma24"].ToString(), boldNormalBlack));
				}
			}
			if (bukrsValue == "4700")
			{
				PdfPCell firstLine = new PdfPCell(new Phrase(dbTable.Rows[0]["firma1"].ToString(), boldNormalBlack));
				firstLine.Colspan = 2;
				firstLine.Border = 0;
				firstLine.Padding = 0f;
				firmaTable.AddCell(firstLine);
				PdfPCell secondLine = new PdfPCell(new Phrase(string.Format("{0}, {1} {2}", dbTable.Rows[0]["firma3"], dbTable.Rows[0]["firma4"], dbTable.Rows[0]["firma5"]), boldNormalBlack));
				secondLine.Colspan = 2;
				secondLine.Border = 0;
				secondLine.Padding = 0f;
				firmaTable.AddCell(secondLine);
				firmaTable.AddCell(new Phrase(descriptions["Telefon"], boldNormalBlack));
				firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma13"].ToString(), boldNormalBlack));
				firmaTable.AddCell(new Phrase(descriptions["Telefax"], boldNormalBlack));
				firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma16"].ToString(), boldNormalBlack));
				firmaTable.AddCell(new Phrase(descriptions["Firmenregister"], boldNormalBlack));
				firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma25"].ToString(), boldNormalBlack));
				if (!(dbTable.Rows[0]["firma23"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["firma23"].ToString()))
				{
					firmaTable.AddCell(new Phrase(descriptions["Bankgiro"], boldNormalBlack));
					firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma23"].ToString(), boldNormalBlack));
				}
				if (!(dbTable.Rows[0]["firma24"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["firma24"].ToString()))
				{
					firmaTable.AddCell(new Phrase(descriptions["Postgiro"], boldNormalBlack));
					firmaTable.AddCell(new Phrase(dbTable.Rows[0]["firma24"].ToString(), boldNormalBlack));
				}
			}
			firstTable.AddCell(firmaTable);
			headTable.AddCell(firstTable);
			PdfPTable secondTable = new PdfPTable(new float[2] { 0.65f, 0.35f });
			secondTable.DefaultCell.Border = 0;
			secondTable.WidthPercentage = 100f;
			secondTable.DefaultCell.Padding = 0f;
			string adresse = string.Empty;
			if (!(dbTable.Rows[0]["Name1"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Name1"].ToString()))
			{
				adresse = string.Concat(adresse, dbTable.Rows[0]["Name1"], "\n");
			}
			if (!(dbTable.Rows[0]["Name2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Name2"].ToString()))
			{
				adresse = string.Concat(adresse, dbTable.Rows[0]["Name2"], "\n");
			}
			if (!(dbTable.Rows[0]["PO_Box"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["PO_Box"].ToString()))
			{
				adresse = string.Concat(adresse, descriptions["PO_Box"], " ", dbTable.Rows[0]["PO_Box"], "\n");
			}
			if (!(dbTable.Rows[0]["Stras"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Stras"].ToString()))
			{
				adresse = string.Concat(adresse, dbTable.Rows[0]["Stras"], "\n");
			}
			if ((!(dbTable.Rows[0]["Pstlz"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Pstlz"].ToString())) || (!(dbTable.Rows[0]["ORT01"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["ORT01"].ToString())))
			{
				string pstlz = ((!(dbTable.Rows[0]["Pstlz"] is DBNull)) ? dbTable.Rows[0]["Pstlz"].ToString() : string.Empty);
				string ort = ((!(dbTable.Rows[0]["ORT01"] is DBNull)) ? dbTable.Rows[0]["ORT01"].ToString() : string.Empty);
				string adressenString = pstlz;
				adressenString += ((!string.IsNullOrEmpty(adressenString) && !string.IsNullOrEmpty(ort)) ? " " : string.Empty);
				adressenString += ort;
				adresse = adresse + adressenString + "\n";
			}
			if (!(dbTable.Rows[0]["ORT02"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["ORT02"].ToString()))
			{
				adresse = string.Concat(adresse, dbTable.Rows[0]["ORT02"], "\n");
			}
			secondTable.AddCell(new Phrase(adresse, boldBigBlack));
			string fakturaKreditnota = string.Empty;
			fakturaKreditnota = ((!(dbTable.Rows[0]["Kreditnota"] is DBNull)) ? (fakturaKreditnota + "Kreditnota " + dbTable.Rows[0]["Kreditnota"]) : (fakturaKreditnota + "Faktura " + dbTable.Rows[0]["Fakturanr"]));
			PdfPCell fakturaLine = new PdfPCell(new Phrase(fakturaKreditnota, boldBigBlack));
			fakturaLine.Border = 0;
			fakturaLine.VerticalAlignment = 5;
			fakturaLine.Padding = 0f;
			secondTable.AddCell(fakturaLine);
			headTable.AddCell(emptyCell);
			headTable.AddCell(emptyCell);
			headTable.AddCell(secondTable);
			PdfPTable thirdTable = new PdfPTable(new float[6] { 0.12f, 0.21f, 0.12f, 0.21f, 0.12f, 0.22f });
			thirdTable.DefaultCell.Border = 0;
			thirdTable.WidthPercentage = 100f;
			thirdTable.DefaultCell.Padding = 2f;
			PdfPCell datumLine = new PdfPCell(new Phrase(string.Concat(descriptions["Rechnungsdatum"], ": ", dbTable.Rows[0]["Fakturadatum"], "  ", descriptions["Verfallsdatum"], ": ", dbTable.Rows[0]["Forfallodatum"]), boldNormalBlack));
			datumLine.Border = 0;
			datumLine.Padding = 0f;
			datumLine.Colspan = 6;
			thirdTable.AddCell(datumLine);
			thirdTable.AddCell(new Phrase(descriptions["Deres_ref"] + ":", boldSmallBlack));
			thirdTable.AddCell(new Phrase(dbTable.Rows[0]["Deres_ref"].ToString(), smallBlack));
			thirdTable.AddCell(new Phrase(descriptions["Lev_bet"] + ":", boldSmallBlack));
			thirdTable.AddCell(new Phrase(dbTable.Rows[0]["Lev_bet"].ToString(), smallBlack));
			thirdTable.AddCell(new Phrase(descriptions["Best_Dato"] + ":", boldSmallBlack));
			thirdTable.AddCell(new Phrase(dbTable.Rows[0]["Best_Dato"].ToString(), smallBlack));
			thirdTable.AddCell(new Phrase(descriptions["Rek_nr"] + ":", boldSmallBlack));
			thirdTable.AddCell(new Phrase(dbTable.Rows[0]["Rek_nr"].ToString(), smallBlack));
			thirdTable.AddCell(new Phrase(descriptions["Lev_mate"] + ":", boldSmallBlack));
			thirdTable.AddCell(new Phrase(dbTable.Rows[0]["Lev_mate"].ToString(), smallBlack));
			thirdTable.AddCell(new Phrase(descriptions["Kunde_nr"] + ":", boldSmallBlack));
			thirdTable.AddCell(new Phrase(dbTable.Rows[0]["Kunde_nr"].ToString(), smallBlack));
			thirdTable.AddCell(new Phrase(descriptions["Ordrenr"] + ":", boldSmallBlack));
			thirdTable.AddCell(new Phrase(dbTable.Rows[0]["Ordrenr"].ToString(), smallBlack));
			thirdTable.AddCell(new Phrase(descriptions["Bet_bet"] + ":", boldSmallBlack));
			thirdTable.AddCell(new Phrase(dbTable.Rows[0]["Bet_bet"].ToString(), smallBlack));
			thirdTable.AddCell(new Phrase(descriptions["Selger"] + ":", boldSmallBlack));
			thirdTable.AddCell(new Phrase(dbTable.Rows[0]["Selger"].ToString(), smallBlack));
			if ((!(dbTable.Rows[0]["ref_"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["ref_"].ToString())) || (!(dbTable.Rows[0]["gjelder_ordre"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["gjelder_ordre"].ToString())) || (!(dbTable.Rows[0]["FAKTURA"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["FAKTURA"].ToString())))
			{
				string text = string.Empty;
				if (!string.IsNullOrEmpty(dbTable.Rows[0]["ref_"].ToString()))
				{
					text = text + descriptions["ref"] + " " + dbTable.Rows[0]["ref_"];
				}
				if (!string.IsNullOrEmpty(dbTable.Rows[0]["gjelder_ordre"].ToString()))
				{
					text += (string.IsNullOrEmpty(text) ? (descriptions["gjelder_ordre"] + " " + dbTable.Rows[0]["gjelder_ordre"]) : (", " + descriptions["gjelder_ordre"] + " " + dbTable.Rows[0]["gjelder_ordre"]));
				}
				if (!string.IsNullOrEmpty(dbTable.Rows[0]["FAKTURA"].ToString()))
				{
					text += (string.IsNullOrEmpty(text) ? (descriptions["Faktura"] + " " + dbTable.Rows[0]["FAKTURA"]) : (", " + descriptions["Faktura"] + " " + dbTable.Rows[0]["FAKTURA"]));
				}
				PdfPCell refLine = new PdfPCell(new Phrase(text, boldNormalBlack));
				refLine.Border = 0;
				refLine.Padding = 0f;
				refLine.Colspan = 6;
				thirdTable.AddCell(refLine);
			}
			headTable.AddCell(emptyCell);
			headTable.AddCell(emptyCell);
			headTable.AddCell(emptyCell);
			headTable.AddCell(emptyCell);
			headTable.AddCell(emptyCell);
			headTable.AddCell(emptyCell);
			headTable.AddCell(thirdTable);
			return headTable;
		}

		private PdfPTable CreateFooterTableLayout2Actebis(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font smallBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			PdfPTable footerTable = new PdfPTable(1);
			footerTable.DefaultCell.Border = 0;
			footerTable.WidthPercentage = 100f;
			footerTable.DefaultCell.Padding = 0f;
			PdfPCell lastLine = new PdfPCell(new Phrase(dbTable.Rows[0]["Info"].ToString(), smallBlack));
			lastLine.MinimumHeight = 10f;
			lastLine.Border = 0;
			lastLine.HorizontalAlignment = 0;
			lastLine.VerticalAlignment = 5;
			footerTable.AddCell(lastLine);
			return footerTable;
		}

		private PdfPTable CreateContentTableLayout2Actebis(DataTable dbTable, string bukrsValue)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font boldNormalBlack = new Font(bf, 9f, 1, BaseColor.BLACK);
			Dictionary<string, string> descriptions = new Dictionary<string, string>();
			if (bukrsValue == "4600")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Varenr", "ARTIKEL" },
					{ "Serienr", "SERIENR." },
					{ "Beskrivelse", "BENÄMNING" },
					{ "Ant", "ANTAL" },
					{ "Enhetspris", "ENHETSPRIS" },
					{ "Rabatt", "RABATT" },
					{ "Belop", "BELOPP" },
					{ "Frakt", "Frakt" },
					{ "SumExklMoms", "Sum exkl moms" },
					{ "Total", "TOTAL" }
				};
			}
			if (bukrsValue == "4700")
			{
				descriptions = new Dictionary<string, string>
				{
					{ "Varenr", "VARENR." },
					{ "Serienr", "SERIENR." },
					{ "Beskrivelse", "BESKRIVELSE" },
					{ "Ant", "ANT." },
					{ "Enhetspris", "ENHETSPRIS" },
					{ "Rabatt", "RABATT" },
					{ "Belop", "BELØP" },
					{ "Frakt", "Frakt" },
					{ "SumExklMoms", "Sum gr.lag" },
					{ "Total", "TOTAL" }
				};
			}
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 10f;
			PdfPTable table = new PdfPTable(1);
			table.DefaultCell.Border = 0;
			table.WidthPercentage = 100f;
			table.DefaultCell.Padding = 0f;
			table.HeaderRows = 1;
			PdfPTable headerTable = new PdfPTable(new float[6] { 0.15f, 0.35f, 0.075f, 0.15f, 0.125f, 0.15f });
			headerTable.DefaultCell.Border = 3;
			headerTable.WidthPercentage = 100f;
			headerTable.DefaultCell.Padding = 0f;
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Varenr"] + ":", normalBlack), BaseColor.WHITE, 4, 0, 3, 0f, 2f, 0.5f, 33f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Beskrivelse"] + " /\n" + descriptions["Serienr"] + ":", normalBlack), BaseColor.WHITE, 4, 0, 3, 0f, 2f, 0.5f, 33f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Ant"] + ":", normalBlack), BaseColor.WHITE, 4, 2, 3, 0f, 2f, 0.5f, 33f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Enhetspris"] + ":", normalBlack), BaseColor.WHITE, 4, 2, 3, 0f, 2f, 0.5f, 33f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Rabatt"] + ":", normalBlack), BaseColor.WHITE, 4, 2, 3, 0f, 2f, 0.5f, 33f));
			headerTable.AddCell(CreateCell(new Phrase(descriptions["Belop"] + ":", normalBlack), BaseColor.WHITE, 4, 2, 3, 0f, 2f, 0.5f, 33f));
			PdfPTable contentTable = new PdfPTable(new float[6] { 0.15f, 0.35f, 0.075f, 0.15f, 0.125f, 0.15f });
			contentTable.DefaultCell.Border = 0;
			contentTable.WidthPercentage = 100f;
			contentTable.DefaultCell.Padding = 0f;
			string decimals = new string('0', 2);
			string format = "{0:#,0." + decimals + "}";
			List<string> varenrList = new List<string>();
			for (int n = 0; n < dbTable.Rows.Count; n++)
			{
				string warennr = dbTable.Rows[n]["VARENR"].ToString();
				if (!string.IsNullOrEmpty(warennr) && !varenrList.Contains(warennr))
				{
					varenrList.Add(warennr);
				}
			}
			for (int m = 0; m < dbTable.Rows.Count; m++)
			{
				if (dbTable.Rows[m]["zusatz"].ToString().ToLower() == "x" || dbTable.Rows[m]["zusatz"].ToString().ToLower() == "y")
				{
					continue;
				}
				string warennr2 = dbTable.Rows[m]["VARENR"].ToString();
				if (!varenrList.Contains(warennr2))
				{
					continue;
				}
				varenrList.Remove(warennr2);
				contentTable.AddCell(CreateCell(new Phrase(warennr2, normalBlack), BaseColor.WHITE, 4, 0, 0, 0f, 5f));
				string beschreibung = dbTable.Rows[m]["BESKRIVELSE"].ToString();
				string ware = beschreibung + "\n";
				List<string> seriennummern = GetSeriennummern(dbTable, warennr2);
				Phrase warePhrase = new Phrase(new Chunk(ware, normalBlack));
				for (int j2 = 0; j2 < seriennummern.Count; j2++)
				{
					if (j2 > 0)
					{
						warePhrase.Add(new Chunk("\n", normalBlack));
					}
					warePhrase.Add(new Chunk(seriennummern[j2], normalBlack));
				}
				contentTable.AddCell(CreateCell(warePhrase, BaseColor.WHITE, 4, 0, 0, 0f, 5f));
				string anzahl = dbTable.Rows[m]["ANT"].ToString();
				contentTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(anzahl)) ? string.Format(format, Convert.ToDecimal(anzahl).ToString(".#")) : anzahl, normalBlack), BaseColor.WHITE, 4, 2, 0, 0f, 5f));
				string enhetspris = dbTable.Rows[m]["ENHETSPRIS"].ToString();
				contentTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(enhetspris)) ? string.Format(format, Convert.ToDecimal(enhetspris)) : enhetspris, normalBlack), BaseColor.WHITE, 4, 2, 0, 0f, 5f));
				string rabatt = dbTable.Rows[m]["RABATT"].ToString();
				contentTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(rabatt)) ? string.Format(format, Convert.ToDecimal(rabatt)) : rabatt, normalBlack), BaseColor.WHITE, 4, 2, 0, 0f, 5f));
				string belop = dbTable.Rows[m]["BELOP"].ToString();
				contentTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(belop)) ? string.Format(format, Convert.ToDecimal(belop)) : belop, normalBlack), BaseColor.WHITE, 4, 2, 0, 0f, 5f));
			}
			PdfPTable summaryTable = new PdfPTable(new float[4] { 0.4f, 0.35f, 0.05f, 0.2f });
			summaryTable.DefaultCell.Border = 0;
			summaryTable.WidthPercentage = 100f;
			summaryTable.DefaultCell.Padding = 0f;
			for (int l = 0; l < dbTable.Rows.Count; l++)
			{
				if (!(dbTable.Rows[l]["zusatz"].ToString().ToLower() != "x"))
				{
					summaryTable.AddCell(emptyCell);
					string beschreibung2 = string.Concat(dbTable.Rows[l]["Beskrivelse"], " ", dbTable.Rows[l]["Proz"]);
					summaryTable.AddCell(CreateCell(new Phrase(string.IsNullOrEmpty(beschreibung2.Trim()) ? beschreibung2 : (beschreibung2 + "%"), boldNormalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
					summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
					summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(dbTable.Rows[l]["Total"].ToString())) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[l]["Total"].ToString())) : dbTable.Rows[l]["Total"].ToString(), normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
				}
			}
			summaryTable.AddCell(emptyCell);
			summaryTable.AddCell(CreateCell(new Phrase(descriptions["Frakt"], boldNormalBlack), BaseColor.WHITE, 5, 0, 2, 0f, 2f, 0.5f, 0f, setLeading: false));
			summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
			string fracht = dbTable.Rows[0]["Frakt"].ToString();
			summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(fracht)) ? string.Format(format, Convert.ToDecimal(fracht)) : fracht, normalBlack), BaseColor.WHITE, 5, 2, 2, 0f, 2f, 0.5f, 0f, setLeading: false));
			summaryTable.AddCell(emptyCell);
			summaryTable.AddCell(CreateCell(new Phrase(descriptions["SumExklMoms"], boldNormalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
			summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
			string summeEinzelposten = dbTable.Rows[0]["Sum_gr_lag"].ToString();
			summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(summeEinzelposten)) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[0]["Sum_gr_lag"].ToString())) : summeEinzelposten, normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
			summaryTable.AddCell(emptyCell);
			summaryTable.AddCell(CreateCell(new Phrase(string.Concat(dbTable.Rows[0]["Proz"], "%"), boldNormalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
			summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
			string mwsProz = dbTable.Rows[0]["MWS_Proz"].ToString();
			summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(mwsProz)) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[0]["MWS_Proz"].ToString())) : mwsProz, normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
			for (int k = 0; k < dbTable.Rows.Count; k++)
			{
				if (!(dbTable.Rows[k]["zusatz"].ToString().ToLower() != "y"))
				{
					summaryTable.AddCell(emptyCell);
					summaryTable.AddCell(CreateCell(new Phrase(string.Concat(dbTable.Rows[k]["Beskrivelse"], " ", dbTable.Rows[k]["Proz"], "%"), boldNormalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
					summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
					summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(dbTable.Rows[k]["Total"].ToString())) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[k]["Total"].ToString())) : dbTable.Rows[k]["Total"].ToString(), normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
				}
			}
			summaryTable.AddCell(emptyCell);
			summaryTable.AddCell(CreateCell(new Phrase(descriptions["Total"] + " " + dbTable.Rows[0]["WAERK"], boldNormalBlack), BaseColor.WHITE, 5, 0, 3, 0f, 2f, 0.5f, 0f, setLeading: false));
			summaryTable.AddCell(CreateCell(new Phrase("", normalBlack), BaseColor.WHITE, 5, 2, 0, 0f, 2f, 0.5f, 0f, setLeading: false));
			string total = dbTable.Rows[0]["TOTAL"].ToString();
			summaryTable.AddCell(CreateCell(new Phrase((!string.IsNullOrEmpty(total)) ? string.Format(format, Convert.ToDecimal(dbTable.Rows[0]["TOTAL"].ToString())) : total, normalBlack), BaseColor.WHITE, 5, 2, 3, 0f, 2f, 0.5f, 0f, setLeading: false));
			PdfPCell summaryTableCell = new PdfPCell(summaryTable);
			summaryTableCell.Border = 0;
			summaryTableCell.Padding = 20f;
			summaryTableCell.Colspan = 6;
			summaryTableCell.HorizontalAlignment = 2;
			for (int j = 0; j < 6; j++)
			{
				contentTable.AddCell(emptyCell);
			}
			contentTable.AddCell(summaryTableCell);
			for (int i = 0; i < 6; i++)
			{
				contentTable.AddCell(emptyCell);
			}
			table.AddCell(headerTable);
			table.AddCell(contentTable);
			return table;
		}

		private void ZipHofmeisterLieferschein(DatabaseBase connection, ExportType type, ITableObjectCollection objects, List<string> sqlList, List<List<IColumn>> columnsList, CancellationToken token, ILanguage language, IOptimization opt, IUser user, string exportDescription, string fileName = "", ViewboxDb.TableObjectCollection tempTableObjects = null, IEnumerable<IOptimization> optimization = null)
		{
			_connection = connection;
			int i = 0;
			if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
			{
				Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
			}
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			using (ZipOutputStream zipstream = new ZipOutputStream(filename + ".zip"))
			{
				zipstream.EnableZip64 = Zip64Option.AsNecessary;
				using StreamWriter ostream = new StreamWriter(zipstream, Encoding.Default);
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "    ";
				settings.Encoding = Encoding.Default;
				foreach (string sql in sqlList)
				{
					ITableObject tobj = objects.ElementAt(i);
					string regex = "[" + string.Join(",", Path.GetInvalidFileNameChars()) + "]";
					string fname = ReplaceSpecialChars(Regex.Replace(tobj.GetDescription(language), (regex ?? "") ?? "", "_"));
					if (type == ExportType.PDF)
					{
						zipstream.PutNextEntry(fname + ".pdf");
						using IDataReader reader = _connection.ExecuteReader(sql);
						ExportHofmeisterLieferscheinPdf(reader, zipstream, tobj, language, token);
					}
					i++;
					ostream.Flush();
				}
			}
			_connection = null;
		}

		private void ExportHofmeisterLieferscheinPdf(IDataReader reader, Stream ostream, ITableObject tobj, ILanguage language, CancellationToken token)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			DataTable dbTable = new DataTable();
			dbTable.Load(reader);
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.PageSizeAndOrientation = PageSize.A4;
			traits.MarginLeft = 25f;
			traits.MarginRight = 25f;
			traits.MarginTop = 285f;
			traits.MarginBottom = 140f;
			global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
			generator.Document.Add(CreateContentTableHofmeisterLieferschein(dbTable));
			generator.WriteFile(ostream, new Phrase(Resources.Table + ": " + tobj.GetDescription(language)), CreateHeadTableHofmeisterLieferschein(dbTable), CreateFooterTableHofmeisterLieferschein(dbTable), new Font(bf, 8f, 0), "Seite", 1, currentAndCompleteSideNr: true, " von ", 85f, printHeaderOnlyOnFirstPage: false, printFooterOnlyOnLastPage: true);
		}

		private PdfPTable CreateHeadTableHofmeisterLieferschein(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font bigNormalBlack = new Font(bf, 13f, 0, BaseColor.BLACK);
			Font boldNormalBlack = new Font(bf, 10f, 1, BaseColor.BLACK);
			Font boldSmallSmallBlack = new Font(bf, 6f, 1, BaseColor.BLACK);
			Font boldSmallBlack = new Font(bf, 8f, 1, BaseColor.BLACK);
			Font boldBigBlack = new Font(bf, 18f, 1, BaseColor.BLACK);
			Font boldMiddleBlack = new Font(bf, 11f, 1, BaseColor.BLACK);
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 5f;
			PdfPTable headTable = new PdfPTable(1);
			headTable.DefaultCell.Border = 0;
			headTable.WidthPercentage = 100f;
			headTable.DefaultCell.Padding = 0f;
			PdfPTable nestedTable = new PdfPTable(new float[3] { 0.48f, 0.14f, 0.38f });
			nestedTable.DefaultCell.Border = 0;
			nestedTable.WidthPercentage = 100f;
			nestedTable.DefaultCell.Padding = 0f;
			PdfPCell rechnungCell = new PdfPCell(new Phrase("Lieferschein", boldBigBlack));
			rechnungCell.Border = 0;
			rechnungCell.Padding = 2f;
			rechnungCell.VerticalAlignment = 6;
			PdfPTable gebietTourTable = new PdfPTable(1);
			gebietTourTable.DefaultCell.Border = 0;
			gebietTourTable.WidthPercentage = 100f;
			gebietTourTable.DefaultCell.Padding = 0f;
			if (!(dbTable.Rows[0]["gebiet"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["gebiet"].ToString()))
			{
				gebietTourTable.AddCell(CreateCell(new Phrase(dbTable.Rows[0]["gebiet"].ToString(), boldMiddleBlack), BaseColor.WHITE, 6, 0, 0, 10f));
			}
			if (!(dbTable.Rows[0]["tour"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["tour"].ToString()))
			{
				gebietTourTable.AddCell(CreateCell(new Phrase(dbTable.Rows[0]["tour"].ToString(), boldMiddleBlack), BaseColor.WHITE, 4, 2, 0, 5f));
			}
			PdfPTable addrHM = new PdfPTable(1);
			addrHM.DefaultCell.Border = 0;
			addrHM.WidthPercentage = 100f;
			addrHM.DefaultCell.Padding = 0f;
			if (!(dbTable.Rows[0]["Addr_HM"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Addr_HM"].ToString()))
			{
				PdfPCell addrHMCell = CreateCell(new Phrase(dbTable.Rows[0]["Addr_HM"].ToString(), boldSmallSmallBlack), BaseColor.WHITE, 5);
				addrHMCell.PaddingLeft = 30f;
				addrHM.AddCell(addrHMCell);
			}
			nestedTable.AddCell(addrHM);
			nestedTable.AddCell(gebietTourTable);
			nestedTable.AddCell(rechnungCell);
			PdfPTable rightTable = new PdfPTable(1);
			rightTable.DefaultCell.Border = 0;
			rightTable.WidthPercentage = 100f;
			rightTable.DefaultCell.Padding = 0f;
			PdfPTable rightNestedTable = new PdfPTable(new float[3] { 0.5f, 0.03f, 0.47f });
			rightNestedTable.DefaultCell.Border = 0;
			rightNestedTable.WidthPercentage = 100f;
			rightNestedTable.DefaultCell.Padding = 2f;
			rightNestedTable.DefaultCell.VerticalAlignment = 6;
			rightNestedTable.AddCell(new Phrase("Auftragsnr:", boldNormalBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["auftr_nr"].ToString(), boldNormalBlack));
			rightNestedTable.AddCell(new Phrase("Lieferdatum:", normalBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["liefdat"].ToString(), normalBlack));
			rightNestedTable.AddCell(new Phrase("Auftragsdatum:", normalBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["auftr_dat"].ToString(), normalBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(emptyCell);
			PdfPCell middleLine = new PdfPCell(new Phrase("Bitte stets angeben", normalBlack));
			middleLine.Colspan = 3;
			middleLine.Border = 0;
			middleLine.Padding = 2f;
			middleLine.VerticalAlignment = 6;
			rightNestedTable.AddCell(middleLine);
			rightNestedTable.AddCell(new Phrase("Kunden-Nr:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["kunden_nr"].ToString(), boldSmallBlack));
			rightTable.AddCell(emptyCell);
			rightTable.AddCell(emptyCell);
			rightTable.AddCell(rightNestedTable);
			PdfPTable leftTable = new PdfPTable(1);
			leftTable.DefaultCell.Border = 0;
			leftTable.WidthPercentage = 100f;
			leftTable.DefaultCell.PaddingLeft = 45f;
			leftTable.DefaultCell.PaddingRight = 0f;
			leftTable.DefaultCell.PaddingTop = 0f;
			leftTable.DefaultCell.PaddingBottom = 0f;
			PdfPTable leftNestedTable = new PdfPTable(1);
			leftNestedTable.DefaultCell.Border = 0;
			leftNestedTable.WidthPercentage = 100f;
			string kundenadresse = string.Empty;
			if (!(dbTable.Rows[0]["address1"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["address1"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["address1"], "\n");
			}
			if (!(dbTable.Rows[0]["address2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["address2"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["address2"], "\n");
			}
			if (!(dbTable.Rows[0]["address3"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["address3"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["address3"], "\n");
			}
			if (!(dbTable.Rows[0]["address4"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["address4"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["address4"], "\n");
			}
			if (!(dbTable.Rows[0]["postal_code"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["postal_code"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["postal_code"], " ");
			}
			if (!(dbTable.Rows[0]["city"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["city"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["city"], "\n");
			}
			leftNestedTable.AddCell(new Phrase(kundenadresse, boldMiddleBlack));
			leftTable.AddCell(leftNestedTable);
			PdfPTable middleTable = new PdfPTable(1);
			middleTable.DefaultCell.Border = 0;
			middleTable.WidthPercentage = 100f;
			middleTable.DefaultCell.PaddingLeft = 0f;
			middleTable.DefaultCell.PaddingRight = 0f;
			middleTable.DefaultCell.PaddingTop = 0f;
			middleTable.DefaultCell.PaddingBottom = 0f;
			PdfPTable middleNestedTable = new PdfPTable(1);
			middleNestedTable.DefaultCell.Border = 0;
			middleNestedTable.WidthPercentage = 100f;
			if (!(dbTable.Rows[0]["liefnr"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["liefnr"].ToString()))
			{
				middleNestedTable.AddCell(CreateCell(new Phrase(dbTable.Rows[0]["liefnr"].ToString(), bigNormalBlack), BaseColor.WHITE, 5, 0, 0, 10f, 30f));
			}
			middleTable.AddCell(middleNestedTable);
			nestedTable.AddCell(leftTable);
			nestedTable.AddCell(middleTable);
			nestedTable.AddCell(rightTable);
			string lieferAdresse = string.Empty;
			if (!(dbTable.Rows[0]["abw_lief_addr1"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["abw_lief_addr1"].ToString()))
			{
				lieferAdresse = string.Concat(lieferAdresse, dbTable.Rows[0]["abw_lief_addr1"], "\n");
			}
			if (!(dbTable.Rows[0]["abw_lief_addr2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["abw_lief_addr2"].ToString()))
			{
				lieferAdresse = string.Concat(lieferAdresse, dbTable.Rows[0]["abw_lief_addr2"], "\n");
			}
			if (!(dbTable.Rows[0]["abw_lief_plz"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["abw_lief_plz"].ToString()))
			{
				lieferAdresse = string.Concat(lieferAdresse, dbTable.Rows[0]["abw_lief_plz"], " ");
			}
			if (!(dbTable.Rows[0]["abw_lief_city"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["abw_lief_city"].ToString()))
			{
				lieferAdresse = string.Concat(lieferAdresse, dbTable.Rows[0]["abw_lief_city"], "\n");
			}
			string lieferinformationen = string.Empty;
			if (!(dbTable.Rows[0]["lief_info"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["lief_info"].ToString()))
			{
				lieferinformationen += dbTable.Rows[0]["lief_info"].ToString();
			}
			Phrase phrase = new Phrase(new Chunk("Lieferinformationen: ", normalBlack));
			phrase.Add(new Chunk(lieferinformationen, boldSmallBlack));
			PdfPCell lieferInformationenCell = new PdfPCell(phrase);
			lieferInformationenCell.Colspan = 3;
			lieferInformationenCell.Border = 0;
			lieferInformationenCell.PaddingLeft = 15f;
			for (int i = 0; i < 6; i++)
			{
				nestedTable.AddCell(emptyCell);
			}
			nestedTable.AddCell(lieferInformationenCell);
			if (!string.IsNullOrEmpty(lieferAdresse))
			{
				PdfPTable lieferAdresseTable = new PdfPTable(new float[2] { 0.17f, 0.83f });
				lieferAdresseTable.DefaultCell.Border = 0;
				lieferAdresseTable.DefaultCell.PaddingLeft = 0f;
				lieferAdresseTable.AddCell(new Phrase("Lieferadresse: ", boldNormalBlack));
				lieferAdresseTable.AddCell(new Phrase(lieferAdresse, boldNormalBlack));
				PdfPCell lieferAdresseCell = new PdfPCell(lieferAdresseTable);
				lieferAdresseCell.Colspan = 3;
				lieferAdresseCell.Border = 0;
				lieferAdresseCell.PaddingLeft = 15f;
				nestedTable.AddCell(lieferAdresseCell);
			}
			PdfPTable auftragsinfosTable = new PdfPTable(new float[3] { 0.25f, 0.5f, 0.25f });
			auftragsinfosTable.DefaultCell.Border = 0;
			auftragsinfosTable.WidthPercentage = 100f;
			auftragsinfosTable.DefaultCell.Padding = 10f;
			PdfPCell auftragsCell = CreateCell(new Phrase("Auftrag H&M: " + ((!(dbTable.Rows[0]["auftr_nr"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["auftr_nr"].ToString())) ? dbTable.Rows[0]["auftr_nr"].ToString() : ""), boldSmallBlack), BaseColor.WHITE, 5, 0, 0, 10f, 1f);
			auftragsinfosTable.AddCell(auftragsCell);
			PdfPCell sachbearbeiterCell = CreateCell(new Phrase("Sachbearbeiter H&M: " + ((!(dbTable.Rows[0]["sachb"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["sachb"].ToString())) ? dbTable.Rows[0]["sachb"].ToString() : ""), normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
			auftragsinfosTable.AddCell(sachbearbeiterCell);
			PdfPCell vaCell = CreateCell(new Phrase("VA: " + ((!(dbTable.Rows[0]["va"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["va"].ToString())) ? dbTable.Rows[0]["va"].ToString() : ""), normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
			vaCell.NoWrap = true;
			auftragsinfosTable.AddCell(vaCell);
			PdfPCell headCell = new PdfPCell(auftragsinfosTable);
			headCell.Colspan = 3;
			headCell.Border = 0;
			headCell.PaddingLeft = 5f;
			headCell.PaddingRight = 5f;
			headCell.PaddingTop = 0f;
			headCell.PaddingBottom = 0f;
			for (int k = 0; k < 6; k++)
			{
				nestedTable.AddCell(emptyCell);
			}
			nestedTable.AddCell(headCell);
			for (int j = 0; j < 3; j++)
			{
				nestedTable.AddCell(emptyCell);
			}
			headTable.AddCell(emptyCell);
			headTable.AddCell(nestedTable);
			return headTable;
		}

		private PdfPTable CreateFooterTableHofmeisterLieferschein(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font smallBlack = new Font(bf, 5.5f, 0, BaseColor.BLACK);
			Font smallBlackBold = new Font(bf, 5.5f, 1, BaseColor.BLACK);
			Font normalBlackBold = new Font(bf, 10f, 1, BaseColor.BLACK);
			PdfPTable footerTable = new PdfPTable(new float[5] { 0.03f, 0.25f, 0.18f, 0.21f, 0.33f });
			footerTable.WidthPercentage = 100f;
			footerTable.DefaultCell.Border = 0;
			footerTable.DefaultCell.Padding = 1f;
			footerTable.DefaultCell.VerticalAlignment = 4;
			footerTable.DefaultCell.HorizontalAlignment = 0;
			PdfPCell emptyLine = new PdfPCell();
			emptyLine.Colspan = 5;
			emptyLine.Border = 0;
			emptyLine.Padding = 0f;
			emptyLine.FixedHeight = 15f;
			PdfPCell leftLine = new PdfPCell(new Phrase("Ihre Bestellung: " + ((!(dbTable.Rows[0]["best_nr"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["best_nr"].ToString())) ? dbTable.Rows[0]["best_nr"].ToString() : ""), normalBlackBold));
			leftLine.Colspan = 3;
			leftLine.Border = 0;
			leftLine.Padding = 0f;
			footerTable.AddCell(leftLine);
			PdfPCell rightLine = new PdfPCell(new Phrase((!(dbTable.Rows[0]["verpackungsart"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["verpackungsart"].ToString())) ? dbTable.Rows[0]["verpackungsart"].ToString() : "", normalBlackBold));
			rightLine.Colspan = 2;
			rightLine.Border = 0;
			rightLine.Padding = 0f;
			footerTable.AddCell(rightLine);
			footerTable.AddCell(emptyLine);
			footerTable.AddCell(emptyLine);
			Chunk hm1 = new Chunk("Hofmeister&Meincke GmbH&Co. KG\n", smallBlackBold);
			Chunk hm2 = new Chunk("Postfach 10 47 09, 28047 Bremen\nCarsten-Dressler-Straße 6, 28279 Bremen\nTel.: 0421 8405-0, Fax: 0421 8405-201", smallBlack);
			Phrase hmPhrase = new Phrase();
			hmPhrase.Add(hm1);
			hmPhrase.Add(hm2);
			footerTable.AddCell("");
			footerTable.AddCell(hmPhrase);
			footerTable.AddCell(new Phrase("Steuer-Nr.: 73/524/21205\nUSt-IdNr.: DE114621869\nBankkonto: Bremer Landesbank\nBLZ 290 500 00\nKto.-Nr. 1 004 616 006", smallBlack));
			footerTable.AddCell(new Phrase("PE = Preiseinheit | ST = Stück\nSTH = 100 Stück | STT = 1000 Stück\nKG = Kilogramm | M = Meter\nL = Liter | QM = Quadratmeter", smallBlack));
			PdfPCell lastcell = new PdfPCell(new Phrase("Wir arbeiten ausschließlich aufgrund unserer Allgemeinen\nGeschäftsbedingungen, Stand 1/2007. Der Wortlaut wird\nihnen auf Wunsch gern zur Verfügung gestellt. Sie können\nihn ebenfalls auch im Internet unter www.hofmei.de\neinsehen und ausdrucken.", smallBlackBold));
			lastcell.Border = 0;
			lastcell.Padding = 1f;
			lastcell.VerticalAlignment = 4;
			lastcell.HorizontalAlignment = 0;
			lastcell.NoWrap = true;
			footerTable.AddCell(lastcell);
			PdfPCell lastEmptyLine = new PdfPCell();
			lastEmptyLine.Border = 0;
			lastEmptyLine.FixedHeight = 35f;
			lastEmptyLine.Colspan = 8;
			footerTable.AddCell(lastEmptyLine);
			return footerTable;
		}

		private PdfPTable CreateContentTableHofmeisterLieferschein(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font smallBlack = new Font(bf, 6f, 0, BaseColor.BLACK);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font boldBlack = new Font(bf, 8f, 1, BaseColor.BLACK);
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 10f;
			PdfPTable table = new PdfPTable(1);
			table.DefaultCell.Border = 0;
			table.WidthPercentage = 100f;
			table.DefaultCell.PaddingLeft = 10f;
			table.DefaultCell.PaddingRight = 10f;
			table.DefaultCell.PaddingTop = 0f;
			table.DefaultCell.PaddingBottom = 0f;
			table.HeaderRows = 1;
			PdfPTable headerTable = new PdfPTable(new float[10] { 0.045f, 0.12f, 0.03f, 0.25f, 0.11f, 0.11f, 0.04f, 0.1f, 0.09f, 0.105f });
			headerTable.DefaultCell.Border = 15;
			headerTable.WidthPercentage = 100f;
			headerTable.DefaultCell.Padding = 0f;
			headerTable.AddCell(CreateCell(new Phrase("Pos", boldBlack), BaseColor.WHITE, 5, 0, 0, 3f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Artikel", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("", boldBlack), BaseColor.WHITE, 5, 0, 0, 3f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Bezeichnung", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("best.\nMenge", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("gel.\nMenge", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("ME", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("E-Preis", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("RAB.%", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Netto", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			PdfPTable contentTable = new PdfPTable(new float[10] { 0.045f, 0.12f, 0.03f, 0.25f, 0.11f, 0.11f, 0.04f, 0.1f, 0.09f, 0.105f });
			contentTable.DefaultCell.Border = 0;
			contentTable.WidthPercentage = 100f;
			contentTable.DefaultCell.Padding = 0f;
			DataRow[] content = dbTable.Select("", "auftr_nr,Pos");
			for (int i = 0; i < content.Length; i++)
			{
				contentTable.AddCell(CreateCell(new Phrase(content[i]["Pos"].ToString(), boldBlack), BaseColor.WHITE, 4, 2, 0, 1f));
				contentTable.AddCell(CreateCell(new Phrase(content[i]["Artikel"].ToString(), boldBlack), BaseColor.WHITE, 4, 0, 0, 5f));
				if (!(dbTable.Rows[i]["Org"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["Org"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(content[i]["Org"].ToString(), normalBlack), BaseColor.WHITE, 6, 2, 0, 1f));
				}
				else
				{
					contentTable.AddCell(CreateCell(new Phrase("00", normalBlack), BaseColor.WHITE, 6, 2, 0, 1f));
				}
				string bez = content[i]["Bezeichnung"].ToString();
				if (!(dbTable.Rows[i]["Bezeichnung2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["Bezeichnung2"].ToString()))
				{
					bez = bez + "\n" + content[i]["Bezeichnung2"];
				}
				Chunk bez2 = new Chunk(bez, boldBlack);
				Phrase bezPhrase = new Phrase();
				bezPhrase.Add(bez2);
				if (!(dbTable.Rows[i]["Bem_Art_txt"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["Bem_Art_txt"].ToString()))
				{
					Chunk bez3 = new Chunk("\n" + dbTable.Rows[i]["Bem_Art_txt"], normalBlack);
					bezPhrase.Add(bez3);
				}
				if (!(dbTable.Rows[i]["lief_aus_rueck"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["lief_aus_rueck"].ToString()))
				{
					Chunk bez4 = new Chunk("\n" + dbTable.Rows[i]["lief_aus_rueck"], normalBlack);
					bezPhrase.Add(bez4);
				}
				else
				{
					bezPhrase.Add(new Chunk("\n ", normalBlack));
				}
				contentTable.AddCell(CreateCell(bezPhrase, BaseColor.WHITE, 4, 0, 0, 5f));
				if (!(dbTable.Rows[i]["best_Menge"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["best_Menge"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(content[i]["best_Menge"].ToString(), boldBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				if (!(dbTable.Rows[i]["gel_Menge"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["gel_Menge"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(content[i]["gel_Menge"].ToString(), boldBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				contentTable.AddCell(CreateCell(new Phrase(content[i]["ME"].ToString(), boldBlack), BaseColor.WHITE, 4, 1, 0, 0f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				if (!(dbTable.Rows[i]["E_Preis"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["E_Preis"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["E_Preis"]).ToString("#,##0.00", CultureInfo.InvariantCulture), boldBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				if (!(dbTable.Rows[i]["RAB"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["RAB"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(content[i]["RAB"].ToString(), boldBlack), BaseColor.WHITE, 4, 2, 0, 5f));
				}
				else
				{
					contentTable.AddCell("");
				}
				contentTable.AddCell("");
				if (!(dbTable.Rows[i]["Bem_Art"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["Bem_Art"].ToString()))
				{
					PdfPCell bemArtCell = CreateCell(new Phrase(content[i]["Bem_Art"].ToString(), smallBlack), BaseColor.WHITE);
					bemArtCell.Colspan = 3;
					bemArtCell.PaddingLeft = 10f;
					contentTable.AddCell(bemArtCell);
					for (int k = 0; k < 7; k++)
					{
						contentTable.AddCell(emptyCell);
					}
				}
				for (int j = 0; j < 10; j++)
				{
					contentTable.AddCell(emptyCell);
				}
			}
			table.AddCell(headerTable);
			table.AddCell(contentTable);
			return table;
		}

		private void ZipHofmeisterSofortRechnung(DatabaseBase connection, ExportType type, ITableObjectCollection objects, List<string> sqlList, List<List<IColumn>> columnsList, CancellationToken token, ILanguage language, IOptimization opt, IUser user, string exportDescription, string fileName = "", ViewboxDb.TableObjectCollection tempTableObjects = null, IEnumerable<IOptimization> optimization = null)
		{
			_connection = connection;
			int i = 0;
			if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
			{
				Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
			}
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			using (ZipOutputStream zipstream = new ZipOutputStream(filename + ".zip"))
			{
				zipstream.EnableZip64 = Zip64Option.AsNecessary;
				using StreamWriter ostream = new StreamWriter(zipstream, Encoding.Default);
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "    ";
				settings.Encoding = Encoding.Default;
				foreach (string sql in sqlList)
				{
					ITableObject tobj = objects.ElementAt(i);
					string regex = "[" + string.Join(",", Path.GetInvalidFileNameChars()) + "]";
					string fname = ReplaceSpecialChars(Regex.Replace(tobj.GetDescription(language), (regex ?? "") ?? "", "_"));
					if (type == ExportType.PDF)
					{
						zipstream.PutNextEntry(fname + ".pdf");
						using IDataReader reader = _connection.ExecuteReader(sql);
						ExportHofmeisterSofortRechnungPdf(reader, zipstream, tobj, language, token);
					}
					i++;
					ostream.Flush();
				}
			}
			_connection = null;
		}

		private void ExportHofmeisterSofortRechnungPdf(IDataReader reader, Stream ostream, ITableObject tobj, ILanguage language, CancellationToken token)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			DataTable dbTable = new DataTable();
			dbTable.Load(reader);
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.PageSizeAndOrientation = PageSize.A4;
			traits.MarginLeft = 25f;
			traits.MarginRight = 25f;
			traits.MarginTop = 285f;
			traits.MarginBottom = 140f;
			global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
			generator.Document.Add(CreateContentTableHofmeisterSofortRechnung(dbTable));
			generator.WriteFile(ostream, new Phrase(Resources.Table + ": " + tobj.GetDescription(language)), CreateHeadTableHofmeisterSofortRechnung(dbTable), CreateFooterTableHofmeisterSofortRechnung(dbTable), new Font(bf, 8f, 0), "Seite", 1, currentAndCompleteSideNr: true, " von ", 85f, printHeaderOnlyOnFirstPage: false, printFooterOnlyOnLastPage: true);
		}

		private PdfPTable CreateHeadTableHofmeisterSofortRechnung(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font bigNormalBlack = new Font(bf, 13f, 0, BaseColor.BLACK);
			Font boldNormalBlack = new Font(bf, 10f, 1, BaseColor.BLACK);
			Font boldSmallBlack = new Font(bf, 8f, 1, BaseColor.BLACK);
			Font boldBigBlack = new Font(bf, 18f, 1, BaseColor.BLACK);
			Font boldMiddleBlack = new Font(bf, 11f, 1, BaseColor.BLACK);
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 5f;
			PdfPTable headTable = new PdfPTable(1);
			headTable.DefaultCell.Border = 0;
			headTable.WidthPercentage = 100f;
			headTable.DefaultCell.Padding = 0f;
			PdfPTable nestedTable = new PdfPTable(new float[3] { 0.4f, 0.2f, 0.4f });
			nestedTable.DefaultCell.Border = 0;
			nestedTable.WidthPercentage = 100f;
			nestedTable.DefaultCell.Padding = 0f;
			PdfPCell rechnungCell = new PdfPCell(new Phrase("Sofortrechnung", boldBigBlack));
			rechnungCell.Border = 0;
			rechnungCell.Padding = 2f;
			rechnungCell.VerticalAlignment = 6;
			PdfPTable gebietTourTable = new PdfPTable(1);
			gebietTourTable.DefaultCell.Border = 0;
			gebietTourTable.WidthPercentage = 100f;
			gebietTourTable.DefaultCell.Padding = 0f;
			if (!(dbTable.Rows[0]["gebiet"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["gebiet"].ToString()))
			{
				gebietTourTable.AddCell(CreateCell(new Phrase(dbTable.Rows[0]["gebiet"].ToString(), boldMiddleBlack), BaseColor.WHITE, 6, 0, 0, 10f));
			}
			if (!(dbTable.Rows[0]["tour"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["tour"].ToString()))
			{
				gebietTourTable.AddCell(CreateCell(new Phrase(dbTable.Rows[0]["tour"].ToString(), boldMiddleBlack), BaseColor.WHITE, 4, 2, 0, 5f));
			}
			nestedTable.AddCell(emptyCell);
			nestedTable.AddCell(gebietTourTable);
			nestedTable.AddCell(rechnungCell);
			PdfPTable rightTable = new PdfPTable(1);
			rightTable.DefaultCell.Border = 0;
			rightTable.WidthPercentage = 100f;
			rightTable.DefaultCell.Padding = 0f;
			PdfPTable rightNestedTable = new PdfPTable(new float[3] { 0.5f, 0.03f, 0.47f });
			rightNestedTable.DefaultCell.Border = 0;
			rightNestedTable.WidthPercentage = 100f;
			rightNestedTable.DefaultCell.Padding = 2f;
			rightNestedTable.DefaultCell.VerticalAlignment = 6;
			rightNestedTable.AddCell(new Phrase("Rechnungsnr:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["rech_nr"].ToString(), boldNormalBlack));
			rightNestedTable.AddCell(new Phrase("Rechnungsdatum:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["rechdat"].ToString(), boldSmallBlack));
			rightNestedTable.AddCell(new Phrase("Auftragsnr:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["auftr_nr"].ToString(), boldSmallBlack));
			rightNestedTable.AddCell(new Phrase("Lieferdatum:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["liefdat"].ToString(), boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(emptyCell);
			PdfPCell middleLine = new PdfPCell(new Phrase("Bitte stets angeben", normalBlack));
			middleLine.Colspan = 3;
			middleLine.Border = 0;
			middleLine.Padding = 2f;
			middleLine.VerticalAlignment = 6;
			rightNestedTable.AddCell(middleLine);
			rightNestedTable.AddCell(new Phrase("Kunden-Nr:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["kunden_nr"].ToString(), boldSmallBlack));
			rightNestedTable.AddCell(new Phrase("Ihre USt-ID-Nr.:", boldSmallBlack));
			rightNestedTable.AddCell(emptyCell);
			rightNestedTable.AddCell(new Phrase(dbTable.Rows[0]["ust_id"].ToString(), boldSmallBlack));
			rightTable.AddCell(emptyCell);
			rightTable.AddCell(emptyCell);
			rightTable.AddCell(rightNestedTable);
			PdfPTable leftTable = new PdfPTable(1);
			leftTable.DefaultCell.Border = 0;
			leftTable.WidthPercentage = 100f;
			leftTable.DefaultCell.PaddingLeft = 45f;
			leftTable.DefaultCell.PaddingRight = 0f;
			leftTable.DefaultCell.PaddingTop = 0f;
			leftTable.DefaultCell.PaddingBottom = 0f;
			PdfPTable leftNestedTable = new PdfPTable(1);
			leftNestedTable.DefaultCell.Border = 0;
			leftNestedTable.WidthPercentage = 100f;
			string kundenadresse = string.Empty;
			if (!(dbTable.Rows[0]["Adressat1"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Adressat1"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Adressat1"], "\n");
			}
			if (!(dbTable.Rows[0]["Adressat2"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Adressat2"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Adressat2"], "\n");
			}
			if (!(dbTable.Rows[0]["Adressat3"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Adressat3"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Adressat3"], "\n");
			}
			if (!(dbTable.Rows[0]["Str_Nr"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["Str_Nr"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["Str_Nr"], "\n");
			}
			if (!(dbTable.Rows[0]["PLZ_Ort"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["PLZ_Ort"].ToString()))
			{
				kundenadresse = string.Concat(kundenadresse, dbTable.Rows[0]["PLZ_Ort"], "\n");
			}
			leftNestedTable.AddCell(new Phrase(kundenadresse, boldMiddleBlack));
			leftTable.AddCell(leftNestedTable);
			PdfPTable middleTable = new PdfPTable(1);
			middleTable.DefaultCell.Border = 0;
			middleTable.WidthPercentage = 100f;
			middleTable.DefaultCell.PaddingLeft = 0f;
			middleTable.DefaultCell.PaddingRight = 0f;
			middleTable.DefaultCell.PaddingTop = 0f;
			middleTable.DefaultCell.PaddingBottom = 0f;
			PdfPTable middleNestedTable = new PdfPTable(1);
			middleNestedTable.DefaultCell.Border = 0;
			middleNestedTable.WidthPercentage = 100f;
			if (!(dbTable.Rows[0]["liefnr"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["liefnr"].ToString()))
			{
				middleNestedTable.AddCell(CreateCell(new Phrase(dbTable.Rows[0]["liefnr"].ToString(), bigNormalBlack), BaseColor.WHITE, 5, 0, 0, 10f, 30f));
			}
			middleTable.AddCell(middleNestedTable);
			nestedTable.AddCell(leftTable);
			nestedTable.AddCell(middleTable);
			nestedTable.AddCell(rightTable);
			string lieferAdresse = string.Empty;
			if (!(dbTable.Rows[0]["lief_addr"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["lief_addr"].ToString()))
			{
				lieferAdresse += dbTable.Rows[0]["lief_addr"];
			}
			string lieferinformationen = string.Empty;
			if (!(dbTable.Rows[0]["lief_info"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["lief_info"].ToString()))
			{
				lieferinformationen += dbTable.Rows[0]["lief_info"].ToString();
			}
			Phrase phrase = new Phrase(new Chunk("Lieferinformationen: ", normalBlack));
			phrase.Add(new Chunk(lieferinformationen, boldSmallBlack));
			PdfPCell lieferInformationenCell = new PdfPCell(phrase);
			lieferInformationenCell.Colspan = 3;
			lieferInformationenCell.Border = 0;
			lieferInformationenCell.PaddingLeft = 15f;
			for (int i = 0; i < 6; i++)
			{
				nestedTable.AddCell(emptyCell);
			}
			nestedTable.AddCell(lieferInformationenCell);
			if (!string.IsNullOrEmpty(lieferAdresse))
			{
				PdfPCell lieferAdresseCell = new PdfPCell(new Phrase("Lieferadresse: " + lieferAdresse, boldSmallBlack));
				lieferAdresseCell.Colspan = 3;
				lieferAdresseCell.Border = 0;
				lieferAdresseCell.PaddingLeft = 15f;
				nestedTable.AddCell(lieferAdresseCell);
			}
			headTable.AddCell(emptyCell);
			headTable.AddCell(nestedTable);
			return headTable;
		}

		private PdfPTable CreateFooterTableHofmeisterSofortRechnung(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font smallBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font smallBlackBold = new Font(bf, 8f, 1, BaseColor.BLACK);
			Font normalBlackBold = new Font(bf, 10f, 1, BaseColor.BLACK);
			Font normalBlack = new Font(bf, 10f, 0, BaseColor.BLACK);
			PdfPTable footerTable = new PdfPTable(new float[8] { 0.13f, 0.14f, 0.1f, 0.1f, 0.1f, 0.09f, 0.09f, 0.25f });
			footerTable.WidthPercentage = 100f;
			footerTable.DefaultCell.Border = 0;
			footerTable.DefaultCell.Padding = 1f;
			footerTable.DefaultCell.VerticalAlignment = 6;
			footerTable.DefaultCell.HorizontalAlignment = 2;
			PdfPCell emptyLine = new PdfPCell();
			emptyLine.Colspan = 8;
			emptyLine.Border = 0;
			emptyLine.Padding = 0f;
			emptyLine.FixedHeight = 15f;
			PdfPCell leftLine = new PdfPCell(new Phrase("Ihre Bestellung: " + ((!(dbTable.Rows[0]["best_nr"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["best_nr"].ToString())) ? dbTable.Rows[0]["best_nr"].ToString() : ""), normalBlack));
			leftLine.Colspan = 3;
			leftLine.Border = 0;
			leftLine.Padding = 0f;
			footerTable.AddCell(leftLine);
			PdfPCell rightLine = new PdfPCell(new Phrase((!(dbTable.Rows[0]["verpackungsart"] is DBNull) || !string.IsNullOrEmpty(dbTable.Rows[0]["verpackungsart"].ToString())) ? dbTable.Rows[0]["verpackungsart"].ToString() : "", normalBlackBold));
			rightLine.Colspan = 5;
			rightLine.Border = 0;
			rightLine.Padding = 0f;
			footerTable.AddCell(rightLine);
			footerTable.AddCell(emptyLine);
			PdfPCell firstLine = new PdfPCell(new Phrase("Bezüglich der Entgeldminderung verweisen wir auf die aktuellen Zahlungs- und Konditionsvereinbarungen", smallBlackBold));
			firstLine.Colspan = 8;
			firstLine.Border = 0;
			firstLine.Padding = 0f;
			footerTable.AddCell(firstLine);
			footerTable.AddCell(emptyLine);
			footerTable.AddCell(new Phrase("ant. Verpa.", smallBlack));
			footerTable.AddCell(new Phrase("Rg.-Wert netto", smallBlackBold));
			footerTable.AddCell(new Phrase("AT-Wert", smallBlack));
			footerTable.AddCell(new Phrase("davon 10%", smallBlack));
			footerTable.AddCell(new Phrase("AT-Steuer", smallBlack));
			footerTable.AddCell(new Phrase("MWST", smallBlackBold));
			footerTable.AddCell(new Phrase("MWST", smallBlackBold));
			footerTable.AddCell(new Phrase("Endbetrag", smallBlackBold));
			if (!(dbTable.Rows[0]["ant_verp"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["ant_verp"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["ant_verp"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlack));
			}
			else
			{
				footerTable.AddCell("0.00");
			}
			if (!(dbTable.Rows[0]["rg_wert_netto"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["rg_wert_netto"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["rg_wert_netto"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlackBold));
			}
			else
			{
				footerTable.AddCell(new Phrase("0.00", smallBlackBold));
			}
			if (!(dbTable.Rows[0]["at_wert"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["at_wert"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["at_wert"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlack));
			}
			else
			{
				footerTable.AddCell("0.00");
			}
			if (!(dbTable.Rows[0]["davon_10proz"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["davon_10proz"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["davon_10proz"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlack));
			}
			else
			{
				footerTable.AddCell("0.00");
			}
			if (!(dbTable.Rows[0]["at_steuer"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["at_steuer"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["at_steuer"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlack));
			}
			else
			{
				footerTable.AddCell("0.00");
			}
			if (!(dbTable.Rows[0]["mwst_proz"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["mwst_proz"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["mwst_proz"]).ToString("#,##0", CultureInfo.InvariantCulture) + "%", smallBlackBold));
			}
			else
			{
				footerTable.AddCell(new Phrase("0%", smallBlackBold));
			}
			if (!(dbTable.Rows[0]["mwst_wert"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["mwst_wert"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["mwst_wert"]).ToString("#,##0.00", CultureInfo.InvariantCulture), smallBlackBold));
			}
			else
			{
				footerTable.AddCell(new Phrase("0.00", smallBlackBold));
			}
			if (!(dbTable.Rows[0]["endbetrag"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[0]["endbetrag"].ToString()))
			{
				footerTable.AddCell(new Phrase(Convert.ToDecimal(dbTable.Rows[0]["endbetrag"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlackBold));
			}
			else
			{
				footerTable.AddCell(new Phrase("0.00", smallBlackBold));
			}
			footerTable.AddCell(emptyLine);
			PdfPCell lastLine = new PdfPCell(new Phrase(dbTable.Rows[0]["Skontotextbed"].ToString(), smallBlack));
			lastLine.Colspan = 7;
			lastLine.Border = 0;
			lastLine.Padding = 0f;
			footerTable.AddCell(lastLine);
			PdfPCell waehrungCell = new PdfPCell(new Phrase(string.Format("Währung: {0}", dbTable.Rows[0]["Waehrung"]), smallBlack));
			waehrungCell.Border = 0;
			waehrungCell.Padding = 0f;
			waehrungCell.HorizontalAlignment = 2;
			footerTable.AddCell(waehrungCell);
			PdfPCell lastEmptyLine = new PdfPCell();
			lastEmptyLine.Border = 0;
			lastEmptyLine.FixedHeight = 35f;
			lastEmptyLine.Colspan = 8;
			footerTable.AddCell(lastEmptyLine);
			return footerTable;
		}

		private PdfPTable CreateContentTableHofmeisterSofortRechnung(DataTable dbTable)
		{
			BaseFont bf = BaseFont.CreateFont("Courier", "Cp1252", embedded: false);
			Font normalBlack = new Font(bf, 8f, 0, BaseColor.BLACK);
			Font boldBlack = new Font(bf, 8f, 1, BaseColor.BLACK);
			PdfPCell emptyCell = new PdfPCell();
			emptyCell.Border = 0;
			emptyCell.FixedHeight = 10f;
			PdfPTable table = new PdfPTable(1);
			table.DefaultCell.Border = 0;
			table.WidthPercentage = 100f;
			table.DefaultCell.PaddingLeft = 10f;
			table.DefaultCell.PaddingRight = 10f;
			table.DefaultCell.PaddingTop = 0f;
			table.DefaultCell.PaddingBottom = 0f;
			table.HeaderRows = 1;
			PdfPTable headerTable = new PdfPTable(new float[11]
			{
				0.045f, 0.12f, 0.03f, 0.23f, 0.07f, 0.04f, 0.065f, 0.105f, 0.09f, 0.1f,
				0.105f
			});
			headerTable.DefaultCell.Border = 15;
			headerTable.WidthPercentage = 100f;
			headerTable.DefaultCell.Padding = 0f;
			headerTable.AddCell(CreateCell(new Phrase("Pos", boldBlack), BaseColor.WHITE, 5, 0, 0, 3f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Artikel", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("", boldBlack), BaseColor.WHITE, 5, 0, 0, 3f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Artikelbezeichnung", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Menge", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("ME", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("MWSt", boldBlack), BaseColor.WHITE, 5, 0, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Brutto", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("RAB.%", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("E-Preis", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			headerTable.AddCell(CreateCell(new Phrase("Gesamt", boldBlack), BaseColor.WHITE, 5, 2, 0, 5f, 2f));
			PdfPTable contentTable = new PdfPTable(new float[11]
			{
				0.045f, 0.12f, 0.03f, 0.23f, 0.07f, 0.04f, 0.065f, 0.105f, 0.09f, 0.1f,
				0.105f
			});
			contentTable.DefaultCell.Border = 0;
			contentTable.WidthPercentage = 100f;
			contentTable.DefaultCell.Padding = 0f;
			DataRow[] content = dbTable.Select("", "auftr_nr,Pos");
			string lastAuftragsNr = string.Empty;
			for (int i = 0; i < content.Length; i++)
			{
				if (lastAuftragsNr != content[i]["auftr_nr"].ToString())
				{
					PdfPTable headTable = new PdfPTable(44);
					headTable.DefaultCell.Border = 0;
					headTable.WidthPercentage = 100f;
					headTable.DefaultCell.Padding = 0f;
					PdfPCell auftragsCell = CreateCell(new Phrase("Auftrag H&M: " + content[i]["auftr_nr"], boldBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					auftragsCell.Colspan = 10;
					headTable.AddCell(auftragsCell);
					PdfPCell sachbearbeiterCell = CreateCell(new Phrase("Sachbearbeiter H&M: " + content[i]["sachb"], normalBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					sachbearbeiterCell.Colspan = 20;
					headTable.AddCell(sachbearbeiterCell);
					PdfPCell vaCell = CreateCell(new Phrase("VA: " + content[i]["VA"], boldBlack), BaseColor.WHITE, 5, 0, 0, 0f, 1f);
					vaCell.Colspan = 14;
					vaCell.NoWrap = true;
					headTable.AddCell(vaCell);
					PdfPCell headCell = new PdfPCell(headTable);
					headCell.Colspan = 11;
					headCell.Border = 0;
					headCell.PaddingLeft = 5f;
					headCell.PaddingRight = 5f;
					headCell.PaddingTop = 0f;
					headCell.PaddingBottom = 0f;
					for (int l = 0; l < 22; l++)
					{
						contentTable.AddCell(emptyCell);
					}
					contentTable.AddCell(headCell);
					for (int k = 0; k < 11; k++)
					{
						contentTable.AddCell(emptyCell);
					}
				}
				contentTable.AddCell(CreateCell(new Phrase(content[i]["Pos"].ToString(), normalBlack), BaseColor.WHITE, 4, 2, 0, 1f));
				contentTable.AddCell(CreateCell(new Phrase(content[i]["Artikel"].ToString(), normalBlack), BaseColor.WHITE, 4, 0, 0, 5f));
				if (!(dbTable.Rows[i]["Org"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["Org"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(content[i]["Org"].ToString(), normalBlack), BaseColor.WHITE, 6, 2, 0, 1f));
				}
				else
				{
					contentTable.AddCell(CreateCell(new Phrase("00", normalBlack), BaseColor.WHITE, 6, 2, 0, 1f));
				}
				string bez = content[i]["Bezeichnung"].ToString();
				bez = ((dbTable.Rows[i]["Bezeichnung2"] is DBNull || string.IsNullOrEmpty(dbTable.Rows[i]["Bezeichnung2"].ToString())) ? (bez + "\n ") : string.Concat(bez, "\n", content[i]["Bezeichnung2"], "\n "));
				contentTable.AddCell(CreateCell(new Phrase(bez, normalBlack), BaseColor.WHITE, 4, 0, 0, 5f));
				if (!(dbTable.Rows[i]["Menge"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["Menge"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["Menge"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				contentTable.AddCell(CreateCell(new Phrase(content[i]["ME"].ToString(), normalBlack), BaseColor.WHITE, 4, 1, 0, 0f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				if (!(dbTable.Rows[i]["mwst"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["mwst"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["mwst"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				if (!(dbTable.Rows[i]["Brutto"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["Brutto"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["Brutto"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				if (!(dbTable.Rows[i]["RAB"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["RAB"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(content[i]["RAB"].ToString(), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f));
				}
				else
				{
					contentTable.AddCell("");
				}
				if (!(dbTable.Rows[i]["E_Preis"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["E_Preis"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["E_Preis"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				if (!(dbTable.Rows[i]["Gesamt"] is DBNull) && !string.IsNullOrEmpty(dbTable.Rows[i]["Gesamt"].ToString()))
				{
					contentTable.AddCell(CreateCell(new Phrase(Convert.ToDecimal(content[i]["Gesamt"]).ToString("#,##0.00", CultureInfo.InvariantCulture), normalBlack), BaseColor.WHITE, 4, 2, 0, 5f, 0f, 0.5f, 0f, setLeading: true, noWrap: true));
				}
				else
				{
					contentTable.AddCell("");
				}
				for (int j = 0; j < 11; j++)
				{
					contentTable.AddCell(emptyCell);
				}
				lastAuftragsNr = content[i]["auftr_nr"].ToString();
			}
			table.AddCell(headerTable);
			table.AddCell(contentTable);
			return table;
		}

		public new static Export Find(string key)
		{
			return (key != null && _jobs.ContainsKey(key)) ? _jobs[key] : null;
		}

		public static Export Create(ITableObjectCollection objects, IOptimization opt, ExportType type, bool getAllFields, string filter = "")
		{
			return new Export(type, objects, opt, getAllFields, filter);
		}

		public static Export Create(ViewboxDb.TableObjectCollection tempTableObjects, IOptimization opt, ExportType type, bool getAllFields, string fromTab, IEnumerable<IOptimization> optimization = null)
		{
			ITableObjectCollection objects = ViewboxApplication.Database.SystemDb.CreateTableObjectCollection();
			foreach (ViewboxDb.TableObject to in tempTableObjects)
			{
				objects.Add(to.Table);
			}
			return new Export(type, tempTableObjects, objects, opt, getAllFields, fromTab, optimization);
		}

		public static Export Create(ITableObjectCollection objects, ExportType type, List<Tuple<string, string, string>> tableOptions = null)
		{
			return new Export(type, objects, tableOptions);
		}

		private Export(ExportType type, ITableObjectCollection objects, IOptimization opt, bool getAllColumns, string filter = "")
		{
			Export export = this;
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			ExportObjects = objects;
			Type = type;
			IsOverviewPdf = false;
			foreach (ITableObject o in ExportObjects)
			{
				foreach (IColumn c in o.Columns)
				{
					if (ColumnsVisible.ContainsKey(o.Id))
					{
						ColumnsVisible[o.Id].Add(new Tuple<int, bool>(c.Id, c.IsVisible));
						continue;
					}
					ColumnsVisible.Add(o.Id, new List<Tuple<int, bool>>
					{
						new Tuple<int, bool>(c.Id, c.IsVisible)
					});
				}
			}
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterDownload(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			string exportDescription = null;
			IProperty property = ViewboxApplication.FindProperty("export_description");
			if (property != null && property.Value.ToLower() == "true")
			{
				exportDescription = ViewboxSession.GetJobInfo(base.Key, (objects.First().Type != TableType.Table) ? true : false);
			}
			StartJob(delegate(object obj, CancellationToken token)
			{
				export.DoExport((ExportType)((object[])obj)[0], (ITableObjectCollection)((object[])obj)[1], (IOptimization)((object[])obj)[2], (ILanguage)((object[])obj)[3], (IUser)((object[])obj)[4], (string)((object[])obj)[5], (bool)((object[])obj)[6], token, filter);
			}, new object[7]
			{
				type,
				objects,
				opt,
				ViewboxSession.Language,
				ViewboxSession.User,
				exportDescription,
				getAllColumns
			});
			ITableObject firstObject = ExportObjects.First();
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				string ttype = "";
				if (ExportObjects.Count > 1)
				{
					if (firstObject.Type == TableType.Table)
					{
						ttype = Resources.ResourceManager.GetString("Tables", culture);
					}
					if (firstObject.Type == TableType.View)
					{
						ttype = Resources.ResourceManager.GetString("Views", culture);
					}
					if (firstObject.Type == TableType.Issue)
					{
						ttype = Resources.ResourceManager.GetString("Issues", culture);
					}
				}
				else
				{
					if (firstObject.Type == TableType.Table)
					{
						ttype = Resources.ResourceManager.GetString("Table", culture);
					}
					if (firstObject.Type == TableType.View)
					{
						ttype = Resources.ResourceManager.GetString("View", culture);
					}
					if (firstObject.Type == TableType.Issue)
					{
						ttype = Resources.ResourceManager.GetString("Issue", culture);
					}
				}
				base.Descriptions[i.CountryCode] = ((objects.Count > 1) ? string.Format(Resources.ResourceManager.GetString("ExportDescription", culture), type.ToString(), objects.Count, ttype) : string.Format(Resources.ResourceManager.GetString("ExportTableLabel", culture), type.ToString(), ttype, firstObject.GetDescription(i)));
			}
		}

		private Export(ExportType type, ViewboxDb.TableObjectCollection tempTableObjects, ITableObjectCollection objects, IOptimization opt, bool getAllFields, string fromTab, IEnumerable<IOptimization> optimization = null)
		{
			Export export = this;
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			Type = type;
			ExportObjects = objects;
			TransformationObjects = tempTableObjects;
			IsOverviewPdf = false;
			foreach (ITableObject o in ExportObjects)
			{
				foreach (IColumn c in o.Columns)
				{
					if (ColumnsVisible.ContainsKey(o.Id))
					{
						ColumnsVisible[o.Id].Add(new Tuple<int, bool>(c.Id, c.IsVisible));
						continue;
					}
					ColumnsVisible.Add(o.Id, new List<Tuple<int, bool>>
					{
						new Tuple<int, bool>(c.Id, c.IsVisible)
					});
				}
			}
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterDownload(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			string exportDescription = null;
			IProperty property = ViewboxApplication.FindProperty("export_description");
			if (property != null && property.Value.ToLower() == "true")
			{
				exportDescription = ViewboxSession.GetJobInfo(base.Key, (tempTableObjects.First().Table.Type != TableType.Table) ? true : false);
			}
			StartJob(delegate(object obj, CancellationToken token)
			{
				export.DoExport((ExportType)((object[])obj)[0], (ViewboxDb.TableObjectCollection)((object[])obj)[1], (IOptimization)((object[])obj)[2], (ILanguage)((object[])obj)[3], (IUser)((object[])obj)[4], (string)((object[])obj)[5], (bool)((object[])obj)[6], token, optimization);
			}, new object[7]
			{
				type,
				tempTableObjects,
				opt,
				ViewboxSession.Language,
				ViewboxSession.User,
				exportDescription,
				getAllFields
			});
			ITableObject firstObject = ExportObjects.First();
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				string ttype = "";
				if (ExportObjects.Count > 1)
				{
					if (firstObject.Type == TableType.Table)
					{
						ttype = Resources.ResourceManager.GetString("Tables", culture);
					}
					if (firstObject.Type == TableType.View)
					{
						ttype = Resources.ResourceManager.GetString("Views", culture);
					}
					if (firstObject.Type == TableType.Issue || (firstObject.Id < 0 && tempTableObjects[firstObject.Id].FilterIssue != null))
					{
						ttype = Resources.ResourceManager.GetString("Issues", culture);
					}
				}
				else
				{
					if (firstObject.Type == TableType.Table)
					{
						ttype = Resources.ResourceManager.GetString("Table", culture);
					}
					if (firstObject.Type == TableType.View)
					{
						ttype = Resources.ResourceManager.GetString("View", culture);
					}
					if (firstObject.Type == TableType.Issue || (firstObject.Id < 0 && tempTableObjects[firstObject.Id].FilterIssue != null) || fromTab == Resources.Issue)
					{
						ttype = Resources.ResourceManager.GetString("Issue", culture);
					}
				}
				base.Descriptions[i.CountryCode] = ((tempTableObjects.Count > 1) ? string.Format(Resources.ResourceManager.GetString("ExportDescription", culture), type.ToString(), tempTableObjects.Count, ttype, base.StartTime.ToString(culture)) : string.Format(Resources.ResourceManager.GetString("ExportTableLabel", culture), type.ToString(), ttype, firstObject.GetDescription(i)));
			}
		}

		private Export(ExportType type, ITableObjectCollection objects, List<Tuple<string, string, string>> tableOptions = null)
		{
			_jobs.Add(base.Key, this);
			Base.AddJob(base.Key, this);
			ExportObjects = objects;
			Type = type;
			IsOverviewPdf = true;
			base.JobFinished += delegate(Base j)
			{
				Janitor.RegisterDownload(j.Key);
			};
			base.JobCrashed += delegate(Base j, Exception e)
			{
				Janitor.RegisterError(j.Key, e);
			};
			StartJob(delegate(object obj, CancellationToken token)
			{
				ExportTableOverview((ITableObjectCollection)((object[])obj)[1], (IOptimization)((object[])obj)[2], (ILanguage)((object[])obj)[3], (IUser)((object[])obj)[4], token, (List<Tuple<string, string, string>>)((object[])obj)[5]);
			}, new object[6]
			{
				type,
				objects,
				ViewboxSession.Optimizations.LastOrDefault(),
				ViewboxSession.Language,
				ViewboxSession.User,
				tableOptions
			});
			if (ExportObjects == null || ExportObjects.Count == 0)
			{
				return;
			}
			ITableObject firstObject = ExportObjects.First();
			foreach (ILanguage i in ViewboxApplication.Languages)
			{
				CultureInfo culture = new CultureInfo(i.CountryCode);
				string ttype = "";
				ttype += ((firstObject.Type == TableType.Table) ? Resources.ResourceManager.GetString("Tables", culture) : "");
				ttype += ((firstObject.Type == TableType.View) ? Resources.ResourceManager.GetString("Views", culture) : "");
				ttype += ((firstObject.Type == TableType.Issue) ? Resources.ResourceManager.GetString("Issues", culture) : "");
				base.Descriptions[i.CountryCode] = string.Format(Resources.ResourceManager.GetString("ExportOverview", culture), type.ToString(), Resources.ResourceManager.GetString("Overview", culture), ttype);
			}
		}

		public string GetMailText()
		{
			string link = ViewboxApplication.ViewboxBasePath + "/Export/Download?key=" + base.Key;
			string cultureName = Thread.CurrentThread.CurrentCulture.Name.Substring(0, 2);
			return base.Descriptions[cultureName] + Environment.NewLine + "Link: " + link;
		}

		public override void Cancel()
		{
			base.Cancel();
			try
			{
				_connection.CancelCommand();
				_connection = null;
			}
			catch
			{
			}
		}

		public override void CleanUp()
		{
			base.CleanUp();
			_jobs.Remove(base.Key);
			try
			{
				string filename = ViewboxApplication.TemporaryDirectory + base.Key + ".zip";
				if (File.Exists(filename))
				{
					File.Delete(filename);
				}
			}
			catch
			{
			}
		}

		private void DoZip(ITableObject tobj, IDataReader reader, ZipOutputStream zipstream, ExportType type, List<IColumn> columns, ILanguage language, string exportDescription, CancellationToken token, IUser user, bool hasGroupSubtotal = false, IOptimization opt = null, SimpleTableModel model = null, ViewboxDb.TableObject tempTableObject = null)
		{
			int numfields = reader.FieldCount;
			if (hasGroupSubtotal)
			{
				numfields--;
			}
			ExcelExport export = new ExcelExport(zipstream, type, tobj, opt, user, language);
			for (int j = 0; j < numfields; j++)
			{
				export.Columns.Add(new ExportColumn(columns[j], language));
			}
			export.WriteCSVStart();
			if (type != ExportType.GDPdU)
			{
				export.WriteHeader();
				export.WriteColumns();
			}
			while (reader.Read())
			{
				token.ThrowIfCancellationRequested();
				List<object> data = new List<object>();
				for (int i = 0; i < numfields; i++)
				{
					if (reader.IsDBNull(i))
					{
						data.Add(string.Empty);
					}
					else
					{
						data.Add(reader.GetValue(i));
					}
				}
				export.Rows.Add(data);
				if (export.Rows.Count > 5000)
				{
					export.DoExport();
					export.Rows.Clear();
				}
			}
			export.DoExport();
		}

		public static PdfPTable CreateTableWithHeader(List<string> headers, bool isOverviewPdf = false, float[] cellWidths = null, int lastIndex = 0, int start = 0, float widthPercentage = 100f)
		{
			PdfPTable table = ((cellWidths == null) ? new PdfPTable(headers.Count) : new PdfPTable(cellWidths));
			table.DefaultCell.Border = 0;
			table.SpacingBefore = 5f;
			table.SpacingAfter = 5f;
			table.WidthPercentage = widthPercentage;
			table.HeaderRows = 1;
			table.HorizontalAlignment = 0;
			string fontPath = HttpRuntime.AppDomainAppPath + "Content\\fonts\\ARIALUNI.TTF";
			BaseFont bf = BaseFont.CreateFont(fontPath, "Identity-H", embedded: true);
			Font headLineNormalGrey = new Font(bf, 10f, 0, BaseColor.GRAY);
			int count = 0;
			for (int i = start; i < lastIndex; i++)
			{
				string name = ((headers.Count > i) ? headers[i] : "");
				PdfPCell cell = new PdfPCell(new Phrase(name, headLineNormalGrey));
				if (!isOverviewPdf)
				{
					if (i == lastIndex - 1)
					{
						cell.Border = 2;
					}
					else
					{
						cell.Border = 10;
					}
				}
				else
				{
					cell.Border = 2;
				}
				cell.VerticalAlignment = 5;
				cell.BorderColor = BaseColor.LIGHT_GRAY;
				cell.PaddingBottom = 5f;
				cell.PaddingTop = 5f;
				count++;
				if (isOverviewPdf && count == 3)
				{
					cell.HorizontalAlignment = 2;
				}
				table.AddCell(cell);
			}
			return table;
		}

		private void ExportBelegPdf(IDataReader dataReader, Stream targetStream, List<IColumn> columns, ITableObject tableObject, CancellationToken cancellationToken, ILanguage language, IOptimization optimization, IUser user, ViewboxDb.TableObjectCollection tempTableObjects)
		{
			IPdfExporter pdfExporter = PdfExporterFactory.GetBelegPdfExporter(dataReader, targetStream, columns, tableObject, cancellationToken, language, optimization, user, tempTableObjects);
			pdfExporter.Closing = false;
			pdfExporter.Export();
		}

		private void ExportPdf(IDataReader dataReader, Stream targetStream, List<IColumn> columns, ITableObject tableObject, CancellationToken cancellationToken, ILanguage language, IOptimization optimization, IUser user, ViewboxDb.TableObjectCollection tempTableObjects, bool hasGroupSubtotal = false)
		{
			IPdfExporter pdfExporter = PdfExporterFactory.GetPdfExporter(dataReader, targetStream, columns, tableObject, cancellationToken, language, optimization, user, tempTableObjects, hasGroupSubtotal);
			pdfExporter.Closing = false;
			pdfExporter.Export();
		}

		private void Zip(DatabaseBase connection, ExportType type, ITableObjectCollection objects, List<string> sqlList, List<List<IColumn>> columnsList, CancellationToken token, ILanguage language, IOptimization opt, IUser user, string exportDescription, string fileName = "", ViewboxDb.TableObjectCollection tempTableObjects = null, IEnumerable<IOptimization> optimization = null)
		{
			_connection = connection;
			int i = 0;
			if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
			{
				Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
			}
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			using (ZipOutputStream zipstream = new ZipOutputStream(filename + ".zip"))
			{
				zipstream.EnableZip64 = Zip64Option.AsNecessary;
				using StreamWriter ostream = new StreamWriter(zipstream, Encoding.GetEncoding("Windows-1252"));
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Indent = true;
				settings.IndentChars = "    ";
				settings.Encoding = Encoding.Default;
				using (XmlWriter xstream = XmlWriter.Create(ostream, settings))
				{
					if (type == ExportType.GDPdU)
					{
						zipstream.PutNextEntry("index.xml");
						GDPdUDescriptor(xstream, objects, language);
						xstream.Flush();
						zipstream.PutNextEntry("gdpdu-01-08-2002.dtd");
						ostream.Write(Resources.GDPdU);
						ostream.Flush();
					}
				}
				foreach (string sql in sqlList)
				{
					token.ThrowIfCancellationRequested();
					ITableObject tobj = objects.ElementAt(i);
					string regex = "[" + string.Join(",", Path.GetInvalidFileNameChars()) + "]";
					string fname = ReplaceSpecialChars(Regex.Replace(tobj.GetDescription(language), (regex ?? "") ?? "", "_"));
					switch (type)
					{
					case ExportType.PDFBeleg:
						using (_connection)
						{
							using IDataReader reader2 = _connection.ExecuteReader(sql);
							token.ThrowIfCancellationRequested();
							int ind3 = 0;
							DataTable table = new DataTable();
							table.Load(reader2);
							while (table.Rows != null && table.Rows.Count != 0)
							{
								token.ThrowIfCancellationRequested();
								if (zipstream.ContainsEntry(fname + ".pdf"))
								{
									while (zipstream.ContainsEntry(fname + "_" + ind3 + ".pdf"))
									{
										ind3++;
									}
									fname = fname + "_" + ind3;
								}
								zipstream.PutNextEntry(fname + ".pdf");
								ExportBelegPdf(table.CreateDataReader(), zipstream, columnsList[i], tobj, token, language, opt, user, tempTableObjects);
								table.Rows.RemoveAt(0);
							}
						}
						break;
					case ExportType.PDF:
					{
						if (zipstream.ContainsEntry(fname + ".pdf"))
						{
							int ind2 = 0;
							while (zipstream.ContainsEntry(fname + "_" + ind2 + ".pdf"))
							{
								ind2++;
							}
							fname = fname + "_" + ind2;
						}
						zipstream.PutNextEntry(fname + ".pdf");
						bool hasGroupSubtotal2 = false;
						try
						{
							hasGroupSubtotal2 = tempTableObjects.ToList().ElementAt(i).GroupSubTotal != null;
						}
						catch
						{
						}
						using (_connection)
						{
							using IDataReader dataReader = _connection.ExecuteReader(sql);
							token.ThrowIfCancellationRequested();
							ExportPdf(dataReader, zipstream, columnsList[i], tobj, token, language, opt, user, tempTableObjects, hasGroupSubtotal2);
						}
						break;
					}
					default:
					{
						if (zipstream.ContainsEntry(fname + ".csv"))
						{
							int ind = 0;
							while (zipstream.ContainsEntry(fname + "_" + ind + ".csv") && ind < 100000)
							{
								ind++;
							}
							fname = fname + "_" + ind;
						}
						zipstream.PutNextEntry(fname + ".csv");
						bool hasGroupSubtotal = false;
						try
						{
							hasGroupSubtotal = tempTableObjects.ToList().ElementAt(i).GroupSubTotal != null;
						}
						catch
						{
						}
						using (IDataReader reader = _connection.ExecuteReader(sql))
						{
							token.ThrowIfCancellationRequested();
							ViewboxDb.TableObject tempObject = null;
							SimpleTableModel model = null;
							if (tempTableObjects != null && tempTableObjects.Count > 0 && tempTableObjects.First().Additional != null)
							{
								tempObject = tempTableObjects.First();
								model = tempObject.Additional as SimpleTableModel;
							}
							DoZip(tobj, reader, zipstream, type, columnsList[i], language, exportDescription, token, user, hasGroupSubtotal, opt, model, tempObject);
						}
						break;
					}
					}
					i++;
					ostream.Flush();
				}
			}
			_connection = null;
		}

		private void SingleExport(DatabaseBase connection, ExportType type, ITableObjectCollection objects, List<string> sqlList, List<List<IColumn>> columnsList, CancellationToken token, ILanguage language, IOptimization opt, IUser user, string exportDescription, string fileName = "", ViewboxDb.TableObjectCollection tempTableObjects = null, IEnumerable<IOptimization> optimization = null)
		{
			_connection = connection;
			int i = 0;
			if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
			{
				Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
			}
			foreach (string sql in sqlList)
			{
				using (_connection)
				{
					using IDataReader reader = _connection.ExecuteReader(sql);
					ITableObject tobj = objects.ElementAt(i);
					IPdfExporter pdfExporter = PdfExporterFactory.GetPdfExporter(reader, null, columnsList[i], tobj, token, language, opt, user, null);
					using FileStream filestream = new FileStream(ViewboxApplication.TemporaryDirectory + base.Key + ".pdf", FileMode.Create);
					pdfExporter.Closing = true;
					pdfExporter.TargetStream = filestream;
					pdfExporter.Export();
				}
			}
			_connection = null;
		}

		private void DoExport(ExportType type, ITableObjectCollection objects, IOptimization opt, ILanguage language, IUser user, string exportDescription, bool getAllFields, CancellationToken token, string filter = "")
		{
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			if (type == ExportType.HTML)
			{
				return;
			}
			try
			{
				ViewboxSession.LoadExportData(Zip, type, objects, token, opt, language, user, exportDescription, 0, 0L, "", getAllFields, filter);
			}
			catch (Exception ex)
			{
				_log.Error("Error while exporting", ex);
				if (File.Exists(filename + ".zip"))
				{
					File.Delete(filename + ".zip");
				}
				throw;
			}
		}

		private void ExportTableOverview(ITableObjectCollection objects, IOptimization opt, ILanguage language, IUser user, CancellationToken token, List<Tuple<string, string, string>> tableOptions = null)
		{
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			try
			{
				if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
				{
					Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
				}
				using ZipOutputStream zipstream = new ZipOutputStream(filename + ".zip");
				zipstream.EnableZip64 = Zip64Option.AsNecessary;
				TableType tableType = objects.FirstOrDefault()?.Type ?? TableType.Issue;
				CultureInfo culture = new CultureInfo(language.CountryCode);
				string ttype = "";
				ttype += ((tableType == TableType.Table) ? Resources.ResourceManager.GetString("Tables", culture) : "");
				ttype += ((tableType == TableType.View) ? Resources.ResourceManager.GetString("Views", culture) : "");
				ttype += ((tableType == TableType.Issue) ? Resources.ResourceManager.GetString("Issues", culture) : "");
				string regex = "[" + string.Join(",", Path.GetInvalidFileNameChars()) + "]";
				string fname = ReplaceSpecialChars(Regex.Replace(ttype, (regex ?? "") ?? "", "_"));
				zipstream.PutNextEntry(fname + ".pdf");
				List<string> stringList = new List<string>
				{
					Resources.TransactionId,
					Resources.Description
				};
				if (tableType != TableType.Issue)
				{
					stringList.Add(Resources.RowCount);
				}
				string fontPath = HttpRuntime.AppDomainAppPath + "Content\\fonts\\ARIALUNI.TTF";
				BaseFont bf = BaseFont.CreateFont(fontPath, "Identity-H", embedded: true);
				Font normalGrey = new Font(bf, 12f, 0, BaseColor.GRAY);
				Font normalBlack = new Font(bf, 12f, 0, BaseColor.BLACK);
				PdfPTable table = ((tableType != TableType.Issue) ? CreateTableWithHeader(stringList, isOverviewPdf: true, new float[3] { 0.2f, 0.6f, 0.2f }, 3) : CreateTableWithHeader(stringList, isOverviewPdf: true, new float[2] { 0.3f, 0.7f }, 2));
				PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
				traits.CreateTableOfContents = false;
				traits.PageSizeAndOrientation = PageSize.A4;
				traits.MarginLeft = 60f;
				traits.MarginRight = 60f;
				traits.MarginTop = 100 + ((tableOptions != null) ? (tableOptions.Count * 8) : 0);
				traits.MarginBottom = 60f;
				global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
				int row = 0;
				foreach (ITableObject o in objects)
				{
					token.ThrowIfCancellationRequested();
					Image image = Image.GetInstance(Thread.GetDomain().BaseDirectory + "/Content/img/pdf/arrow.png");
					Chunk key = new Chunk(image, 0f, 1f);
					Chunk value = new Chunk("  " + o.TransactionNumber.ToString(), normalGrey);
					Phrase content = new Phrase();
					content.Add(key);
					content.Add(value);
					PdfPCell cell3 = new PdfPCell(content);
					cell3.VerticalAlignment = 4;
					cell3.Border = 2;
					cell3.BorderColor = BaseColor.LIGHT_GRAY;
					cell3.PaddingBottom = 5f;
					cell3.PaddingTop = 5f;
					cell3.NoWrap = true;
					table.AddCell(cell3);
					string description = o.GetDescription(language);
					PdfPCell cell4 = new PdfPCell(new Phrase((o.GetName() == description) ? description : (o.GetName() + "\n" + description), normalBlack));
					cell4.ExtraParagraphSpace = 5f;
					cell4.VerticalAlignment = 4;
					cell4.Border = 2;
					cell4.BorderColor = BaseColor.LIGHT_GRAY;
					cell4.PaddingBottom = 5f;
					cell4.PaddingTop = 5f;
					table.AddCell(cell4);
					if (tableType != TableType.Issue)
					{
						PdfPCell cell2 = new PdfPCell(new Phrase($"{o.GetRowCount(opt):#,0}", normalGrey));
						cell2.HorizontalAlignment = 2;
						cell2.VerticalAlignment = 4;
						cell2.Border = 2;
						cell2.BorderColor = BaseColor.LIGHT_GRAY;
						cell2.PaddingBottom = 5f;
						cell2.PaddingTop = 5f;
						table.AddCell(cell2);
					}
					row++;
				}
				if (row == 0)
				{
					for (int i = 0; i < 3; i++)
					{
						PdfPCell cell = new PdfPCell();
						cell.Border = 0;
						table.AddCell(cell);
					}
				}
				generator.Document.Add(table);
				List<Tuple<string, string>> optimizationTexts = opt.GetDescriptions(language);
				optimizationTexts.Reverse();
				generator.WriteFile(zipstream, new Phrase(Resources.ResourceManager.GetString("Overview", culture) + " " + ttype), addPageHeader: false, addViewboxStyle: true, optimizationTexts, new Tuple<string, string>(Resources.Processor, user.GetName()), Resources.PdfPageFrom, 1, "page", "part", closing: true, 0, bukrs: true, gjahr: true, tableOptions);
			}
			catch (Exception)
			{
				if (File.Exists(filename + ".zip"))
				{
					File.Delete(filename + ".zip");
				}
				throw;
			}
		}

		private void DoExport(ExportType type, ViewboxDb.TableObjectCollection tempTableObjects, IOptimization opt, ILanguage language, IUser user, string exportDescription, bool getAllFields, CancellationToken token, IEnumerable<IOptimization> optimization)
		{
			string filename = ViewboxApplication.TemporaryDirectory + base.Key;
			long size = 0L;
			ViewboxDb.TableObject firstTableObject = tempTableObjects.FirstOrDefault();
			if (firstTableObject != null && tempTableObjects.Count == 1 && firstTableObject.Table != null)
			{
				PageSystem ps = firstTableObject.Table.PageSystem;
				if (ps != null && ps.CountStep != 0)
				{
					size = ps.CountStep;
				}
			}
			switch (type)
			{
			case ExportType.HTML:
				return;
			case ExportType.PDF:
			{
				if (tempTableObjects.Any((ViewboxDb.TableObject t) => t.Additional != null))
				{
					if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
					{
						Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
					}
					using ZipOutputStream zipstream = new ZipOutputStream(filename + ".zip");
					zipstream.EnableZip64 = Zip64Option.AsNecessary;
					ViewboxDb.TableObject tempObject = tempTableObjects.First();
					IIssue issue2 = tempObject.Table as IIssue;
					UniversalTableModel uni = tempObject.Additional as UniversalTableModel;
					if (uni != null)
					{
						for (int dataColIdx = 0; dataColIdx < uni.Columns.Count; dataColIdx++)
						{
							string name4 = $"{tempObject.Table.Descriptions[language]} - {dataColIdx + 1} - {uni.Columns[dataColIdx].GetDescription(language)}";
							zipstream.PutNextEntry(string.Format("{0}.pdf", specialChars.Replace(name4, "")));
							WritePdfFromUniversalStructure(zipstream, tempObject.Table, uni, opt, language, user, dataColIdx, token);
						}
						return;
					}
					SapBalanceModel balance2 = tempObject.Additional as SapBalanceModel;
					if (balance2 != null)
					{
						zipstream.PutNextEntry("Bilanz.pdf");
						WritePdfFromBalance(zipstream, tempObject.Table, balance2, opt, language, user, token);
						return;
					}
					DcwBalanceModel dcwbalance2 = tempObject.Additional as DcwBalanceModel;
					if (dcwbalance2 != null)
					{
						string name5 = ((issue2 != null && issue2.Flag == 3) ? "GUV" : "Bilanz");
						zipstream.PutNextEntry(name5 + ".pdf");
						WritePdfFromDcwBalance(zipstream, dcwbalance2, opt, language, tempObject.Table, user, token);
						return;
					}
					StructuedTableModel guvBalance = tempObject.Additional as StructuedTableModel;
					if (guvBalance != null)
					{
						string name6 = "";
						if (issue2 != null)
						{
							switch (issue2.Flag)
							{
							case 4:
								name6 = "Guv";
								break;
							case 5:
								name6 = "Bilanz";
								break;
							case 6:
								name6 = "Bilanzstruktur anzeigen";
								break;
							case 7:
								name6 = "Kostenartenhierarhie";
								break;
							case 8:
								name6 = "Kostenstellengruppe anzeigen";
								break;
							}
						}
						zipstream.PutNextEntry(name6 + ".pdf");
						WritePdfFromStructuredBalance(zipstream, guvBalance, opt, language, tempObject.Table, user, token);
						return;
					}
				}
				if (tempTableObjects.Count != 1)
				{
					break;
				}
				ViewboxDb.TableObject obj = tempTableObjects.FirstOrDefault();
				if (obj == null || obj.Table == null || obj.Filter == null)
				{
					break;
				}
				PdfExporterConfigElement exporter = PdfExporterConfig.GetExporter(obj.Table.Database, obj.Table.TableName);
				if (exporter == null || string.IsNullOrEmpty(exporter.ExtInfo5))
				{
					break;
				}
				object val = obj.Filter.GetColumnValue(exporter.ExtInfo5);
				if (val != null)
				{
					FileName = string.Concat(val, ".pdf");
					if (FileName.StartsWith("0x"))
					{
						FileName = FileName.Substring(2);
					}
					ViewboxSession.LoadExportData(SingleExport, type, tempTableObjects, token, opt, language, user, exportDescription, 0, size, FileName, getAllFields);
					return;
				}
				break;
			}
			}
			if (tempTableObjects.Any((ViewboxDb.TableObject t) => t.Additional != null))
			{
				if (!Directory.Exists(ViewboxApplication.TemporaryDirectory))
				{
					Directory.CreateDirectory(ViewboxApplication.TemporaryDirectory);
				}
				using ZipOutputStream zipOutputStream = new ZipOutputStream(filename + ".zip");
				zipOutputStream.EnableZip64 = Zip64Option.AsNecessary;
				using StreamWriter ostream = new StreamWriter(zipOutputStream, Encoding.Default);
				ViewboxDb.TableObject tempObject2 = tempTableObjects.First();
				UniversalTableModel uni2 = tempObject2.Additional as UniversalTableModel;
				if (uni2 != null)
				{
					int deepestLevel4 = GetDeepestUniversalLevel(uni2.TreeRoot, 0);
					string name3 = string.Format("{0}.csv", specialChars.Replace(tempObject2.Table.Descriptions[language], ""));
					if (type == ExportType.GDPdU)
					{
						XmlWriterSettings settings4 = new XmlWriterSettings();
						settings4.Indent = true;
						settings4.IndentChars = "    ";
						settings4.Encoding = Encoding.Default;
						using XmlWriter xmlWriter = XmlWriter.Create(ostream, settings4);
						zipOutputStream.PutNextEntry("index.xml");
						GDPdUDescriptorForUniversalTable(xmlWriter, name3, uni2.KontoColumnName, uni2.Columns);
						xmlWriter.Flush();
						zipOutputStream.PutNextEntry("gdpdu-01-08-2002.dtd");
						ostream.Write(Resources.GDPdU);
						ostream.Flush();
					}
					zipOutputStream.PutNextEntry(name3);
					GenerateCSVFromUniversalStructure(type, zipOutputStream, tempObject2.Table, uni2, opt, language, user, tempObject2, token);
					return;
				}
				SapBalanceModel balance = tempObject2.Additional as SapBalanceModel;
				if (balance != null)
				{
					int deepestLevel3 = GetDeepestLevel(balance.TreeRoot, 0);
					if (type == ExportType.GDPdU)
					{
						XmlWriterSettings settings3 = new XmlWriterSettings();
						settings3.Indent = true;
						settings3.IndentChars = "    ";
						settings3.Encoding = Encoding.Default;
						using XmlWriter xmlWriter2 = XmlWriter.Create(ostream, settings3);
						zipOutputStream.PutNextEntry("index.xml");
						GDPdUDescriptorForBalance(xmlWriter2, deepestLevel3);
						xmlWriter2.Flush();
						zipOutputStream.PutNextEntry("gdpdu-01-08-2002.dtd");
						ostream.Write(Resources.GDPdU);
						ostream.Flush();
					}
					string name2 = string.Format("{0}.csv", specialChars.Replace(tempObject2.Table.Descriptions[language], ""));
					zipOutputStream.PutNextEntry(name2);
					GenerateCsvFromBalance(zipOutputStream, balance, type, deepestLevel3, tempObject2.Table, opt, language, user, tempObject2, token);
					ostream.Flush();
					return;
				}
				DcwBalanceModel dcwbalance = tempObject2.Additional as DcwBalanceModel;
				if (dcwbalance != null)
				{
					int deepestLevel2 = GetDeepestDcwLevel(dcwbalance.TreeRoot, 0);
					if (type == ExportType.GDPdU)
					{
						XmlWriterSettings settings2 = new XmlWriterSettings();
						settings2.Indent = true;
						settings2.IndentChars = "    ";
						settings2.Encoding = Encoding.Default;
						using XmlWriter xmlWriter3 = XmlWriter.Create(ostream, settings2);
						zipOutputStream.PutNextEntry("index.xml");
						GDPdUDescriptorForDcwBalance(xmlWriter3, deepestLevel2);
						xmlWriter3.Flush();
						zipOutputStream.PutNextEntry("gdpdu-01-08-2002.dtd");
						ostream.Write(Resources.GDPdU);
						ostream.Flush();
					}
					zipOutputStream.PutNextEntry("Bilanz.csv");
					WriteCsvFromDcwBalance(ostream, dcwbalance, type, deepestLevel2, token);
					ostream.Flush();
					return;
				}
				StructuedTableModel guvModel = tempObject2.Additional as StructuedTableModel;
				if (guvModel != null)
				{
					int deepestLevel = GetDeepestStructureLevel(guvModel.TreeRoot, 0);
					if (type == ExportType.GDPdU)
					{
						XmlWriterSettings settings = new XmlWriterSettings();
						settings.Indent = true;
						settings.IndentChars = "    ";
						settings.Encoding = Encoding.Default;
						using XmlWriter xstream = XmlWriter.Create(ostream, settings);
						zipOutputStream.PutNextEntry("index.xml");
						GDPdUDescriptorForDcwBalance(xstream, deepestLevel);
						xstream.Flush();
						zipOutputStream.PutNextEntry("gdpdu-01-08-2002.dtd");
						ostream.Write(Resources.GDPdU);
						ostream.Flush();
					}
					IIssue issue = tempObject2.Table as IIssue;
					string name = "";
					if (issue != null)
					{
						switch (issue.Flag)
						{
						case 4:
							name = "Guv";
							break;
						case 5:
							name = "Bilanz";
							break;
						case 6:
							name = "Bilanzstruktur anzeigen";
							break;
						case 7:
							name = "Kostenartenhierarhie";
							break;
						case 8:
							name = "Kostenstellengruppe anzeigen";
							break;
						}
					}
					zipOutputStream.PutNextEntry(((name == "") ? tempObject2.Table.TableName : name) + ".csv");
					WriteCsvFromGuvBalance(ostream, guvModel, type, deepestLevel, token);
					ostream.Flush();
					return;
				}
			}
			try
			{
				ViewboxSession.LoadExportData(Zip, type, tempTableObjects, token, opt, language, user, exportDescription, 0, 0L, "", getAllFields, optimization);
			}
			catch (Exception)
			{
				if (File.Exists(filename + ".zip"))
				{
					File.Delete(filename + ".zip");
				}
				throw;
			}
		}

		private int GetDeepestUniversalLevel(UniversalTableModel.Node node, int deepestLevel)
		{
			List<int> deepestLevels = new List<int>();
			foreach (UniversalTableModel.Node c in node.Children)
			{
				deepestLevels.Add(GetDeepestUniversalLevel(c, deepestLevel + 1));
			}
			if (deepestLevels.Count > 0)
			{
				return deepestLevels.Max();
			}
			return deepestLevel;
		}

		private int GetDeepestLevel(SapBalanceModel.Node node, int deepestLevel)
		{
			List<int> deepestLevels = new List<int>();
			foreach (SapBalanceModel.Node c in node.Children)
			{
				if (c.HasValueChildren)
				{
					deepestLevels.Add(GetDeepestLevel(c, deepestLevel + 1));
				}
			}
			if (deepestLevels.Count > 0)
			{
				return deepestLevels.Max();
			}
			return deepestLevel;
		}

		private int GetDeepestDcwLevel(DcwBalanceModel.DcwNode node, int deepestLevel)
		{
			List<int> deepestLevels = new List<int>();
			foreach (DcwBalanceModel.DcwNode c in node.Children)
			{
				deepestLevels.Add(GetDeepestDcwLevel(c, deepestLevel + 1));
			}
			if (deepestLevels.Count > 0)
			{
				return deepestLevels.Max();
			}
			return deepestLevel;
		}

		private int GetDeepestStructureLevel(StructuedTableModel.Node node, int deepestLevel)
		{
			List<int> deepestLevels = new List<int>();
			foreach (StructuedTableModel.Node c in node.Children)
			{
				deepestLevels.Add(GetDeepestStructureLevel(c, deepestLevel + 1));
			}
			if (deepestLevels.Count > 0)
			{
				return deepestLevels.Max();
			}
			return deepestLevel;
		}

		private void GenerateCsvFromBalance(Stream outStream, SapBalanceModel balance, ExportType type, int deepestLevel, ITableObject tobj, IOptimization opt, ILanguage language, IUser user, ViewboxDb.TableObject tempTableObject, CancellationToken token)
		{
			ExcelExport export = new ExcelExport(outStream, type, tobj, opt, user, language);
			Stack<LevelNode> nodes = new Stack<LevelNode>();
			nodes.Push(new LevelNode(balance.TreeRoot, 0, ""));
			for (int i = 0; i < deepestLevel - 1; i++)
			{
				export.Columns.Add(new ExportColumn(Resources.ExportWriteCsvFromBalanceLabel + " " + i));
			}
			export.Columns.Add(new ExportColumn(Resources.ExportWriteCsvFromBalanceAccountDescription));
			export.Columns.Add(new ExportColumn(Resources.ExportWriteCsvFromBalanceAccountValue));
			foreach (Tuple<string, string> param in balance.GetParameters())
			{
				export.Parameters.Add(param.Item1, param.Item2);
			}
			while (nodes.Count != 0)
			{
				token.ThrowIfCancellationRequested();
				LevelNode levelNode = nodes.Pop();
				if (levelNode.level != 0 && levelNode.node.Type != SapBalanceModel.StructureType.AccountNode && levelNode.node.Type != SapBalanceModel.StructureType.RangeNode)
				{
					List<object> data = new List<object>();
					List<SapBalanceModel.Node> parentNodes = new List<SapBalanceModel.Node>();
					SapBalanceModel.Node n = levelNode.node;
					while (n.Parent != null)
					{
						n = n.Parent;
						if (n.HasValueChildren)
						{
							parentNodes.Add(n);
						}
					}
					parentNodes.Reverse();
					foreach (SapBalanceModel.Node p in parentNodes)
					{
						data.Add((p.Description != null) ? p.Description.Replace("\"", "\"\"") : "");
					}
					if (levelNode.level > 1)
					{
						for (int m = levelNode.level; m < deepestLevel; m++)
						{
							data.Add("");
						}
					}
					data.Add((levelNode.node.Description != null) ? levelNode.node.Description.Replace("\"", "\"\"") : "");
					if (levelNode.level == 1)
					{
						for (int l = levelNode.level; l < deepestLevel; l++)
						{
							data.Add("");
						}
					}
					data.Add(levelNode.node.Value);
					export.Rows.Add(data);
				}
				int valueChildCounter = 0;
				for (int k = levelNode.node.Children.Count() - 1; k >= 0; k--)
				{
					if (levelNode.node.Children.ElementAt(k).HasValueChildren)
					{
						valueChildCounter++;
					}
				}
				for (int j = levelNode.node.Children.Count() - 1; j >= 0; j--)
				{
					if (levelNode.node.Children.ElementAt(j).HasValueChildren)
					{
						string levelString = levelNode.levelString + "." + valueChildCounter;
						if (levelNode.level == 0)
						{
							levelString = Convert.ToString(valueChildCounter);
						}
						valueChildCounter--;
						nodes.Push(new LevelNode(levelNode.node.Children.ElementAt(j), levelNode.level + 1, levelString));
					}
				}
			}
			export.DoExport();
		}

		private void WriteCsvFromDcwBalance(StreamWriter ostream, DcwBalanceModel balance, ExportType type, int deepestLevel, CancellationToken token)
		{
			string dataSeperator = ";";
			Stack<DcwLevelNode> nodes = new Stack<DcwLevelNode>();
			nodes.Push(new DcwLevelNode(balance.TreeRoot, 0, ""));
			if (type == ExportType.Excel)
			{
				for (int j = 1; j <= deepestLevel; j++)
				{
					ostream.Write($"\"{Resources.DCWDescription} {j}\"");
					ostream.Write(dataSeperator);
				}
				ostream.Write(string.Format("\"{0}\"", "Konto - Nr"));
				ostream.Write(dataSeperator);
				ostream.Write($"\"{Resources.DCWValue}\"");
				ostream.Write(dataSeperator);
				if (balance.HasVJahr)
				{
					ostream.Write($"\"{Resources.DCWOldValue}\"");
					ostream.Write(dataSeperator);
				}
				ostream.WriteLine();
			}
			while (nodes.Count != 0)
			{
				token.ThrowIfCancellationRequested();
				DcwLevelNode levelNode = nodes.Pop();
				if (levelNode.level != 0)
				{
					DcwBalanceModel.DcwNode node = levelNode.node;
					List<string> levels = new List<string>();
					for (int k = 1; k <= deepestLevel; k++)
					{
						if (levelNode.level != k)
						{
							levels.Add("\"\"");
						}
						else
						{
							levels.Add($"\"{node.OrderingText} {node.Description}\"");
						}
					}
					ostream.Write(string.Join(dataSeperator, levels));
					ostream.Write(dataSeperator);
					ostream.Write($"\"{node.Key}\"");
					ostream.Write(dataSeperator);
					ostream.Write(string.Format("\"{0}\"", ((double)node.Sign * node.Value).ToString("#,0.00")));
					ostream.Write(dataSeperator);
					if (balance.HasVJahr)
					{
						ostream.Write(string.Format("\"{0}\"", ((double)node.Sign * node.Value2).ToString("#,0.00")));
						ostream.Write(dataSeperator);
					}
					ostream.WriteLine();
				}
				int valueChildCounter = levelNode.node.Children.Count();
				for (int i = levelNode.node.Children.Count() - 1; i >= 0; i--)
				{
					string levelString = levelNode.levelString + "." + valueChildCounter;
					if (levelNode.level == 0)
					{
						levelString = Convert.ToString(valueChildCounter);
					}
					valueChildCounter--;
					nodes.Push(new DcwLevelNode(levelNode.node.Children.ElementAt(i), levelNode.level + 1, levelString));
				}
			}
		}

		private void WriteCsvFromGuvBalance(StreamWriter ostream, StructuedTableModel balance, ExportType type, int deepestLevel, CancellationToken token)
		{
			string dataSeperator = ";";
			Stack<StructuredLevelNode> nodes = new Stack<StructuredLevelNode>();
			nodes.Push(new StructuredLevelNode(balance.TreeRoot, 0, ""));
			if (type == ExportType.Excel)
			{
				for (int j = 1; j <= deepestLevel; j++)
				{
					ostream.Write($"\"{Resources.DCWDescription} {j}\"");
					ostream.Write(dataSeperator);
				}
				ostream.Write(string.Format("\"{0}\"", "Konto - Nr"));
				ostream.Write(dataSeperator);
				ostream.Write(string.Format("\"{0}\"", "Saldo"));
				ostream.Write(dataSeperator);
				ostream.WriteLine();
			}
			while (nodes.Count != 0)
			{
				token.ThrowIfCancellationRequested();
				StructuredLevelNode levelNode = nodes.Pop();
				if (levelNode.level != 0)
				{
					StructuedTableModel.Node node = levelNode.node;
					List<string> levels = new List<string>();
					for (int k = 1; k <= deepestLevel; k++)
					{
						if (levelNode.level != k)
						{
							levels.Add("\"\"");
						}
						else
						{
							levels.Add($"\"{node.Description}\"");
						}
					}
					ostream.Write(string.Join(dataSeperator, levels));
					ostream.Write(dataSeperator);
					ostream.Write($"\"{node.Key}\"");
					ostream.Write(dataSeperator);
					if (node.Values == null || node.Values.Count == 0)
					{
						ostream.Write(string.Join(" ; ", ""));
					}
					else
					{
						ostream.Write(string.Join(" ; ", node.Values.ToArray()));
					}
					ostream.Write(dataSeperator);
					ostream.WriteLine();
				}
				int valueChildCounter = levelNode.node.Children.Count();
				for (int i = levelNode.node.Children.Count() - 1; i >= 0; i--)
				{
					string levelString = levelNode.levelString + "." + valueChildCounter;
					if (levelNode.level == 0)
					{
						levelString = Convert.ToString(valueChildCounter);
					}
					valueChildCounter--;
					nodes.Push(new StructuredLevelNode(levelNode.node.Children.ElementAt(i), levelNode.level + 1, levelString));
				}
			}
		}

		private void WritePdfFromUniversalStructure(Stream outStream, ITableObject tobj, UniversalTableModel uni, IOptimization opt, ILanguage language, IUser user, int dataColIdx, CancellationToken token)
		{
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.MarginTop = 100f;
			traits.MarginBottom = 60f;
			global::PdfGenerator.PdfGenerator pdf = new global::PdfGenerator.PdfGenerator(traits, token);
			PdfPTable table = new PdfPTable(new float[2] { 5f, 1f });
			table.DefaultCell.Border = 0;
			table.SpacingBefore = 5f;
			table.SpacingAfter = 5f;
			table.WidthPercentage = 100f;
			Stack<UniversalLevelNode> nodes = new Stack<UniversalLevelNode>();
			nodes.Push(new UniversalLevelNode(uni.TreeRoot, 0, ""));
			BaseFont bfTimes = BaseFont.CreateFont("Times-Roman", "Cp1252", embedded: false);
			Font redFont = new Font(bfTimes, 12f, 0, BaseColor.RED);
			Font normalFont = new Font(bfTimes, 12f, 0);
			Font font = new Font(Font.FontFamily.HELVETICA, 10f);
			table.AddCell(new Phrase(tobj.GetDescription(language), font));
			table.AddCell(new PdfPCell
			{
				HorizontalAlignment = 2,
				Border = 0
			});
			table.AddCell(new PdfPCell
			{
				FixedHeight = 12f,
				Border = 0
			});
			table.AddCell(new PdfPCell
			{
				FixedHeight = 12f,
				HorizontalAlignment = 2,
				Border = 0
			});
			List<Tuple<string, string>> parameters = uni.GetParameters(language);
			foreach (Tuple<string, string> parameter in parameters)
			{
				PdfPCell phrase = new PdfPCell(new Phrase(parameter.Item1 + ": " + parameter.Item2, font));
				phrase.Border = 0;
				table.AddCell(phrase);
				table.AddCell(new PdfPCell
				{
					HorizontalAlignment = 2,
					Border = 0
				});
			}
			table.AddCell(new PdfPCell
			{
				FixedHeight = 12f,
				Border = 0
			});
			table.AddCell(new PdfPCell
			{
				FixedHeight = 12f,
				HorizontalAlignment = 2,
				Border = 0
			});
			while (nodes.Count != 0)
			{
				UniversalLevelNode levelNode = nodes.Pop();
				if (levelNode.level != 0)
				{
					string tabs = "";
					for (int j = 0; j < levelNode.level; j++)
					{
						tabs += "\t\t\t\t\t";
					}
					table.AddCell(new Phrase(tabs + levelNode.levelString + " " + levelNode.node.Description, font));
					Font correctFont = ((levelNode.node.Data[dataColIdx].dataType == UniversalTableModel.Node.DataStruct.DataType.Value && levelNode.node.Data[dataColIdx].value < 0.0) ? redFont : normalFont);
					PdfPCell rightAligned = ((levelNode.node.Data[dataColIdx].dataType == UniversalTableModel.Node.DataStruct.DataType.Field) ? new PdfPCell(new Phrase(levelNode.node.Data[dataColIdx].field, correctFont)) : new PdfPCell(new Phrase(levelNode.node.Data[dataColIdx].value.ToString("N", new CultureInfo("de-DE")), correctFont)));
					rightAligned.HorizontalAlignment = 2;
					rightAligned.Border = 0;
					table.AddCell(rightAligned);
				}
				int valueChildCounter = levelNode.node.Children.Count;
				for (int i = levelNode.node.Children.Count() - 1; i >= 0; i--)
				{
					string levelString = levelNode.levelString + "." + valueChildCounter;
					if (levelNode.level == 0)
					{
						levelString = Convert.ToString(valueChildCounter);
					}
					valueChildCounter--;
					nodes.Push(new UniversalLevelNode(levelNode.node.Children.ElementAt(i), levelNode.level + 1, levelString));
				}
			}
			pdf.Document.Add(table);
			List<Tuple<string, string>> optimizationTexts = opt.GetDescriptions(language);
			optimizationTexts.Reverse();
			bool bukrs = tobj.Type == TableType.Issue && ((IIssue)tobj).NeedBukrs;
			bool gjahr = tobj.Type == TableType.Issue && ((IIssue)tobj).NeedGJahr;
			string name = $"{uni.KontoColumnName} - {uni.Columns[dataColIdx].GetDescription(language)}";
			bool isClosing = dataColIdx == uni.Columns.Count - 1;
			pdf.WriteFile(outStream, new Phrase(name), addPageHeader: false, addViewboxStyle: true, optimizationTexts, new Tuple<string, string>(Resources.Processor, user.GetName()), Resources.PdfPageFrom, 1, Resources.Page, Resources.Part, isClosing, 0, bukrs, gjahr);
		}

		private void GenerateCSVFromUniversalStructure(ExportType type, Stream outStream, ITableObject tobj, UniversalTableModel uni, IOptimization opt, ILanguage language, IUser user, ViewboxDb.TableObject tempTableObject, CancellationToken token)
		{
			ExcelExport export = new ExcelExport(outStream, type, tobj, opt, user, language);
			export.Columns.Add(new ExportColumn(uni.KontoColumnName));
			for (int dataColIdx2 = 0; dataColIdx2 < uni.Columns.Count; dataColIdx2++)
			{
				export.Columns.Add(new ExportColumn(uni.Columns[dataColIdx2], language));
			}
			foreach (Tuple<string, string> param in uni.GetParameters(language))
			{
				export.Parameters.Add(param.Item1, param.Item2);
			}
			Stack<UniversalLevelNode> nodes = new Stack<UniversalLevelNode>();
			nodes.Push(new UniversalLevelNode(uni.TreeRoot, 0, ""));
			while (nodes.Count != 0)
			{
				token.ThrowIfCancellationRequested();
				UniversalLevelNode levelNode = nodes.Pop();
				if (levelNode.level != 0)
				{
					List<object> data = new List<object>();
					string indent = "";
					for (int j = 0; j < levelNode.level - 1; j++)
					{
						indent += "  ";
					}
					data.Add(indent + levelNode.levelString + " " + levelNode.node.Description);
					for (int dataColIdx = 0; dataColIdx < uni.Columns.Count; dataColIdx++)
					{
						if (levelNode.node.Data[dataColIdx].dataType == UniversalTableModel.Node.DataStruct.DataType.Field)
						{
							data.Add(levelNode.node.Data[dataColIdx].field);
						}
						else
						{
							data.Add(levelNode.node.Data[dataColIdx].value);
						}
					}
					export.Rows.Add(data);
				}
				int valueChildCounter = levelNode.node.Children.Count;
				for (int i = levelNode.node.Children.Count() - 1; i >= 0; i--)
				{
					string levelString = ((levelNode.level == 0) ? (levelString = Convert.ToString(valueChildCounter) + ".") : (levelNode.levelString + valueChildCounter + "."));
					valueChildCounter--;
					nodes.Push(new UniversalLevelNode(levelNode.node.Children.ElementAt(i), levelNode.level + 1, levelString));
				}
			}
			export.DoExport();
		}

		private void WritePdfFromBalance(Stream outStream, ITableObject tobj, SapBalanceModel balance, IOptimization opt, ILanguage language, IUser user, CancellationToken token)
		{
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.MarginTop = 100f;
			traits.MarginBottom = 60f;
			global::PdfGenerator.PdfGenerator pdf = new global::PdfGenerator.PdfGenerator(traits, token);
			PdfPTable table = new PdfPTable(new float[2] { 5f, 1f });
			table.DefaultCell.Border = 0;
			table.SpacingBefore = 5f;
			table.SpacingAfter = 5f;
			table.WidthPercentage = 100f;
			Stack<LevelNode> nodes = new Stack<LevelNode>();
			nodes.Push(new LevelNode(balance.TreeRoot, 0, ""));
			BaseFont bfTimes = BaseFont.CreateFont("Times-Roman", "Cp1252", embedded: false);
			IProperty fontProp = ViewboxApplication.FindProperty(user, "defaultPDFfontSize");
			int fontSize = ((fontProp != null) ? Convert.ToInt32(fontProp.Value) : 0);
			Font redFont = new Font(bfTimes, (fontSize != 0) ? fontSize : 12, 0, BaseColor.RED);
			Font normalFont = new Font(bfTimes, (fontSize != 0) ? fontSize : 12, 0);
			Font font = new Font(Font.FontFamily.HELVETICA, (fontSize != 0) ? fontSize : 10);
			table.AddCell(new Phrase(tobj.GetDescription(), font));
			table.AddCell(new PdfPCell
			{
				HorizontalAlignment = 2,
				Border = 0
			});
			table.AddCell(new PdfPCell
			{
				FixedHeight = 12f,
				Border = 0
			});
			table.AddCell(new PdfPCell
			{
				FixedHeight = 12f,
				HorizontalAlignment = 2,
				Border = 0
			});
			List<Tuple<string, string>> parameters = balance.GetParameters();
			foreach (Tuple<string, string> parameter in parameters)
			{
				PdfPCell phrase = new PdfPCell(new Phrase(parameter.Item1 + ": " + parameter.Item2, font));
				phrase.Border = 0;
				table.AddCell(phrase);
				table.AddCell(new PdfPCell
				{
					HorizontalAlignment = 2,
					Border = 0
				});
			}
			table.AddCell(new PdfPCell
			{
				FixedHeight = 12f,
				Border = 0
			});
			table.AddCell(new PdfPCell
			{
				FixedHeight = 12f,
				HorizontalAlignment = 2,
				Border = 0
			});
			while (nodes.Count != 0)
			{
				LevelNode levelNode = nodes.Pop();
				if (levelNode.level != 0 && levelNode.node.Type != SapBalanceModel.StructureType.AccountNode && levelNode.node.Type != SapBalanceModel.StructureType.RangeNode)
				{
					Paragraph leftAligned = new Paragraph(levelNode.levelString + " " + levelNode.node.Description, font);
					leftAligned.IndentationLeft = levelNode.level * 10;
					PdfPCell leftCell = new PdfPCell();
					leftCell.Border = 0;
					leftCell.AddElement(leftAligned);
					table.AddCell(leftCell);
					Font correctFont = ((levelNode.node.Value < 0m) ? redFont : normalFont);
					PdfPCell rightAligned = new PdfPCell(new Phrase(levelNode.node.Value.ToString("N", new CultureInfo("de-DE")) + " €", correctFont));
					rightAligned.HorizontalAlignment = 2;
					rightAligned.PaddingTop = 8f;
					rightAligned.Border = 0;
					table.AddCell(rightAligned);
				}
				int valueChildCounter = 0;
				for (int j = levelNode.node.Children.Count() - 1; j >= 0; j--)
				{
					if (levelNode.node.Children.ElementAt(j).HasValueChildren)
					{
						valueChildCounter++;
					}
				}
				for (int i = levelNode.node.Children.Count() - 1; i >= 0; i--)
				{
					if (levelNode.node.Children.ElementAt(i).HasValueChildren)
					{
						string levelString = levelNode.levelString + "." + valueChildCounter;
						if (levelNode.level == 0)
						{
							levelString = Convert.ToString(valueChildCounter);
						}
						valueChildCounter--;
						nodes.Push(new LevelNode(levelNode.node.Children.ElementAt(i), levelNode.level + 1, levelString));
					}
				}
			}
			pdf.Document.Add(table);
			List<Tuple<string, string>> optimizationTexts = opt.GetDescriptions(language);
			optimizationTexts.Reverse();
			pdf.WriteFile(outStream, new Phrase("Bilanz"), addPageHeader: false, addViewboxStyle: true, optimizationTexts, new Tuple<string, string>(Resources.Processor, user.GetName()), Resources.PdfPageFrom, 1, Resources.Page, Resources.Part);
		}

		private void WritePdfFromDcwBalance(Stream outStream, DcwBalanceModel balance, IOptimization opt, ILanguage language, ITableObject tab, IUser user, CancellationToken token)
		{
			List<Tuple<string, string>> optimizationTexts = opt.GetDescriptions(language);
			optimizationTexts.Reverse();
			optimizationTexts.AddRange(balance.GetParameters());
			int flag = (tab as IIssue)?.Flag ?? 0;
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.MarginTop = 100f;
			traits.MarginBottom = 60f;
			if (optimizationTexts.Count > 5)
			{
				traits.MarginTop += (optimizationTexts.Count - 6) * 14;
			}
			global::PdfGenerator.PdfGenerator pdf = new global::PdfGenerator.PdfGenerator(traits, token);
			PdfPTable tablebefore = new PdfPTable(new float[2] { 5f, 1f });
			tablebefore.DefaultCell.Border = 0;
			tablebefore.WidthPercentage = 100f;
			pdf.Document.Add(tablebefore);
			PdfPTable table = new PdfPTable((!balance.HasVJahr) ? new float[3] { 5f, 1.5f, 1f } : new float[4] { 4f, 1f, 1f, 1f });
			table.DefaultCell.Border = 0;
			table.SpacingBefore = 15f;
			table.SpacingAfter = 5f;
			table.WidthPercentage = 100f;
			Stack<DcwLevelNode> nodes = new Stack<DcwLevelNode>();
			nodes.Push(new DcwLevelNode(balance.TreeRoot, 0, ""));
			BaseFont bfTimes = BaseFont.CreateFont("Times-Roman", "Cp1252", embedded: false);
			Font redFont = new Font(bfTimes, 12f, 0, BaseColor.RED);
			Font normalFont = new Font(bfTimes, 12f, 0);
			Font boldFont = new Font(bfTimes, 12f, 1);
			Font font = new Font(Font.FontFamily.HELVETICA, 10f);
			table.AddCell(new Phrase("", normalFont));
			table.AddCell(new PdfPCell(new Phrase("", normalFont))
			{
				HorizontalAlignment = 2,
				Border = 0
			});
			table.AddCell(new PdfPCell(new Phrase("", normalFont))
			{
				HorizontalAlignment = 2,
				Border = 0
			});
			if (balance.HasVJahr)
			{
				table.AddCell(new PdfPCell(new Phrase("", normalFont))
				{
					HorizontalAlignment = 2,
					Border = 0
				});
			}
			table.AddCell(new Phrase("", normalFont));
			table.AddCell(new PdfPCell(new Phrase("", normalFont))
			{
				HorizontalAlignment = 2,
				Border = 0
			});
			table.AddCell(new PdfPCell(new Phrase("", normalFont))
			{
				HorizontalAlignment = 2,
				Border = 0
			});
			if (balance.HasVJahr)
			{
				table.AddCell(new PdfPCell(new Phrase("", normalFont))
				{
					HorizontalAlignment = 2,
					Border = 0
				});
			}
			table.AddCell(new Phrase("Struktur / Konto", boldFont));
			PdfPCell rightAlignedHeader3 = new PdfPCell(new Phrase("Konto-Nr", boldFont));
			rightAlignedHeader3.HorizontalAlignment = 2;
			rightAlignedHeader3.Border = 0;
			table.AddCell(rightAlignedHeader3);
			PdfPCell rightAlignedHeader = new PdfPCell(new Phrase("GJahr", boldFont));
			rightAlignedHeader.HorizontalAlignment = 2;
			rightAlignedHeader.Border = 0;
			table.AddCell(rightAlignedHeader);
			if (balance.HasVJahr)
			{
				PdfPCell rightAlignedHeader2 = new PdfPCell(new Phrase("VJahr", boldFont));
				rightAlignedHeader2.HorizontalAlignment = 2;
				rightAlignedHeader2.Border = 0;
				table.AddCell(rightAlignedHeader2);
			}
			while (nodes.Count != 0)
			{
				DcwLevelNode levelNode = nodes.Pop();
				if (levelNode.node.Key != "")
				{
					string tabs = "";
					for (int k = 0; k < levelNode.level; k++)
					{
						tabs += "\t\t\t\t\t";
					}
					table.AddCell(new Phrase(tabs + levelNode.node.OrderingText + " " + levelNode.node.Description, font));
					Font correctFont = ((levelNode.node.Value < 0.0) ? redFont : normalFont);
					PdfPCell rightAlignedK = new PdfPCell(new Phrase(levelNode.node.Key));
					rightAlignedK.HorizontalAlignment = 2;
					rightAlignedK.Border = 0;
					table.AddCell(rightAlignedK);
					PdfPCell rightAligned = new PdfPCell(new Phrase(((double)levelNode.node.Sign * levelNode.node.Value).ToString("N", new CultureInfo("de-DE")), correctFont));
					rightAligned.HorizontalAlignment = 2;
					rightAligned.Border = 0;
					table.AddCell(rightAligned);
					if (balance.HasVJahr)
					{
						Font correctFont2 = ((levelNode.node.Value2 < 0.0) ? redFont : normalFont);
						PdfPCell rightAlignedV = new PdfPCell(new Phrase(((double)levelNode.node.Sign * levelNode.node.Value2).ToString("N", new CultureInfo("de-DE")), correctFont2));
						rightAlignedV.HorizontalAlignment = 2;
						rightAlignedV.Border = 0;
						table.AddCell(rightAlignedV);
					}
				}
				int valueChildCounter = 0;
				for (int j = levelNode.node.Children.Count() - 1; j >= 0; j--)
				{
					valueChildCounter++;
				}
				for (int i = levelNode.node.Children.Count() - 1; i >= 0; i--)
				{
					string levelString = levelNode.levelString + "." + valueChildCounter;
					if (levelNode.level == 0)
					{
						levelString = Convert.ToString(valueChildCounter);
					}
					valueChildCounter--;
					nodes.Push(new DcwLevelNode(levelNode.node.Children.ElementAt(i), levelNode.level + 1, levelString));
				}
			}
			pdf.Document.Add(table);
			Phrase header = new Phrase("Bilanz");
			pdf.WriteFile(outStream, header, addPageHeader: false, addViewboxStyle: true, optimizationTexts, new Tuple<string, string>(Resources.Processor, user.GetName()), Resources.PdfPageFrom, 1, Resources.Page, Resources.Part);
		}

		private void WritePdfFromStructuredBalance(Stream outStream, StructuedTableModel balance, IOptimization opt, ILanguage language, ITableObject tab, IUser user, CancellationToken token)
		{
			List<Tuple<string, string>> optimizationTexts = opt.GetDescriptions(language);
			optimizationTexts.Reverse();
			optimizationTexts.AddRange(balance.GetParameters());
			int flag = (tab as IIssue)?.Flag ?? 0;
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			traits.CreateTableOfContents = false;
			traits.MarginTop = 100f;
			traits.MarginBottom = 60f;
			if (optimizationTexts.Count > 5)
			{
				traits.MarginTop += (optimizationTexts.Count - 6) * 14;
			}
			global::PdfGenerator.PdfGenerator pdf = new global::PdfGenerator.PdfGenerator(traits, token);
			PdfPTable tablebefore = new PdfPTable(new float[2] { 5f, 1f });
			tablebefore.DefaultCell.Border = 0;
			tablebefore.WidthPercentage = 100f;
			pdf.Document.Add(tablebefore);
			List<float> temporaryList = new List<float>();
			temporaryList.Add(4f);
			for (int m = 0; m < balance.MaximumValueCount; m++)
			{
				if (m == 0)
				{
					temporaryList.Add(1.5f);
				}
				else
				{
					temporaryList.Add(1f);
				}
			}
			PdfPTable table = new PdfPTable(temporaryList.ToArray());
			table.DefaultCell.Border = 0;
			table.SpacingBefore = 15f;
			table.SpacingAfter = 5f;
			table.WidthPercentage = 100f;
			Stack<StructuredLevelNode> nodes = new Stack<StructuredLevelNode>();
			nodes.Push(new StructuredLevelNode(balance.TreeRoot, 0, ""));
			BaseFont bfTimes = BaseFont.CreateFont("Times-Roman", "Cp1252", embedded: false);
			Font redFont = new Font(bfTimes, 12f, 0, BaseColor.RED);
			Font normalFont = new Font(bfTimes, 12f, 0);
			Font boldFont = new Font(bfTimes, 12f, 1);
			Font font = new Font(Font.FontFamily.HELVETICA, 10f);
			while (nodes.Count != 0)
			{
				StructuredLevelNode levelNode = nodes.Pop();
				if (levelNode.node.Key != "" && (levelNode.node.Description != null || !(levelNode.node.Key == "0")) && levelNode.node != balance.TreeRoot)
				{
					string tabs = "";
					for (int l = 0; l < levelNode.level; l++)
					{
						tabs += "\t\t\t\t\t";
					}
					table.AddCell(new Phrase(tabs + " " + levelNode.node.Description, font));
					for (int k = 0; k < balance.MaximumValueCount; k++)
					{
						PdfPCell rightAligned = new PdfPCell(new Phrase(levelNode.node.Value(k).ToString("N", new CultureInfo("de-DE")), (levelNode.node.Value(k) < 0.0) ? redFont : normalFont));
						rightAligned.HorizontalAlignment = 2;
						rightAligned.Border = 0;
						table.AddCell(rightAligned);
					}
				}
				int valueChildCounter = 0;
				for (int j = levelNode.node.Children.Count() - 1; j >= 0; j--)
				{
					valueChildCounter++;
				}
				for (int i = levelNode.node.Children.Count() - 1; i >= 0; i--)
				{
					string levelString = levelNode.levelString + "." + valueChildCounter;
					if (levelNode.level == 0)
					{
						levelString = Convert.ToString(valueChildCounter);
					}
					valueChildCounter--;
					nodes.Push(new StructuredLevelNode(levelNode.node.Children.ElementAt(i), levelNode.level + 1, levelString));
				}
			}
			pdf.Document.Add(table);
			Phrase header = new Phrase((flag == 4) ? "GUV" : "Bilanz");
			pdf.WriteFile(outStream, header, addPageHeader: false, addViewboxStyle: true, optimizationTexts, new Tuple<string, string>(Resources.Processor, user.GetName()), Resources.PdfPageFrom, 1, Resources.Page, Resources.Part);
		}

		private void WritePdfFromKontoauszuege(ZipOutputStream zipstream, ViewboxDb.TableObject tempObject, IOptimization opt, ILanguage language, IUser user, CancellationToken token)
		{
			BaseFont bfTimes = BaseFont.CreateFont("Times-Roman", "Cp1252", embedded: false);
			Font redFont = new Font(bfTimes, 12f, 0, BaseColor.RED);
			Font normalFont = new Font(bfTimes, 12f, 0);
			Font boldFont = new Font(bfTimes, 12f, 1);
			Font font = new Font(Font.FontFamily.HELVETICA, 10f);
			List<Tuple<string, string>> optimizationTexts = (tempObject.Additional as SimpleTableModel).DisplayParameters;
			PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			global::PdfGenerator.PdfGenerator pdf = new global::PdfGenerator.PdfGenerator(traits, token);
			Phrase header = new Phrase("Kontoauszüge");
			pdf.WriteFile(zipstream, header, addPageHeader: false, addViewboxStyle: true, optimizationTexts, new Tuple<string, string>(Resources.Processor, user.GetName()), Resources.PdfPageFrom, 1, Resources.Page, Resources.Part);
		}

		private void GDPdUDescriptor(XmlWriter xstream, ITableObjectCollection objects, ILanguage language)
		{
			xstream.WriteStartDocument();
			xstream.WriteDocType("DataSet", null, "gdpdu-01-08-2002.dtd", null);
			xstream.WriteStartElement("DataSet");
			xstream.WriteElementString("Version", "1.17.6");
			xstream.WriteStartElement("DataSupplier");
			xstream.WriteElementString("Name", null);
			xstream.WriteElementString("Location", null);
			xstream.WriteElementString("Comment", null);
			xstream.WriteEndElement();
			xstream.WriteStartElement("Media");
			xstream.WriteElementString("Name", "CD 1");
			foreach (ITableObject tobj in objects)
			{
				GDPdUDescriptorForTable(xstream, tobj, language);
			}
			xstream.WriteEndElement();
			xstream.WriteEndElement();
		}

		private void GDPdUDateTimeColumn(XmlWriter xstream, string name, string descr)
		{
			xstream.WriteStartElement("VariableColumn");
			xstream.WriteElementString("Name", name);
			xstream.WriteElementString("Description", descr + " (original as text)");
			xstream.WriteElementString("AlphaNumeric", "");
			xstream.WriteElementString("MaxLength", "19");
			xstream.WriteEndElement();
			xstream.WriteStartElement("VariableColumn");
			xstream.WriteElementString("Name", name + "__date");
			xstream.WriteElementString("Description", descr + " (splitted)");
			xstream.WriteStartElement("Date");
			xstream.WriteElementString("Format", "DD.MM.YYYY");
			xstream.WriteEndElement();
			xstream.WriteEndElement();
			xstream.WriteStartElement("VariableColumn");
			xstream.WriteElementString("Name", name + "__time");
			xstream.WriteElementString("Description", descr + " (splitted)");
			xstream.WriteElementString("AlphaNumeric", "");
			xstream.WriteElementString("MaxLength", "8");
			xstream.WriteEndElement();
		}

		private void GDPdUDescriptorForTable(XmlWriter xstream, ITableObject tobj, ILanguage language)
		{
			xstream.WriteStartElement("Table");
			string regex = "[" + string.Join(",", Path.GetInvalidFileNameChars()) + "]";
			string description = tobj.GetDescription(language);
			object obj = regex ?? "";
			if (obj == null)
			{
				obj = "";
			}
			string fname = ReplaceSpecialChars(Regex.Replace(description, (string)obj, "_"));
			xstream.WriteElementString("URL", fname + ".csv");
			xstream.WriteElementString("Name", fname);
			xstream.WriteElementString("Description", (tobj.TableName == tobj.GetDescription(language)) ? string.Empty : ReplaceSpecialChars(tobj.GetDescription(language)));
			xstream.WriteElementString("DecimalSymbol", string.Equals("de", language.CountryCode, StringComparison.CurrentCultureIgnoreCase) ? "," : ".");
			xstream.WriteElementString("DigitGroupingSymbol", null);
			xstream.WriteStartElement("VariableLength");
			foreach (IColumn column in tobj.Columns.Where((IColumn c) => c.IsVisible && !c.IsEmpty))
			{
				string name = column.Name;
				string descr = ((column.Name == column.GetDescription(language)) ? string.Empty : ReplaceSpecialChars(column.GetDescription(language)));
				SqlType type = ((column.DataType == SqlType.DateTime) ? SqlType.Date : column.DataType);
				string formatHint = string.Empty;
				List<IColumn> missingLengthInfos = new List<IColumn>();
				switch (type)
				{
				case SqlType.String:
				case SqlType.Integer:
				case SqlType.Decimal:
				case SqlType.Numeric:
				case SqlType.Binary:
					if (column.MaxLength == 0)
					{
						missingLengthInfos.Add(column);
					}
					else
					{
						formatHint = column.MaxLength.ToString();
					}
					break;
				}
				if (missingLengthInfos.Count > 0)
				{
					RetrieveMissingColumnLengthInfo(missingLengthInfos, tobj);
					formatHint = column.MaxLength.ToString();
				}
				if (column.DataType == SqlType.DateTime)
				{
					GDPdUDateTimeColumn(xstream, name, descr);
				}
				else
				{
					GDPdUDescriptorForColumn(xstream, name, descr, type, formatHint);
				}
			}
			xstream.WriteEndElement();
			xstream.WriteEndElement();
		}

		private void RetrieveMissingColumnLengthInfo(List<IColumn> missingLengthInfos, ITableObject tobj)
		{
			IColumn fc = missingLengthInfos.FirstOrDefault();
			if (fc == null)
			{
				return;
			}
			string[] param2 = missingLengthInfos.Select((IColumn c) => $"'{c.Name}'").ToArray();
			string sql = "";
			sql = ((param2.Length != 1) ? string.Format("SELECT `COLUMN_NAME`, `CHARACTER_MAXIMUM_LENGTH`, `NUMERIC_PRECISION`, `NUMERIC_SCALE` FROM `INFORMATION_SCHEMA`.`COLUMNS` WHERE `table_schema` = '{0}' AND `table_name` = '{1}' AND `column_name` IN ({2})", (fc.Table == null) ? (tobj.UserDefined ? ViewboxApplication.TempDatabaseName : tobj.Database) : (fc.Table.UserDefined ? ViewboxApplication.TempDatabaseName : fc.Table.Database), (fc.Table != null) ? fc.Table.TableName : tobj.TableName, string.Join(",", missingLengthInfos.Select((IColumn c) => $"'{c.Name}'").ToArray())) : string.Format("SELECT ii.`COLUMN_NAME`, tt.theColumnLength, ii.NUMERIC_PRECISION, ii.NUMERIC_SCALE FROM (SELECT i.`COLUMN_NAME`, i.NUMERIC_PRECISION, i.NUMERIC_SCALE FROM `INFORMATION_SCHEMA`.`COLUMNS` i WHERE i.`table_schema` = '{0}' AND i.`table_name` = '{1}' AND i.`column_name`  IN ({2})) ii,(SELECT max(length(t.`{3}`)) AS theColumnLength FROM `{0}`.`{1}` t) tt", (fc.Table == null) ? (tobj.UserDefined ? ViewboxApplication.TempDatabaseName : tobj.Database) : (fc.Table.UserDefined ? ViewboxApplication.TempDatabaseName : fc.Table.Database), (fc.Table != null) ? fc.Table.TableName : tobj.TableName, string.Join(",", param2), param2[0].Replace("'", "")));
			IColumn col = null;
			using DatabaseBase connection = ViewboxApplication.Database.ConnectionManager.GetConnection();
			using (IDataReader reader = connection.ExecuteReader(sql))
			{
				while (reader.Read())
				{
					string colName = reader.GetString(0);
					col = missingLengthInfos.SingleOrDefault((IColumn c) => string.Equals(c.Name, colName, StringComparison.CurrentCultureIgnoreCase));
					switch (col.DataType)
					{
					case SqlType.String:
					case SqlType.Binary:
						if (!reader.IsDBNull(1))
						{
							int length = (col.MaxLength = reader.GetInt32(1));
						}
						else
						{
							col.MaxLength = 255;
						}
						break;
					case SqlType.Integer:
						if (!reader.IsDBNull(2))
						{
							int precision = (col.MaxLength = reader.GetInt32(2));
						}
						break;
					case SqlType.Decimal:
					case SqlType.Numeric:
						if (!reader.IsDBNull(3))
						{
							int scale = (col.MaxLength = reader.GetInt32(3));
						}
						break;
					}
				}
			}
			connection.DbMapping.Save(col);
		}

		private void GDPdUDescriptorForColumn(XmlWriter xstream, string name, string descr, SqlType type, string formatHint)
		{
			xstream.WriteStartElement("VariableColumn");
			xstream.WriteElementString("Name", name);
			xstream.WriteElementString("Description", descr);
			switch (type)
			{
			case SqlType.String:
				xstream.WriteElementString("AlphaNumeric", null);
				xstream.WriteElementString("MaxLength", formatHint);
				break;
			case SqlType.Integer:
			case SqlType.Decimal:
			case SqlType.Numeric:
				xstream.WriteStartElement("Numeric");
				xstream.WriteElementString("Accuracy", formatHint);
				xstream.WriteEndElement();
				break;
			case SqlType.Date:
				xstream.WriteStartElement("Date");
				xstream.WriteElementString("Format", "dd.MM.yyyy");
				xstream.WriteEndElement();
				break;
			case SqlType.Time:
			case SqlType.DateTime:
				xstream.WriteElementString("AlphaNumeric", null);
				break;
			}
			xstream.WriteEndElement();
		}

		private void GDPdUDescriptorForUniversalTable(XmlWriter xstream, string name, string mainCol, List<IColumn> cols)
		{
			xstream.WriteStartDocument();
			xstream.WriteDocType("DataSet", null, "gdpdu-01-08-2002.dtd", null);
			xstream.WriteStartElement("DataSet");
			xstream.WriteElementString("Version", "1.17.6");
			xstream.WriteStartElement("DataSupplier");
			xstream.WriteElementString("Name", null);
			xstream.WriteElementString("Location", null);
			xstream.WriteElementString("Comment", null);
			xstream.WriteEndElement();
			xstream.WriteStartElement("Media");
			xstream.WriteElementString("Name", "CD 1");
			xstream.WriteStartElement("Table");
			xstream.WriteElementString("URL", name);
			xstream.WriteElementString("Name", name.Replace(".csv", ""));
			xstream.WriteElementString("Description", name.Replace(".csv", ""));
			xstream.WriteElementString("DecimalSymbol", null);
			xstream.WriteElementString("DigitGroupingSymbol", null);
			xstream.WriteStartElement("VariableLength");
			GDPdUDescriptorForColumn(xstream, "DESCR", mainCol, SqlType.String, string.Empty);
			for (int i = 0; i < cols.Count; i++)
			{
				GDPdUDescriptorForColumn(xstream, cols[i].Name, cols[i].GetDescription(ViewboxSession.Language), cols[i].DataType, string.Empty);
			}
			xstream.WriteEndElement();
			xstream.WriteEndElement();
			xstream.WriteEndElement();
			xstream.WriteEndElement();
		}

		private void GDPdUDescriptorForBalance(XmlWriter xstream, int deepestLevel)
		{
			xstream.WriteStartDocument();
			xstream.WriteDocType("DataSet", null, "gdpdu-01-08-2002.dtd", null);
			xstream.WriteStartElement("DataSet");
			xstream.WriteElementString("Version", "1.17.6");
			xstream.WriteStartElement("DataSupplier");
			xstream.WriteElementString("Name", null);
			xstream.WriteElementString("Location", null);
			xstream.WriteElementString("Comment", null);
			xstream.WriteEndElement();
			xstream.WriteStartElement("Media");
			xstream.WriteElementString("Name", "CD 1");
			xstream.WriteStartElement("Table");
			xstream.WriteElementString("URL", "Bilanz.csv");
			xstream.WriteElementString("Name", "Bilanz");
			xstream.WriteElementString("Description", "Bilanz");
			xstream.WriteElementString("DecimalSymbol", null);
			xstream.WriteElementString("DigitGroupingSymbol", null);
			xstream.WriteStartElement("VariableLength");
			for (int i = 0; i < deepestLevel - 1; i++)
			{
				GDPdUDescriptorForColumn(xstream, Resources.ExportWriteCsvFromBalanceLabel + " " + i, Resources.ExportWriteCsvFromBalanceLabel + " " + i, SqlType.String, string.Empty);
			}
			GDPdUDescriptorForColumn(xstream, Resources.ExportWriteCsvFromBalanceAccountDescription, Resources.ExportWriteCsvFromBalanceAccountDescription, SqlType.String, string.Empty);
			GDPdUDescriptorForColumn(xstream, Resources.ExportWriteCsvFromBalanceAccountValue, Resources.ExportWriteCsvFromBalanceAccountValue, SqlType.String, string.Empty);
			xstream.WriteEndElement();
			xstream.WriteEndElement();
			xstream.WriteEndElement();
			xstream.WriteEndElement();
		}

		private void GDPdUDescriptorForDcwBalance(XmlWriter xstream, int deepestLevel)
		{
			xstream.WriteStartDocument();
			xstream.WriteDocType("DataSet", null, "gdpdu-01-08-2002.dtd", null);
			xstream.WriteStartElement("DataSet");
			xstream.WriteElementString("Version", "1.17.6");
			xstream.WriteStartElement("DataSupplier");
			xstream.WriteElementString("Name", null);
			xstream.WriteElementString("Location", null);
			xstream.WriteElementString("Comment", null);
			xstream.WriteEndElement();
			xstream.WriteStartElement("Media");
			xstream.WriteElementString("Name", "CD 1");
			xstream.WriteStartElement("Table");
			xstream.WriteElementString("URL", "Bilanz.csv");
			xstream.WriteElementString("Name", "Bilanz");
			xstream.WriteElementString("Description", "Bilanz");
			xstream.WriteElementString("DecimalSymbol", null);
			xstream.WriteElementString("DigitGroupingSymbol", null);
			xstream.WriteStartElement("VariableLength");
			for (int i = 1; i <= deepestLevel; i++)
			{
				string dcwDesc = $"\"{Resources.DCWDescription} {i}\"";
				GDPdUDescriptorForColumn(xstream, dcwDesc, dcwDesc, SqlType.String, string.Empty);
			}
			GDPdUDescriptorForColumn(xstream, "Konto - Nr", "Konto - Nr", SqlType.String, string.Empty);
			GDPdUDescriptorForColumn(xstream, Resources.DCWValue, Resources.DCWValue, SqlType.String, string.Empty);
			GDPdUDescriptorForColumn(xstream, Resources.DCWOldValue, Resources.DCWOldValue, SqlType.String, string.Empty);
			xstream.WriteEndElement();
			xstream.WriteEndElement();
			xstream.WriteEndElement();
			xstream.WriteEndElement();
		}

		public string ReplaceSpecialChars(string str)
		{
			StringBuilder sb = new StringBuilder();
			foreach (char ch in str)
			{
				sb.Append(ReplaceSpecialChar(ch));
			}
			return sb.ToString();
		}

		private string ReplaceSpecialChar(char ch)
		{
			return ch switch
			{
				'ä' => "ae", 
				'Ä' => "Ae", 
				'ö' => "oe", 
				'Ö' => "Oe", 
				'ü' => "ue", 
				'Ü' => "Ue", 
				_ => ch.ToString(CultureInfo.InvariantCulture), 
			};
		}

		private PdfPCell CreateCell(Phrase phrase, BaseColor backgroundColor, int verticalAlignment = 4, int horizontalAlignment = 0, int border = 0, float paddingLeftRight = 0f, float paddingTopBottom = 0f, float borderWidth = 0.5f, float fixedHeight = 0f, bool setLeading = true, bool noWrap = false)
		{
			PdfPCell cell = new PdfPCell(phrase);
			cell.Border = border;
			cell.BackgroundColor = backgroundColor;
			cell.VerticalAlignment = verticalAlignment;
			cell.HorizontalAlignment = horizontalAlignment;
			cell.PaddingBottom = paddingTopBottom;
			cell.PaddingTop = paddingTopBottom;
			cell.PaddingLeft = paddingLeftRight;
			cell.PaddingRight = paddingLeftRight;
			cell.BorderWidth = borderWidth;
			if (fixedHeight != 0f)
			{
				cell.FixedHeight = fixedHeight;
			}
			if (setLeading)
			{
				cell.SetLeading(3f, 1f);
			}
			cell.NoWrap = noWrap;
			return cell;
		}
	}
}
