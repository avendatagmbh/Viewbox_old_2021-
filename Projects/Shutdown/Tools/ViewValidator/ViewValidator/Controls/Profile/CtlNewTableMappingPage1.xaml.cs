using System;
using System.Windows.Controls;
using DbAccess;
using ViewValidator.Interfaces;
using ViewValidator.Models.Profile;

namespace ViewValidator.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für CtlNewTableMappingPage1.xaml
    /// </summary>
    public partial class CtlNewTableMappingPage1 : UserControl, INewTableMappingPage {
        public CtlNewTableMappingPage1() {
            InitializeComponent();
        }

        TableMappingModel Model { get { return DataContext as TableMappingModel; } }

        private string CheckCustomTable(string table) {
            try {
                using (IDatabase conn = ConnectionManager.CreateConnection(Model.DbConfigView)) {
                    conn.Open();
                    if(conn.TableExists(table))
                        return "";
                    return "Tabelle " + table + " existiert nicht.";
                }
            } catch (Exception ex) {
                return ex.Message;
            }
        }

        public string ValidationError() {
            if (string.IsNullOrEmpty(tbViewTable.Text) && lbDestination.SelectedIndex == -1)
                return "Bitte eine View-Tabelle aus der Liste auswählen oder eine eigene Tabelle in das Textfeld eintragen.";
            if (!string.IsNullOrEmpty(tbViewTable.Text))
                return CheckCustomTable(tbViewTable.Text);
            return "";
        }

        public string SelectedTable() {
            if (!string.IsNullOrEmpty(tbViewTable.Text)) return tbViewTable.Text;
            if (lbDestination.SelectedItem != null) return (string) lbDestination.SelectedItem;
            return "";
        }

        private void SetSelectedItem() {
            Model.SelectedTableView = SelectedTable();
        }

        private void lbDestination_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SetSelectedItem();
        }

        private void tbViewTable_TextChanged(object sender, TextChangedEventArgs e) {
            SetSelectedItem();
        }
    }
}
