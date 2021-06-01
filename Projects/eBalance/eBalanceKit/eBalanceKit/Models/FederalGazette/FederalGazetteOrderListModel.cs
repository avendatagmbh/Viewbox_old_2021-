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
    public class FederalGazetteOrderListModel {
        public FederalGazetteOrderListModel(FederalGazetteOrderManager manager) {
            _manager = manager;
            //CmdListOrders = new DelegateCommand(o => true, o => _manager.LoadOrders());
            _manager.LoadOrders();
            CmdRefreshStatus = new DelegateCommand(o => true, delegate(object o) {
                System.Diagnostics.Debug.Assert(o is Order);
                _manager.RequestStatus(o as Order);
            });
        }

        private FederalGazetteOrderManager _manager;
        public IEnumerable<Order> Orders { get { return _manager.Orders; } }

        public DelegateCommand CmdListOrders { get; set; }
        public DelegateCommand CmdShowOrderDatails { get; set; }
        public DelegateCommand CmdRefreshStatus { get; set; }

    }
}