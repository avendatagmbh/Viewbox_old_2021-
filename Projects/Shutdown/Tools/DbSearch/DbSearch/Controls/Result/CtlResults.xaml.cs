using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AvdWpfControls;
using DbSearch.Models.Result;
using DbSearch.Structures.Results;
using DbSearch.Windows;
using DbSearchLogic.SearchCore.Structures.Result;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Controls.Result {
    /// <summary>
    /// Interaktionslogik für CtlResults.xaml
    /// </summary>
    public partial class CtlResults : UserControl {
        public CtlResults() {
            InitializeComponent();
        }

        ResultsModel Model { get { return DataContext as ResultsModel; } }
        private IList _lastSelectedItems = null;
        //Used for the Context menu entry miSelectAllWithResultsInTable
        private bool _ignoreSelectionChanged = false;


        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!_ignoreSelectionChanged) {
                Model.SelectionChanged(dgColumns.SelectedItems);
                _lastSelectedItems = dgColumns.SelectedItems;
            }
        }

        private void btnAddMapping_Click(object sender, RoutedEventArgs e) {
            ColumnHitInfoView view = ((ImageButton)sender).DataContext as ColumnHitInfoView;
            Model.AddMapping(view);
        }

        private void btnRemoveMapping_Click(object sender, RoutedEventArgs e) {
            ColumnHitInfoView view = ((ImageButton) sender).DataContext as ColumnHitInfoView;
            Model.DeleteMapping(view);
            //if (_lastSelectedItems != null && _lastSelectedItems.Count == 1) {
            //    Model.DeleteMapping(view);
            //}
        }

        private void dgColumnDetails_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGridRow row = UIHelpers.TryFindFromPoint<DataGridRow>(dgColumnDetails, e.GetPosition(dgColumnDetails));
            if (row == null || _lastSelectedItems == null || _lastSelectedItems.Count == 0) return;
            ColumnHitInfoView hitInfo = row.Item as ColumnHitInfoView;
            if (hitInfo == null) return;
            Dictionary<ColumnResult,List<ColumnHitInfo>> hitInfos = new Dictionary<ColumnResult, List<ColumnHitInfo>>();

            //Fill the dictionary which has for each columnresult a list of column hit infos all referring to the same double clicked result table
            foreach(ColumnResultView selectedItem in _lastSelectedItems)
                foreach(var columnHit in selectedItem.ColumnResult.ColumnHits)
                    if(columnHit.TableInfo.Name == hitInfo.ColumnHitInfo.TableInfo.Name) {
                        List<ColumnHitInfo> currentColumnHitInfoList;
                        if (!hitInfos.TryGetValue(selectedItem.ColumnResult, out currentColumnHitInfoList)) {
                            currentColumnHitInfoList = new List<ColumnHitInfo>();
                            hitInfos.Add(selectedItem.ColumnResult, currentColumnHitInfoList);
                        }
                        currentColumnHitInfoList.Add(columnHit);
                    }
            
            HitsInDatabaseModel hitsInDatabaseModel = new HitsInDatabaseModel(hitInfos, Model.Query);
            DlgHitsInDatabase dlg = new DlgHitsInDatabase(){DataContext = hitsInDatabaseModel};
            Query query = Model.Query;
            Task.Factory.StartNew(() => hitsInDatabaseModel.LoadData(query));
            dlg.Show();
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e) {
            if (Model.SelectedResults == null) return;
            if(MessageBox.Show("Möchten Sie das ausgewählte Ergebnis wirklich löschen?", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                Model.DeleteSelectedResult();
        }

        private void miSelectAllWithResultsInTable_Click(object sender, RoutedEventArgs e) {
            DataGridRow row = UIHelpers.TryFindFromPoint<DataGridRow>(dgColumnDetails, _lastClick);
            if(row == null) return;
            ColumnHitInfoView view = row.Item as ColumnHitInfoView;
            if (view == null) return;
            List<ColumnResultView> selection = Model.SelectAllWithResultsInTable(view.TableName);
            try {
                _ignoreSelectionChanged = true;
                dgColumns.SelectedItems.Clear();

                foreach (var selectedItem in selection) {
                    if (selectedItem == selection[selection.Count-1]) _ignoreSelectionChanged = false;
                    dgColumns.SelectedItems.Add(selectedItem);
                }
            } finally {
                _ignoreSelectionChanged = false;
            }
        }

        private Point _lastClick;
        private void dgColumnDetails_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
            _lastClick = e.GetPosition(dgColumnDetails);
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        }

        private void btnReload_Click(object sender, RoutedEventArgs e) {
            Model.Reload();
        }

    }
}
