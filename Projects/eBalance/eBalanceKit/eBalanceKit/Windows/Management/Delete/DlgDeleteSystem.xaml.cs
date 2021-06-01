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
    /// Interaktionslogik für DlgDeleteSystem.xaml
    /// </summary>
    public partial class DlgDeleteSystem : Window {
        public DlgDeleteSystem() {
            InitializeComponent();
        }

        DeleteSystemModel Model { get { return DataContext as DeleteSystemModel; } }

        private void btnCancelClick(object sender, RoutedEventArgs e) {
            Close();
        }

        private void btnOkClick(object sender, RoutedEventArgs e) {
            try {
                if (Model.DeleteItem()) {
                    MessageBox.Show(this, "Das System wurde erfolgreich gelöscht.", "", MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                    Close();
                }
            } catch (Exception ex) {
                MessageBox.Show(this, "Es ist ein Fehler beim Löschen des Systems aufgetreten: " + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
