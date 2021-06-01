using System;
using System.Collections.Generic;
using System.Data;
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
using Utils;
using eBalanceKitManagement.Models;
using eBalanceKitResources.Localisation;
using UIHelpers = eBalanceKitBase.UIHelpers;

namespace eBalanceKitManagement.Controls {
    /// <summary>
    /// Interaktionslogik für CtlLogging.xaml
    /// </summary>
    public partial class CtlLogging : UserControl {
        public CtlLogging() {
            InitializeComponent();
        }

        private MainWindowModel Model { get { return DataContext as MainWindowModel; } }

        private void btnShowLogs_Click(object sender, RoutedEventArgs e) {
            List<LogConfig> configs = new List<LogConfig> { this.Model.AdminLogConfig, this.Model.ReportLogConfig, this.Model.SendLogConfig };
            try {
                if (tabControl.SelectedIndex < configs.Count) {
                    configs[tabControl.SelectedIndex].ShowLogs();
                    if (configs[tabControl.SelectedIndex].LogFiles.LogList.Count == 0) {
                        var owner = UIHelpers.TryFindParent<Window>(sender as Button);
                        MessageBox.Show(owner, ResourcesLogging.NoEntries, string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

            } catch (Exception ex) {
                var owner = UIHelpers.TryFindParent<Window>(sender as Button);
                MessageBox.Show(owner, ResourcesLogging.ShowLogError + Environment.NewLine + ex.Message);
            }
        }

        /*private void btnPreviousPage_Click(object sender, RoutedEventArgs e) {
            if (tabControl.SelectedIndex == 0)
                this.Model.AdminLogConfig.PreviousPage();
            if (tabControl.SelectedIndex == 1)
                this.Model.ReportLogConfig.PreviousPage();
            if (tabControl.SelectedIndex == 2)
                this.Model.SendLogConfig.PreviousPage();
        }

        private void btnNextPage_Click(object sender, RoutedEventArgs e) {
            if (tabControl.SelectedIndex == 0)
                this.Model.AdminLogConfig.NextPage();
            if (tabControl.SelectedIndex == 1)
                this.Model.ReportLogConfig.NextPage();
            if (tabControl.SelectedIndex == 2)
                this.Model.SendLogConfig.NextPage();

        }*/

        private void btnExportCsv_Click(object sender, RoutedEventArgs e) {
            var done = string.Empty;
            switch (tabControl.SelectedIndex) {
                case 0:
                    done = Model.AdminLogConfig.ExportCsv();
                    break;
                case 1:
                    done = Model.ReportLogConfig.ExportCsv();
                    break;
                case 2:
                    done = Model.SendLogConfig.ExportCsv();
                    break;
            }

            if (done == "cancelled") {
                return;
            }
            if (!string.IsNullOrEmpty(done)) {
                MessageBox.Show(ResourcesExport.ExportErrorMessage + done, ResourcesExport.ExportError, 
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            } else {
                MessageBox.Show(ResourcesExport.ExportSuccessMessage, ResourcesExport.ExportSuccess, MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            /*
            if (Model == null) {
                return;
            }
            //tabControl.SelectedIndex = 0;
            foreach (TabItem item in tabControl.Items) {
                if (item.IsEnabled) {
                    tabControl.SelectedItem = item;
                    break;
                }
            }
            */
        }
    }
}
