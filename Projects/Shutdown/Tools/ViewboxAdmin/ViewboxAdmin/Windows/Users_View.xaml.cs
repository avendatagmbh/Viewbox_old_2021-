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
using ViewboxAdmin.ViewModels.Users;

namespace ViewboxAdmin.Windows
{
    /// <summary>
    /// Interaction logic for Users_View.xaml
    /// </summary>
    public partial class Users_View : UserControl
    {
        public Users_View() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            UsersViewModel viewModel = (DataContext as UsersViewModel);
            viewModel.VerifyUserDelete += Approve_UserDelete;
            viewModel.CreateOrEditUser += viewModel_CreateOrEditUser;
            viewModel.ExceptionOccured += viewModel_ExceptionOccured;
            viewModel.UniqueKeyWarning += viewModel_UniqueKeyWarning;
        }

        void viewModel_UniqueKeyWarning(object sender, Structures.MessageBoxActions e) {
            MessageBox.Show("The user with the given name is already exist", "Invalid user");
        }

        void viewModel_ExceptionOccured(object sender, System.IO.ErrorEventArgs e) {
            MessageBox.Show(e.GetException().Message, "Error", MessageBoxButton.OK);
        }

        private void viewModel_CreateOrEditUser(object sender, NewUserEventArg e) {
            UserEditorWindow userEditorWindow = new UserEditorWindow();
            userEditorWindow.DataContext = e.User;
            bool? result = userEditorWindow.ShowDialog();
            if (result==true) {
                e.Yes();
            }
            else {
                e.No();
            }
        }

        private void Approve_UserDelete(object sender, Structures.MessageBoxActions e) {
            MessageBoxResult result = MessageBox.Show("Do you really want to remove the selected user?", "Delete user",MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK) {
                e.OnYesClick();
            }
            else {
                e.OnNoClick();
            }
        }
    }
}
