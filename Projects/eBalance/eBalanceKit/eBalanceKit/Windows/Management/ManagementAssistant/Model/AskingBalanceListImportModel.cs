// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-07-05
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using Utils.Commands;
using eBalanceKitBusiness.Import;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.ManagementAssistant.Model {
    public class AskingBalanceListImportModel :AskingModelBase {
         // Question
        // CmdYes
        // CmdNo

        public AskingBalanceListImportModel(Window window) :base(window) {
            CmdYes = new DelegateCommand(o => true, delegate(object o) { StartSuSaAssistent(window);
                if (o == null) {
                    GoForward();
                    return;
                }
                bool goForward;
                var convertable = bool.TryParse(o.ToString(), out goForward);
                if (!convertable || (convertable && goForward)) {
                    GoForward();
                }
            });
            CmdNo = new DelegateCommand(o => true, o => {
                GoForward();
                //GoForward();
            }
            );
        }

        private void StartSuSaAssistent(Window window) {

            var suSaAssistant = new BalanceList.BalListImportAssistant(null, window);

            var result = ShowWindow(suSaAssistant);

            //if (result){
            //    GoForward();
            //}
        }

        public override ObjectTypes ObjectType { get { return ObjectTypes.Other; } set { } }

        public DelegateCommand CmdYes { get; set; }

        public DelegateCommand CmdNo { get; set; }
        
        public string Question { get { return ResourcesManamgement.QuestionBalanceListImport; } }
    }
}