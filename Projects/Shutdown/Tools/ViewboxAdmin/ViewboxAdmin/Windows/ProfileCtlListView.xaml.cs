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
using ViewboxAdmin.ViewModels;

namespace ViewboxAdmin.Windows
{
    /// <summary>
    /// Interaction logic for AttilaCtlListView.xaml
    /// </summary>
    public partial class ProfileCtlListView : UserControl
    {
        public ProfileCtlListView() {
            InitializeComponent();
        }

        public ProfileManagement_ViewModel ViewModel;

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ViewModel = DataContext as ProfileManagement_ViewModel;
                ViewModel.DeleteItem += DeleteProfileMessageBox;
                ViewModel.AddNewProfile += AddNewProfileWindow;
        }

        private void AddNewProfileWindow(object sender, Structures.NewProfileWindowEventArg e) {
            // showing a new window for creating a new profile
            DlgNewProfile dlgNewProfileDialog = new DlgNewProfile();
            dlgNewProfileDialog.DataContext = e.CreateNewProfileVM;
            dlgNewProfileDialog.Show();
        }

        private void DeleteProfileMessageBox(object sender, Structures.MessageBoxActions e) {
            //showing a messagebox for verifying the profile deletion
            var messageBoxText = "Do you really want to delete the selected profile?";
            var messageBoxCaption = "Delete Profile";
            MessageBoxResult result = MessageBox.Show(messageBoxText, messageBoxCaption,
                                                                MessageBoxButton.YesNo, MessageBoxImage.Question,
                                                                MessageBoxResult.No);

            if (result== MessageBoxResult.Yes) {
                e.OnYesClick();
            }
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e) {
            // should be replaced by commands
           ViewModel.Delete();
        }

        private void btnAddItem_Click(object sender, RoutedEventArgs e) {
            // should be replaced by commands
            ViewModel.AddNewProfileRequest();
        }
    }
}
