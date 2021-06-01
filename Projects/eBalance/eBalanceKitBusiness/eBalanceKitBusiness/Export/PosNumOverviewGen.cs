using System;
using System.Collections.Generic;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness.Structures.ValueTree;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfGenerator;
using Taxonomy.PresentationTree;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using System.Linq;

namespace eBalanceKitBusiness.Export {

    /// <summary>
    /// 
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06.19</since>
    public class PosNumOverviewGen {

        private Font font = new Font(Font.FontFamily.HELVETICA, 8);
        private Font fontItalic = new Font(Font.FontFamily.HELVETICA, 8, Font.ITALIC);
        private PdfGenerator.PdfGenerator pdf;

        public void Generate(string filename) {

            try {
                PdfTraits traits = new PdfTraits("eBilanz-Kit", "eBilanz-Kit") {
                    MarginLeft = 30, 
                    MarginRight = 30, 
                    MarginBottom = 20, 
                    xfont = new Font(Font.FontFamily.HELVETICA, 9)
                };
                pdf = new PdfGenerator.PdfGenerator(traits);

               GenerateContent();

                Phrase headerPhrase = new Phrase("eBilanz-Kit - Übersicht Positionsnummern");
                pdf.WriteFile(filename, headerPhrase);

            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }

        private void GenerateContent() {
            //try {
            //    GCDTaxonomyIdManager tm = GCDTaxonomyIdManager.Instance;

            //    List<TaxonomyIdAssignment> taxonomyIdAssignments = new List<TaxonomyIdAssignment>(tm.GetTaxonomyIdAssignments());
            //    var valuesByNumber = from item in taxonomyIdAssignments
            //                         orderby item.Number
            //                         select item;

            //    List<TaxonomyIdAssignment> gcd_document = new List<TaxonomyIdAssignment>();
            //    List<TaxonomyIdAssignment> gcd_report = new List<TaxonomyIdAssignment>();
            //    List<TaxonomyIdAssignment> gcd_company = new List<TaxonomyIdAssignment>();
            //    List<TaxonomyIdAssignment> gcd_other = new List<TaxonomyIdAssignment>();
                
            //    List<TaxonomyIdAssignment> gaap_bs_ass = new List<TaxonomyIdAssignment>();
            //    List<TaxonomyIdAssignment> gaap_bs_eqLiab = new List<TaxonomyIdAssignment>();
            //    List<TaxonomyIdAssignment> gaap_is = new List<TaxonomyIdAssignment>();
            //    List<TaxonomyIdAssignment> gaap_other = new List<TaxonomyIdAssignment>();

            //    foreach (var item in valuesByNumber) {
            //        // gcd taxonomy
            //        if (item.XbrlElementId.StartsWith("de-gcd")) {
            //            if (item.XbrlElementId.StartsWith("de-gcd_genInfo.doc")) gcd_document.Add(item);
            //            else if (item.XbrlElementId.StartsWith("de-gcd_genInfo.doc")) gcd_document.Add(item);
            //            else if (item.XbrlElementId.StartsWith("de-gcd_genInfo.report")) gcd_report.Add(item);
            //            else if (item.XbrlElementId.StartsWith("de-gcd_genInfo.company")) gcd_company.Add(item);
            //            else { gcd_other.Add(item); }

            //            // gaap taxonomy
            //        } else if (item.XbrlElementId.StartsWith("de-gaap-ci")) {
            //            if (item.XbrlElementId.StartsWith("de-gaap-ci_bs.ass")) gaap_bs_ass.Add(item);
            //            else if (item.XbrlElementId.StartsWith("de-gaap-ci_bs.eqLiab")) gaap_bs_eqLiab.Add(item);
            //            else if (item.XbrlElementId.StartsWith("de-gaap-ci_is")) gaap_is.Add(item);
            //            else { gaap_other.Add(item); }
            //        }
            //    }

            //    pdf.AddHeadline("GCD Taxonomie");
            //    AddSupChapter("Dokumentinformationen", gcd_document);
            //    AddSupChapter("Reportinformationen", gcd_report);
            //    AddSupChapter("Firmenstammdaten", gcd_company);
            //    AddSupChapter("Sonstige", gcd_other);

            //    pdf.AddHeadline("GAAP Taxonomie");
            //    AddSupChapter("Bilanz (Aktiva)", gaap_bs_ass);
            //    AddSupChapter("Bilanz (Passiva)", gaap_bs_eqLiab);
            //    AddSupChapter("Gewinn- und Verlustrechnung", gaap_is);
            //    AddSupChapter("Sonstige", gaap_other);

            //} catch (Exception ex) {
            //    System.Diagnostics.Debug.WriteLine(ex.Message);
            //}
        }

        private void AddSupChapter(string name, List<TaxonomyIdAssignment> items) {
            //GCDTaxonomyIdManager tm = GCDTaxonomyIdManager.Instance;
            //pdf.AddSubHeadline(name);
            //PdfPTable table = new PdfPTable(2);
            //table.TotalWidth = 1;
            //table.SetWidths(new float[] { 0.06f, 0.94f });
            //table.DefaultCell.Border = PdfPCell.NO_BORDER;
            //table.WidthPercentage = 100f;
            //pdf.CurrentChapter.Add(table);

            //foreach (var item in items) {
            //    var elem = tm.GetElement(item.Id);
            //    table.AddCell(new Phrase(item.Number, pdf.Traits.xfont));
            //    table.AddCell(new Phrase(elem.Id + " (" + elem.Label + ")", pdf.Traits.xfont));
            //}
        }

    }
}
