using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Models.Search {
    public class ColumnOverviewModel : NotifyPropertyChangedBase{
        public ColumnOverviewModel(Query query) {
            Query = query;

            foreach (var column in Query.Columns) {
                column.PropertyChanged += column_PropertyChanged;
            }
        }

        void column_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if(e.PropertyName == "IsUsedInSearch")
                OnPropertyChanged("SearchAllColumns");
            else if(e.PropertyName == "IsVisible")
                OnPropertyChanged("AllVisible");
        }

        #region Properties
        public Query Query { get; set; }

        #region SearchAllColumns
        public bool? SearchAllColumns {
            get {
                bool? whichValue = null;
                foreach (var column in Query.Columns) {
                    if (whichValue.HasValue && column.IsUsedInSearch != whichValue.Value) return null;
                    whichValue = column.IsUsedInSearch;
                }
                return whichValue != null && whichValue.Value;
            }
            set {
                if (SearchAllColumns != value) {
                    if (value.HasValue) {
                        foreach (var column in Query.Columns)
                            column.IsUsedInSearch = value.Value;
                    }
                    OnPropertyChanged("SearchAllColumns");
                }
            }
        }
        #endregion SearchAllColumns

        #region AllVisible
        public bool? AllVisible {
            get {
                bool? whichValue = null;
                foreach (var column in Query.Columns) {
                    if (whichValue.HasValue && column.IsVisible != whichValue.Value) return null;
                    whichValue = column.IsVisible;
                }
                return whichValue != null && whichValue.Value;
            }
            set {
                if (AllVisible != value) {
                    if (value.HasValue) {
                        foreach (var column in Query.Columns)
                            column.IsVisible = value.Value;
                    }
                    OnPropertyChanged("AllVisible");
                }
            }
        }
        #endregion AllVisible

        #endregion Properties

        public void DuplicateColumn(Column column) {
            Query.DuplicateColumn(column);
        }
    }
}
