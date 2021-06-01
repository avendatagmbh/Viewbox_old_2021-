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
using System.Windows.Shapes;
using ViewBuilder.Models;

namespace ViewBuilder.Windows {
    /// <summary>
    /// Interaktionslogik für DlgManageFakes.xaml
    /// </summary>
    public partial class DlgManageFakes : Window {
        public DlgManageFakes() {
            InitializeComponent();
        }

        ManageFakesModel Model { get { return DataContext as ManageFakesModel; } }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Model.Finish(this);
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }
    }
}
