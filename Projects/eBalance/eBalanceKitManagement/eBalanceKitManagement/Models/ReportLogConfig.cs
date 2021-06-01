using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Logs;
using System.Collections.ObjectModel;
using eBalanceKitResources.Localisation;

namespace eBalanceKitManagement.Models {
    class ReportLogConfig : LogConfig, INotifyPropertyChanged{
        ReportConfig ReportConfig;

        public ReportLogConfig(ReportConfig reportConfig) {
            ReportConfig = reportConfig;
            Items = new ObservableCollection<LogEntryValueChangeBase>();
        }

        public override void ShowLogs() {
            LogFilter filter = new LogFilter();
            

            LogFiles.LogList.Clear();
            if (ReportConfig.SelectedItem == null) return;
            filter.ReportId = ((Document)ReportConfig.SelectedItem).Id;
            LogFiles.ReadReportLogs(filter);

            //base.ShowLogs();
            Items = new ObservableCollection<LogEntryValueChangeBase>();
            foreach (LogEntryBase logEntryBase in LogFiles.LogList) {
                Items.Add(logEntryBase as LogEntryValueChangeBase);
            }
        }

        private ObservableCollection<LogEntryValueChangeBase> _reportLogItems;
        public new ObservableCollection<LogEntryValueChangeBase> Items {
            get{return _reportLogItems;}
            set {
                _reportLogItems = value;
                OnPropertyChanged("Items");
            }
        }
        
        #region events
        public new event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion 

        public new LogEntryValueChangeBase SelectedItem { get; set; }

        /// <summary>
        /// Export the shown log into a csv file.
        /// </summary>
        internal override string ExportCsv() {
            string returnValue = string.Empty;
            try {
                LogFilter filter = new LogFilter();
                LogFiles.LogList.Clear();
                if (ReportConfig.SelectedItem == null) return ResourcesLogging.NoReportSelected;
                filter.ReportId = ((Document)ReportConfig.SelectedItem).Id;
                LogFiles.ReadReportLogs(filter);
            
                string filename = "Report_log.csv";
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
                    int logEntryCounter = 0;
                    foreach (LogEntryValueChangeBase logEntry in LogFiles.LogList) {
                        logEntryCounter++;
                        try {
                            // write a line of the log.
                            writer.WriteCsvData(new[] {
                                logEntry.TimestampString,
                                UserManager.Instance.GetUser(logEntry.UserId()).DisplayString,
                                logEntry.Message, logEntry.GetOldValue, logEntry.GetNewValue
                            });
                        } catch (Exception e) {
                            returnValue += Environment.NewLine + "skipped entry " + logEntryCounter;
                            continue;
                        }
                    }
                }
                return returnValue;
            } catch (Exception e) {
                return e.Message;
            }
        }
    }
}
