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
    /// Interaktionslogik für DlgEditProfile.xaml
    /// </summary>
    public partial class DlgEditProfile : Window {
        public DlgEditProfile() {
            InitializeComponent();
        }

        EditProfileModel Model {
            get { return DataContext as EditProfileModel; }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) { Model.Save(); }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Model.Cancel();
        }
    }
}
