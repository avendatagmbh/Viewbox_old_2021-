using System;
using System.Collections.Generic;
using System.IO;
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

namespace OldTransDATARegister {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) { 
            Dictionary<string, string> regdata = new Dictionary<string, string>();
            var registryDataReader = new StreamReader("Registrierung\\registry.csv");
            while(!registryDataReader.EndOfStream) {
                var line = registryDataReader.ReadLine();
                var parts = line.Split(';');
                regdata.Add(parts[1].Replace("\"", ""), parts[2].Replace("\"", ""));
            }
            registryDataReader.Close();

            if (!regdata.ContainsKey(txtTDKey.Text)) {
                MessageBox.Show("Der eingegebene TransDATA Code existiert nicht!");
                return;
            }

            StreamWriter writer = new StreamWriter("Registrierung\\software.csv", true);
            writer.WriteLine(txtCompany.Text + ";" + txtName.Text + ";" + txtForename.Text + ";" + regdata[txtTDKey.Text] + ";" + System.DateTime.Now.ToShortDateString());
            writer.Close();

            txtKey.Text = regdata[txtTDKey.Text];
        }
    }
}
