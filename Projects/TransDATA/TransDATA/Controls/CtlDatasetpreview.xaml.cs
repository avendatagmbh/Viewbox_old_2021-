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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TransDATA.Models;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlDatasetpreview.xaml
    /// </summary>
    public partial class CtlDatasetpreview : UserControl {
        private SelectedProfileModel Model { get { return DataContext as SelectedProfileModel; } }

        public CtlDatasetpreview() {
            InitializeComponent();
        }

        void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (this.Dispatcher.CheckAccess()) { 
                CreateDgColumns(e);
            } else {
                this.Dispatcher.Invoke(DispatcherPriority.Normal,
                                       new Action(() => { CreateDgColumns(e); }));
            }
        }

        //private void DataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e) {
        //    Dictionary<int, string> newOrder = new Dictionary<int, string>();
        //    for (int i = 0; i < ((DataGrid)sender).Columns.Count; i++) newOrder.Add(i, string.Empty);
        //        foreach (var col in ((DataGrid)sender).Columns) {
        //            newOrder[col.DisplayIndex] = col.Header.ToString();
        //        }
        //    Model.ColumnReordered(newOrder);
        //}

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (Model != null) Model.PropertyChanged += Model_PropertyChanged;
        }

        private void CreateDgColumns(System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "DataPreview" && Model != null && Model.DataPreview != null)
                AvdCommon.DataGridHelper.DataGridCreater.CreateColumns(dgPreview, Model.DataPreview);
        }

        private void dgPreview_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGrid dataGrid = sender as DataGrid;
            if (dataGrid.SelectedCells.Count == 0) return;
            Model.ColumnSelectionChanged(dataGrid.SelectedCells[0].Column.Header.ToString());
        }
    }
}
