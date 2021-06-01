// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-20
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy.Interfaces;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.EventArgs {
    public class AssignedTaxonomyInfoChangedEventArgs : System.EventArgs {
        internal AssignedTaxonomyInfoChangedEventArgs(ITaxonomyInfo oldValue, ITaxonomyInfo newValue) {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public ITaxonomyInfo OldValue { get; private set; }
        public ITaxonomyInfo NewValue { get; private set; }
    }
}