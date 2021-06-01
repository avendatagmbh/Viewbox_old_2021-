using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ViewValidator.Controls.Datagrid;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Models.Datagrid {
    public class ColumnHeaderModel : INotifyPropertyChanged {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        public ColumnHeaderModel(Column column, int which, CtlColumnHeader control) {
            Column = column;
            _which = which;
            HeaderControl = control;
            column.Rules.AllRules.CollectionChanged += AllRules_CollectionChanged;
        }

        void AllRules_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged("HasRules");
        }

        public Column Column { get; set; }
        private int _which { get; set; }

        public bool ShowValidationImage { get { return _which == 0; } }
        public bool ShowViewImage { get { return _which == 1; } }
        public CtlColumnHeader HeaderControl { get; private set; }

        public bool HasRules { get { return Column.Rules.AllRules.Count > 0; } }

        public void Hide() {
            Column.ColumnMapping.IsVisible = false;
        }
    }
}
