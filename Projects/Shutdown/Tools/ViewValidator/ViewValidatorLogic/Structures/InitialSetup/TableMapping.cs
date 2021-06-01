// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 11:40:46
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AvdCommon.Rules;
using DbAccess;
using ViewValidatorLogic.Structures.InitialSetup.StoredProcedures;
using ViewValidatorLogic.Structures.Logic;

namespace ViewValidatorLogic.Structures.InitialSetup {
    public class TableMapping : INotifyPropertyChanged{
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
        #endregion

        #region Properties
        public ObservableCollection<ColumnMapping> ColumnMappings { get; set; }
        public ObservableCollection<ColumnMapping> KeyEntryMappings { get; set; }
        public RuleSet Rules { get; set; }

        #region Used
        private bool _used;
        public bool Used {
            get { return _used; }
            set {
                if (_used != value) {
                    _used = value; 
                    OnPropertyChanged("Used");
                }
            }
        }
        #endregion

        public Table[] Tables { get; set; }

        #region TableValidation
        private Table _tableValidation;
        public Table TableValidation {
            get { return _tableValidation; }
            set { _tableValidation = value; Tables[0] = value; }
        }
        #endregion

        #region TableView
        private Table _tableView;
        public Table TableView {
            get { return _tableView; }
            set { _tableView = value; Tables[1] = value;  }
        }
        #endregion

        public string UniqueName { get { return TableValidation.Name + "???" + TableView.Name; } }
        public string DisplayString { get { return TableValidation.Name + " - " + TableView.Name; } }
        public StoredProcedure StoredProcedure { get; set; }
        #endregion Properties

        #region Constructor
        public TableMapping() {
            this.ColumnMappings = new ObservableCollection<ColumnMapping>();
            this.Tables = new Table[2] { TableValidation, TableView };
            this.Used = true;
            this.Rules = new RuleSet();
            this.KeyEntryMappings = new ObservableCollection<ColumnMapping>();
            this.StoredProcedure = new StoredProcedure();
        }
        #endregion

        #region Methods
        internal string GetColumns(IDatabase conn, int which) {
            string result = "";
            foreach (var mapping in ColumnMappings) {
                result += conn.Enquote(mapping.GetColumnName(which)) + ",";
            }
            //Delete last comma
            result = result.Substring(0, result.Length - 1);
            return result;
        }

        //return a list of column infos in the same order as the column mappings
        internal List<ColumnInfoHelper> GetColumnInfosFromMapping(int which) {
            List<ColumnInfoHelper> result = new List<ColumnInfoHelper>();

            List<DbAccess.Structures.DbColumnInfo> columnInfos = Tables[which].ColumnInfos;
            foreach (var columnMapping in ColumnMappings) {
                bool found = false;
                for(int i = 0; i < columnInfos.Count; ++i){
                    if (columnMapping.GetColumnName(which).ToLower() == columnInfos[i].Name.ToLower()) {
                        found = true;
                        result.Add(ColumnInfoHelper.FromDbColumnInfo(columnInfos[i],i));
                        break;
                    }
                }
                if (!found) throw new Exception("Could not find column " + columnMapping.GetColumnName(which));
            }

            return result;
        }
        #endregion
    }
}
