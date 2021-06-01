// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Controls;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlTableList.xaml
    /// </summary>
    public partial class CtlTableList : UserControl {
        public CtlTableList() {
            InitializeComponent();
        }

        private void btnSortTables(object sender, System.Windows.RoutedEventArgs e) { SortTablesPopup.IsOpen = true; }

        private void btnSortEmptyTables(object sender, RoutedEventArgs e) { SortEmptyTablesPopup.IsOpen = true; }

        private void btnFilterTables(object sender, RoutedEventArgs e) { FilterTablesPopup.IsOpen = true; }

        private void btnFilterEmptyTables(object sender, RoutedEventArgs e) { FilterEmptyTablesPopup.IsOpen = true; }
    }
}