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

namespace DbSearch.Windows {
    /// <summary>
    /// Interaktionslogik für DlgSelectThreadNumber.xaml
    /// </summary>
    public partial class DlgSelectThreadNumber : Window {
        public DlgSelectThreadNumber() {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false; 
            Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }
    }
}
