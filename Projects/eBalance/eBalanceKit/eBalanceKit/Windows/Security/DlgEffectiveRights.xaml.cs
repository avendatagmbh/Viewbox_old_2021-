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
    /// Interaktionslogik für DlgEffectiveRights.xaml
    /// </summary>
    public partial class DlgEffectiveRights : Window {
        public DlgEffectiveRights() { InitializeComponent(); }
        private void CtlEffectiveRights_Ok(object sender, RoutedEventArgs e) { Close(); }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                Close();
            }
        }
    }
}
