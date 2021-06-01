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
using ViewboxDatabaseCreator.Models;

namespace ViewboxDatabaseCreator {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            DataContext = new MainWindowModel();
        }

        private MainWindowModel Model {
            get { return DataContext as MainWindowModel; }
        }

        private void btnCreateViewboxDb_Click(object sender, RoutedEventArgs e) {
            try {
                if (Model.ViewboxDbExists()) {
                    if (MessageBox.Show(this, "Viewbox Datenbank existiert bereits, überschreiben?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        return;
                }
                Model.CreateViewboxDb();
            }catch(Exception ex) {
                MessageBox.Show(this, "Es ist ein Fehler aufgetreten: " + ex.Message);
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e) {
            if (Model.ViewboxDbExists()) {
                if (MessageBox.Show(this, "Viewbox Datenbank existiert bereits, überschreiben?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }
            try {
                Model.Test();
            } catch (Exception ex) {
                MessageBox.Show(this, "Es ist ein Fehler aufgetreten: " + ex.Message);
            }
        }
    }
}
