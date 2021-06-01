using System.Windows;
using WpfControlsSample.Models;

namespace WpfControlsSample.Windows {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowModel(this);
        }
    }
}
