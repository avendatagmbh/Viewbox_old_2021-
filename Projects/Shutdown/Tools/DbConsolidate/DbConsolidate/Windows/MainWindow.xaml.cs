using System.Windows;
using DbConsolidate.Models;

namespace DbConsolidate.Windows {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowModel();
        }
    }
}
