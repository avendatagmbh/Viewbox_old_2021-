using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.Win32;
using BelegeArchivePDFGenerator.DataModels;
using System.Windows;


namespace BelegeArchivePDFGenerator.PdfGenerator
{
    public class ConcretePrintDocPDFGenerator : APDFGenerator
    {
        private string directoryPath;
        private string logoPath;

        public ConcretePrintDocPDFGenerator(DataModel dm, string directoryPath, string logoPath) : base(dm) 
        {
            this.directoryPath = directoryPath;
            this.logoPath = logoPath;
        }

        public override void Generate()
        {
            using (MemoryStream myMemoryStream = new MemoryStream())
            {
                Rectangle rec = new Rectangle(PageSize.A4);
                Document doc = new Document(rec, 24, 24, 24, 24);
                PdfWriter writer = PdfWriter.GetInstance(doc, myMemoryStream);
                var blankCell = new PdfPCell();
                blankCell.Border = Rectangle.NO_BORDER;
                doc.Open();

                // Init Fonts
                Font times6Normal = FontFactory.GetFont(FontFactory.TIMES, 6);
                Font times7Normal = FontFactory.GetFont(FontFactory.TIMES, 7);
                Font times8Bold = FontFactory.GetFont(FontFactory.TIMES_BOLD, 8);
                Font times9Normal = FontFactory.GetFont(FontFactory.TIMES, 9);
                Font times9Bold = FontFactory.GetFont(FontFactory.TIMES_BOLD, 9);
                Font times11Normal = FontFactory.GetFont(FontFactory.TIMES, 11);
                Font times11Underline = FontFactory.GetFont(FontFactory.TIMES, 11, Font.UNDERLINE);
                Font times11Bold = FontFactory.GetFont(FontFactory.TIMES_BOLD, 11);
                
                /**
                 * Document Meta Data
                 */
                /*doc.AddTitle("Hello World example");
                doc.AddSubject("This is an Example 4 of Chapter 1 of Book 'iText in Action'");
                doc.AddKeywords("Metadata, iTextSharp 5.4.4, Chapter 1, Tutorial");
                doc.AddCreator("iTextSharp 5.4.4");
                doc.AddAuthor("Debopam Pal");
                doc.AddHeader("Nothing", "No Header");*/

                /**
                 * Add Logo to the left-top
                 */
                var header = new PdfPTable(2);
                header.WidthPercentage = 100;
                header.TotalWidth = 750f;
                float[] headerWidths = { 500f, 250f};
                header.SetWidths(headerWidths);
                header.DefaultCell.Border = Rectangle.NO_BORDER;

                Image logoImage = Image.GetInstance(logoPath);
                logoImage.ScalePercent(25.0f);

                var table = new PdfPTable(1);
                var cell = new PdfPCell();
                cell.Colspan = 2;
                cell.Border = Rectangle.NO_BORDER;
                //0=Left, 1=Centre, 2=Right
                cell.HorizontalAlignment = 0;
                cell.AddElement(logoImage);

                header.AddCell(cell);
                doc.Add(table);

                // HEADER texts - LEFT
                var headerCellLeft = new PdfPCell();
                headerCellLeft.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                headerCellLeft.Border = Rectangle.NO_BORDER;
                // Address

                headerCellLeft.AddElement(new Paragraph("E.ON Thüringer Energie AG · Postfach 90 01 32 · 99104 Erfurt", times7Normal));
                headerCellLeft.AddElement(Chunk.NEWLINE);
                headerCellLeft.AddElement(new Paragraph(row.Name, times11Normal));
                headerCellLeft.AddElement(new Paragraph(row.Address, times11Normal));
                header.AddCell(headerCellLeft);

                // HEADER texts - RIGHT
                var headerCellRight = new PdfPCell();
                headerCellRight.HorizontalAlignment = PdfPCell.ALIGN_RIGHT; 
                headerCellRight.Border = Rectangle.NO_BORDER;
                // Customer info

                var paragraphToTheRight = new Paragraph("Hauptverwaltung", times7Normal);
                paragraphToTheRight.Alignment = Element.ALIGN_RIGHT;
                headerCellRight.AddElement(paragraphToTheRight);
                paragraphToTheRight = new Paragraph("Schwerborner Str. 30", times7Normal);
                paragraphToTheRight.Alignment = Element.ALIGN_RIGHT;
                headerCellRight.AddElement(paragraphToTheRight);
                paragraphToTheRight = new Paragraph("99087 Erfurt", times7Normal);
                paragraphToTheRight.Alignment = Element.ALIGN_RIGHT;
                headerCellRight.AddElement(paragraphToTheRight);

                headerCellRight.AddElement(Chunk.NEWLINE);

                paragraphToTheRight = new Paragraph("Ansprechpartner", times8Bold);
                paragraphToTheRight.Alignment = Element.ALIGN_RIGHT;
                headerCellRight.AddElement(paragraphToTheRight);
                paragraphToTheRight = new Paragraph("Czekalla, Annegret", times11Normal);
                paragraphToTheRight.Alignment = Element.ALIGN_RIGHT;
                headerCellRight.AddElement(paragraphToTheRight);
                paragraphToTheRight = new Paragraph("Telefon", times8Bold);
                paragraphToTheRight.Alignment = Element.ALIGN_RIGHT;
                headerCellRight.AddElement(paragraphToTheRight);
                paragraphToTheRight = new Paragraph("0361-652-2623", times11Normal);
                paragraphToTheRight.Alignment = Element.ALIGN_RIGHT;
                headerCellRight.AddElement(paragraphToTheRight);
                paragraphToTheRight = new Paragraph("Telefax", times8Bold);
                paragraphToTheRight.Alignment = Element.ALIGN_RIGHT;
                headerCellRight.AddElement(paragraphToTheRight);
                paragraphToTheRight = new Paragraph("0361-652-3464", times11Normal);
                paragraphToTheRight.Alignment = Element.ALIGN_RIGHT;
                headerCellRight.AddElement(paragraphToTheRight);

                headerCellRight.AddElement(Chunk.NEWLINE);

                header.AddCell(headerCellRight);
                header.AddCell(blankCell);

                var idCell = new PdfPCell();
                //idCell.Border = Rectangle.NO_BORDER;
                idCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                var contentMiddle = new Paragraph("Kundennummber  " + row.Kunden_nr, times11Normal);
                contentMiddle.Alignment = Element.ALIGN_LEFT;
                idCell.AddElement(contentMiddle);
                contentMiddle = new Paragraph("Verstagskonto  " + row.Verstags_konto, times11Normal);
                contentMiddle.Alignment = Element.ALIGN_LEFT;
                idCell.AddElement(contentMiddle);
                contentMiddle = new Paragraph("Referenznummer  " + row.Referenz_nummber, times11Normal);
                contentMiddle.Alignment = Element.ALIGN_LEFT;
                idCell.AddElement(contentMiddle);
                idCell.AddElement(Chunk.NEWLINE);

                header.AddCell(idCell);


                var dataTableBody0 = new PdfPTable(2);
                dataTableBody0.WidthPercentage = 100;
                
                var dataTableBody0_Left = new PdfPCell();
                dataTableBody0_Left.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
                dataTableBody0_Left.Border = Rectangle.NO_BORDER;
                var dataTableBody0_Right = new PdfPCell();
                dataTableBody0_Right.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                dataTableBody0_Right.Border = Rectangle.NO_BORDER;

                dataTableBody0_Left.AddElement(new Paragraph("Stromrechnung Nr. " + row.Strom_rechnung, times11Normal));
                dataTableBody0_Left.AddElement( Chunk.NEWLINE );
                dataTableBody0_Left.AddElement(new Paragraph("Abrechnung für " + row.abrechnun_date, times11Normal));
                dataTableBody0_Left.AddElement(Chunk.NEWLINE);
                dataTableBody0_Left.AddElement(new Paragraph("zum Zählpunkt: " + row.zahlpunkt, times11Normal));
                dataTableBody0_Left.AddElement(Chunk.NEWLINE);
                dataTableBody0_Left.AddElement(new Paragraph("Verbrauchsstelle: " + row.verbrauchersstelle, times11Normal));
                dataTableBody0_Left.AddElement(Chunk.NEWLINE);

                var rightContent = new Paragraph("Rechnungsdatum    " + row.rechnung_date, times11Normal);
                rightContent.Alignment = Element.ALIGN_RIGHT;
                dataTableBody0_Right.AddElement(rightContent);
                rightContent = new Paragraph("Fälligkeitsdatum  " + row.falligkeits_date, times11Normal);
                rightContent.Alignment = Element.ALIGN_RIGHT;
                dataTableBody0_Right.AddElement(rightContent);

                dataTableBody0.AddCell(dataTableBody0_Left);
                dataTableBody0.AddCell(dataTableBody0_Right);

                // table containg the data!
                var dataTableBody1 = new PdfPTable(5);
                dataTableBody1.WidthPercentage = 100;
                dataTableBody1.DefaultCell.Border = Rectangle.NO_BORDER;
                dataTableBody1.AddCell(new Paragraph("Verbrauchsart", times11Bold));
                dataTableBody1.AddCell(new Paragraph("verrechneter", times11Bold));
                dataTableBody1.AddCell(new Paragraph("Wert", times11Bold));
                dataTableBody1.AddCell(new Paragraph("Preis", times11Bold));
                dataTableBody1.AddCell(new Paragraph("Summe", times11Bold));

                dataTableBody1.AddCell(new Paragraph("Leistung", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.leistung_ver, times11Normal));
                dataTableBody1.AddCell(new Paragraph("kW", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.leistung_preis + " EUR/kW/1", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.leistung_sum + " EUR", times11Normal));

                dataTableBody1.AddCell(new Paragraph("Wirkarbeit HT", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.wirkarbeit_ht_ver, times11Normal));
                dataTableBody1.AddCell(new Paragraph("kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.wirkarbeit_ht_preis + " Ct/kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.wirkarbeit_ht_sum + " EUR", times11Normal));

                dataTableBody1.AddCell(new Paragraph("Wirkarbeit NT", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.wirkarbeit_nt_ver, times11Normal));
                dataTableBody1.AddCell(new Paragraph("kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.wirkarbeit_nt_preis + " Ct/kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.wirkarbeit_nt_sum + " EUR", times11Normal));

                dataTableBody1.AddCell(new Paragraph("Gutschrift Messpreis", times11Normal));
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(new Paragraph(row.gutschrift_messpreis + " EUR", times11Normal));

                dataTableBody1.AddCell(new Paragraph("Messung*", times11Normal));
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(new Paragraph(row.messung + " EUR", times11Normal));

                dataTableBody1.AddCell(new Paragraph("Abrechnung*", times11Normal));
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(new Paragraph(row.abrechnung + " EUR", times11Normal));

                dataTableBody1.AddCell(new Paragraph("Messstellenbetrieb*", times11Normal));
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(new Paragraph(row.messstellenbetrieb + " EUR", times11Normal));

                dataTableBody1.AddCell(new Paragraph("EEG", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.eeg_ver, times11Normal));
                dataTableBody1.AddCell(new Paragraph(" kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.eeg_preis + " Ct/kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.eeg_sum + " EUR", times11Normal));

                dataTableBody1.AddCell(new Paragraph("KWKG", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.kwkg_ver, times11Normal));
                dataTableBody1.AddCell(new Paragraph(" kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.kwkg_preis + " Ct/kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.kwkg_sum + " EUR", times11Normal));

                dataTableBody1.AddCell(new Paragraph("Stromsteuer", times11Normal));
                dataTableBody1.AddCell("");
                dataTableBody1.AddCell("");
                dataTableBody1.AddCell("");
                dataTableBody1.AddCell("");

                dataTableBody1.AddCell(new Paragraph("Wirkarbeit", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.wirkaibeit_ver, times11Normal));
                dataTableBody1.AddCell(new Paragraph(" kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.wirkaibeit_preis + " Ct/kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph(row.wirkaibeit_sum + " EUR", times11Underline));

                dataTableBody1.AddCell("");
                dataTableBody1.AddCell("");
                dataTableBody1.AddCell(new Paragraph(" %", times11Normal));
                dataTableBody1.AddCell(new Paragraph(" Umsatzsteuer", times11Normal));
                dataTableBody1.AddCell(new Paragraph(" EUR", times11Underline));

                dataTableBody1.AddCell(new Paragraph("Summe Wirkarbeit", times11Normal));
                dataTableBody1.AddCell(" ");
                dataTableBody1.AddCell(new Paragraph(" kWh", times11Normal));
                dataTableBody1.AddCell(new Paragraph("Rechnungsbetrag", times11Bold));
                dataTableBody1.AddCell(new Paragraph(row.rechnungsbetrag + " EUR", times11Underline));

                var footer = new PdfPTable(3);
                footer.HorizontalAlignment = 1;
                footer.WidthPercentage = 100;
                footer.DefaultCell.Border = Rectangle.NO_BORDER;

                var footerLeftCell = new PdfPCell();
                var footerMiddleCell = new PdfPCell();
                var footerRightCell = new PdfPCell();
                footerLeftCell.HorizontalAlignment = 1;
                footerLeftCell.VerticalAlignment = 1;
                footerLeftCell.Border = Rectangle.NO_BORDER;
                footerMiddleCell.HorizontalAlignment = 1;
                footerMiddleCell.VerticalAlignment = 1;
                footerMiddleCell.Border = Rectangle.NO_BORDER;
                footerRightCell.HorizontalAlignment = 1;
                footerRightCell.VerticalAlignment = 1;
                footerRightCell.Border = Rectangle.NO_BORDER;

                footerLeftCell.AddElement(new Paragraph("Vorstand: Reimund Gotzel (Vorsitzender),", times6Normal));
                footerLeftCell.AddElement(new Paragraph("Jürgen Gnauck (stellv. Vorsitzender),", times6Normal));
                footerLeftCell.AddElement(new Paragraph("Dr. Hilmar Klepp, Stefan G. Reindl", times6Normal));
                footerLeftCell.AddElement(new Paragraph("Vorsitzender des Aufsichtsrates: Bernd Romeike,", times6Normal));
                footerLeftCell.AddElement(new Paragraph("Sitz Erfurt,", times6Normal));
                footerLeftCell.AddElement(new Paragraph("Registergericht Jena: HRB 502044", times6Normal));
                footerLeftCell.AddElement(new Paragraph("USt-IdNr. DE258057295", times6Normal));
                footer.AddCell(footerLeftCell);

                footerMiddleCell.AddElement(new Paragraph("Bankkonten:", times6Normal));
                footerMiddleCell.AddElement(new Paragraph("Bayerische Hypo- und Vereinsbank AG Erfurt,", times6Normal));
                footerMiddleCell.AddElement(new Paragraph("Konto-Nr. 3 915 506, BLZ 820 200 86", times6Normal));
                footerMiddleCell.AddElement(new Paragraph("Deutsche Bank AG Erfurtt", times6Normal));
                footerMiddleCell.AddElement(new Paragraph("Konto-Nr. 1 338 888, BLZ 820 700 00", times6Normal));
                footer.AddCell(footerMiddleCell);

                footerRightCell.AddElement(new Paragraph("Schwerborner Straße 30", times6Normal));
                footerRightCell.AddElement(new Paragraph("Postfach 90 01 32", times6Normal));
                footerRightCell.AddElement(new Paragraph("99104 Erfurt", times6Normal));
                footerRightCell.AddElement(new Paragraph("Telefon 0361-652-0", times6Normal));
                footerRightCell.AddElement(new Paragraph("Telefax 0361-652-3490", times6Normal));
                footerRightCell.AddElement(new Paragraph("kundenservice@eon-thueringerenergie.com", times6Normal));
                footerRightCell.AddElement(new Paragraph(@"http://www.eon-thueringerenergie.com", times6Normal));
                footer.AddCell(footerRightCell);


                var dataTableBody2 = new PdfPTable(6);
                dataTableBody2.TotalWidth = 550.0f;
                dataTableBody2.LockedWidth = true;
                float[] widths = { 220f, 50f, 80f, 50f, 50f, 100f };
                dataTableBody2.SetWidths(widths);
                dataTableBody2.DefaultCell.Border = Rectangle.NO_BORDER;

                dataTableBody2.AddCell(new Paragraph("Verbrauchsart", times11Bold));
                dataTableBody2.AddCell(new Paragraph("Zählernummer", times11Bold));
                dataTableBody2.AddCell(new Paragraph("Zählerstand alt neu", times11Bold));
                dataTableBody2.AddCell(new Paragraph("Differenz", times11Bold));
                dataTableBody2.AddCell(new Paragraph("Faktor", times11Bold));
                dataTableBody2.AddCell(new Paragraph("gemessener Wert", times11Bold));

                dataTableBody2.AddCell(new Paragraph("Leist. Bez. Max HT", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.zahlnummer, times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.leistung_ver + " kW", times11Normal));

                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Wirkarb. Bez. HT", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.zahlnummer, times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.wirkarbeit_ht_ver + " kWh", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Wirkarb. Bez. NT", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.zahlnummer, times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.wirkarbeit_nt_ver + " kWh", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Blindarb. pos. HT", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.zahlnummer, times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" kVarh", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Blindarb. pos. NT", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.zahlnummer, times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" kVarh", times11Normal));

                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Erläuterungen", times11Bold));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Vorhalteleistung/Übertragungsleistung", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" kW", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Bereitgestellte Leistung", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" kW", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Mindestleistung (70% d. bereitgest. Leistung)", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" kW", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Gemessene Leistung", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.leistung_ver + " kW", times11Normal));

                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Verrechnete Werte", times11Bold));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Verrechnungsleistung", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(row.leistung_ver + " kW", times11Normal));

                dataTableBody2.AddCell(new Paragraph("Blindarbeit HT ( > 40% Wirkarbeit HT )", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" ", times11Normal));
                dataTableBody2.AddCell(new Paragraph(" kVarh", times11Normal));

                doc.Add(header);
                doc.Add(dataTableBody0);
                doc.Add(dataTableBody1);
                doc.Add(new Paragraph(" " + Chunk.NEWLINE + Chunk.NEWLINE + Chunk.NEWLINE));
                doc.Add(footer);
                doc.Add(new Paragraph("Abrechnung für " + row.abrechnun_date, times11Normal));
                doc.Add(new Paragraph("Vertragskonto " + row.Verstags_konto, times11Normal));
                doc.Add(new Paragraph("Stromrechnung Nr. " + row.Strom_rechnung + Chunk.NEWLINE + Chunk.NEWLINE, times11Normal));

                doc.Add(new Paragraph("Bitte überweisen Sie den Rechnungsbetrag bis zum 20.07.2010 auf das Konto 1338888", times11Normal));
                doc.Add(new Paragraph("BLZ 82070000 Deutsche Bank Erfurt unter Angabe Ihrer Kundennummer." + Chunk.NEWLINE + Chunk.NEWLINE, times11Normal));
                doc.Add(new Paragraph("Die berechneten Beträge nach EEG¹ und KWKG² sind vorläufig.", times11Normal));
                doc.Add(new Paragraph("BLZ 82070000 Deutsche Bank Erfurt unter Angabe Ihrer Kundennummer.", times11Normal));
                doc.Add(new Paragraph("¹ EEG: Gesetz für den Vorrang Erneuerbarer Energien", times11Normal));
                doc.Add(new Paragraph("² KWKG: Gesetz zum Schutz der Stromerzeugung aus Kraft-Wärme-Kopplung" + Chunk.NEWLINE + Chunk.NEWLINE, times11Normal));
                doc.Add(new Paragraph("Verbrauchsermittlung" + Chunk.NEWLINE + Chunk.NEWLINE, times11Bold));
                doc.Add(new Paragraph("Ablesung: 01.06.2010 - 30.06.2010" + Chunk.NEWLINE + Chunk.NEWLINE, times11Normal));
                doc.Add(dataTableBody2);
                doc.Add(new Paragraph(" " + Chunk.NEWLINE + Chunk.NEWLINE));
                doc.Add(new Paragraph("Ausweis der Netzentgelte, Stromkennzeichnung und Umweltauswirkungen" + Chunk.NEWLINE + Chunk.NEWLINE, times9Bold));
                doc.Add(new Paragraph("Im Bruttobetrag von " + row.rechnungsbetrag + " EUR sind Kosten für die Netznutzung von 1.020,34 EUR, gesetzliche Steuern und Abgaben von" +
                        "1.942,56 EUR enthalten. In den Kosten für die Netznutzung sind 60,00 EUR für den Messstellenbetrieb und 9,00 EUR für die Messung enthalten." + Chunk.NEWLINE + Chunk.NEWLINE, times9Normal));
                doc.Add(new Paragraph("Ausweis der Netzentgelte, Stromkennzeichnung und Umweltauswirkungen Unser Energiemix setzt sich aus 14 % Kernkraft, 67 % fossilen und sonstigen Energieträgern sowie 19 % erneuerbaren Energien" +
                        "zusammen. Damit sind 564 g/kWh CO 2-Emissionen und 0,0004 g/kWh radioaktiver Abfall verbunden. Der Energiemix in Deutschland setzt sich im Durchschnitt aus 25 % Kernkraft, 59 % fossilen und sonstigen Energieträgern sowie 16 % erneuerbaren" +
                        "Energien zusammen. Damit sind 506 g/kWh CO 2-Emissionen und 0,0007 g/kWh radioaktiver Abfall verbunden." + Chunk.NEWLINE + Chunk.NEWLINE, times9Normal));
                doc.Add(new Paragraph("Diese Angaben entsprechen den Anforderungen nach § 42 Energiewirtschaftsgesetz (EnWG). * Die Preise für Messstellenbetrieb, Messung und Abrechnung werden taggenau auf Grundlage der im Preisblatt enthaltenen Jahrespreise berechnet. (Jahrespreis / 365 Tage x Anzahl der Tage des Abrechnungszeitraumes)", times9Normal));
                doc.Close();

                byte[] bytes = myMemoryStream.ToArray();
                using (FileStream fs = File.Create(directoryPath + @"\" + row.Kunden_nr + "-" + row.Verstags_konto + "-" + row.Referenz_nummber + ".pdf"))
                {
                    fs.Write(bytes, 0, (int)bytes.Length);
                }
            }
        }

        private string LoadLogo()
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Select Logo";
            dialog.ShowDialog();

            return dialog.FileName;
        }
    }
}
