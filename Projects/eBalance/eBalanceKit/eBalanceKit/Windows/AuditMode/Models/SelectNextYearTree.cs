// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-23
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.AuditMode.Models {
    public class SelectNextYearTree {
        public SelectNextYearTree() {
            RootNodes = new List<FinancialYearNode>();

            var document = DocumentManager.Instance.CurrentDocument;
            while (document != null) {
                var nextYearDocuments = DocumentManager.Instance.GetFollowingYearReports(document).ToList();
                // We need to get the next FinancialYear and create a node for it
                var fyear = new FinancialYearNode(GetNextFinancialYear(document));

                foreach (var doc in nextYearDocuments) {
                    var rights = new ReportRights(doc);
                    if (!rights.WriteRestAllowed && !rights.WriteTransferValuesAllowed) continue;
                    fyear.Children.Add(new DocumentNode(doc, fyear, rights));
                }

                if (fyear.Children.Any())
                    RootNodes.Add(fyear);
                
                document = nextYearDocuments.FirstOrDefault();
            }
        }

        public List<FinancialYearNode> RootNodes { get; private set; }

        public List<Document> GetCheckedDocuments() { return RootNodes.SelectMany(fyear => fyear.Children.Where(d => d.IsChecked).Select(d => d.Document)).ToList(); }

        public int CheckedDocumentsCount { get { return GetCheckedDocuments().Count; } }

        /// <summary>
        /// Returns the next financial year or null if none existing.
        /// </summary>
        /// <param name="document">The report from the reference year.</param>
        /// <returns>FinancialYear that followes the FinancialYear of the document (parameter).</returns>
        public FinancialYear GetNextFinancialYear(Document document) {
            return
                document.Company.FinancialYears.FirstOrDefault(
                    finYear => finYear.FYear == document.FinancialYear.FYear + 1);
        }
    }
}