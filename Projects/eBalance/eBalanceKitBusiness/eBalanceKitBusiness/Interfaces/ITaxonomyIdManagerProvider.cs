// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-03
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.Interfaces {
    internal interface ITaxonomyIdManagerProvider {
        TaxonomyIdManager TaxonomyIdManager { get; }
    }
}