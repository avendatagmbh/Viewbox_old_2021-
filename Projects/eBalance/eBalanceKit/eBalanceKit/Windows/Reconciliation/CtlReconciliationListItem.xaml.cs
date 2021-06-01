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
using eBalanceKit.Windows.Reconciliation.Models;
using eBalanceKitBusiness.Reconciliation.Interfaces.ReconciliationTypes;

namespace eBalanceKit.Windows.Reconciliation {
    /// <summary>
    /// Interaktionslogik für CtlReconciliationListItem.xaml
    /// </summary>
    public partial class CtlReconciliationListItem : UserControl {
        public CtlReconciliationListItem() {
            InitializeComponent();
        }

        ChangeReconciliationModel Model { get { return UIHelpers.TryFindParent<Window>(this).DataContext as ChangeReconciliationModel; } }
        IReconciliation Item { get { return DataContext as IReconciliation; } }

        private void ClickableControlMouseClick(object sender, RoutedEventArgs e) {
            Model.Name = Item.Name;
            Model.Comment = Item.Comment;
                 
            DlgChangeReconciliation dlg = new DlgChangeReconciliation() { Owner = UIHelpers.TryFindParent<Window>(this), DataContext = Model};
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value) {
                Item.Name = Model.Name;
                Item.Comment = Model.Comment;
            }
        }
    }
}
