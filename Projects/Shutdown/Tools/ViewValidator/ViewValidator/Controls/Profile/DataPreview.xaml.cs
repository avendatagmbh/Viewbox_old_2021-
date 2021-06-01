using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using ViewValidator.Converters;
using ViewValidator.Models.Profile;
using Binding = System.Windows.Data.Binding;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;
using UserControl = System.Windows.Controls.UserControl;

namespace ViewValidator.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für DataPreview.xaml
    /// </summary>
    public partial class DataPreview : UserControl {
        public DataPreview() {
            InitializeComponent();
        }

        DataPreviewModel Model { get { return this.DataContext as DataPreviewModel; } }

        private void UpdateDataView(DataGrid grid, int index) {
            grid.Columns.Clear();
            if (Model == null) return;


            int i = 0;
            foreach (var columnMapping in Model.TableMapping.ColumnMappings) {
                
                DataGridTextColumn column = new DataGridTextColumn {
                        Header = columnMapping.GetColumnName(index),
                        Binding = new Binding( string.Format("[{0}].DisplayString", i) )
                    };
                Style cellStyle = new Style(typeof(DataGridCell));
                cellStyle.Setters.Add(new Setter(FontStyleProperty, new Binding("") { Converter = new DbNullToFontConverter(), ConverterParameter = i }));
                column.CellStyle = cellStyle;

                grid.Columns.Add(column);
                        
                ++i;
            }
        }

        public void Update() {
            UpdateDataView(dgvPreviewValidation, 0);
            UpdateDataView(dgvPreviewView, 1);
            dgvPreviewValidation.Items.Refresh();
            dgvPreviewView.Items.Refresh();

            Font usedFont = new Font(System.Drawing.FontFamily.GenericSerif,
                                        (float) dgvPreviewValidation.FontSize);
            for (int i = 0; i < dgvPreviewValidation.Columns.Count; ++i) {
                int maxSize = -1;
                foreach (var row in Model.DataValidation.Data) {
                    maxSize = Math.Max(TextRenderer.MeasureText(row[i].DisplayString,
                                                    usedFont).Width, maxSize);
                }
                foreach (var row in Model.DataView.Data) {
                    maxSize = Math.Max(TextRenderer.MeasureText(row[i].DisplayString,
                                                    usedFont).Width, maxSize);
                }
                maxSize = Math.Max(TextRenderer.MeasureText(dgvPreviewValidation.Columns[i].Header.ToString(),
                                                usedFont).Width, maxSize);
                maxSize = Math.Max(TextRenderer.MeasureText(dgvPreviewView.Columns[i].Header.ToString(),
                                                usedFont).Width, maxSize);

                dgvPreviewValidation.Columns[i].Width = new DataGridLength(maxSize);
                dgvPreviewView.Columns[i].Width = new DataGridLength(maxSize);
                    
            }

        }

        private void RowDefinition_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
                dgvPreviewValidation.ItemsSource = Model.DataValidation.Data;

                dgvPreviewView.ItemsSource = Model.DataView.Data;
            }
        }

        private void btnShowAll_Click(object sender, RoutedEventArgs e) {
            Model.FillData(false);
            dgvPreviewValidation.Items.Refresh();
            dgvPreviewView.Items.Refresh();
        }
    }

}
