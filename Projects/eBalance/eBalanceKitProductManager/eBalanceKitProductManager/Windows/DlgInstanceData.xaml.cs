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
using eBalanceKitProductManager.Business.Structures.DbMapping;

namespace eBalanceKitProductManager.Windows {
    /// <summary>
    /// Interaktionslogik für DlgInstanceData.xaml
    /// </summary>
    public partial class DlgInstanceData : Window {
        
        public DlgInstanceData(Instance instance) {
            InitializeComponent();
            this.Instance = instance;
            this.DataContext = instance;
        }

        public Instance Instance { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
        }
    }
}
