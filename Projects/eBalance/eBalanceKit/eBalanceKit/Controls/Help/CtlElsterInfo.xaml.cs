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

namespace eBalanceKit.Controls.Help {
    /// <summary>
    /// Interaktionslogik für CtlElsterInfo.xaml
    /// </summary>
    public partial class CtlElsterInfo : UserControl {
        public CtlElsterInfo() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start("https://www.elsteronline.de/eportal/eop/auth/Registrierung.tax"); }
    }
}
