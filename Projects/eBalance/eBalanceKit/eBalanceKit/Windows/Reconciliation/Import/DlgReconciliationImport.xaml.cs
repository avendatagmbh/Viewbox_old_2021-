using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using AvdWpfControls;
using Utils.Commands;
using eBalanceKitBusiness.Reconciliation.Interfaces;
using eBalanceKitBusiness.Reconciliation.ViewModels;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Reconciliation.Import {
    /// <summary>
    /// Interaction logic for DlgReconciliationImport.xaml
    /// </summary>
    public partial class DlgReconciliationImport : Window {
        public DlgReconciliationImport() {
            InitializeComponent();
        }

        //protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
        //    base.OnPropertyChanged(e);
        //    if (e.Property == FrameworkElement.DataContextProperty) {
        //        if (Model is IReconciliationImportModel && this.tab1.Content != null && this.tab1.Content is CtlImportOpenFile) {
        //            (Model as IReconciliationImportModel).SetCreateExampleFileCommand(new DelegateCommand((o) => true, (this.tab1.Content as CtlImportOpenFile).ShowReferenceListWindow));
        //        }
        //    }
        //}

        ReconciliationImportModel Model { get { return DataContext as ReconciliationImportModel; } }
        
        private void AssistantControlTabPanel_OnBack(object sender, RoutedEventArgs e) {
            AssistantControl assistantControl = sender as AssistantControl;
            if (assistantControl == null) return;
            // in the model the current step is 0 base indexed.
            if (!Model.ValidateAndSetDialogPage(assistantControl.CurrentStep)) assistantControl.NavigateNext();
        }

        private void AssistantControlTabPanel_OnNext(object sender, RoutedEventArgs e) {
            AssistantControl assistantControl = sender as AssistantControl;
            if (assistantControl == null) return;
            if (Model.ValidateAndSetDialogPage(assistantControl.CurrentStep)) {
                if (assistantControl.CurrentStep == 2)
                    if (AreFilesAvailable())
                        Model.CreatePreviewData();
                    else
                        assistantControl.NavigateBack();
                if (assistantControl.CurrentStep == 3)
                    if (AreFilesAvailable())
                        Model.PreviewImport(this);
                    else
                        assistantControl.NavigateBack();
            } else assistantControl.NavigateBack();
        }

        private void BtnImport_OnClick(object sender, RoutedEventArgs e) {
            try {
                Model.Import(this);
                MessageBox.Show(ResourcesReconciliation.ImportSuccessfulMessage, ResourcesCommon.FileSaved, MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
            } catch (Exception) {
                throw;
            }
        }

        private bool AreFilesAvailable() {
            bool retVal = true;
            foreach (string item in Model.CsvFiles) {
                Exception ex = null;
                if (Utils.FileHelper.IsFileBeingUsed(item, out ex, FileShare.ReadWrite)) {
                    System.Windows.MessageBox.Show(ex.Message, ResourcesCommon.OpenFileEror, MessageBoxButton.OK, MessageBoxImage.Error);
                    retVal = false;
                }
            }
            return retVal;
        }
    }
}
