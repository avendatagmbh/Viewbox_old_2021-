// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Windows;
using Taxonomy.Enums;
using Utils.Commands;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Add.Models {
    public class AddObjectModel :Utils.NotifyPropertyChangedBase {

        public AddObjectModel(ObjectTypes objectType) { ObjectType = objectType; }

        public AddObjectModel(System.Windows.Window window, ObjectTypes objectType) {
            Owner = window;
            CmdCancel = new DelegateCommand(o => true, o => Cancel());
            CmdOk = new DelegateCommand(o => true, o => Ok());
            ObjectType = objectType;
            if (ObjectType == ObjectTypes.Report) {
                Name = ResourcesCommon.DefaultReportName;
                AddReportModel = new AddReportModel(window);
            }
        }

        private System.Windows.Window Owner { get; set; }
        public ObjectTypes ObjectType { get; set; }

        public string Object { get { return ResourcesMain.ResourceManager.GetString(ObjectType.ToString()); } }

        public string NameLabel{get {
            switch (ObjectType) {
                case ObjectTypes.System:
                    return ResourcesManamgement.NameLabelSystem;
                case ObjectTypes.Company:
                    return ResourcesManamgement.NameLabelCompany;
                case ObjectTypes.Report:
                    return ResourcesManamgement.NameLabelReport;
            }
            return ResourcesCommon.Name;
        }}

        public DelegateCommand CmdOk { get; set; }
        public DelegateCommand CmdCancel { get; set; }

        public bool Success { get; private set; }
        public INamedObject Result { get; private set; }
        public AddReportModel AddReportModel { get; set; }

        #region Name
        private string _name;

        public string Name {
            get { return _name; }
            set {
                if (_name != value) {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        #endregion Name

        #region Comment
        private string _comment;

        public string Comment {
            get { return _comment; }
            set {
                if (_comment != value) {
                    _comment = value;
                    OnPropertyChanged("Comment");
                }
            }
        }
        #endregion Comment

        public void Cancel() {
            Success = false;
            Owner.Close();
        }

        private void GenerateDocument(eBalanceKitBase.Structures.ProgressInfo progressInfo = null) {

            Document document = DocumentManager.Instance.AddDocument(Document.GetTaxonomyInfoFromBusinessSector(AddReportModel.SelectedSpecialAccountingStandard.Name));
            document.Name = Name;
            document.Comment = Comment;

            document.ReportRights = new eBalanceKitBusiness.Rights.ReportRights(document);

            document.LoadDetails(progressInfo);
            var specialAccountingSingleCoice = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.specialAccountingStandard"] as XbrlElementValue_SingleChoice;
            if (specialAccountingSingleCoice != null)
                specialAccountingSingleCoice.SelectedValue = AddReportModel.SelectedSpecialAccountingStandard;
            
            var accountingSingleCoice = document.ValueTreeGcd.Root.Values["de-gcd_genInfo.report.id.accountingStandard"] as XbrlElementValue_SingleChoice;
            if (accountingSingleCoice != null)
                accountingSingleCoice.SelectedValue = AddReportModel.SelectedAccountingStandard;


            Result = document;
        }

        public void GenerateReport(eBalanceKitBase.Structures.ProgressInfo progressInfo = null) {
            var dlg = new eBalanceKitBase.Windows.DlgProgress(Owner)
                      {ProgressInfo = {Caption = ResourcesCommon.ProgressCreatingReport, IsIndeterminate = true}};
            dlg.ExecuteModal(() => GenerateDocument(dlg.ProgressInfo));
        }

        private void Ok() {
            if (ObjectType == ObjectTypes.Report) {
                var dlg = new eBalanceKitBase.Windows.DlgProgress(Owner) { ProgressInfo = { Caption = ResourcesCommon.ProgressCreatingReport, IsIndeterminate = true } };
                dlg.ExecuteModal(() => GenerateDocument(dlg.ProgressInfo));
                SetDialogResult(true);
            }
            else if (ObjectType == ObjectTypes.Company) {
                bool createNew = true;
                bool? dlgResult = false;
                //Owner.Hide();
                //Owner.Visibility = Visibility.Hidden;

                var existingCompany = CompanyManager.Instance.Companies.Where(comp => !string.IsNullOrEmpty(comp.Name) && comp.Name.ToLower().Equals(Name.ToLower())).ToList();
                var existingAllowedCompany = CompanyManager.Instance.AllowedCompanies.Where(comp => !string.IsNullOrEmpty(comp.Name) && comp.Name.ToLower().Equals(Name.ToLower())).ToList();
                
                if (existingCompany.Any()) {
                    var msg = string.Format(ResourcesManamgement.MsgExistingObject, ResourcesMain.Company);
                    
                    if (!existingAllowedCompany.Any()) {
                        msg += Environment.NewLine + string.Format(ResourcesManamgement.MsgNoRightsFor, ResourcesMain.Company);
                    }
                    msg += Environment.NewLine + string.Format(ResourcesManamgement.QNewSameName, ResourcesMain.Company);
                    var msgBox = MessageBox.Show(msg, string.Format(ResourcesManamgement.HeaderFoundIdentical, ResourcesMain.Company), MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    
                    createNew = msgBox == MessageBoxResult.Yes;
                    dlgResult = createNew;
                }
                
                if(createNew) {
                    var dlg = new DlgAddCompany(){Owner = Owner};
                    //var owner = Utils.UIHelpers.TryFindParent<DlgManagementAssistant>(Owner);
                    //var owner = Owner.Owner;
                    dlg.DataContext = new AddCompanyModel(dlg, Name);


                    dlg.Height = Owner.Height;
                    dlg.Width = Owner.Width;
                    dlg.Owner = Owner;
                    dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    Owner.Hide();

                    //dlg.Owner = Owner;
                    dlgResult = dlg.ShowDialog();

                    Result = (dlg.DataContext as AddCompanyModel).Company;

                    Owner.Show();
                }
                //Owner.ShowDialog();
                //Owner.Visibility = Visibility.Visible;
                SetDialogResult(dlgResult);

            } else if (ObjectType == ObjectTypes.System) {
                //dlg = new Windows.Management.Add.DlgAddSystem();
                //(dlg.DataContext as Models.AddSystemModel)
                Result = new eBalanceKitBusiness.Structures.DbMapping.System() {Comment = Comment, Name = Name};
                SystemManager.Instance.AddSystem(Result as eBalanceKitBusiness.Structures.DbMapping.System);
            }
            Owner.Close();
        }

        private void SetDialogResult(bool? result, Window window = null) {
            try {
                Success = result ?? false;
                (window ?? Owner).DialogResult = result;
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }
    }
}