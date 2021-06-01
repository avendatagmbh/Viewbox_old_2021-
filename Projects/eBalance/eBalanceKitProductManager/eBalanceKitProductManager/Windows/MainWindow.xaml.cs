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
using System.Windows.Shapes;
using eBalanceKitProductManager.Business;
using eBalanceKitProductManager.Models;
using eBalanceKitProductManager.Business.Structures.DbMapping;

namespace eBalanceKitProductManager.Windows {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private MainWindowModel Model { get; set; }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            try {
                AppConfig.Init();
                this.Model = new MainWindowModel(this);
                this.DataContext = this.Model;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void Window_Closed(object sender, EventArgs e) {
            AppConfig.Dispose();
        }

        private void btnAddInstance_Click(object sender, RoutedEventArgs e) {
            Model.AddInstance();
        }

        private void btnDeleteInstance_Click(object sender, RoutedEventArgs e) {
            List<Instance> SelectedInstances = new List<Instance>();
            foreach (var instance in dgInstances.SelectedItems) {
                SelectedInstances.Add(instance as Instance);
            }
            foreach(Instance instance in SelectedInstances) Model.DeleteInstance(instance);
        }

        private void btnSaveInstances_Click(object sender, RoutedEventArgs e) {
            Model.SaveInstances();
        }
    }
}
