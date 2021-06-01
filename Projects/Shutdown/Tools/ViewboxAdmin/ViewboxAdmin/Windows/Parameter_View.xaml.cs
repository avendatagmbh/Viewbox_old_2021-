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
    /// Interaction logic for Parameter_View.xaml
    /// </summary>
    public partial class Parameter_View : UserControl
    {
        public Parameter_View() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            (DataContext as Parameters_ViewModel).DataContextChange+=Parameter_View_DataContextChange;
        }

        void Parameter_View_DataContextChange(object sender, CustomEventArgs.DataContextChangeEventArg<CollectionEdit_ViewModel> e) {
            UserControl collectionView = new Collection_View();
            collectionView.DataContext = e.ViewModel;
            placeholder.Children.Clear();
            placeholder.Children.Add(collectionView);
        }

        

        
    }
}
