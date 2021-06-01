using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using AvdCommon.Rules.Gui.DragDrop;
using ViewValidator.Controls.Datagrid;
using ViewValidator.Converters;
using ViewValidator.Factories;
using ViewValidator.Models.Datagrid;
using ViewValidator.Models.Rules;
using ViewValidatorLogic.Structures.InitialSetup;
using Binding = System.Windows.Data.Binding;
using Brushes = System.Windows.Media.Brushes;
using DataGrid = System.Windows.Controls.DataGrid;
using DataGridCell = System.Windows.Controls.DataGridCell;
using DragEventArgs = System.Windows.DragEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace ViewValidator.Controls.Rules {
    /// <summary>
    /// Interaktionslogik für ExtendedDataPreview.xaml
    /// </summary>
    public partial class ExtendedDataPreview : UserControl {
        public ExtendedDataPreview() {
            InitializeComponent();
        }

        ExtendedDataPreviewModel Model { get { return DataContext as ExtendedDataPreviewModel; } }

        private Style CreateCellStyle(int column) {
            Style cellStyle = new Style(typeof(DataGridCell));
            cellStyle.Setters.Add(new Setter(FontStyleProperty,
                                                new Binding("") {
                                                    Converter = new DbNullToFontConverter(),
                                                    ConverterParameter = column
                                                }));
            cellStyle.Setters.Add(
                new Setter(ToolTipProperty, new Binding(
                                                string.Format("[{0}].DisplayString", column)
                                                )));
            cellStyle.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));
            cellStyle.Setters.Add(
                new Setter(BackgroundProperty, new Binding("") {
                                                                    Converter = new RuleAppliedToBrushConverter(),
                                                                    ConverterParameter = column
                                                                }
                    ));
            return cellStyle;
        }

        private void UpdateDataView(DataGrid grid, int index) {

            grid.Columns.Clear();
            if (Model == null) return;


            int i = 0;
            foreach (var columnMapping in Model.TableMapping.ColumnMappings) {
                //columnMapping.PropertyChanged -= columnMapping_PropertyChanged;
                //columnMapping.PropertyChanged += columnMapping_PropertyChanged;
                
                //if (!columnMapping.IsVisible) continue;

                DataGridTextColumn column = new DataGridTextColumn {
                    //Header = HeaderFactory.GetGridViewHeader(index, columnMapping.GetColumn(index)),
                    Binding = new Binding(string.Format("[{0}].RuleDisplayString", i))
                };

                HeaderFactory.SetHeader(column, columnMapping, index);
                HeaderFactory.RegisterForColumnVisibilityHandling(grid, index, columnMapping);
                column.Visibility = columnMapping.IsVisible ? Visibility.Visible : Visibility.Collapsed;

                Style cellStyle = CreateCellStyle(i);
                column.CellStyle = cellStyle;
                
                grid.Columns.Add(column);
                ++i;
            }
            
        }

        public void Update() {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(
            () => {
                UpdateDataView(dgvPreviewValidation, 0);
                UpdateDataView(dgvPreviewView, 1);
                if (Model != null) {
                    dgvPreviewValidation.ItemsSource = Model.DataValidation.Data;
                    dgvPreviewView.ItemsSource = Model.DataView.Data;
                }
                dgvPreviewValidation.Items.Refresh();
                dgvPreviewView.Items.Refresh();


                Font usedFont = new Font(System.Drawing.FontFamily.GenericSerif,
                                         (float) dgvPreviewValidation.FontSize);
                for (int i = 0; i < dgvPreviewValidation.Columns.Count; ++i) {
                    int maxSize = -1;

                    foreach (var row in Model.DataValidation.Data) {
                        maxSize = Math.Max(System.Windows.Forms.TextRenderer.MeasureText(row[i].RuleDisplayString,
                                                     usedFont).Width, maxSize);
                    }
                    foreach (var row in Model.DataView.Data) {
                        maxSize = Math.Max(System.Windows.Forms.TextRenderer.MeasureText(row[i].RuleDisplayString,
                                                     usedFont).Width, maxSize);
                    }
                    //Estimate size of header control
                    maxSize = Math.Max(System.Windows.Forms.TextRenderer.MeasureText(HeaderFactory.HeaderToHeaderModel(dgvPreviewValidation.Columns[i].Header).Column.Name,
                                                 usedFont).Width + 20, maxSize);
                    maxSize = Math.Max(System.Windows.Forms.TextRenderer.MeasureText(HeaderFactory.HeaderToHeaderModel(dgvPreviewView.Columns[i].Header).Column.Name,
                                                 usedFont).Width + 20, maxSize);

                    dgvPreviewValidation.Columns[i].Width = new DataGridLength(maxSize);
                    dgvPreviewView.Columns[i].Width = new DataGridLength(maxSize);
                    
                }
            }));
        }

        private void RowDefinition_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
//                dgvPreviewValidation.ItemsSource = Model.DataValidation.Data;
//                dgvPreviewView.ItemsSource = Model.DataView.Data;
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null){
                Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Model_PropertyChanged);
            }   
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "DataValidation"){
                Task.Factory.StartNew(Update);
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(
                () => {
                    if (Model.TableMapping != null) {
                        //Receive the events that a rule changed in order to update the tables, this is necessary as color changes can only be displayed in this way
                        foreach (var columnMapping in Model.TableMapping.ColumnMappings) {
                            for (int i = 0; i < 2; ++i) {
                                columnMapping.GetColumn(i).Rules.ExecuteRules.CollectionChanged -=
                                    UsedRules_CollectionChanged;
                                columnMapping.GetColumn(i).Rules.ExecuteRules.CollectionChanged +=
                                    UsedRules_CollectionChanged;
                            }
                        }
                    }
                }));
            }
        }

        void UsedRules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(
            () => {
                //Updates colors
                dgvPreviewValidation.Items.Refresh();
                dgvPreviewView.Items.Refresh();
            }));
        }

        private void dgvPreviewValidation_Drop(object sender, DragEventArgs e) {
            HandleDrop(sender as DataGrid, e, 0);
        }

        private void dgvPreviewView_Drop(object sender, DragEventArgs e) {
            HandleDrop(sender as DataGrid, e, 1);
        }

        private void HandleDrop(DataGrid grid, DragEventArgs e, int which) {
            RuleListDragDropData data = e.Data.GetData(typeof(RuleListDragDropData)) as RuleListDragDropData;
            if (data != null) {
                DataGridCell lbi = UIHelpers.TryFindFromPoint<DataGridCell>(grid, e.GetPosition(grid));
                string columnName = null;
                int index = 0;
                
                if (lbi != null) {
                    columnName = HeaderFactory.HeaderToHeaderModel(lbi.Column.Header).Column.Name;
                    index = lbi.Column.DisplayIndex;
                }
                else {
                    DataGridColumnHeader header = UIHelpers.TryFindFromPoint<DataGridColumnHeader>(grid, e.GetPosition(grid));
                    if (header != null) {
                        Column column = HeaderFactory.HeaderToHeaderModel(header.Column.Header).Column;
                        columnName = column.Name;
                        //columnName = header.Column.Header as string;
                        index = header.Column.DisplayIndex;
                    }

                }
                if (!string.IsNullOrEmpty(columnName)) {
                    Model.RuleAssignmentModel.AddRules(data.Rules.ToList(), columnName, which, index);
                }

                e.Handled = true;
                data.Dragging = false;
            }
        }
    }
}
