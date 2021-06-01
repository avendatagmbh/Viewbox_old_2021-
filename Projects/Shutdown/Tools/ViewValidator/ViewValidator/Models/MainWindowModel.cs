using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using Utils;
using ViewValidator.Controls;
using ViewValidator.Controls.Result;
using ViewValidator.Controls.Rules;
using ViewValidator.Models.ListView;
using ViewValidator.Models.Result;
using ViewValidator.Models.Rules;
using ViewValidator.Structures;
using ViewValidator.Windows;
using ViewValidatorLogic.Config;
using ViewValidatorLogic.Logic;
using ViewValidatorLogic.Manager;
using ViewValidatorLogic.Structures.InitialSetup;
using MessageBox = System.Windows.MessageBox;
using ViewValidator.Utils;

namespace ViewValidator.Models  {
    public class MainWindowModel : INotifyPropertyChanged {

        public MainWindowModel(ProfileConfig lastProfile, string lastTableMapping, DlgMainWindow dlgMainWindow) {
            this.DlgMainWindow = dlgMainWindow;
            this.RuleSelectionDetailsModels = new Dictionary<TableMapping, RuleSelectionDetailsModel>();
            _ruleAssignmentModel = new RuleAssignmentModel(this, DlgMainWindow);
            this.ListViewTableMappingModel = new ListViewTableMappingModel(this) { Owner = DlgMainWindow };
            CtlListView ctlListView = new CtlListView(ListViewTableMappingModel);
            //this.ProfileNames = ProfileManager.ProfileNames;
            this.Profiles = ProfileManager.Profiles;
            InitWithProfile(lastProfile);


            SelectedProfile = lastProfile;
            if (SelectedProfile == null && Profiles != null && Profiles.Count > 0)
                SelectedProfile = Profiles[0];

            //Select last table mapping if available
            if (SelectedProfile != null && TableMappings != null) {
                foreach(var tableMapping in TableMappings)
                    if (tableMapping.UniqueName == lastTableMapping) {
                        SelectedTableMapping = tableMapping;
                        break;
                    }
            }

            //Get name of currently logged in user
            CurrentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            if (CurrentUser.LastIndexOf('\\') != -1) {
                CurrentUser = CurrentUser.Substring(CurrentUser.LastIndexOf('\\') + 1);
            }

            //Init navigation
            NavigationTreeEntry generalEntry = NavigationTree.AddEntry("Verwaltung", null);
            NavigationTree.AddEntry("Profile", new CtlListView(new ListViewProfileModel() { Owner = DlgMainWindow }), generalEntry);

            //ctlListView.DataScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;

            _viewEntry = NavigationTree.AddEntry("Views", ctlListView, generalEntry);
            NavigationTree.AddEntry("Einstellungen (Gesamt)", this.SettingsCtl, generalEntry);
            generalEntry.IsExpanded = true;


            ResultNavigationEntry = NavigationTree.AddEntry("Ergebnisse", null);
            ResultNavigationEntry.IsVisible = false;
            ResultNavigationEntry.IsExpanded = true;

            NavigationTree.AddEntry("Fehlende Zeilen", this.ResultRowsMissingCtl, ResultNavigationEntry);
            NavigationTree.AddEntry("Verschiedene Zeilen", this.ResultRowsDifferentCtl, ResultNavigationEntry);
            NavigationTree.AddEntry("Verschiedene Zeilen (Spaltenweise)", this.ResultRowsDifferentColumnWiseCtl, ResultNavigationEntry);

            //Create RuleAssignment window
            _dlgRuleAssignment = new DlgRuleAssignment();
            _dlgRuleAssignment.DataContext = _ruleAssignmentModel;
            var myScreen = Screen.PrimaryScreen; //Screen.FromControl(this);
            var otherScreen = Screen.AllScreens.FirstOrDefault(s => s != myScreen)
                                ?? myScreen;
            _dlgRuleAssignment.Left = otherScreen.WorkingArea.Left;
            _dlgRuleAssignment.Top = otherScreen.WorkingArea.Top;
            _dlgRuleAssignment.Width = otherScreen.WorkingArea.Width;
            _dlgRuleAssignment.Height = otherScreen.WorkingArea.Height;
            _dlgRuleAssignment.IsVisibleChanged += new DependencyPropertyChangedEventHandler(_ruleAssignmentWindow_IsVisibleChanged);
            _dlgRuleAssignment.Show();

        }


        private void InitWithProfile(ProfileConfig profile) {
            ResultsModel = new ResultsModel();
            ResultsModel.PropertyChanged += new PropertyChangedEventHandler(ResultsModel_PropertyChanged);
            SelectedProfile = profile;
            if(profile != null) RuleManager.Instance.SetProfileRules(profile);

            TableMappings = profile == null ? null : profile.ValidationSetup.TableMappings;

            this.SettingsModel = profile == null ? null : new Models.SettingsModel(profile.ValidationSetup);
            SettingsCtl.DataContext = SettingsModel;
        }

        void ResultsModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == "ResultsCalculated") {
                ResultNavigationEntry.IsVisible = ResultsModel.ResultsCalculated;
            }
        }

        #region INotifyPropertyChanged Member
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        #region Properties

        #region SelectedProfile
        private ProfileConfig _selectedProfile;

        public ProfileConfig SelectedProfile {
            get { return _selectedProfile; }
            set {
                if (_selectedProfile != value) {
                    _selectedProfile = value;
                    InitWithProfile(_selectedProfile);
                    
                    OnPropertyChanged("SelectedProfile");
                    OnPropertyChanged("ProfileSelectionVisibility");
                }
            }
        }
        #endregion SelectedProfile
        public SettingsModel SettingsModel { get; set; }

        #region TableMappings
        private ObservableCollection<TableMapping> _tableMappings;

        public ObservableCollection<TableMapping> TableMappings {
            get { return _tableMappings; }
            set {
                if (_tableMappings != value) {
                    _tableMappings = value;
                    if (_tableMappings != null) {
                        int id = 0;
                        foreach (var tableMapping in _tableMappings) {
                            RuleSelectionDetailsModels[tableMapping] = new RuleSelectionDetailsModel(tableMapping, id++) { TextColor = Brushes.White };
                        }
                        if (_tableMappings.Count != 0) SelectedTableMapping = _tableMappings[0];
                    }

                    OnPropertyChanged("TableMappings");
                    OnPropertyChanged("TableMappingSelectionVisibility");
                }
            }
        }
        #endregion

        public ResultsModel ResultsModel { get; set; }

        #region CurrentUser
        private string _currentUser;

        public string CurrentUser {
            get { return _currentUser; }
            private set { _currentUser = value; }
        }
        #endregion

        //public bool HasResults { get { return ResultsModel.AvailableTableResults.Count != 0; } }

        #region NavigationTree
        private NavigationTree _navigationTree = new NavigationTree(null);
        public NavigationTree NavigationTree {
            get { return _navigationTree; }
            set { _navigationTree = value; }
        }
        #endregion

        #region Controls

        #region SettingsCtl
        Settings _settingsCtl = new Settings();

        public Settings SettingsCtl {
            get { return _settingsCtl; }
            set { _settingsCtl = value; }
        }
        #endregion

        #region ResultRowsDifferentCtl
        ResultRowsDifferent _resultRowsDifferentCtl = new ResultRowsDifferent();

        public ResultRowsDifferent ResultRowsDifferentCtl {
            get { return _resultRowsDifferentCtl; }
            set { _resultRowsDifferentCtl = value; }
        }
        #endregion

        #region ResultRowsDifferentColumnWiseCtl
        ResultRowsDifferentColumnWise _resultRowsDifferentColumnWiseCtl = new ResultRowsDifferentColumnWise();

        public ResultRowsDifferentColumnWise ResultRowsDifferentColumnWiseCtl {
            get { return _resultRowsDifferentColumnWiseCtl; }
            set { _resultRowsDifferentColumnWiseCtl = value; }
        }
        #endregion

        #region ResultRowsMissingCtl
        ResultRowsMissing _resultRowsMissingCtl = new ResultRowsMissing();

        public ResultRowsMissing ResultRowsMissingCtl {
            get { return _resultRowsMissingCtl; }
            set { _resultRowsMissingCtl = value; }
        }

        #endregion
        #endregion

        #region SelectedTableMapping
        private TableMapping _selectedTableMapping;

        public TableMapping SelectedTableMapping {
            get { return _selectedTableMapping; }
            set {
                if (_selectedTableMapping != value) {
                    _selectedTableMapping = value;
                    if (_selectedTableMapping != null) {
                        foreach (var tableMapping in TableMappings) {
                            tableMapping.Used = tableMapping == _selectedTableMapping;
                        }

                        if (!RuleSelectionDetailsModels.ContainsKey(_selectedTableMapping))
                            RuleSelectionDetailsModels[_selectedTableMapping] = new RuleSelectionDetailsModel(_selectedTableMapping, RuleSelectionDetailsModels.Count) { TextColor = Brushes.White };
                        SelectedRuleSelectionDetailsModel = RuleSelectionDetailsModels[_selectedTableMapping];
                        ResultsModel.SetTableMappingResult(_selectedTableMapping);
                        if (ResultsModel != null && ResultsModel.ValidationResults != null) {
                            UpdateResultDataContexts();
                        }
                    }
                    if(ListViewTableMappingModel != null) ListViewTableMappingModel.SelectedItem = _selectedTableMapping;
                    //ResultsModel.CurrentTableResult = ResultsModel.SelectedTableResult
                    OnPropertyChanged("SelectedRuleSelectionDetailsModel");
                    OnPropertyChanged("SelectedTableMapping");
                    OnPropertyChanged("TableMappingSelectionVisibility");
                }
            }
        }
        #endregion

        #region RuleSelectionDetailsModel
        private RuleSelectionDetailsModel _selectedRuleSelectionDetailsModel;

        public RuleSelectionDetailsModel SelectedRuleSelectionDetailsModel {
            get { return _selectedRuleSelectionDetailsModel; }
            set {
                if (_selectedRuleSelectionDetailsModel != value) {
                    _selectedRuleSelectionDetailsModel = value;
                    OnPropertyChanged("SelectedRuleSelectionDetailsModel");
                }
            }
        }
        #endregion

        private Dictionary<TableMapping, RuleSelectionDetailsModel> RuleSelectionDetailsModels { get; set; }

        public ListViewTableMappingModel ListViewTableMappingModel { get; set; }
        public ObservableCollection<ProfileConfig> Profiles { get; set; }
        public DlgMainWindow DlgMainWindow { get; set; }
        private ViewValidatorLogic.Logic.ViewValidator _viewValidator;
        private DlgPopupProgress _dlgPopupProgress;
        public bool TableMappingSelectionVisibility { get { return TableMappings != null && TableMappings.Count > 0; } }
        public bool ProfileSelectionVisibility { get { return Profiles != null && Profiles.Count > 0; } }
        private readonly NavigationTreeEntry _viewEntry;
        public Visibility RuleAssignmentWindowVisibility { get { return _dlgRuleAssignment.Visibility; } }
        private DlgRuleAssignment _dlgRuleAssignment;
        private RuleAssignmentModel _ruleAssignmentModel;
        public NavigationTreeEntry ResultNavigationEntry { get; set; }

        #endregion

        #region Methods
        private void UpdateResultDataContexts() {
            ResultRowsMissingCtl.DataContext = ResultsModel.CurrentTableResult;
            ResultRowsDifferentCtl.DataContext = ResultsModel.CurrentTableResult;
            ResultRowsDifferentColumnWiseCtl.DataContext = new RowsDifferentColumnWiseModel(ResultsModel.CurrentTableResult);
            
        }

        public void RemovedTableMapping(TableMapping removedTableMapping) {
            if (TableMappings != null && TableMappings.Count > 0 && (SelectedTableMapping == removedTableMapping || SelectedTableMapping == null))
                SelectedTableMapping = TableMappings[0];
        }

        public void DeletedProfile(ProfileConfig profileConfig) {
            if (Profiles.Count > 0) SelectedProfile = Profiles[0];
            else SelectedProfile = null;
        }

        internal void Update(ViewValidatorLogic.Logic.ViewValidator viewValidator) {
            ResultsModel.NewResults(viewValidator.Results);
            ResultsModel.SetTableMappingResult(SelectedTableMapping);
            OnPropertyChanged("ResultsModel");
            //ResultsOverviewCtl.DataContext = new ResultOverviewModel(viewValidator.Results);
            //ResultsOverviewCtl.NewResults();
            UpdateResultDataContexts();
        }

        public void AddNewView() {
            if (ListViewTableMappingModel.AddItem()) {
                _viewEntry.IsSelected = true;
                _viewEntry.IsExpanded = true;
                _viewEntry.Parent.IsExpanded = true;
            }
        }

        #region Validate
        public void ValidateInMemory() {
            Validate(SortMethod.Memory);
        }

        public void Validate() {
            Validate(SortMethod.Database);
        }

        public void Validate(SortMethod sortMethod) {
            if (SelectedTableMapping == null) {
                MessageBox.Show(DlgMainWindow, "Keine View zum Validieren ausgewählt.");
                return;
            }

            foreach (var tableMapping in SelectedProfile.ValidationSetup.TableMappings) {
                tableMapping.Used = tableMapping.DisplayString == SelectedTableMapping.DisplayString;
            }

            ProfileManager.Save(SelectedProfile);
            try {

                ResultsModel.ResultTableDetailsModel.HasValidationFinished = false;

                ProgressCalculator progressCalculator = new ProgressCalculator();
                
                progressCalculator.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
                progressCalculator.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
                //progressCalculator.ProgressChanged += new ProgressChangedEventHandler(bgWorker_ProgressChanged);
                progressCalculator.WorkerReportsProgress = true;

                _viewValidator = new ViewValidatorLogic.Logic.ViewValidator(SelectedProfile.ValidationSetup, progressCalculator);
                //ViewValidator = new ViewValidatorLogic.Logic.ViewValidator(validationSetup, bgWorker);
                _dlgPopupProgress = new DlgPopupProgress() { Owner = DlgMainWindow, DataContext = progressCalculator };
                progressCalculator.RunWorkerAsync(sortMethod);
                bool? showDialog = _dlgPopupProgress.ShowDialog();
                if (showDialog != null && !showDialog.Value) {
                    //User aborted operation
                    _viewValidator.AbortOperation();
                }

                ResultsModel.ResultTableDetailsModel.HasValidationFinished = true;
            } catch (Exception ex) {
                MessageBox.Show(DlgMainWindow, "Ein Fehler ist aufgetreten:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        #endregion

        #region BgWorker Stuff
        //void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
        //    _dlgPopupProgress.UpdateProgress(e.ProgressPercentage);
        //}

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            //_dlgPopupProgress.UpdateProgress(75);
            if (e.Error != null && e.Error is OperationCanceledException) return;
            if (e.Error != null) {
                MessageBox.Show(DlgMainWindow, "Es ist ein Fehler aufgetreten:" + Environment.NewLine + e.Error.Message,
                                "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                _dlgPopupProgress.DialogResult = true;

            Update(_viewValidator);
            //UpdateHasResults();
            _dlgPopupProgress.Close();
        }

        void bgWorker_DoWork(object sender, DoWorkEventArgs e) {
            SortMethod sortMethod = (SortMethod) e.Argument;
            _viewValidator.StartValidation(sortMethod);
        }
        #endregion

        internal void ShowRuleAssignmentWindow() {
            if (!_dlgRuleAssignment.IsVisible) {
                _dlgRuleAssignment.Visibility = Visibility.Visible;
                _dlgRuleAssignment.Focus();
            }
        }
        #endregion

        #region EventHandler
        void _ruleAssignmentWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            OnPropertyChanged("RuleAssignmentWindowVisibility");
        }
        #endregion

        #region ExportToExcel
        internal void ExportToExcel() {
            if (!ResultsModel.ResultsCalculated) {
                MessageBox.Show(DlgMainWindow, "Aktuell sind keine Ergebnisse zum Export vorhanden.");
                return;
            }
            DlgExcelExport dlgExcelExport = new DlgExcelExport(){Owner = DlgMainWindow};
            bool? showDialog = dlgExcelExport.ShowDialog();
            if (showDialog != null && showDialog.Value) {
                try {
                    ProgressCalculator exportToExcelWorker = new ProgressCalculator();
                    exportToExcelWorker.DoWork += exportToExcelWorker_DoWork;
                    exportToExcelWorker.RunWorkerCompleted += exportToExcelWorker_RunWorkerCompleted;
                    //exportToExcelWorker.ProgressChanged += bgWorker_ProgressChanged;
                    exportToExcelWorker.WorkerReportsProgress = true;

                    _dlgPopupProgress = new DlgPopupProgress() { Owner = DlgMainWindow, DataContext=exportToExcelWorker };
                    exportToExcelWorker.RunWorkerAsync();
                    _dlgPopupProgress.ShowDialog();
                } catch (Exception ex) {
                    MessageBox.Show(DlgMainWindow, "Ein Fehler ist aufgetreten:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #region ExportToExcelWorker
        void exportToExcelWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if (e.Error != null)
                MessageBox.Show(DlgMainWindow, "Es ist ein Fehler aufgetreten:" + Environment.NewLine + e.Error.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                MessageBox.Show(DlgMainWindow, "Excel Datei erfolgreich exportiert", "", MessageBoxButton.OK, MessageBoxImage.Information);
            _dlgPopupProgress.Close();
        }


        void exportToExcelWorker_DoWork(object sender, DoWorkEventArgs e) {
            DataGridHelper.ExportResultsToExcel(ResultsModel.CurrentTableResult, 
                ApplicationManager.ApplicationConfig.LastExcelFile, sender as ProgressCalculator);
        }
        #endregion
        #endregion

    }
}
