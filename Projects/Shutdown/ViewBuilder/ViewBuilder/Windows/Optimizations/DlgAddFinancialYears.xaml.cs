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
using ViewBuilder.Models;

namespace ViewBuilder.Windows.Optimizations {
    /// <summary>
    /// Interaktionslogik für DlgAddFinancialYears.xaml
    /// </summary>
    public partial class DlgAddFinancialYears : Window {
        public DlgAddFinancialYears() {
            InitializeComponent();
        }

        AddFinancialYearsModel Model { get { return DataContext as AddFinancialYearsModel; } }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Model.Save();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
