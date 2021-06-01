// -----------------------------------------------------------
// Created by Benjamin Held - 15.08.2011 12:57:31
// Copyright AvenDATA 2011
// -----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using AvdCommon.Rules;
using AvdCommon.Rules.ExecuteRules;
using DbAccess;
using DbAccess.Structures;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Models.Profile {
    public class PreviewData {
        public class Entry : INotifyPropertyChanged {
            #region events
            public event PropertyChangedEventHandler PropertyChanged;
            private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
            #endregion

            private object Value{get;set;}

            public Entry(object value, RuleSet rules) {
                this.Value = value;
                Rules = rules.ExecuteRules;
            }

            public override string ToString(){
                if (Value == null) return "DBNULL";
                if (Value is Byte[]) {
                    System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                    return enc.GetString(Value as Byte[]);
                }
                return Value.ToString();
            }
            public void UpdateRuleDisplayString() {
                RuleDisplayString = ToString();
                if (Rules != null) {
                    foreach (var rule in Rules) {
                        RuleDisplayString = rule.Execute(RuleDisplayString);
                    }
                }
                OnPropertyChanged("RuleDisplayString");
                //OnPropertyChanged("");
            }

            public string DisplayString { get { return ToString(); } }
            public string RuleDisplayString { get; set; }

            private ObservableCollection<ExecuteRule> _rules;
            public ObservableCollection<ExecuteRule> Rules {
                get { return _rules; }
                set {
                    if (_rules != value) {
                        _rules = value;
                        if(_rules != null)
                            _rules.CollectionChanged += _rules_CollectionChanged;
                        UpdateRuleDisplayString();

                    }
                }
            }

            void _rules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
                UpdateRuleDisplayString();
            }
        }

        public PreviewData() {
            this.Data = new List<List<Entry>>();
        }

        public List<List<Entry>> Data { get; private set; }

        public void Clear() {
            Data.Clear();
        }

    }

    public class DataPreviewModel : INotifyPropertyChanged{
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        public PreviewData DataValidation { get; set; }
        public PreviewData DataView { get; set; }
        public PreviewData[] Data { get; set; }
        private DbConfig[] DbConfigs { get; set; }

        private TableMapping _tableMapping;
        public TableMapping TableMapping {
            get { return _tableMapping; }
            set {
                if (_tableMapping != value) {
                    _tableMapping = value;
                    OnPropertyChanged("TableMapping");
                }
            }
        }

        //public TableMapping TableMapping { get; private set; }

        public DataPreviewModel(TableMapping tableMapping){
            Init(tableMapping);
        }

        public DataPreviewModel(){
            Init(null);
        }

        public void Init(TableMapping tableMapping){
            if (tableMapping == null) this.DbConfigs = new DbConfig[2] {null, null}; 
            else DbConfigs = new DbConfig[2] { tableMapping.TableValidation.DbConfig, tableMapping.TableView.DbConfig };
            this.TableMapping = tableMapping;
            this.DataValidation = new PreviewData();
            this.DataView = new PreviewData();
            this.Data = new PreviewData[] {DataValidation, DataView};
        }

        public string GetSQLStatement(int which, IDatabase conn, bool addLimit) {
            List<string> cols = new List<string>();

            foreach (var mapping in TableMapping.ColumnMappings)
                cols.Add(conn.Enquote(mapping.GetColumnName(which)));

            string sql;
            string sortSql = "";
            if (TableMapping.KeyEntryMappings.Count != 0) {
                List<string> sortCriteria = new List<string>();
                foreach (var sortMapping in TableMapping.KeyEntryMappings)
                    sortCriteria.Add(conn.Enquote(which == 0 ? sortMapping.Source : sortMapping.Destination));
                sortSql = " ORDER BY " + string.Join(",", sortCriteria);
            }

            if (which == 0) {
                string limitSql = (addLimit ? " TOP 15 " : "");
                sql = "SELECT " + limitSql + string.Join(",", cols) + " FROM " + conn.Enquote(TableMapping.TableValidation.Name) + " " + TableMapping.TableValidation.Filter.GetFilterSQL() + sortSql;
            } else {
                string limitSql = (addLimit ? " LIMIT 15 " : "");
                sql = "SELECT " + string.Join(",", cols) + " FROM " + conn.Enquote(TableMapping.TableView.Name) + " " + TableMapping.TableView.Filter.GetFilterSQL() + sortSql + limitSql;
            }
            return sql;
        }

        public virtual void FillData(bool addLimit) {
            if (DbConfigs[0] == null || DbConfigs[1] == null || TableMapping == null) return;

            PreviewData[] Data = new PreviewData[2] { DataValidation, DataView };

            for (int i = 0; i < 2; ++i) {
                Data[i].Clear();
                if (TableMapping.ColumnMappings.Count == 0) continue;
                using (IDatabase conn = ConnectionManager.CreateConnection(DbConfigs[i])) {
                    conn.Open();
                    if (!conn.TableExists(TableMapping.Tables[i].Name)) {
                        List<PreviewData.Entry> entries = new List<PreviewData.Entry>();
                        for (int entry = 0; entry < TableMapping.ColumnMappings.Count; ++entry)
                            entries.Add(new PreviewData.Entry(entry == 0 ? "Die Tabelle \"" + TableMapping.Tables[i].Name +  
                                "\" existiert nicht" : "", TableMapping.ColumnMappings[entry].GetColumn(i).Rules));
                        Data[i].Data.Add(entries);
                        continue;
                    }
                    //Read column types
                    List<DbColumnInfo> columnInfos = conn.GetColumnInfos(TableMapping.Tables[i].Name);
                    foreach (var columnMapping in TableMapping.ColumnMappings)
                        foreach (var columnInfo in columnInfos)
                            if (columnInfo.Name == columnMapping.GetColumnName(i)) {
                                columnMapping.GetColumn(i).Type = columnInfo.Type;
                                break;
                            }


                    string sql = GetSQLStatement(i, conn, addLimit);
                    try {
                        using (var reader = conn.ExecuteReader(sql)) {
                            while (reader.Read()) {
                                List<PreviewData.Entry> entries = new List<PreviewData.Entry>();
                                for (int entry = 0; entry < TableMapping.ColumnMappings.Count; ++entry) {
                                    if (entry >= reader.FieldCount) continue;
                                    RuleSet rules = TableMapping.ColumnMappings[entry].GetColumn(i).Rules;
                                    if (reader.IsDBNull(entry)) entries.Add(new PreviewData.Entry(null, rules));
                                    else entries.Add(new PreviewData.Entry(reader[entry], rules));
                                }
                                Data[i].Data.Add(entries);
                            }
                        }
                    } catch (Exception ex) {
                        throw new Exception(ex.Message + Environment.NewLine + "SQL Statement:" + Environment.NewLine + sql);
                    }
                }
            }
        }
    }
}
