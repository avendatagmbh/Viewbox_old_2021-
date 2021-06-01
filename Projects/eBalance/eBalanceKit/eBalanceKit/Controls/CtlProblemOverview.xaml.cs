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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using eBalanceKit.Windows;
using eBalanceKit.Windows.AuditMode.Models;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Controls {
    /// <summary>
    /// Interaktionslogik für CtlProblemOverview.xaml
    /// </summary>
    public partial class CtlProblemOverview : UserControl {
        public CtlProblemOverview() {
            InitializeComponent();
#if !DEBUG
            btExportAsPdf.Visibility = Visibility.Collapsed;
#endif

        }

        private void btExportAsPdf_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog dlg = new SaveFileDialog { FileName = "problem.pdf"};
            if (!dlg.ShowDialog(Utils.UIHelpers.TryFindParent<Window>(this)).Value) return;

            var model = DataContext as TransferAuditCorrectionsModel;
            if (model != null)
                model.Problems.GeneratePdf(dlg.FileName);           
        }
    }
}
