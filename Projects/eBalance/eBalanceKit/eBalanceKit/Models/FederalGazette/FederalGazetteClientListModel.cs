// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-11-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitResources.Localisation;
using federalGazetteBusiness.Structures.Manager;

namespace eBalanceKit.Models.FederalGazette {

    public enum SortDirection {  Original, Name, Id }

    public class FederalGazetteClientListModel : Utils.NotifyPropertyChangedBase {
        public FederalGazetteClientListModel(System.Windows.Window owner, FederalGazetteClientManager manager = null, string company = null) {

            if (manager == null) {
                manager = new FederalGazetteClientManager(eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument);
            }

            CompaniesListed = new Utils.ObservableCollectionAsync<KeyValuePair<string, string>>();
            
            Owner = owner;
            _manager = manager;
            Company = company;
            CmdGetCompanies = new Utils.Commands.DelegateCommand(o => true, delegate(object o) {
                
                var progress = new DlgProgress((o as System.Windows.Window) ?? Owner) {
                    ProgressInfo = {
                        IsIndeterminate = true,
                        Caption = ResourcesFederalGazette.ProgressCaptionLoading
                    }
                };
                progress.ExecuteModal(QueryCompanies);

                if (!string.IsNullOrEmpty(_lastError) && !QueriedCompanies.Any()) {
                    System.Windows.MessageBox.Show(Owner, _lastError, eBalanceKitResources.Localisation.ResourcesCommon.Warning,
                                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            } );

            CmdClose = new Utils.Commands.DelegateCommand(o => true, delegate(object o) {
                try {
                    bool result;
                    if (o != null && bool.TryParse(o.ToString(), out result)) {
                        Owner.DialogResult = result;
                    }
                    Owner.Close();
                } catch (Exception e) {
                    eBalanceKitBase.Structures.ExceptionLogging.LogException(e);
                }
            });

            CmdGetCompanies.Execute(Owner.Owner);
            if (QueriedCompanies.Count == 1) {
                SelectedEntry = CompaniesListed.First();
                CmdClose.Execute(true);
            }
        }

        private System.Windows.Window Owner;

        private FederalGazetteClientManager _manager;

        #region SelectedEntry
        private KeyValuePair<string, string> _selectedEntry;

        public KeyValuePair<string, string> SelectedEntry {
            get { return _selectedEntry; }
            set {
                _selectedEntry = value;
                OnPropertyChanged("SelectedEntry");
            }
        }
        #endregion // SelectedEntry

        private void QueryCompanies() {
            
                try {
                    QueriedCompanies = _manager.GetSenderClients(Company);
                } catch (ExceptionBase e) {
                    //Console.WriteLine(e);
                    LastError = e.Message;
                } catch (Exception e) {
                    //Console.WriteLine(e);
                    LastError = e.Message;
                }
            ApplyFilter();
        }

        #region LastError
        private string _lastError;

        public string LastError {
            get { return _lastError; }
            set {
                if (_lastError != value) {
                    _lastError = value;
                    OnPropertyChanged("LastError");
                }
            }
        }
        #endregion // LastError

        #region Company
        private string _company;

        public string Company {
            get { return _company; }
            set {
                if (_company != value) {
                    _company = value;
                    OnPropertyChanged("Company");
                }
            }
        }
        #endregion // Company

        #region QueriedCompanies
        private Dictionary<string,string> _queriedCompanies;
        /// <summary>
        /// Dictionary that contains all results that where returned by <see cref="federalGazetteBusiness.FederalGazetteManager.GetSenderClients"/>.
        /// </summary>
        public Dictionary<string,string> QueriedCompanies {
            get { return _queriedCompanies; }
            set {
                if (_queriedCompanies != value) {
                    _queriedCompanies = value;
                    OnPropertyChanged("QueriedCompanies");
                }
            }
        }
        #endregion // QueriedCompanies

        #region CompaniesListed
        private Utils.ObservableCollectionAsync<KeyValuePair<string,string>> _companiesListed;

        public Utils.ObservableCollectionAsync<KeyValuePair<string,string>> CompaniesListed {
            get { return _companiesListed; }
            set {
                if (_companiesListed != value) {
                    _companiesListed = value;
                    OnPropertyChanged("CompaniesListed");
                }
            }
        }
        #endregion // CompaniesListed


        #region SortDirection
        private SortDirection _sortDirection;

        public SortDirection SortDirection {
            get { return _sortDirection; }
            set {
                _sortDirection = value;
                ApplyFilter();
            }
        }
        #endregion // SortDirection

        #region Filter
        private string _filter;

        public string Filter {
            get { return _filter; }
            set {
                _filter = value;
                ApplyFilter();
            }
        }
        #endregion // Filter

        #region ApplyFilter
        /// <summary>
        /// Applies the filter.
        /// </summary>
        private void ApplyFilter() {
            if (!QueriedCompanies.Any()) {
                return;
            }
            var filter = (Filter == null ? string.Empty : Filter.ToLower());
            _companiesListed.Clear();

            //Dictionary<string,string> res = new Dictionary<string, string>();
            IEnumerable<KeyValuePair<string, string>> res = new Dictionary<string, string>();
            //CompaniesListed.Clear();

            switch (SortDirection) {
                case SortDirection.Original:
                    res = QueriedCompanies.Where(c => c.Value.Contains(filter));
                    break;
                case SortDirection.Name:
                    res = QueriedCompanies.Where(c => c.Value.Contains(filter)).OrderBy(c => c.Value);
                    break;
                case SortDirection.Id:
                    res = QueriedCompanies.Where(c => c.Value.Contains(filter)).OrderBy(c => c.Key);
                    break;
            }

            foreach (var entry in res) {
                CompaniesListed.Add(entry);
            }
        }
        #endregion ApplyFilter



        public Utils.Commands.DelegateCommand CmdGetCompanies { get; set; }
        public Utils.Commands.DelegateCommand CmdClose { get; set; }
    }
}