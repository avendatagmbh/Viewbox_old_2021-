// --------------------------------------------------------------------------------
// author: Marcus Gerlach
// since: 2012-02-21
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using AV.Log;
using AvdCommon.DataGridHelper;
using Business.Structures;
using Business.Structures.InputAgents;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Utils;
using log4net;

namespace TransDATA.Models.ConfigModels {
    public class SelectCsvInputModel : NotifyPropertyChangedBase{
        internal ILog _log = LogHelper.GetLogger();

        public SelectCsvInputModel(IProfile profile) {
            Profile = profile;
            InputConfig = Profile.InputConfig.Config as ICsvInputConfig;
            profile.InputConfig.PropertyChanged += InputConfig_PropertyChanged;
            _csvFiles = new ObservableCollectionAsync<FileView>();
            CreatePreviewCsvFiles();
        }

        ~SelectCsvInputModel() {
            if(InputConfig != null)
                InputConfig.PropertyChanged -= CsvInputConfig_PropertyChanged;
        }

        void InputConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Config") {
                InputConfig = Profile.InputConfig.Config as ICsvInputConfig;
                if (InputConfig != null) {
                    InputConfig.PropertyChanged -= CsvInputConfig_PropertyChanged;
                    InputConfig.PropertyChanged += CsvInputConfig_PropertyChanged;
                }
            }
        }

        void CsvInputConfig_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Folder" || e.PropertyName == "ImportSubDirectories") {
                CreatePreviewCsvFiles();
            }
        }

        #region Properties
        public IProfile Profile {get; set; }

        #region InputConfig
        private ICsvInputConfig _inputConfig;

        public ICsvInputConfig InputConfig {
            get { return _inputConfig; }
            set {
                if (_inputConfig != value) {
                    _inputConfig = value;
                    OnPropertyChanged("InputConfig");
                }
            }
        }
        #endregion InputConfig

        #region CsvFiles
        private ObservableCollection<FileView> _csvFiles { get; set; }
        private ICollectionView _csvFilesView;

        public ICollectionView CsvFiles {
            get {
                if (_csvFilesView == null) {
                    InitCsvFilesView();
                }
                return _csvFilesView;
            }
        }

        #endregion CsvFiles

        #region CsvPreview
        private DataTable _csvPreview;
        //private Window _owner;

        public DataTable CsvPreview {
            get { return _csvPreview; }
            set {
                if (_csvPreview != value) {
                    _csvPreview = value;
                    OnPropertyChanged("CsvPreview");
                }
            }
        }
        #endregion CsvPreview

        private CancellationTokenSource _cancellationTokenSource;
        #endregion Properties

        void _csvFilesView_CurrentChanged(object sender, EventArgs e) {
            if (CsvFiles.CurrentItem == null)
                CsvPreview = null;
            else {
                try {
                    CsvPreview =
                        DataGridCreater.CreateDataTable(InputAgentCsv.DataTableFromFilename(10,
                                                                                            ((FileView)
                                                                                             CsvFiles.CurrentItem).
                                                                                                FullFilePath,
                                                                                            InputConfig));
                } catch (Exception ex) {
                    using (NDC.Push(LogHelper.GetNamespaceContext()))
                    {
                        _log.Error(ex.Message, ex);
                    }

                    MessageBox.Show("Es ist ein Fehler beim Laden der Vorschau aufgetreten: " + ex.Message, "",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region CreatePreviewCsvFiles
        private void CreatePreviewCsvFiles() {
            if(_cancellationTokenSource != null) {
                _cancellationTokenSource.Cancel();
            }
            InitCsvFilesView();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(CreatePreviewCsvFilesImpl, _cancellationTokenSource.Token).ContinueWith(HandlePreviewCsvError, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void InitCsvFilesView() {
            bool firstTime = _csvFilesView != null;

            _csvFiles = new ObservableCollectionAsync<FileView>();
            CollectionViewSource source = new CollectionViewSource() {Source = _csvFiles};
            if(firstTime)
                _csvFilesView.CurrentChanged -= _csvFilesView_CurrentChanged;
            _csvFilesView = source.View;
            _csvFilesView.CurrentChanged += _csvFilesView_CurrentChanged;
            //if (firstTime)
                OnPropertyChanged("CsvFiles");
        }

        private void HandlePreviewCsvError(Task task) {
            if (task.Exception != null) {
                using (NDC.Push(LogHelper.GetNamespaceContext()))
                {
                    _log.Error(task.Exception.Message, task.Exception);
                }

                MessageBox.Show(
                                "Es ist ein Fehler beim Anzeigen der Csv Vorschau aufgetreten: " +
                                task.Exception.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreatePreviewCsvFilesImpl() {
            if(InputConfig == null) return;
            CancellationToken token = _cancellationTokenSource.Token;
            _csvFiles.Clear();
            if(string.IsNullOrEmpty(InputConfig.Folder) || !Directory.Exists(InputConfig.Folder))
                return;
            foreach (var file in InputConfig.GetCsvFilesInFolder()) {
                if (token.IsCancellationRequested)
                    break;
                _csvFiles.Add(new FileView(file) {Folder = InputConfig.Folder});
            }
        }
        #endregion CreatePreviewCsvFiles

        #region DetectLineEnd
        public void DetectLineEnd(Window owner) {
            //TODO: (csb) translate
            if (!Directory.Exists(InputConfig.Folder)) {
                MessageBox.Show(owner, string.Format("Der Ordner \"{0}\" existiert nicht.", InputConfig.Folder), "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try {
                bool foundLineEnd = false;
                string foundLineEndInFile = "";
                foreach (var file in Directory.EnumerateFiles(InputConfig.Folder, "*.csv", InputConfig.ImportSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)) {
                    CsvReader reader = new CsvReader(file);
                    string lineEnd = reader.DetectCommonFileEnd(10, Encoding.ASCII);
                    if (!string.IsNullOrEmpty(lineEnd)) {
                        InputConfig.LineEndSeperator = lineEnd;
                        foundLineEnd = true;
                        foundLineEndInFile = file;
                        break;
                    }
                }
                if (foundLineEnd)
                    MessageBox.Show(owner,
                                    string.Format("Es wurde das Zeilenende {0} aus der Datei \"{1}\" erkannt", InputConfig.LineEndSeperator, new FileInfo(foundLineEndInFile).Name),
                                    "", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(owner,
                                    string.Format("Es konnte kein Zeilenende erkannt werden, weil es entweder keine CSV-Dateien im angegebenen Verzeichnis gibt oder diese jeweils nur aus einer Zeile bestehen."),
                                    "", MessageBoxButton.OK, MessageBoxImage.Warning);
            } catch (Exception ex) {
                using (NDC.Push(LogHelper.GetNamespaceContext()))
                {
                    _log.Error(ex.Message, ex);
                }

                MessageBox.Show(owner,
                                "Es ist ein unerwarteter Fehler beim Erkennen der Dateiendungen aufgetreten: " +
                                Environment.NewLine + ex.Message);
            }
        }
        #endregion DetectLineEnd


         #region DetectEncoding
        public void DetectEncoding(Window owner)
        {
            //TODO: (csb) translate
            if (!Directory.Exists(InputConfig.Folder))
            {
                MessageBox.Show(owner, string.Format("Der Ordner \"{0}\" existiert nicht.", InputConfig.Folder), "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try {
                bool foundLineEnd = false;
                string foundLineEndInFile = "";
                foreach (var file in Directory.EnumerateFiles(InputConfig.Folder, "*.csv", InputConfig.ImportSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)) {

                    CsvReader reader = new CsvReader(file);
                    Encoding encoding = reader.DetectEncoding(1024*1024*10);
                    if (encoding!=null) {
                        InputConfig.FileEncoding = encoding.WebName;
                        foundLineEnd = true;
                        foundLineEndInFile = file;
                        break;
                    }
                }
                if (foundLineEnd)
                    MessageBox.Show(owner,
                                    string.Format("Es wurde das Zeilenende {0} aus der Datei \"{1}\" erkannt", InputConfig.FileEncoding, new FileInfo(foundLineEndInFile).Name),
                                    "", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(owner,
                                    string.Format("Es konnte kein Zeilenende erkannt werden, weil es entweder keine CSV-Dateien im angegebenen Verzeichnis gibt oder diese jeweils nur aus einer Zeile bestehen."),
                                    "", MessageBoxButton.OK, MessageBoxImage.Warning);
            } catch (Exception ex) {
                using (NDC.Push(LogHelper.GetNamespaceContext()))
                {
                    _log.Error(ex.Message, ex);
                }

                MessageBox.Show(owner,
                                "Es ist ein unerwarteter Fehler beim Erkennen der Dateiendungen aufgetreten: " +
                                Environment.NewLine + ex.Message);
            }
        }
        #endregion DetectEncoding
    }
}