using System;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.Security.Models;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Security {

    /// <summary>
    /// Interaktionslogik für CtlSecurityManagement.xaml
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-08</since>
    public partial class CtlSecurityManagement : UserControl {

        public class DragDropData {
            public object Data { get; set; }

            public string DisplayString {
                get {
                    if (Data is Role) return ResourcesCommon.Role + ": " + (Data as Role).DisplayString;
                    else if (Data is User)
                        return ResourcesCommon.User + ": " +
                               (Data as User).DisplayString;
                    else return "unknown drag&drop data";
                }
            }

            public bool AllowDrop { get; set; }
        }

        public CtlSecurityManagement() {
            InitializeComponent();
            
            DataContext = new CtlSecurityManagementModel();

            if (IPGlobalProperties.GetIPGlobalProperties().DomainName == string.Empty)
                btnImportAdUser.Visibility = Visibility.Collapsed;
        }

        private CtlSecurityManagementModel Model { get { return DataContext as CtlSecurityManagementModel; } }

        #region user
        private void CreateNewUser() {
            if (!UserManager.Instance.CurrentUser.AllowUserManagement) return;
            Model.CreateUser();
            var owner = UIHelpers.TryFindParent<Window>(this);
            new DlgEditUser {Owner = owner, DataContext = Model}.ShowDialog();
        }
        private void CreateNewUserFromActiveDirectory() {
            if (!UserManager.Instance.CurrentUser.AllowUserManagement) return;
            var owner = UIHelpers.TryFindParent<Window>(this);
            new DlgActiveDirectoryUserImport {Owner = owner, DataContext = Model}.ShowDialog();
        }
        
        private void EditSelectedUser() {
            if (!UserManager.Instance.CurrentUser.AllowUserManagement) return;
            if (Model.SelectedUser != null && !Model.SelectedUser.IsDomainUser) {
                var owner = UIHelpers.TryFindParent<Window>(this);
                DlgEditUser dlg = new DlgEditUser {Owner = owner, DataContext = Model};
                dlg.ShowDialog();
            }
            if (Model.SelectedUser != null && Model.SelectedUser.IsDomainUser) {
                var owner = UIHelpers.TryFindParent<Window>(this);
                CtlEditDomainUser dlg = new CtlEditDomainUser { Owner = owner, DataContext = Model };
                dlg.ShowDialog();
            }
        }

        private void EditUser(User user) {
            if (!UserManager.Instance.CurrentUser.AllowUserManagement) return;
            if (Model.SelectedUser != null) {
                var owner = UIHelpers.TryFindParent<Window>(this);
                new DlgEditUser {Owner = owner, DataContext = Model}.ShowDialog();
            }
        }

        private void EditUserRights() {
            if (Model.SelectedUser == null) return;
            Role role = RoleManager.GetUserRole(Model.SelectedUser);
            role.LoadDetails();
            new DlgRoleRights {
                Owner = UIHelpers.TryFindParent<Window>(this),
                DataContext = new RoleRightTreeViewModel(role)
            }.ShowDialog();
        }

        private void ShowEffectiveRights() {
            if (Model.SelectedUser == null) return;
            new DlgEffectiveRights {
                Owner = UIHelpers.TryFindParent<Window>(this),
                DataContext = new CtlEffectiveRightsModel(Model.SelectedUser)
            }.ShowDialog();
        }

        private void DeleteUserListSelectedEntry() {
            if (!UserManager.Instance.CurrentUser.AllowUserManagement) return;
            if (Model.UserListSelectedItem == null) return;
            if (Model.UserListSelectedItem is User) {
                var user = Model.UserListSelectedItem as User;
                if (user == UserManager.Instance.CurrentUser) {
                    MessageBox.Show(
                        ResourcesCommon.DeleteCurrentUserNotAllowed,
                        ResourcesCommon.DeleteCurrentUserNotAllowedCaption,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (MessageBox.Show(
                    string.Format(ResourcesCommon.DeleteUserRequest, user.DisplayString),
                    ResourcesCommon.DeleteUserRequestTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes) {

                    Model.DeleteUser(user);
                }
            }
        }

        private void btnDeleteUser_Click(object sender, RoutedEventArgs e) {

            if (tvUsers.SelectedItem is Role) {
                
                // remove assigned role from user
                var role = tvUsers.SelectedItem as Role;
                var item = UIHelpers.GetTreeViewItem(tvUsers, role);
                var parent = ItemsControl.ItemsControlFromItemContainer(item) as TreeViewItem;
                var user = parent.Header as eBalanceKitBusiness.Structures.DbMapping.User;

                user = tvUsers.Tag as User;

                RightManager.RemoveAssignedRole(user, role);
            } else {
                // remove user
                DeleteUserListSelectedEntry();
            }
            e.Handled = true;
        }

        private void btnChangeUserRights_Click(object sender, RoutedEventArgs e) { EditUserRights(); }
        private void btnCreateUser_Click(object sender, RoutedEventArgs e) { CreateNewUser(); }
        private void btnEditUser_Click(object sender, RoutedEventArgs e) { EditSelectedUser(); }
        private void btnShowEffectiveRights_Click(object sender, RoutedEventArgs e) { ShowEffectiveRights(); }

        private void btnImportuser_Click(object sender, RoutedEventArgs e) { CreateNewUserFromActiveDirectory(); }

        #endregion user


        #region role
        private void CreateNewRole() {
            if (!UserManager.Instance.CurrentUser.AllowUserManagement) return;
            Model.CreateRole();
            var owner = UIHelpers.TryFindParent<Window>(this);
            new DlgEditRole { Owner = owner, DataContext = Model }.ShowDialog();
        }


        private void EditSelectedRole() {
            if (!UserManager.Instance.CurrentUser.AllowUserManagement) return;
            if (Model.SelectedRole != null) {
                var owner = UIHelpers.TryFindParent<Window>(this);
                new DlgEditRole { Owner = owner, DataContext = Model }.ShowDialog();
            }
        }

        private void ChangeRoleRights() {
            if (Model.SelectedRole == null) return;
            Model.SelectedRole.LoadDetails();
            new DlgRoleRights {
                Owner = UIHelpers.TryFindParent<Window>(this),
                DataContext = new RoleRightTreeViewModel(Model.SelectedRole)
            }.ShowDialog();
        }

        private void DeleteRoleListSelectedEntry() {
            if (!UserManager.Instance.CurrentUser.AllowUserManagement) return;
            if (Model.RoleListSelectedItem == null) return;
            if (Model.RoleListSelectedItem is User) {
                var user = Model.RoleListSelectedItem as User;
                if (user == UserManager.Instance.CurrentUser) {
                    MessageBox.Show(
                        ResourcesCommon.DeleteCurrentUserNotAllowed,
                        ResourcesCommon.DeleteCurrentUserNotAllowedCaption,
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (MessageBox.Show(
                    string.Format(ResourcesCommon.DeleteUserRequest, user.DisplayString),
                    ResourcesCommon.DeleteUserRequestTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes) {

                    Model.DeleteUser(user);
                }
            } else if (Model.RoleListSelectedItem is Role) {
                var role = Model.RoleListSelectedItem as Role;
                if (MessageBox.Show(
                    string.Format(ResourcesCommon.DeleteRoleRequest, role.DisplayString),
                    ResourcesCommon.DeleteUserRequestTitle,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes) {

                    Model.DeleteRole(role);
                }
            }
        }

        private void btnDeleteRole_Click(object sender, RoutedEventArgs e) {
            if (tvRoles.SelectedItem is eBalanceKitBusiness.Structures.DbMapping.User) {
                // remove assigned user from role
                var user = tvRoles.SelectedItem as eBalanceKitBusiness.Structures.DbMapping.User;
                var item = UIHelpers.GetTreeViewItem(tvRoles, user);
                var parent = ItemsControl.ItemsControlFromItemContainer(item) as TreeViewItem;
                var role = parent.Header as Role;
                RightManager.RemoveAssignedRole(user, role);
            } else {
                // remove role
                DeleteRoleListSelectedEntry();
            }
            e.Handled = true;
        }

        private void btnChangeRoleRights_Click(object sender, RoutedEventArgs e) { ChangeRoleRights(); }
        private void btnCreateRole_Click(object sender, RoutedEventArgs e) { CreateNewRole(); }
        private void btnEditRole_Click(object sender, RoutedEventArgs e) { EditSelectedRole(); }

        #endregion role


        #region drag&drop

        /// <summary>
        /// Start position of mouse drag event, needed to init drag&drop process.
        /// </summary>
        private Point _startPoint;

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            _startPoint = e.GetPosition(this);
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed && _startPoint != null && DragDropPopup.IsOpen == false) {

                Point position = e.GetPosition(this);
                if (Math.Abs(position.X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(position.Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance) {

                    TreeView dragSource = UIHelpers.TryFindFromPoint<TreeView>(this, _startPoint);
                    if (dragSource == null) return;

                    TreeViewItem tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(this, _startPoint);
                    if (tvi == null) return;

                    var parent = ItemsControl.ItemsControlFromItemContainer(tvi) as TreeViewItem;
                    if (parent != null) return; // drag&drop is only allowed for root entries

                    //if (!RightManager.HasDocumentRestWriteRight(Document, UIHelpers.TryFindParent<Window>(this))) return;

                    //List<IBalanceListEntry> accounts = new List<IBalanceListEntry>();
                    //foreach (object obj in this.accountList.SelectedItems) {
                    //    accounts.Add((IBalanceListEntry)obj);
                    //}

                    DragDropData data = new DragDropData { Data = dragSource.SelectedItem };
                    DataObject dragData = new DataObject(typeof(DragDropData), data);

                    DragDropPopup.DataContext = data;
                    if (!DragDropPopup.IsOpen) DragDropPopup.IsOpen = true;

                    DragDrop.DoDragDrop(this, dragData, DragDropEffects.Move);

                    if (DragDropPopup.IsOpen) DragDropPopup.IsOpen = false;
                }
            }
        }

        private void UserControl_PreviewDragOver(object sender, DragEventArgs e) {
            if (this.DragDropPopup.IsOpen) {
                var pos = e.GetPosition(this);

                Size popupSize = new Size(DragDropPopup.ActualWidth, DragDropPopup.ActualHeight);
                DragDropPopup.PlacementRectangle = new Rect(pos, popupSize);

                DragDropData data = DragDropPopup.DataContext as DragDropData;
                if (data != null) {
                    TreeViewItem tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(this, e.GetPosition(this));
                    if (tvi == null) {
                        data.AllowDrop = false;
                        e.Effects = DragDropEffects.None;
                        return;
                    }

                    if ((tvi.Header is Role && data.Data is eBalanceKitBusiness.Structures.DbMapping.User) ||
                        (tvi.Header is eBalanceKitBusiness.Structures.DbMapping.User && data.Data is Role)) {
                        
                        data.AllowDrop = true;
                        e.Effects = DragDropEffects.Move;

                    } else {
                        data.AllowDrop = false;
                        e.Effects = DragDropEffects.None;

                    }
                }

                e.Handled = true;
            }
        }

        private void UserControl_GiveFeedback(object sender, GiveFeedbackEventArgs e) {
            DragDropData data = DragDropPopup.DataContext as DragDropData;
            if (data == null) return;
            else if (data.AllowDrop) popupBorder.Opacity = 1.0;
            else popupBorder.Opacity = 0.5;
        }
        /*
                    if (e.Data.GetDataPresent(typeof(DragDropData))) {
                        DragDropData data = e.Data.GetData(typeof(DragDropData)) as DragDropData;
                        data.AllowDrop = false;
                        //e.Effects = DragDropEffects.None;
                    }           
         */

        #endregion drag&drop

        private void UserControl_Drop(object sender, DragEventArgs e) {

            
            TreeViewItem tvi = UIHelpers.TryFindFromPoint<TreeViewItem>(this, e.GetPosition(this));
            if (tvi == null) return;

            DragDropData data = null;
            if (e.Data.GetDataPresent(typeof(DragDropData))) 
                data = e.Data.GetData(typeof(DragDropData)) as DragDropData;

            if (data == null || data.AllowDrop == false) return;

            if (tvi.Header is Role) {
                var role = tvi.Header as Role;
                var user = data.Data as eBalanceKitBusiness.Structures.DbMapping.User;
                RightManager.AddAssignedRole(user, role);
                e.Handled = true;

            } else if (tvi.Header is eBalanceKitBusiness.Structures.DbMapping.User) {
                var role = data.Data as Role;
                var user = tvi.Header as eBalanceKitBusiness.Structures.DbMapping.User;
                RightManager.AddAssignedRole(user, role); 
                e.Handled = true;           
            }
            
        }



        private void tvUsers_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Model.UserListSelectedItem = tvUsers.SelectedItem;
        }

        private void tvRoles_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Model.RoleListSelectedItem = tvRoles.SelectedItem;
        }

        private void tvUsers_DragEnter(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.Move;
        }

        private void tvUsers_DragLeave(object sender, DragEventArgs e) {
            e.Effects = DragDropEffects.None;
        }

        private void tvUsers_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Delete:
                    if (tvUsers.SelectedItem is Role) {
                        // remove assigned role from user
                        var role = tvUsers.SelectedItem as Role;
                        TreeViewItem item = e.OriginalSource as TreeViewItem;
                        var parent = ItemsControl.ItemsControlFromItemContainer(item) as TreeViewItem;
                        var user = parent.Header as eBalanceKitBusiness.Structures.DbMapping.User;
                        RightManager.RemoveAssignedRole(user, role);
                    } else {
                        // remove user
                        DeleteUserListSelectedEntry();
                    }
                    e.Handled = true;
                    break;

                case Key.Insert:
                    CreateNewUser();
                    e.Handled = true;
                    break;
                case Key.F5:
                    CreateNewUserFromActiveDirectory();
                    e.Handled=true;
                    break;

                case Key.F2:
                    EditSelectedUser();
                    e.Handled = true;
                    break;

                case Key.F3:
                    EditUserRights();
                    e.Handled = true;
                    break;

                case Key.F4:
                    ShowEffectiveRights();
                    e.Handled = true;
                    break;
            }

            tvUsers.Focus();
        }

        private void tvRoles_KeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Delete:
                    if (tvRoles.SelectedItem is eBalanceKitBusiness.Structures.DbMapping.User) {
                        // remove assigned user from role
                        var user = tvRoles.SelectedItem as eBalanceKitBusiness.Structures.DbMapping.User;
                        TreeViewItem item = e.OriginalSource as TreeViewItem;
                        var parent = ItemsControl.ItemsControlFromItemContainer(item) as TreeViewItem;
                        var role = parent.Header as Role;
                        RightManager.RemoveAssignedRole(user, role);
                    } else {
                        // remove role
                        DeleteRoleListSelectedEntry();
                    }
                    e.Handled = true;
                    break;

                case Key.Insert:
                    CreateNewRole();
                    e.Handled = true;
                    break;

                case Key.F2:
                    if (tvRoles.SelectedItem is eBalanceKitBusiness.Structures.DbMapping.User) {
                        EditSelectedUser();
                    } else {
                        EditSelectedRole();
                    }
                    e.Handled = true;
                    break;

                case Key.F3:
                    ChangeRoleRights();
                    e.Handled = true;
                    break;
            }

            tvRoles.Focus();
        }
        

        private void TvUsers_OnSelected(object sender, RoutedEventArgs e) {
            if (tvUsers.SelectedItem is Role) {
                // Store the User that owns the selected role in the TreeView tag
                TreeViewItem item = e.OriginalSource as TreeViewItem;
                var parent = ItemsControl.ItemsControlFromItemContainer(item) as TreeViewItem;
                if (parent != null) {
                    var user = parent.Header as eBalanceKitBusiness.Structures.DbMapping.User;

                    tvUsers.Tag = user;
                }
            }
        }
    }
}
