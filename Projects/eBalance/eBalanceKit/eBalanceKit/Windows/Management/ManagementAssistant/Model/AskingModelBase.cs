// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public abstract class AskingModelBase : Utils.NotifyPropertyChangedBase {
        protected AskingModelBase(Window owner) { Owner = owner; }
        
        protected Window Owner { get; set; }

        protected void GoForward() {
            var dlgManagementAssistant = Owner as DlgManagementAssistant;
            if (dlgManagementAssistant != null)
                dlgManagementAssistant.assistantControl.NavigateNext();
        }

        public object Result { get; set; }
        public abstract ObjectTypes ObjectType { get; set; }

        public string QuestionGenerating {
            get {
                return
                    string.Format(
                        ResourcesManamgement.NotExisiting + Environment.NewLine + ResourcesManamgement.WishToCreate,
                        ResourcesMain.ResourceManager.GetString(ObjectType.ToString()));
            }
        }
        public string QuestionIfExisting {
            get {
                return
                    string.Format(
                        ResourcesManamgement.QuestionExisting,
                        ResourcesMain.ResourceManager.GetString(ObjectType.ToString()));
            }
        }

        protected bool ShowWindow(Window dlg) {
            dlg.Height = Owner.Height;
            dlg.Width = Owner.Width;
            dlg.Owner = Owner.Owner ?? Owner;
            dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            Owner.Hide();
            //Owner.Visibility = Visibility.Collapsed;
            

            var result = dlg.ShowDialog();
            var addObject = (dlg.DataContext is Add.Models.AddObjectModel)
                                ? (dlg.DataContext as Add.Models.AddObjectModel).Success
                                : (result.HasValue && result.Value);

            if (addObject) {
                if (dlg.DataContext is Windows.Management.Add.Models.AddCompanyModel) {
                    Result = (dlg.DataContext as Windows.Management.Add.Models.AddCompanyModel).Company;
                }
                if (dlg.DataContext is Windows.Management.Add.Models.AddSystemModel) {
                    Result = (dlg.DataContext as Windows.Management.Add.Models.AddSystemModel).System;
                }
                if (dlg.DataContext is SelectionModel) {
                    Result = (dlg.DataContext as SelectionModel).SelectedObject;
                }
                if (dlg.DataContext is Add.Models.AddObjectModel) {
                    Result = (dlg.DataContext as Add.Models.AddObjectModel).Result;
                }
                if (dlg.DataContext is Models.Assistants.BalListImportAssistantModel) {
                    Result = (dlg.DataContext as Models.Assistants.BalListImportAssistantModel).Importer;
                }
                var managementAssistantModel = Owner.DataContext as ManagementAssistantModel;
                var goToNextPage = (managementAssistantModel != null && !managementAssistantModel.BalanceListImported && dlg.DataContext is Models.Assistants.BalListImportAssistantModel);
                OnPropertyChanged("Result");
                goToNextPage = goToNextPage || !(dlg.DataContext is Models.Assistants.BalListImportAssistantModel);
                if (goToNextPage) {
                    GoForward();
                }
            }
            Owner.ShowDialog();
            //Owner.Visibility = Visibility.Visible;
            return addObject;
        }
    }
}