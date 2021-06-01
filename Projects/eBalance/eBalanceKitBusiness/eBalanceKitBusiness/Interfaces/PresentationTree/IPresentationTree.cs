// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures.DbMapping;
namespace eBalanceKitBusiness {
    /// <summary>
    /// Extension of Taxonomy.IPresentationTree, to provide additional members,
    /// which are needed when the tree data should be displayed in the GUI.
    /// </summary>
    public interface IPresentationTree : Taxonomy.Interfaces.PresentationTree.IPresentationTree, ITaxonomyTree {
        Document Document { get; set; }

        bool ValidationError { get; }
        bool ValidationWarning { get; }

        bool ShowTransferValues { get; set; }
        bool ContainsHiddenValue { get; }

        void ClearAssignedItems();

        void UnselectAll(object selectedItem = null);
        void ExpandSelected(IPresentationTreeNode node);

        /// <summary>
        /// Gets the presentation tree node with the specified internal id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IPresentationTreeNode GetNode(int id);
    }
}