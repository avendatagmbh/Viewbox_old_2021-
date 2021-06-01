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
using DbAccess;
using ViewBuilder.Models;

namespace ViewBuilder.Windows {
    /// <summary>
    /// Interaktionslogik für DlgCreateViewboxDb.xaml
    /// </summary>
    public partial class DlgCreateViewboxDb : Window {
        public DlgCreateViewboxDb() {
            InitializeComponent();
        }

        CreateViewboxDbModel Model { get { return DataContext as CreateViewboxDbModel; } }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Model.CreateViewboxDb(this);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }
    }
}
