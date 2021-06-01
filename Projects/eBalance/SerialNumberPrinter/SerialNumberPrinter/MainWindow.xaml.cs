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
using System.Drawing.Printing;
using System.Drawing;
using System.Data;

namespace SerialNumberPrinter {

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
     
        public MainWindow() {
            InitializeComponent();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e) {
            for (int i = 0; i < txtSerialNumbers.LineCount; i++) {
                string line = txtSerialNumbers.GetLineText(i).Trim().ToUpper();
                Print.PrintSerialNumber(line);
            }
            MessageBox.Show("Vorgang abgeschlossen.");
        }
    }

}
