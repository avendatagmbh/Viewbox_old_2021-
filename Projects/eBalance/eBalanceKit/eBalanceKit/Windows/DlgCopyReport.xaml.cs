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
    /// Interaktionslogik für DlgCopyReport.xaml
    /// </summary>
    public partial class DlgCopyReport : Window {
        public DlgCopyReport() {
            InitializeComponent();
        }
        private void YesButtonClick(object sender, RoutedEventArgs e) { DialogResult = true; }

        private void NoButtonClick(object sender, RoutedEventArgs e) { DialogResult = false; }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    NoButtonClick(sender, null);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    YesButtonClick(sender, null);
                    e.Handled = true;
                    break;
            }
        }
    }
}
