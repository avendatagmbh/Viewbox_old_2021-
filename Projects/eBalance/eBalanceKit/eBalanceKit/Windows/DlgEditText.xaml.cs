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
    /// Interaktionslogik für DlgEditText.xaml
    /// </summary>
    public partial class DlgEditText : Window {
        
        public DlgEditText() {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                this.DialogResult = false;
            } else if (e.Key == Key.F2) {
                this.DialogResult = true;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }
        
        public string Header {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(DlgEditText), new UIPropertyMetadata("-"));



        public string Value {
            get { return (string)GetValue(ValueProperty); }
            set { 
                SetValue(ValueProperty, value);
                txtValue.Focus();
                txtValue.Text = value;
                txtValue.SelectAll();
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(string), typeof(DlgEditText), new UIPropertyMetadata(string.Empty));

        
        
    }
}
