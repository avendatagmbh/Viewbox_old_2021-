// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Structures;

namespace eBalanceKitBusiness.Interfaces {
    public interface ITaxonomyTreeNode {
        void UpdateVisibility(PresentationTreeFilter filter, bool updateIsExpanded = false);
        void ResetFilter();
         
    }
}