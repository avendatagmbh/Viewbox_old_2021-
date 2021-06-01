using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utils;
using eBalanceKitBusiness.Logs;
using System.Collections.ObjectModel;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKitManagement.Models {
    class AdminLogConfig : LogConfig, INotifyPropertyChanged {

        public AdminLogConfig() {
            Items = new ObservableCollection<LogEntryValueChangeBase>();
        }

        public override void ShowLogs() {
            LogFilter filter = new LogFilter();

            LogFiles.LogList.Clear();
            LogFiles.ReadAdminLogs(filter);

            //base.ShowLogs();
            Items = new ObservableCollection<LogEntryValueChangeBase>();
            foreach (LogEntryBase logEntryBase in LogFiles.LogList) {
                Items.Add(logEntryBase as LogEntryValueChangeBase);
            }
        }

        private ObservableCollection<LogEntryValueChangeBase> _adminLogitems;
        public new ObservableCollection<LogEntryValueChangeBase> Items {
            get{return _adminLogitems;}
            set {
                _adminLogitems = value;
                OnPropertyChanged("Items");
            }
        }

        public new LogEntryValueChangeBase SelectedItem { get; set; }

        #region events
        public new event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        /// <summary>
        /// Save the logs into a csv file.
        /// </summary>
        internal override string ExportCsv() {

            try {

                LogFilter filter = new LogFilter();

                LogFiles.LogList.Clear();
                LogFiles.ReadAdminLogs(filter);
                
                string filename = "General_log.csv";
                SaveFileDialog dlgSaveFile = new SaveFileDialog {
                    FileName = filename,
                    DefaultExt = ".csv",
                    Filter = "csv " + ResourcesCommon.Files + " (*.csv)|*.csv"
                };
                DialogResult result = dlgSaveFile.ShowDialog();
                if (result != DialogResult.OK && result != DialogResult.Yes) return "cancelled";
                filename = dlgSaveFile.FileName;

                using (CsvWriter writer = new CsvWriter(filename, Encoding.ASCII)) {

                    writer.WriteCsvData(new[] { ResourcesLogging.Time, ResourcesCommon.User, ResourcesLogging.Event, ResourcesCommon.OldValueLabel, ResourcesCommon.NewValueLabel });

                    foreach (LogEntryValueChangeBase logEntry in LogFiles.LogList) {
                        // Write a line
                        writer.WriteCsvData(new[] {
                            logEntry.TimestampString,
                            UserManager.Instance.GetUser(logEntry.UserId()).DisplayString,
                            logEntry.Message, logEntry.GetOldValue, logEntry.GetNewValue
                        });
                    }
                }

                return string.Empty;

            } catch (Exception e) {
                return e.Message;
            }
        }
    }
}
