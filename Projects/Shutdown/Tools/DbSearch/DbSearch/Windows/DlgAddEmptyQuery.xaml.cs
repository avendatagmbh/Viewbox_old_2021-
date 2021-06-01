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
using DbSearch.Models;

namespace DbSearch.Windows {
    /// <summary>
    /// Interaktionslogik für DlgAddEmptyQuery.xaml
    /// </summary>
    public partial class DlgAddEmptyQuery : Window {
        public DlgAddEmptyQuery() {
            InitializeComponent();
        }

        AddEmptyQueryModel Model { get { return DataContext as AddEmptyQueryModel; } }
        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Model.Save();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
