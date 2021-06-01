// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-26
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Utils;

namespace eBalanceKit.Windows.Management.Management.Models {
    public class ReportTreeNode : IReportTreeItem {

        public ReportTreeNode(object item, string displayString) {
            Item = item;
            DisplayString = displayString;
        }

        #region Children
        private readonly ObservableCollectionAsync<IReportTreeItem> _children = new ObservableCollectionAsync<IReportTreeItem>();

        public ObservableCollectionAsync<IReportTreeItem> Children { get { return _children; } }
        #endregion // Children

        public ReportTreeNode Parent { get; set; }

        #region DisplayString
        private string _displayString;

        public string DisplayString {
            get { return _displayString; }
            set {
                if (_displayString == value) return;
                _displayString = value;
            }
        }

        public object Item { get; private set; }
        
        #endregion // DisplayString

        public override string ToString() { return DisplayString; }
    }
}