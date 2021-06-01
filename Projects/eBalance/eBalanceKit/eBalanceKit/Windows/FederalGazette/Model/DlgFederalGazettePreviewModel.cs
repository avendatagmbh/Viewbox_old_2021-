// --------------------------------------------------------------------------------
// author: Solueman Hussain
// since: 2012-01-04
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using Utils;
using eBalanceKit.Structures;
using eBalanceKitBase.Structures;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness;
using eBalanceKitBusiness.FederalGazette;
using eBalanceKitBusiness.FederalGazette.Model;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.FederalGazette.Model {
    internal class DlgFederalGazettePreviewModel : INotifyPropertyChanged {
        public DlgFederalGazettePreviewModel(FederalGazetteMainModel model) {
            _documentWrapper = new ObjectWrapper<Document>();

            _navigationTree = new NavigationTreeFg(_documentWrapper);
            _federalGazetteModel = model;
            NavigationTree.InitNavigation(_federalGazetteModel);

        }

        #region locals
        
        public Window Owner { get; private set; }
        private DlgProgress _progress;

        private ProgressInfo _progressInfo;

        private readonly ObjectWrapper<Document> _documentWrapper;

        private readonly NavigationTreeFg _navigationTree;
        public NavigationTreeFg NavigationTree { get { return _navigationTree; } }
        private FederalGazetteMainModel _federalGazetteModel;
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        #endregion

        #region CurrentDocument
        private Document _currentDocument;
        public Document CurrentDocument {
            get { return _currentDocument; }
            set {
                if (_currentDocument != null) {
                    foreach (IBalanceList balanceList in _currentDocument.BalanceLists) balanceList.ClearEntries();
                    GC.Collect();
                }
                _currentDocument = value;

                OnPropertyChanged("CurrentDocument");
                if (value != null) {
                    _progress = new DlgProgress(Owner);
                    _progressInfo = _progress.ProgressInfo;
                    _progressInfo.IsIndeterminate = true;
                    _progressInfo.Caption = "report wird geladen";
                    new Thread(LoadValueTree).Start();
                    _progress.ShowDialog();

                }
                _documentWrapper.Value = _currentDocument;

            }
        }
        #endregion

        #region SelectedNavigationEntry
        private NavigationTreeEntryFg _selectNavigationEntry;
        public NavigationTreeEntryFg SelectedNavigationEntry {
            get { return _selectNavigationEntry; }
            set {
                if (_selectNavigationEntry != value) {
                    _selectNavigationEntry = value;
                    OnPropertyChanged("SelectedNavigationEntry");
                }
            }
        }
        #endregion

        #region LoadValueTree
        private void LoadValueTree() {
            //CurrentDocument.LoadBalanceLists();
            //CurrentDocument.LoadValueTrees(_progressInfo);

        }
        #endregion

        #region savefiles

        public void SaveFiles(string file) {

            var getXbrl = new FederalGazetteGetXbrl(_federalGazetteModel);

            if (_federalGazetteModel.ExportBalanceSheet) {
                var xbrl = getXbrl.GetBalanceSheet();
                using (var writeFile = new StreamWriter(file.Insert(file.LastIndexOf("."), "_Bilanz"))) {
                    writeFile.WriteLine(xbrl);
                }
            }
            if (_federalGazetteModel.ExportFixedAssets) {
                var xbrl = getXbrl.GetFixedAssets();
                using (var writeFile = new StreamWriter(file.Insert(file.LastIndexOf("."), "_AnlageSpiegel"))) {
                    writeFile.WriteLine(xbrl);
                }
            }
            if (_federalGazetteModel.ExportIncomeStatement) {
                var xbrl = getXbrl.GetIncomeStatement();
                using (var writeFile = new StreamWriter(file.Insert(file.LastIndexOf("."), "_GuV"))) {
                    writeFile.WriteLine(xbrl);
                }
            }

            if (_federalGazetteModel.ExportManagementReport) {
                var xbrl = getXbrl.GetManagementReport();
                using (var writeFile = new StreamWriter(file.Insert(file.LastIndexOf("."), "_Lagebericht"))) {
                    writeFile.WriteLine(xbrl);
                }
            }

            if (_federalGazetteModel.ExportNetProfit) {
                var xbrl = getXbrl.GetNetProfit();
                using (var writeFile = new StreamWriter(file.Insert(file.LastIndexOf("."), "_Ergbnisverwendeung"))) {
                    writeFile.WriteLine(xbrl);
                }
            }

            if (_federalGazetteModel.ExportNotes) {
                var xbrl = getXbrl.GetNotes();
                using (var writeFile = new StreamWriter(file.Insert(file.LastIndexOf("."), "_Anhang"))) {
                    writeFile.WriteLine(xbrl);
                }
            }

            var xbrlAll = getXbrl.GetAllAccounts();
            using (var writeFile = new StreamWriter(file)) {
                writeFile.WriteLine(xbrlAll);
            }
        }
        #endregion
    }
}