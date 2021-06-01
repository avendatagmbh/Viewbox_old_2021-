// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-20
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using PdfGenerator;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.ValueMappings;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace eBalanceKitBusiness.Structures {
    public class CompanyUpgradeResults : NotifyPropertyChangedBase {

        private readonly List<CompanyUpgradeResult> _results = new List<CompanyUpgradeResult>();
        public IEnumerable<CompanyUpgradeResult> Results { get { return _results; } }

        internal readonly List<ValuesGCD_Company> DeletedValues = new List<ValuesGCD_Company>();
        internal readonly List<ValuesGCD_Company> UpdatedValues = new List<ValuesGCD_Company>();

        public bool HasResults() { return _results.Count > 0; }

        #region IsSelected
        private bool _isSelected;

        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion
        
        public CompanyUpgradeResult CreateResult(Company company) {
            var result = new CompanyUpgradeResult(company);
            _results.Add(result);
            return result;
        }

        public void SaveResults(string fileName) {
            Font font = new Font(Font.FontFamily.HELVETICA, 8);
            try {
                PdfTraits traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit");
                traits.MarginLeft = 20;
                traits.MarginRight = 20;
                PdfGenerator.PdfGenerator pdfDocument = new PdfGenerator.PdfGenerator(traits);
                var headerPhrase = new Phrase("eBilanz-Kit - Ergebnisse Taxonomie-Upgrade (Firmenstammdaten)", pdfDocument.Traits.fontH1);

                foreach (var result in Results) {
                    pdfDocument.AddHeadline(result.Company.Name);
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

                pdfDocument.WriteFile(fileName, headerPhrase);
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }

        public void ProcessChanges() { CompanyManager.Instance.ProcessUpdateChanges(this); }
    }
}