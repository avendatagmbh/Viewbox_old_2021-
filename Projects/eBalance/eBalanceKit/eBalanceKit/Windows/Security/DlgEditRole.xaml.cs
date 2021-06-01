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

namespace eBalanceKit.Windows.Security {
    /// <summary>
    /// Interaktionslogik für DlgEditRole.xaml
    /// </summary>
    public partial class DlgEditRole : Window {
        public DlgEditRole() { InitializeComponent(); }
        private void CtlEditUser_Ok(object sender, RoutedEventArgs e) { Close(); }
        private void CtlEditUser_Cancel(object sender, RoutedEventArgs e) { Close(); }
    }
}
