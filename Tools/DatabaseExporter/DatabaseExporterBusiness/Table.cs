using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Utils;

namespace DatabaseExporterBusiness {
    [XmlRoot("Table", Namespace = "", IsNullable = false)]
    public class Table : NotifyPropertyChangedBase{
        #region Constructor
        public Table() {
            
        }
        public Table(string name) {
            Name = name;
            SetCsvName();
        }

        private void SetCsvName() {
            CsvFile = Name + "_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";
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
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name

        #region IsChecked
        private bool _isChecked;

        public bool IsChecked {
            get { return _isChecked; }
            set {
                if (_isChecked != value) {
                    _isChecked = value;
                    SetCsvName();
                    OnPropertyChanged("IsChecked");
                }
            }
        }
        #endregion IsChecked

        #region CsvFile
        private string _csvFile;

        public string CsvFile {
            get { return _csvFile; }
            set {
                if (_csvFile != value) {
                    _csvFile = value;
                    OnPropertyChanged("CsvFile");
                }
            }
        }
        #endregion CsvFile

        #region SecondLine
        private string _secondLine;

        public string SecondLine {
            get { return _secondLine; }
            set {
                if (_secondLine != value) {
                    _secondLine = value;
                    OnPropertyChanged("SecondLine");
                }
            }
        }
        #endregion SecondLine

        #region OrderBy
        private string _orderBy;

        public string OrderBy {
            get { return _orderBy; }
            set {
                if (_orderBy != value) {
                    _orderBy = value;
                    OnPropertyChanged("OrderBy");
                }
            }
        }
        #endregion OrderBy
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
