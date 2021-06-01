using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace DatabaseSplitter.Models {
    public class MainModel : NotifyPropertyChangedBase {

        public MainModel() {
            DatabaseModel = new DatabaseModel() {
                Database = "heitkamp_sap",
                DatabaseAnalyser = "heitkamp_sap_transfer_analyser",
                Host = "dbheitkamp",
                Password = "avendata",
                User = "root"
            };
            SplitFile = @"C:\Users\mag\Desktop\Heitkamp_Split.xml";
        }

        #region DatabaseModel
        private DatabaseModel _databaseModel;

        public DatabaseModel DatabaseModel {
            get { return _databaseModel; }
            set {
                if (_databaseModel != value) {
                    _databaseModel = value;
                    OnPropertyChanged("DatabaseModel");
                }
            }
        }
        #endregion DatabaseModel

        #region SplitFile
        private string _splitFile;

        public string SplitFile {
            get { return _splitFile; }
            set {
                if (_splitFile != value) {
                    _splitFile = value;
                    OnPropertyChanged("SplitFile");
                }
            }
        }
        #endregion SplitFile
    }
}
