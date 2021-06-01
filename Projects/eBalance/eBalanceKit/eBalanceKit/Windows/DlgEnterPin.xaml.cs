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
    /// Interaktionslogik für DlgEnterPin.xaml
    /// </summary>
    public partial class DlgEnterPin : Window {
        public DlgEnterPin() {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void btn1_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "1";
        }

        private void btn2_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "2";
        }

        private void btn3_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "3";
        }

        private void btn4_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "4";
        }

        private void btn5_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "5";
        }

        private void btn6_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "6";
        }

        private void btn7_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "7";
        }

        private void btn8_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "8";
        }

        private void btn9_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "9";
        }

        private void btn0_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "0";
        }

        private void btnStar_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "*";
        }

        private void btnHash_Click(object sender, RoutedEventArgs e) {
            txtPin.Password += "#";
        }

        private void btnBackspace_Click(object sender, RoutedEventArgs e) {
            if (txtPin.Password.Length > 0) txtPin.Password = txtPin.Password.Remove(txtPin.Password.Length - 1, 1);
        }

        private void btnX_Click(object sender, RoutedEventArgs e) {
            txtPin.Clear();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }

        private bool _shiftPressed;
        
        private void Window_KeyDown(object sender, KeyEventArgs e) {

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) _shiftPressed = true;
                        
            switch (e.Key) {
                case Key.Escape:
                    this.DialogResult = false;
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (txtPin.Password.Length == 0) {
                        MessageBox.Show("Es wurde noch keine PIN eingegeben.");
                    }

                    this.DialogResult = true;
                    e.Handled = true;
                    break;

                case Key.D0:
                case Key.NumPad0:
                    txtPin.Password += "0";
                    e.Handled = true;
                    break;

                case Key.D1:
                case Key.NumPad1:
                    txtPin.Password += "1";
                    e.Handled = true;
                    break;

                case Key.D2:
                case Key.NumPad2:
                    txtPin.Password += "2";
                    e.Handled = true;
                    break;

                case Key.D3:
                case Key.NumPad3:
                    txtPin.Password += "3";
                    e.Handled = true;
                    break;

                case Key.D4:
                case Key.NumPad4:
                    txtPin.Password += "4";
                    e.Handled = true;
                    break;

                case Key.D5:
                case Key.NumPad5:
                    txtPin.Password += "5";
                    e.Handled = true;
                    break;

                case Key.D6:
                case Key.NumPad6:
                    txtPin.Password += "6";
                    e.Handled = true;
                    break;

                case Key.D7:
                case Key.NumPad7:
                    txtPin.Password += "7";
                    e.Handled = true;
                    break;

                case Key.D8:
                case Key.NumPad8:
                    txtPin.Password += "8";
                    e.Handled = true;
                    break;

                case Key.D9:
                case Key.NumPad9:
                    txtPin.Password += "9";
                    e.Handled = true;
                    break;

                case Key.Oem2: // #
                    txtPin.Password += "#";
                    e.Handled = true;
                    break;

                case Key.OemPlus:
                    if (_shiftPressed) {
                        txtPin.Password += "*";
                        e.Handled = true;
                    }
                    break;

                case Key.Multiply:
                    txtPin.Password += "*";
                    e.Handled = true;
                    break;

                case Key.Back:
                    if (txtPin.Password.Length > 0) txtPin.Password = txtPin.Password.Remove(txtPin.Password.Length - 1, 1);
                    e.Handled = true;
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) _shiftPressed = false;
        }
    }
}
