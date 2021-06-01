using System;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.Interfaces.PresentationTree {

    /// <summary>
    /// Extension of Taxonomy.IPresentationTreeNode, to provide additional members,
    /// which are needed when the tree data should be displayed in the GUI.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-25</since>
    public interface IPresentationTreeNode : Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode, ITaxonomyTreeNode, IsSelectable {


        Document Document { get; }
        IValueTreeEntry Value { get; set; }
        decimal DecimalValue { get; }

        /// <summary>
        /// True, iif. a balance list entry (account, account group or splitted account) could be assigned to the 
        /// node or if the node values (manual correction value, transfer values etc.) could be changed.
        /// </summary>
        bool IsValueEditingAllowed { get; }

        bool ValidationError { get; }
        bool ValidationWarning { get; }

        bool IsExpanded { get; set; }
        bool IsVisible { get; set; }



        void ExpandAllChildren();
        void CollapseAllChildren();

        /// <summary>
        /// Unselects all items.
        /// </summary>
        /// <param name="selectedItem">Item which should stay selected.</param>
        void UnselectAll(object selectedItem = null);
    }
}
