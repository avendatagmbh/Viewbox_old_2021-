// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-04
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using Utils.Commands;
using eBalanceKitBusiness.Interfaces;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public class AskingRepeatModel {
        public AskingRepeatModel(Window owner) { Owner = owner;
            CmdNo = new DelegateCommand(o => true, o => owner.Close());
            CmdYes = new DelegateCommand(o => true, o => GoBackToStart());
        }

        protected Window Owner { get; set; }

        public DelegateCommand CmdYes { get; set; }
        public DelegateCommand CmdNo { get; set; }

        public string Question { get { return ResourcesManamgement.QuestionRestart; } }

        protected void GoBackToStart() {
            var dlgManagementAssistant = Owner as DlgManagementAssistant;
            if (dlgManagementAssistant != null)
                dlgManagementAssistant.assistantControl.SelectedIndex = 0;
        }
    }
}