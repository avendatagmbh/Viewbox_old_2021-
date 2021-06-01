using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace AvdCommon.DataGridHelper
{
    public class DataGridCreater
    {
        public static void CreateColumns(DataGrid dataGrid, DataTable dataTable)
        {
            dataGrid.Columns.Clear();
            if (dataTable == null)
                return;
            int i = 0;
            foreach (var column in dataTable.Columns)
            {
                DataGridTemplateColumn gridColumn = new DataGridTemplateColumn();
                gridColumn.Header = column.Name;
                gridColumn.SortMemberPath = string.Format("[{0}].DisplayString", i);
                //Set cell template
                FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof (TextBlock));
                Binding b = new Binding(string.Format("[{0}].DisplayString", i));
                textBlockFactory.SetValue(TextBlock.TextProperty, b);
                textBlockFactory.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                if (!column.Color.IsEmpty)
                    textBlockFactory.SetValue(TextBlock.BackgroundProperty,
                                              new SolidColorBrush(Color.FromArgb(column.Color.A, column.Color.R,
                                                                                 column.Color.G, column.Color.B)));
                DataTemplate cellTemplate = new DataTemplate();
                cellTemplate.VisualTree = textBlockFactory;
                gridColumn.CellTemplate = cellTemplate;
                dataGrid.Columns.Add(gridColumn);
                ++i;
            }
        }

        public static DataTable CreateDataTable(System.Data.DataTable dataTable)
        {
            DataTable result = new DataTable();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                result.AddColumn(dataTable.Columns[i].ColumnName);
            }
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                var row = result.CreateRow();
                for (int j = 0; j < dataTable.Columns.Count; j++)
                {
                    row[j] = new DataRowEntry(dataTable.Rows[i].ItemArray[j]);
                }
                result.AddRow(row);
            }
            return result;
        }
    }
}