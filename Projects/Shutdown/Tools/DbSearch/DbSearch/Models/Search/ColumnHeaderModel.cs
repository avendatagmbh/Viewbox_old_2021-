using System.ComponentModel;
using System.Windows.Media;
using DbSearch.Manager;
using DbSearch.Models.Rules;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearch.Models.Search {
    public class ColumnHeaderModel : INotifyPropertyChanged {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion Events

        public ColumnHeaderModel(Column column, Query query) {
            Column = column;
            Query = query;
            Query.UserColumnMappings.ColumnMappings.CollectionChanged += ColumnMappings_CollectionChanged;
            RuleListBoxModel = new RuleListBoxModel(true) {Rules = Column.RuleSet};

        }

        void ColumnMappings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged("FillColor");
        }

        #region Properties
        public RuleListBoxModel RuleListBoxModel { get; set; }
        public Column Column { get; set; }
        public Query Query { get; set; }

        public Brush FillColor{
            get { 
                ColumnMapping mapping = Query.UserColumnMappings.GetMapping(Column);
                if (mapping == null) return Brushes.Transparent;
                return ColorManager<TableInfo>.GetBrush(mapping.ResultTable);
            }
        }
        #endregion Properties
    }
}
