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
using eBalanceKitBusiness;
using eBalanceKit.Structures;
using AvdWpfControls;

namespace eBalanceKit.Windows.BalanceList {
    /// <summary>
    /// Interaktionslogik für CtlEditBalanceList.xaml
    /// </summary>
    public partial class CtlEditBalanceList : UserControl {
        public CtlEditBalanceList() { InitializeComponent(); }

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
        internal DlgEditBalanceListModel Model { get { return DataContext as DlgEditBalanceListModel; } }
        #endregion properties

        private void btnCreateAccountGroup_Click(object sender, RoutedEventArgs e) { Model.CreateAccountGroup(); }

        //--------------------------------------------------------------------------------

        #region tvAccountGroups

        private void tvAccountGroups_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && _startPoint != null && tvAccountGroups.SelectedItem != null &&
                tvAccountGroups.SelectedItem is IAccount) {

                Point position = e.GetPosition(tvAccountGroups);

                TreeViewItem tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tvAccountGroups, position);
                if (tvi == null) return;
                if (!(tvi.Header is IAccount)) return;

                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {

                    List<IBalanceListEntry> accounts = new List<IBalanceListEntry>
                    {(IAccount) this.tvAccountGroups.SelectedItem};

                    foreach (IAccount account in accounts) {
                        account.AccountGroup.RemoveAccount(account);
                    }

                    AccountListDragDropData data = new AccountListDragDropData(accounts);
                    DataObject dragData = new DataObject("AccountListDragDropData", data);

                    DragDropPopup.DataContext = data;
                    if (!DragDropPopup.IsOpen) DragDropPopup.IsOpen = true;

                    DragDrop.DoDragDrop(tvAccountGroups, dragData, DragDropEffects.Move);
                    if (DragDropPopup.IsOpen) DragDropPopup.IsOpen = false;
                }
            }
        }

        private void tvAccountGroups_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { _startPoint = e.GetPosition(tvAccountGroups); }

        private void tvAccountGroups_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("AccountListDragDropData")) {
                AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;
                var tv = sender as TreeView;
                var pos = e.GetPosition(tv);

                var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tv, pos);
                if (tvi == null) return;

                IAccountGroup group = null;
                if (tvi.Header is IAccountGroup) {
                    group = tvi.Header as IAccountGroup;
                } else if (tvi.Header is IAccount) {
                    var dropAccount = tvi.Header as IAccount;
                    if (dropAccount.AccountGroup != null)
                        group = dropAccount.AccountGroup;
                }

                if (group != null) {
                    foreach (var account in data.Accounts) {
                        if (!(account is IAccount)) continue;
                        group.AddAccount(account as IAccount);
                    }
                }
            }
        }

        private void tvAccountGroups_DragOver(object sender, DragEventArgs e) { AccountsDragOver(e); }

        private void tvAccountGroups_DragEnter(object sender, DragEventArgs e) { AccountsDragEnter(e); }

        private void tvAccountGroups_DragLeave(object sender, DragEventArgs e) { AccountsDragLeave(e); }

        private void tvAccountGroups_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                e.Handled = true;
                var tv = sender as TreeView;

                if (tv.SelectedItem == null || !(tv.SelectedItem is IAccount)) return;
                var account = tv.SelectedItem as IAccount;
                account.IsSelected = false;
                account.AccountGroup.IsSelected = true;
                account.AccountGroup.RemoveAccount(account);
            }
        }

        //--------------------------------------------------------------------------------
        #endregion tvAccountGroups

        #region accountList
        //--------------------------------------------------------------------------------

        private void accountList_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && accountList.SelectedItem != null && _startPoint != null) {

                ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(accountList, _startPoint);
                if (lbi == null) return; // no account item under mouse pointer

                Point position = e.GetPosition(accountList);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {

                    List<IBalanceListEntry> accounts = new List<IBalanceListEntry>();
                    foreach (object obj in this.accountList.SelectedItems) {
                        accounts.Add((IBalanceListEntry) obj);
                    }

                    AccountListDragDropData data = new AccountListDragDropData(accounts);
                    DataObject dragData = new DataObject("AccountListDragDropData", data);

                    DragDropPopup.DataContext = data;
                    if (!DragDropPopup.IsOpen) DragDropPopup.IsOpen = true;

                    DragDrop.DoDragDrop(accountList, dragData, DragDropEffects.Move);

                    if (DragDropPopup.IsOpen) DragDropPopup.IsOpen = false;
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

        private void accountList_DragOver(object sender, DragEventArgs e) { AccountsDragOver(e); }

        private void accountList_DragEnter(object sender, DragEventArgs e) { AccountsDragEnter(e); }

        private void accountList_DragLeave(object sender, DragEventArgs e) { AccountsDragLeave(e); }

        private void accountList_Drop(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent("AccountListDragDropData")) {
                AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;
            }
        }

        //--------------------------------------------------------------------------------
        #endregion accountList Drag&Drop

        private void Window_DragOver(object sender, DragEventArgs e) {
            if (this.DragDropPopup.IsOpen) {
                Size popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
                DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
            }
        }

        private void dragItem_GiveFeedback(object sender, GiveFeedbackEventArgs e) {

            AccountListDragDropData data = DragDropPopup.DataContext as AccountListDragDropData;
            if (data == null) return;

            if (data.AllowDrop) {
                popupBorder.Opacity = 1.0;
                //this.Cursor = Cursors.No;
            } else {
                popupBorder.Opacity = 0.5;
                //this.Cursor = Cursors.Hand;
            }
        }

        private static void AccountsDragEnter(DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData")) {
                e.Effects = DragDropEffects.None;
                return;
            }
        }

        private static void AccountsDragOver(DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData")) {
                e.Effects = DragDropEffects.None;
            } else {
                AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;
                e.Effects = DragDropEffects.Move;
                data.AllowDrop = true;
            }
        }

        private static void AccountsDragLeave(DragEventArgs e) {
            if (e.Data.GetDataPresent("AccountListDragDropData")) {
                AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;
                e.Effects = DragDropEffects.None;
                data.AllowDrop = false;
            }
        }

        private void btnReimportBalanceList_Click(object sender, RoutedEventArgs e) {
            var slideControl = UIHelpers.TryFindParent<SlideControlBase>(sender as Button);
            if (slideControl != null) slideControl.ShowChild(0);
        }

        private void btnTemplates_Click(object sender, RoutedEventArgs e) {
            var slideControl = UIHelpers.TryFindParent<SlideControlBase>(sender as Button);
            if (slideControl != null) slideControl.ShowChild(1, dataContext: new BalListTemplatesModel());
        }

        private void btnFilterAccountList(object sender, RoutedEventArgs e) { AccountFilterPopup.IsOpen = true; }

        private void btnSortAccountList(object sender, RoutedEventArgs e) { SortConfigPopup.IsOpen = true; }

        private void btnFilterAccountGroupList(object sender, RoutedEventArgs e) { AccountGroupsFilterPopup.IsOpen = true; }

        private void btnFilterSplitAccountGroupList(object sender, RoutedEventArgs e) { SplitAccountGroupsFilterPopup.IsOpen = true; }
    }
}
