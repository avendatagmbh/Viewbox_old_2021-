// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-12-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using Utils.Commands;
using federalGazetteBusiness.Structures;
using federalGazetteBusiness.Structures.Manager;

namespace eBalanceKit.Models.FederalGazette {
    public class FederalGazetteOrderManagerModel {
        public FederalGazetteOrderManagerModel(System.Windows.Window owner, FederalGazetteOrderManager manager = null, string company = null) {
            _owner = owner;
            _manager = manager ?? new FederalGazetteOrderManager();

        }


        private System.Windows.Window _owner;

        private FederalGazetteOrderManager _manager;



        public DelegateCommand CmdGetOrderStatus { get; set; }
        public DelegateCommand CmdClose { get; set; }
    }
}