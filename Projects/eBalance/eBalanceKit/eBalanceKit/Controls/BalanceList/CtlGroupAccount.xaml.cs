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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaktionslogik für CtlGroupAccount.xaml
    /// </summary>
    public partial class CtlGroupAccount : UserControl {
        public CtlGroupAccount() {
            InitializeComponent();
        }

        #region events

        #region Ok
        public event EventHandler Ok;
        private void OnOk() { if (Ok != null) Ok(this, new System.EventArgs()); }
        #endregion

        #region Cancel
        public event EventHandler Cancel;
        private void OnCancel() { if (Cancel != null) Cancel(this, new System.EventArgs()); }
        #endregion

        #endregion

        private void btnOk_Click(object sender, RoutedEventArgs e) { OnOk(); }        
        private void btnCancel_Click(object sender, RoutedEventArgs e) { OnCancel(); }

        private void Textbox_GotFocus(object sender, RoutedEventArgs e) {
            var tb = sender as TextBox;
            tb.SelectionStart = tb.Text.Length;
        }
    }
}
