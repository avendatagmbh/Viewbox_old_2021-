using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using ViewValidator.Converters;
using ViewValidator.Factories;
using ViewValidator.Models.Result;
using ViewValidatorLogic.Structures.Results;

namespace ViewValidator.Controls.Result {
    /// <summary>
    /// Interaktionslogik für ResultRowDifferentColumnWise.xaml
    /// </summary>
    public partial class ResultRowsDifferentColumnWise : UserControl {
        public ResultRowsDifferentColumnWise() {
            InitializeComponent();
        }
        RowsDifferentColumnWiseModel Model { get { return this.DataContext as RowsDifferentColumnWiseModel; } }

        private void UpdateResultDataView(DataGrid grid, TableValidationResult validationResult, int columnIndex) {
            grid.Columns.Clear();
            if (Model == null) return;
            
            for (int index = 0; index < 2; index++) {
                DataGridTextColumn column = new DataGridTextColumn {
                    //Header = validationResult.TableMapping.ColumnMappings[columnIndex].Get(index),
                    Header = HeaderFactory.GetGridViewHeader(index, validationResult.TableMapping.ColumnMappings[columnIndex].GetColumn(index)),

                    Binding = new Binding(index == 0 ?
                        string.Format("RowValidation[{0}].RuleDisplayString", columnIndex) :
                        string.Format("RowView[{0}].RuleDisplayString", columnIndex))
                };

                Style cellStyle = new Style(typeof(DataGridCell));
                //cellStyle.Setters.Add(new Setter(ToolTipProperty, new Binding("") {
                //    Converter = new StringToByteConverter(), ConverterParameter = new KeyValuePair<int, int>(index, columnIndex)
                //}));
                cellStyle.Setters.Add(new Setter(ToolTipProperty, new Binding(index == 0 ?
                        string.Format("RowValidation[{0}].DisplayString", columnIndex) :
                        string.Format("RowView[{0}].DisplayString", columnIndex))
                    ));
                    
                


                cellStyle.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));
                column.CellStyle = cellStyle;

                grid.Columns.Add(column);
            }
            //int i = 0;
            //foreach (var columnMapping in tableMapping.ColumnMappings) {

            //    for (int index = 0; index < 2; index++) {
            //        DataGridTextColumn column = new DataGridTextColumn {
            //            Header = columnMapping.GetColumn(index),
            //            Binding = new Binding(index == 0 ?
            //                string.Format("RowValidation[{0}].Value", i) :
            //                string.Format("RowView[{0}].Value", i))
            //        };
            //        //Style cellStyle = new Style(typeof(DataGridCell));
            //        //cellStyle.Setters.Add(new Setter(DataGridCell.BackgroundProperty, new Binding("") { Converter = new ErrorToColorConverter(), ConverterParameter = i }));
            //        //column.CellStyle = cellStyle;
            //        grid.Columns.Add(column);

            //    }
            //    ++i;
            //}
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model == null) return;
            //UpdateDataView(dgvColumnMapping, Model.TableMapping);
        }

        private void dgvColumnMapping_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count == 1 && dgvColumnMapping.SelectedIndex != -1) {
                int columnIndex = dgvColumnMapping.SelectedIndex;
                Model.SelectedColumn(columnIndex);
                UpdateResultDataView(dgvColumnResult, Model.TableValidationResult, columnIndex);
                //ColumnMappingHelper columnMapping = e.AddedItems[0] as ColumnMappingHelper;
            }
        }
    }
}
