// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2010-10-11
// copyright 2010 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using AvdWpfControls;
using Utils;
using eBalanceKit.Models;
using eBalanceKit.Structures;
using eBalanceKit.Windows.FederalGazette;
using eBalanceKit.Windows.Import;
using eBalanceKit.Windows.Management.Add;
using eBalanceKit.Windows.Management.Delete;
using eBalanceKit.Windows.Management.Delete.Models;
using eBalanceKit.Windows.Management.Management;
using eBalanceKit.Windows.Reconciliation;
using eBalanceKit.Windows.Security.Models;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitBusiness.Structures;
using eBalanceKit.Windows.Security;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows {
    public partial class MainWindow {
        #region constructor
        public MainWindow() {
            // avoid disposing the application config, unless the initalization process has not been succeed
            _keepAppConfig = true;

            _infoGridRowHeight = new GridLength(160, GridUnitType.Pixel);
            _infoGridRowMinHeight = 0;

            InitializeComponent();

            CtlMonetarySources.GiveFeedback += dragItem_GiveFeedback;

            GlobalResources.MainWindow = this;

            DataContextChanged += (sender, args) => {
                if (RightManager.RightDeducer.HasAnyGrantRight) btnUserManagement.Visibility = Visibility.Visible;
            };
            #if !DEBUG
                btnImportBalanceList.Visibility = Visibility.Collapsed;
            #endif
        }
        #endregion constructor

        #region Init
        public void Init() {
            
            Model = new MainWindowModel(this);
            DataContext = Model;

            // initalization succeed without errors
            _keepAppConfig = false;

            // select first system if only one system does exist
            // must be done after DataContext is set to avoid gui bugs (e.g. the .ReconciliationInfo control 
            // is visible unless the initialisation process has been finished)
            if (Model.Systems.Count == 1) {
                Model.SelectedSystem = Model.Systems[0] as eBalanceKitBusiness.Structures.DbMapping.System;
            }
            
            try {
                Model.LoadEnvironment();
            } catch (Exception) {
                MessageBox.Show("The environment couldn't be loaded correctly", ResourcesCommon.Error);
            }

            //DocumentManager.Instance.InitRemainingDocuments(Model.RefreshCompanies);

            //Start the setup assistant when you haven't got any documents
            //TODO: This sleep is only a workaround
            //Thread.Sleep(1000);
            if (DocumentManager.Instance.AllowedDocuments.Count == 0) {
                new Management.ManagementAssistant.DlgManagementAssistant() { Owner = this }.ShowDialog();
            }
        }
        #endregion Init

        #region event handler

        internal void dragItem_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
            var data = DragDropPopup.DataContext as AccountListDragDropData;
            if (data == null) return;
            else if (data.AllowDrop) popupBorder.Opacity = 1.0;
            else popupBorder.Opacity = 0.5;
        }

        internal void Window_PreviewDragOver(object sender, DragEventArgs e) {
            if (DragDropPopup.IsOpen) {
                var popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
                DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
            }
        }

        private void Window_Closed(object sender, System.EventArgs e) {
            DocumentManager.Instance.RequestCancellation();
            Thread.Sleep(100);
            if (!_keepAppConfig) AppConfig.Dispose();
            if(Model != null) Model.SaveEnvironment();
        }

        #region info expander
        private void Expander_Expanded(object sender, RoutedEventArgs e) {
            InfoGridRow.Height = _infoGridRowHeight;
            InfoGridRow.MinHeight = _infoGridRowMinHeight;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e) {
            _infoGridRowHeight = InfoGridRow.Height;
            InfoGridRow.Height = GridLength.Auto;
            InfoGridRow.MinHeight = 0;
        }
        #endregion info expander

        #region nav bar buttons
        private void btnValidate_Click(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            Model.Validate();
        }

        private void btnTemplates_Click(object sender, RoutedEventArgs e) {
            Model.ShowTemplateDialog();
        }

        private void btnSendData_Click(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            if (!RightManager.SendAllowed(Model.CurrentDocument)) {
                MessageBox.Show(this, ResourcesSend.NotSendAllowed, string.Empty, MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            var dlg = new DlgSendData(Model.CurrentDocument) {Owner = this};
            dlg.ShowDialog();
        }

        private void btnSendDataFederalGazette_Click(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            if (!RightManager.SendAllowed(Model.CurrentDocument)) {
                MessageBox.Show(this, ResourcesSend.NotSendAllowedFederal, string.Empty,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                return;
            }

            var dlg = new DlgFederalGazettePreview(Model.CurrentDocument) {Owner = this};
            dlg.ShowDialog();
        }

        private void btnDisconnect_Click(object sender, RoutedEventArgs e) {
            DocumentManager.Instance.RequestCancellation();
            Thread.Sleep(100);
            DocumentManager.Reinitialize();
            new Login().Show();
            _keepAppConfig = true;
            Model.SaveEnvironment();
            UserManager.Instance.Logoff();
            Close();
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e) { new DlgInfo {Owner = this}.ShowDialog(); }
        #endregion  nav bar buttons

        private void btnEditCurrentUser_Click(object sender, RoutedEventArgs e) {
            if (UserManager.Instance.CurrentUser.IsDomainUser) {
                var owner = UIHelpers.TryFindParent<Window>(this);
                CtlEditDomainUser dlg = new CtlEditDomainUser {
                    Owner = owner,
                    DataContext =
                        new CtlSecurityManagementModel {UserListSelectedItem = UserManager.Instance.CurrentUser}
                };
                dlg.ShowDialog();
            } else
                new DlgEditUser {
                    Owner = this,
                    DataContext =
                        new CtlSecurityManagementModel {UserListSelectedItem = UserManager.Instance.CurrentUser}
                }.
                    ShowDialog();
        }

        private void BtnHelpClick(object sender, RoutedEventArgs e) { new DlgHelp {Owner = this}.ShowDialog(); }
        private void BtnAuditModeClick(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            new AuditMode.DlgTaxAudit {Owner = this}.ShowDialog();
        }
        private void BtnUserManagementClick(object sender, RoutedEventArgs e) { new DlgUserManagement { Owner = GlobalResources.MainWindow }.ShowDialog(); }

        private void BtnReconciliationClick(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            DlgReconciliation dlgReconciliation = new DlgReconciliation {
                Owner = this,
                DataContext = new ReconciliationsModel(Model.CurrentDocument)
            };
            dlgReconciliation.SetGlobalSearchContext(dlgReconciliation.DataContext);
            dlgReconciliation.ShowDialog();
        }

        #endregion event handler

        #region fields

        private readonly double _infoGridRowMinHeight;
        private GridLength _infoGridRowHeight;

        /// <summary>
        /// Indicates wehther to the application config should be disposed if this instance is closed.
        /// </summary>
        private bool _keepAppConfig;

        internal MainWindowModel Model { get; set; }

        private void navSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e) { Model.SelectedNavigationEntry = (sender as HierarchicalTabControl).SelectedItem as NavigationTreeEntry; }

        private void btnGlobalConfig_Click(object sender, RoutedEventArgs e) { new DlgGlobalOptions(Model) {Owner = this}.ShowDialog(); }

        private readonly Stopwatch _sizeChangedTimer = new Stopwatch();

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (!_sizeChangedTimer.IsRunning) {
                content.Visibility = Visibility.Collapsed;
                new Thread(ShowContent).Start();
            }

            _sizeChangedTimer.Reset();
            _sizeChangedTimer.Start();
        }

        private void ShowContent() {
            while (_sizeChangedTimer.Elapsed.Milliseconds < 200)
                Thread.Sleep(10);

            _sizeChangedTimer.Reset();

            Dispatcher.BeginInvoke(new Action(() => {
                content.Visibility = Visibility.Visible;
            }), DispatcherPriority.Render);
        }

        #endregion fields

        private void BtnSetupAssistantButtonClick(object sender, RoutedEventArgs e) { new Management.ManagementAssistant.DlgManagementAssistant() { Owner = GlobalResources.MainWindow }.ShowDialog(); }
        private void BtnAddSystemButtonClick(object sender, RoutedEventArgs e) { new DlgAddSystem { Owner = GlobalResources.MainWindow }.ShowDialog(); }
        private void BtnAddCompanyButtonClick(object sender, RoutedEventArgs e) { new DlgAddCompany { Owner = GlobalResources.MainWindow }.ShowDialog(); }
        private void BtnAddReportButtonClick(object sender, RoutedEventArgs e) { new DlgAddReport { Owner = GlobalResources.MainWindow }.ShowDialog(); }

        private void BtnDeleteSystemButtonClick(object sender, RoutedEventArgs e) {
            var dlg = new DlgDeleteList(ResourcesManamgement.DlgDeleteSystemCaption, ResourcesManamgement.AvailableSystemsCaption,
                              "deleteSystem16.png", null) { Owner = GlobalResources.MainWindow };
            dlg.DataContext = new DeleteSystemModel(dlg);
            dlg.ShowDialog();
        }

        private void BtnDeleteCompanyButtonClick(object sender, RoutedEventArgs e) {
            var dlg = new DlgDeleteList(ResourcesManamgement.DlgDeleteCompanyCaption, ResourcesManamgement.AvallableCompaniesCaption,
                  "deleteCompany16.png", null) { Owner = GlobalResources.MainWindow };
            dlg.DataContext = new DeleteCompanyModel(dlg);
            dlg.ShowDialog();
        }

        private void BtnDeleteReportButtonClick(object sender, RoutedEventArgs e) {
            var dlg = new DlgDeleteList(ResourcesManamgement.DlgDeleteReportCaption, ResourcesManamgement.AvailableReportsCaption,
                  "deleteReport16.png", null) { Owner = GlobalResources.MainWindow };
            dlg.DataContext = new DeleteReportModel(dlg);
            dlg.ShowDialog();
        }

        private void BtnImportCompanyDataButtonClick(object sender, RoutedEventArgs e) { new DlgImportOrSampleFileSelectionCompany().ShowDialog(); }

        private void BtnSystemManagementButtonClick(object sender, RoutedEventArgs e) {
            new DlgManagementList(ResourcesManamgement.DlgSystemManagementCaption, ResourcesManamgement.AvailableSystemsCaption, "SystemManagement16.png",
                              SystemManager.Instance.Systems) { Owner = GlobalResources.MainWindow }.ShowDialog();
        }

        private void BtnCompanyManagementButtonClick(object sender, RoutedEventArgs e) {
            new DlgManagementList(ResourcesManamgement.DlgCompanyManagementCaption, ResourcesManamgement.AvallableCompaniesCaption, "CompanyManagement16.png",
                  CompanyManager.Instance.AllowedCompanies) { Owner = GlobalResources.MainWindow }.ShowDialog();

        }
        
        private void BtnReportManagementButtonClick(object sender, RoutedEventArgs e) { new DlgReportManagement() { Owner = GlobalResources.MainWindow }.ShowDialog(); }

        private void BtnPdfExportButtonClick(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            Model.Export(ExportTypes.Pdf, exportLikeXbrl: false);
        }

        private bool CheckCurrentDocumentValid() {
            if (Model.CurrentDocument == null) {
                MessageBox.Show(this, ResourcesSend.SelectReport, string.Empty, MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void BtnPdfLikeXbrlExportButtonClick(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            Model.Export(ExportTypes.Pdf, exportLikeXbrl: true);
        }
        private void BtnCsvExportClick(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            Model.Export(ExportTypes.Csv, exportLikeXbrl: false);
        }
        private void BtnXbrlExportClick(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            Model.Export(ExportTypes.Xbrl, exportLikeXbrl: false);
        }

        private void BtnImportBalanceListClick(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            new BalanceList.BalListImportAssistant(Model.CurrentDocument, GlobalResources.MainWindow) { Owner = GlobalResources.MainWindow }.ShowDialog();
        }
        
        private void BtnCopyReport(object sender, RoutedEventArgs e) {
            if (!CheckCurrentDocumentValid()) return;
            try {
                new DlgProgress(this) {
                    ProgressInfo = { IsIndeterminate = true, Caption = "Kopieren des Berichts" }
                }.ExecuteModal(Model.CurrentDocument.Copy);
                Model.CurrentDocument = DocumentManager.Instance.Documents.Last();
                MessageBox.Show(this, "Bericht wurde erfolgreich kopiert", "", MessageBoxButton.OK,
                                MessageBoxImage.Information);
            } catch (Exception ex) {
                MessageBox.Show(this, "Fehler beim Kopieren des Berichts: " + ex.Message, "", MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
    }
}