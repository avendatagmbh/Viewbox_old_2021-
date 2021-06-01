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
    /// Interaktionslogik für CtlBalanceFilter.xaml
    /// </summary>
    public partial class PresentationTreeFilter : UserControl {
        public PresentationTreeFilter() {
            InitializeComponent();
        }
        #region txtFilter_KeyDown
        private void txtFilter_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                txtFilter.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                e.Handled = true;
            }
        }
        #endregion
        
        private void btnFilter_Click(object sender, RoutedEventArgs e) { txtFilter.GetBindingExpression(TextBox.TextProperty).UpdateSource(); }

        private void btnResetFilter_Click(object sender, RoutedEventArgs e) {
            txtFilter.Clear();
            txtFilter.GetBindingExpression(TextBox.TextProperty).UpdateSource();
           //Model.PresentationTree.ResetFilter();
        }
    }
}
