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
using eBalanceKitBusiness.Reconciliation.ViewModels;

namespace eBalanceKit.Windows.Reconciliation
{
    /// <summary>
    /// Interaction logic for DlgReferenceList.xaml
    /// </summary>
    public partial class DlgReferenceList : Window {
        public DlgReferenceList() {
            InitializeComponent();
        }
        
        private void CtlReferenceList_Ok(object sender, RoutedEventArgs e) { this.Close(); }
        private void CtlReferenceList_Cancel(object sender, RoutedEventArgs e) { this.Close(); }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) { e.Handled = true; Close(); }
        }
    }
}
