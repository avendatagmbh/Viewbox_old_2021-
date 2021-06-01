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
using Microsoft.Win32;
using ViewValidator.Interfaces;
using ViewValidator.Models.Profile;

namespace ViewValidator.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für CtlNewTableMappingPage2.xaml
    /// </summary>
    public partial class CtlNewTableMappingPage2 : UserControl, INewTableMappingPage {
        public CtlNewTableMappingPage2() {
            InitializeComponent();
        }

        TableMappingModel Model { get { return DataContext as TableMappingModel; } }

        public string ValidationError() {
            if (lbSource.SelectedIndex == -1)
                return "Bitte eine Verprobungs-Tabelle aus der Liste auswählen.";
            return "";
        }

        public string SelectedTable() {
            if (lbSource.SelectedItem != null) return (string)lbSource.SelectedItem;
            return "";
        }

        private void btnSelectValidationDatabase_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog() {
                DefaultExt = ".mdb",
                Filter = "Access Datenbank (.mdb, .accdb)|*.mdb;*.accdb"
            };
            bool? result = dlg.ShowDialog();
            if (result.Value) {
                Model.SelectedValidationConfig.Hostname = dlg.FileName;
                try {
                    Model.ReloadValidationTables();
                } catch (Exception ex) {
                    MessageBox.Show(UIHelpers.TryFindParent<Window>(this), "Ein Fehler ist aufgetreten beim Öffnen der Datenbank Verbindung:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }
    }
}
