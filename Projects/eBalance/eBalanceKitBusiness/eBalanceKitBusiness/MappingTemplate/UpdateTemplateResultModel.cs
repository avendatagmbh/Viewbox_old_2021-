// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using PdfGenerator;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;
using iTextSharp.text;
using iTextSharp.text.pdf;
using IElement = Taxonomy.IElement;

namespace eBalanceKitBusiness.MappingTemplate {
    public class UpdateTemplateResultModel {
        internal UpdateTemplateResultModel(MappingTemplateHead template) { Template = template; }

        public MappingTemplateHead Template { get; set; }

        public List<UpdateTemplateResultListEntry> _deletedEntries = new List<UpdateTemplateResultListEntry>();

        public IEnumerable<UpdateTemplateResultListEntry> DeletedEntries {
            get { return _deletedEntries; }
        }

        internal bool HasDeletedEntries { get { return _deletedEntries.Count > 0; } }

        internal void AddDeletedEntry(string account, string type, IElement previousAssignment) {
            _deletedEntries.Add(new UpdateTemplateResultListEntry(account, type, previousAssignment));
        }

        public void ProcessUpgradeTemplate() {
            var taxonomy = TaxonomyManager.GetTaxonomy(TaxonomyManager.GetLatestTaxonomyInfo(Template.TaxonomyInfo.Type));
            HashSet<MappingTemplateElementInfo> blackList = new HashSet<MappingTemplateElementInfo>();
            using (var conn = AppConfig.ConnectionManager.GetConnection()) {
                conn.BeginTransaction();
                try {
                    foreach (var elementInfo in Template.ElementInfos) {
                        if (!taxonomy.Elements.ContainsKey(elementInfo.ElementId)) {
                            conn.DbMapping.Delete(elementInfo);
                            // blackList introduced by sev (23/03/2012)
                            blackList.Add(elementInfo);
                        }
                    }
                    conn.CommitTransaction();
                    // added by sev (23/03/2012) because otherwise the old / not longer valid infos will still be stored in the template 
                    // (SaveTemplate stores the template like it's given and doesn't check any DB stuff
                    foreach (var blackListEntry in blackList) {
                        Template.ElementInfos.Remove(blackListEntry);
                    }
                } catch (Exception ex) {
                    conn.RollbackTransaction();
                    throw new Exception(ex.Message);
                }
            }


            Template.TaxonomyInfo =
                TaxonomyManager.GetLatestTaxonomyInfo(Template.TaxonomyInfo.Type);

            TemplateManager.SaveTemplate(Template);
        }

        public void SaveResults(string fileName) {
            Font font = new Font(Font.FontFamily.HELVETICA, 8);
            try {
                PdfTraits traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit");
                traits.MarginLeft = 20;
                traits.MarginRight = 20;
                PdfGenerator.PdfGenerator pdfDocument = new PdfGenerator.PdfGenerator(traits);
                var headerPhrase = new Phrase("eBilanz-Kit Taxonomy Upgrade (Vorlagen)", pdfDocument.Traits.fontH1);

                pdfDocument.AddHeadline("gelöschte Zuordnungen");

                PdfPTable table = new PdfPTable(1) {HorizontalAlignment = Element.ALIGN_LEFT};
                table.DefaultCell.Border = PdfPCell.NO_BORDER;
                table.DefaultCell.BackgroundColor = null;
                table.AddCell(new PdfPCell {Border = PdfPCell.NO_BORDER});
                int i = 1;
                foreach (var deletedEntry in DeletedEntries) {
                    table.AddCell(new PdfPCell {Border = PdfPCell.NO_BORDER});
                    table.AddCell(new PdfPCell {Border = PdfPCell.TOP_BORDER}); // separator line
                    if (deletedEntry.Type != null) table.AddCell(new Phrase(i + ". " + deletedEntry.Account + " (" + deletedEntry.Type + ")", font));
                    if (deletedEntry.PreviousAssignment != null) table.AddCell(new Phrase("Taxonomieposition: " + deletedEntry.PreviousAssignment.Label, font));
                    if (deletedEntry.PreviousAssignment != null) table.AddCell(new Phrase(deletedEntry.PreviousAssignment.Id, font));
                    i++;
                }
                pdfDocument.CurrentChapter.Add(table);

                pdfDocument.WriteFile(fileName, headerPhrase);
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}

/*
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

            } */