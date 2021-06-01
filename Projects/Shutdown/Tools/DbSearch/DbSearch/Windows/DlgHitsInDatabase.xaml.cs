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
using System.Windows.Shapes;
using System.Windows.Threading;
using AvdCommon.DataGridHelper;
using DbSearch.Models.Result;

namespace DbSearch.Windows {
    /// <summary>
    /// Interaktionslogik für DlgHitsInDatabase.xaml
    /// </summary>
    public partial class DlgHitsInDatabase : Window {
        public DlgHitsInDatabase() {
            InitializeComponent();
        }

        HitsInDatabaseModel Model { get { return DataContext as HitsInDatabaseModel; } }

        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null) {
                Model.PropertyChanged += Model_PropertyChanged;
            }
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Loaded") {
                Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new Action(() => {
                        if (Model.Loaded) {
                            //DataGridCreater.CreateColumns(dgvValues, Model.Values);
                            //DataGridCreater.CreateColumns(dgvHitMatrix, Model.Data);
                            CreateColumns(dgvValues, Model.Values);
                            CreateColumns(dgvHitMatrix, Model.Data);
                        }
                    }
                        ))
                ;
            }
        }

        public static void CreateColumns(DataGrid dataGrid, DataTable dataTable) {
            int i = 0;
            foreach (var column in dataTable.Columns) {
                DataGridTemplateColumn gridColumn = new DataGridTemplateColumn();
                gridColumn.Header = column.Name;
                gridColumn.SortMemberPath = string.Format("[{0}].DisplayString", i);

                //Set cell template
                FrameworkElementFactory textBlockFactory = new FrameworkElementFactory(typeof(TextBlock));
                Binding b = new Binding(string.Format("[{0}].DisplayString", i));
                textBlockFactory.SetValue(TextBlock.TextProperty, b);
                textBlockFactory.SetValue(TextBlock.ForegroundProperty, Brushes.Black);
                textBlockFactory.SetValue(TextBlock.BackgroundProperty,new Binding(string.Format("[{0}].Background", i)));

                DataTemplate cellTemplate = new DataTemplate();
                cellTemplate.VisualTree = textBlockFactory;
                gridColumn.CellTemplate = cellTemplate;

                dataGrid.Columns.Add(gridColumn);
                ++i;
            }

        }
    }
}
