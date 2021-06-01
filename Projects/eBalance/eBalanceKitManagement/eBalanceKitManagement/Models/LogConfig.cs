using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using DbAccess;
using Utils;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Manager;
using System.Threading;
using eBalanceKitBusiness.Structures.DbMapping;
using System.Collections.ObjectModel;
using System.ComponentModel;
using eBalanceKitManagement.Logs;
using eBalanceKitResources.Localisation;

namespace eBalanceKitManagement.Models {
    
    public abstract class LogConfig : INotifyPropertyChanged {

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        public LogConfig() {
            this.LogFiles = new eBalanceKitBusiness.Logs.Logs();
    }

        public Window Owner { get; set; }
        public eBalanceKitBusiness.Logs.Logs LogFiles { get; private set; }

        private ObservableCollection<LogEntryBase> _items;
        public virtual ObservableCollection<LogEntryBase> Items {
            get{return _items;}
            set {
                _items = value;
                OnPropertyChanged("Items");
            }
        }
        public virtual LogEntryBase SelectedItem { get; set; }

        public virtual void ShowLogs() {
            Items = new ObservableCollection<LogEntryBase>(
                UserManager.Instance.CurrentUser.IsAdmin
                    ? LogFiles.LogList.ToArray()
                    : LogFiles.LogList.Where(entry => entry.UserId() == UserManager.Instance.CurrentUser.Id).ToArray()
                );

            //if (LogFiles.LogList.Count > 0)
            //    currentPageInfo.UpdatePage(LogFiles.LogList.First().Timestamp, LogFiles.LogList.Last().Timestamp);
        }

        protected LogPageInfo currentPageInfo = new LogPageInfo();
        public virtual void PreviousPage() {
            currentPageInfo.PreviousPage();
            ShowLogs();
        }

        public virtual void NextPage() {
            currentPageInfo.NextPage();
            ShowLogs();
        }

        internal abstract string ExportCsv();
    }
}
