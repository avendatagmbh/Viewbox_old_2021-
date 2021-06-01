using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Microsoft.Win32;
using Utils;
using Utils.Commands;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Import;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;
using System.Diagnostics;
using eBalanceKitBase.Structures;

namespace eBalanceKit.Models.Import
{
    class ImportFullModel : NotifyPropertyChangedBase {

        #region Constructor
        public ImportFullModel(Window owner, Action refreshCompaniesAction){
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
        public bool HasNoSystem { get { return System == null; } }
        public bool HasNoCompany { get { return Company == null; } }
        public bool HasNoReportName { get { return ReportName == null; } }
        public bool HasNoFinancialYear { get { return FinancialYear == null; } }

        public string FinancialYear { get; set; }
        public XmlNode Company { get; set; }
        public XmlNode System { get; set; }
        public XmlNode Report { get; set; }
        public string CompanyName { get; set; }
        public string SystemName { get; set; }
        public string ReportName { get; set; }
        public XbrlImporter XbrlImporter;
        public FullImporter FullImporter;
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

        public void SetDetailsFromFullExport() {
            if (XbrlFile.Count > 0) {
                using(FileStream stream = File.Open(XbrlFile[0], FileMode.Open, FileAccess.Read)) {
                    XbrlImporter = new XbrlImporter(stream);
                    Tuple<XmlNode, string, XmlNode, XmlNode> valuesForDialog = XbrlImporter.GetFullDetailsForDialog();
                    if (valuesForDialog.Item2 != null) {
                        FinancialYear = valuesForDialog.Item2;
                    }
                    if (valuesForDialog.Item1 != null) {
                        Company = valuesForDialog.Item1;
                        foreach (XmlNode node in Company.ChildNodes.Cast<XmlNode>().Where(node => node.Name == "genInfo.company.id.name")) {
                            CompanyName = node.InnerText;
                            break;
                        }
                    }
                    if (valuesForDialog.Item3 != null) {
                        System = valuesForDialog.Item3;
                        SystemName = System.InnerText;
                    }
                    if (valuesForDialog.Item4 != null && valuesForDialog.Item4.Attributes != null) {
                        Report = valuesForDialog.Item4;
                        ReportName = valuesForDialog.Item4.Attributes["name"].Value;
                    }
                }
                

                OnPropertyChanged("FinancialYear");
                OnPropertyChanged("Company");
                OnPropertyChanged("CompanyName");
                OnPropertyChanged("System");
                OnPropertyChanged("SystemName");
                OnPropertyChanged("ReportName");

                OnPropertyChanged("HasNoFinancialYear");
                OnPropertyChanged("HasNoCompany");
                OnPropertyChanged("HasNoSystem");
                OnPropertyChanged("HasNoReportName");
            }
        }

        public void Import(Action action) {
            var mainWindowModel = ((MainWindowModel)GlobalResources.MainWindow.DataContext);
            var progress = new DlgProgress(Owner) { ProgressInfo = { Caption = ResourcesCommon.ProgressImportingFullReport, IsIndeterminate = true } };

            /*var watch = new Stopwatch();
            watch.Start();*/

            //progress.ExecuteModal(() => {
            //    using (new MainWindowLocker(new LockableSourceParameter(progress.ProgressInfo), false))
            //    {
            //        progress.ProgressInfo.Caption = ResourcesCommon.ProgressImportingFullReport;
            //        progress.ProgressInfo.IsIndeterminate = true;
            //        using (var stream = File.Open(XbrlFile[0], FileMode.Open, FileAccess.Read))
            //        {
            //            FullImporter = new FullImporter(stream);
            //        }
            //        using (var stream = File.Open(XbrlFile[0], FileMode.Open, FileAccess.Read))
            //        {
            //            FullImporter.Import(Report, System, Company, FinancialYear, stream);
            //        }
            //        action();
            //    }
            //});

            progress.ExecuteModal(() =>
            {
                LockableSourceParameter lsp = new LockableSourceParameter(progress.ProgressInfo);
                using (new MainWindowLocker(
                    Owner,
                    lsp,
                    (LockableSourceParameter p) => { ((MainWindowModel)GlobalResources.MainWindow.DataContext).LockSource(lsp); },
                    (LockableSourceParameter p) => { ((MainWindowModel)GlobalResources.MainWindow.DataContext).UnLockSource(lsp); },
                    true,
                    true))
                {
                    progress.ProgressInfo.Caption = ResourcesCommon.ProgressImportingFullReport;
                    progress.ProgressInfo.IsIndeterminate = true;
                    using (var stream = File.Open(XbrlFile[0], FileMode.Open, FileAccess.Read))
                    {
                        FullImporter = new FullImporter(stream);
                    }
                    using (var stream = File.Open(XbrlFile[0], FileMode.Open, FileAccess.Read))
                    {
                        FullImporter.Import(Report, System, Company, FinancialYear, stream);
                    }
                    action();
                }
            });

            var mainWindow = GlobalResources.MainWindow;
            var tmp = mainWindow.Model.CurrentDocument;
            mainWindow.Model.CurrentDocument = null;
            mainWindow.Model.CurrentDocument = tmp;
            mainWindow.Model.LoadEnvironment();

            /*watch.Stop();
            Console.WriteLine("full time: " + watch.ElapsedMilliseconds);*/
        }

    }
}
