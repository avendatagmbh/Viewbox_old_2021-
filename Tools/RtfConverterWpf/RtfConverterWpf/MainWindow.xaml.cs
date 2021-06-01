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

namespace RtfConverterWpf {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowModel(this);
        }

        MainWindowModel Model { get { return DataContext as MainWindowModel; } }

        private void AssistantControl_BeforeNext(object sender, RoutedEventArgs e) {
            if (!Model.ValidateConnection()) {
                e.Handled = true;
            } else {
                Model.LoadTables();
            }
        }

        private void AssistantControl_OnFinish(object sender, RoutedEventArgs e) {
            Model.Start();
        }

    }
}
