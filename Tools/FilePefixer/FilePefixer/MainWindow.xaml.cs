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
using System.IO;

namespace FilePefixer {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            foreach (var file in Directory.GetFiles(tbSource.Text)) {
                var fi = new FileInfo(file);
                if((bool)cbDelete.IsChecked) {
                    File.Move(file, fi.Directory + "\\" + fi.Name.Replace(tbPrefix.Text, string.Empty));
                } else {
                    File.Move(file, fi.Directory + "\\" + tbPrefix.Text + fi.Name);
                }
            }

            MessageBox.Show("Finished");
        }
    }
}
