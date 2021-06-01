using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using eBalanceKitBusiness.Manager;
using System.ComponentModel;

namespace eBalanceKitManagement.Models {
    public class ReportConfig : INotifyPropertyChanged {

        public void Init() {
            _items = UserManager.Instance.CurrentUser.IsAdmin ? DocumentManager.Instance.Documents : DocumentManager.Instance.AllowedDocuments;
            OnPropertyChanged("Items");
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region Items
        public object Items { get { return _items; } }
        private ObservableCollection<eBalanceKitBusiness.Structures.DbMapping.Document> _items;
        #endregion

        #region SelectedItem
        public object SelectedItem { get; set; }
        #endregion
    }
}
