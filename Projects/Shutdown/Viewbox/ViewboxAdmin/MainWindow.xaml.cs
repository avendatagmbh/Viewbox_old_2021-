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
using ViewboxAdmin.Properties;


namespace ViewboxAdmin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e) {
            string tmpDB = "temp";
            string dataProv = "MySQL";
            string connectionString =
                "server=viewboxpres;User Id=root;password=avendata;database=viewbox_atp;Connect Timeout=1000;Default Command Timeout=0;Allow Zero Datetime=True";

            _db = new ViewboxDb.ViewboxDb(tmpDB);
            _db.ViewboxDbInitialized += DbOnViewboxDbInitialized;
            _db.Connect(dataProv, connectionString);
        }

        private void DbOnViewboxDbInitialized(object sender, EventArgs eventArgs) {
            listBox1.Items.Add("DB is initialized...");
        }

        private ViewboxDb.ViewboxDb _db;
    }
}
