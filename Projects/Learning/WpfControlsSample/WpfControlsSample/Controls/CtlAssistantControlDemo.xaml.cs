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

namespace WpfControlsSample.Controls {
    /// <summary>
    /// Interaktionslogik für CtlAssistantControlDemo.xaml
    /// </summary>
    public partial class CtlAssistantControlDemo : UserControl {
        public CtlAssistantControlDemo() {
            InitializeComponent();
        }

        private void OkClick(object sender, RoutedEventArgs e) {
            MessageBox.Show("Okidoki");
        }

        private void OnCancel(object sender, RoutedEventArgs e) {
            MessageBox.Show("Cancel");
        }
    }
}
