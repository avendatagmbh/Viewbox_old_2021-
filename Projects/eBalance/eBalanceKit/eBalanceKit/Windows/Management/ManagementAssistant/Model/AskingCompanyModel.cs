// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using System.Windows;
using Utils.Commands;
using eBalanceKit.Windows.Management.ManagementAssistant.Add;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public class AskingCompanyModel : AskingModelBase, IAskingModel {

        public AskingCompanyModel(Window window, string name) : base(window) {
            CmdNo  = new DelegateCommand(o => true, o => No());
            CmdYes= new DelegateCommand(o => true, o => Yes());
            ObjectType = ObjectTypes.Company;
            //Result = new Company();
            CompanyName = name;
        }

        public AskingCompanyModel(Window window, Company company) : base(window) {
            CmdNo  = new DelegateCommand(o => true, o => No());
            CmdYes= new DelegateCommand(o => true, o => Yes());
            ObjectType = ObjectTypes.Company;
            //Result = new Company();
            CompanyName = company.Name;
            Company = company;
        }

        private Company Company { get; set; }
        public string CompanyName;

        #region Implementation of IAskingModel
        public DelegateCommand CmdYes { get; set; }

        public DelegateCommand CmdNo { get; set; }

        public override sealed ObjectTypes ObjectType { get; set; }
        public string Question { get { return ResourcesManamgement.QuestionInsertCompanyDetails; } }
        #endregion

        private void Yes() {
            InsertCompanyDetails();
        }

        private void No() {
            GoForward();
        }

        private void SelectCompany() {
            DlgSelectObject dlg = new DlgSelectObject();
            dlg.DataContext = new Model.SelectionModel(dlg,
                                                       eBalanceKitBusiness.Manager.CompanyManager.Instance.
                                                           AllowedCompanies, ObjectTypes.Company);

            ShowWindow(dlg);
        }

        private void InsertCompanyDetails() {
            var dlg = new DlgAddCompany() { Owner = Owner };
            dlg.DataContext = Company != null ? new Add.Models.AddCompanyModel(dlg, Company) : new Add.Models.AddCompanyModel(dlg, CompanyName);

            ShowWindow(dlg);
        }

        private void AddCompany() {
            //DlgAddCompany dlg = new DlgAddCompany();

            DlgNameComment dlg = new DlgNameComment();
            dlg.DataContext = new Add.Models.AddObjectModel(dlg, ObjectTypes.Company);

            ShowWindow(dlg);

            //while (result.HasValue && result.Value) {
                //if (MessageBox.Show(Owner, "Das Unternehmen wurde erfolgreich angelegt. Möchten Sie noch ein weiteres Unternehmen anlegen?", "Weiteres Unternehmen anlegen?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                //    dlg = new DlgAddCompany();
                //    result = dlg.ShowDialog();
                //}
                //else
                //    break;
            //}
        }

    }
}