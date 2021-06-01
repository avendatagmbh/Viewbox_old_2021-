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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TransDATA.Models;

namespace TransDATA.Controls.Config {
    /// <summary>
    /// Interaktionslogik für CtlMailConfig.xaml
    /// </summary>
    public partial class CtlMailConfig : UserControl {
        public CtlMailConfig() {
            InitializeComponent();
        }

        private void AddMailRecipientButton_Click(object sender, RoutedEventArgs e) { (DataContext as EditMailModel).AddRecipient(tbNewRecipient.Text); }

        private void RemoveMailRecipientButton_Click(object sender, RoutedEventArgs e) {
            (DataContext as EditMailModel).RemoveRecipient(lbmailRecipients.SelectedItem.ToString());
        }
    }
}
