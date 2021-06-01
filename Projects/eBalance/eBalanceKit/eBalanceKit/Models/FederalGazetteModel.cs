// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-11-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Utils.Commands;
using eBalanceKit.Models.FederalGazette;
using eBalanceKit.Windows.EditPresentationTreeDetails;
using eBalanceKit.Windows.Management.ManagementAssistant.Model;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness;
using federalGazetteBusiness;
using eBalanceKitResources.Localisation;
using System.Linq;
using federalGazetteBusiness.Structures;
using federalGazetteBusiness.Structures.Enum;
using federalGazetteBusiness.Structures.Manager;

namespace eBalanceKit.Models {

    /// <summary>
    /// DlgAssisstant dialog navigation bar's tabs.
    /// </summary>
    public enum FederalGazetteAssistantTabs
    {
        Login,
        NotDefined,
        CompanySize,
        ReportOptions,
        TreeView,
        Init
    }

    public class FederalGazetteModel : Utils.NotifyPropertyChangedBase {
        private const string TaxonomyIdMgmtReport = "de-gaap-ci_mgmtRep";
        private const string TaxonomyIdNotes = "de-gaap-ci_nt";
        private FederalGazetteOrderManager _manager;
        public FederalGazetteModel(eBalanceKitBusiness.Structures.DbMapping.Document document, Window owner) {
            //PreviewString = GenerateHtmlStringMessage(ResourcesFederalGazette.PreviewNotAvailable);
            Owner = owner;
            this.document = document;

            document.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) {
                if (args.PropertyName.Equals("ValueTreeMainFg") && _manager == null) {
                    Init();
                }
            };

            TransferDataModel = new FederalGazetteCopyValuesQueryModel() {
                CmdYes = new DelegateCommand(o => true, o => LoadData(true)),
                CmdNo = new DelegateCommand(o => true, o => LoadData(false))
            };

            //LoadData();
            Init();
        }



        public federalGazetteBusiness.Structures.FederalGazetteElementList SendParameterList { get { return _manager == null ? null : _manager.SendParameter; } }

        #region TransferDataModel
        private IAskingModel _transferDataModel;

        public IAskingModel TransferDataModel {
            get { return _transferDataModel; }
            set {
                if (_transferDataModel != value) {
                    _transferDataModel = value;
                    OnPropertyChanged("TransferDataModel");
                }
            }
        }
        #endregion // TransferDataModel

        private Window Owner { get; set; }

        private void LoadData(object transfer = null) {
            if (transfer == null) {
                var msgBoxResult = MessageBox.Show(Owner, ResourcesFederalGazette.OvertakeDataQuestion,
                                                   ResourcesFederalGazette.OvertakeDataCaption,
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Question);
                transfer = msgBoxResult == MessageBoxResult.Yes;
                //transfer = false;
            }
            bool transferValues;
            if (!bool.TryParse(transfer.ToString(), out transferValues)) {
                Debug.Fail("there should be a parameter or an Answer!");
                return;
            }

            var msg = transferValues
                          ? "Übertragen der Daten von der E-Bilanz in den Bericht für den Bundesanzeiger"
                          : "Vorbereiten der Berichterstellung für den Bundesanzeiger";
            //var owner = Owner;

            try {

                new DlgProgress(Owner) {
                    ProgressInfo = { IsIndeterminate = true, Caption = msg }
                }.ExecuteModal(InitFederalGazette, transferValues);
            }
            catch (Exception) {
                new DlgProgress(GlobalResources.MainWindow) {
                    ProgressInfo = {IsIndeterminate = true, Caption = msg}
                }.ExecuteModal(InitFederalGazette, transferValues);
                //throw;
            }

            DataLoaded = true;
        }


        private void InitFederalGazette(object param) {
            bool p;
            if (param == null || !bool.TryParse(param.ToString(), out p)) {
                return;
            }
            document.InitFederalGazette(p);
        }

        private eBalanceKitBusiness.Structures.DbMapping.Document document;

        private void Init() {

            _manager = new FederalGazetteOrderManager();
            //var x = manager.GetPreview("","","","");

            var mgmtReportValue = document.ValueTreeMainFg.GetValue(TaxonomyIdMgmtReport);
            var notesValue = document.ValueTreeMainFg.GetValue(TaxonomyIdNotes);

            System.Diagnostics.Debug.Assert(mgmtReportValue != null);
            System.Diagnostics.Debug.Assert(notesValue != null);

            //TreeViewBalance = document.GaapPresentationTreesFg["0"];
            //TreeViewBalance = document.GaapPresentationTreesFg.Values.FirstOrDefault(tree => tree.Nodes.Any(node => node.Element.Name.Equals("bs.ass")));
            TreeViewBalance = document.GetPresentationTreeBalance(document.GaapPresentationTreesFg);
            TreeViewIncomeStatement = document.GetPresentationTreeIncomeStatement(document.GaapPresentationTreesFg);
            //TreeViewIncomeStatement = document.GaapPresentationTreesFg["0"];

            TreeViewBalanceModel = new FederalGazette.FederalGazetteTreeViewModel(TreeViewBalance, _manager, this);
            TreeViewIncomeStatementModel = new FederalGazette.FederalGazetteTreeViewModel(TreeViewIncomeStatement, _manager, this);

            RtfModelMgmtReport = new PresentationTreeDetailModelRtf(document, mgmtReportValue);
            RtfModelNotes = new PresentationTreeDetailModelRtf(document, notesValue);
            SendModel = new FederalGazetteSendModel(document.Company);

            SendModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) {
                //if (args.PropertyName.Equals("User")) {
                _manager.LoginData.Username = SendModel.User;
                _manager.LoginData.Password = SendModel.Password;
                //}
            };


#if DEBUG
            SendModel.User = "DemoEinsender";
            SendModel.Password = "tx12pr97";
            SendModel.CompanyId = "2400000008";
#endif

            //document.ValueTreeMainFg.Root.Values.Values.pr


            document.ValueTreeMainFg.Root.PropertyChanged += (sender, args) => RefreshPreview(FederalGazetteAssistantTabs.Init);
            TreeViewBalance.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { RefreshPreview(FederalGazetteAssistantTabs.Init); OnPropertyChanged("ValidBalance"); };


            //document.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) { RefreshPreview(); };

            //foreach (eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode node in TreeViewBalance.Nodes) {
            //    node.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) {
            //        if (!args.PropertyName.Equals("Value")) {
            //            return;
            //        }
            //        RefreshPreview();
            //    };
            //}

            foreach (var value in document.ValueTreeMainFg.Root.Values.Values) {
                value.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) {
                    if (!args.PropertyName.Equals("Value")) {
                        return;
                    }
                    //RefreshPreview();
                    OnPropertyChanged("ValidBalance");
                };
            }
            _valueTree = document.ValueTreeMainFg;

            //_manager.SendParameter.CompanySize.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args) {
            //    var pSize = GetSelectedCompanySize();
            //    CompanySize = pSize;
            //    //TreeViewBalanceModel.SetCheckedItemsByCompanySize(pSize, true, true);
            //    //TreeViewIncomeStatementModel.SetCheckedItemsByCompanySize(pSize, true, false);

            //    //RefreshPreview(FederalGazetteAssistantTabs.CompanySize);
            //};
            OnPropertyChanged("SendParameterList");
        }

        private CompanySize GetSelectedCompanySize() {
            CompanySize result = CompanySize.Small;
            var value = _manager.SendParameter.CompanySize as federalGazetteBusiness.Structures.ValueTypes.FederalGazetteElementSelectionBase;
            if (value != null && value.Value != null) {
                //switch (value.SelectedOption.Value.ToString()) {
                //    case "small":
                //        return CompanySize.Small;
                //    case "small":
                //        return CompanySize.Midsize;
                //    case "small":
                //        return CompanySize.Small;
                //}
                try {
                    result = StringToEnum<CompanySize>(value.Value.ToString());
                } catch (Exception e) {
                    eBalanceKitBase.Structures.ExceptionLogging.LogException(e);
                }
                return result;
            }
            Debug.Fail("Check requiered why Company Size should not be set.");
            return result;
        }

        private void SetSelectedCompanySize(CompanySize companySize) {
            var value = _manager.SendParameter.CompanySize as federalGazetteBusiness.Structures.ValueTypes.FederalGazetteElementSelectionBase;
            if (value != null) {
                value.SelectedOption =
                    value.Options.FirstOrDefault(
                        option => option.ElementName.ToLower() == companySize.ToString().ToLower());
            }
        }

        public static T StringToEnum<T>(string name) {
            if (!Enum.IsDefined(typeof(T), name)) {
                Debug.Fail("Converting of Enum failed! " + name + " in " + typeof(T).ToString());
            }
            return (T)Enum.Parse(typeof(T), name, true);
        }

        #region GetValueTreeMainDecimalValue
        /// <summary>
        /// Returns the DecimalValue for the specified entry in ValueTreeMain.
        /// In debug mode a new exception will be thrown instead of returning 0 if the valueTreeMainRootValueId is not existing in the ValueTreeMain.
        /// </summary>
        /// <param name="taxonomyId">The Id to identify the ValueTreeMain entry.</param>
        /// <returns>The decimal value. (0 if not set)</returns>
        private Decimal GetValueTreeDecimalValue(string taxonomyId) {
            IValueTreeEntry tmp = _valueTree.GetValue(taxonomyId);
            if (tmp == null) {
#if DEBUG
                //throw new Exception("ValueTreeMain has no value with ID " + valueTreeMainRootValueId);
#endif
                return 0;
            }

            return tmp.DecimalValue;
        }
        #endregion GetValueTreeMainDecimalValue

        private eBalanceKitBusiness.Structures.ValueTree.ValueTree _valueTree;

        public bool ValidBalance { get {
            if (_valueTree == null || _valueTree.Document == null || _valueTree.Document.TaxonomyPart == null) {
                return true;
            }
            var balanceDifference =
                Decimal.Subtract(GetValueTreeDecimalValue(_valueTree.Document.TaxonomyPart.BalanceSheetAss),
                                 GetValueTreeDecimalValue(_valueTree.Document.TaxonomyPart.BalanceSheetEqLiab));

            return balanceDifference.CompareTo(new decimal(0)) == 0;
        } }

        #region CompanySize
        //ert
        //09-11-2012
        private CompanySize _companySize;
        /// <summary>
        /// Gets or sets the size of the company and set this value for 
        /// _treeViewBalanceModel and _treeViewIncomeStatementModel too.
        /// </summary>
        /// <value>
        /// The size of the company.
        /// </value>
        public CompanySize CompanySize
        {
            get { return _companySize; }
            set 
            {
                if (_companySize != value)
                {
                    _companySize = value;
                    _treeViewBalanceModel.CompanySize = value;
                    _treeViewIncomeStatementModel.CompanySize = value;
                    SetSelectedCompanySize(value);
                    //SetSelectedCompanySize(value);
                    OnPropertyChanged("CompanySize");
                }
            }
        }
        #endregion

        #region DataLoaded
        private bool _dataLoaded;

        public bool DataLoaded {
            get { return _dataLoaded; }
            set {
                if (_dataLoaded != value) {
                    _dataLoaded = value;
                    OnPropertyChanged("DataLoaded");
                }
            }
        }
        #endregion // DataLoaded

        #region CmdClosePreview
        private DelegateCommand _cmdClosePreview;

        public DelegateCommand CmdClosePreview {
            get { return _cmdClosePreview ?? (_cmdClosePreview = new DelegateCommand(o => true, o => ShowPreview = false)); }
            //set {
            //    if (_cmdClosePreview != value) {
            //        _cmdClosePreview = value;
            //        OnPropertyChanged("CmdClosePreview");
            //    }
            //}
        }
        #endregion // CmdClosePreview

        #region CmdOpenPreview
        private DelegateCommand _cmdOpenPreview;

        public DelegateCommand CmdOpenPreview {
            get { return _cmdOpenPreview ?? (_cmdOpenPreview = new DelegateCommand(o => true, o => ShowPreview = true)); }
            //set {
            //    if (_cmdOpenPreview != value) {
            //        _cmdOpenPreview = value;
            //        OnPropertyChanged("CmdOpenPreview");
            //    }
            //}
        }
        #endregion // CmdOpenPreview

        #region ShowPreview
        private bool _showPreview;

        public bool ShowPreview {
            get { return _showPreview; }
            set {
                if (_showPreview != value) {
                    _showPreview = value;
                    OnPropertyChanged("ShowPreview");
                }
            }
        }
        #endregion // ShowPreview

        #region PreviewStream
        private Stream _previewStream;

        public Stream PreviewStream
        {
            get { return _previewStream; }
            set
            {
                if (_previewStream != value)
                {
                    _previewStream = value;
                    OnPropertyChanged("PreviewStream");
                }
            }
        }
        #endregion // PreviewStream

        #region PreviewString
        private string _previewString;

        public string PreviewString {
            get { return _previewString; }
            set {
                if (_previewString != value) {
                    _previewString = value;
                    OnPropertyChanged("PreviewString");
                }
            }
        }
        #endregion

        #region RefreshPreview
        private string _xbrlContent;


        #region FederalGazetteAssistantCurrentTab
        /// <summary>
        /// This property contains wich is the current tab item on DlgAssistant dialog.
        /// </summary>
        private FederalGazetteAssistantTabs _federalGazetteAssistantCurrentTab = FederalGazetteAssistantTabs.NotDefined;

        public FederalGazetteAssistantTabs FederalGazetteAssistantCurrentTab {
            get { return _federalGazetteAssistantCurrentTab; }
            set {
                if (_federalGazetteAssistantCurrentTab != value) {
                    _federalGazetteAssistantCurrentTab = value;
                    OnPropertyChanged("FederalGazetteAssistantCurrentTab");
                }
            }
        }
        #endregion FederalGazetteAssistantCurrentTab

        /// <summary>
        /// Determines whether this instance [can current tab refreshing preview] the specified current tab enum.
        /// </summary>
        /// <param name="pCurrentTabEnum">The current tab enum.</param>
        /// <returns>
        ///   <c>true</c> if this instance [can current tab refreshing preview] the specified current tab enum; otherwise, <c>false</c>.
        /// </returns>
        private bool CanCurrentTabRefreshingPreview(FederalGazetteAssistantTabs pCurrentTabEnum){
            switch (pCurrentTabEnum) {
                case FederalGazetteAssistantTabs.Login:
                    return true;
                case FederalGazetteAssistantTabs.NotDefined:
                    return false;
                case FederalGazetteAssistantTabs.CompanySize:
                    return true;
                case FederalGazetteAssistantTabs.ReportOptions:
                    return true;
                case FederalGazetteAssistantTabs.TreeView:
                    return true;
                case FederalGazetteAssistantTabs.Init:
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// ert:
        /// Refreshing data in preview browser. If throw any exception, or no data, show the message in the browser.
        /// </summary>
        /// <param name="pSendingTabTag">The sending tab tag as FederalGazetteAssistantTabs.</param>
        public void RefreshPreview(FederalGazetteAssistantTabs pSendingTabTag) {
            try {
                if (!CanCurrentTabRefreshingPreview(FederalGazetteAssistantCurrentTab)
                    || pSendingTabTag != FederalGazetteAssistantCurrentTab)
                    return;

                BrowserHasContent = false;
                PreviewString = null;
                PreviewStream = null;
                string xbrl = eBalanceKitBusiness.Export.XbrlExporter.GenerateXbrlForFederalGazette(eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument);//_manager.GenerateXbrl();

                if (xbrl.Equals(_xbrlContent))
                    return;

                _xbrlContent = xbrl;

                BrowserHasContent = true;
                PreviewStream = _manager.GetPreview(_xbrlContent, SendModel.User, SendModel.Password, SendModel.CompanyId);
                
                if (PreviewStream == null)
                    PreviewString = GenerateHtmlStringMessage(ResourcesFederalGazette.PreviewNoData);
            }
            catch (Exception exception) {
                HandleException(exception);
                //System.Windows.MessageBox.Show("oops" + Environment.NewLine + e.Message);
            }
        }

        private void HandleException(Exception exception) {
            string message = exception.Message;
            if (!(exception is federalGazetteBusiness.Structures.Exceptions.FederalGazetteResponseError)) {
                eBalanceKitBase.Structures.ExceptionLogging.LogException(exception);
            }
            if (exception is System.ServiceModel.ServerTooBusyException) {
                message = "Der Server ist derzeit ausgelastet und meldet:" + Environment.NewLine + exception.Message;
            }
            if (exception is System.TimeoutException) {
                message = "Beim warten auf eine Antwort des Servers wurde eine Zeitüberschreitung festgestellt: " + Environment.NewLine + exception.Message;
            }
            PreviewString = GenerateHtmlStringMessage(message);
        }

        #region TosAccepted
        private bool _tosAccepted;

        public bool TosAccepted {
            get { return _tosAccepted; }
            set {
                if (_tosAccepted != value) {
                    _tosAccepted = value;
                    OnPropertyChanged("TosAccepted");
                }
            }
        }
        #endregion // TosAccepted

        public void SendOrder() {
             try {
                 var sendResult = _manager.SendOrder(_xbrlContent, SendModel.User, SendModel.Password, SendModel.CompanyId);
                 var message = "Glückwunsch, wurde erfolgreich eingereicht." + Environment.NewLine + "Auftragsnummer: {0}";
                 if (sendResult is Order) {
                     message = string.Format(message, (sendResult as Order).OrderNo);
                     PreviewString = GenerateHtmlStringMessage(message, false);
                 } else {
                     Debug.Fail("something went wrong please check the result of SendOrder");
                 }
             }
            catch (Exception exception) {
                HandleException(exception);
            }           
        }

        /// <summary>
        /// ert:
        /// Create an html template with the error message.
        /// </summary>
        /// <param name="pMessage"></param>
        /// <param name="isError"> </param>
        /// <returns></returns>
        private string GenerateHtmlStringMessage(string pMessage, bool isError = true) {
            string guilty = System.Net.WebUtility.HtmlEncode(ResourcesFederalGazette.ErrorOfWebservice + Environment.NewLine);
            string errorTemplate = "<!DOCTYPE html PUBLIC " + '"' + "-//W3C//DTD XHTML 1.0 Transitional//EN" + '"' +
                                   "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" + '"' + "><html xmlns=" +
                                   '"' + "http://www.w3.org/1999/xhtml" + '"' +
                                   "><head></head><body>" + (isError ? guilty : "<br />") + "<br /><br />" + System.Net.WebUtility.HtmlEncode(pMessage) + "</body></html>";
            return errorTemplate;
        }
        #endregion

        #region TreeViewBalance
        private IPresentationTree _treeViewBalance;

        public IPresentationTree TreeViewBalance {
            get { return _treeViewBalance; }
            set {
                if (_treeViewBalance != value) {
                    _treeViewBalance = value;
                    OnPropertyChanged("TreeViewBalance");
                }
            }
        }
        #endregion // TreeViewBalance

        #region TreeViewBalanceModel
        private FederalGazette.FederalGazetteTreeViewModel _treeViewBalanceModel;

        public FederalGazette.FederalGazetteTreeViewModel TreeViewBalanceModel
        {
            get { return _treeViewBalanceModel; }
            set
            {
                if (_treeViewBalanceModel != value)
                {
                    _treeViewBalanceModel = value;
                    OnPropertyChanged("TreeViewBalanceModel");
                }
            }
        }
        #endregion // TreeViewBalanceModel

        #region TreeViewIncomeStatementModel
        private FederalGazette.FederalGazetteTreeViewModel _treeViewIncomeStatementModel;

        public FederalGazette.FederalGazetteTreeViewModel TreeViewIncomeStatementModel
        {
            get { return _treeViewIncomeStatementModel; }
            set
            {
                if (_treeViewIncomeStatementModel != value)
                {
                    _treeViewIncomeStatementModel = value;
                    OnPropertyChanged("TreeViewIncomeStatementModel");
                }
            }
        }
        #endregion // TreeViewIncomeStatementModel

        #region TreeViewIncomeStatement
        private IPresentationTree _treeViewIncomeStatement;

        public IPresentationTree TreeViewIncomeStatement {
            get { return _treeViewIncomeStatement; }
            set {
                if (_treeViewIncomeStatement != value) {
                    _treeViewIncomeStatement = value;
                    OnPropertyChanged("TreeViewIncomeStatement");
                }
            }
        }
        #endregion // TreeViewIncomeStatement

        #region RtfModelNotes
        private PresentationTreeDetailModelRtf _rtfModelNotes;

        /// <summary>
        /// Model for the content of the notes ("Anhang")
        /// </summary>
        public PresentationTreeDetailModelRtf RtfModelNotes {
            get { return _rtfModelNotes; }
            set {
                if (_rtfModelNotes != value) {
                    _rtfModelNotes = value;
                    OnPropertyChanged("RtfModelNotes");
                }
            }
        }
        #endregion // RtfModelNotes

        #region RtfModelMgmtReport
        private PresentationTreeDetailModelRtf _rtfModelMgmtReport;
        /// <summary>
        /// Model for the content of the management report ("Lagebericht")
        /// </summary>
        public PresentationTreeDetailModelRtf RtfModelMgmtReport {
            get { return _rtfModelMgmtReport; }
            set {
                if (_rtfModelMgmtReport != value) {
                    _rtfModelMgmtReport = value;
                    OnPropertyChanged("RtfModelMgmtReport");
                }
            }
        }
        #endregion // RtfModelMgmtReport

        #region SendModel
        private FederalGazetteSendModel _sendModel;

        public FederalGazetteSendModel SendModel {
            get { return _sendModel; }
            set {
                if (_sendModel != value) {
                    _sendModel = value;
                    OnPropertyChanged("SendModel");
                }
            }
        }
        #endregion // SendModel

        #region BrowserHasContent
        private bool _browserHasContent;

        public bool BrowserHasContent {
            get { return _browserHasContent; }
            set {
                if (_browserHasContent != value) {
                    _browserHasContent = value;
                    OnPropertyChanged("BrowserHasContent");
                }
            }
        }
        #endregion BrowserHasContent    

        public DelegateCommand CmdSend { get; set; }
    }

    public class FederalGazetteLoginModel : Utils.NotifyPropertyChangedBase {

        #region User
        private string _user;

        public string User {
            get { return _user; }
            set {
                if (_user != value) {
                    _user = value;
                    OnPropertyChanged("User");
                }
            }
        }
        #endregion // User

        #region Password
        private string _password;

        public string Password {
            get { return _password; }
            set {
                if (_password != value) {
                    _password = value;
                    OnPropertyChanged("Password");
                }
            }
        }
        #endregion // Password
    }

    public class FederalGazetteSendModel : FederalGazetteLoginModel {

        public FederalGazetteSendModel(eBalanceKitBusiness.Structures.DbMapping.Company company) {


            CmdShowClientList = new DelegateCommand(o => true, delegate(object o) {
                var dlg = new Windows.FederalGazette.DlgFederalGazetteClientList();
                if (o is System.Windows.Window) {
                    dlg.Owner = o as System.Windows.Window;
                }
                var model =
                    new FederalGazette.FederalGazetteClientListModel(dlg, null, company.Name);
                dlg.DataContext = model;

                //dlg.DataContext =
                //    new Models.FederalGazette.FederalGazetteClientListModel(dlg, manager,
                //                                                                document.Company.Name);

                var res = dlg.ShowDialog();
                if (res.HasValue && res.Value) {
                    try {
                        //var selectedItem = (KeyValuePair<string, string>) dlg.ctlClientList.lstCompanies.SelectedItem;
                        if (!model.SelectedEntry.Equals(default(KeyValuePair<string, string>)) && !string.IsNullOrEmpty(model.SelectedEntry.Key)) {
                            CompanyId = model.SelectedEntry.Key;
                        }
                    } catch (Exception e) {
                        eBalanceKitBase.Structures.ExceptionLogging.LogException(e);
                    }
                }
            });

        }


        #region CmdShowClientList
        private DelegateCommand _cmdShowClientList;

        public DelegateCommand CmdShowClientList {
            get { return _cmdShowClientList; }
            set {
                if (_cmdShowClientList != value) {
                    _cmdShowClientList = value;
                    OnPropertyChanged("CmdShowClientList");
                }
            }
        }
        #endregion // CmdShowClientList

        #region CompanyId
        private string _companyId;

        public string CompanyId {
            get { return _companyId; }
            set {
                if (_companyId != value) {
                    _companyId = value;
                    OnPropertyChanged("CompanyId");
                }
            }
        }
        #endregion // CompanyId
    }
}