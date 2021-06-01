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
using eBalanceKit.Windows.Management.Delete.Models;

namespace eBalanceKit.Windows.Management.Edit {
    /// <summary>
    /// Interaktionslogik für DlgEditCompanySelection.xaml
    /// </summary>
    public partial class DlgEditCompanySelection : Window {
        public DlgEditCompanySelection() {
            InitializeComponent();
        }

        private void btnOkClick(object sender, RoutedEventArgs e) {
            Close();
            DlgEditCompany dlg = new DlgEditCompany(Model.SelectedItem) { Owner = this.Owner };
            dlg.ShowDialog();
        }

        DeleteCompanyModel Model { get { return DataContext as DeleteCompanyModel; } }

        private void btnCancelClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
