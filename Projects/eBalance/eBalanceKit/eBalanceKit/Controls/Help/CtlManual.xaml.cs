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
    /// Interaktionslogik für CtlManual.xaml
    /// </summary>
    public partial class CtlManual : UserControl {
        public CtlManual() {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e) { System.Diagnostics.Process.Start(@"Documents\Handbuch.pdf"); }
    }
}
