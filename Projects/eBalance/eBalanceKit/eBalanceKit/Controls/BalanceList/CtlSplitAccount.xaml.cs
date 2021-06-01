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
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKit.Windows;
using eBalanceKitBusiness;

namespace eBalanceKit.Controls.BalanceList {
    
    /// <summary>
    /// Interaktionslogik für CtlSplitAccount.xaml
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-21</since>
    public partial class CtlSplitAccount : UserControl {
        public CtlSplitAccount() {
            InitializeComponent();
        }

        #region events

        #region Ok
        public event EventHandler Ok;
        private void OnOk() { if (Ok != null) Ok(this, new System.EventArgs()); }
        #endregion

        #region Cancel
        public event EventHandler Cancel;
        private void OnCancel() { if (Cancel != null) Cancel(this, new System.EventArgs()); }
        #endregion
        
        #endregion

        private ISplitAccountGroup SplitAccountGroup { get { return this.DataContext as ISplitAccountGroup; } }
        
        private void btnAddItem_Click(object sender, RoutedEventArgs e) {
            SplitAccountGroup.AddNewItem();
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e) {
            List<ISplittedAccount> itemsToDelete = new List<ISplittedAccount>();
            foreach (var item in dgSplittedAccounts.SelectedItems) itemsToDelete.Add(item as ISplittedAccount);
            foreach (var item in itemsToDelete) SplitAccountGroup.Remove(item);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) { OnOk(); }        
        private void btnCancel_Click(object sender, RoutedEventArgs e) { OnCancel(); }

        private void optAbsoluteChecked(object sender, RoutedEventArgs e) {
            PercentValueCol.Visibility = System.Windows.Visibility.Collapsed;
            tbPercentLabel.Visibility = System.Windows.Visibility.Collapsed;
            tbPercentValue.Visibility = System.Windows.Visibility.Collapsed;
            correctionValueCol.Visibility = System.Windows.Visibility.Collapsed;

        }

        private void optRelativeChecked(object sender, RoutedEventArgs e) {
            PercentValueCol.Visibility = System.Windows.Visibility.Visible;
            tbPercentLabel.Visibility = System.Windows.Visibility.Visible;
            tbPercentValue.Visibility = System.Windows.Visibility.Visible;
            correctionValueCol.Visibility = System.Windows.Visibility.Visible;
            
        }

        private void dgSplittedAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach (ISplittedAccount item in e.RemovedItems) item.IsSelected = false;
            foreach (ISplittedAccount item in e.AddedItems) item.IsSelected = true;
        }

        private void Textbox_GotFocus(object sender, RoutedEventArgs e) {
            var tb = sender as TextBox;
            tb.SelectionStart = tb.Text.Length;
        }
    }
}
