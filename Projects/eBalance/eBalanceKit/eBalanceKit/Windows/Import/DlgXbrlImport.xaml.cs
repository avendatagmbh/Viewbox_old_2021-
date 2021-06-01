using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
using System.Xml;
using AvdWpfControls;
using eBalanceKit.Models;
using eBalanceKit.Models.Import;
using eBalanceKitBase.Structures;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Import;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Import
{
    /// <summary>
    /// Interaction logic for DlgXbrlImport.xaml
    /// </summary>
    public partial class DlgXbrlImport : Window
    {
        public DlgXbrlImport(Action companyRefresher) {
            InitializeComponent();
            ImportXbrlModel importer = new ImportXbrlModel(this, companyRefresher);
            DataContext = importer;
            _action = companyRefresher;
        }

        ImportXbrlModel Model { get { return DataContext as ImportXbrlModel; } }
        Action _action;
        public ProgressInfo ProgressInfo { get; private set; }
        

        private void ImportAssistantControlNext(object sender, RoutedEventArgs e) {
            AssistantControl assistantControl = sender as AssistantControl;
            if (assistantControl == null) return;
            if (assistantControl.SelectedItem == lastPageTab)
                assistantControl.NextButtonCaptionLastPage = ResourcesCommon.Import;
        }

        private void ImportAssistantControlBeforeNext(object sender, RoutedEventArgs e) {
            Model.SetDetailsFromXbrl();
            if(Model.Company == null || Model.FinancialYear == null) {
                MessageBox.Show(ResourcesCommon.ImportXbrlNoCompanyOrFinancialYear);
                e.Handled = true;
            }
        }

        private void ImportAssistantControlFinish(object sender, RoutedEventArgs e) {
            if(Model.SelectedSystem == null) {
                MessageBox.Show(!SystemManager.Instance.HasAllowedSystems
                                    ? ResourcesCommon.ImportXbrlNoSystemAvailable
                                    : ResourcesCommon.ImportXbrlNoSystemSelected);
                return;
            }
            Model.Import(_action);
            Close();
            if(Model.Myimporter.HasErrors) {
                string errors = string.Join("\n", Model.Myimporter.XbrlImportErrors);
                MessageBox.Show(ResourcesCommon.ImportXbrlErrorsOccured + "\n" + errors);
            }
            else {
                MessageBox.Show(ResourcesCommon.ImportXbrlSuccessful);
            }
        }
    }
}
