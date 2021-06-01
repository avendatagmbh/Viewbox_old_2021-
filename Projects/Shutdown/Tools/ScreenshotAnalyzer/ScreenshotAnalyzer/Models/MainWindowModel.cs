// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-11-22
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using ScreenshotAnalyzer.Controls;
using ScreenshotAnalyzer.Controls.Results;
using ScreenshotAnalyzer.Models.ListView;
using ScreenshotAnalyzer.Models.Results;
using ScreenshotAnalyzer.Resources.Localisation;
using ScreenshotAnalyzer.Structures.Navigation;
using ScreenshotAnalyzer.Windows;
using ScreenshotAnalyzerBusiness.Manager;
using ScreenshotAnalyzerBusiness.Structures;
using ScreenshotAnalyzerBusiness.Structures.Config;
using ScreenshotAnalyzerBusiness.Structures.Results;
using Utils;

namespace ScreenshotAnalyzer.Models {
    public class MainWindowModel : NotifyPropertyChangedBase{
        #region Constructor

        public MainWindowModel(DlgMainWindow dlgMainWindow) {
            DlgMainWindow = dlgMainWindow;

            //Init navigation
            NavigationTree = new NavigationTree(dlgMainWindow);
            NavigationTreeEntry generalEntry = NavigationTree.AddEntry(ResourcesGui.MainWindowModel_Navigation_Management, null);
            NavigationTree.AddEntry(ResourcesGui.MainWindowModel_Navigation_Profile, new CtlListView(new ListViewProfileModel(this) { Owner = DlgMainWindow }), generalEntry);
            NavigationTree.AddEntry(ResourcesGui.MainWindowModel_Navigation_ScreenshotGroups, new CtlListView(new ListViewTableModel(this) {Owner = DlgMainWindow}),generalEntry);
            CtlSelectOcrAreas = new CtlSelectOcrAreas();
            NavigationTree.AddEntry(ResourcesGui.MainWindowModel_Navigation_Textselection, CtlSelectOcrAreas).IsSelected = true;
            TextRecognitionResultModel = new TextRecognitionResultModel(this);
            NavigationTree.AddEntry(ResourcesGui.MainWindowModel_Navigation_RecognitionResult, new CtlTextRecognitionResult(){DataContext = TextRecognitionResultModel});
            generalEntry.IsExpanded = true;


            //_screenshotGroup = new ScreenshotGroup();
            ScreenshotListModel = new ScreenshotListModel();
            ScreenshotListModel.PropertyChanged += ScreenshotListModel_PropertyChanged;
            SelectOcrAreasModel = new SelectOcrAreasModel(this);
            CtlSelectOcrAreas.DataContext = SelectOcrAreasModel;

            var lastProfile = ProfileManager.GetProfile(ApplicationManager.ApplicationConfig.LastProfile);
            if (lastProfile != null) {
                SelectedProfile = lastProfile;

            } else if (SelectedProfile == null && Profiles != null && Profiles.Count > 0)
                SelectedProfile = Profiles[0];

        }


        #endregion Constructor

        #region Properties
        public DlgMainWindow DlgMainWindow { get; set; }
        public NavigationTree NavigationTree { get; set; }
        private CtlSelectOcrAreas CtlSelectOcrAreas { get; set; }

        //private ScreenshotGroup _screenshotGroup;
        public ScreenshotListModel ScreenshotListModel { get; private set; }
        public SelectOcrAreasModel SelectOcrAreasModel { get; private set; }
        public TextRecognitionResultModel TextRecognitionResultModel { get; set; }


        #region SelectedProfile
        private Profile _selectedProfile;

        public Profile SelectedProfile {
            get { return _selectedProfile; }
            set {
                if (_selectedProfile != value) {
                    _selectedProfile = value;
                    //SelectedTable = null;

                    if (_selectedProfile != null) {
                        if (_selectedProfile.IsStatusOk) {
                            try {
                                _selectedProfile.Load();
                            } catch (Exception ex) {
                                MessageBox.Show(DlgMainWindow,
                                                string.Format(ResourcesGui.MainWindowModel_SelectedProfile_Error_LoadingProfile, _selectedProfile.Name,Environment.NewLine + ex.Message));
                            }
                            if (_selectedProfile.Tables.Count > 0)
                                SelectedTable = _selectedProfile.Tables[0];
                            //ProfileModel profileModel = ProfileModelManager.GetModel(SelectedProfile);
                        }
                    }

                    OnPropertyChanged("SelectedProfile");
                    OnPropertyChanged("ProfileSelectionVisibility");
                }
            }
        }
        #endregion SelectedProfile

        #region SelectedTable
        private Table _selectedTable;
        public Table SelectedTable {
            get { return _selectedTable; }
            set {
                if (_selectedTable != value) {
                    _selectedTable = value;
                    if (_selectedTable != null) {
                        SelectedScreenshotGroup = _selectedTable.Groups[0];
                    }
                    ScreenshotListModel.Screenshots = _selectedTable == null ? null : SelectedScreenshotGroup;
                    SelectOcrAreasModel.ScreenshotGroup = _selectedTable == null ? null : SelectedScreenshotGroup;
                    OnPropertyChanged("SelectedTable");
                }
                TableExists = _selectedProfile != null && _selectedProfile.Tables.Count > 0;
            }
        }
        #endregion

        public ScreenshotGroup SelectedScreenshotGroup { get; set; }
        public ObservableCollectionAsync<Profile> Profiles { get { return ProfileManager.Profiles; } }

        #region TableExists
        private bool _tableExists;
        public bool TableExists {
            get { return _tableExists; }
            set {
                if (_tableExists != value) {
                    _tableExists = value;
                    OnPropertyChanged("TableExists");
                }
            }
        }
        #endregion TableExists

        #endregion Properties

        #region EventHandler
        void ScreenshotListModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "SelectedScreenshot") {
                SelectOcrAreasModel.Screenshot = ScreenshotListModel.SelectedScreenshot;
            }
        }
        #endregion EventHandler

        public void AddedProfile(Profile newProfile) {
            //throw new NotImplementedException();
        }

        public void DeletedProfile(Profile profile) {
            if (Profiles.Count > 0)
                SelectedProfile = Profiles[0];
            else SelectedProfile = null;
        }

        #region ExportAccessDatabase
        public void ExportAccessDatabase() {
            if (SelectedTable == null) throw new InvalidOperationException(ResourcesGui.MainWindowModel_ExportAccessDatabase_Error_NoScreenshotGroupSelected);
            if (SelectedTable.RecognitionResult == null) throw new InvalidOperationException(ResourcesGui.MainWindowModel_ExportAccessDatabase_Error_NoRecognitionResultPresent);
            if (string.IsNullOrEmpty(SelectedProfile.AccessPath)) throw new InvalidOperationException(ResourcesGui.MainWindowModel_ExportAccessDatabase_Error_NoAccessOutputPathSelected);
            if (string.IsNullOrEmpty(SelectedTable.TableName)) throw new InvalidOperationException(ResourcesGui.MainWindowModel_ExportAccessDatabase_Error_NoTablenameSelected);
            AccessExporter.Export(SelectedProfile.AccessPath, SelectedTable);
        }
        #endregion ExportAccessDatabase
    }
}
