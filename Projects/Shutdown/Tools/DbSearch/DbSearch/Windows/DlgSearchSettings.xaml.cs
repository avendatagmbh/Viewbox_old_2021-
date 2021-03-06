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
using DbSearch.Models.Search;

namespace DbSearch.Windows {
    /// <summary>
    /// Interaktionslogik für DlgSearchSettings.xaml
    /// </summary>
    public partial class DlgSearchSettings : Window {
        public DlgSearchSettings() {
            InitializeComponent();
        }

        SearchSettingsModel Model { get { return DataContext as SearchSettingsModel; } }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Model.AcceptButton();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
