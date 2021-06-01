using System;
using System.Collections.Generic;
using System.DirectoryServices;
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
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Controls {
    /// <summary>
    /// Interaktionslogik für CtlSortPopup.xaml
    /// </summary>
    public partial class CtlSortPopup : UserControl {
        public CtlSortPopup() {
            InitializeComponent();
        }

        #region IsOpen
        public bool IsOpen {
            get { return SortConfigPopup.IsOpen; }
            set {
                SortConfigPopup.IsOpen = value;
            }
        }
        #endregion IsOpen

        private BalanceListSortOptions Options {
            get { return DataContext as BalanceListSortOptions; }
        }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            SortConfigPopup.IsOpen = false;
        }

        private void btnReset_Click(object sender, RoutedEventArgs e) {
            if (Options != null)
                Options.Reset();
            SortConfigPopup.IsOpen = false;
        }
    }
}
