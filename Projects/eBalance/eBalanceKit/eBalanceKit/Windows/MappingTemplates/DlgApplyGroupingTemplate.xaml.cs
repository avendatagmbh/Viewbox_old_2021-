// --------------------------------------------------------------------------------
// author: Márton Garai
// since: 2012-09-17
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Structures;
using eBalanceKitBusiness;
using eBalanceKitBusiness.Interfaces.BalanceList;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.MappingTemplate;

namespace eBalanceKit.Windows.MappingTemplates
{
    /// <summary>
    /// Interaction logic for DlgApplyGroupingTemplate.xaml
    /// </summary>
    public partial class DlgApplyGroupingTemplate : Window
    {
        public DlgApplyGroupingTemplate() {
            InitializeComponent();
        }
        #region fields

        /// <summary>
        /// Start position of mouse drag event, needed to init drag&drop process.
        /// </summary>
        private Point _startPoint;

        #endregion fields

        #region properties
        internal AccountGroupingModel Model { get { return DataContext as AccountGroupingModel; } }
        #endregion properties

        private void BtnCreateAccountGroupClick(object sender, RoutedEventArgs e) {
            // different implementation from the DlgEditBalanceList.xaml.cs
            // there is no saving of the AccountGroup, only an AccountGroupInfo is added.
            var group = AccountGroupManager.CreateAccountGroup(Model.SelectedBalanceList);
            
            group.Validate(); // init validation messages

            DlgGroupAccount dlg = new DlgGroupAccount {
                Owner = this,
                DataContext = group,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value) {
                Model.SelectedBalanceListInfo.AccountGroupByBalanceList.Add(new AccountGroupInfo {
                    AccountName = group.Name,
                    AccountNumber = group.Number,
                    MaybeBalanceList = Model.SelectedBalanceList,
                    ContainingList = Model.SelectedBalanceListInfo.AccountGroupByBalanceList,
                    GroupComment = group.Comment,
                    ChildrenInfos = new ObservableCollection<AccountGroupChildInfo>(),
                    IsSelected = false
                });
            }
            AccountGroupingModel model = (AccountGroupingModel) DataContext;
            model.RefreshInfoLines();
        }


        //--------------------------------------------------------------------------------

        #region tvAccountGroups

        private void TvAccountGroupsPreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed || tvAccountGroups.SelectedItem == null ||
                !(tvAccountGroups.SelectedItem is AccountGroupChildInfo)) return;
            Point position = e.GetPosition(tvAccountGroups);

            TreeViewItem tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tvAccountGroups, position);
            if (tvi == null) return;
            AccountGroupChildInfo childInfo = tvi.Header as AccountGroupChildInfo;
            if (childInfo == null) return;

            if (Math.Abs(position.X - _startPoint.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(position.Y - _startPoint.Y) <= SystemParameters.MinimumVerticalDragDistance) return;

            IBalanceListEntry draggedItem = null;
            foreach (IAccount account in childInfo.Parent.MaybeBalanceList.Accounts) {
                if (account.Number == childInfo.Number) {
                    draggedItem = account;
                    break;
                }
            }
            Debug.Assert(draggedItem != null);
            draggedItem.IsVisibleForGroupMapping = true;
            childInfo.Parent.ChildrenInfos.Remove(childInfo);
            childInfo.Parent.RefreshCount();
            AccountGroupingModel model = (AccountGroupingModel) DataContext;
            model.RefreshInfoLines();
            draggedItem.IsVisibleForGroupMapping = true;

            AccountListDragDropData data = new AccountListDragDropData(new List<IBalanceListEntry> {draggedItem});
            DataObject dragData = new DataObject("AccountListDragDropData", data);

            DragDropPopup.DataContext = data;
            if (!DragDropPopup.IsOpen) DragDropPopup.IsOpen = true;

            DragDrop.DoDragDrop(tvAccountGroups, dragData, DragDropEffects.Move);
            if (DragDropPopup.IsOpen) DragDropPopup.IsOpen = false;
        }

        private void TvAccountGroupsPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) { _startPoint = e.GetPosition(tvAccountGroups); }

        private void TvAccountGroupsDrop(object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData")) return;
            AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;
            Debug.Assert(data != null);
            var tv = (TreeView)sender;
            var pos = e.GetPosition(tv);

            var tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(tv, pos);
            if (tvi == null) return;

            AccountGroupInfo groupInfo = tvi.Header as AccountGroupInfo;
            AccountGroupChildInfo groupChildInfo = tvi.Header as AccountGroupChildInfo;
            if (groupChildInfo != null) {
                groupInfo = groupChildInfo.Parent;
            }
            if (groupInfo == null)
                return;
            foreach (IBalanceListEntry balanceListEntry in data.Accounts) {
                balanceListEntry.IsVisibleForGroupMapping = false;
                groupInfo.ChildrenInfos.Add(new AccountGroupChildInfo {
                    Comment = balanceListEntry.Comment,
                    Number = balanceListEntry.Number,
                    Name = balanceListEntry.Name,
                    IsSelected = false,
                    IsVisible = true,
                    Parent = groupInfo
                });
                groupInfo.RefreshCount();
            }
            AccountGroupingModel model = (AccountGroupingModel) DataContext;
            model.RefreshInfoLines();
        }

        private void TvAccountGroupsDragOver(object sender, DragEventArgs e) { AccountsDragOver(e); }

        private void TvAccountGroupsDragEnter(object sender, DragEventArgs e) { AccountsDragEnter(e); }

        private void TvAccountGroupsDragLeave(object sender, DragEventArgs e) { AccountsDragLeave(e); }

        private void TvAccountGroupsKeyDown(object sender, KeyEventArgs e) {
            if (e.Key != Key.Delete) return;
            e.Handled = true;
            var tv = (TreeView)sender;

            if (tv.SelectedItem == null || !(tv.SelectedItem is IAccount)) return;
            var account = tv.SelectedItem as IAccount;
            account.IsSelected = false;
            account.AccountGroup.IsSelected = true;
            account.AccountGroup.RemoveAccount(account);
        }

        //--------------------------------------------------------------------------------
        #endregion tvAccountGroups

        #region accountList
        //--------------------------------------------------------------------------------

        private void AccountListPreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton != MouseButtonState.Pressed || accountList.SelectedItem == null) return;
            ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(accountList, _startPoint);
            if (lbi == null) return; // no account item under mouse pointer

            Point position = e.GetPosition(accountList);
            if (Math.Abs(position.X - _startPoint.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(position.Y - _startPoint.Y) <= SystemParameters.MinimumVerticalDragDistance) return;
            List<IBalanceListEntry> accounts = new List<IBalanceListEntry>();
            foreach (object obj in accountList.SelectedItems) {
                accounts.Add((IBalanceListEntry) obj);
            }

            AccountListDragDropData data = new AccountListDragDropData(accounts);
            DataObject dragData = new DataObject("AccountListDragDropData", data);

            DragDropPopup.DataContext = data;
            if (!DragDropPopup.IsOpen) DragDropPopup.IsOpen = true;

            DragDrop.DoDragDrop(accountList, dragData, DragDropEffects.Move);

            if (DragDropPopup.IsOpen) DragDropPopup.IsOpen = false;
        }

        private void AccountListPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(accountList);

            //if (_lShiftKeyPressed || _rShiftKeyPressed || _lCtrlKeyPressed || _rCtrlKeyPressed) return;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) return;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Control) return;
            // prevent deselect of selecting items when clicking on selected item
            ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(accountList, _startPoint);
            if (lbi == null || !lbi.IsSelected) return;
            var btn = UIHelpers.TryFindFromPoint<Button>(accountList, _startPoint);
            var cb = UIHelpers.TryFindFromPoint<CheckBox>(accountList, _startPoint);
            if (btn == null && cb == null) e.Handled = true;
        }

        private void AccountListPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            //if (_lShiftKeyPressed || _rShiftKeyPressed || _lCtrlKeyPressed || _rCtrlKeyPressed) return;
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) return;
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Control) return;
            ListBoxItem lbi = UIHelpers.TryFindFromPoint<ListBoxItem>(accountList, _startPoint);
            if (lbi == null) return;
            accountList.UnselectAll();
            lbi.IsSelected = true;
        }

        private void AccountListDragOver(object sender, DragEventArgs e) { AccountsDragOver(e); }

        private void AccountListDragEnter(object sender, DragEventArgs e) { AccountsDragEnter(e); }

        private void AccountListDragLeave(object sender, DragEventArgs e) { AccountsDragLeave(e); }

        //--------------------------------------------------------------------------------
        #endregion accountList Drag&Drop

        private void WindowDragOver(object sender, DragEventArgs e) {
            if (DragDropPopup.IsOpen) {
                Size popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
                DragDropPopup.PlacementRectangle = new Rect(e.GetPosition(this), popupSize);
            }
        }

        private void DragItemGiveFeedback(object sender, GiveFeedbackEventArgs e) {

            AccountListDragDropData data = DragDropPopup.DataContext as AccountListDragDropData;
            if (data == null) return;

            popupBorder.Opacity = data.AllowDrop ? 1.0 : 0.5;
        }

        private static void AccountsDragEnter(DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData")) {
                e.Effects = DragDropEffects.None;
            }
        }

        private static void AccountsDragOver(DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData")) {
                e.Effects = DragDropEffects.None;
            } else {
                AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;
                Debug.Assert(data != null);
                e.Effects = DragDropEffects.Move;
                data.AllowDrop = true;
            }
        }

        private static void AccountsDragLeave(DragEventArgs e) {
            if (!e.Data.GetDataPresent("AccountListDragDropData")) return;
            AccountListDragDropData data = e.Data.GetData("AccountListDragDropData") as AccountListDragDropData;
            Debug.Assert(data != null);
            e.Effects = DragDropEffects.None;
            data.AllowDrop = false;
        }

        /*private void BtnReimportBalanceListClick(object sender, RoutedEventArgs e) {
            var slideControl = UIHelpers.TryFindParent<SlideControlBase>(sender as Button);
            if (slideControl != null) slideControl.ShowChild(0);
        }

        private void BtnTemplatesClick(object sender, RoutedEventArgs e) {
            var slideControl = UIHelpers.TryFindParent<SlideControlBase>(sender as Button);
            if (slideControl != null) slideControl.ShowChild(1, new BalListTemplatesModel());
        }*/

        private void BtnFilterAccountList(object sender, RoutedEventArgs e) { AccountFilterPopup.IsOpen = true; }

        private void BtnSortAccountList(object sender, RoutedEventArgs e) { SortConfigPopup.IsOpen = true; }

        //private void BtnFilterAccountGroupList(object sender, RoutedEventArgs e) { AccountGroupsFilterPopup.IsOpen = true; }

        private void FinishTemplateGroupings(object sender, RoutedEventArgs e) {
            Model.ApplyGroupings();
            DialogResult = true;
        }

        private void Cancel(object sender, RoutedEventArgs e) { DialogResult = false; }
    }
}
