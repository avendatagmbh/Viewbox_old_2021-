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
using ViewboxAdmin.ViewModels.Users;

namespace ViewboxAdmin.Windows
{
    /// <summary>
    /// Interaction logic for UserEditorWindow.xaml
    /// </summary>
    public partial class UserEditorWindow : Window
    {
        public UserEditorWindow() {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = true; 
            ((UserModel) this.DataContext).Password = passwordBox1.Password;
            this.Close();
        }

        private void button2_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }
    }
}
