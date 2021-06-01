using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearch.Structures.Results;
using DbSearchLogic.Structures.TableRelated;
using Utils;

namespace DbSearch.Models.Result {
    class AddMappingsModel {
        public AddMappingsModel(List<Column> columns, List<ColumnHitInfoView> columnHitInfoViews) {
            ObsSource = new ObservableCollectionAsync<DraggableColumnView>();
            foreach(var column in columns) ObsSource.Add(new DraggableColumnView(){Column = column});
            ObsDestination = new ObservableCollectionAsync<DraggableColumnView>();
            foreach(var columnHitInfoView in columnHitInfoViews) ObsDestination.Add(new DraggableColumnView(){ColumnHitInfoView = columnHitInfoView});
        }

        #region Properties
        public ObservableCollectionAsync<DraggableColumnView> ObsSource { get; set; }
        public ObservableCollectionAsync<DraggableColumnView> ObsDestination { get; set; }
        #endregion


    }

    class DraggableColumnView {
        public Column Column { get; set; }
        public ColumnHitInfoView ColumnHitInfoView { get; set; }

        public string DisplayString{get {
            if (Column != null)
                return Column.Name;
            if (ColumnHitInfoView != null)
                return ColumnHitInfoView.TableInfo.Name + "." + ColumnHitInfoView.ColumnInfo.Name;
            return "Default";
        }}
    }
}
