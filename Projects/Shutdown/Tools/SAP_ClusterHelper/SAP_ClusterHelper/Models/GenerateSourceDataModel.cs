// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace SAP_ClusterHelper.Models {
    public class GenerateSourceDataModel : Utils.NotifyPropertyChangedBase {
        
        public GenerateSourceDataModel() {
            DbUser = "root";
            DbPassword = "avendata";

            DbHostname = "dbheitkamp";
            DbDatabase = "heitkamp_sap";
        }
        
        public bool GenerateSTXL { get; set; }

        #region GeneratePCLAll
        private bool? _generatePCLAll;

        public bool? GeneratePCLAll {
            get { return _generatePCLAll; }
            set {
                if (_generatePCLAll == value) return;
                _generatePCLAll = value;

                if (value.HasValue) {
                    _generatePCL1 = value.Value;
                    _generatePCL2 = value.Value;
                    _generatePCL3 = value.Value;
                    _generatePCL4 = value.Value;
                }

                OnPropertyChanged("GeneratePCLAll");
                OnPropertyChanged("GeneratePCL1");
                OnPropertyChanged("GeneratePCL2");
                OnPropertyChanged("GeneratePCL3"); 
                OnPropertyChanged("GeneratePCL4");
            }
        }
        #endregion // GeneratePCLAll

        #region GeneratePCL1
        private bool _generatePCL1;

        public bool GeneratePCL1 {
            get { return _generatePCL1; }
            set {
                if (_generatePCL1 == value) return;
                _generatePCL1 = value;
                UpdateGeneratePCLxState();
                OnPropertyChanged("GeneratePCL1");
            }
        }
        #endregion // GeneratePCL1

        #region GeneratePCL2
        private bool _generatePCL2;

        public bool GeneratePCL2 {
            get { return _generatePCL2; }
            set {
                if (_generatePCL2 == value) return;
                _generatePCL2 = value;
                UpdateGeneratePCLxState();
                OnPropertyChanged("GeneratePCL2");
            }
        }
        #endregion // GeneratePCL2

        #region GeneratePCL3
        private bool _generatePCL3;

        public bool GeneratePCL3 {
            get { return _generatePCL3; }
            set {
                if (_generatePCL3 == value) return;
                _generatePCL3 = value;
                UpdateGeneratePCLxState();
                OnPropertyChanged("GeneratePCL3");
            }
        }
        #endregion // GeneratePCL3

        #region GeneratePCL4
        private bool _generatePCL4;

        public bool GeneratePCL4 {
            get { return _generatePCL4; }
            set {
                if (_generatePCL4 == value) return;
                _generatePCL4 = value;
                UpdateGeneratePCLxState();
                OnPropertyChanged("GeneratePCL4");
            }
        }
        #endregion // GeneratePCL4

        #region database config
        public string DbHostname { get; set; }
        public string DbUser { get; set; }
        public string DbPassword { get; set; }
        public string DbDatabase { get; set; }
        #endregion // database config

        #region ExportFolder
        private string _exportFolder;

        public string ExportFolder {
            get { return _exportFolder; }
            set {
                if (_exportFolder == value) return;
                _exportFolder = value;
                OnPropertyChanged("ExportFolder");
            }
        }
        #endregion // ExportFolder
        
        public string ScriptFolder { get { return ExportFolder + "\\scripts"; } }
        public string DataFolder { get { return ExportFolder + "\\data"; } }


        #region UpdateGeneratePCLxState
        private void UpdateGeneratePCLxState() {
            if (GeneratePCL1 == GeneratePCL2 && GeneratePCL1 == GeneratePCL3 && GeneratePCL1 == GeneratePCL4) _generatePCLAll = GeneratePCL1;
            else _generatePCLAll = null;

            OnPropertyChanged("GeneratePCLAll");
        }
        #endregion // UpdateGeneratePCLxState
    }
}