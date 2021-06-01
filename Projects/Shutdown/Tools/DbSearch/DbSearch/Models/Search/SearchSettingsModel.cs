using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using DbSearchLogic.Structures.TableRelated;
using Utils;
using Utils.Commands;

namespace DbSearch.Models.Search {
    public class SearchSettingsModel : NotifyPropertyChangedBase{
        #region Constructor
        public SearchSettingsModel(IEnumerable<Query> allQueries) {
            Queries = allQueries;

            TransferSettingsFromQueryCommand = new DelegateCommand(o => true, TransferSettingsFromQuery);
        }

        #endregion Constructor

        #region Properties
        #region WhitelistTables
        private string _whitelistTables;

        public string WhitelistTables {
            get { return _whitelistTables; }
            set {
                if (_whitelistTables != value) {
                    _whitelistTables = value;
                    OnPropertyChanged("WhitelistTables");
                }
            }
        }
        #endregion WhitelistTables

        #region BlacklistTables
        private string _blacklistTables;

        public string BlacklistTables {
            get { return _blacklistTables; }
            set {
                if (_blacklistTables != value) {
                    _blacklistTables = value;
                    OnPropertyChanged("BlacklistTables");
                }
            }
        }
        #endregion BlacklistTables

        public bool Accept { get; set; }
        public IEnumerable<Query> Queries { get; private set; }

        #region SelectedQuery
        private Query _selectedQuery;

        public Query SelectedQuery {
            get { return _selectedQuery; }
            set {
                if (_selectedQuery != value) {
                    _selectedQuery = value;
                    OnPropertyChanged("SelectedQuery");
                }
            }
        }
        #endregion SelectedQuery

        public ICommand TransferSettingsFromQueryCommand { get; set; }
        #endregion Properties

        #region Methods
        public void AcceptButton() {
            Accept = true;
        }

        private void TransferSettingsFromQuery(object o) {
            if (SelectedQuery == null)
                return;
            SelectedQuery.Load();
            WhitelistTables = SelectedQuery.SearchTableDecider.WhitelistTablesAsString();
            BlacklistTables = SelectedQuery.SearchTableDecider.BlacklistTablesAsString();
        }
        #endregion Methods

    }
}
