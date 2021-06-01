// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2010-08-12
// copyright 2010 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using eBalanceKit.Structures;
using eBalanceKit.Windows;
using eBalanceKit.Windows.BalanceList;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Manager;
using IPresentationTreeNode = Taxonomy.Interfaces.PresentationTree.IPresentationTreeNode;

namespace eBalanceKit.Controls.BalanceList {
    
    public partial class CtlBalanceList {

        public CtlBalanceList() { InitializeComponent(); }

        #region fields

        /// <summary>
        /// Start position of mouse drag event, needed to init drag&drop process.
        /// </summary>
        private Point _startPoint;

        private bool _lCtrlKeyPressed;
        private bool _rCtrlKeyPressed;
        private bool _lShiftKeyPressed;
        private bool _rShiftKeyPressed;

        #endregion fields

        #region properties
        private eBalanceKitBusiness.Structures.DbMapping.Document Document {
            get { return mainGrid.DataContext as eBalanceKitBusiness.Structures.DbMapping.Document; } 
        }
        
        #endregion properties

        /// <summary>
        /// Handles the PreviewMouseMove event of the accountList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void accountList_PreviewMouseMove(object sender, MouseEventArgs e) {

            if (e.LeftButton == MouseButtonState.Pressed && accountList.SelectedItem != null && _startPoint != null) {

                ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(accountList, _startPoint);
                if (lbi == null) return; // no account item under mouse pointer
                if (!RightManager.HasDocumentRestWriteRight(Document, UIHelpers.TryFindParent<Window>(this))) return;

                Point position = e.GetPosition(accountList);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {

                    List<IBalanceListEntry> accounts = new List<IBalanceListEntry>();
                    foreach (object obj in this.accountList.SelectedItems) {
                        accounts.Add((IBalanceListEntry) obj);
                    }

                    AccountListDragDropData data = new AccountListDragDropData(accounts);
                    DataObject dragData = new DataObject("AccountListDragDropData", data);

                    var popup = UIHelpers.TryFindParent<MainWindow>(this).DragDropPopup;
                    popup.DataContext = data;
                    if (!popup.IsOpen) popup.IsOpen = true;
                    try {
                        DragDrop.DoDragDrop(accountList, dragData, DragDropEffects.Move);
                    } catch (eBalanceKitBusiness.Exceptions.AssignmentNotAllowedException ex) {
                        MessageBox.Show(ex.Message, ex.Header, MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    if (popup.IsOpen) popup.IsOpen = false;
                }
            }
        }

        private void accountList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {          
            _startPoint = e.GetPosition(accountList);

            if (!_lShiftKeyPressed && !_rShiftKeyPressed && !_lCtrlKeyPressed && !_rCtrlKeyPressed) {
                // prevent deselect of selecting items when clicking on selected item
                ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(accountList, _startPoint);
                if (lbi != null && lbi.IsSelected) {
                    var btn = UIHelpers.TryFindFromPoint<Button>(accountList, _startPoint);
                    var cb = UIHelpers.TryFindFromPoint<CheckBox>(accountList, _startPoint);
                    if (btn == null && cb == null) e.Handled = true;
                }
            }
        }

        private void accountList_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if (!_lShiftKeyPressed && !_rShiftKeyPressed && !_lCtrlKeyPressed && !_rCtrlKeyPressed) {
                ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(accountList, _startPoint);
                if (lbi != null) {
                    accountList.UnselectAll();
                    lbi.IsSelected = true;
                }
            }
        }
        
        private void accountList_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.LeftShift: 
                    _lShiftKeyPressed = true;
                    break;

                case Key.RightShift:
                    _rShiftKeyPressed = true;
                    break;

                case Key.LeftCtrl:
                    _lCtrlKeyPressed = true;
                    break;

                case Key.RightCtrl:
                    _rCtrlKeyPressed = true;
                    break;
            }
        }

        private void accountList_PreviewKeyUp(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.LeftShift:
                    _lShiftKeyPressed = false;
                    break;

                case Key.RightShift:
                    _rShiftKeyPressed = false;
                    break;

                case Key.LeftCtrl:
                    _lCtrlKeyPressed = false;
                    break;

                case Key.RightCtrl:
                    _rCtrlKeyPressed = false;
                    break;
            }
        }

        private void accountList_DragOver(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData")) {
                e.Effects = DragDropEffects.None;
            } else {
                AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;
                e.Effects = DragDropEffects.Move;
                data.AllowDrop = true;
            }
        }

        private void accountList_DragEnter(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData")) {
                e.Effects = DragDropEffects.None;
            }
        }

        private void accountList_DragLeave(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("AccountListDragDropData")) {
                AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;
                e.Effects = DragDropEffects.None;
                data.AllowDrop = false;
            }
        }


        private void accountList_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("AccountListDragDropData")) {
                AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;

                // remove existing assignments
                foreach (IBalanceListEntry account in data.Accounts) {
                    Document.RemoveAssignment(account);
                    account.RemoveFromParents();
                }
            }
        }
        
        private void btnDeleteAssignments_Click(object sender, RoutedEventArgs e) {
            var owner = UIHelpers.TryFindParent<Window>(this);
            if (!RightManager.HasDocumentRestWriteRight(Document, owner)) return;
            if (MessageBox.Show(
                owner, "Möchten Sie wirklich die ausgewählten Zuordnungen Löschen?",
                "Zuordnungen Löschen", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes) {
                List<IBalanceListEntry> removeList = new List<IBalanceListEntry>();
                foreach (IBalanceListEntry account in this.Document.SelectedBalanceList.AssignedItems) {
                    if (account.IsChecked) removeList.Add(account);
                }

                Remove(removeList);
            }
        }

        private void Remove(List<IBalanceListEntry> removeList) {
            if (!RightManager.HasDocumentRestWriteRight(Document, UIHelpers.TryFindParent<Window>(this))) return;
            foreach (IBalanceListEntry account in removeList) {
                account.IsChecked = false;
                List<IPresentationTreeNode> tmpParents = new List<IPresentationTreeNode>(account.Parents);
                foreach (var node in tmpParents) node.RemoveChildren(account);
            }
            this.Document.RemoveAssignments(removeList);
        }

        private void btnEditBalanceList(object sender, RoutedEventArgs e) {
            var owner = UIHelpers.TryFindParent<Window>(this);
            var dlg = new DlgEditBalanceList(Document.SelectedBalanceList) { Owner = owner };
            dlg.ShowDialog();
        }

        private void btnSortBalanceList(object sender, RoutedEventArgs e) { SortConfigPopup.IsOpen = true; }

        private void btnFilterBalanceList(object sender, RoutedEventArgs e) { FilterPopup.IsOpen = true; }

    }
}
