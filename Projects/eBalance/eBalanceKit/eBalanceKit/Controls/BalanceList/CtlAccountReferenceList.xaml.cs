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
using System.IO;
using eBalanceKitBase.Windows;
using eBalanceKitResources.Localisation;
using System.ComponentModel;
using Microsoft.Win32;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Reconciliation;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitBusiness;
using eBalanceKit.Models;
using System.Windows.Threading;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaction logic for CtlAccountReferenceList.xaml
    /// </summary>
    public partial class CtlAccountReferenceList : UserControl {

        private const string delimiter = ";";

        public CtlAccountReferenceList() {
            InitializeComponent();
        }

        #region events
        //--------------------------------------------------------------------------------
        public event RoutedEventHandler Cancel;
        private void OnCancel() {
            CancelChanges();
            if (Cancel != null) Cancel(this, new RoutedEventArgs());
        }

        public event RoutedEventHandler Ok;
        private void OnOk() {
            SaveChanges();
            if (Ok != null) Ok(this, new RoutedEventArgs());
        }
        //--------------------------------------------------------------------------------
        #endregion events


        private void SaveChanges() {
            
        }

        private void CancelChanges() {
            
        }

        private TaxonomyViewModel Model {
            get {
                if (Dispatcher.CheckAccess()) return DataContext as TaxonomyViewModel;
                TaxonomyViewModel obj = null;
                Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new Action(
                        delegate { obj = DataContext as TaxonomyViewModel; }));
                return obj;
            }
        }

        private void BtnExpandAllNodesClick(object sender, RoutedEventArgs e) {
            if (Model != null)
                new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                    ProgressInfo = { IsIndeterminate = true, Caption = eBalanceKitResources.Localisation.ResourcesCommon.LoadingPositions }
                }.ExecuteModal(Model.PresentationTree.ExpandAllNodes);
        }

        private void BtnCollapseAllNodesClick(object sender, RoutedEventArgs e) {
            Model.PresentationTree.CollapseAllNodes();
        }
        
        private void ReferenceListCancel_Click(object sender, RoutedEventArgs e) {
            UIHelpers.TryFindParent<Window>(this).Close();
        }

        private void ReferenceListSave_Click(object sender, RoutedEventArgs e) {
            // select location
            var dlg = new SaveFileDialog();
            dlg.FileOk += SaveToFile;
            dlg.Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv";
            dlg.ShowDialog();
        }

        private void SaveToFile(object sender, CancelEventArgs e) {
            bool successful = false;
            string fileName = ((SaveFileDialog)sender).FileName;
            // save to file
            var progress = new DlgProgress(UIHelpers.TryFindParent<Window>(this)) { ProgressInfo = { IsIndeterminate = true, Caption = ResourcesReconciliation.ProgressSaveReferenceList } };
            try {
                progress.ExecuteModal(delegate() { successful = CreateFileFile(fileName); });
            } catch (ExceptionBase ex) {
                MessageBox.Show(ex.Message, ex.Header);
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            } finally {
                try {
                    progress.Close();
                } catch (Exception exc) {
                    System.Diagnostics.Debug.WriteLine(exc);
                }
            }
            if (successful)
                UIHelpers.TryFindParent<Window>(this).Close();

            if (successful && MessageBox.Show(ResourcesCommon.FileSaveSuccessfulOpen, ResourcesCommon.FileSaved, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {
                System.Diagnostics.Process.Start(fileName);
            }
        }

        private bool CreateFileFile(string fileName) {
            try {
                FileInfo file = new FileInfo(fileName);
                DirectoryInfo directory = new DirectoryInfo(file.DirectoryName);
                if (!directory.Exists) directory.Create();

                AccountReferenceList referenceList = BalanceListManager.Instance.ReferenceList;

                Exception ex = null;
                StringBuilder csvContent = new StringBuilder();
                if (!Utils.FileHelper.IsFileBeingUsed(fileName, out ex)) {
                    using (StreamWriter csvWrite = new StreamWriter(File.Open(fileName, FileMode.Create), Encoding.Default)) {
                        csvContent.AppendLine(ResourcesBalanceList.ReferenceListAccountNumber + delimiter + ResourcesBalanceList.ReferenceListAccountNote);
                        foreach (IAccountReferenceListItem referenceListItem in referenceList.Items) {
                            csvContent.AppendLine(CreateReferencListCsvEntry(referenceListItem));
                        }
                        csvWrite.WriteLine(csvContent);
                        csvWrite.Close();
                    }
                    return true;
                } else {
                    System.Windows.MessageBox.Show(ex.Message, ResourcesCommon.SaveFileEror, MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            } catch (Exception ex) {
                throw new Exception(
                    ResourcesReconciliation.ReferenceListCreationError + Environment.NewLine + ex.Message, ex);
            }
        }

        private string CreateReferencListCsvEntry(IAccountReferenceListItem referenceListItem) {
            IBalanceListEntry entry = null;
            switch (referenceListItem.AccountType) {
                case AccountTypeEnum.Account:
                    entry = DocumentManager.Instance.CurrentDocument.SelectedBalanceList.AssignedItems.FirstOrDefault(a => a is Account && ((Account)a).Id == referenceListItem.AccountId);
                    break;
                case AccountTypeEnum.AccountGroup:
                    entry = DocumentManager.Instance.CurrentDocument.SelectedBalanceList.AssignedItems.FirstOrDefault(a => a is AccountGroup && ((AccountGroup)a).Id == referenceListItem.AccountId);
                    break;
                case AccountTypeEnum.SplittedAccount:
                    entry = DocumentManager.Instance.CurrentDocument.SelectedBalanceList.AssignedItems.FirstOrDefault(a => a is SplittedAccount && ((SplittedAccount)a).Id == referenceListItem.AccountId);
                    break;
            }
            if (entry == null)
                return "N/A" + delimiter + "N/A";
            return entry.Number + delimiter + entry.Name;
        }
    }


}
