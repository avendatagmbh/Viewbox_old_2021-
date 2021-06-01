using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.BalanceList;
using eBalanceKit.Models.Document;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaktionslogik für CtlBalanceListManager.xaml
    /// </summary>
    public partial class CtlBalanceListManager : UserControl {
        
        public CtlBalanceListManager() {
            InitializeComponent();
        }

        private EditDocumentModel Model { get { return this.DataContext as EditDocumentModel; } }
        public Window Owner { get; set; }

        private void btnAddItem_Click(object sender, RoutedEventArgs e) {
            if (!RightManager.HasDocumentRestWriteRight(Model.Document,Owner)) return;
            BalListImportAssistant dlg = new BalListImportAssistant(this.Model.Document, this.Owner) ;
            
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true) {
                // reload document - workaround for gui update problem (assigned accounts will not be shown unless the document will be reloaded)
                var mainWindow = GlobalResources.MainWindow;
                var tmp = mainWindow.Model.CurrentDocument;
                mainWindow.Model.CurrentDocument = null;
                mainWindow.Model.CurrentDocument = tmp;
            }
            
            if (Model.Document.BalanceListsVisible.Count >= 1)
                Model.Document.SelectedBalanceList = this.Model.Document.BalanceListsVisible[0];
            
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e) {
            if (!RightManager.HasDocumentRestWriteRight(Model.Document,UIHelpers.TryFindParent<Window>(this))) return;
            if (lstItems.SelectedIndex == -1) return;
            if (MessageBox.Show(UIHelpers.TryFindParent<Window>(this), ResourcesBalanceList.RequestDeleteBalanceList, "", MessageBoxButton.YesNo,
                MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                return;
            BalanceListManager.RemoveBalanceList(this.Model.Document.SelectedBalanceList);
        }

        private void btnEditItem_Click(object sender, RoutedEventArgs e) { EditSelectedBalanceList(); }
        private void lstItems_MouseDoubleClick(object sender, MouseButtonEventArgs e) { EditSelectedBalanceList(); }

        private void EditSelectedBalanceList() {
            if (!RightManager.HasDocumentRestWriteRight(Model.Document,UIHelpers.TryFindParent<Window>(this))) return;

            if (Model.Document.SelectedBalanceList == null) return;
            var owner = UIHelpers.TryFindParent<Window>(this);
            var dlg = new DlgEditBalanceList(Model.Document.SelectedBalanceList) { Owner = owner };
            dlg.ShowDialog();
        }

    }
}
