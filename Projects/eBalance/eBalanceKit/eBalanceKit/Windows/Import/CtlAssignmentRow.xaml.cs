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
using eBalanceKitBusiness.HyperCubes.Import;
using eBalanceKitBusiness.HyperCubes.Import.Templates;

namespace eBalanceKit.Windows.Import {
    /// <summary>
    /// Interaktionslogik für CtlAssignmentRow.xaml
    /// </summary>
    public partial class CtlAssignmentRow : UserControl {
        public CtlAssignmentRow() {
            InitializeComponent();
        }
        
        internal eBalanceKitBusiness.HyperCubes.Import.Importer Model { get { return DataContext as eBalanceKitBusiness.HyperCubes.Import.Importer; } }
        
        private int GetRowNumber(System.Windows.Controls.DataGrid dataGrid) {
            return Model.CsvData.Rows.IndexOf(dataGrid.CurrentCell.Item as CsvRow);
        }


        private int _currentRow;

        private void dataGrid1_MouseUp(object sender, MouseButtonEventArgs e) {
            _currentRow = GetRowNumber(dataGrid1);
            PopUp.IsOpen = true;
        }

        private void dataGrid1_KeyUp(object sender, System.Windows.Input.KeyEventArgs e) {
            if (e.Key == Key.Delete) {

                    Model.RemoveRowAssignment(_currentRow);
            }
            else if (e.Key == Key.Enter) {
                dataGrid1_MouseUp(sender, null);
            }
            e.Handled = true;
        }

        private void listEntries_MouseUp(object sender, MouseButtonEventArgs e) {
            if (listEntries.SelectedItems.Count == 1) {
                Model.StoreRowAssignment(_currentRow, (listEntries.SelectedItem as TemplateBase.HyperCubeHeader));
                PopUp.IsOpen = false;
            }   
        }

        private void PopUp_Opened(object sender, System.EventArgs e) {

            if (Model.Entry.IsInverse) {
                var entry = from pos in Model.TemplateGenerator.ColumnHeaders where pos.CsvPosition == _currentRow select pos;
                if (entry.Any()) {
                    entry.First().AssignmentFlag = null;
                }
                listEntries.DataContext = Model.TemplateGenerator.ColumnHeaders;
            }
            else {
                var entry = from pos in Model.TemplateGenerator.RowHeaders where pos.CsvPosition == _currentRow select pos;
                if (entry.Any()) {
                    entry.First().AssignmentFlag = null;
                }
                listEntries.DataContext = Model.TemplateGenerator.RowHeaders;
            }
        }

        private void listEntries_SelectionChanged(object sender, SelectionChangedEventArgs e) { e.Handled = true; }
        
        private void PopUp_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                PopUp.IsOpen = false;
            }
            else if (e.Key == Key.Enter) {
                listEntries_MouseUp(sender, null);
            }
            e.Handled = true;
        }

        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e) { PopUp.IsOpen = false; }

        private void dataGrid1_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) { PopUp.IsOpen = false; }

        private void dataGrid1_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Delete) {
                Model.RemoveRowAssignment(_currentRow);
            }
            else if (e.Key == Key.Enter) {
                dataGrid1_MouseUp(sender, null);
            }
            else if (e.Key == Key.Escape) {
                PopUp.IsOpen = false;
            }
            e.Handled = true;
        }

        private void dataGrid1_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                dataGrid1_MouseUp(sender, null);
                e.Handled = true;
            }
            else if (e.Key == Key.Delete) {
                Model.RemoveRowAssignment(_currentRow);
                e.Handled = true;
            }
            else if (e.Key == Key.Tab && PopUp.IsOpen) {
                listEntries.Focus();
                e.Handled = true;
            }
        }

        private void PopUp_Closed(object sender, System.EventArgs e) {
            //dataGrid1.Focus();
            /*
            if (dataGrid1.SelectedIndex > 0) {
                (dataGrid1.SelectedItem as DataGridCell).Focus();
            }
            */
        }
    }
}
