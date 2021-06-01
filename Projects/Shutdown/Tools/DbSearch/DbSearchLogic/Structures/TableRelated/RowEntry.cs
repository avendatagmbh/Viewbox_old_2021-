using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using AvdCommon.Rules;
using DbSearchDatabase.Interfaces;

namespace DbSearchLogic.Structures.TableRelated {
    public enum RowEntryStatus {
        Used,
        NullOrEmpty,
        Duplicate,
    }

    public class RowEntry : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        #region Constructor
        public RowEntry(IDbRowEntry dbRowEntry, RuleSet rules) {
            _rules = rules;
            DbRowEntry = dbRowEntry;
            SetRuleDisplayString(rules);
            rules.ExecuteRules.CollectionChanged += ExecuteRules_CollectionChanged;
        }

        void ExecuteRules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            SetRuleDisplayString(_rules);
        }

        ~RowEntry() {
            _rules.ExecuteRules.CollectionChanged -= ExecuteRules_CollectionChanged;
        }

        #endregion

        #region Properties
        private RuleSet _rules;
        internal IDbRowEntry DbRowEntry { get; set; }
        public string DisplayString {
            get {
                string result;

                if (DbRowEntry.Value == null) result = "DBNULL";
                else result = DbRowEntry.Value.ToString();

                if (EditedValue != result && !string.IsNullOrEmpty(EditedValue)) return EditedValue;
                return result;
            }
        }

        #region RuleDisplayString
        private string _ruleDisplayString;
        public string RuleDisplayString {
            get { return _ruleDisplayString; }
            set {
                if (_ruleDisplayString != value) {
                    _ruleDisplayString = value;
                    OnPropertyChanged("RuleDisplayString");
                }
            }
        }
        #endregion RuleDisplayString

        //private string _editedValue;
        public string EditedValue {
            get { return DbRowEntry.EditedValue; }
            set {
                if (DbRowEntry.EditedValue != value) {
                    DbRowEntry.EditedValue = value;
                    OnPropertyChanged("EditedValue");
                    OnPropertyChanged("DisplayString");
                    SetRuleDisplayString(_rules);
                }
            }
        }


        private RowEntryStatus _status = RowEntryStatus.Used;
        public RowEntryStatus Status {
            get { return _status; }
            set {
                if (_status != value) {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }


        #endregion

        private void SetRuleDisplayString(RuleSet rules) {
            string result = rules.ExecuteRules.Aggregate(DisplayString, (current, rule) => rule.Execute(current));
            RuleDisplayString = result;
        }

        public bool IsDbNull() {
            return DisplayString == "DBNULL";
        }

        public void SetValue(object value) {
            DbRowEntry.Value = value;
            OnPropertyChanged("DisplayString");
        }

        public void UpdateRuleDisplayString() {
            SetRuleDisplayString(_rules);
        }
    }
}
