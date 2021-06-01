using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ViewValidator.Converters;
using ViewValidator.Factories;
using ViewValidator.Models.Rules;
using ViewValidator.Windows;
using ViewValidatorLogic.Structures.InitialSetup;
using ViewValidatorLogic.Structures.Logic;
using ViewValidatorLogic.Structures.Results;

namespace ViewValidator.Controls.Result {
    /// <summary>
    /// Interaktionslogik für ResultRowsDifferent.xaml
    /// </summary>
    public partial class ResultRowsDifferent : UserControl {
        public ResultRowsDifferent() {
            InitializeComponent();
        }

        TableValidationResult Model { get { return this.DataContext as TableValidationResult; } }

        private void UpdateDataView(DataGrid grid, TableMapping tableMapping) {
            grid.Columns.Clear();
            if (Model == null) return;

            int columnIndex = 0;
            foreach (var columnMapping in tableMapping.ColumnMappings) {
                
                for (int index = 0; index < 2; index++) {
                    DataGridTextColumn column = new DataGridTextColumn {
                            Binding = new Binding( index == 0 ?
                                string.Format("RowValidation[{0}].RuleDisplayString", columnIndex) :
                                string.Format("RowView[{0}].RuleDisplayString", columnIndex))
                        };
                    HeaderFactory.SetHeader(column, columnMapping, index);
                    HeaderFactory.RegisterForColumnVisibilityHandling(grid, index, columnMapping);
                    column.Visibility = columnMapping.IsVisible ? Visibility.Visible : Visibility.Collapsed;
                    
                    Style cellStyle = new Style(typeof(DataGridCell));
                    //cellStyle.Setters.Add(new Setter(BackgroundProperty, new Binding("") { Converter = new ErrorToColorConverter(), ConverterParameter = columnIndex }));
                    cellStyle.Setters.Add(new Setter(BackgroundProperty, new Binding("") { Converter = new RuleEntryTypeToBrushConverter(), ConverterParameter = columnIndex }));
                    cellStyle.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));
                    //cellStyle.Setters.Add(new Setter(ToolTipProperty, new Binding("") { 
                    //    Converter = new StringToByteConverter(), ConverterParameter = new KeyValuePair<int,int>(index, i) 
                    //}));
                    cellStyle.Setters.Add(new Setter(ToolTipProperty, new Binding(index == 0 ?
                        string.Format("RowValidation[{0}].DisplayString", columnIndex) :
                        string.Format("RowView[{0}].DisplayString", columnIndex))
                    ));
                    //cellStyle.Triggers.Add(new Trigger());
                    column.CellStyle = cellStyle;
                    
                    
                    grid.Columns.Add(column);
                        
                }
                ++columnIndex;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model == null) return;
            UpdateDataView(dgvDifferentRows, Model.TableMapping);
        }

        private void dgvDifferentRows_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            DependencyObject source = (DependencyObject)e.OriginalSource;
            var cell = UIHelpers.TryFindParent<DataGridCell>(source);
            var row = UIHelpers.TryFindParent<DataGridRow>(source);
            if (cell == null || row == null) return;
            RowEntry rowEntry;
            if(cell.Column.DisplayIndex % 2 == 0)
                rowEntry = (RowEntry)((RowDifference)row.Item).RowValidation[cell.Column.DisplayIndex/2];
            else {
                rowEntry = (RowEntry)((RowDifference)row.Item).RowView[cell.Column.DisplayIndex/2];
            }
            DlgRowEntryDetails dlg = new DlgRowEntryDetails() { DataContext = new RowEntryDetailsModel(rowEntry) };
            dlg.Show();

        }
    }
}
