using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using eBalanceKitBusiness.HyperCubes.Import;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using Utils;

namespace eBalanceKit.Models.Import {
    public class ImportModel : NotifyPropertyChangedBase {
        private AvdWpfControls.AssistantControlTabPanel _tabPanel;
        private IHyperCube Cube;
        public Importer Importer { get; private set; }
        private System.Windows.Controls.DataGrid _dataGridRow;
        private System.Windows.Controls.DataGrid _dataGridColumn;
        private System.Windows.Controls.DataGrid _dataGridSummary;

        public ImportModel(IHyperCube cube, AvdWpfControls.AssistantControlTabPanel tabPanel, System.Windows.Controls.DataGrid dataGridRow, System.Windows.Controls.DataGrid dataGridColumn, System.Windows.Controls.DataGrid dataGridSummary) { 
            Cube = cube;
            Importer = new Importer(cube);
            _tabPanel = tabPanel;
            _dataGridColumn = dataGridColumn;
            _dataGridRow = dataGridRow;
            _dataGridSummary = dataGridSummary;
            //this.Importer.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Importer_PropertyChanged);
        }

        public ImportModel(IHyperCube cube, AvdWpfControls.AssistantControlTabPanel tabPanel, System.Windows.Controls.DataGrid dataGridSummary) { 
            Cube = cube;
            Importer = new Importer(cube);
            _tabPanel = tabPanel;
            _dataGridSummary = dataGridSummary;
            //this.Importer.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Importer_PropertyChanged);
        }
        
        #region eventHandler
        void Importer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {

            if (e.PropertyName == "TemplateGenerator")
                NextPage();
        }
        #endregion

        public void SaveTemplate() {
            Importer.TemplateGenerator.Entry.DimensionOrder = string.Join("|", from dim in Importer.Config.Dimensions select dim.UniquIdentifier);
            Importer.TemplateGenerator.SaveXml();
        }

        public void LoadPreview() {
            Importer.LoadImportPreview();
        }

        public void LoadCsv() {
            Importer.LoadCsv();
            CreateColumns(_dataGridColumn);
            CreateColumns(_dataGridRow);
        }

        public void GoToColmnAssignment() {
            Importer.GoToColumnAssignment();
        }

        public void GoToRowAssignment() {
            Importer.GoToRowAssignment();
        }

        public void GoToSummary() {
            Importer.GoToSummaryAssignment();
            CreateColumns(_dataGridSummary, true);
        }

        public void Import() {
            // Has to be here (after _tabPanel.NavigateNext()) because we can jump to this site if an existing template is used
            if (Importer.CsvData == null) {
                Importer.LoadCsv();
            }
            // do the import and show the result
            Importer.Import();
        }

        public void NextImportPage() {
            switch (_tabPanel.SelectedIndex) {
                case 2:
                    Importer.LoadCsv();                    
                    GoToSummary();
                    break;
            }
        }

        public void NextPage() {
            switch (_tabPanel.SelectedIndex) {
                case 0:
                    if (string.IsNullOrWhiteSpace(Importer.Config.CsvFileName)) {
                        return;
                    }
                    //Importer.Config.CsvFileName
                    break;
                case 3:
                    //if (Importer.PreviewData == null)
                    Importer.LoadImportPreview();
                    break;
                case 4:
                    Importer.LoadCsv();
                    CreateColumns(_dataGridColumn);
                    CreateColumns(_dataGridRow);
                    //CreateColumns(_dataGridColumn);
                    Importer.GoToColumnAssignment();
                    break;
                case 5:
                    //CreateColumns(_dataGridRow);
                    Importer.GoToRowAssignment();
                    break;
                case 6:
                    CreateColumns(_dataGridSummary, true);
                    Importer.GoToSummaryAssignment();
                    break;
            }
        }


        public void PreviousPage() {
            
            switch (_tabPanel.SelectedIndex) {
                case 5:
                    Importer.GoToColumnAssignment();
                    break;
                case 6:
                    Importer.GoToRowAssignment();
                    break;
                case 7:
                    Importer.GoToSummaryAssignment();
                    break;
                case 8:
                    // Import done, no next / back option anymore
                    throw new Exception("Should not be here!");
            }
        }

        private bool Validate() {
            if (Importer.CsvData == null) {
                return false;
            }
            return true;
        }

        private void CreateColumns(System.Windows.Controls.DataGrid dataGrid, bool isSummary = false) {
            CreateColumns(dataGrid, Importer.CsvData, isSummary);
        }


        private void CreateColumns(System.Windows.Controls.DataGrid dataGrid, AvdCommon.DataGridHelper.DataTable dataTable, bool isSummary) {
            dataGrid.Columns.Clear();
            int i = 0;
            foreach (var column in dataTable.Columns) {
                System.Windows.Controls.DataGridTemplateColumn gridColumn = new System.Windows.Controls.DataGridTemplateColumn();
                //DataGridTextColumn x = new DataGridTextColumn();
                //x.Binding
                System.Windows.Controls.TextBlock headerText = new System.Windows.Controls.TextBlock();
                headerText.TextTrimming = System.Windows.TextTrimming.CharacterEllipsis;
                headerText.DataContext = column;
                headerText.SetBinding(System.Windows.Controls.TextBlock.TextProperty, new System.Windows.Data.Binding("Name"));
                gridColumn.Header = headerText;

                if (isSummary && string.IsNullOrEmpty(column.Name)) {
                    // hide the not assigned columns
                    gridColumn.Visibility = Visibility.Collapsed;
                dataGrid.Columns.Add(gridColumn);
                    ++i;
                    continue;
                }

                gridColumn.SortMemberPath = String.Format("[{0}].DisplayString", i);

                //Set cell template
                System.Windows.FrameworkElementFactory textBlockFactory = new System.Windows.FrameworkElementFactory(typeof(System.Windows.Controls.TextBlock));
                System.Windows.Data.Binding b = new System.Windows.Data.Binding(String.Format("[{0}].DisplayString", i));
                textBlockFactory.SetValue(System.Windows.Controls.TextBlock.TextProperty, b);
                textBlockFactory.SetValue(System.Windows.Controls.TextBlock.ForegroundProperty, System.Windows.Media.Brushes.Black);
                textBlockFactory.SetValue(System.Windows.Controls.TextBlock.TextWrappingProperty, System.Windows.TextWrapping.Wrap);

                textBlockFactory.SetValue(System.Windows.Controls.TextBlock.BackgroundProperty,
                                          new System.Windows.Data.Binding(String.Format("[{0}].Flag", i)) {
                                              Converter =
                                              new AvdWpfControls.Converters.BoolToBrushConverter()
                                              {True = System.Windows.Media.Brushes.Red}
                                          });

                textBlockFactory.SetValue(System.Windows.Controls.TextBlock.MaxWidthProperty, 200.00);
                
                    // Attaching the CellTemplate with the configured textBlockFactory
                    System.Windows.DataTemplate cellTemplate = new System.Windows.DataTemplate();
                    cellTemplate.VisualTree = textBlockFactory;
                    gridColumn.CellTemplate = cellTemplate;
                
                dataGrid.Columns.Add(gridColumn);
                ++i;
            }
        }


    }
}
