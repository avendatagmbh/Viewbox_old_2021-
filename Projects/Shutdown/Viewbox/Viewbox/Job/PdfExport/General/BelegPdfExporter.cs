using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using SystemDb;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfGenerator;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Job.PdfExport.General
{
	public class BelegPdfExporter : IPdfExporter
	{
		private readonly CancellationToken token;

		public IDataReader DataReader { get; set; }

		public Stream TargetStream { get; set; }

		public List<IColumn> Columns { get; set; }

		public ITableObject TableObject { get; set; }

		public CancellationToken CancellationToken { get; set; }

		public ILanguage Language { get; set; }

		public IOptimization Optimization { get; set; }

		public IUser User { get; set; }

		public PdfExporterConfigElement ExporterConfig { get; set; }

		public bool Closing { get; set; }

		public bool HasGroupSubtotal { get; set; }

		public TableObjectCollection TempTableObjects { get; set; }

		public BelegPdfExporter(CancellationToken token)
		{
			this.token = token;
		}

		public void Init()
		{
		}

		public void Export()
		{
			//PdfTraits traits = new PdfTraits("Viewbox", "Viewbox");
			//traits.CreateTableOfContents = false;
			//traits.PageSizeAndOrientation = PageSize.A4;
			//traits.MarginLeft = 24f;
			//traits.MarginRight = 24f;
			//traits.MarginTop = 24f;
			//traits.MarginBottom = 24f;
			//global::PdfGenerator.PdfGenerator generator = new global::PdfGenerator.PdfGenerator(traits, token);
			//DataTable dbTable = new DataTable();
			//dbTable.Load(DataReader);
			//generator.Document.NewPage();
			//PdfPCell blankCell = new PdfPCell();
			//blankCell.Border = 0;
			//Font times6Normal = FontFactory.GetFont("Times", 6f);
			//Font times7Normal = FontFactory.GetFont("Times", 7f);
			//Font times8Bold = FontFactory.GetFont("Times-Bold", 8f);
			//Font times9Normal = FontFactory.GetFont("Times", 9f);
			//Font times9Bold = FontFactory.GetFont("Times-Bold", 9f);
			//Font times11Normal = FontFactory.GetFont("Times", 11f);
			//Font times11Underline = FontFactory.GetFont("Times", 11f, 4);
			//Font times11Bold = FontFactory.GetFont("Times-Bold", 11f);
			//PdfPTable header = new PdfPTable(2);
			//header.WidthPercentage = 100f;
			//header.TotalWidth = 750f;
			//float[] headerWidths = new float[2] { 500f, 250f };
			//header.SetWidths(headerWidths);
			//header.DefaultCell.Border = 0;
			//Image logoImage = Image.GetInstance(Resources.logo, ImageFormat.Bmp);
			//logoImage.ScalePercent(25f);
			//PdfPTable table = new PdfPTable(1);
			//PdfPCell cell = new PdfPCell();
			//cell.Colspan = 2;
			//cell.Border = 0;
			//cell.HorizontalAlignment = 0;
			//cell.AddElement(logoImage);
			//header.AddCell(cell);
			//generator.Document.Add(table);
			//PdfPCell headerCellLeft = new PdfPCell();
			//headerCellLeft.HorizontalAlignment = 0;
			//headerCellLeft.Border = 0;
			//headerCellLeft.AddElement(new Paragraph("E.ON Thüringer Energie AG · Postfach 90 01 32 · 99104 Erfurt", times7Normal));
			//headerCellLeft.AddElement(Chunk.NEWLINE);
			//headerCellLeft.AddElement(new Paragraph(dbTable.Rows[0][0].ToString(), times11Normal));
			//headerCellLeft.AddElement(new Paragraph(dbTable.Rows[0][5].ToString(), times11Normal));
			//header.AddCell(headerCellLeft);
			//PdfPCell headerCellRight = new PdfPCell();
			//headerCellRight.HorizontalAlignment = 2;
			//headerCellRight.Border = 0;
			//Paragraph paragraphToTheRight = new Paragraph("Hauptverwaltung", times7Normal);
			//paragraphToTheRight.Alignment = 2;
			//headerCellRight.AddElement(paragraphToTheRight);
			//paragraphToTheRight = new Paragraph("Schwerborner Str. 30", times7Normal);
			//paragraphToTheRight.Alignment = 2;
			//headerCellRight.AddElement(paragraphToTheRight);
			//paragraphToTheRight = new Paragraph("99087 Erfurt", times7Normal);
			//paragraphToTheRight.Alignment = 2;
			//headerCellRight.AddElement(paragraphToTheRight);
			//headerCellRight.AddElement(Chunk.NEWLINE);
			//paragraphToTheRight = new Paragraph("Ansprechpartner", times8Bold);
			//paragraphToTheRight.Alignment = 2;
			//headerCellRight.AddElement(paragraphToTheRight);
			//paragraphToTheRight = new Paragraph("Czekalla, Annegret", times11Normal);
			//paragraphToTheRight.Alignment = 2;
			//headerCellRight.AddElement(paragraphToTheRight);
			//paragraphToTheRight = new Paragraph("Telefon", times8Bold);
			//paragraphToTheRight.Alignment = 2;
			//headerCellRight.AddElement(paragraphToTheRight);
			//paragraphToTheRight = new Paragraph("0361-652-2623", times11Normal);
			//paragraphToTheRight.Alignment = 2;
			//headerCellRight.AddElement(paragraphToTheRight);
			//paragraphToTheRight = new Paragraph("Telefax", times8Bold);
			//paragraphToTheRight.Alignment = 2;
			//headerCellRight.AddElement(paragraphToTheRight);
			//paragraphToTheRight = new Paragraph("0361-652-3464", times11Normal);
			//paragraphToTheRight.Alignment = 2;
			//headerCellRight.AddElement(paragraphToTheRight);
			//headerCellRight.AddElement(Chunk.NEWLINE);
			//header.AddCell(headerCellRight);
			//header.AddCell(blankCell);
			//PdfPCell idCell = new PdfPCell();
			//idCell.HorizontalAlignment = 0;
			//Paragraph contentMiddle = new Paragraph("Kundennummber  " + dbTable.Rows[0][6].ToString(), times11Normal);
			//contentMiddle.Alignment = 0;
			//idCell.AddElement(contentMiddle);
			//contentMiddle = new Paragraph("Verstagskonto  " + dbTable.Rows[0][7].ToString(), times11Normal);
			//contentMiddle.Alignment = 0;
			//idCell.AddElement(contentMiddle);
			//contentMiddle = new Paragraph("Referenznummer  " + dbTable.Rows[0][8].ToString(), times11Normal);
			//contentMiddle.Alignment = 0;
			//idCell.AddElement(contentMiddle);
			//idCell.AddElement(Chunk.NEWLINE);
			//header.AddCell(idCell);
			//PdfPTable dataTableBody0 = new PdfPTable(2);
			//dataTableBody0.WidthPercentage = 100f;
			//PdfPCell dataTableBody0_Left = new PdfPCell();
			//dataTableBody0_Left.HorizontalAlignment = 0;
			//dataTableBody0_Left.Border = 0;
			//PdfPCell dataTableBody0_Right = new PdfPCell();
			//dataTableBody0_Right.HorizontalAlignment = 2;
			//dataTableBody0_Right.Border = 0;
			//dataTableBody0_Left.AddElement(new Paragraph("Stromrechnung Nr. " + dbTable.Rows[0][9].ToString(), times11Normal));
			//dataTableBody0_Left.AddElement(Chunk.NEWLINE);
			//dataTableBody0_Left.AddElement(new Paragraph(dbTable.Rows[0][10].ToString() + " " + dbTable.Rows[0][11].ToString(), times11Normal));
			//dataTableBody0_Left.AddElement(Chunk.NEWLINE);
			//dataTableBody0_Left.AddElement(new Paragraph("zum Zählpunkt: " + dbTable.Rows[0][14].ToString(), times11Normal));
			//dataTableBody0_Left.AddElement(Chunk.NEWLINE);
			//dataTableBody0_Left.AddElement(new Paragraph(dbTable.Rows[0][15].ToString() + dbTable.Rows[0][16].ToString(), times11Normal));
			//dataTableBody0_Left.AddElement(new Paragraph(dbTable.Rows[0][17].ToString() + " Str " + dbTable.Rows[0][18].ToString(), times11Normal));
			//dataTableBody0_Left.AddElement(new Paragraph(dbTable.Rows[0][19].ToString() + " Str " + dbTable.Rows[0][20].ToString(), times11Normal));
			//dataTableBody0_Left.AddElement(Chunk.NEWLINE);
			//Paragraph rightContent = new Paragraph("Rechnungsdatum    " + dbTable.Rows[0][13].ToString(), times11Normal);
			//rightContent.Alignment = 2;
			//dataTableBody0_Right.AddElement(rightContent);
			//rightContent = new Paragraph("Fälligkeitsdatum  " + dbTable.Rows[0][12].ToString(), times11Normal);
			//rightContent.Alignment = 2;
			//dataTableBody0_Right.AddElement(rightContent);
			//dataTableBody0.AddCell(dataTableBody0_Left);
			//dataTableBody0.AddCell(dataTableBody0_Right);
			//PdfPTable dataTableBody1 = new PdfPTable(5);
			//dataTableBody1.WidthPercentage = 100f;
			//dataTableBody1.DefaultCell.Border = 0;
			//dataTableBody1.AddCell(new Paragraph("Verbrauchsart", times11Bold));
			//dataTableBody1.AddCell(new Paragraph("verrechneter", times11Bold));
			//dataTableBody1.AddCell(new Paragraph("Wert", times11Bold));
			//dataTableBody1.AddCell(new Paragraph("Preis", times11Bold));
			//dataTableBody1.AddCell(new Paragraph("Summe", times11Bold));
			//dataTableBody1.AddCell(new Paragraph("Leistung", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][21].ToString(), times11Normal));
			//dataTableBody1.AddCell(new Paragraph("kW", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][33].ToString() + " EUR/kW/1", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][45].ToString() + " EUR", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("Wirkarbeit HT", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][22].ToString(), times11Normal));
			//dataTableBody1.AddCell(new Paragraph("kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][34].ToString() + " Ct/kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][46].ToString() + " EUR", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("Wirkarbeit NT", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][23].ToString(), times11Normal));
			//dataTableBody1.AddCell(new Paragraph("kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][35].ToString() + " Ct/kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][47].ToString() + " EUR", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("Gutschrift Messpreis", times11Normal));
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][36].ToString() + " EUR", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("Messung*", times11Normal));
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][37].ToString() + " EUR", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("Abrechnung*", times11Normal));
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][38].ToString() + " EUR", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("Messstellenbetrieb*", times11Normal));
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][39].ToString() + " EUR", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("EEG", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][28].ToString(), times11Normal));
			//dataTableBody1.AddCell(new Paragraph(" kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][40].ToString() + " Ct/kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][52].ToString() + " EUR", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("KWKG", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][29].ToString(), times11Normal));
			//dataTableBody1.AddCell(new Paragraph(" kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][41].ToString() + " Ct/kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][39].ToString() + " EUR", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("Stromsteuer", times11Normal));
			//dataTableBody1.AddCell("");
			//dataTableBody1.AddCell("");
			//dataTableBody1.AddCell("");
			//dataTableBody1.AddCell("");
			//dataTableBody1.AddCell(new Paragraph("Wirkarbeit", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][31].ToString(), times11Normal));
			//dataTableBody1.AddCell(new Paragraph(" kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][43].ToString() + " Ct/kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][45].ToString() + " EUR", times11Underline));
			//dataTableBody1.AddCell("");
			//dataTableBody1.AddCell("");
			//dataTableBody1.AddCell(new Paragraph(" %", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(" Umsatzsteuer", times11Normal));
			//dataTableBody1.AddCell(new Paragraph(" EUR", times11Underline));
			//dataTableBody1.AddCell(new Paragraph("Summe Wirkarbeit", times11Normal));
			//dataTableBody1.AddCell(" ");
			//dataTableBody1.AddCell(new Paragraph(" kWh", times11Normal));
			//dataTableBody1.AddCell(new Paragraph("Rechnungsbetrag", times11Bold));
			//dataTableBody1.AddCell(new Paragraph(dbTable.Rows[0][56].ToString() + " EUR", times11Underline));
			//PdfPTable footer = new PdfPTable(3);
			//footer.HorizontalAlignment = 1;
			//footer.WidthPercentage = 100f;
			//footer.DefaultCell.Border = 0;
			//PdfPCell footerLeftCell = new PdfPCell();
			//PdfPCell footerMiddleCell = new PdfPCell();
			//PdfPCell footerRightCell = new PdfPCell();
			//footerLeftCell.HorizontalAlignment = 1;
			//footerLeftCell.VerticalAlignment = 1;
			//footerLeftCell.Border = 0;
			//footerMiddleCell.HorizontalAlignment = 1;
			//footerMiddleCell.VerticalAlignment = 1;
			//footerMiddleCell.Border = 0;
			//footerRightCell.HorizontalAlignment = 1;
			//footerRightCell.VerticalAlignment = 1;
			//footerRightCell.Border = 0;
			//footerLeftCell.AddElement(new Paragraph("Vorstand: Reimund Gotzel (Vorsitzender),", times6Normal));
			//footerLeftCell.AddElement(new Paragraph("Jürgen Gnauck (stellv. Vorsitzender),", times6Normal));
			//footerLeftCell.AddElement(new Paragraph("Dr. Hilmar Klepp, Stefan G. Reindl", times6Normal));
			//footerLeftCell.AddElement(new Paragraph("Vorsitzender des Aufsichtsrates: Bernd Romeike,", times6Normal));
			//footerLeftCell.AddElement(new Paragraph("Sitz Erfurt,", times6Normal));
			//footerLeftCell.AddElement(new Paragraph("Registergericht Jena: HRB 502044", times6Normal));
			//footerLeftCell.AddElement(new Paragraph("USt-IdNr. DE258057295", times6Normal));
			//footer.AddCell(footerLeftCell);
			//footerMiddleCell.AddElement(new Paragraph("Bankkonten:", times6Normal));
			//footerMiddleCell.AddElement(new Paragraph("Bayerische Hypo- und Vereinsbank AG Erfurt,", times6Normal));
			//footerMiddleCell.AddElement(new Paragraph("Konto-Nr. 3 915 506, BLZ 820 200 86", times6Normal));
			//footerMiddleCell.AddElement(new Paragraph("Deutsche Bank AG Erfurtt", times6Normal));
			//footerMiddleCell.AddElement(new Paragraph("Konto-Nr. 1 338 888, BLZ 820 700 00", times6Normal));
			//footer.AddCell(footerMiddleCell);
			//footerRightCell.AddElement(new Paragraph("Schwerborner Straße 30", times6Normal));
			//footerRightCell.AddElement(new Paragraph("Postfach 90 01 32", times6Normal));
			//footerRightCell.AddElement(new Paragraph("99104 Erfurt", times6Normal));
			//footerRightCell.AddElement(new Paragraph("Telefon 0361-652-0", times6Normal));
			//footerRightCell.AddElement(new Paragraph("Telefax 0361-652-3490", times6Normal));
			//footerRightCell.AddElement(new Paragraph("kundenservice@eon-thueringerenergie.com", times6Normal));
			//footerRightCell.AddElement(new Paragraph("http://www.eon-thueringerenergie.com", times6Normal));
			//footer.AddCell(footerRightCell);
			//PdfPTable dataTableBody2 = new PdfPTable(6);
			//dataTableBody2.TotalWidth = 550f;
			//dataTableBody2.LockedWidth = true;
			//float[] widths = new float[6] { 220f, 50f, 80f, 50f, 50f, 100f };
			//dataTableBody2.SetWidths(widths);
			//dataTableBody2.DefaultCell.Border = 0;
			//dataTableBody2.AddCell(new Paragraph("Verbrauchsart", times11Bold));
			//dataTableBody2.AddCell(new Paragraph("Zählernummer", times11Bold));
			//dataTableBody2.AddCell(new Paragraph("Zählerstand alt neu", times11Bold));
			//dataTableBody2.AddCell(new Paragraph("Differenz", times11Bold));
			//dataTableBody2.AddCell(new Paragraph("Faktor", times11Bold));
			//dataTableBody2.AddCell(new Paragraph("gemessener Wert", times11Bold));
			//dataTableBody2.AddCell(new Paragraph("Leist. Bez. Max HT", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("6547233", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(dbTable.Rows[0][21].ToString() + " kW", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Wirkarb. Bez. HT", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("6547233", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(dbTable.Rows[0][22].ToString() + " kWh", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Wirkarb. Bez. NT", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("6547233", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(dbTable.Rows[0][23].ToString() + " kWh", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Blindarb. pos. HT", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("6547233", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" kVarh", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Blindarb. pos. NT", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("6547233", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" kVarh", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Erläuterungen", times11Bold));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Vorhalteleistung/Übertragungsleistung", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" kW", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Bereitgestellte Leistung", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" kW", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Mindestleistung (70% d. bereitgest. Leistung)", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" kW", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Gemessene Leistung", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("?? kW", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Verrechnete Werte", times11Bold));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Verrechnungsleistung", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(dbTable.Rows[0][21].ToString() + " kW", times11Normal));
			//dataTableBody2.AddCell(new Paragraph("Blindarbeit HT ( > 40% Wirkarbeit HT )", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
			//dataTableBody2.AddCell(new Paragraph(" kVarh", times11Normal));
			//generator.Document.Add(header);
			//generator.Document.Add(dataTableBody0);
			//generator.Document.Add(dataTableBody1);
			//generator.Document.Add(new Paragraph(string.Concat(" ", Chunk.NEWLINE, Chunk.NEWLINE, Chunk.NEWLINE)));
			//generator.Document.Add(footer);
			//generator.Document.Add(new Paragraph(dbTable.Rows[0][10].ToString(), times11Normal));
			//generator.Document.Add(new Paragraph("Vertragskonto " + dbTable.Rows[0][7].ToString(), times11Normal));
			//generator.Document.Add(new Paragraph(string.Concat("Stromrechnung Nr. ", dbTable.Rows[0][9].ToString(), Chunk.NEWLINE, Chunk.NEWLINE), times11Normal));
			//generator.Document.Add(new Paragraph("Bitte überweisen Sie den Rechnungsbetrag bis zum 20.07.2010 auf das Konto 1338888", times11Normal));
			//generator.Document.Add(new Paragraph(string.Concat("BLZ 82070000 Deutsche Bank Erfurt unter Angabe Ihrer Kundennummer.", Chunk.NEWLINE, Chunk.NEWLINE), times11Normal));
			//generator.Document.Add(new Paragraph("Die berechneten Beträge nach EEG¹ und KWKG² sind vorläufig.", times11Normal));
			//generator.Document.Add(new Paragraph("BLZ 82070000 Deutsche Bank Erfurt unter Angabe Ihrer Kundennummer.", times11Normal));
			//generator.Document.Add(new Paragraph("¹ EEG: Gesetz für den Vorrang Erneuerbarer Energien", times11Normal));
			//generator.Document.Add(new Paragraph(string.Concat("² KWKG: Gesetz zum Schutz der Stromerzeugung aus Kraft-Wärme-Kopplung", Chunk.NEWLINE, Chunk.NEWLINE), times11Normal));
			//generator.Document.Add(new Paragraph(string.Concat("Verbrauchsermittlung", Chunk.NEWLINE, Chunk.NEWLINE), times11Bold));
			//generator.Document.Add(new Paragraph(string.Concat("Ablesung: 01.06.2010 - 30.06.2010", Chunk.NEWLINE, Chunk.NEWLINE), times11Normal));
			//generator.Document.Add(dataTableBody2);
			//generator.Document.Add(new Paragraph(string.Concat(" ", Chunk.NEWLINE, Chunk.NEWLINE)));
			//generator.Document.Add(new Paragraph(string.Concat("Ausweis der Netzentgelte, Stromkennzeichnung und Umweltauswirkungen", Chunk.NEWLINE, Chunk.NEWLINE), times9Bold));
			//generator.Document.Add(new Paragraph(string.Concat("Im Bruttobetrag von ", dbTable.Rows[0][56].ToString(), " EUR sind Kosten für die Netznutzung von 1.020,34 EUR, gesetzliche Steuern und Abgaben von1.942,56 EUR enthalten. In den Kosten für die Netznutzung sind 60,00 EUR für den Messstellenbetrieb und 9,00 EUR für die Messung enthalten.", Chunk.NEWLINE, Chunk.NEWLINE), times9Normal));
			//generator.Document.Add(new Paragraph(string.Concat("Ausweis der Netzentgelte, Stromkennzeichnung und Umweltauswirkungen Unser Energiemix setzt sich aus 14 % Kernkraft, 67 % fossilen und sonstigen Energieträgern sowie 19 % erneuerbaren Energienzusammen. Damit sind 564 g/kWh CO 2-Emissionen und 0,0004 g/kWh radioaktiver Abfall verbunden. Der Energiemix in Deutschland setzt sich im Durchschnitt aus 25 % Kernkraft, 59 % fossilen und sonstigen Energieträgern sowie 16 % erneuerbarenEnergien zusammen. Damit sind 506 g/kWh CO 2-Emissionen und 0,0007 g/kWh radioaktiver Abfall verbunden.", Chunk.NEWLINE, Chunk.NEWLINE), times9Normal));
			//generator.Document.Add(new Paragraph("Diese Angaben entsprechen den Anforderungen nach § 42 Energiewirtschaftsgesetz (EnWG). * Die Preise für Messstellenbetrieb, Messung und Abrechnung werden taggenau auf Grundlage der im Preisblatt enthaltenen Jahrespreise berechnet. (Jahrespreis / 365 Tage x Anzahl der Tage des Abrechnungszeitraumes)", times9Normal));
			//generator.WriteFile(TargetStream, new Phrase(Resources.Table + ": " + TableObject.GetDescription(Language)), addPageHeader: false, addViewboxStyle: false, null, new Tuple<string, string>(Resources.Processor, User.GetName()), Resources.PdfPageFrom, 0, Resources.Page, Resources.Part, closing: false);
		}
	}
}
