// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Utils;
using eBalanceKitBusiness.FederalGazette;
using eFederalGazette;

namespace eBalanceKit.Windows.FederalGazette.Model {
    public class DlgFederalGazetteManagerModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        public DlgFederalGazetteManagerModel(FederalGazetteModel model) { FederalGazetteModel = model; }

        private FederalGazetteModel FederalGazetteModel { get; set; }

        private List<ClientsList> _clientsLists = new List<ClientsList>();

        private ObservableCollectionAsync<ClientsList> _filteredClientList = new ObservableCollectionAsync<ClientsList>();
        public IEnumerable<ClientsList> ClientsLists { get { return _filteredClientList; } }


        private string _filter;

        public string Filter {
            get { return _filter; }
            set {
                _filter = value;
                
                ApplyFilter();
                OnPropertyChanged("Filter");
            }
        }

        private void ApplyFilter() {
            _filteredClientList.Clear();
            var filter = Filter == null ? null : Filter.ToLower();
            if (filter == null) {
                foreach (var clientsList in _clientsLists) {
                    _filteredClientList.Add(clientsList);
                }
                return;
            }

            foreach (var client in _clientsLists) {
                if ((client.ClientId.ToLower().Contains(filter) || client.CompanyName.ToLower().Contains(filter))) 
                    _filteredClientList.Add(client);
            }
            

        }

        public void GetClientsList() {
            var getClient = new FederalGazetteOperations(FederalGazetteModel);
            foreach (var client in getClient.GetClientList()) {
                _clientsLists.Add(client);
            }
            
            ApplyFilter();
            OnPropertyChanged("ClientsLists");
        }

        protected void OnPropertyChanged(string name) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

    }
}