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
    /// Interaktionslogik für CtlAssignmentColumn.xaml
    /// </summary>
    public partial class CtlAssignmentColumn : UserControl {
        public CtlAssignmentColumn() {
            InitializeComponent();
        }


        internal eBalanceKitBusiness.HyperCubes.Import.Importer Model { get { return DataContext as eBalanceKitBusiness.HyperCubes.Import.Importer; } }

        
        private int GetColumnNumber(System.Windows.Controls.DataGrid dataGrid) {
            return dataGrid.CurrentColumn.DisplayIndex;
        }

        

        private int _currentColumn;

        private void dataGrid1_MouseUp(object sender, MouseButtonEventArgs e) {

            DependencyObject dep = (DependencyObject)e.OriginalSource;

            while ((dep != null) && !(dep is DataGridRow) && !(dep is System.Windows.Controls.Primitives.DataGridColumnHeader) && !(dep is DataGridCell)) {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;


            if (dep is System.Windows.Controls.Primitives.DataGridColumnHeader) {
                System.Windows.Controls.Primitives.DataGridColumnHeader header =
                    dep as System.Windows.Controls.Primitives.DataGridColumnHeader;

                _currentColumn = header.DisplayIndex;
                PopUp.IsOpen = true;
            }
            else if (dep is DataGridCell) {
                _currentColumn = GetColumnNumber(dataGrid1);
                PopUp.IsOpen = true;
            }
            else if (dataGrid1.CurrentColumn != null) {
                _currentColumn = GetColumnNumber(dataGrid1);
                PopUp.IsOpen = true;
            }
        }
        
        private void listEntries_MouseUp(object sender, MouseButtonEventArgs e) {
            if (listEntries.SelectedItems.Count != 1) {
                return;
            }
            if ((listEntries.SelectedItem as TemplateBase.HyperCubeHeader).AssignmentFlag == true) {
                return;
            }
            Model.StoreColumnAssignment(_currentColumn, (listEntries.SelectedItem as TemplateBase.HyperCubeHeader));
            PopUp.IsOpen = false;
            //dataGrid1.CurrentColumn.
        }
        
        private void PopUp_Opened(object sender, System.EventArgs e) {
            
                if (Model.Entry.IsInverse) {
                    var entry = from pos in Model.TemplateGenerator.RowHeaders where pos.CsvPosition == _currentColumn select pos;
                    if (entry.Any()) {
                        entry.First().AssignmentFlag = null;
                    }
                    listEntries.DataContext = Model.TemplateGenerator.RowHeaders;
                }
                else {

                    var entry = from pos in Model.TemplateGenerator.ColumnHeaders where pos.CsvPosition == _currentColumn select pos;
                    if (entry.Any()) {
                        entry.First().AssignmentFlag = null;
                    }
                    listEntries.DataContext = Model.TemplateGenerator.ColumnHeaders;
                }
            listEntries.Items.Refresh();

        }

        private void listEntries_SelectionChanged(object sender, SelectionChangedEventArgs e) { e.Handled = true; }

        private void PopUp_KeyUp(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                PopUp.IsOpen = false;
            }
            else if (e.Key == Key.Enter) {
                listEntries_MouseUp(sender, null);
            }
        }
        
        private void dataGrid1_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                //dataGrid1_MouseUp(sender, null);
                _currentColumn = GetColumnNumber(dataGrid1);
                PopUp.IsOpen = true;
                e.Handled = true;
            }
            else if (e.Key == Key.Delete) {
                Model.RemoveColumnAssignment(_currentColumn);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && PopUp.IsOpen) {
                PopUp.IsOpen = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Tab && PopUp.IsOpen) {
                listEntries.Focus();
                e.Handled = true;
            }
        }

        private void dataGrid1_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e) { PopUp.IsOpen = false; }

        private void dataGrid1_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            while ((dep != null) && !(dep is DataGridRow) && !(dep is System.Windows.Controls.Primitives.DataGridColumnHeader) && !(dep is DataGridCell)) {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep == null)
                return;


            if (dep is System.Windows.Controls.Primitives.DataGridColumnHeader) {
                System.Windows.Controls.Primitives.DataGridColumnHeader header =
                    dep as System.Windows.Controls.Primitives.DataGridColumnHeader;

                Model.RemoveColumnAssignment(header.DisplayIndex);
            }

            //if (dep is DataGridCell) {
                //DataGridCell cell = dep as DataGridCell;
                //could do something
            //}
        }

        private void listEntries_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            listEntries.Items.Refresh();
        }


    }
}
