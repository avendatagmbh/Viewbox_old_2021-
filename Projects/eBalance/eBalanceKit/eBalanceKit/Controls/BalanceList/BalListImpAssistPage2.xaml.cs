/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2012-01-18      initial implementation
 *************************************************************************************************************/

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
using AvdCommon.DataGridHelper;
using eBalanceKit.Models.Assistants;
using eBalanceKitBusiness.Import;

namespace eBalanceKit.Controls.BalanceList {

    /// <summary>
    /// Interaktionslogik für BalListImpAssistPage2.xaml
    /// </summary>
    public partial class BalListImpAssistPage2 : BalListImpAssistPageBase {

        public BalListImpAssistPage2() {
            InitializeComponent();
        }

        BalListImportAssistantModel Model {
            get { return this.DataContext as BalListImportAssistantModel; }
        }

        private void optSignedBalance_Checked(object sender, RoutedEventArgs e) {
            SetImportType(BalanceListImportType.SignedBalance);
        }

        private void optBalanceWithDCFlagOneCol_Checked(object sender, RoutedEventArgs e) {
            SetImportType(BalanceListImportType.DebitCreditFlagOneColumn);
        }

        private void optBalanceWithDCFlagTwoCols_Checked(object sender, RoutedEventArgs e) {
            SetImportType(BalanceListImportType.DebitCreditFlagTwoColumns);
        }

        private void optBalanceInTwoCols_Checked(object sender, RoutedEventArgs e) {
            SetImportType(BalanceListImportType.BalanceInTwoColumns);
        }

        private void SetImportType(BalanceListImportType type) {
            if (this.Model != null && Model.Importer != null && Model.Importer.PreviewData != null) {
                Model.Importer.ClearColumns();
                this.Model.Importer.Config.ImportType = type;
                DataGridCreater.CreateColumns(preview.dgCsvData, Model.Importer.PreviewData);
                preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
                preview.dgCsvData.AutoGenerateColumns = false;
            }
        }

        private void ChangePreview() {
            if (Model != null && Model.Importer.PreviewData != null) {
                DataGridCreater.CreateColumns(preview.dgCsvData, Model.Importer.PreviewData);
                preview.dgCsvData.ItemsSource = Model.Importer.PreviewData.Rows;
                preview.dgCsvData.AutoGenerateColumns = false;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            ChangePreview();
        }

        private void cboEncoding_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ChangePreview();
        }

    }
}
