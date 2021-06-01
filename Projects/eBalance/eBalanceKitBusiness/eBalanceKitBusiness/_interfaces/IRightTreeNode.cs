using System.ComponentModel;
using System.Collections.ObjectModel;
namespace eBalanceKitBusiness {

    /// <summary>
    /// Interface for RoleRightTreeNode and EffectiveRightTreeNode.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-08-25</since>
    public interface IRightTreeNode {

        event PropertyChangedEventHandler PropertyChanged;
        ObservableCollection<IRightTreeNode> Children { get; }
        string HeaderString { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        bool? ReadChecked { get; set; }
        bool? WriteChecked { get; set; }
        bool? SendChecked { get; set; }
        bool? ExportChecked { get; set; }
        bool? GrantChecked { get; set; }
        bool InheritResultGrant { get; }
        bool InheritResultWrite { get; }
        bool InheritResultRead { get; }
        bool InheritResultExport { get; }
        bool InheritResultSend { get; }
        bool IsEditAllowed { get; }
        bool IsSpecialRight { get; }
    }
}
