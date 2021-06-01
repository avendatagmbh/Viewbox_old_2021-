// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using System.Windows.Input;
using TransDATA.Models;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für DlgProfileManagement.xaml
    /// </summary>
    public partial class DlgProfileManagement : Window {
        public DlgProfileManagement() {
            InitializeComponent();
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                Close();
            }
        }
    }
}