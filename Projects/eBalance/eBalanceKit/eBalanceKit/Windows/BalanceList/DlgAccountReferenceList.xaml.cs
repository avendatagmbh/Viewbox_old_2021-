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

namespace eBalanceKit.Windows.BalanceList {
    /// <summary>
    /// Interaction logic for DlgAccountReferenceList.xaml
    /// </summary>
    public partial class DlgAccountReferenceList : Window {
        public DlgAccountReferenceList() {
            InitializeComponent();
        }

        private void CtlAccountReferenceList_Ok(object sender, RoutedEventArgs e) { this.Close(); }
        private void CtlAccountReferenceList_Cancel(object sender, RoutedEventArgs e) { this.Close(); }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) { e.Handled = true; Close(); }
        }
    }
}
