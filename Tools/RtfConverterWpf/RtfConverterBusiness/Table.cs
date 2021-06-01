using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Utils;

namespace RtfConverterBusiness {
    [XmlRoot("Table", Namespace = "", IsNullable = false)]
    public class Table : NotifyPropertyChangedBase{
        #region Constructor
        public Table() {
            
        }
        public Table(string name) {
            Name = name;
        }
        #endregion Constructor

        #region Properties

        #region Name
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    _targetName = _name;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name

        #region TargetName
        private string _targetName;

        public string TargetName {
            get { return _targetName; }
            set {
                if (_targetName != value) {
                    _targetName = value;
                    OnPropertyChanged("TargetName");
                }
            }
        }
        #endregion TargetName

        #region IsChecked
        private bool _isChecked;

        public bool IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked != value) {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }
        #endregion IsChecked

        #region ColumnString
        private string _columnString;

        public string ColumnString {
            get { return _columnString; }
            set {
                if (_columnString != value) {
                    _columnString = value == null ? value : value.ToLower();
                    OnPropertyChanged("ColumnString");
                }
            }
        }
        #endregion ColumnString

        public List<string> Columns{get {
            if(string.IsNullOrEmpty(ColumnString)) return new List<string>();
            return new List<string>(from column in ColumnString.Split(new char[] {';'}) select column.Trim());
        }}
        #endregion Properties

        #region Methods
        #endregion Methods

    }
}
