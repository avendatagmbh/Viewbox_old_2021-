// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-05-14
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using AvdWpfControls;
using eBalanceKit.Models;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.AuditMode {
    public partial class CtlAuditCorrections {
        public CtlAuditCorrections() { InitializeComponent(); }

        #region ACM (AuditCorrectionManager)
        private IAuditCorrectionManager ACM {
            get {
                var document = ((MainWindowModel)DataContext).CurrentDocument;
                return document == null ? null : document.AuditCorrectionManager;
            }
        }
        #endregion // ACM

        private void BtnNewAuditCorrection_OnClick(object sender, RoutedEventArgs e) { ACM.AddPositionCorrection(); }
        
        private void BtnDeleteAuditCorrection_OnClick(object sender, RoutedEventArgs e) {
            var correction = ((IAuditCorrectionManager)((Button)sender).DataContext).SelectedPositionCorrection;
            if (MessageBox.Show(
                string.Format(ResourcesAuditCorrections.DeleteCorrection, correction.Name),
                ResourcesAuditCorrections.DeleteCorrectionCaption, 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question, 
                MessageBoxResult.No) != MessageBoxResult.Yes) {

                return;
            }

            ACM.DeleteSelectedPositionCorrection();
        }

        private void BtnDeleteAuditValue_OnClick(object sender, RoutedEventArgs e) {
            var owner = UIHelpers.TryFindParent<Window>(this);

            var transaction = ((Button)sender).DataContext as IAuditCorrectionTransaction;
            Debug.Assert(transaction != null);

            if (MessageBox.Show(
                owner,
                string.Format(ResourcesAuditCorrections.DeleteCorrectionTransaction, transaction.Element.Label),
                ResourcesAuditCorrections.DeleteCorrectionTransactionCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            transaction.Remove();
        }

        private void Position_OnMouseClick(object sender, RoutedEventArgs e) {
            var transaction = (sender as ClickableControl).DataContext as IAuditCorrectionTransaction;
            transaction.IsSelected = true;
            transaction.ShowInTreeview();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e) {
            var owner = UIHelpers.TryFindParent<Window>(this);
            var correction = (IAuditCorrection)((Button)sender).DataContext;
            new DlgTaxAudit(correction) {Owner = owner}.ShowDialog();
        }
        
    }
}
