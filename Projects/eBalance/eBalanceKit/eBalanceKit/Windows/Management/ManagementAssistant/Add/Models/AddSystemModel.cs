// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Add.Models {
    public class AddSystemModel {
        public AddSystemModel(Window owner) {
            Owner = owner;
            System = new eBalanceKitBusiness.Structures.DbMapping.System();
            System.Validate();
        }

        public Window Owner { get; set; }

        public eBalanceKitBusiness.Structures.DbMapping.System System { get; private set; }

        public void SaveSystem() { SystemManager.Instance.AddSystem(System); }
    }
}