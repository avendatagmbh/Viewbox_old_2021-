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
using eBalanceKitManagement.Models;
using System.Globalization;
using eBalanceKitBusiness.Structures;
using System.Threading;
using eBalanceKitBusiness.Manager;
using eBalanceKitBase.Windows;

namespace eBalanceKitManagement.Windows {
    
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        
        public MainWindow() {
            InitializeComponent();

            this.Model = new MainWindowModel(this);
            this.DataContext = this.Model;

            try {
                Model.Init();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message); 
                Close(); 
                return;
            }            
        }

        private MainWindowModel Model { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) { e.Handled = true; this.Close(); } }
        private void Window_Closed(object sender, EventArgs e) { AppConfig.Dispose(); }


    
    }
}
