// --------------------------------------------------------------------------------
// author: Gabor Bauer
// since: 2012-05-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using Utils;
using eBalanceKitBase.Interfaces;

namespace eBalanceKitBase.Structures {
    public class ImportPreview : NotifyPropertyChangedBase, IImportPreview {
        #region [ Members ]

        private int fileIndex = -1;

        #endregion [ Members ]

        #region [ Constructor ]

        public ImportPreview(ObservableCollectionAsync<string> csvFiles, PropertyChangedEventHandler propertyChangedDelegate, Encoding encoding, char textDelimiter, char separator) {
            this.CsvFileNames = new ObservableCollectionAsync<string>();
            if (csvFiles == null)
                this.CsvFiles = new ObservableCollectionAsync<string>();
            else {
                this.CsvFiles = new ObservableCollectionAsync<string>();
                this.CsvFiles.CollectionChanged += CsvFilesOnCollectionChanged;
                //this.CsvFiles = csvFiles;
                foreach (string csvFile in csvFiles) {
                    this.CsvFiles.Add(csvFile);   
                }
                this.fileIndex = 0;
                UpdatePreview();
            }
            _encoding = encoding;
            _textDelimiter = textDelimiter;
            _separator = separator;
            this.PropertyChanged += propertyChangedDelegate;
            }

        #endregion [ Constructor ]

        #region [ IImportPreview members ]

        private char _textDelimiter = '\"';

        public char TextDelimiter {
            get { return _textDelimiter; }
            set {
                _textDelimiter = value;
                OnPropertyChanged("TextDelimiter");
                UpdatePreview();
            }
        }

        private char _separator = ';';

        public char Separator {
            get { return _separator; }
            set {
                _separator = value;
                OnPropertyChanged("Separator");
                UpdatePreview();
            }
        }

        private Encoding _encoding = Encoding.Default;

        public Encoding Encoding {
            get { return _encoding; }
            set {
                _encoding = value;
                OnPropertyChanged("Encoding");
                UpdatePreview();
            }
        }

        public ObservableCollectionAsync<string> CsvFiles { get; private set; }
        public ObservableCollectionAsync<string> CsvFileNames { get; private set; }

        private System.Data.DataTable _previewData = null;

        public System.Data.DataTable PreviewData {
            get { return _previewData; }
            private set {
                _previewData = value;
                OnPropertyChanged("PreviewData");
            }
        }

        public void Update(char separator, char textDelimiter, Encoding encoding) {
            this.TextDelimiter = textDelimiter;
            this.Separator = separator;
            this.Encoding = encoding;
            UpdatePreview();
        }

        public void PreviewFile(string fileName) {
            int fileIndex = this.CsvFiles.IndexOf(fileName);
            if (fileIndex > -1) {
                this.fileIndex = fileIndex;
                UpdatePreview();
            }
        }

        public bool Next() {
            if (this.IsNextAllowed) {
                this.fileIndex += 1;
                UpdatePreview();
                OnPropertyChanged("IsNextAllowed");
                OnPropertyChanged("IsPreviousAllowed");
                return true;
            }
            return false;
        }

        public bool Previous() {
            if (this.IsPreviousAllowed) {
                this.fileIndex -= 1;
                UpdatePreview();
                OnPropertyChanged("IsNextAllowed");
                OnPropertyChanged("IsPreviousAllowed");
                return true;
            }
            return false;
        }

        private bool _isNextAllowed;

        public bool IsNextAllowed {
            get {
                if (this.fileIndex == -1) _isNextAllowed = false;
                else _isNextAllowed = this.fileIndex < CsvFiles.Count - 1;
                return _isNextAllowed;
            }
            private set {
                _isNextAllowed = value;
                OnPropertyChanged("IsNextAllowed");
            }
        }

        private bool _isPreviousAllowed;

        public bool IsPreviousAllowed {
            get {
                if (this.fileIndex == -1) _isPreviousAllowed = false;
                else _isPreviousAllowed = this.fileIndex > 0;
                return _isPreviousAllowed;
            }
            private set {
                _isNextAllowed = value;
                OnPropertyChanged("IsPreviousAllowed");
            }
        }

        #endregion [ IImportPreview members ]

        #region [ Methods ]

        private void UpdatePreview() {
            if (CsvFiles.Count == 0) return;
            CsvReader reader = new CsvReader(CsvFiles[this.fileIndex]) {
                Separator = Separator,
                HeadlineInFirstRow = true,
                StringsOptionallyEnclosedBy = TextDelimiter
            };
            PreviewData = reader.GetCsvData(500, Encoding);
            PreviewData.TableName = reader.Filename;
        }

        private void CsvFilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) { 
            this.CsvFileNames.Clear();
            foreach (string csvFile in CsvFiles) {
                this.CsvFileNames.Add(GetFileName(csvFile));
            }
        }

        public string GetFileName(string path) {
            if (path == null) return null;
            return System.IO.Path.GetFileName(path);
        }

        #endregion [ Methods ]
    }
}
