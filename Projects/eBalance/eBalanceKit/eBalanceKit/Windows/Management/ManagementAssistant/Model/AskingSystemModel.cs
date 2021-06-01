// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using System.Windows;
using Utils.Commands;
using eBalanceKit.Windows.Management.Add;
using eBalanceKitResources.Localisation;
using DlgAddSystem = eBalanceKit.Windows.Management.ManagementAssistant.Add.DlgAddSystem;
using System = eBalanceKitBusiness.Structures.DbMapping.System;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public class AskingSystemModel : AskingModelBase, IAskingModel {
        public AskingSystemModel(Window window)
            : base(window) {
                CmdNo  = new DelegateCommand(o => true, o => No());
            CmdYes= new DelegateCommand(o => true, o => Yes());
            ObjectType = ObjectTypes.System;
            //Result = new eBalanceKitBusiness.Structures.DbMapping.System();
        }

        #region Implementation of IAskingModel
        public DelegateCommand CmdYes { get; set; }

        public DelegateCommand CmdNo { get; set; }

        public override sealed ObjectTypes ObjectType { get; set; }
        public string Question { get { return eBalanceKitBusiness.Manager.SystemManager.Instance.Systems.Any() ? QuestionIfExisting : QuestionGenerating; } }
        #endregion


        private void Yes() {
            if (!eBalanceKitBusiness.Manager.SystemManager.Instance.Systems.Any()) {
                AddSystem();
            }
            else {
                SelectSystem();
            }
        }

        private void No() {
            if (eBalanceKitBusiness.Manager.SystemManager.Instance.Systems.Any()) {
                AddSystem();
            }
            else {
                Result = null;
            }
        }

        private void SelectSystem() {
            DlgSelectObject dlg = new DlgSelectObject();
            dlg.DataContext = new Model.SelectionModel(dlg,
                                                       eBalanceKitBusiness.Manager.SystemManager.Instance.
                                                           Systems, ObjectTypes.System);

            ShowWindow(dlg);
        }
        
        private void AddSystem() {
            DlgAddSystem dlg = new DlgAddSystem();

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