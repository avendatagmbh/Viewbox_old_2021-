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
using ViewboxAdmin.CustomEventArgs;
using ViewboxAdmin.Structures;
using ViewboxAdmin.ViewModels;

namespace ViewboxAdmin.Windows
{
    /// <summary>
    /// Interaction logic for Collection_View.xaml
    /// </summary>
    public partial class Collection_View : UserControl
    {
        public Collection_View() {
            InitializeComponent();
        }

        private void OnUserDeleteRequest(object sender, MessageBoxActions e) {
            // if the user clicks to delete parametercollection button...
            MessageBoxResult result = MessageBox.Show("Do you really want to delete the selected collection?", "Delete collection", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes) {
                e.OnYesClick();
            }
            else {
                e.OnNoClick();
            }
        }

        private void OnUserNewCollectionRequest(object sender, DataContextChangeEventArg<CreateParameterValue_ViewModel> e) {
            CreateNewParameterValue_View createNewParameterValueView = new CreateNewParameterValue_View();
            createNewParameterValueView.DataContext = e.ViewModel;
            createNewParameterValueView.Show();
            
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            (DataContext as CollectionEdit_ViewModel).UserApproveRequest += OnUserDeleteRequest;
            (DataContext as CollectionEdit_ViewModel).UserNewCollectionRequest += OnUserNewCollectionRequest;
        }

        
    }
}
