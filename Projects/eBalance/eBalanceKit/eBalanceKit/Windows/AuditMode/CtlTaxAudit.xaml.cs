// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;
using eBalanceKit.Windows.AuditMode.Models;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.AuditMode {
    public partial class CtlTaxAudit {
        public CtlTaxAudit() { InitializeComponent(); }

        private TransferAuditCorrectionsModel Model { get { return DataContext as TransferAuditCorrectionsModel; } }

        private void AssistantControlOk(object sender, RoutedEventArgs e) { Model.TransferAuditCorrectionValues(); tabControl.NavigateNext(); }
        private void UserControlKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) UIHelpers.TryFindParent<Window>(this).Close(); }

        private void AssistantControl_Next(object sender, RoutedEventArgs e) {
            
            // We just navigate to the last page before submitting
            if (tabControl.SelectedIndex == tabControl.Items.Count - 2) {
                // check if everything is fine
                Model.CheckAuditCorrectionValues();
                // 
                tabControl.OkButtonEnabled = Model.OkButtonEnabled;
            }

            if (tabControl.SelectedIndex == tabControl.Items.Count - 1) {
                
                if (Model.Problems == null || Model.Problems.Categories.Count == 0) {
                    Utils.UIHelpers.TryFindParent<Window>(this).Close();
                    MessageBox.Show(ResourcesAuditCorrections.TransferSuccessMessage, ResourcesCommon.Ok, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }
}
