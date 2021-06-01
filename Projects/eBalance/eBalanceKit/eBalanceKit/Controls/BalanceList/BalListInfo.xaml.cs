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
using eBalanceKitBusiness;
using eBalanceKit.Windows.BalanceList;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaktionslogik für BalListInfo.xaml
    /// </summary>
    public partial class BalListInfo : UserControl {
        public BalListInfo() {
            InitializeComponent();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e) { SelectMe(); }

        private void SelectMe() {
            var lbi = UIHelpers.TryFindParent<ListBoxItem>(this);
            if (lbi != null) lbi.IsSelected = true;
        }
    }
}
