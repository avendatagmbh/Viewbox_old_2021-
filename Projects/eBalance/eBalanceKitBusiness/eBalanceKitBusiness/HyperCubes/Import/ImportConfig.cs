using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Interfaces.ViewModels;

namespace eBalanceKitBusiness.HyperCubes.Import {
    public class ImportConfig {

        public ImportConfig() {
            Dimensions = new ObservableCollection<DimensionInfo>();
        }

        public void Initialize() {
            this.Encoding = Encoding.Default;
            this.Seperator = ";";
            this.TextDelimiter = "\"";
        }

        #region events
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string property) {
            if (PropertyChanged != null)
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(property));
        }
        #endregion events

        #region CsvFileName
        private string _csvFileName;

        public string CsvFileName {
            get { return _csvFileName; }
            set {
                _csvFileName = value;
                OnPropertyChanged("CsvFileName");
            }
        }
        #endregion CsvFileName

        #region CsvFileName1
        private string _csvFileName1;

        public string CsvFileName1 {
            get { return _csvFileName1; }
            set {
                _csvFileName1 = value;
                OnPropertyChanged("CsvFileName1");
            }
        }
        #endregion CsvFileName2

        #region CsvFileName2
        private string _csvFileName2;

        public string CsvFileName2 {
            get { return _csvFileName2; }
            set {
                _csvFileName2 = value;
                OnPropertyChanged("CsvFileName2");
            }
        }
        #endregion CsvFileName2

        #region ElementId
        private string _elementId;

        public string ElementId {
            get { return _elementId; }
            set {
                _elementId = value;
                OnPropertyChanged("ElementId");
            }
        }
        #endregion ElementId


        public enum PossibleHyperCubes
        {
            ChangesEquityStatement,
            NtAssNet,
            NtAssGross,
            NtAssGrossShort
        }

        #region Warning
        private string _warning;

        public string Warning {
            get { return _warning; }
            set {
                if (_warning != value) {
                    _warning = value;
                    OnPropertyChanged("Warning");
                }
            }
        }
        #endregion Warning

        #region Encoding
        public Encoding Encoding {
            get { return _encoding; }
            set {
                if (_encoding != value) {
                    _encoding = value;
                    OnPropertyChanged("Encoding");
                }
            }
        }
        private Encoding _encoding;
        #endregion

        #region Seperator
        public string Seperator {
            get { return _seperator; }
            set {
                if (_seperator != value) {
                    _seperator = value;
                    OnPropertyChanged("Seperator");
                }
            }
        }
        private string _seperator;
        #endregion

        #region TextDelimiter
        public string TextDelimiter {
            get { return _textDelimiter; }
            set {
                if (_textDelimiter != value) {
                    _textDelimiter = value;
                    OnPropertyChanged("TextDelimiter");
                }
            }
        }
        private string _textDelimiter;
        #endregion

        public static Dictionary<PossibleHyperCubes, string> HyperCubeDictionary =
            new Dictionary<PossibleHyperCubes, string>() {
                {PossibleHyperCubes.NtAssNet, "de-gaap-ci_table.nt.ass.net"},
                {PossibleHyperCubes.ChangesEquityStatement, "de-gaap-ci_table.eqCh"},
                {PossibleHyperCubes.NtAssGrossShort, "de-gaap-ci_table.nt.ass.gross_short"},
                {PossibleHyperCubes.NtAssGross, "de-gaap-ci_table.nt.ass.gross"}
            };


        public struct FailedEntry {
            public FailedEntry(string message, long hyperCubeKey, string rowHeader, string columnHeader, string csvValue, string autoValue) {
                _message = message;
                _hyperCubeKey = hyperCubeKey;
                _rowHeader = rowHeader;
                _columnHeader = columnHeader;
                _csvValue = csvValue;
                _autoValue = autoValue;
            }

            private string _message;
            private long _hyperCubeKey;
            private string _rowHeader;
            private string _columnHeader;
            private string _csvValue;
            private string _autoValue;

            public string Message { get { return _message; } set { _message = value; } }

            public long HyperCubeKey { get { return _hyperCubeKey; } set { _hyperCubeKey = value; } }

            public string RowHeader { get { return _rowHeader; } set { _rowHeader = value; } }

            public string ColumnHeader { get { return _columnHeader; } set { _columnHeader = value; } }

            public string CsvValue { get { return _csvValue; } set { _csvValue = value; } }

            public string AutoValue { get { return _autoValue; } set { _autoValue = value; } }
        }

        public class DimensionInfo {
            
            public string DimensionName { get; set; }
            public int Index { get; set; }
            public IHyperCubeDimensionValue HyperCubeDimensionValue;
            public long UniquIdentifier { get; set; }
        }

        #region Dimensions
        private ObservableCollection<DimensionInfo> _dimensions;

        public ObservableCollection<DimensionInfo> Dimensions {
            get { return _dimensions; }
            set {
                if (_dimensions != value) {
                    _dimensions = value;
                    OnPropertyChanged("Dimensions");
                }
            }
        }
        #endregion Dimensions

    }
}
