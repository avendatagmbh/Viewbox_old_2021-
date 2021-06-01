using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Taxonomy.Enums;
using eBalanceKit.Windows.EditPresentationTreeDetails;
using eBalanceKit.Windows.Reconciliation;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness;
using eBalanceKitBusiness.AuditCorrections;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKit.Models;
using eBalanceKit.Controls.BalanceList;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping.BalanceList;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.ResourceDictionaries {
    public partial class TaxonomyTreeView {

        public TaxonomyTreeView() { InitializeComponent(); }

        private void ManualValueTextbox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {

            if (e.Key == System.Windows.Input.Key.Subtract) {
                TextBox tb = sender as TextBox;
                int selStart = tb.SelectionStart;
                string before = tb.Text.Substring(0, selStart);
                string after = tb.Text.Substring(before.Length);
                tb.Text = string.Concat(before, "-", after);
                tb.SelectionStart = before.Length + 1;
                e.Handled = true;


            } else if (e.Key == System.Windows.Input.Key.Add) {
                TextBox tb = sender as TextBox;
                int selStart = tb.SelectionStart;
                string before = tb.Text.Substring(0, selStart);
                string after = tb.Text.Substring(before.Length);
                tb.Text = string.Concat(before, "+", after);
                tb.SelectionStart = before.Length + 1;
                e.Handled = true;

            } else if (e.Key == System.Windows.Input.Key.Enter || e.Key == System.Windows.Input.Key.Return) {
                TextBox tb = sender as TextBox;
                tb.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            }
        }

        private void BtnEditDetails_Click(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;
            var node = btn.DataContext as IPresentationTreeNode;
            var owner = UIHelpers.TryFindParent<Window>(btn);
            if (node == null) return;

            if (node.IsHypercubeContainer) {
                // preload cube data
                var progress = new DlgProgress(owner) { ProgressInfo = { Caption = "Lade Tabelle...", IsIndeterminate = true } };
                progress.ExecuteModal(node.Document.GetHyperCube(node.Element.Id).PreloadItems);

                new DlgEditHyperCubeDetails(node) { Owner = owner }.ShowDialog();
                return;
            }

            switch (node.Element.ValueType) {
                case XbrlElementValueTypes.Abstract:
                case XbrlElementValueTypes.None:
                case XbrlElementValueTypes.SingleChoice:
                case XbrlElementValueTypes.MultipleChoice:
                case XbrlElementValueTypes.Tuple:
                    if (node.Element.IsList) {
                        new DlgEditListDetails(node) {
                            Owner = owner,
                            DataContext = new PresentationTreeDetailModel(node.Document, node.Value)
                        }.ShowDialog();
                    }
                    return;
                
                case XbrlElementValueTypes.Boolean:
                case XbrlElementValueTypes.Date:
                case XbrlElementValueTypes.Int:
                case XbrlElementValueTypes.Numeric:
                    // no detail editor implemented
                    return;

                case XbrlElementValueTypes.Monetary:
                    new DlgEditMonetaryDetails {
                        Owner = owner,
                        DataContext = new PresentationTreeDetailModel(node.Document, node.Value)
                    }.ShowDialog();
                    return;

                case XbrlElementValueTypes.String:
                    new DlgEditTextDetails {
                        Owner = owner,
                        DataContext = new PresentationTreeDetailModel(node.Document, node.Value)
                    }.ShowDialog();
                    return;
            }
        }

        private void BtnAddHypercubeClick(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;
            var node = btn.DataContext as IPresentationTreeNode;
            var owner = UIHelpers.TryFindParent<Window>(btn);
            if (node == null) return;

            var tableNtAssCubes = new List<string> {
                "de-gaap-ci_table.nt.ass.gross",
                "de-gaap-ci_table.nt.ass.gross_short",
                "de-gaap-ci_table.nt.ass.net"
            };

            if (tableNtAssCubes.Contains(node.Element.Id)) {
                tableNtAssCubes.Remove(node.Element.Id);
                if (tableNtAssCubes.Any(name => node.Document.ExistsHyperCube(name))) {
                    MessageBox.Show(owner,
                                    "Es kann nur eine Anlagespiegel-Variante zur Zeit angelegt werden.",
                                    ResourcesCommon.Error,
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            node.Document.CreateHyperCube(node.Element.Id);
        }
        
        private void BtnDeleteHypercubeClick(object sender, RoutedEventArgs e) {
            var btn = (Button)sender;
            var node = btn.DataContext as IPresentationTreeNode;
            if (node == null) return;
            var owner = UIHelpers.TryFindParent<Window>(btn);

            if (MessageBox.Show(owner,
                "Alle Daten für " + node.Element.Label + " wirklich Löschen?",
                node.Element.Label + " löschen?",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            node.Document.DeleteHyperCube(node.Element.Id);
        }

        private void BtnResetClick(object sender, RoutedEventArgs e) {
            Button btn = sender as Button;
            DependencyObject grid = VisualTreeHelper.GetParent(btn);
            ComboBox cb = VisualTreeHelper.GetChild(grid, 0) as ComboBox;
            cb.SelectedIndex = -1;
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {

            var img = (Image)sender;
            var node = img.DataContext as IPresentationTreeNode;
            if (node != null) node.Value.SupressWarningMessages = !node.Value.SupressWarningMessages;
        }

        private void ChkSelectedChanged(object sender, RoutedEventArgs e) {
            CheckBox chk = (CheckBox)sender;
            
            AccountTypeEnum accountType = AccountTypeEnum.Account;
            long id = -1;

            if (chk.DataContext is Account) {
                Account node = chk.DataContext as Account;
                if (node != null) {
                    accountType = AccountTypeEnum.Account;
                    id = node.Id;
                    node.IsAssignedToReferenceList = chk.IsChecked ?? false;
                }
            } else if (chk.DataContext is AccountGroup) {
                AccountGroup node = chk.DataContext as AccountGroup;
                if (node != null) {
                    accountType = AccountTypeEnum.AccountGroup;
                    id = node.Id;
                    node.IsAssignedToReferenceList = chk.IsChecked ?? false;
                }
            } else if (chk.DataContext is SplittedAccount) {
                SplittedAccount node = chk.DataContext as SplittedAccount;
                if (node != null) {
                    accountType = AccountTypeEnum.SplittedAccount;
                    id = node.Id;
                    node.IsAssignedToReferenceList = chk.IsChecked ?? false;
                }
            }
            
            if (id == -1) return;
                
            // save selection changes
            if (chk.IsChecked.HasValue && chk.IsChecked.Value)
                BalanceListManager.Instance.AddItemToReferenceList(new AccountReferenceListItem(accountType, id));
            else
                BalanceListManager.Instance.RemoveItemFromReferenceList(new AccountReferenceListItem(accountType, id));
        }

        private void BtnAssignPositionClick(object sender, RoutedEventArgs e) {
            var node = ((Button) sender).DataContext as IPresentationTreeNode;
            Debug.Assert(node != null && node.Value != null);
            
            var acm = node.Value.Parent.ValueTree.Document.AuditCorrectionManager;
            Debug.Assert(acm != null);

            acm.SelectedPositionCorrection.AddTransaction(node);
        }

        private void BtnDeleteAuditValue_OnClick(object sender, RoutedEventArgs e) {
            var btn = sender as Button;
            if (btn == null) return;

            var transaction = btn.DataContext as IAuditCorrectionTransaction;
            Debug.Assert(transaction != null);

            var owner = UIHelpers.TryFindParent<Window>(btn);
            if (MessageBox.Show(
                owner,
                string.Format(ResourcesAuditCorrections.DeleteCorrectionTransaction, transaction.Element.Label),
                ResourcesAuditCorrections.DeleteCorrectionTransactionCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            transaction.Remove();
        }

        private void BtnDeleteAccount_OnClick(object sender, RoutedEventArgs e) {

            var btn = sender as Button;
            if (btn == null) return;

            var balanceListEntry = btn.DataContext as IBalanceListEntry;
            Debug.Assert(balanceListEntry != null);

            var owner = UIHelpers.TryFindParent<Window>(btn);
            if (MessageBox.Show(
                owner,
                string.Format(ResourcesMain.RequestDeleteAccount, balanceListEntry.Number, balanceListEntry.Name),
                ResourcesMain.RequestDeleteAccountCaption,
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question, 
                MessageBoxResult.No) != MessageBoxResult.Yes)
                return;

            if (!RightManager.HasDocumentRestWriteRight(balanceListEntry.BalanceList.Document, owner)) return;

            balanceListEntry.IsChecked = false;
            foreach (var node in balanceListEntry.Parents.ToList()) node.RemoveChildren(balanceListEntry);
            balanceListEntry.BalanceList.Document.RemoveAssignment(balanceListEntry);
        }

    }
}
