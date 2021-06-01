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
    /// Interaction logic for SystemDeleteDialog.xaml
    /// </summary>
    public partial class SystemDeleteView : UserControl
    {
        public SystemDeleteView() {
            InitializeComponent();
        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e) { }


        void SystemDeleteView_DeleteSystemRequest(object sender, Structures.MessageBoxActions e) {
            var messageBoxText = "Do you really want to delete the selected system? Please make a backup copy before the operation";
            var messageBoxCaption = "Delete System";
            MessageBoxResult result = MessageBox.Show(messageBoxText, messageBoxCaption,
                                                                MessageBoxButton.YesNo, MessageBoxImage.Question,
                                                                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                e.OnYesClick();
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            try {
                (DataContext as SystemDeleteViewModel).DeleteSystemRequest += SystemDeleteView_DeleteSystemRequest;
            }
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }

        }
    }
}
