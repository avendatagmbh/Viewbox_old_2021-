// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKitResources.Localisation;
using eBalanceKit.Windows.Security.Models;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;

namespace eBalanceKit.Windows.Security {
    /// <summary>
    /// Interaktionslogik für CtlEditRole.xaml
    /// </summary>
    public partial class CtlEditRole : UserControl {
        public CtlEditRole() {
            InitializeComponent();
            txtName.Focus();
        }

        private Window Owner {
            get { return UIHelpers.TryFindParent<Window>(this); }
        }

        private CtlSecurityManagementModel Model {
            get { return DataContext as CtlSecurityManagementModel; }
        }

        private Role Role {
            get { return Model.EditedRole; }
        }

        private void SaveChanges() {
            UpdateSources();
            Model.SaveEditedRole();
        }

        public void UpdateSources() {
            txtName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtComment.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            Model.SaveEditedRole();
        }

        public bool Validate() {
            bool result = true;

            // validate name
            if (string.IsNullOrEmpty(txtName.Text)) {
                errorName.Message = ResourcesCommon.RoleNameErrorEmpty;
                result = false;
            } else {
                if ((Role.Name == null || !Role.Name.ToLower().Equals(txtName.Text.ToLower())) &&
                    RoleManager.Exists(txtName.Text)) {
                    errorName.Message = ResourcesCommon.RoleNameErrorUsed;
                    result = false;
                } else {
                    errorName.Message = null;
                }
            }

            return result;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                SaveChanges();
                Owner.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Model.CancelRoleEdit();
            Owner.Close();
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                Model.CancelRoleEdit();
                Owner.Close();
            }
        }
    }
}