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
    /// Interaction logic for MergerView.xaml
    /// </summary>
    public partial class MergerView : UserControl
    {
        public MergerView() {
            InitializeComponent();
        }

        public Merger_ViewModel VM = null;

       
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // make the ViewModel available here... only for testing purpose
             //ViewModel = DataContext as Merger_ViewModel;
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            //ViewModel.MergeDataBases();
        }

        
    }
}
