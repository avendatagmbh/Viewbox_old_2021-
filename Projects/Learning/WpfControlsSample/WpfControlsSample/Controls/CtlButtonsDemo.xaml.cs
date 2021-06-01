using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utils;
using WpfControlsSample.Models;
using WpfControlsSample.Windows;

namespace WpfControlsSample.Controls {
    /// <summary>
    /// Interaktionslogik für CtlButtonsDemo.xaml
    /// </summary>
    public partial class CtlButtonsDemo : UserControl {
        public CtlButtonsDemo() {
            InitializeComponent();
        }

        ButtonsDemoModel Model { get { return DataContext as ButtonsDemoModel; } }

        private void btnCreateProgress_Click(object sender, RoutedEventArgs e) {
            Model.CreateProgress();
        }

    }
}
