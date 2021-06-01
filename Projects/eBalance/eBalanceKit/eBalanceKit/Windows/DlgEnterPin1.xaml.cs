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
    /// Interaktionslogik für DlgEnterPin1.xaml
    /// </summary>
    public partial class DlgEnterPin1 : Window {
        public DlgEnterPin1() {
            InitializeComponent();
            txtPin.Focus();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Escape:
                    DialogResult = false;
                    e.Handled = true;
                    break;

                case Key.Return:
                    DialogResult = true;
                    e.Handled = true;
                    break;
            }
        }
    }
}
