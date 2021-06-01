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
using eBalanceKitConfig.Models;

namespace eBalanceKitConfig.Controls {
    /// <summary>
    /// Interaktionslogik für CtlDbConfig_MySQL.xaml
    /// </summary>
    public partial class CtlDbConfig_MySQL : UserControl {
        public CtlDbConfig_MySQL() {
            InitializeComponent();
        }

        ConfigModel Model {
            get { return this.DataContext as ConfigModel; }
        }

        public void InitPassword() {
            txtPassword.Password = this.Model.Password;
        }

        public void UpdatePassword() {
            this.Model.Password = txtPassword.Password;
        }
    }
}
