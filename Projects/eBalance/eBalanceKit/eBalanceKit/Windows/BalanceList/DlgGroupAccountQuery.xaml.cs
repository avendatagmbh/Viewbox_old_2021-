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
using eBalanceKit.Models;
using eBalanceKitBusiness.Options;

namespace eBalanceKit.Windows.BalanceList
{
    /// <summary>
    /// Interaction logic for DlgGroupAccountQuery.xaml
    /// </summary>
    public partial class DlgGroupAccountQuery : Window
    {
        public DlgGroupAccountQuery()
        {
            InitializeComponent();
            DontAskAgain = false;
            DataContext = this;
        }

        public bool DontAskAgain { get; set; }
        private void SaveDontAskAgain()
        {
            if (!DontAskAgain) return;
            GlobalUserOptions.UserOptions.DontAskForGroupAccounts = DialogResult;
            GlobalUserOptions.UserOptions.SaveConfiguration();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            SaveDontAskAgain();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            SaveDontAskAgain();
        }
    }
}
