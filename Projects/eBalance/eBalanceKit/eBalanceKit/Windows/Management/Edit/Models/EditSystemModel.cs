// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;

namespace eBalanceKit.Windows.Management.Edit.Models {
    public class EditSystemModel {
        public EditSystemModel(Window owner, eBalanceKitBusiness.Structures.DbMapping.System system) {
            Owner = owner;
            OriginalSystem = system;
            System = system.CreateTempObject();
            System.Validate();
        }

        public Window Owner { get; set; }

        private eBalanceKitBusiness.Structures.DbMapping.System OriginalSystem { get; set; }
        public eBalanceKitBusiness.Structures.DbMapping.System System { get; private set; }

        public void SaveSystem() { OriginalSystem.UpdateValues(System); }
    }
}