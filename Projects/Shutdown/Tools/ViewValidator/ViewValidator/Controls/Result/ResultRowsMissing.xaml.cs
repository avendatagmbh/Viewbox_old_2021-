using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ViewValidator.Controls.Datagrid;
using ViewValidator.Factories;
using ViewValidator.Manager;
using ViewValidator.Models.Datagrid;
using ViewValidator.Models.Rules;
using ViewValidator.Windows;
using ViewValidatorLogic.Interfaces;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Structures.Logic;
using ViewValidatorLogic.Structures.Results;
using Brushes = System.Windows.Media.Brushes;

namespace ViewValidator.Controls.Result {
    /// <summary>
    /// Interaktionslogik für ResultRowsMissing.xaml
    /// </summary>
    public partial class ResultRowsMissing : UserControl {
        public ResultRowsMissing() {
            InitializeComponent();
        }

        TableValidationResult Model { get { return this.DataContext as TableValidationResult; } }

        private void UpdateDataView(DataGrid grid, TableMapping tableMapping, int index) {
            grid.Columns.Clear();
            if (Model == null) return;

            int columnIndex = 0;
            foreach (var columnMapping in tableMapping.ColumnMappings) {
                
                DataGridTextColumn column = new DataGridTextColumn {
                        Binding = new Binding(string.Format("[{0}].RuleDisplayString", columnIndex))
                    };
                HeaderFactory.SetHeader(column, columnMapping, index);
                HeaderFactory.RegisterForColumnVisibilityHandling(grid, index, columnMapping);
                column.Visibility = columnMapping.IsVisible ? Visibility.Visible : Visibility.Collapsed;

                //Change color of Key Entry column
                Style cellStyle = new Style(typeof(DataGridCell));
                if (tableMapping.Tables[index].KeyEntries.Contains(columnIndex)) {
                    cellStyle.Setters.Add(new Setter(BackgroundProperty, ColorManager.BrushFromRowEntryType(RowEntryType.KeyEntry)));
                    cellStyle.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));
                }
                cellStyle.Setters.Add(new Setter(ToolTipProperty, new Binding(index == 0 ?
                        string.Format("[{0}].DisplayString", columnIndex) :
                        string.Format("[{0}].DisplayString", columnIndex))
                    ));
                column.CellStyle = cellStyle;

                grid.Columns.Add(column);
                ++columnIndex;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model == null) return;
            UpdateDataView(dgvMissingRowsValidation, Model.TableMapping, 0);
            UpdateDataView(dgvMissingRowsView, Model.TableMapping, 1);

            Font usedFont = new Font(System.Drawing.FontFamily.GenericSerif,
                                     (float)dgvMissingRowsValidation.FontSize);
            for (int i = 0; i < dgvMissingRowsValidation.Columns.Count; ++i) {
                int maxSize = -1;

                foreach (var row in Model.ResultValidationTable.MissingRows) {
                    maxSize = Math.Max(System.Windows.Forms.TextRenderer.MeasureText(row[i].RuleDisplayString,
                                                 usedFont).Width, maxSize);
                }
                foreach (var row in Model.ResultViewTable.MissingRows) {
                    maxSize = Math.Max(System.Windows.Forms.TextRenderer.MeasureText(row[i].RuleDisplayString,
                                                 usedFont).Width, maxSize);
                }
                maxSize = Math.Max(System.Windows.Forms.TextRenderer.MeasureText(HeaderFactory.HeaderToHeaderModel(dgvMissingRowsValidation.Columns[i].Header).Column.Name,
                                             usedFont).Width+20, maxSize);
                maxSize = Math.Max(System.Windows.Forms.TextRenderer.MeasureText(HeaderFactory.HeaderToHeaderModel(dgvMissingRowsView.Columns[i].Header).Column.Name,
                                             usedFont).Width+20, maxSize);

                dgvMissingRowsValidation.Columns[i].Width = new DataGridLength(maxSize);
                dgvMissingRowsView.Columns[i].Width = new DataGridLength(maxSize);

            }
        }

        private void ClickedDataGrid(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            DependencyObject source = (DependencyObject)e.OriginalSource;
            var cell = UIHelpers.TryFindParent<DataGridCell>(source);
            var row = UIHelpers.TryFindParent<DataGridRow>(source);
            if (cell == null || row == null) return;
            var rowEntry = ((Row) row.Item)[cell.Column.DisplayIndex];
            
            DlgRowEntryDetails dlg = new DlgRowEntryDetails() { DataContext = new RowEntryDetailsModel(rowEntry) };
            dlg.Show();
        }

        private void dgvMissingRowsValidation_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            ClickedDataGrid(sender, e);
        }

        private void dgvMissingRowsView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            ClickedDataGrid(sender, e);
        }
    }
}
