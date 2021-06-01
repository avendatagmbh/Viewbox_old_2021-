using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DbAccess.Structures;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Models.Profile {
    public class MappingChangedEventArgs : EventArgs {
        public MappingChangedEventArgs(string s, bool removed, ColumnMapping mapping) {
            msg = s;
            this.Mapping = mapping;
            this.Removed = removed;
        }
        private string msg;
        public string Message {
            get { return msg; }
        }
        public ColumnMapping Mapping { get; set; }
        public bool Removed { get; set; }
    }

    public class ColumnMappingModel : INotifyPropertyChanged {
        #region Constructor
        public ColumnMappingModel(TableMapping tableMapping, DbConfig dbConfigView) {
            this.TableMapping = tableMapping;
            this.FilterValidation = new Filter("");
            this.FilterView = new Filter("");
            this.Mapping = tableMapping.ColumnMappings;
            this.Sort = tableMapping.KeyEntryMappings;
            StoredProcedureModel = new StoredProcedureModel(tableMapping.StoredProcedure, dbConfigView);
        }
        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }

        public event EventHandler<MappingChangedEventArgs> MappingChangedEvent;
        protected virtual void OnMappingChanged(ColumnMapping mapping, bool removed) {
            EventHandler<MappingChangedEventArgs> handler = MappingChangedEvent;
            if (handler != null)
                handler(this, new MappingChangedEventArgs("", removed, mapping));
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
        private ObservableCollection<ColumnMapping> _mapping = new ObservableCollection<ColumnMapping>();
        public ObservableCollection<ColumnMapping> Mapping {
            get { return _mapping; }
            set { _mapping = value; }
        }
        #endregion

        #region Sort
        private ObservableCollection<ColumnMapping> _sort = new ObservableCollection<ColumnMapping>();
        public ObservableCollection<ColumnMapping> Sort {
            get { return _sort; }
            private set { _sort = value; }
        }
        #endregion
        public Filter FilterValidation { get; set; }
        public Filter FilterView { get; set; }

        public TableMapping TableMapping { get; private set; }
        public StoredProcedureModel StoredProcedureModel { get; set; }
        #endregion

        #region Methods
        public bool ContainsMapping(ColumnMapping mapping) {
            foreach (var curMapping in Mapping)
                if (curMapping.Source == mapping.Source && curMapping.Destination == mapping.Destination)
                    return true;
            return false;
        }

        public void AddMapping(ColumnMapping mapping) {
            Mapping.Add(mapping);
            ObsSource.Remove(mapping.Source);
            ObsDestination.Remove(mapping.Destination);
            OnMappingChanged(mapping, false);
        }

        public virtual void RemoveMapping(ColumnMapping mapping) {
            Mapping.Remove(mapping);
            if (!ObsSource.Contains(mapping.Source)) ObsSource.Add(mapping.Source);
            if (!ObsDestination.Contains(mapping.Destination)) ObsDestination.Add(mapping.Destination);
            OnMappingChanged(mapping, true);
            Sort.Remove(mapping);
        }

        public void AutomaticAssign() {
            List<ColumnMapping> lMaps = new List<ColumnMapping>();
            foreach (string colSource in ObsSource) {
                foreach (string colDestination in ObsDestination) {
                    if (colSource.ToLower().Equals(colDestination.ToLower()) ||
                        colSource.ToLower().Replace("_", string.Empty).Equals(colDestination.ToLower()) ||
                        colSource.ToLower().Equals(colDestination.ToLower().Replace("_", string.Empty)) ||
                        colSource.ToLower().Replace("_", string.Empty).Equals(colDestination.ToLower().Replace("_", string.Empty)) ||
                        colSource.ToLower().Replace("-", string.Empty).Equals(colDestination.ToLower()) ||
                        colSource.ToLower().Equals(colDestination.ToLower().Replace("-", string.Empty)) ||
                        colSource.ToLower().Replace("-", string.Empty).Equals(colDestination.ToLower().Replace("-", string.Empty))) {
                        lMaps.Add(new ColumnMapping(colSource, colDestination ));
                    }
                }
            }

            foreach (ColumnMapping map in lMaps) {
                ObsSource.Remove(map.Source);
                ObsDestination.Remove(map.Destination);
                Mapping.Add(map);
            }
        }

        internal void UseMapping() {
            foreach (ColumnMapping map in Mapping) {
                ObsSource.Remove(map.Source);
                ObsDestination.Remove(map.Destination);
            }
        }

        internal void AddObsDestination(string table) {
            //Only add table if destination is not already in mapping
            foreach (var mapping in Mapping)
                if (mapping.Destination == table) return;
            ObsDestination.Add(table);
        }

        internal void AddObsSource(string table) {
            //Only add table if source is not already in mapping
            foreach (var mapping in Mapping)
                if (mapping.Source == table) return;
            ObsSource.Add(table);
        }

        public bool HasSortMapping(ColumnMapping mapping) {
            foreach (var sortMapping in Sort)
                if (sortMapping.Source.ToLower() == mapping.Source.ToLower() && sortMapping.Destination.ToLower() == mapping.Destination.ToLower())
                    return true;
            return false;
        }


        #endregion Methods
    }
}
