// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-26
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Management.Management.Models {
    public class ReportTreeLeaf : IReportTreeItem {
        public ReportTreeLeaf(Document document) { Document = document; }
        
        public Document Document { get; private set; }
        public ReportTreeNode Parent { get; set; }

        public override string ToString() { return Document.Name; }
    }
}