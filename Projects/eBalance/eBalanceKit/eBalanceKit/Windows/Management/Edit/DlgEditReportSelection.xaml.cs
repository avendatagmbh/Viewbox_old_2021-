using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using eBalanceKit.Windows.Management.Delete.Models;

namespace eBalanceKit.Windows.Management.Edit {
    /// <summary>
    /// Interaktionslogik für DlgEditReportSelection.xaml
    /// </summary>
    public partial class DlgEditReportSelection : Window {
        public DlgEditReportSelection() {
            InitializeComponent();
        }

        DeleteReportModel Model { get { return DataContext as DeleteReportModel; } }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            if (Model.SelectedReport != null) {
                Close();
                DlgEditReport dlg = new DlgEditReport(Model.SelectedReport){Owner = Owner};
                var dlgResult = dlg.ShowDialog();
                if (eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument == null) {
                    //GlobalResources.MainWindow.Model.SelectedSystem = Model.SelectedReport.System;
                    //GlobalResources.MainWindow.Model.SelectedCompany = Model.SelectedReport.Company;
                    //GlobalResources.MainWindow.Model.SelectedFinancialYear = Model.SelectedReport.FinancialYear;
                    //eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument = Model.SelectedReport;
                    GlobalResources.MainWindow.Model.CurrentDocument = Model.SelectedReport;
                }
            }
        }

        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
