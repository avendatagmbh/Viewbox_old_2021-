// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-14
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using PdfGenerator;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Document = eBalanceKitBusiness.Structures.DbMapping.Document;

namespace eBalanceKitBusiness.MappingTemplate {
    public class ApplyTemplateModel : TemplateModelBase {
        public ApplyTemplateModel(Document document, MappingTemplateHead template)
            : base(document, template) {
            
            AssignmentErrors = new List<AssignmentErrorListEntry>();
        }

        public bool ReplaceAutoComputeEnabledFlag { get; set; }
        public bool ReplaceSendAccountBalanceFlag { get; set; }
        public bool ReplaceIgnoreWarningMessageFlag { get; set; }

        public List<AssignmentErrorListEntry> AssignmentErrors { get; private set; }
        
        public void SaveResults(string fileName) {
            Font font = new Font(Font.FontFamily.HELVETICA, 8);
            try {
                PdfTraits traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit");
                traits.MarginLeft = 20;
                traits.MarginRight = 20;
                PdfGenerator.PdfGenerator pdfDocument = new PdfGenerator.PdfGenerator(traits);
                var headerPhrase = new Phrase("eBilanz-Kit Fehlerreport", pdfDocument.Traits.fontH1);

                foreach (var assignmentErrorListEntry in AssignmentErrors) {
                    pdfDocument.AddHeadline(assignmentErrorListEntry.BalanceList.Name);
                    PdfPTable table = new PdfPTable(1) { HorizontalAlignment = Element.ALIGN_LEFT };
                    table.DefaultCell.Border = PdfPCell.NO_BORDER;
                    table.DefaultCell.BackgroundColor = null;
                    table.AddCell(new PdfPCell { Border = PdfPCell.NO_BORDER });
                    int i = 1;
                    foreach (var assignmentError in assignmentErrorListEntry.Errors) {
                        table.AddCell(new PdfPCell { Border = PdfPCell.NO_BORDER });
                        table.AddCell(new PdfPCell { Border = PdfPCell.TOP_BORDER }); // separator line
                        table.AddCell(new Phrase(i + ". " + assignmentError.Message, font));
                        table.AddCell(new Phrase(assignmentError.AccountLabel, font));
                        table.AddCell(new Phrase("Taxonomieposition: " + assignmentError.Element.Label, font));
                        table.AddCell(new Phrase(assignmentError.Element.Id, font));
                        i++;
                    }
                    pdfDocument.CurrentChapter.Add(table);
                }

                pdfDocument.WriteFile(fileName, headerPhrase);
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }            
        }
    }
}
