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
    /// Interaction logic for OptimizationView.xaml
    /// </summary>
    public partial class OptimizationView : UserControl
    {
        public OptimizationView() {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            (DataContext as OptimizationTree_ViewModel).DeleteOptimizationRequest += OptimizationView_DeleteOptimizationRequest;
        }

        void OptimizationView_DeleteOptimizationRequest(object sender, Structures.MessageBoxActions e) { 
            MessageBoxResult result = MessageBox.Show("Do you really want to remove the selected optimizations?","Delete optimization",MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes) {
                e.OnYesClick();
            }
            e.OnNoClick();

        }
    }
}
