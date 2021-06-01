using System.Data;
using System.ComponentModel;
using DbAccess;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;

namespace ViewValidator.Structures {
    class MainScreenModel : INotifyPropertyChanged {

        private ValidationTable _viewDataGrid;
        private ValidationTable _testDataGrid;

        public DataGrid DataGrid_View { get; set; }
        public DataGrid DataGrid_Test { get; set; }

        private string _errorMessage;

        public MainScreenModel() {
            _viewDataGrid = new ValidationTable();
            _testDataGrid = new ValidationTable();
            _errorMessage = "Log-Bereich";
        }


        public ValidationTable ViewDataGrid {
            get { return _viewDataGrid; }
            set {
                _viewDataGrid = value;
                OnPropertyChanged("ViewDataGrid");
            }
        }

        public ValidationTable TestDataGrid {
            get { return _testDataGrid; }
            set {
                _testDataGrid = value;
                OnPropertyChanged("TestDataGrid");
            }
        }


        public string ErrorMessage {
            get { return _errorMessage; }
            set {
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }


        public void querySelectedData(CompareDataColl _dataCollection) {

            IDatabase mySqlDB = ConnectionManager.CreateConnection(_dataCollection.ConfModel.MySql);
            mySqlDB.Open();

            string queryString = "SELECT ";
            foreach (string colName in _dataCollection.ConfModel.ChosenMySqlCols)
                queryString += mySqlDB.Enquote(colName) + ", ";

            queryString = queryString.Remove(queryString.Length - 2);
            queryString += " FROM " + _dataCollection.ConfModel.ChosenDBMySQL + "." + _dataCollection.ConfModel.ChosenTableMySQL;

            DataTable dt = mySqlDB.GetDataTable(queryString);

            ViewDataGrid = new ValidationTable();
            foreach(DataColumn col in dt.Columns)
                ViewDataGrid.AddColumn(col.ColumnName);

            foreach (DataRow row in dt.Rows)
                ViewDataGrid.AddRow(row.ItemArray);
            mySqlDB.Close();

            IDatabase accessDB = ConnectionManager.CreateConnection(_dataCollection.ConfModel.Access);
            accessDB.Open();
            
            queryString = "SELECT ";
            foreach (string colName in _dataCollection.ConfModel.ChosenAccessCols)
                queryString += colName + ", ";

            queryString = queryString.Remove(queryString.Length - 2);
            queryString += " FROM " + _dataCollection.ConfModel.ChosenTableAccess;

            dt = accessDB.GetDataTable(queryString);

            TestDataGrid = new ValidationTable();
            foreach (DataColumn col in dt.Columns)
                TestDataGrid.AddColumn(col.ColumnName);

            foreach (DataRow row in dt.Rows)
                TestDataGrid.AddRow(row.ItemArray);
            accessDB.Close();

            CreateDataGridColumns();
        }

        private void CreateDataGridColumns() {
            DataGrid_View.Columns.Clear();

            int i = 0;
            foreach (var col in ViewDataGrid.Columns) {
                DataGridTemplateColumn tc = new DataGridTemplateColumn();
                DataTemplate dt = new DataTemplate();
                
                FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
                //border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
                border.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
                border.SetValue(Border.BorderThicknessProperty, new Thickness(0));
                border.SetBinding(Border.BackgroundProperty, new Binding("[" + i + "].Color"));

                FrameworkElementFactory tb = new FrameworkElementFactory(typeof(TextBlock));
                tb.SetBinding(TextBlock.TextProperty, new Binding("[" + i + "].Name"));
                border.AppendChild(tb);

                dt.VisualTree = border;
                tc.CellTemplate = dt;
                DataGrid_View.Columns.Add(tc);
                i++;
            }

            DataGrid_Test.Columns.Clear();
            
            int j = 0;
            foreach (var col in TestDataGrid.Columns) {
                DataGridTemplateColumn tc = new DataGridTemplateColumn();
                DataTemplate dt = new DataTemplate();

                FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
                //border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
                border.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
                border.SetValue(Border.BorderThicknessProperty, new Thickness(0));
                border.SetBinding(Border.BackgroundProperty, new Binding("[" + j + "].Color"));

                FrameworkElementFactory tb = new FrameworkElementFactory(typeof(TextBlock));
                tb.SetBinding(TextBlock.TextProperty, new Binding("[" + j + "].Name"));
                border.AppendChild(tb);

                dt.VisualTree = border;
                tc.CellTemplate = dt;
                DataGrid_Test.Columns.Add(tc);
                j++;
            }
        }

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }

        #endregion
    }
}
