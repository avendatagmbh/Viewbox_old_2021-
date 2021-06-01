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
using AvdWpfControls;
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Windows {
    /// <summary>
    /// Interaction logic for DlgWelcomeNewVersion.xaml
    /// </summary>
    public partial class DlgWelcomeNewVersion : Window {
        public DlgWelcomeNewVersion() {
            InitializeComponent();
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            e.Handled = true;
            DialogResult = true;
        }

    }
}
