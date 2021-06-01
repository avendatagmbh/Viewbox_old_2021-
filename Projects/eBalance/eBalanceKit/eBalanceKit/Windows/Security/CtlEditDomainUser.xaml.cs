using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using eBalanceKit.Windows.Security.Models;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitResources.Localisation;
using System.Collections.ObjectModel;
using System.Linq;

namespace eBalanceKit.Windows.Security {
    /// <summary>
    /// Interaktionslogik für CtlEditDomainUser.xaml
    /// </summary>
    public partial class CtlEditDomainUser : Window {
        public CtlEditDomainUser() {
            InitializeComponent();
            txtName.Focus();
            Loaded += delegate
            {
                InitUserSelection();
            }; 
        }
        #region events
        //--------------------------------------------------------------------------------
        public event RoutedEventHandler Cancel;
        private void OnCancel() {
            Model.CancelUserEdit();
            if (Cancel != null)
                Cancel(this, new RoutedEventArgs());
            this.Close();
        }

        public event RoutedEventHandler Ok;
        private void OnOk() {
            SaveChanges();
            if (Ok != null)
                Ok(this, new RoutedEventArgs());
            this.Close();
        }
        //--------------------------------------------------------------------------------
        #endregion events

	private bool _selectionChanged = false;
        private CtlSecurityManagementModel Model { get { return DataContext as CtlSecurityManagementModel; } }
        private eBalanceKitBusiness.Structures.DbMapping.User User { get { return Model.EditedUser; } }

        public bool Validate() {
            bool result = true;

            // validate name
            if (string.IsNullOrEmpty(txtName.Text)) {
                errorShortName.Message = ResourcesCommon.UserShortnameErrorEmpty;
                result = false;
            } else {

                if ((User.UserName == null || !User.UserName.ToLower().Equals(txtName.Text.ToLower())) &&
                    UserManager.Instance.Exists(txtName.Text)) {

                    errorShortName.Message = ResourcesCommon.UserShortnameErrorUsed;
                    result = false;
                } else {
                    errorShortName.Message = null;
                }
            }

            if (chkIsCompanyAdmin.IsChecked.HasValue && chkIsCompanyAdmin.IsChecked.Value) {
                if (lstCompanies.SelectedItems.Count == 0) {
                    errorCompanyAdmin.Message = ResourcesCommon.CompanyAdminNoCompanySelectedError;
                    result = false;
                }
            }

            errorPassword.Message = null;
            if (User.PasswordHash == null) {
                if (string.IsNullOrEmpty(txtPassword.Password)) {
                    result = false;
                    errorPassword.Message = ResourcesCommon.MissingPasswordError;
                } else if (txtPassword.Password != txtPassword2.Password) {
                    result = false;
                    errorPassword.Message = ResourcesCommon.PaswordsNotEqualError;
                }
            } else if (txtPassword.Password != txtPassword2.Password && (txtPassword.Password.Length > 0 || txtPassword2.Password.Length > 0)) {
                if (txtPassword.Password != txtPassword2.Password) {
                    result = false;
                    errorPassword.Message = ResourcesCommon.PaswordsNotEqualError;
                }
            }

            return result;
        }

        private void SaveChanges() {
            UpdateSources();
            Model.SaveEditedUser();
        }

        public void UpdateSources() {
            txtName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            txtFullName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            chkIsActive.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            chkIsAdmin.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();
            chkIsCompanyAdmin.GetBindingExpression(CheckBox.IsCheckedProperty).UpdateSource();

            if (_selectionChanged) {
                Model.EditedUser.AssignedCompanies.Clear();
                foreach (Company item in lstCompanies.SelectedItems) {
                    Model.EditedUser.AssignedCompanies.Add(new CompanyInfo(item));
                }
            }

            if (txtPassword.Password.Length > 0 || txtPassword2.Password.Length > 0) {
                UserManager.Instance.SetPassword(User, txtPassword.Password);
            }
        }

        private void Validate(object sender, RoutedEventArgs e) { /* Validate(); */ }
        private void btnOk_Click(object sender, RoutedEventArgs e) { if (Validate()) OnOk(); }
        private void btnAcceptChanges_Click(object sender, RoutedEventArgs e) { if (Validate()) SaveChanges(); }
        private void btnCancel_Click(object sender, RoutedEventArgs e) { OnCancel(); }
        private void CtlEditDomainUser_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) { e.Handled = true; OnCancel(); } }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            //DEVNOTE: should not save the changes promptly, only on OK button click
            //foreach (Company addedItem in e.AddedItems) {
            //    if (!Model.EditedUser.AssignedCompanies.Any(c => c.Id == addedItem.Id))
            //        Model.EditedUser.AssignedCompanies.Add(new CompanyInfo(addedItem));
            //}
            //foreach (Company addedItem in e.RemovedItems) {
            //    Model.EditedUser.AssignedCompanies.Remove(new CompanyInfo(addedItem));
            //}
            _selectionChanged = true;
        }

        private void InitUserSelection() {
            foreach (Company item in lstCompanies.Items) {
                if (Model.EditedUser.AssignedCompanies.Any(c => c.Id == item.Id))
                    lstCompanies.SelectedItems.Add(item);
            }

            lstCompanies.SelectionChanged += OnSelectionChanged;
        }

        private void CtlEditDomainUser_Loaded(object sender, RoutedEventArgs e) {
            if (Model.SelectedUser == null || !Model.SelectedUser.IsDomainUser)
                return;
            stckplPassword.Visibility = Visibility.Collapsed;
            txtFullName.IsEnabled = false;
            txtName.IsEnabled = false;
        }
    }
}
