using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using AV.Log;
using AvdCommon.Logging;
using DbSearch.Controls.Search;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Models.Search {
    public class InfoModel : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        public InfoModel(DataGrid searchDataGrid, MainWindowModel mainWindowModel) {
            SearchDataGrid = searchDataGrid;
            SearchDataGrid.CurrentCellChanged += new EventHandler<EventArgs>(SearchDataGrid_CurrentCellChanged);
            mainWindowModel.PropertyChanged += mainWindowModel_PropertyChanged;
            
        }
        
        #region EventHandler
        void mainWindowModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "SelectedQuery") {
                CurrentQuery = ((MainWindowModel) sender).SelectedQuery;
                if (CurrentQuery != null) {
                    TableSearchParamsModel = new SearchParamsModel(Brushes.White, CurrentQuery.SearchParams);
                }
                SelectedColumnHeaderModel = null;
                CallPropertyChanged();
            }
        }

        private void CallPropertyChanged() {
            OnPropertyChanged("Comment");
            OnPropertyChanged("ColumnSelected");
            OnPropertyChanged("CommentHeader");
        }

        void SearchDataGrid_CurrentCellChanged(object sender, EventArgs e) {
            if (CurrentQuery != null) {
                if (SearchDataGrid.CurrentColumn != null) {
                    var columnHeaderModel = SearchDataGrid.CurrentColumn.Header as ColumnHeaderModel;
                    if (columnHeaderModel != null) {
                        SelectedColumnHeaderModel = columnHeaderModel;
                    }
                }
            }
            else {
                SelectedColumnHeaderModel = null;
            }
            CallPropertyChanged();
        }
        #endregion EventHandler

        #region Properties
        public ObservableCollectionAsync<AV.Log.LogEntry> LogEntries {
            get { return LogHelper.LogEntries; }
        }
        private DataGrid SearchDataGrid { get; set; }
        private ColumnHeaderModel _selectedColumnHeaderModel;
        public ColumnHeaderModel SelectedColumnHeaderModel {
            get { return _selectedColumnHeaderModel; }
            set {
                if (_selectedColumnHeaderModel != value) {
                    _selectedColumnHeaderModel = value;
                    if (_selectedColumnHeaderModel != null) ColumnSearchParamsModel = new SearchParamsModel(Brushes.White, SelectedColumnHeaderModel.Column.SearchParams);
                    else ColumnSearchParamsModel = null;
                }
            }
        }

        public string Comment{
            get {
                if (SelectedColumnHeaderModel != null) return SelectedColumnHeaderModel.Column.Comment;
                return null;
            }
            set {
                if (SelectedColumnHeaderModel != null) {
                    SelectedColumnHeaderModel.Column.Comment = value;
                }
                
            }
        }

        public string CommentHeader {
            get { 
                if (SelectedColumnHeaderModel != null)
                    return "Kommentar für Spalte " + SelectedColumnHeaderModel.Column.Name;
                return "Keine Spalte ausgewählt";
            }
        }

        public bool ColumnSelected {
            get { return SelectedColumnHeaderModel != null; }
        }

        private SearchParamsModel _columnSearchParamsModel;
        public SearchParamsModel ColumnSearchParamsModel {
            get { return _columnSearchParamsModel; }
            set {
                if (_columnSearchParamsModel != value) {
                    _columnSearchParamsModel = value;
                    OnPropertyChanged("ColumnSearchParamsModel");
                }
            }
        }

        #region TableSearchParamsModel
        private SearchParamsModel _tableSearchParamsModel;
        public SearchParamsModel TableSearchParamsModel {
            get { return _tableSearchParamsModel; }
            set {
                if (_tableSearchParamsModel != value) {
                    _tableSearchParamsModel = value;
                    OnPropertyChanged("TableSearchParamsModel");
                }
            }
        }



        #endregion TableSearchParamsModel

        #region CurrentQuery
        private Query _currentQuery;
        public Query CurrentQuery {
            get { return _currentQuery; }
            set {
                if (_currentQuery != value) {
                    _currentQuery = value;
                    OnPropertyChanged("CurrentQuery");
                }
            }
        }
        #endregion CurrentQuery

        #endregion
    }
}
