using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using DbAccess;
using DbAccess.Structures;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Models.Profile {
    public class TableMappingModel : INotifyPropertyChanged {
        #region TableMappingChangedEventArgs
        public class TableMappingChangedEventArgs : EventArgs {
            public TableMappingChangedEventArgs(string s, bool removed, TableMapping mapping) {
                _msg = s;
                this.Mapping = mapping;
                this.Removed = removed;
            }
            private string _msg;
            public string Message {
                get { return _msg; }
            }
            public TableMapping Mapping { get; set; }
            public bool Removed { get; set; }
        }
        #endregion TableMappingChangedEventArgs

        #region Constructor
        public TableMappingModel(ObservableCollection<TableMapping> tableMappings, DbConfig dbConfigView, DbConfig lastDbConfigValidation) {
            Mapping = tableMappings;
            DbConfigView = dbConfigView;
            SelectedValidationConfig = (DbConfig) lastDbConfigValidation.Clone();
        }
        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }

        public event EventHandler<TableMappingChangedEventArgs> TableMappingChangedEvent;
        protected virtual void TableOnMappingChanged(TableMapping mapping, bool removed) {
            EventHandler<TableMappingChangedEventArgs> handler = TableMappingChangedEvent;
            if (handler != null)
                handler(this, new TableMappingChangedEventArgs("", removed, mapping));
        }
        #endregion

        #region Properties
        #region ObsSource
        private ObservableCollection<string> _obsSource = new ObservableCollection<string>();
        public ObservableCollection<string> ObsSource {
            get { return _obsSource; }
            set { _obsSource = value; }
        }
        #endregion

        #region ObsDestination
        private ObservableCollection<string> _obsDestination = new ObservableCollection<string>();
        public ObservableCollection<string> ObsDestination {
            get { return _obsDestination; }
            set { _obsDestination = value; }
        }
        #endregion

        #region Mapping
        private ObservableCollection<TableMapping> _mapping = new ObservableCollection<TableMapping>();
        public ObservableCollection<TableMapping> Mapping {
            get { return _mapping; }
            set { _mapping = value; }
        }
        #endregion

        public DbConfig DbConfigView { get; private set; }

        public int CurrentPage {
            get { return _currentPage; }
            set {
                if (_currentPage != value) {
                    _currentPage = value;

                    if (_currentPage == 2) {
                        //When switching to the validation table page, we have to load the tables of the database
                        //If an error occurs then no database has been selected in a prior run
                        try {
                            ReloadValidationTables();
                        } catch (Exception) {
                            
                        }
                    }

                    OnPropertyChanged("CurrentPage");
                    OnPropertyChanged("IsFirstPage");
                    OnPropertyChanged("IsLastPage");
                    OnPropertyChanged("Page1Visibility");
                    OnPropertyChanged("Page2Visibility");
                }
            }
        }
        private int _currentPage = 1;

        public bool IsFirstPage { get { return this.CurrentPage == 1; } }
        public bool IsLastPage { get { return this.CurrentPage == this.PageCount; } }

        public int PageCount {
            get { return 2; }
        }

        public Visibility Page1Visibility { get { return CurrentPage == 1 ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility Page2Visibility { get { return CurrentPage == 2 ? Visibility.Visible : Visibility.Collapsed; } }

        #region SelectedTableView
        private string _selectedTableView;
        public string SelectedTableView {
            get { return _selectedTableView; }
            set {
                if (_selectedTableView != value) {
                    _selectedTableView = value;
                    OnPropertyChanged("SelectedTableView");
                }
            }
        }
        #endregion SelectedTableView

        #region SelectedTableValidation
        private string _selectedTableValidation;
        public string SelectedTableValidation {
            get { return _selectedTableValidation; }
            set {
                if (_selectedTableValidation != value) {
                    _selectedTableValidation = value;
                    OnPropertyChanged("SelectedTableValidation");
                }
            }
        }

        public DbConfig SelectedValidationConfig { get; set; }

        #endregion

        #endregion

        #region Methods
        public bool ContainsMapping(TableMapping mapping){
            return Mapping.Contains(mapping);
            //foreach (var curMapping in Mapping)
            //    if (curMapping.Source == mapping.Source && curMapping.Destination == mapping.Destination)
            //        return true;
            //return false;
        }

        public void AddMapping(TableMapping mapping) {
            Mapping.Add(mapping);
            ObsSource.Remove(mapping.TableValidation.Name);
            ObsDestination.Remove(mapping.TableView.Name);
            TableOnMappingChanged(mapping, false);
        }

        public virtual void RemoveMapping(TableMapping mapping) {
            Mapping.Remove(mapping);
            if (!ObsSource.Contains(mapping.TableValidation.Name)) ObsSource.Add(mapping.TableValidation.Name);
            if (!ObsDestination.Contains(mapping.TableView.Name)) ObsDestination.Add(mapping.TableView.Name);
            TableOnMappingChanged(mapping, true);
            
        }

        internal void AddObsDestination(string table) {
            //Only add table if destination is not already in mapping
            foreach (var mapping in Mapping)
                if (mapping.TableView.Name == table) return;
            ObsDestination.Add(table);
        }

        internal void AddObsSource(string table) {
            //Only add table if source is not already in mapping
            foreach (var mapping in Mapping)
                if (mapping.TableValidation.Name == table) return;
            ObsSource.Add(table);
        }

        internal void Clear() {
            CurrentPage = 1;
            SelectedTableView = string.Empty;
            SelectedTableValidation = string.Empty;
        }
        #endregion Methods

        public void ReloadValidationTables() {
            using (IDatabase access = ConnectionManager.CreateConnection(SelectedValidationConfig)) {
                access.Open();
                ObsSource.Clear();
                foreach (string table in access.GetTableList()) AddObsSource(table);
            }

        }
    }
}
