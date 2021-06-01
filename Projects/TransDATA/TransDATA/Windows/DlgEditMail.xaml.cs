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
using TransDATA.Models;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für DlgEditMail.xaml
    /// </summary>
    public partial class DlgEditMail : Window {
        public DlgEditMail() {
            InitializeComponent();
        }

        EditMailModel Model { get { return DataContext as EditMailModel; } }

        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            Model.Cancel(this);
        }

        private void BtnSaveClick(object sender, RoutedEventArgs e) {
            Model.Save(this);
        }
    }
}
