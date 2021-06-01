using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PdfGenerator;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace eBalanceKit.Windows.MappingTemplates.Models {
    public class ExportChangedTaxonomyToPdf {
        private readonly List<MappingTemplateLine> _mappingTemplate;
        public ExportChangedTaxonomyToPdf(List<MappingTemplateLine> mappingTemplate) { _mappingTemplate = mappingTemplate; }

        public void ExportPdf() {
            var dlgExportDeletedElements = new DlgExportDeletedElements();
            dlgExportDeletedElements.ShowDialog();

            if (dlgExportDeletedElements.Export) {

                var traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit");
                var table = new PdfPTable(1) {HorizontalAlignment = Element.ALIGN_LEFT};

                try {
                    var pdfDocument = new PdfGenerator.PdfGenerator(traits);
                    var headerPhrase = new Phrase("eBilanz-Kit", pdfDocument.Traits.fontH1);
                    pdfDocument.AddHeadline("Gelöschte Elemente");

                    foreach (var mappingTemplateLine in _mappingTemplate) {
                        var accountName = new PdfPCell(new Phrase("Bezeichnung: ")) {Border = 0, BackgroundColor = null};
                        table.AddCell(accountName);

                        var accountValue = new PdfPCell(new Phrase(mappingTemplateLine.AccountName))
                        {Border = 0, BackgroundColor = null};
                        table.AddCell(accountValue);

                        var accountNumber = new PdfPCell(new Phrase("Kontonr.: ")) {Border = 0, BackgroundColor = null};
                        table.AddCell(accountNumber);

                        var accountNumberValue = new PdfPCell(new Phrase(mappingTemplateLine.AccountNumber))
                        {Border = 0, BackgroundColor = null};
                        table.AddCell(accountNumberValue);

                        var debitID = new PdfPCell(new Phrase("Debit ID: ")) {Border = 0, BackgroundColor = null};
                        table.AddCell(debitID);

                        var debitIDValue = new PdfPCell(new Phrase(mappingTemplateLine.DebitElementId))
                        {Border = 0, BackgroundColor = null};
                        table.AddCell(debitIDValue);

                        var creditID = new PdfPCell(new Phrase("Credit ID: ")) {Border = 0, BackgroundColor = null};
                        table.AddCell(creditID);

                        var creditIDValue = new PdfPCell(new Phrase(mappingTemplateLine.CreditElementId))
                        {Border = 0, BackgroundColor = null};
                        table.AddCell(creditIDValue);

                        var placeHolder = new PdfPCell(new Phrase("")) {Border = 0, BackgroundColor = null};
                        table.AddCell(placeHolder);
                    }

                    pdfDocument.CurrentChapter.Add(table);
                    if (!string.IsNullOrEmpty(dlgExportDeletedElements.txtFile.Text)) pdfDocument.WriteFile(dlgExportDeletedElements.txtFile.Text, headerPhrase);
                    dlgExportDeletedElements.Close();
                } catch (DocumentException ex) {

                    throw new Exception(ex.Message);
                }

            }
        }
    }
}
