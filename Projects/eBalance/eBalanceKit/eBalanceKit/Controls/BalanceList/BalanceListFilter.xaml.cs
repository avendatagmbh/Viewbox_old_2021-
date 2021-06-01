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
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaktionslogik für BalanceListFilter.xaml
    /// </summary>
    public partial class BalanceListFilter : UserControl {
        public BalanceListFilter() {
            InitializeComponent();
        }

        BalanceListFilterOptions FilterOptions { get { return this.DataContext as BalanceListFilterOptions; } }

        private void optSearchInfoIsCheckedChanged(object sender, RoutedEventArgs e) {
            var f = FilterOptions;
            
            if (f == null) return;
            
            if (optSearchAll.IsChecked.HasValue && optSearchAll.IsChecked.Value == true) {
                f.SearchAccountNames = true;
                f.SearchAccountNumbers = true;

            } if (optSearchAccountNumbers.IsChecked.HasValue && optSearchAccountNumbers.IsChecked.Value == true) {
                f.SearchAccountNames = false;
                f.SearchAccountNumbers = true;

            } if (optSearchAccountNames.IsChecked.HasValue && optSearchAccountNames.IsChecked.Value == true) {
                f.SearchAccountNames = true;
                f.SearchAccountNumbers = false;
            }
        }
    }
}