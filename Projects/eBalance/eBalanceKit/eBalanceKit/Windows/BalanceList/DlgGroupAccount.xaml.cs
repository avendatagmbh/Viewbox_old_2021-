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
    /// Interaktionslogik für DlgGroupAccounts.xaml
    /// </summary>
    public partial class DlgGroupAccount : Window {
        public DlgGroupAccount() {
            InitializeComponent();

            ctlGroupAccount.Ok += new EventHandler(ctlGroupAccount_Ok);
            ctlGroupAccount.Cancel += new EventHandler(ctlGroupAccount_Cancel);
        }

        void ctlGroupAccount_Cancel(object sender, System.EventArgs e) {
            UnregisterEventHandler();
            this.DialogResult = false;
        }

        void ctlGroupAccount_Ok(object sender, System.EventArgs e) {
            UnregisterEventHandler();
            this.DialogResult = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        private void UnregisterEventHandler() {
            ctlGroupAccount.Ok -= new EventHandler(ctlGroupAccount_Ok);
            ctlGroupAccount.Cancel -= new EventHandler(ctlGroupAccount_Cancel);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) { e.Handled = true;  this.Close(); }
        }
    }
}
