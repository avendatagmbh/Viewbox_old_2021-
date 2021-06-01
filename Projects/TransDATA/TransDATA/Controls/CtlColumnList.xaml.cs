// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows.Controls;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlColumnList.xaml
    /// </summary>
    public partial class CtlColumnList : UserControl {
        public CtlColumnList() {
            InitializeComponent();
        }

        private void btnSortColumns(object sender, System.Windows.RoutedEventArgs e) { SortColumnsPopup.IsOpen = true; }
    }
}