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
using DbSearch.Models.Profile;
using DbSearchDatabase.TableRelated;
using Microsoft.Win32;
using Utils;

namespace DbSearch.Windows.Profile {
    /// <summary>
    /// Interaktionslogik für DlgImportQueries.xaml
    /// </summary>
    public partial class DlgImportQueries : Window {
        public DlgImportQueries() {
            InitializeComponent();
        }

        ImportQueriesModel Model { get { return DataContext as ImportQueriesModel; } }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            ((ImportTable) ((CheckBox) e.OriginalSource).DataContext).Use = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            ((ImportTable)((CheckBox)e.OriginalSource).DataContext).Use = false;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog() {
                DefaultExt = ".mdb",
                Filter = "Access Datenbank (.mdb)|*.mdb"
            };
            dlg.Multiselect = true;
            bool? result = dlg.ShowDialog();
            if (result.Value) {
                foreach (var path in dlg.FileNames) {
                    try {
                        Model.AddValidationPath(path);
                    }
                    catch (Exception ex) {
                        Window parent = UIHelpers.TryFindParent<Window>(this);
                        if (parent == null) {
                            MessageBox.Show("Fehler beim öffnen der Datenbank" + Environment.NewLine + ex.Message, "",
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        } else {
                            MessageBox.Show(parent,
                                            "Fehler beim öffnen der Datenbank" + Environment.NewLine + ex.Message, "",
                                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            if (lbValidationPaths.SelectedItems.Count == 0) return;
            Model.DeleteValidationPaths(lbValidationPaths.SelectedItems);
        }
    }
}
