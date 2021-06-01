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

namespace eBalanceKit.Windows {
    /// <summary>
    /// Interaktionslogik für DlgHelp.xaml
    /// </summary>
    public partial class DlgHelp : Window {
        public DlgHelp() {
            InitializeComponent();
        }

        private void NoButtonClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
