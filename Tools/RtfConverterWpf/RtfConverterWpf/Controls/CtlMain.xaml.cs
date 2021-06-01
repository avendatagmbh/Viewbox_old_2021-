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
using RtfConverterWpf.Models;

namespace RtfConverterWpf.Controls {
    /// <summary>
    /// Interaktionslogik für CtlMain.xaml
    /// </summary>
    public partial class CtlMain : UserControl {
        public CtlMain() {
            InitializeComponent();
            chkIsViewCheckedHeader.DataContext = DataContext;
        }

        MainWindowModel Model { get { return DataContext as MainWindowModel; } }

        private void chkIsViewCheckedHeader_Checked(object sender, RoutedEventArgs e) {
            Model.IsCheckedHeaderState = true;
        }

        private void chkIsViewCheckedHeader_Unchecked(object sender, RoutedEventArgs e) {
            Model.IsCheckedHeaderState = false;
        }

    }
}
