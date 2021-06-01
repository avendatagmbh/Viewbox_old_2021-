// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures;
namespace eBalanceKitBusiness {
    /// <summary>
    /// Extension of Taxonomy.IPresentationTree, to provide additional members,
    /// which are needed when the tree data should be displayed in the GUI.
    /// </summary>
    public interface IPresentationTree : Taxonomy.Interfaces.PresentationTree.IPresentationTree {
        Document Document { get; set; }

        PresentationTreeFilter Filter { get; set; }

        bool ValidationError { get; }
        bool ValidationWarning { get; }

        bool ShowTransferValues { get; set; }

        void ClearAssignedItems();

        void ExpandAllNodes();
        void CollapseAllNodes();
        void ResetFilter();

        void UnselectAll();
        void ExpandSelected(IPresentationTreeNode node);
    }
}