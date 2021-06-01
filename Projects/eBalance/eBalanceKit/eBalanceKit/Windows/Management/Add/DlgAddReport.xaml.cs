// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;
using eBalanceKit.Models;
using eBalanceKit.Windows.Management.Add.Models;
using eBalanceKitBase.Windows;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.Add {
    public partial class DlgAddReport {
        public DlgAddReport() {
            InitializeComponent();
            DataContext = new AddReportModel(this);
            ctlReportOverview.Owner = this;
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            ((MainWindowModel) GlobalResources.MainWindow.DataContext).CurrentDocument = ((AddReportModel) DataContext).Document;
            DialogResult = true;
        }
        
        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            ((AddReportModel) DataContext).Cancel();
            DialogResult = false;
        }

        private void WindowKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                ((AddReportModel)DataContext).Cancel();
                DialogResult = false;
            }
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e) {
            var model = ((AddReportModel) DataContext);
            var dlg = new DlgProgress(this) { ProgressInfo = { Caption = ResourcesCommon.ProgressCreatingReport, IsIndeterminate = true } };
            dlg.ExecuteModal(() => model.InitContentModel(dlg.ProgressInfo));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (DialogResult == null) {
                ((AddReportModel)DataContext).Cancel();
                DialogResult = false;
            }
        }
    }
}