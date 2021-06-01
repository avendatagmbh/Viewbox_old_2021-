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

namespace ViewValidator.Windows {
    /// <summary>
    /// Interaktionslogik für DlgRowEntryDetails.xaml
    /// </summary>
    public partial class DlgRowEntryDetails : Window {
        public DlgRowEntryDetails() {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
