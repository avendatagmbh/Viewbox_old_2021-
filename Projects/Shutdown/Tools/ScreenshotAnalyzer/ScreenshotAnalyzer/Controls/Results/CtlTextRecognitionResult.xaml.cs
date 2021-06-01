using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvdCommon.DataGridHelper;
using AvdCommon.DataGridHelper.Interfaces;
using ScreenshotAnalyzer.Converter;
using ScreenshotAnalyzer.Models.Results;

namespace ScreenshotAnalyzer.Controls.Results {
    /// <summary>
    /// Interaktionslogik für CtlTextRecognitionResult.xaml
    /// </summary>
    public partial class CtlTextRecognitionResult : UserControl {
        public CtlTextRecognitionResult() {
            InitializeComponent();
            this.IsVisibleChanged += new DependencyPropertyChangedEventHandler(CtlTextRecognitionResult_IsVisibleChanged);
        }

        void CtlTextRecognitionResult_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            CreateColumns();
        }

        TextRecognitionResultModel Model { get { return DataContext as TextRecognitionResultModel; } }

        private void CreateColumns() {
            if(Model != null && Model.RecognitionResult != null)
                CreateColumns(dgvTextTable, Model.RecognitionResult.TextTable);
            else
                CreateColumns(dgvTextTable, new DataTable());
        }

        public static void SetHeader(DataGridColumn gridColumn, IDataColumn dataColumn) {
            Style headerStyle = new Style(typeof(DataGridColumnHeader));
            headerStyle.Setters.Add(new Setter(Control.TemplateProperty, Application.Current.TryFindResource("ColumnHeaderTemplate")));
            gridColumn.HeaderStyle = headerStyle;

            gridColumn.Header = new ColumnHeaderModel(dataColumn);
        }


        public void CreateColumns(DataGrid dataGrid, DataTable dataTable) {
            dataGrid.Columns.Clear();
            int i = 0;
            foreach (var column in dataTable.Columns) {
                DataGridTemplateColumn gridColumn = new DataGridTemplateColumn();
                SetHeader(gridColumn, column);
                //gridColumn.Header = column.Name;
                gridColumn.SortMemberPath = string.Format("[{0}].EditedText", i);

                Style cellStyle = new Style(typeof(DataGridCell));
                //cellStyle.Setters.Add(new Setter(BackgroundProperty, new Binding("") { Converter = new ErrorToColorConverter(), ConverterParameter = columnIndex }));
                cellStyle.Setters.Add(new Setter(BackgroundProperty, new Binding("") { Converter = new RecognitionToBrushConverter(), ConverterParameter = i }));
                cellStyle.Setters.Add(new Setter(ForegroundProperty, Brushes.Black));
                cellStyle.Setters.Add(new Setter(ToolTipProperty, new Binding(string.Format("[{0}].DisplayString", i))));
                gridColumn.CellStyle = cellStyle;

                //Set cell template
                FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                Binding b = new Binding(string.Format("[{0}].EditedText", i));
                textBlockFactory.SetValue(TextBlock.TextProperty, b);
                textBlockFactory.SetValue(TextBlock.ForegroundProperty, Brushes.Black);

                DataTemplate cellTemplate = new DataTemplate();
                cellTemplate.VisualTree = textBlockFactory;
                gridColumn.CellTemplate = cellTemplate;

                dataGrid.Columns.Add(gridColumn);
                ++i;
            }

        }


        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model == null) return;

            CreateColumns();
            Model.PropertyChanged += Model_PropertyChanged;
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "RecognitionResult") {
                CreateColumns();
            }
        }

    }
}
