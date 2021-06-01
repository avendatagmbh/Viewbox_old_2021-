using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AvdWpfControls;
using Utils;
using eBalanceKit.Models.Import;
using eBalanceKit.Windows.Import.Models;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für DlgImportCompanyDetails.xaml
    /// </summary>
    public partial class DlgImportCompanyDetails : Window {
        public DlgImportCompanyDetails() {
            InitializeComponent();
            DataContext = new ImportCompanyDetailsModel(new ImportCompanyDataModel(this));
        }

        ImportCompanyDetailsModel Model { get { return DataContext as ImportCompanyDetailsModel; } }

        private void BtnImport_OnClick(object sender, RoutedEventArgs e) {
            Model.Import(this);
            ImportCompanyAssistantControl.OkButtonCaption = ResourcesCommon.Ok;
            ImportCompanyAssistantControl.OkButtonEnabled = true;
            ImportCompanyAssistantControl.Ok += delegate {DialogResult = true;};
            ImportCompanyAssistantControl.CancelButtonEnabled = false;
            ImportCompanyAssistantControl.CancelButtonVisibility = Visibility.Hidden;
            //MessageBox.Show(ResourcesCompanyImport.ImportSuccessful, ResourcesCommon.FileSaved, MessageBoxButton.OK, MessageBoxImage.Information);
            //DialogResult = true;
        }

        private static int NotRelativeCurrentStep(Selector control) {
            int i = 0;
            for (; i < control.Items.Count; i++) {
                if (control.Items[i] == control.SelectedItem) {
                    break;
                }
            }
            return i + 1;
        }

        private void AssistantControlTabPanel_OnBack(object sender, RoutedEventArgs e) {
            Model.ValidateAndSetDialogPage(NotRelativeCurrentStep(ImportCompanyAssistantControl));
            //ImportCompanyAssistantControl.ValidationValue = null;
        }

        private void AssistantControlTabPanel_OnNext(object sender, RoutedEventArgs e) {
            Model.ValidateAndSetDialogPage(NotRelativeCurrentStep(ImportCompanyAssistantControl));
        }

        private void AssistantControlTabPanel_OnBeforeNext(object o, RoutedEventArgs e) { 
            if (NotRelativeCurrentStep(ImportCompanyAssistantControl) == 1) {
                Exception exception = null;
                if (Model.CsvFiles.Any(file => FileHelper.IsFileBeingUsed(file, out exception, FileShare.ReadWrite))) {
                    MessageBox.Show(exception.Message);
                    e.Handled = true;
                    return;
                }
                Model.CreatePreviewData();
            } else if (NotRelativeCurrentStep(ImportCompanyAssistantControl) == 2) {
                Exception exception = null;
                if (Model.CsvFiles.Any(file => FileHelper.IsFileBeingUsed(file, out exception, FileShare.ReadWrite))) {
                    MessageBox.Show(exception.Message);
                    e.Handled = true;
                    return;
                }
                Model.PreviewImport(this);
                if (Model.CompanyTableList.Count != Model.CsvFiles.Count) {
                    e.Handled = true;
                }
                Model.IgnoreErrors = false;
            } else if (NotRelativeCurrentStep(ImportCompanyAssistantControl) == 4){
                Model.CorrectEntries();
                //if (Model.CompanyDetailsImporter.DateErrors.Count != 0 || Model.CompanyDetailsImporter.ValueErrors.Count != 0) {
                if (!Model.IgnoreErrors && Model.HasErrors) {
                    MessageBox.Show(ResourcesCompanyImport.PleaseMap, ResourcesCommon.ErrorsCaption, MessageBoxButton.OK);
                    e.Handled = true;
                } 
            }
        }

        //public void DoUpdateValidation(object o, RoutedEventArgs e) {
        //    
        //}

    }
}
