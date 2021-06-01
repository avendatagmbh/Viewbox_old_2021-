// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-20
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using PdfGenerator;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace eBalanceKitBusiness.Structures {
    public class DocumentUpgradeResults {
        public DocumentUpgradeResult DocumentUpgradeResultGcd { get; internal set; }
        public DocumentUpgradeResult DocumentUpgradeResultGaap { get; internal set; }

        public void SaveResults(string fileName) {
            Font font = new Font(Font.FontFamily.HELVETICA, 8);
            try {
                PdfTraits traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit");
                traits.MarginLeft = 20;
                traits.MarginRight = 20;
                PdfGenerator.PdfGenerator pdfDocument = new PdfGenerator.PdfGenerator(traits);
                var headerPhrase = new Phrase("eBilanz-Kit - Ergebnisse Taxonomie-Upgrade (Firmenstammdaten)", pdfDocument.Traits.fontH1);

                SaveResults(pdfDocument, DocumentUpgradeResultGcd, font, "Allgemeine Informationen");
                SaveResults(pdfDocument, DocumentUpgradeResultGaap, font, "Reportbestandteile");

                pdfDocument.WriteFile(fileName, headerPhrase);
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        private void SaveResults(PdfGenerator.PdfGenerator pdfDocument, DocumentUpgradeResult result, Font font, string header) {
            pdfDocument.AddHeadline(header);
            PdfPTable table = new PdfPTable(1) { HorizontalAlignment = Element.ALIGN_LEFT };
            table.DefaultCell.Border = PdfPCell.NO_BORDER;
            table.DefaultCell.BackgroundColor = null;
            table.AddCell(new PdfPCell { Border = PdfPCell.NO_BORDER });
            int i = 1;
            foreach (var deletedValue in result.DeletedValues) {
                table.AddCell(new PdfPCell { Border = PdfPCell.NO_BORDER });
                table.AddCell(new PdfPCell { Border = PdfPCell.TOP_BORDER }); // separator line
                if (deletedValue.Element != null) {
                    table.AddCell(new Phrase(i + ". " + deletedValue.Element.Label, font));
                    table.AddCell(new Phrase(deletedValue.Element.Id, font));
                }
                table.AddCell(new Phrase(deletedValue.Value, font));
                i++;
            }
            pdfDocument.CurrentChapter.Add(table);
        }
    }
}