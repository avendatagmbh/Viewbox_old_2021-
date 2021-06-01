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

namespace eBalanceKit.Windows.Management.Delete {
    /// <summary>
    /// Interaktionslogik für DlgDeleteCompany.xaml
    /// </summary>
    public partial class DlgDeleteCompany : Window {
        public DlgDeleteCompany() {
            InitializeComponent();
        }

        DeleteCompanyModel Model { get { return DataContext as DeleteCompanyModel; } }

        private void btnOkClick(object sender, RoutedEventArgs e) {
            try {
                if (Model.DeleteItem()) {
                    DialogResult = true;
                }
            } catch (Exception ex) {
                MessageBox.Show(this, "Es ist ein Fehler beim Löschen des Unternehmens aufgetreten: " + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancelClick(object sender, RoutedEventArgs e) { DialogResult = false; }
    }
}
