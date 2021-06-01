// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace eBalanceKit.Windows.Management.Management.Models {
    public class CompanyManagementModel {
        public CompanyManagementModel(Window owner) { Owner = owner; }
        
        private Window Owner { get; set; }
    }
}