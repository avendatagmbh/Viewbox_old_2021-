using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using AvdWpfControls;
using eBalanceKit.Models.Import;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Import
{
    /// <summary>
    /// Interaction logic for DlgFullImport.xaml
    /// </summary>
    public partial class DlgFullImport : Window
    {
        public DlgFullImport(Action companyRefresher) {
            InitializeComponent();
            ImportFullModel importer = new ImportFullModel(this, companyRefresher);
            DataContext = importer;
            _action = companyRefresher;
        }

        ImportFullModel Model { get { return DataContext as ImportFullModel; } }
        Action _action;
        public ProgressInfo ProgressInfo { get; private set; }


        private void ImportAssistantControlNext(object sender, RoutedEventArgs e) {
            AssistantControl assistantControl = sender as AssistantControl;
            if (assistantControl == null) return;
            if (assistantControl.SelectedItem == lastPageTab)
                assistantControl.NextButtonCaptionLastPage = ResourcesCommon.Import;
        }

        private void ImportAssistantControlBeforeNext(object sender, RoutedEventArgs e) {
            Model.SetDetailsFromFullExport();
        }

        private void ImportAssistantControlFinish(object sender, RoutedEventArgs e) {
            if (Model.SelectedSystem == null) {
                MessageBox.Show(!SystemManager.Instance.HasAllowedSystems
                                    ? ResourcesCommon.ImportXbrlNoSystemAvailable
                                    : ResourcesCommon.ImportXbrlNoSystemSelected);
                return;
            }
            Model.Import(_action);
            Close();
            if (Model.XbrlImporter.HasErrors) {
                string errors = string.Join("\n", Model.XbrlImporter.XbrlImportErrors);
                MessageBox.Show(ResourcesCommon.ImportXbrlErrorsOccured + "\n" + errors);
            }
            else {
                MessageBox.Show(ResourcesCommon.ImportXbrlSuccessful);
            }
        }
    }
}
