using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using Utils;
using Utils.Commands;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Import;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Models.Import
{
    class ImportXbrlModel : NotifyPropertyChangedBase {

        #region Constructor
        public ImportXbrlModel(Window owner, Action refreshCompaniesAction){
            AddFileCommand = new DelegateCommand(o => true, AddFile);
            DeleteFileCommand = new DelegateCommand(o => true, DeleteFile);
            XbrlFile = new ObservableCollectionAsync<string>();
            Systems = SystemManager.Instance.Systems;
            SelectedSystem = GlobalResources.MainWindow.Model.SelectedSystem;
            ReportName = null;
            Owner = owner;
            //_refreshCompaniesAction = refreshCompaniesAction;
        }

        #endregion Constructor

        #region Properties

        public Window Owner { get; private set; }
        public ICommand AddFileCommand { get; set; }
        public ICommand DeleteFileCommand { get; set; }
        public ObservableCollectionAsync<string> XbrlFile { get; set; }
        public ObservableCollection<eBalanceKitBusiness.Structures.DbMapping.System> Systems { get; set; }
        public bool HasSelectedFile { get { return !string.IsNullOrEmpty(SelectedFile); } }
        public bool HasNoFile { get { return XbrlFile.Count == 0; } }
        public string FinancialYear { get; set; }
        public string Company { get; set; }
        public string ReportName { get; set; }
        public XbrlImporter Myimporter;
        //private Action _refreshCompaniesAction;

        private string _selectedFile;

        public string SelectedFile {
            get { return _selectedFile; }
            set {
                if (_selectedFile != value) {
                    _selectedFile = value;
                    OnPropertyChanged("SelectedFile");
                    OnPropertyChanged("HasSelectedFile");
                }
            }
        }

        public eBalanceKitBusiness.Structures.DbMapping.System SelectedSystem { get; set; }


        #endregion Properties

        private void AddFile(object parameter) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileOk += dlg_FileOk;
            dlg.Filter = "XML Files (.xml)|*.xml|All Files (*.*)|*.*";
            dlg.Multiselect = false;
            dlg.ShowDialog();
        }

        private void DeleteFile(object parameter) {

            XbrlFile.Remove(SelectedFile);
            OnPropertyChanged("HasNoFile");
            OnPropertyChanged("XbrlFile");
        }

        private void dlg_FileOk(object sender, CancelEventArgs e) {
            var fileDialog = (OpenFileDialog)sender;
            foreach (string filename in fileDialog.FileNames.Where(filename => XbrlFile.All(f => string.Compare(f, filename, StringComparison.InvariantCultureIgnoreCase) != 0))) {
                XbrlFile.Add(filename);
            }
            OnPropertyChanged("HasNoFile");
            OnPropertyChanged("XbrlFile");
        }

        public void SetDetailsFromXbrl() {
            if(XbrlFile.Count > 0) {
                FileStream stream = File.Open(XbrlFile[0], FileMode.Open, FileAccess.Read);
                Myimporter = new XbrlImporter(stream);
                Tuple<string, string> valuesForDialog = Myimporter.GetXbrlDetailsForDialog();
                FinancialYear = valuesForDialog.Item2;
                Company = valuesForDialog.Item1;
                OnPropertyChanged("FinancialYear");
                OnPropertyChanged("Company");   
            }
        }

        public void Import(Action action) {
            DlgProgress progress = new DlgProgress(Owner) { ProgressInfo = { Caption = ResourcesCommon.ProgressImportingXbrl, IsIndeterminate = true } };
            progress.ExecuteModal(() => { 
                Myimporter.Import(ReportName, SelectedSystem);
                action();
            });
        }
    }
}
