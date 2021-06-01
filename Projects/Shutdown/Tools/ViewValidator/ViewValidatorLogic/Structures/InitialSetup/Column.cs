// -----------------------------------------------------------
// Created by Benjamin Held - 25.08.2011 14:19:53
// Copyright AvenDATA 2011
// -----------------------------------------------------------


using System.ComponentModel;
using AvdCommon.Rules;
using AvdCommon.Rules.Interfaces;
using DbAccess.Structures;

namespace ViewValidatorLogic.Structures.InitialSetup {
    public class Column : INotifyPropertyChanged, IColumn {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        public string Name { get; set; }
        public RuleSet Rules { get; set; }

        private DbColumnTypes _type;
        public DbColumnTypes Type {
            get { return _type; }
            set {
                if (_type != value) {
                    _type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public ColumnMapping ColumnMapping { get; set; }

        public Column(string name, ColumnMapping mapping = null){
            Name = name;
            Rules = new RuleSet();
            Type = DbColumnTypes.DbUnknown;
            ColumnMapping = mapping;
        }
    }
}
